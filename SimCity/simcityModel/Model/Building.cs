using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BuildingType { Industry, OfficeBuilding, Home, Stadium, PoliceStation, FireStation, Road }
    
    public abstract class Building
    {
        #region Fields

        protected BuildingType _type;
        protected (int x, int y) _topLeftCoordinate;

        #endregion

        #region Properties

        public BuildingType Type { get => _type; set => _type = value; }
        public (int x, int y) TopLeftCoordinate
        {
            get => _topLeftCoordinate;
            set => _topLeftCoordinate = value;
        }

        [JsonIgnore]
        public virtual List<(int x, int y)> Coordinates
        {
            get => new List<(int x, int y)>() { _topLeftCoordinate };
        }

        #endregion

        #region Constructor

        public Building((int x, int y) coordinates, BuildingType type)
        {
            _type = type;    
            _topLeftCoordinate = coordinates;
        }

        #endregion
    }

    public class PeopleBuilding : Building
    {
        #region Fields

        private ObservableCollection<Person> _people;
        private bool _onFire;
        private float _fireProb;

        #endregion

        #region Properties

        [JsonIgnore]
        public ObservableCollection<Person> People { get => _people; set => _people = value; }
        public bool OnFire { get => _onFire; set => _onFire = value; }
        public float FireProbability { get => _fireProb; set => _fireProb = value; }

        #endregion

        #region Events

        public event EventHandler? NumberOfPeopleChanged;

        #endregion

        public PeopleBuilding((int x, int y) coordinates, BuildingType type) : base(coordinates, type)
        {
            _people = new ObservableCollection<Person>();
            _people.CollectionChanged += new NotifyCollectionChangedEventHandler(OnNumberOfPeopleChanged);
        }

        public void CalculateFire() { }

        #region Private event triggers

        private void OnNumberOfPeopleChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            NumberOfPeopleChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }

    public class ServiceBuilding : Building
    {
        #region Fields
        private const int POLICESTATION_EFFECT_VALUE = 5;
        private const float FIRESTATION_EFFECT_VALUE = 0.1f;
        private const int STADIUM_EFFECT_VALUE = 30;

        private bool _onFire;
        private float _fireProb;
        private int _effectSugar;
        private List<(int x, int y)> _effectCoordinates;

        #endregion

        #region Properties

        public bool Onfire { get => _onFire; set => _onFire = value; }

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
                    case BuildingType.PoliceStation: return 4;
                    case BuildingType.FireStation: return 5;
                    case BuildingType.Stadium: return 2;
                    default: return 0;
                }
            }

            set => _effectSugar = value;
        }
        public List<(int, int)> EffectCoordinates
        {
            get => _effectCoordinates;
            set => _effectCoordinates = value;
        }

        #endregion

        #region Constructor

        public ServiceBuilding((int x, int y) coordinates, BuildingType type) : base(coordinates, type)
        {
            _effectCoordinates = new List<(int, int)>();

            foreach ((int x, int y) coords in Coordinates)
            {
                for (int i = -1 * EffectSugar; i <= EffectSugar; i++)
                {
                    for (int j = -1 * EffectSugar; j <= EffectSugar; j++)
                    {
                        if (!_effectCoordinates.Contains((coords.x + i, coords.y + j)))
                            _effectCoordinates.Add((coords.x + i, coords.y + j));
                    }
                }
            }
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
                        fields[coordinates.x, coordinates.y].FieldSafety += POLICESTATION_EFFECT_VALUE;
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
                        fields[coordinates.x, coordinates.y].FieldSafety -= POLICESTATION_EFFECT_VALUE;
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
        private List<Vehicle> _vehicles;
        public List<Vehicle> Vehicles { get => _vehicles; set => _vehicles = value;  }

        public Road((int x, int y) coordinates) : base(coordinates, BuildingType.Road)
        {  
        }
    }
}
