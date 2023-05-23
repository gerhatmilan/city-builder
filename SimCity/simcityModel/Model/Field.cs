using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace simcityModel.Model
{
    //[JsonConverter(typeof(StringEnumConverter))]
    public enum FieldType { IndustrialZone = 3, OfficeZone = 2, ResidentalZone = 1, GeneralField = 0 }

    public class FieldStat
    {
        #region Private fields

        private (int x, int y) _parentCoordinates;
        private FieldType _type;
        private bool _hasBuilding;
        private int _distance;
        private (int x, int y) _coordinates;
        private Queue<(int x, int y)> _route;

        #endregion

        #region Properties
        public (int x, int y) ParentCoordinates { get => _parentCoordinates; set => _parentCoordinates = value; }
        public FieldType Type { get => _type; set => _type = value; }
        public bool HasBuilding { get => _hasBuilding; set => _hasBuilding = value; }
        public int Distance { get => _distance; set => _distance = value; }
        public Queue<(int x, int y)> Route { get => _route; set => _route = value; }
        public (int x, int y) Coordinates { get => _coordinates; set => _coordinates = value; }

        #endregion

        #region Constructor
        public FieldStat((int x, int y) parentCoordinates, FieldType type, bool hasBuilding, int distance, (int x, int y) coordinates, Queue<(int x, int y)> route)
        {
            _parentCoordinates = parentCoordinates;
            _type = type;
            _hasBuilding = hasBuilding;
            _distance = distance;
            _coordinates = coordinates;
            _route = route;
        }

        #endregion
    }

    public class Field
    {
        #region Private fields

        private int _x;
        private int _y;
        private FieldType _type;
        private Building? _building;
        private int _fieldHappiness;
        private int _fieldSafety;
        private List<FieldStat> _stats;

        #endregion

        #region Public fields

        public static readonly int RESIDENTAL_CAPACITY = 20;
        public static readonly int INDUSTRIAL_CAPACITY = 40;
        public static readonly int OFFICE_CAPACITY = 40;

        #endregion

        #region Events

        public event EventHandler<(int x, int y)>? NumberOfPeopleChanged;
        public event EventHandler<(int x, int y)>? PeopleHappinessChanged;
        public event EventHandler<(int x, int y)>? FieldChanged;
        public event EventHandler<(int x, int y)>? BuildingBurntDown;

        #endregion

        #region Properties

        public int X { get => _x; set => _x = value; }
        public int Y { get => _y; set => _y = value; }

        public FieldType Type
        {
            get { return _type; }
            set { _type = value; OnFieldChanged(this, EventArgs.Empty); }
        }

        public List<FieldStat> FieldStats { get => _stats; set => _stats = value; }

        [JsonIgnore]
        public Building? Building
        {
            get { return _building; }
            set
            {
                _building = value;
                if (_building != null)
                {
                    _building.GotOnFire += new EventHandler(OnFieldChanged);
                    _building.FireWentOut += new EventHandler(OnFieldChanged);
                    _building.BurntDown += new EventHandler(OnBuildingBurntDown);

                    if (_building.GetType() == typeof(PeopleBuilding))
                    {
                        ((PeopleBuilding)_building).NumberOfPeopleChanged += new EventHandler(OnNumberOfPeopleChanged);
                    }
                }

                OnFieldChanged(this, EventArgs.Empty);
            }
        }

        public int Capacity
        {
            get
            {
                switch (_type)
                {
                    case FieldType.ResidentalZone: return RESIDENTAL_CAPACITY;
                    case FieldType.IndustrialZone: return INDUSTRIAL_CAPACITY;
                    case FieldType.OfficeZone: return OFFICE_CAPACITY;
                    default: return 0;
                }
            }
        }

        public int NumberOfPeople { get => CalculateNumberOfPeople(); }

        public int FieldHappiness
        {
            get => _fieldHappiness;
            set
            {
                _fieldHappiness = value;
                if (_fieldHappiness < 0) _fieldHappiness = 0;
                else if (_fieldHappiness > 100) _fieldHappiness = 100;
            }
        }

        public int FieldSafety { get => _fieldSafety; set => _fieldSafety = value; }

        public double PeopleHappiness { get => CalculatePeopleHappiness(); }

        #endregion

        #region Constructor

        public Field(int x, int y)
        {
            _x = x;
            _y = y;
            _type = FieldType.GeneralField;
            _stats = new List<FieldStat>();
            _building = null;
            _fieldHappiness = 0;
        }

        #endregion

        #region Public methods

        public void UpdateFieldStats(SimCityModel model)
        {
            _stats.Clear();
            if (_type == FieldType.GeneralField && (_building == null || _building!.Type == BuildingType.Road)) { return; }

            (bool[,] routeExists, bool allBuildingsFound, (int, int)[,] parents, int[,] distance) = model.BreadthFirst((_x, _y), true);
            for (int i = 0; i < model.GameSize; i++)
            {
                for (int j = 0; j < model.GameSize; j++)
                {
                    if (routeExists[i, j] && !(model.Fields[i, j].Type == FieldType.GeneralField && (model.Fields[i, j].Building == null || model.Fields[i, j].Building!.Type == BuildingType.Road)))
                    {
                        var fType = model.Fields[i, j].Type;
                        bool hasBuilding = (model.Fields[i, j].Building != null);
                        int dist = distance[i, j];
                        var route = model.CalculateRoute((_x, _y), (i, j), true);
                        var stat = new FieldStat((_x, _y), fType, hasBuilding, dist, (i, j), route);
                        _stats.Add(stat);
                    }
                }
            }
            _stats.Sort((x, y) => x.Distance.CompareTo(y.Distance));
        }

        #endregion

        #region Private methods

        private double CalculatePeopleHappiness()
        {
            if (_building != null)
            {
                if (_building.GetType() == typeof(PeopleBuilding) && ((PeopleBuilding)_building).People.Count > 0)
                {
                    double sum = 0;
                    int people = ((PeopleBuilding)_building).People.Count;

                    foreach (Person person in ((PeopleBuilding)_building).People)
                    {
                        sum += person.Happiness;
                    }

                    return sum / people;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        private int CalculateNumberOfPeople()
        {
            if (_building != null)
            {
                if (_building.GetType() == typeof(PeopleBuilding))
                {
                    return ((PeopleBuilding)_building).People.Count;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region Public event triggers

        public void OnPeopleHappinessChanged(object? sender, EventArgs e)
        {
            PeopleHappinessChanged?.Invoke(this, (X, Y));
        }

        #endregion

        #region Private event triggers

        private void OnNumberOfPeopleChanged(object? sender, EventArgs e)
        {
            NumberOfPeopleChanged?.Invoke(this, (X, Y));
        }

        private void OnFieldChanged(object? sender, EventArgs e)
        {
            FieldChanged?.Invoke(this, (X, Y));
        }

        private void OnBuildingBurntDown(object? sender, EventArgs e)
        {
            BuildingBurntDown?.Invoke(this, (X, Y));
        }

        #endregion
    }
}
