using simcityPersistance.Persistance;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace simcityModel.Model
{
    public class SimCityModel
    {
        #region Private fields

        private const int GAMESIZE = 18;
        private const float PRICERETURN_MULTIPLIER = 2f / 3;
        private const int TAX_PER_PERSON = 5;

        private readonly Dictionary<FieldType, (int price, int returnPrice)> _zonePrices = new Dictionary<FieldType, (int, int)>()
        {
            { FieldType.IndustrialZone, (150, CalculateReturnPrice(150)) },
            { FieldType.OfficeZone, (150, CalculateReturnPrice(150)) },
            { FieldType.ResidentalZone, (150, CalculateReturnPrice(150)) },
            { FieldType.GeneralField, (0, CalculateReturnPrice(0)) }
        };

        private readonly Dictionary<FieldType, BuildingType> _zoneBuilding = new Dictionary<FieldType, BuildingType>()
        {
            {FieldType.IndustrialZone,BuildingType.Industry},
            {FieldType.OfficeZone,BuildingType.OfficeBuilding},
            {FieldType.ResidentalZone,BuildingType.Home}
        };

        private readonly Dictionary<BuildingType, (int price, int returnPrice, int maintenceCost)> _buildingPrices = new Dictionary<BuildingType, (int, int, int)>()
        {
            { BuildingType.Industry, (0, CalculateReturnPrice(0), 0) },
            { BuildingType.OfficeBuilding, (0, CalculateReturnPrice(0), 0) },
            { BuildingType.Home, (0, CalculateReturnPrice(0), 0) },
            { BuildingType.Stadium, (500, CalculateReturnPrice(500), 200) },
            { BuildingType.PoliceStation, (300, CalculateReturnPrice(300), 150) },
            { BuildingType.FireStation, (400, CalculateReturnPrice(400), 170) },
            { BuildingType.Road, (50, CalculateReturnPrice(50), 20) }
        };

        private Dictionary<BuildingType, int> _numberOfBuildings = new Dictionary<BuildingType, int>()
        {
            { BuildingType.Home, 0},
            { BuildingType.OfficeBuilding, 0 },
            { BuildingType.Industry, 0 },
            { BuildingType.Stadium, 0 },
            { BuildingType.PoliceStation, 0 },
            { BuildingType.FireStation, 0 },
            { BuildingType.Road, 0 }
        };

        private static Random _random = new Random();

        private IDataAccess _dataAccess;
        private DateTime _gameTime;
        private int _daysPassedSinceNegativeBudget;
        private int _population;
        private int _money;
        private double _happiness;
        private Field[,] _fields;
        private List<Person> _people;
        private List<Building> _buildings;
        private ObservableCollection<BudgetRecord> _incomeList;
        private ObservableCollection<BudgetRecord> _expenseList;

        #endregion

        #region Events

        public event EventHandler<(int, int)>? MatrixChanged;
        public event EventHandler? GameInfoChanged;
        public event EventHandler<ObservableCollection<BudgetRecord>>? IncomeListChanged;
        public event EventHandler<ObservableCollection<BudgetRecord>>? ExpenseListChanged;
        public event EventHandler? GameOver;
        public event EventHandler<SimCityModel>? GameLoaded;
        public event EventHandler? OneDayPassed;
        public event EventHandler? OneMonthPassed;
        public event EventHandler<(int x, int y)>? NumberOfPeopleChanged;
        public event EventHandler<(int x, int y)>? PeopleHappinessChanged;


        #endregion

        #region Properties

        public Field[,] Fields
        {   
            get => _fields; 
            set
            {
                for (int i = 0; i < GameSize; i++)
                {
                    for (int j = 0; j < GameSize; j++)
                    {
                        bool shouldUpdateField = (_fields[i, j].Type != FieldType.GeneralField || _fields[i, j].Building != null);

                        _fields[i, j] = value[i, j];
                        _fields[i, j].NumberOfPeopleChanged += new EventHandler<(int x, int y)>(OnNumberOfPeopleChanged);
                        _fields[i, j].PeopleHappinessChanged += new EventHandler<(int x, int y)>(OnPeopleHappinessChanged);
                        _fields[i, j].FieldChanged += new EventHandler<(int x, int y)>(OnMatrixChanged);

                        if (_fields[i, j].Type != FieldType.GeneralField || _fields[i, j].Building != null || shouldUpdateField)
                           OnMatrixChanged(this, (i, j));
                    }
                }

                foreach(Building building in Buildings)
                {
                    foreach((int x, int y) coords in building.Coordinates)
                    {
                        _fields[coords.x, coords.y].Building = building;
                    }
                }

                foreach (Person person in People)
                {
                    person.Home = _fields[person.Home.X, person.Home.Y];
                    person.Work = _fields[person.Work.X, person.Work.Y];
                    ((PeopleBuilding)_fields[person.Home.X, person.Home.Y].Building!).People.Add(person);
                    ((PeopleBuilding)_fields[person.Work.X, person.Work.Y].Building!).People.Add(person);
                }
            }
        }
        public List<Person> People { get => _people; set => _people = value; }
        public List<Building> Buildings { get => _buildings; set => _buildings = value; }
        public Dictionary<BuildingType, int> NumberOfBuildings { get => _numberOfBuildings; set => _numberOfBuildings = value; }
        public int NumberOfIndustrialBuildings { get => _numberOfBuildings[BuildingType.Industry]; }
        public int NumberOfOfficeBuildings { get => _numberOfBuildings[BuildingType.OfficeBuilding]; }
        public ObservableCollection<BudgetRecord> IncomeList
        {
            get => _incomeList;
            set
            {
                _incomeList = value;
                _incomeList.CollectionChanged += new NotifyCollectionChangedEventHandler(OnIncomeListChanged);
            }
        }
        public ObservableCollection<BudgetRecord> ExpenseList
        {
            get => _expenseList;
            set
            {
                _expenseList = value;
                _expenseList.CollectionChanged += new NotifyCollectionChangedEventHandler(OnExpenseListChanged);
            }
        }
        public int GameSize { get => GAMESIZE; }
        public DateTime GameTime { get => _gameTime; set { _gameTime = value; OnGameInfoChanged(); } }
        public int DaysPassedSinceNegativeBudget { get => _daysPassedSinceNegativeBudget; set => _daysPassedSinceNegativeBudget = value; }
        public int Population { get => _population; set { _population = value; OnGameInfoChanged(); } }
        public int Money { get => _money; set { _money = value; OnGameInfoChanged(); } }
        public double Happiness { get => _happiness; set { _happiness = value; OnGameInfoChanged(); } }

        #endregion

        #region Constructors

        public SimCityModel(IDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            _fields = new Field[GAMESIZE, GAMESIZE];

            for (int i = 0; i < GAMESIZE; i++)
            {
                for (int j = 0; j < GAMESIZE; j++)
                {
                    _fields[i, j] = new Field(i, j);
                    _fields[i, j].NumberOfPeopleChanged += new EventHandler<(int x, int y)>(OnNumberOfPeopleChanged);
                    _fields[i, j].PeopleHappinessChanged += new EventHandler<(int x, int y)>(OnPeopleHappinessChanged);
                    _fields[i, j].FieldChanged += new EventHandler<(int x, int y)>(OnMatrixChanged);
                }
            }

            GameTime = DateTime.Now;
            Population = 0;
            Money = 3000;
            Happiness = 50;

            _people = new List<Person>();
            _buildings = new List<Building>();
            _incomeList = new ObservableCollection<BudgetRecord>();
            _expenseList = new ObservableCollection<BudgetRecord>();

            _incomeList.CollectionChanged += new NotifyCollectionChangedEventHandler(OnIncomeListChanged);
            _expenseList.CollectionChanged += new NotifyCollectionChangedEventHandler(OnExpenseListChanged);

            OneDayPassed += new EventHandler(MoveIn);
            OneDayPassed += new EventHandler(HandleNegativeBudget);
            OneDayPassed += new EventHandler(RefreshPeopleHappiness);
            OneDayPassed += new EventHandler(MovePeopleAwayDependingOnHappiness);

            OneMonthPassed += new EventHandler(GetTax);
            OneMonthPassed += new EventHandler(DeductMaintenceCost);
            OneMonthPassed += new EventHandler(CheckGameOver);
        }

        #endregion

        #region Private methods

        private void MoveIn(object? sender, EventArgs e)
        {
            int pendingMoveIns = 1; // (int)(_random.NextDouble() * (double)Happiness);
            var moveInList = new List<FieldStat>();
            foreach (var field in _fields)
            {
                if (field.FieldStats.Count > 0)
                {
                    moveInList.AddRange(field.FieldStats);
                }
            }
            moveInList.Sort((x, y) => x.Distance.CompareTo(y.Distance));

            while (pendingMoveIns > 0 && moveInList.Count > 0)
            {
                var fieldStat = moveInList[0];
                moveInList.RemoveAt(0);

                Field sourceField = _fields[fieldStat.ParentCoordinates.x, fieldStat.ParentCoordinates.y];
                Field targetField = _fields[fieldStat.Coordinates.x, fieldStat.Coordinates.y];
                if (sourceField.Type == FieldType.GeneralField || targetField.Type == FieldType.GeneralField || sourceField.Type == targetField.Type || (sourceField.Type != FieldType.ResidentalZone && targetField.Type != FieldType.ResidentalZone))
                {
                    continue;
                }
                Field home = sourceField.Type == FieldType.ResidentalZone ? sourceField : targetField;
                Field work = targetField.Type == FieldType.ResidentalZone ? sourceField : targetField;
                if (!IsAdjacentWithRoad(home) || !IsAdjacentWithRoad(work)) continue;


                bool homeBuildNeeded = (home.Building == null);
                bool workBuildNeeded = (work.Building == null);

                bool homeFull()
                {
                    bool full = false;
                    if (home.Building != null)
                    {
                        full = (((PeopleBuilding)(home.Building!)).People.Count == Field.RESIDENTAL_CAPACITY);
                    }
                    return full;
                }
                bool workFull()
                {
                    bool full = false;
                    if (work.Building != null)
                    {
                        switch (work.Building!.Type)
                        {
                            case BuildingType.Industry:
                                full = (((PeopleBuilding)(work.Building!)).People.Count == Field.INDUSTRIAL_CAPACITY);
                                break;
                            case BuildingType.OfficeBuilding:
                                full = (((PeopleBuilding)(work.Building!)).People.Count == Field.OFFICE_CAPACITY);
                                break;
                            default:
                                break;
                        }
                    }
                    return full;
                }

                if (homeFull() || workFull()) continue;
                if (homeBuildNeeded)
                {
                    MakeBuildingOnZone((home.X, home.Y));
                    homeBuildNeeded = false;
                }
                if (workBuildNeeded)
                {
                    MakeBuildingOnZone((work.X, work.Y));
                    workBuildNeeded = false;
                }

                while (!homeFull() && !workFull() && pendingMoveIns > 0)
                {
                    Person person = new Person(home, work, fieldStat.Distance);
                    _people.Add(person);
                    ((PeopleBuilding)home.Building!).People.Add(person);
                    ((PeopleBuilding)work.Building!).People.Add(person);
                    Population++;
                    pendingMoveIns -= 1;
                }
            }
        }

        private void MovePeopleAwayDependingOnHappiness(object? sender, EventArgs e)
        {
            List<Person> peopleToMoveAway = new List<Person>();

            foreach (Person person in _people)
            {
                if (person.Happiness < 25)
                {
                    int randomNumber = _random.Next(0, 25);
                    if (person.Happiness < randomNumber)
                    {
                        peopleToMoveAway.Add(person);
                    }
                }
            }

            foreach (Person person in peopleToMoveAway)
                person.MoveAway(this);
        }

        private void RefreshPeopleHappiness(object? sender, EventArgs e)
        {
            foreach (Person person in _people)
            {
                person.RefreshHappiness(this);
            }
        }

        private void RefreshCityHappiness(object? sender, EventArgs e)
        {
            double happinessSum = 0;

            foreach (Person person in People)
            {
                happinessSum += person.Happiness;
            }

            if (People.Count > 0)
                Happiness = happinessSum / People.Count;
            else
                Happiness = 0;
        }

        private void HandleNegativeBudget(object? sender, EventArgs e)
        {
            if (Money < 0) DaysPassedSinceNegativeBudget++;
            else DaysPassedSinceNegativeBudget = 0;
        }

        private void GetTax(object? sender, EventArgs e)
        {
            int sum = 0;

            for (int i = 0; i < GameSize; i++)
            {
                for (int j = 0; j < GameSize; j++)
                {
                    if (_fields[i, j].Type != FieldType.GeneralField)
                    {
                        sum += _fields[i, j].NumberOfPeople * TAX_PER_PERSON;
                    }
                }
            }

            Money += sum;

            if (sum > 0)
            {
                _incomeList.Add(new BudgetRecord($"{GameTime.ToString("yyyy. MM. dd.")} - Adóbevétel", sum));
            }
        }

        private void DeductMaintenceCost(object? sender, EventArgs e)
        {
            int sum = 0;

            foreach (BuildingType key in _numberOfBuildings.Keys)
            {
                sum += _numberOfBuildings[key] * _buildingPrices[key].maintenceCost;
            }

            Money -= sum;

            if (sum > 0)
            {
                _expenseList.Add(new BudgetRecord($"{GameTime.ToString("yyyy. MM. dd.")} - Havi fenntartási költségek", sum));
            }
        }

        private static int CalculateReturnPrice(int originalPrice)
        {
            return Convert.ToInt32(originalPrice * PRICERETURN_MULTIPLIER);
        }

        private void CheckGameOver(object? sender, EventArgs e)
        {
            if (Happiness < 5) OnGameOver();
        }

        private void AddServiceBuildingEffects(ServiceBuilding building)
        {
            for (int i = 0; i < building.EffectCoordinates.Count; i++)
            {
                if (!ValidCoordinates(building.EffectCoordinates[i]))
                {
                    building.EffectCoordinates.Remove(building.EffectCoordinates[i]);
                    i--;
                }
            }

            building.AddEffect(_fields);
        }

        private void RemoveServiceBuildingEffects(ServiceBuilding building)
        {
            building.RemoveEffect(_fields);
        }

        private bool ValidCoordinates((int x, int y) coordinates)
        {
            bool valid = true;
            if (coordinates.x >= GAMESIZE || coordinates.y >= GAMESIZE || coordinates.x < 0 || coordinates.y < 0)
                valid = false;

            return valid;
        }

        private bool isRoad((int x, int y) coordinates)
        {
            bool road = false;
            if (ValidCoordinates(coordinates) && Fields[coordinates.x, coordinates.y].Building != null && Fields[coordinates.x, coordinates.y].Building!.Type == BuildingType.Road)
            {
                road = true;
            }
            return road;
        }

        private List<(int, int)> GetAdjacentCoordinates((int x, int y) origin)
        {
            var adjacentCoordinates = new List<(int, int)>();
            if (!ValidCoordinates(origin)) return adjacentCoordinates;

            if (ValidCoordinates((origin.x + 1, origin.y)))
                adjacentCoordinates.Add((origin.x + 1, origin.y));
            if (ValidCoordinates((origin.x - 1, origin.y)))
                adjacentCoordinates.Add((origin.x - 1, origin.y));
            if (ValidCoordinates((origin.x, origin.y + 1)))
                adjacentCoordinates.Add((origin.x, origin.y + 1));
            if (ValidCoordinates((origin.x, origin.y - 1)))
                adjacentCoordinates.Add((origin.x, origin.y - 1));

            return adjacentCoordinates;
        }

        private bool IsAdjacentWithRoad(Building building)
        {
            foreach ((int x, int y) coordinates in building.Coordinates)
            {
                List<(int, int)> adjacentCoordinates = GetAdjacentCoordinates((coordinates.x, coordinates.y));
                foreach ((int x, int y) adjacentCoordinate in adjacentCoordinates)
                {
                    if (isRoad((adjacentCoordinate.x, adjacentCoordinate.y))) return true;
                }
            }

            return false;
        }

        private bool IsAdjacentWithRoad(Field field)
        {

            List<(int, int)> adjacentCoordinates = GetAdjacentCoordinates((field.X, field.Y));
            foreach ((int x, int y) adjacentCoordinate in adjacentCoordinates)
            {
                if (isRoad((adjacentCoordinate.x, adjacentCoordinate.y))) return true;
            }

            return false;
        }

        private bool AdjacentBuildingsAreStillAccessibleAfterDestruction((int x, int y) coords)
        {
            bool returnValue = true;

            List<(int, int)> adjacentCoordinates = GetAdjacentCoordinates((coords.x, coords.y));
            foreach ((int x, int y) adjacentCoordinate in adjacentCoordinates)
            {
                if (_fields[adjacentCoordinate.x, adjacentCoordinate.y].Building != null && _fields[adjacentCoordinate.x, adjacentCoordinate.y].Building!.Type != BuildingType.Road)
                {
                    if (!IsAdjacentWithRoad(_fields[adjacentCoordinate.x, adjacentCoordinate.y].Building!)) returnValue = false;
                }
            }

            return returnValue;
        }

        private bool MapIsConnected()
        {
            bool connected = true;
            if (_buildings.Count > 0)
            {
                var source = _buildings[0].TopLeftCoordinate;
                var (_, allBuildingsFound, _, _) = BreadthFirst(source);
                connected = allBuildingsFound;
            }
            return connected;
        }

        private bool MapConnectedAfterDestruction((int x, int y) coordinates)
        {
            if (!isRoad(coordinates) || _buildings.Count <= 1) return true;

            Road saveRoadInstance = (Road)_fields[coordinates.x, coordinates.y].Building!;
            _fields[coordinates.x, coordinates.y].Building = null;
            _buildings.Remove(saveRoadInstance);
            _numberOfBuildings[BuildingType.Road] -= 1;


            bool connected = true;
            if (!MapIsConnected())
            {
                connected = false;
            }
            if (!AdjacentBuildingsAreStillAccessibleAfterDestruction((coordinates.x, coordinates.y)))
            {
                connected = false;
            }

            _fields[coordinates.x, coordinates.y].Building = saveRoadInstance;
            _buildings.Add(saveRoadInstance);
            _numberOfBuildings[BuildingType.Road] += 1;

            return connected;
        }

        private void MakeBuildingOnZone((int x, int y) coordinates)
        {
            PeopleBuilding building;

            switch (_fields[coordinates.x, coordinates.y].Type)
            {
                case FieldType.ResidentalZone:
                    building = new PeopleBuilding((coordinates.x, coordinates.y), BuildingType.Home);
                    break;
                case FieldType.OfficeZone:
                    building = new PeopleBuilding((coordinates.x, coordinates.y), BuildingType.OfficeBuilding);
                    break;
                case FieldType.IndustrialZone:
                    building = new PeopleBuilding((coordinates.x, coordinates.y), BuildingType.Industry);
                    break;
                default:
                    return;
            }

            if (!IsAdjacentWithRoad(building)) return;

            _fields[coordinates.x, coordinates.y].Building = building;
            _buildings.Add(_fields[coordinates.x, coordinates.y].Building!);
            _numberOfBuildings[building.Type]++;
            foreach (var field in _fields)
            {
                field.UpdateFieldStats(this);
            }
        }

        #endregion

        #region Public methods

        public async Task SaveGameAsync(string path)
        {
            Newtonsoft.Json.JsonConverter[] converters = { new BuildingConverter() };
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = { new BuildingConverter(), new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore
            };
            string jsonString = JsonConvert.SerializeObject(this, settings);


            await _dataAccess.SaveAsync(path, jsonString);
        }

        public async Task LoadGameAsync(string path)
        {
            string gameData = await _dataAccess.LoadAsync(path);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = { new BuildingConverter(), new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore
            };

            SimCityModel loadedModel = JsonConvert.DeserializeObject<SimCityModel>(gameData, settings)!;
            SimCityModel newModel = new SimCityModel(new FileDataAccess());
            OnGameLoaded(newModel);

            newModel.GameTime = loadedModel.GameTime;
            newModel.DaysPassedSinceNegativeBudget = loadedModel.DaysPassedSinceNegativeBudget;
            newModel.Population = loadedModel._population;
            newModel.Money = loadedModel.Money;
            newModel.Happiness = loadedModel.Happiness;
            newModel.NumberOfBuildings = loadedModel.NumberOfBuildings;
            newModel.People = loadedModel.People;
            newModel.Buildings = loadedModel.Buildings;
            newModel.Fields = loadedModel.Fields;
            newModel.IncomeList = loadedModel.IncomeList;
            newModel.ExpenseList = loadedModel.ExpenseList;
        }

        public void AdvanceTime()
        {
            GameTime = GameTime.AddDays(1);

            OnOneDayPassed();
            if (GameTime.Day == 1) OnOneMonthPassed();
        }

        public (bool[,] routeExists, bool allBuildingsFound, (int, int)[,] parents, int[,] distance) BreadthFirst((int x, int y) source, bool includeFields = false)
        {
            //inits
            Queue<(int, int)> q = new Queue<(int, int)>();
            bool[,] found = new bool[GAMESIZE, GAMESIZE];
            int[,] distance = new int[GAMESIZE, GAMESIZE];
            (int x, int y)[,] parents = new (int, int)[GAMESIZE, GAMESIZE];
            bool allBuildingsFound = true;
            var numberOfVisitedBuildings = new Dictionary<BuildingType, int>(_numberOfBuildings);
            if (!ValidCoordinates(source)) return (found, allBuildingsFound, parents, distance);
            foreach (var key in numberOfVisitedBuildings.Keys)
            {
                numberOfVisitedBuildings[key] = 0;
            }

            //breadth first search
            q.Enqueue(source);
            found[source.x, source.y] = true;
            if (_fields[source.x, source.y].Building != null)
            {
                numberOfVisitedBuildings[_fields[source.x, source.y].Building!.Type] = 1;
            }
            parents[source.x, source.y] = (-1, -1);
            while (q.Count != 0)
            {
                (int x, int y) v = q.Dequeue();
                foreach ((int x, int y) u in GetAdjacentCoordinates((v.x, v.y)))
                {
                    if (!found[u.x, u.y] && Fields[u.x, u.y].Building != null)
                    {
                        if (Fields[u.x, u.y].Building!.Type == BuildingType.Road)
                            q.Enqueue(u);
                        numberOfVisitedBuildings[Fields[u.x, u.y].Building!.Type] += 1;
                        foreach (var c in Fields[u.x, u.y].Building!.Coordinates)
                        {
                            if (!found[c.x, c.y])
                            {
                                found[c.x, c.y] = true;
                                distance[c.x, c.y] = distance[v.x, v.y] + 1;
                                parents[c.x, c.y] = v;
                            }
                        }
                    }

                    if (!found[u.x, u.y] && includeFields)
                    {
                        found[u.x, u.y] = true;
                        distance[u.x, u.y] = distance[v.x, v.y] + 1;
                        parents[u.x, u.y] = v;
                    }
                }
            }

            //check if all buildings have been found. yes => map is connected.
            foreach (var (key, value) in _numberOfBuildings)
            {
                if (_numberOfBuildings[key] != numberOfVisitedBuildings[key]) allBuildingsFound = false;
            }

            return (found, allBuildingsFound, parents, distance);
        }

        public Queue<(int x, int y)> CalculateRoute((int x, int y) start, (int x, int y) end, bool includeFields = false)
        {
            Queue<(int x, int y)> route = new Queue<(int x, int y)>();
            if (!ValidCoordinates(start) || !ValidCoordinates(end))
                return route;
            var (used, allBuildingsFound, parents, distance) = BreadthFirst((start.x, start.y), includeFields);
            if (used[start.x, start.y])
            {
                (int x, int y) v = start;
                while (v != (-1, -1))
                {
                    route.Enqueue(v);
                    v = parents[v.x, v.y];
                }
            }

            route = new Queue<(int x, int y)>(route.Reverse());
            return route;
        }

        public void MakeZone(int x, int y, FieldType newFieldType)
        {
            if (!ValidCoordinates((x, y))) throw new CannotBuildException("Pályán kívülre nem építhetsz.");

            if (_fields[x, y].Type == FieldType.GeneralField && _fields[x, y].Building == null)
            {
                _fields[x, y].Type = newFieldType;

                Money -= _zonePrices[newFieldType].price;
                _expenseList.Add(new BudgetRecord($"{GameTime.ToString("yyyy. MM. dd.")} - Zónalerakás", _zonePrices[newFieldType].price));
                foreach (var field in _fields)
                {
                    field.UpdateFieldStats(this);
                }
            }
            else
            {
                throw new CannotBuildException("Ezt a mezőt nem jelölheted ki zóna mezőnek.");
            }
        }
        public void MakeBuilding(int x, int y, BuildingType newBuildingType)
        {
            if (newBuildingType == BuildingType.Home || newBuildingType == BuildingType.OfficeBuilding || newBuildingType == BuildingType.Industry) throw new CannotBuildException("Ilyen épületet nem építhetsz.");
            if (!ValidCoordinates((x, y))) throw new CannotBuildException("Pályán kívülre nem építhetsz.");

            switch (newBuildingType)
            {
                case BuildingType.Road:

                    if ( _fields[x, y].Type == FieldType.GeneralField && _fields[x, y].Building == null)
                    {
                        _fields[x, y].Building = new Road((x, y));

                        if (!IsAdjacentWithRoad(_fields[x, y].Building!) && _numberOfBuildings[BuildingType.Road] > 0)
                        {
                            _fields[x, y].Building = null;
                            throw new CannotBuildException("Csak út mellé építhetsz utat.");
                        }

                        _buildings.Add(_fields[x, y].Building!);
                        _numberOfBuildings[newBuildingType]++;
                        foreach (var field in _fields)
                        {
                            field.UpdateFieldStats(this);
                        }

                        Money -= _buildingPrices[newBuildingType].price;
                        _expenseList.Add(new BudgetRecord($"{GameTime.ToString("yyyy. MM. dd.")} - Útlerakás", _buildingPrices[newBuildingType].price));
                    }
                    else
                    {
                        throw new CannotBuildException("Erre a mezőre nem rakhatsz le utat.");
                    }

                    break;
                default:
                    ServiceBuilding building = new ServiceBuilding((x, y), newBuildingType);
                    foreach ((int x, int y) coords in building.Coordinates)
                    {
                        if (!ValidCoordinates((coords.x, coords.y)) || _fields[coords.x, coords.y].Type != FieldType.GeneralField || _fields[coords.x, coords.y].Building != null)
                        {
                            throw new CannotBuildException("Ide nem építhetsz ilyen épületet.");
                        }
                    }

                    if (!IsAdjacentWithRoad(building)) throw new CannotBuildException("Csak út mellé építhetsz épületet.");

                    foreach ((int x, int y) coords in building.Coordinates)
                    {
                        _fields[coords.x, coords.y].Building = building;
                    }
                    foreach (var field in _fields)
                    {
                        field.UpdateFieldStats(this);
                    }

                    AddServiceBuildingEffects((ServiceBuilding)building);
                    _buildings.Add(building);
                    _numberOfBuildings[newBuildingType]++;
                    Money -= _buildingPrices[newBuildingType].price;
                    _expenseList.Add(new BudgetRecord($"{GameTime.ToString("yyyy. MM. dd.")} - Épületlerakás", _buildingPrices[newBuildingType].price));

                    break;
            }
        }

        public void Destroy(int x, int y)
        {
            if (!ValidCoordinates((x, y))) throw new CannotDestroyException("Pályán kívül nincs mit rombolni.");

            switch (_fields[x, y].Type)
            {
                case FieldType.IndustrialZone:
                case FieldType.ResidentalZone:
                case FieldType.OfficeZone:
                    if (_fields[x, y].Building == null)
                    {
                        Money += _zonePrices[_fields[x, y].Type].returnPrice;
                        _incomeList.Add(new BudgetRecord($"{GameTime.ToString("yyyy. MM. dd.")} - Zónarombolás", _zonePrices[_fields[x, y].Type].returnPrice));

                        _fields[x, y].Type = FieldType.GeneralField;
                        foreach (var field in _fields)
                        {
                            field.UpdateFieldStats(this);
                        }
                    }
                    else if ((_fields[x, y].Building != null && ((PeopleBuilding)_fields[x, y].Building!).People.Count == 0))
                    {
                        _buildings.Remove(_fields[x, y].Building!);
                        _numberOfBuildings[_fields[x, y].Building!.Type]--;
                        _fields[x, y].Building = null;;
                        foreach (var field in _fields)
                        {
                            field.UpdateFieldStats(this);
                        }
                    }
                    else
                    {
                        throw new CannotDestroyException("Ezt a fajta épületet csak akkor rombolhatod le, ha nincsenek benne emberek.");
                    }
                    break;
                case FieldType.GeneralField:
                    switch (_fields[x, y].Building)
                    {
                        case null:
                            break;
                        case Road:
                            if (MapConnectedAfterDestruction((x, y)))
                            {
                                Money += _buildingPrices[_fields[x, y].Building!.Type].returnPrice;
                                _incomeList.Add(new BudgetRecord($"{GameTime.ToString("yyyy. MM. dd.")} - Útrombolás", _buildingPrices[_fields[x, y].Building!.Type].returnPrice));

                                _numberOfBuildings[_fields[x, y].Building!.Type]--;
                                _buildings.Remove(_fields[x, y].Building!);
                                _fields[x, y].Building = null;
                                foreach (var field in _fields)
                                {
                                    field.UpdateFieldStats(this);
                                }
                            }
                            else
                            {
                                throw new CannotDestroyException("Mivel megszakadna az úthálózat, ezért ezt a mezőt nem rombolhatod le.");
                            }

                            break;
                        default:
                            Money += _buildingPrices[_fields[x, y].Building!.Type].returnPrice;
                            _incomeList.Add(new BudgetRecord($"{GameTime.ToString("yyyy. MM. dd.")} - Épületrombolás", _buildingPrices[_fields[x, y].Building!.Type].returnPrice));

                            _numberOfBuildings[_fields[x, y].Building!.Type]--;
                            _buildings.Remove(_fields[x, y].Building!);
                            RemoveServiceBuildingEffects((ServiceBuilding)_fields[x, y].Building!);

                            foreach ((int x, int y) coords in ((ServiceBuilding)_fields[x, y].Building!).Coordinates)
                            {
                                _fields[coords.x, coords.y].Building = null;
                            }
                            foreach (var field in _fields)
                            {
                                field.UpdateFieldStats(this);
                            }

                            break;
                    }
                    break;
            }
        }

        #endregion

        #region Private event triggers

        private void OnMatrixChanged(object? sender, (int x, int y) coordinates)
        {
            MatrixChanged?.Invoke(this, coordinates);
        }

        private void OnGameInfoChanged()
        {
            GameInfoChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnIncomeListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            IncomeListChanged?.Invoke(this, _incomeList);
        }

        private void OnExpenseListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ExpenseListChanged?.Invoke(this, _expenseList);
        }

        private void OnGameOver()
        {
            GameOver?.Invoke(this, new EventArgs());
        }

        private void OnGameLoaded(SimCityModel newModel)
        {
            GameLoaded?.Invoke(this, newModel);
        }

        private void OnOneDayPassed()
        {
            OneDayPassed?.Invoke(this, new EventArgs());

        }

        private void OnOneMonthPassed()
        {
            OneMonthPassed?.Invoke(this, new EventArgs());
        }

        private void OnNumberOfPeopleChanged(object? sender, (int x, int y) coords)
        {
            NumberOfPeopleChanged?.Invoke(this, (coords.x, coords.y));           
        }

        private void OnPeopleHappinessChanged(object? sender, (int x, int y) coords)
        {
            PeopleHappinessChanged?.Invoke(this, (coords.x, coords.y));
            RefreshCityHappiness(sender, EventArgs.Empty);
        }

        #endregion
    }
}
