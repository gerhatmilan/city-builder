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
    public abstract class Building
    {
        protected BuildingType _type;

        public BuildingType Type { get => _type; }

        public Building(BuildingType type)
        {
            _type = type;     
        }
    }

    public class PeopleBuilding : Building
    {
        private List<Person> _people;
        private bool _onFire;
        private float _fireProb;

        public List<Person> People { get => _people; }
        public bool OnFire { get => OnFire; }

        public PeopleBuilding(BuildingType type) : base(type)
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
        private (int x, int y) _coordinates;

        #endregion

        #region Properties

        public int Price { get => _price; }
        public int MaintenceCost { get => _maintenanceCost; }
        public bool Onfire { get => _onFire; }
        public List<(int, int)> Coordinates
        {
            get
            {
                List<(int, int)> returnList = new List<(int, int)>();

                switch (_type)
                {
                    case BuildingType.PoliceStation:
                    case BuildingType.FireStation:
                        returnList.Add((_coordinates.x, _coordinates.y));
                        break;
                    case BuildingType.Stadium:
                        returnList.Add((_coordinates.x, _coordinates.y));
                        returnList.Add((_coordinates.x, _coordinates.y + 1));
                        returnList.Add((_coordinates.x + 1, _coordinates.y));
                        returnList.Add((_coordinates.x + 1, _coordinates.y + 1));
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

        public ServiceBuilding((int x, int y) coordinates, BuildingType type) : base(type)
        {
            _coordinates = coordinates;
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
        private Vehicle _vehicle;
        private int _price;
        private int _maintenanceCost;

        public Vehicle Vehicle { get => _vehicle; }
        public int Price { get => _price; }
        public int MaintenceCost { get => _maintenanceCost; }

        public Road() : base(BuildingType.Road)
        {
            
        }
    }
}
