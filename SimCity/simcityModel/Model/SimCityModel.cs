using simcityPersistance.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Collections;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Xml;
using Newtonsoft.Json;
using System.Diagnostics.Tracing;

namespace simcityModel.Model
{
    public class SimCityModel
    {
        #region Private fields

        private const int GAMESIZE = 18;
        private const float PRICERETURN_MULTIPLIER = 2f / 3;
        private const int TAX_PER_PERSON = 5;


        private Dictionary<FieldType, (int price, int returnPrice)> _zonePrices = new Dictionary<FieldType, (int, int)>()
        {
            { FieldType.IndustrialZone, (150, CalculateReturnPrice(150)) },
            { FieldType.OfficeZone, (150, CalculateReturnPrice(150)) },
            { FieldType.ResidentalZone, (150, CalculateReturnPrice(150)) },
            { FieldType.GeneralField, (0, CalculateReturnPrice(0)) }
        };
        private Dictionary<FieldType, BuildingType> _zoneBuilding = new Dictionary<FieldType, BuildingType>()
        {
            {FieldType.IndustrialZone,BuildingType.Industry},
            {FieldType.OfficeZone,BuildingType.OfficeBuilding},
            {FieldType.ResidentalZone,BuildingType.Home}
        };

        private Dictionary<BuildingType, (int price, int returnPrice, int maintenceCost)> _buildingPrices = new Dictionary<BuildingType, (int, int, int)>()
        {
            { BuildingType.Industry, (0, CalculateReturnPrice(0), 0) },
            { BuildingType.OfficeBuilding, (0, CalculateReturnPrice(0), 0) },
            { BuildingType.Home, (0, CalculateReturnPrice(0), 0) },
            { BuildingType.Stadium, (500, CalculateReturnPrice(500), 1000) },
            { BuildingType.PoliceStation, (300, CalculateReturnPrice(300), 600) },
            { BuildingType.FireStation, (400, CalculateReturnPrice(400), 800) },
            { BuildingType.Road, (100, CalculateReturnPrice(100), 200) }
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
        private int _population;
        private int _money;
        private int _happiness;
        private Field[,] _fields;
        private List<Person> _people;
        private List<Building> _buildings;
        private List<BudgetRecord> _incomeList;
        private List<BudgetRecord> _expenseList;


        #endregion

        #region Events

        public event EventHandler<(int, int)>? MatrixChanged
            ;
        public event EventHandler<GameEventArgs>? GameInfoChanged;

        public event EventHandler<List<BudgetRecord>>? IncomeListChanged;

        public event EventHandler<List<BudgetRecord>>? ExpenseListChanged;

        public event EventHandler? GameOver;

        public event EventHandler? OneDayPassed;

        public event EventHandler? OneMonthPassed;

        public event EventHandler<(int x, int y)>? NumberOfPeopleChanged;

        public event EventHandler<(int x, int y)>? PeopleHappinessChanged;

        public event EventHandler<SimCityModel>? GameLoaded;


        #endregion

        #region Properties

        public Field[,] Fields { get => _fields; }
        public List<Person> People { get => _people; }
        public List<Building> Buildings { get => _buildings; }
        public List<BudgetRecord> IncomeList { get => _incomeList; }
        public List<BudgetRecord> ExpenseList { get => _expenseList; }
        public int GameSize { get => GAMESIZE; }
        public DateTime GameTime { get => _gameTime; }
        public int Population { get => _population; }
        public int Money { get => _money; }
        public int Happiness { get => _happiness; }

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
                }
            }

            _people      = new List<Person>();
            _buildings = new List<Building>();
            _incomeList  = new List<BudgetRecord>();
            _expenseList = new List<BudgetRecord>();

            OneDayPassed += new EventHandler(MoveIn);
            OneDayPassed += new EventHandler(RefreshPeopleHappiness);

