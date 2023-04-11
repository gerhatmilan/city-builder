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
    public enum BuildingType { Industry, OfficeBuilding, Home, Stadium, PoliceStation, FireStation, Road }
    public enum Vehicle { Car, Firecar }
    
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
        /// Game info changed event.
        /// Gets invoked every time date, money or population count changes in the model.
        /// As a parameter, it passes GameEventArgs to the subscriber, which holds data of the game.
        /// </summary>
        public event EventHandler<GameEventArgs>? GameInfoChanged;

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
                    _fields[i, j] = new Field();
                }
            }

            _people = new List<Person>();
            _incomeList = new List<BudgetRecord>();
            _expenseList = new List<BudgetRecord>();
        }

        #endregion

        #region Private methods

        private static int CalculateReturnPrice(int originalPrice)
        {
            return Convert.ToInt32(originalPrice * PRICERETURN_MULTIPLIER);
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

            OnGameInfoChanged();
        }

        public void AdvanceTime()
        {
            /* ... */
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
            OnGameInfoChanged();
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
                        OnGameInfoChanged();
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
                        _incomeList.Add(new BudgetRecord("Zónarombolás", _zonePrices[_fields[x, y].Type].returnPrice));
                        OnIncomeListChanged();
                        OnGameInfoChanged();

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
                            OnGameInfoChanged();

                            _fields[x, y].Building = null;
                            OnMatrixChanged((x, y));

                          break;
                        default:
                            /* maintence cost needs to be handled  */
                            /* certain percantage of the price must be returned */
                            /* additional effects needs to be handled (eg. happiness of nearby people) */

                            _money += _buildingPrices[_fields[x, y].Building!.Type].returnPrice;
                            _incomeList.Add(new BudgetRecord("Épületrombolás", _buildingPrices[_fields[x, y].Building!.Type].returnPrice));
                            OnIncomeListChanged();
                            OnGameInfoChanged();

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

        #endregion
    }
}
