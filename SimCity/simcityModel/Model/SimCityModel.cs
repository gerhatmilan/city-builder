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

namespace simcityModel.Model
{
    public enum GameSpeed { Paused, Normal, Fast, Fastest }
    public enum FieldType { IndustrialZone, OfficeZone, ResidentalZone, GeneralField }
    public enum BuildingType { Industry, OfficeBuilding, Home, Stadium, PoliceStation, FireStation, Road}
    public enum Vehicle { Car, Firecar, None }
    
    public class SimCityModel
    {
        #region Private fields

        private const int GAMESIZE = 10;
        private const float PRICERETURN_MULTIPLIER = 2f / 3;
        private const int TAX_PER_PERSON = 5;


        private Dictionary<FieldType, (int price, int returnPrice)> _zonePrices = new Dictionary<FieldType, (int, int)>()
        {
            { FieldType.IndustrialZone, (150, CalculateReturnPrice(150)) },
            { FieldType.OfficeZone, (150, CalculateReturnPrice(150)) },
            { FieldType.ResidentalZone, (150, CalculateReturnPrice(150)) },
            { FieldType.GeneralField, (0, CalculateReturnPrice(0)) }
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

        private IDataAccess _dataAccess;
        private DateTime _gameTime;
        private GameSpeed _gameSpeed;
        private int _population;
        private int _money;
        private int _happiness;
        private Field[,] _fields;
        private List<Person> _people;
        private Queue<Person> _homeless;
        private Queue<Person> _unemployed;
        private List<BudgetRecord> _incomeList;
        private List<BudgetRecord> _expenseList;

        #endregion

        #region Events

        /// <summary>
        /// Game matrix change event.
        /// Gets invoked when an element's type in the game matrix changes.
        /// As a parameter, it passes the changed element's coordinates to the subscriber.
        /// </summary>
        public event EventHandler<(int, int)>? MatrixChanged;

        /// <summary>
        /// Game advance event.
        /// Gets invoked every time AdvanceTime method is called.
        /// As a parameter, it passes GameEventArgs to the subscriber, which holds data of the game.
        /// </summary>
        public event EventHandler<GameEventArgs>? GameAdvanced;

        /// <summary>
        /// Income list change event.
        /// Gets invoked when the income list changes.
        /// As a parameter, it passes a list of BudgetRecord, which holds the data of every single income.
        /// </summary>
        public event EventHandler<List<BudgetRecord>>? IncomeListChanged;

        /// <summary>
        /// Expense list change event.
        /// Gets invoked when the expense list changes.
        /// As a parameter, it passes a list of BudgetRecord, which holds the data of every single expense.
        /// </summary>
        public event EventHandler<List<BudgetRecord>>? ExpenseListChanged;

        /// <summary>
        /// Game speed change event.
        /// Gets invoked when the game speed changes.
        /// As a parameter, it passes the new GameSpeed.
        /// </summary>
        public event EventHandler<GameSpeed>? GameSpeedChanged;

        /// <summary>
        /// Game over event.
        /// Gets invoked when the game is over.
        /// </summary>
        public event EventHandler? GameOver;

        #endregion

        #region Properties

        /// <summary>
        /// Getter property for _fields 
        /// </summary>
        public Field[,] Fields
        {
            get
            {
                return _fields;
            }
        }

        /// <summary>
        /// Getter property for GAMESIZE
        /// </summary>
        public int GameSize
        {
            get
            {
                return GAMESIZE;
            }
        }

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
                }
            }

            _people = new List<Person>();
            _homeless = new Queue<Person>();
            _unemployed = new Queue<Person>();
            _incomeList = new List<BudgetRecord>();
            _expenseList = new List<BudgetRecord>();

