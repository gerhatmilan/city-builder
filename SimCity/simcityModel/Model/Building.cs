using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public enum BuildingType { Industry, OfficeBuilding, Home, Stadium, PoliceStation, FireStation, Road }

    public class BuildingStat
    {
        private BuildingType _type;
        private int _distance;
        private (int x, int y) _coordinates;
        private Queue<(int x, int y)> _route;

        public BuildingStat(BuildingType type, int distance, (int x, int y) coordinates, Queue<(int x, int y)> route)
        {
            _type = type;
            _distance = distance;
            _coordinates = coordinates;
            _route = route;
        }

        public BuildingType Type { get => _type; }
        public int Distance { get => _distance; }
        public (int x, int y) Coordinates { get => _coordinates; }
        public Queue<(int x, int y)> Route { get => _route; }
    }
    
    public abstract class Building
    {
        protected BuildingType _type;
        protected (int x, int y) _topLeftCoordinate;
        protected List<BuildingStat> _stats;
        public void updateBuildingStats(SimCityModel model)
        {  
           (bool[,] routeExists, bool allBuildingsFound, (int, int)[,] parents, int[,] distance)  = model.BreadthFirst(_topLeftCoordinate);
            for (int i = 0; i < model.GameSize; i++)
            {
                for (int j = 0; j < model.GameSize; j++)
                {
                    if (model.Fields[i, j].Building != null && model.Fields[i, j].Building!.Type != BuildingType.Road)
                    {
                        var bType = model.Fields[i, j].Building!.Type;
                        int dist  = distance[i, j];
                        var route = model.CalculateRoute((_topLeftCoordinate.x, _topLeftCoordinate.y), (i, j));
                        var stat = new BuildingStat(bType, dist, (i, j), route);
                        _stats.Add(stat);
                    }
                }
            }
            _stats.Sort((x, y) => x.Distance.CompareTo(y.Distance));
        }

        public List<BuildingStat> BuildingStats { get => _stats; } 
        public BuildingType Type { get => _type; }
        public virtual bool Full { get => true; }
        public (int x, int y) TopLeftCoordinate
        {
            get => _topLeftCoordinate;
        }


        public virtual List<(int x, int y)> Coordinates
        {
            get
            {
                List<(int x, int y)> coordinates = new List<(int x, int y)>();
                coordinates.Add(_topLeftCoordinate);
                return coordinates;
            }
        }

        public Building((int x, int y) coordinates, BuildingType type)
        {
            _type = type;    
            _topLeftCoordinate = coordinates;
            _stats = new List<BuildingStat>();
        }

    }

    public class PeopleBuilding : Building
    {
        private List<Person> _people;
        private bool _onFire;
        private float _fireProb;

        public List<Person> People { get => _people; }
        public bool OnFire { get => OnFire; }

        public PeopleBuilding((int x, int y) coordinates, BuildingType type) : base(coordinates, type)
        {
            _people = new List<Person>();
        }

        public void CalculateFire() { }
    }

    public class ServiceBuilding : Building
    {
        #region Fields
        private const int POLICESTATION_EFFECT_VALUE = 20;
        private const float FIRESTATION_EFFECT_VALUE = 0.1f;
        private const int STADIUM_EFFECT_VALUE = 30;

        private int _price;
        private int _maintenanceCost;
        private bool _onFire;
        private float _fireProb;

        #endregion

        #region Properties

        public int Price { get => _price; }
        public int MaintenceCost { get => _maintenanceCost; }
        public bool Onfire { get => _onFire; }
        public override List<(int, int)> Coordinates
        {
            get
            {
                List<(int, int)> returnList = new List<(int, int)>();

                switch (_type)
                {
                    case BuildingType.PoliceStation:
                    case BuildingType.FireStation:
                        returnList.Add((_topLeftCoordinate.x, _topLeftCoordinate.y));
                        break;
                    case BuildingType.Stadium:
                        returnList.Add((_topLeftCoordinate.x, _topLeftCoordinate.y));
                        returnList.Add((_topLeftCoordinate.x, _topLeftCoordinate.y + 1));
                        returnList.Add((_topLeftCoordinate.x + 1, _topLeftCoordinate.y));
                        returnList.Add((_topLeftCoordinate.x + 1, _topLeftCoordinate.y + 1));
                        break;
                }

                return returnList;
            }
        }
        private int EffectSugar
        {
            get
            {
                switch (_type)
                {
                    case BuildingType.PoliceStation: return 1;
                    case BuildingType.FireStation: return 2;
                    case BuildingType.Stadium: return 2;
                    default: return 0;
                }
            }
        }
        public List<(int, int)> EffectCoordinates
        {
            get
            {
                List<(int, int)> returnList = new List<(int, int)>();

                foreach ((int x, int y) coordinates in Coordinates)
                { 
                    for (int i = -1 * EffectSugar; i <= EffectSugar; i++)
                    {
                        for (int j = -1 * EffectSugar; j <= EffectSugar; j++)
                        {
                            returnList.Add((coordinates.x + i, coordinates.y + j));
                        }
                    }
                }

                return returnList.Distinct().ToList();
            }
        }

        #endregion

        #region Constructor

        public ServiceBuilding((int x, int y) coordinates, BuildingType type) : base(coordinates, type)
        {
            _topLeftCoordinate = coordinates;
        }

        #endregion

        #region Public methods
        public void CalculateFire() { }

        public void AddEffect(Field[,] fields)
        {
            foreach((int x, int y) coordinates in EffectCoordinates)
            {
                switch (_type)
                {
                    case BuildingType.PoliceStation:
                        fields[coordinates.x, coordinates.y].FieldHappiness += POLICESTATION_EFFECT_VALUE;
                        break;
                    case BuildingType.FireStation:
                        /* TODO */
                        break;
                    case BuildingType.Stadium:
                        fields[coordinates.x, coordinates.y].FieldHappiness += STADIUM_EFFECT_VALUE;
                        break;
                }
            }
        }

        public void RemoveEffect(Field[,] fields)
        {
            foreach ((int x, int y) coordinates in EffectCoordinates)
            {
                switch (_type)
                {
                    case BuildingType.PoliceStation:
                        fields[coordinates.x, coordinates.y].FieldHappiness -= POLICESTATION_EFFECT_VALUE;
                        break;
                    case BuildingType.FireStation:
                        /* TODO */
                        break;
                    case BuildingType.Stadium:
                        fields[coordinates.x, coordinates.y].FieldHappiness -= STADIUM_EFFECT_VALUE;
                        break;
                }
            }
        }

        #endregion
    }

    public class Road : Building
    {
        private VehicleType _vehicle;
        private int _price;
        private int _maintenanceCost;

        public VehicleType Vehicle { get => _vehicle; }
        public int Price { get => _price; }
        public int MaintenceCost { get => _maintenanceCost; }

        public Road((int x, int y) coordinates) : base(coordinates, BuildingType.Road)
        {  
        }
    }
}