            OneMonthPassed += new EventHandler(GetTax);
            OneMonthPassed += new EventHandler(DeductMaintenceCost);
            OneMonthPassed += new EventHandler(CheckGameOver);
        }

        #endregion

        #region Private methods
        
        private void MoveIn(object? sender, EventArgs e)
        {
            int pendingMoveIns = 1; // (int)(_random.NextDouble() * (double)_happiness);
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
                    MakeBuilding(home.X, home.Y, _zoneBuilding[home.Type]);
                    homeBuildNeeded = false;
                }
                if (workBuildNeeded)
                {
                    MakeBuilding(work.X, work.Y, _zoneBuilding[work.Type]);
                    workBuildNeeded = false;
                }

                while (!homeFull() && !workFull() && pendingMoveIns > 0)
                {
                    Person person = new Person(home, work, fieldStat.Distance);
                    _people.Add(person);
                    ((PeopleBuilding)home.Building!).People.Add(person);
                    OnNumberOfPeopleChanged(null, (home.X, home.Y));
                    ((PeopleBuilding)work.Building!).People.Add(person);
                    OnNumberOfPeopleChanged(null, (work.X, work.Y));
                    pendingMoveIns -= 1;
                }
            }
        }

        private void RefreshPeopleHappiness(object? sender, EventArgs e)
        {
            foreach (Person person in _people)
            {
                person.RefreshHappiness(GameSize);
            }
        }

        private void RefreshCityHappiness()
        {
            int happinessSum = 0;

            foreach (Person person in People)
            {
                happinessSum += person.Happiness;
            }

           _happiness = happinessSum / People.Count;
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

            _money += sum;

            if (sum > 0)
            {
                _incomeList.Add(new BudgetRecord($"{_gameTime.ToString("yyyy. MM. dd.")} - Adóbevétel", sum));
                OnIncomeListChanged();
                OnGameInfoChanged();
            }
        }

        private void DeductMaintenceCost(object? sender, EventArgs e)
        {
            int sum = 0;

            foreach(BuildingType key in _numberOfBuildings.Keys)
            {
                sum += _numberOfBuildings[key] * _buildingPrices[key].maintenceCost;
            }

            _money -= sum;

            if (sum > 0)
            {
                _expenseList.Add(new BudgetRecord($"{_gameTime.ToString("yyyy. MM. dd.")} - Havi fenntartási költségek", sum));
                OnExpenseListChanged();
                OnGameInfoChanged();
            }
        }

        private static int CalculateReturnPrice(int originalPrice)
        {
            return Convert.ToInt32(originalPrice * PRICERETURN_MULTIPLIER);
        }

        private void CheckGameOver(object? sender, EventArgs e)
        {
            if (_happiness < 10) OnGameOver();
        }

        private void AddServiceBuildingEffects(ServiceBuilding building)
        {
            foreach ((int x, int y) coordinates in building.EffectCoordinates)
            {
                if (!ValidCoordinates(coordinates))
                {
                    building.EffectCoordinates.Remove(coordinates);
                }
            }

            building.AddEffect(_fields);
        }

        private void RemoveServiceBuildingEffects(ServiceBuilding building)
        {
            foreach ((int x, int y) coordinates in building.EffectCoordinates)
            {
                if (!ValidCoordinates(coordinates))
                {
                    building.EffectCoordinates.Remove(coordinates);
                }
            }

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

        private List<(int,int)> GetAdjacentCoordinates((int x, int y) origin)
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

        private List<Building> GetAdjacentBuildings((int x, int y) origin)
        {
            var adjacentBuildings = new List<Building>();
            if (!ValidCoordinates(origin)) return adjacentBuildings;

            if (ValidCoordinates((origin.x + 1, origin.y)) && _fields[origin.x + 1, origin.y].Building != null)
                adjacentBuildings.Add(_fields[origin.x + 1, origin.y].Building!);
            if (ValidCoordinates((origin.x - 1, origin.y)) && _fields[origin.x - 1, origin.y].Building != null)
                adjacentBuildings.Add(_fields[origin.x - 1, origin.y].Building!);
            if (ValidCoordinates((origin.x, origin.y + 1)) && _fields[origin.x, origin.y + 1].Building != null)
                adjacentBuildings.Add(_fields[origin.x, origin.y + 1].Building!);
            if (ValidCoordinates((origin.x, origin.y - 1)) && _fields[origin.x, origin.y - 1].Building != null)
                adjacentBuildings.Add(_fields[origin.x, origin.y - 1].Building!);

            return adjacentBuildings;
        }

        private bool IsAdjacentWithRoad(Building building)
        {
            foreach((int x, int y) coordinates in building.Coordinates)
            {
                List<(int, int)> adjacentCoordinates = GetAdjacentCoordinates((coordinates.x, coordinates.y));
                foreach ((int x, int y) adjacentCoordinate in adjacentCoordinates)
                {
                    if (isRoad((adjacentCoordinate.x, adjacentCoordinate.y))) return true;
                }
            }

            return false;
        }

        private bool MapConnectedAfterDestruction((int x, int y) coordinates)
        {
            if (!isRoad(coordinates) || _buildings.Count <= 1) return true;
            
            Road saveRoadInstance = (Road)_fields[coordinates.x, coordinates.y].Building!;
            _fields[coordinates.x, coordinates.y].Building = null;
            _buildings.Remove(saveRoadInstance);
            _numberOfBuildings[BuildingType.Road] -= 1;

            
            bool connected = true;
            if (!mapIsConnected())
            {
                connected = false;
            }

            _fields[coordinates.x, coordinates.y].Building = saveRoadInstance;
            _buildings.Add(saveRoadInstance);
            _numberOfBuildings[BuildingType.Road] += 1;

            return connected;
        }

        #endregion

        #region Public methods

        public void InitializeGame()
        {
            _gameTime = DateTime.Now;
            _population = 0;
            _money = 3000;
            _happiness = 50;

            OnGameInfoChanged();
        }

        public async Task SaveGameAsync(string path)
        {
            string gameData = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            await _dataAccess.SaveAsync(path, gameData);
        }

        public async Task LoadGameAsync(string path)
        {
            string gameData = await _dataAccess.LoadAsync(path);

            SimCityModel loadedModel = Newtonsoft.Json.JsonConvert.DeserializeObject<SimCityModel>(gameData)!;
            OnGameLoaded(loadedModel);
        }

        public void AdvanceTime()
        {
            _gameTime = _gameTime.AddDays(1);
            OnOneDayPassed();

            if (_gameTime.Day == 1) OnOneMonthPassed();

            OnGameInfoChanged();
        }

        public (bool[,] routeExists, bool allBuildingsFound, (int, int)[,] parents, int[,] distance) BreadthFirst((int x, int y) source, bool includeFields = false)
        {
            //inits
            Queue<(int, int)> q = new Queue<(int, int)>();
            bool[,] found = new bool[GAMESIZE,GAMESIZE];
            int[,] distance = new int[GAMESIZE, GAMESIZE];
            (int x, int y)[,] parents = new (int,int)[GAMESIZE, GAMESIZE];
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
                            found[c.x, c.y] = true;
                            distance[c.x, c.y] = distance[v.x, v.y] + 1;
                            parents[c.x, c.y] = v;
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

        public bool mapIsConnected()
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
            if (_fields[x, y].Type == FieldType.GeneralField && _fields[x, y].Building == null)
            {
                _fields[x, y].Type = newFieldType;
                OnMatrixChanged((x, y));

                _money -= _zonePrices[newFieldType].price;
                _expenseList.Add(new BudgetRecord($"{_gameTime.ToString("yyyy. MM. dd.")} - Zónalerakás", _zonePrices[newFieldType].price));
                foreach (var field in _fields)
                {
                    field.updateFieldStats(this);
                }

                OnExpenseListChanged();
                OnGameInfoChanged();
            }
            else
            {
                throw new CannotBuildException("Ezt a mezőt nem jelölheted ki zóna mezőnek.");
            }
        }
        public void MakeBuilding(int x, int y, BuildingType newBuildingType)
        {
            switch (newBuildingType)
            {
                case BuildingType.OfficeBuilding:
                case BuildingType.Industry:
                case BuildingType.Home:
                    Building building = new PeopleBuilding((x, y), newBuildingType);
                    if (!IsAdjacentWithRoad(building)) return;

                    _fields[x, y].Building = building;
                    _buildings.Add(_fields[x, y].Building!);
                    _numberOfBuildings[newBuildingType]++;
                    OnMatrixChanged((x, y));
                    foreach (var field in _fields)
                    {
                        field.updateFieldStats(this);
                    }

                    break;
                case BuildingType.Road:

                    if (_fields[x, y].Type == FieldType.GeneralField && _fields[x, y].Building == null)
                    {
                        _fields[x, y].Building = new Road((x, y));

                        if (!IsAdjacentWithRoad(_fields[x, y].Building!) && _numberOfBuildings[BuildingType.Road] > 0)
                        {
                            _fields[x, y].Building = null;
                            return;
                        }

                        _buildings.Add(_fields[x, y].Building!);
                        _numberOfBuildings[newBuildingType]++;
                        OnMatrixChanged((x, y));
                        foreach (var field in _fields)
                        {
                            field.updateFieldStats(this);
                        }

                        _money -= _buildingPrices[newBuildingType].price;
                        _expenseList.Add(new BudgetRecord($"{_gameTime.ToString("yyyy. MM. dd.")} - Útlerakás", _buildingPrices[newBuildingType].price));
                        OnExpenseListChanged();
                        OnGameInfoChanged();
                    }
                    else
                    {
                        throw new CannotBuildException("Erre a mezőre nem rakhatsz le utat.");
                    }

                    break;
                default:
                    building = new ServiceBuilding((x, y), newBuildingType);
                    foreach ((int x, int y) coords in building.Coordinates)
                    {
                        if (!ValidCoordinates((coords.x, coords.y)) || _fields[coords.x, coords.y].Type != FieldType.GeneralField || _fields[coords.x, coords.y].Building != null)
                        {
                            throw new CannotBuildException("Ide nem építhetsz ilyen épületet.");
                        }
                    }

                    if (!IsAdjacentWithRoad(building)) return;

                    foreach ((int x, int y) coords in building.Coordinates)
                    {
                        _fields[coords.x, coords.y].Building = building;
                        OnMatrixChanged((coords.x, coords.y));
                    }
                    foreach (var field in _fields)
                    {
                        field.updateFieldStats(this);
                    }

                    AddServiceBuildingEffects((ServiceBuilding)building);
                    _buildings.Add(building);
                    _numberOfBuildings[newBuildingType]++;
                    _money -= _buildingPrices[newBuildingType].price;
                    _expenseList.Add(new BudgetRecord($"{_gameTime.ToString("yyyy. MM. dd.")} - Épületlerakás", _buildingPrices[newBuildingType].price));
                    OnExpenseListChanged();
                    OnGameInfoChanged();

                    break;
            }
        }

        public void Destroy(int x, int y)
        {
            switch (_fields[x, y].Type)
            {
                case FieldType.IndustrialZone:
                case FieldType.ResidentalZone:
                case FieldType.OfficeZone:
                    if (_fields[x, y].Building == null)
                    {
                        _money += _zonePrices[_fields[x, y].Type].returnPrice;
                        _incomeList.Add(new BudgetRecord($"{_gameTime.ToString("yyyy. MM. dd.")} - Zónarombolás", _zonePrices[_fields[x, y].Type].returnPrice));
                        OnIncomeListChanged();
                        OnGameInfoChanged();

                        _fields[x, y].Type = FieldType.GeneralField;
                        OnMatrixChanged((x, y));
                        foreach (var field in _fields)
                        {
                            field.updateFieldStats(this);
                        }
                    }
                    if ((_fields[x, y].Building != null && ((PeopleBuilding)_fields[x, y].Building!).People.Count == 0))
                    {
                        _buildings.Remove(_fields[x, y].Building!);
                        _numberOfBuildings[_fields[x, y].Building!.Type]--;
                        _fields[x, y].Building = null;
                        OnMatrixChanged((x, y));
                        foreach (var field in _fields)
                        {
                            field.updateFieldStats(this);
                        }
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
                                _money += _buildingPrices[_fields[x, y].Building!.Type].returnPrice;
                                _incomeList.Add(new BudgetRecord($"{_gameTime.ToString("yyyy. MM. dd.")} - Útrombolás", _buildingPrices[_fields[x, y].Building!.Type].returnPrice));
                                OnIncomeListChanged();
                                OnGameInfoChanged();

                                _numberOfBuildings[_fields[x, y].Building!.Type]--;
                                _buildings.Remove(_fields[x, y].Building!);
                                _fields[x, y].Building = null;
                                OnMatrixChanged((x, y));
                                foreach (var field in _fields)
                                {
                                    field.updateFieldStats(this);
                                }
                            }

                            break;
                        default:
                            _money += _buildingPrices[_fields[x, y].Building!.Type].returnPrice;
                            _incomeList.Add(new BudgetRecord($"{_gameTime.ToString("yyyy. MM. dd.")} - Épületrombolás", _buildingPrices[_fields[x, y].Building!.Type].returnPrice));
                            OnIncomeListChanged();
                            OnGameInfoChanged();

                            _numberOfBuildings[_fields[x, y].Building!.Type]--;
                            _buildings.Remove(_fields[x, y].Building!);
                            RemoveServiceBuildingEffects((ServiceBuilding)_fields[x, y].Building!);

                            foreach ((int x, int y) coords in ((ServiceBuilding)_fields[x, y].Building!).Coordinates)
                            {
                                _fields[coords.x, coords.y].Building = null;
                                OnMatrixChanged((coords.x, coords.y));
                            }
                            foreach (var field in _fields)
                            {
                                field.updateFieldStats(this);
                            }

                            break;
                    }
                    break;
            }
        }

        #endregion

        #region Private event triggers
        /// <summary>
        /// Invoking MatrixChanged event.
        /// </summary>
        /// <param name="coordinates">Changed element's coordinates.</param>
        private void OnMatrixChanged((int x, int y) coordinates)
        {
            MatrixChanged?.Invoke(this, coordinates);
        }

        /// <summary>
        /// Invoking GameInfoChanged event.
        /// </summary>
        private void OnGameInfoChanged()
        {
            GameInfoChanged?.Invoke(this, new GameEventArgs(_gameTime, _money, _population));
        }

        /// <summary>
        /// Invoking IncomeListChanged event.
        /// </summary>
        private void OnIncomeListChanged()
        {
            IncomeListChanged?.Invoke(this, _incomeList);
        }

        /// <summary>
        /// Invoking ExpenseListChanged event.
        /// </summary>
        private void OnExpenseListChanged()
        {
            ExpenseListChanged?.Invoke(this, _expenseList);
        }

        
        /// <summary>
        /// Invoking GameOver event.
        /// </summary>
        private void OnGameOver()
        {
            GameOver?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Invoking GameOver event.
        /// </summary>
        private void OnOneDayPassed()
        {
            OneDayPassed?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Invoking GameOver event.
        /// </summary>
        private void OnOneMonthPassed()
        {
            OneMonthPassed?.Invoke(this, new EventArgs());
        }

        private void OnNumberOfPeopleChanged(object? sender, (int x, int y) coords)
        {
            NumberOfPeopleChanged?.Invoke(this, (coords.x, coords.y));
            RefreshCityHappiness();
        }

        private void OnPeopleHappinessChanged(object? sender, (int x, int y) coords)
        {
            PeopleHappinessChanged?.Invoke(this, (coords.x, coords.y));
        }

        private void OnGameLoaded(SimCityModel loadedModel)
        {
            GameLoaded?.Invoke(this, loadedModel);
        }

        #endregion
    }
}
