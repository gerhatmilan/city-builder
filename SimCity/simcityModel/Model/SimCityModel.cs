using simcityPersistance.Persistance;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace simcityModel.Model
{
    public class SimCityModel
    {
        #region Private fields

        private const int GAMESIZE = 18;
        private const float PRICERETURN_MULTIPLIER = 2f / 3;
        private const int TAX_PER_PERSON = 10;

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
        private List<Vehicle> _vehicles;
        private List<Building> _availableFirestations;
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
            set => _fields = value;
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
                OnIncomeListChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        public ObservableCollection<BudgetRecord> ExpenseList
        {
            get => _expenseList;
            set
            {
                _expenseList = value;
                OnExpenseListChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
                    _fields[i, j].BuildingBurntDown += new EventHandler<(int x, int y)>(OnBuildingBurntDown);

                }
            }

            GameTime = DateTime.Now;
            Population = 0;
            Money = 3000;
            Happiness = 50;

            _people = new List<Person>();
            _buildings = new List<Building>();
            _vehicles = new List<Vehicle>();
            _availableFirestations = new List<Building>();
            _incomeList = new ObservableCollection<BudgetRecord>();
            _expenseList = new ObservableCollection<BudgetRecord>();

            _incomeList.CollectionChanged += new NotifyCollectionChangedEventHandler(OnIncomeListChanged);
            _expenseList.CollectionChanged += new NotifyCollectionChangedEventHandler(OnExpenseListChanged);

            OneDayPassed += new EventHandler(HandlePeople);
            OneDayPassed += new EventHandler(MoveVehicles);
            OneDayPassed += new EventHandler(SpawnVehicles);
            OneDayPassed += new EventHandler(HandleFireSituations);

            OneMonthPassed += new EventHandler(HandleMonthlySituations);
        }

        #endregion

        #region Private methods

        private void HandlePeople(object? sender, EventArgs e)
        {
            MoveIn();
            HandleNegativeBudget();
            RefreshPeopleHappiness();
            MovePeopleAwayDependingOnHappiness();
        }

        private void HandleFireSituations(object? sender, EventArgs e)
        {
            IncreaseDaysPassedSinceBuildingOnFire();
            TryToSetARandomBuildingOnFire();
            TryToSpreadFire();
        }

        private void HandleMonthlySituations(object? sender, EventArgs e)
        {
            GetTax();
            DeductMaintenceCost();
            CheckGameOver();
        }

        private void MoveIn()
        {
            int pendingMoveIns = Math.Max(1, _random.Next(0,(int)Happiness) / 8);
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
                int chosen = _random.Next(0, Math.Max(1, moveInList.Count / 2));
                var fieldStat = moveInList[chosen];
                moveInList.RemoveAt(chosen);

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

        private void MovePeopleAwayDependingOnHappiness()
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

        private void RefreshPeopleHappiness()
        {
            foreach (Person person in _people)
            {
                person.RefreshHappiness(this);
            }
        }

        private void RefreshCityHappiness()
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

        private void HandleNegativeBudget()
        {
            if (Money < 0) DaysPassedSinceNegativeBudget++;
            else DaysPassedSinceNegativeBudget = 0;
        }

        private void IncreaseDaysPassedSinceBuildingOnFire()
        {;
            foreach (Building building in _buildings.ToList())
            {
                if (building.OnFire) building.DaysPassedSinceOnFire++;
            }
        }

        private void TryToSetARandomBuildingOnFire()
        {
            int randomX = _random.Next(0, GameSize);
            int randomY = _random.Next(0, GameSize);

            if (Fields[randomX, randomY].Building != null)
                Fields[randomX, randomY].Building!.TryToSetOnFire();
        }

        private void TryToSpreadFire()
        {
            foreach (Building building in Buildings)
            {
                if (building.OnFire && building.DaysPassedSinceOnFire > 15)
                {
                    List<Building> adjacentBuildings = GetAdjacentBuildings(building);

                    foreach (Building adjacentBuilding in adjacentBuildings)
                    {
                        adjacentBuilding.TryToSpreadFire(building);
                    }
                }
            }
        }

        private void GetTax()
        {
            int sum = 0;

            for (int i = 0; i < GameSize; i++)
            {
                for (int j = 0; j < GameSize; j++)
                {
                    switch (_fields[i, j].Type)
                    {
                        case FieldType.OfficeZone:
                            sum += _fields[i, j].NumberOfPeople * TAX_PER_PERSON;
                            break;
                        case FieldType.IndustrialZone:
                            sum += (int)Math.Floor(_fields[i, j].NumberOfPeople * TAX_PER_PERSON * 1.5);
                            break;
                    }
                }
            }

            Money += sum;

            if (sum > 0)
            {
                _incomeList.Add(new BudgetRecord($"{GameTime.ToString("yyyy. MM. dd.")} - Adóbevétel", sum));
            }
        }

        private void DeductMaintenceCost()
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

        private void CheckGameOver()
        {
            if (Happiness < 5) OnGameOver();
        }

        private static int CalculateReturnPrice(int originalPrice)
        {
            return Convert.ToInt32(originalPrice * PRICERETURN_MULTIPLIER);
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

        private List<Building> GetAdjacentBuildings(Building building)
        {
            List<Building> adjacentBuildings = new List<Building>();

            foreach ((int x, int y) buildingCoords in building.Coordinates)
            {
                if (ValidCoordinates((buildingCoords.x + 1, buildingCoords.y)) && _fields[buildingCoords.x + 1, buildingCoords.y].Building != null && _fields[buildingCoords.x + 1, buildingCoords.y].Building != building)
                    adjacentBuildings.Add(_fields[buildingCoords.x + 1, buildingCoords.y].Building!);
                if (ValidCoordinates((buildingCoords.x - 1, buildingCoords.y)) && _fields[buildingCoords.x - 1, buildingCoords.y].Building != null && _fields[buildingCoords.x - 1, buildingCoords.y].Building != building)
                    adjacentBuildings.Add(_fields[buildingCoords.x - 1, buildingCoords.y].Building!);
                if (ValidCoordinates((buildingCoords.x, buildingCoords.y + 1)) && _fields[buildingCoords.x, buildingCoords.y + 1].Building != null && _fields[buildingCoords.x, buildingCoords.y + 1].Building != building)
                    adjacentBuildings.Add(_fields[buildingCoords.x, buildingCoords.y + 1].Building!);
                if (ValidCoordinates((buildingCoords.x, buildingCoords.y - 1)) && _fields[buildingCoords.x, buildingCoords.y - 1].Building != null && _fields[buildingCoords.x, buildingCoords.y - 1].Building != building)
                    adjacentBuildings.Add(_fields[buildingCoords.x, buildingCoords.y - 1].Building!);
            }

            return adjacentBuildings.Distinct().ToList();
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

        private void MoveVehicles(Object? sender, EventArgs e)
        {
            var toRemove = new List<Vehicle>();
            foreach (var car in _vehicles)
            {
                var pos = car.CurrentPosition;
                var nextPos = car.PeekNextPos();
                var nextDir = car.NextDirection();
                Road thisRoad = (Road)(Fields[pos.x, pos.y].Building!);
                // If the car arrived, get rid of it / put out the fire
                if (car.Arrived)
                {
                    if (car.Type == VehicleType.Firecar && Fields[pos.x, pos.y].Building != null)
                    {
                        Fields[pos.x, pos.y].Building!.PutOutFire();
                        OnMatrixChanged(this, pos);
                        _availableFirestations.Add(car.StartBuilding);
                    }
                    thisRoad.Vehicles.Remove(car);
                    toRemove.Add(car);
                    OnMatrixChanged(this, pos);
                }
                if (!car.Arrived)
                {
                    Road nextRoad = (Road)(Fields[nextPos.x, nextPos.y].Building!);
                    // Firecar has priority
                    if (car.Type == VehicleType.Firecar)
                    {
                        // but it can only move if a Firecar does not block its way
                        bool noBlockingFirecars = true;
                        foreach (var vehicle in nextRoad.Vehicles)
                        {
                            if (vehicle.Type == VehicleType.Firecar && !vehicle.FacingOpposite(nextDir))
                            {
                                noBlockingFirecars = false;
                            }
                        }
                        // in this case, all other blocking cars stop to make way for the Firecar, and it moves
                        if (noBlockingFirecars)
                        { 
                            foreach (var vehicle in nextRoad.Vehicles)
                            {
                                if (!vehicle.FacingOpposite(nextDir))
                                {
                                    nextRoad.Vehicles.Remove(vehicle);
                                    toRemove.Add(vehicle);
                                    OnMatrixChanged(this, nextPos);
                                }
                            }
                            thisRoad.Vehicles.Remove(car);
                            OnMatrixChanged(this, pos);
                            car.Move();
                            nextRoad.Vehicles.Add(car);
                            OnMatrixChanged(this, nextPos);
                        }
                    }
                    // other cars move if they can
                    else if (nextRoad.Vehicles.Count == 0 || (nextRoad.Vehicles.Count == 1 && nextRoad.Vehicles[0].FacingOpposite(nextDir)))
                    {
                        thisRoad.Vehicles.Remove(car);
                        OnMatrixChanged(this, pos);
                        car.Move();
                        nextRoad.Vehicles.Add(car);
                        OnMatrixChanged(this, nextPos);
                    }
                }
            }
            foreach (var car in toRemove)
            {
                _vehicles.Remove(car);
            }
        }

        private void SpawnVehicles(Object? sender, EventArgs e)
        {
            var peopleBuildings = new List<PeopleBuilding>();
            foreach (Building building in _buildings)
            {
                if (building.Type == BuildingType.Home || building.Type == BuildingType.Industry || building.Type == BuildingType.OfficeBuilding)
                {
                    if (((PeopleBuilding)building).People.Count > 0)
                    {
                        peopleBuildings.Add((PeopleBuilding)building);
                    }
                }
            }
            
            int spawnCount = _random.Next(0, peopleBuildings.Count);
            while (spawnCount > 0)
            {
                // choose a building and pick a random person in it, get where the person wants to go
                int spawn = _random.Next(0, spawnCount);
                PeopleBuilding chosenBuilding = peopleBuildings[spawn];
                int randomPerson = _random.Next(0, chosenBuilding.People.Count);
                Field start;
                Field end;
                if (chosenBuilding.Type == BuildingType.Home)
                {
                    start = chosenBuilding.People[randomPerson].Home;
                    end = chosenBuilding.People[randomPerson].Work;
                }
                else
                {
                    start = chosenBuilding.People[randomPerson].Work;
                    end = chosenBuilding.People[randomPerson].Home;
                }
                
                // calculate the route, get rid of the building at the beginning
                Queue<(int x, int y)> route = CalculateRoute((start.X, start.Y), (end.X, end.Y));
                route.Dequeue();
                
                // if the route starts on a road, and a vehicle fits there, add a vehicle
                if (route.Count > 1)
                {
                    (int x, int y) f = route.Peek();
                    Road first = (Road)(_fields[f.x, f.y].Building!);
                    var vehicle = new Vehicle(f, route, chosenBuilding);
                    if (first.Vehicles.Count == 0)
                    {
                        first.Vehicles.Add(vehicle);
                        _vehicles.Add(vehicle);
                        OnMatrixChanged(this, f);
                    }
                    else if (first.Vehicles.Count == 1 && vehicle.FacingOpposite(first.Vehicles[0].CurrentDirection))
                    {
                        first.Vehicles.Add(vehicle);
                        _vehicles.Add(vehicle);
                        OnMatrixChanged(this, f);
                    }
                }

                // discard the Building from pool
                peopleBuildings.Remove(chosenBuilding);
                spawnCount--;
            }
        }

        private static void CopyModel(SimCityModel target, SimCityModel source)
        {
            for (int i = 0; i < source.GameSize; i++)
            {
                for (int j = 0; j < source.GameSize; j++)
                {
                    target.Fields[i, j] = source.Fields[i, j];
                    target.Fields[i, j].NumberOfPeopleChanged += new EventHandler<(int x, int y)>(target.OnNumberOfPeopleChanged);
                    target.Fields[i, j].PeopleHappinessChanged += new EventHandler<(int x, int y)>(target.OnPeopleHappinessChanged);
                    target.Fields[i, j].FieldChanged += new EventHandler<(int x, int y)>(target.OnMatrixChanged);
                    target.Fields[i, j].BuildingBurntDown += new EventHandler<(int x, int y)>(target.OnBuildingBurntDown);


                    if (target.Fields[i, j].Type != FieldType.GeneralField || target.Fields[i, j].Building != null)
                        target.OnMatrixChanged(target, (i, j));
                }
            }

            foreach (Building building in source.Buildings)
            {
                foreach ((int x, int y) coords in building.Coordinates)
                {
                    target.Fields[coords.x, coords.y].Building = building;
                }

                target.Buildings.Add(building);
            }


            foreach (Person person in source.People)
            {
                person.Home = target.Fields[person.Home.X, person.Home.Y];
                person.Work = target.Fields[person.Work.X, person.Work.Y];
                ((PeopleBuilding)target.Fields[person.Home.X, person.Home.Y].Building!).People.Add(person);
                ((PeopleBuilding)target.Fields[person.Work.X, person.Work.Y].Building!).People.Add(person);

                target.People.Add(person);
            }

            target.GameTime = source.GameTime;
            target.DaysPassedSinceNegativeBudget = source.DaysPassedSinceNegativeBudget;
            target.Population = source.Population;
            target.Money = source.Money;
            target.Happiness = source.Happiness;
            target.NumberOfBuildings = source.NumberOfBuildings;
            target.IncomeList = source.IncomeList;
            target.IncomeList.CollectionChanged += new NotifyCollectionChangedEventHandler(target.OnIncomeListChanged);
            target.ExpenseList = source.ExpenseList;
            target.ExpenseList.CollectionChanged += new NotifyCollectionChangedEventHandler(target.OnExpenseListChanged);
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
                Converters = { new BuildingConverter()},
                NullValueHandling = NullValueHandling.Ignore
            };

            SimCityModel loadedModel = JsonConvert.DeserializeObject<SimCityModel>(gameData, settings)!;
            SimCityModel newModel = new SimCityModel(new FileDataAccess());
            OnGameLoaded(newModel);

            CopyModel(newModel, loadedModel);
        }

        public void AdvanceTime()
        {
            GameTime = GameTime.AddDays(1);

            OnOneDayPassed();
            if (GameTime.Day == 1) OnOneMonthPassed();
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
                    if (newBuildingType == BuildingType.FireStation)
                    {
                        _availableFirestations.Add(building);
                    }
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
                (int x, int y) v = end;
                while (v != (-1, -1))
                {
                    route.Enqueue(v);
                    v = parents[v.x, v.y];
                }
            }

            route = new Queue<(int x, int y)>(route.Reverse());
            return route;
        }
        
        public void SendFireTruck((int x, int y) coords)
        {
            if (Fields[coords.x, coords.y].Building == null || (Fields[coords.x, coords.y].Building != null && !Fields[coords.x, coords.y].Building!.OnFire) || _numberOfBuildings[BuildingType.FireStation] < 1) return;
            if (_availableFirestations.Count == 0) return;

            // Closest path needed: look for the closest available fire station from coords, then send a fire truck to coords from the found fire station
            Building closestStation = _availableFirestations[0];
            Queue<(int x, int y)> route = CalculateRoute(closestStation.TopLeftCoordinate, coords);
            int smallestDistance = route.Count;
            int currentStationIndex = 1;
            while(currentStationIndex < _availableFirestations.Count)
            {
                Building potential = _availableFirestations[currentStationIndex];
                Queue<(int x, int y)> potentialRoute = CalculateRoute(potential.TopLeftCoordinate, coords);
                if (potentialRoute.Count < smallestDistance)
                {
                    closestStation = potential;
                    smallestDistance = potentialRoute.Count;
                }
                currentStationIndex++;
            }
            // get rid of the building at the beginning, and put out the fire if its next to the station (or on the station)
            route.Dequeue();
            if (route.Count <= 1)
            {
                Fields[coords.x, coords.y].Building!.PutOutFire();
            }      
            else
            { 
                // Provisional fire truck
                (int x, int y) f = route.Peek();
                Road first = (Road)(_fields[f.x, f.y].Building!);
                var fireCar = new Vehicle(f, route, closestStation, VehicleType.Firecar);

                // Check if there is a fire truck in the way of spawning - return if there is
                foreach (var car in first.Vehicles)
                {
                    if (car.Type == VehicleType.Firecar && !fireCar.FacingOpposite(car.CurrentDirection)) return;
                }
                
                // Remove the vehicles on the road if they are in the way of the fire truck
                foreach (var car in first.Vehicles)
                {
                    if (!fireCar.FacingOpposite(car.CurrentDirection))
                    {
                        first.Vehicles.Remove(car);
                        _vehicles.Remove(car);
                        OnMatrixChanged(this, f);
                    }
                }
                // Before the fire truck starts: make the fire station unavailable until it gets its job done (put out the fire), because a fire station can only send a single unit at the same time
                _availableFirestations.Remove(closestStation);

                // Spawn the fire truck
                first.Vehicles.Add(fireCar);
                _vehicles.Add(fireCar);
                OnMatrixChanged(this, f);
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
            RefreshCityHappiness();
        }

        private void OnBuildingBurntDown(object? sender, (int x, int y) coords)
        {
            if (_fields[coords.x, coords.y].Building == null) return;

            if (_fields[coords.x, coords.y].Building!.GetType() == typeof(PeopleBuilding))
            {
                List<Person> peopleToMoveAway = new List<Person>();

                foreach (Person person in ((PeopleBuilding)Fields[coords.x, coords.y].Building!).People)
                    peopleToMoveAway.Add(person);

                foreach (Person person in peopleToMoveAway)
                    person.MoveAway(this);
            }
            else
            {
                ((ServiceBuilding)Fields[coords.x, coords.y].Building!).RemoveEffect(Fields);
            }

            _buildings.Remove(_fields[coords.x, coords.y].Building!);
            _numberOfBuildings[_fields[coords.x, coords.y].Building!.Type]--;
            foreach ((int x, int y) buildingCoords in Fields[coords.x, coords.y].Building!.Coordinates)
            {
                _fields[buildingCoords.x, buildingCoords.y].Building = null;
                _fields[buildingCoords.x, buildingCoords.y].Type = FieldType.GeneralField;
            }

            foreach (var field in _fields)
            {
                field.UpdateFieldStats(this);
            }

            RefreshCityHappiness();
        }

        #endregion
    }
}