            InitializeGame();
        }

        #endregion

        #region Private methods

        private static int CalculateReturnPrice(int originalPrice)
        {
            return Convert.ToInt32(originalPrice * PRICERETURN_MULTIPLIER);
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
                adjacentCoordinates.Add((origin.x, origin.y +1));
            if (ValidCoordinates((origin.x, origin.y - 1)))
                adjacentCoordinates.Add((origin.x, origin.y - 1));

            return adjacentCoordinates;
        }

        private bool NewWorkplaceNeeded()
        {
            bool needed = false;
            if (_unemployed.Count > Field.OFFICE_CAPACITY)
            {
                needed = true;
            }
            return needed;
        }
        private bool NewHomeNeeded()
        {
            bool needed = false;
            if (_homeless.Count > Field.RESIDENTAL_CAPACITY)
            {
                needed = true;
            }
            return needed;
        }
        private bool NewPeopleNeeded()
        {
            return false;
        }
        private bool NewCarSampleNeeded()
        {
            return false;
        }
        private void SampleNewCars()
        { 
        }
        private void MoveCars()
        { 
        }
        private bool TaxDay()
        {
            return false;
        }


        #endregion

        #region Public methods

        public void InitializeGame()
        {
            _gameTime = DateTime.Now;
            _gameSpeed = GameSpeed.Normal;
            _population = 0;
            _money = 3000;
            _happiness = 0;
        }

        public void AdvanceTime()
        {
            if(NewHomeNeeded())
            {
                foreach (Field field in Fields)
                {
                    if (field.Type == FieldType.ResidentalZone && field.Building == null)
                    {
                        MakeBuilding(field.X, field.Y, BuildingType.Home);
                        while (_homeless.Count > 0 && field.Capacity > field.NumberOfPeople)
                        {
                            Person person = _homeless.Dequeue();
                            person.home = (PeopleBuilding?)field.Building;
                            person.home!.People.Add(person);                            
                        }
                        break;
                    }
                }
            }
            if(NewWorkplaceNeeded())
            {
                foreach (Field field in Fields)
                {
                    if ((field.Type == FieldType.IndustrialZone || field.Type == FieldType.OfficeZone) && field.Building == null)
                    {
                        if (field.Type == FieldType.IndustrialZone)
                        {
                            MakeBuilding(field.X, field.Y, BuildingType.Industry);
                        }
                        else
                        {
                            MakeBuilding(field.X, field.Y, BuildingType.OfficeBuilding);
                        }

                        while (_unemployed.Count > 0 && field.Capacity > field.NumberOfPeople)
                        {
                            Person person = _unemployed.Dequeue();
                            person.work = (PeopleBuilding?)field.Building;
                            person.work!.People.Add(person);
                        }
                        break;
                    }
                }
            }
            while(NewPeopleNeeded())
            {
                var person = new Person();
                _people.Add(person);
                _homeless.Enqueue(person);
                _unemployed.Enqueue(person);
            }
            if(TaxDay())
            {
                /* TODO */
            }
            
            MoveCars();
            if (NewCarSampleNeeded())
            {
                SampleNewCars();
            }
            OnGameAdvanced();
        }

        public void ChangeGameSpeed(GameSpeed newSpeed)
        {
            _gameSpeed = newSpeed;
            OnGameSpeedChanged();
        }

        public void GetTax()
        {
            int sum = 0;

            for (int i = 0; i < GameSize; i++)
            {
                for (int j = 0; j < GameSize; j++)
                {
                    if (_fields[i, j].Type != FieldType.GeneralField)
                    {
                        sum += _fields[i, j].NumberOfPeople * 5;
                    }
                }
            }

            _money += sum;
            _incomeList.Add(new BudgetRecord("Adóbevétel", sum));
            OnIncomeListChanged();
        }
        
        public (bool[,] routeExists, (int, int)[,] parents, int[,] distance) BreadthFirst((int x, int y) source)
        {
            Queue<(int, int)> q = new Queue<(int, int)>();
            bool[,] used = new bool[GAMESIZE,GAMESIZE];
            int[,] distance = new int[GAMESIZE, GAMESIZE];
            (int x, int y)[,] parents = new (int,int)[GAMESIZE, GAMESIZE];
            if (!ValidCoordinates(source)) return (used, parents, distance);

            q.Enqueue(source);
            used[source.x, source.y] = true;
            parents[source.x, source.y] = (-1, -1);
            while (q.Count != 0)
            {
                (int x, int y) v = q.Dequeue();
                foreach ((int x, int y) u in GetAdjacentCoordinates((v.x, v.y)))
                    if (!used[u.x, u.y] && Fields[u.x,u.y].Building != null)
                    {
                        used[u.x,u.y] = true;
                        if (Fields[u.x,u.y].Building!.Type == BuildingType.Road)
                            q.Enqueue(u);
                        distance[u.x, u.y] = distance[v.x, v.y] + 1;
                        parents[u.x, u.y] = v;
                    }
            }   
            
            return (used, parents, distance);
        }
        public Queue<(int x, int y)> CalculateRoute((int x, int y) start, (int x, int y) end)
        {
            Queue<(int x, int y)> route = new Queue<(int x, int y)>();
            if (!ValidCoordinates(start) || !ValidCoordinates(end))
                return route;
            var (used, parents, distance) = BreadthFirst((start.x, start.y));
            if (used[start.x, start.y])
            {
                (int x, int y) v = start;
                while (v != (-1, -1))
                {
                    route.Enqueue(v);
                    v = parents[v.x, v.y];
                }
            }
            return route;
        }

        public void MakeZone(int x, int y, FieldType newFieldType)
        {
            if (_fields[x, y].Type == FieldType.GeneralField && _fields[x, y].Building == null)
            {
                _fields[x, y].Type = newFieldType;
                OnMatrixChanged((x, y));

                _money -= _zonePrices[newFieldType].price;
                _expenseList.Add(new BudgetRecord("Zónalerakás", _zonePrices[newFieldType].price));
                OnExpenseListChanged();
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
                    _fields[x, y].Building = new PeopleBuilding(newBuildingType);
                    OnMatrixChanged((x, y));

                    break;
                case BuildingType.Road:
                    if (_fields[x, y].Type == FieldType.GeneralField && _fields[x, y].Building == null)
                    {
                        _fields[x, y].Building = new Road();
                        OnMatrixChanged((x, y));

                        _money -= _buildingPrices[newBuildingType].price;
                        _expenseList.Add(new BudgetRecord("Útlerakás", _buildingPrices[newBuildingType].price));
                        OnExpenseListChanged();
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
                        if (coords.x >= GameSize || coords.y >= GameSize || _fields[coords.x, coords.y].Type != FieldType.GeneralField || _fields[coords.x, coords.y].Building != null)
                        {
                            throw new CannotBuildException("Ide nem építhetsz ilyen épületet.");
                        }
                    }

                    foreach ((int x, int y) coords in building.Coordinates)
                    {
                        _fields[coords.x, coords.y].Building = building;
                        OnMatrixChanged((coords.x, coords.y));
                    }
                    _money -= _buildingPrices[newBuildingType].price;

                    _expenseList.Add(new BudgetRecord("Épületlerakás", _buildingPrices[newBuildingType].price));
                    OnExpenseListChanged();

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
                        _incomeList.Add(new BudgetRecord("Zónarombolás", _zonePrices[_fields[x, y].Type].returnPrice));
                        OnIncomeListChanged();

                        _fields[x, y].Type = FieldType.GeneralField;
                        OnMatrixChanged((x, y));
                    }
                    break;
                case FieldType.GeneralField:
                    switch (_fields[x, y].Building)
                    {
                        case null:
                            break;
                        case Road:
                            /* need to check if every building is still accessible after destroyation */
                            /* maintence cost needs to be handled  */
                            /* certain percantage of the price must be returned */

                            _money += _buildingPrices[_fields[x, y].Building!.Type].returnPrice;
                            _incomeList.Add(new BudgetRecord("Útrombolás", _buildingPrices[_fields[x, y].Building!.Type].returnPrice));
                            OnIncomeListChanged();

                            _fields[x, y].Building = null;
                            OnMatrixChanged((x, y));

                          break;
                        default:
                            /* maintence cost needs to be handled  */
                            /* certain percantage of the price must be returned */
                            /* additional effects needs to be handled (eg. happiness of nearby people) */

                            _money += _buildingPrices[_fields[x, y].Building!.Type].returnPrice;
                            _incomeList.Add(new BudgetRecord("Útrombolás", _buildingPrices[_fields[x, y].Building!.Type].returnPrice));
                            OnIncomeListChanged();

                            foreach ((int x, int y) coords in ((ServiceBuilding)_fields[x, y].Building!).Coordinates)
                            {
                                _fields[coords.x, coords.y].Building = null;
                                OnMatrixChanged((coords.x, coords.y));
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
        /// Invoking GameAdvanced event.
        /// </summary>
        private void OnGameAdvanced()
        {
            GameAdvanced?.Invoke(this, new GameEventArgs(_gameTime, _money, _population));
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
        /// Invoking ExpenseListChanged event.
        /// </summary>
        private void OnGameSpeedChanged()
        {
            GameSpeedChanged?.Invoke(this, _gameSpeed);
        }

        /// <summary>
        /// Invoking GameOver event.
        /// </summary>
        private void OnGameOver()
        {
            GameOver?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
