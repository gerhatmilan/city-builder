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
    //[JsonConverter(typeof(StringEnumConverter))]
    public enum BuildingType { Industry = 3, OfficeBuilding = 2, Home = 1, Stadium = 7, PoliceStation = 6, FireStation = 5, Road = 4 }
    
    public abstract class Building
    {
        #region Fields

        private static readonly Dictionary<BuildingType, double> _fireProbabilities = new Dictionary<BuildingType, double>()
        {
            { BuildingType.Home, 0.25},
            { BuildingType.OfficeBuilding, 0.35 },
            { BuildingType.Industry, 0.5 },
            { BuildingType.Stadium, 0.35 },
            { BuildingType.PoliceStation, 0.3 },
            { BuildingType.FireStation, 0 },
            { BuildingType.Road, 0 }
        };

        private double _fireProbability;
        private bool _onFire;
        private int _daysPassedSinceOnFire;
        private int _daysUntilCanGetOnFire;
        protected Random _random = new Random();
        protected BuildingType _type;
        protected (int x, int y) _topLeftCoordinate;
        #endregion

        #region Events

        public event EventHandler? GotOnFire;
        public event EventHandler? FireWentOut;
        public event EventHandler? BurntDown;

        #endregion

        #region Properties

        public BuildingType Type { get => _type; set => _type = value; }
        public (int x, int y) TopLeftCoordinate
        {
            get => _topLeftCoordinate;
            set => _topLeftCoordinate = value;
        }
        public bool OnFire
        {
            get => _onFire;
            set
            {
                _onFire = value;
                if (value) OnGotOnFire();
                else OnFireWentOut();
            }
        }

        public int DaysPassedSinceOnFire
        {
            get => _daysPassedSinceOnFire;
            set
            {
                _daysPassedSinceOnFire = value;
                if (_daysPassedSinceOnFire >= 50) OnBurntDown();
            }
        }

        public int DaysUntilCanGetOnFire
        {
            get => _daysUntilCanGetOnFire;
            set
            {
                if (value < 0) return;
                if (value > 30) return;
                _daysUntilCanGetOnFire = value;
            }
        }

        public double FireProbability
        {
            get => _fireProbability;
            set
            {
                if (_fireProbabilities[Type] == 0) return;

                _fireProbability = value;
                if (_fireProbability < 0.1) _fireProbability = 0.1;
                else if (_fireProbability > _fireProbabilities[Type]) _fireProbability = _fireProbabilities[Type];
            }
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
            _fireProbability = _fireProbabilities[type];
            _onFire = false;
            _daysPassedSinceOnFire = 0;
            _topLeftCoordinate = coordinates;
        }

        #endregion

        #region Public methods

        public void TryToSpreadFire(Building adjacentBuildingOnFire)
        {
            if (!OnFire && DaysUntilCanGetOnFire == 0 && FireProbability > 0 && _random.NextDouble() + (adjacentBuildingOnFire.DaysPassedSinceOnFire / 100.0) > 0.7)
            {
                OnFire = true;
            }
        }

        public void TryToSetOnFire()
        {
            if (_random.NextDouble() <= FireProbability)
                OnFire = true;
        }

        public void PutOutFire()
        {
            OnFire = false;
            DaysUntilCanGetOnFire = 30;
        }

        #region Private event triggers

        private void OnGotOnFire()
        {
            GotOnFire?.Invoke(this, EventArgs.Empty);
        }

        private void OnFireWentOut()
        {
            FireWentOut?.Invoke(this, EventArgs.Empty);
            DaysPassedSinceOnFire = 0;
        }

        private void OnBurntDown()
        {
            BurntDown?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #endregion
    }

    public class PeopleBuilding : Building
    {
        #region Fields

        private ObservableCollection<Person> _people;

        #endregion

        #region Properties

        [JsonIgnore]
        public ObservableCollection<Person> People { get => _people; set => _people = value; }

        #endregion

        #region Events

        public event EventHandler? NumberOfPeopleChanged;

        #endregion

        public PeopleBuilding((int x, int y) coordinates, BuildingType type) : base(coordinates, type)
        {
            _people = new ObservableCollection<Person>();
            _people.CollectionChanged += new NotifyCollectionChangedEventHandler(OnNumberOfPeopleChanged);
        }

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
        private const double FIRESTATION_EFFECT_VALUE = -0.1;
        private const int STADIUM_EFFECT_VALUE = 30;

        private const int POLICESTATION_EFFECT_SUGAR = 4;
        private const int FIRESTATION_EFFECT_SUGAR = 5;
        private const int STADIUM_EFFECT_SUGAR = 2;

        private List<(int x, int y)> _effectCoordinates;

        #endregion

        #region Properties

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

        [JsonIgnore]
        private int EffectSugar
        {
            get
            {
                switch (_type)
                {
                    case BuildingType.PoliceStation: return POLICESTATION_EFFECT_SUGAR;
                    case BuildingType.FireStation: return FIRESTATION_EFFECT_SUGAR;
                    case BuildingType.Stadium: return STADIUM_EFFECT_SUGAR;
                    default: return 0;
                }
            }
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
                        if (fields[coordinates.x, coordinates.y].Building != null)
                            fields[coordinates.x, coordinates.y].Building!.FireProbability += FIRESTATION_EFFECT_VALUE;
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
                        if (fields[coordinates.x, coordinates.y].Building != null)
                            fields[coordinates.x, coordinates.y].Building!.FireProbability -= FIRESTATION_EFFECT_VALUE;
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
            _vehicles = new List<Vehicle>();
        }
    }
}
