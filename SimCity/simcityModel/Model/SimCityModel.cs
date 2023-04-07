using simcityPersistance.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Security.Cryptography;

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
            _people = new List<Person>();
            _incomeList = new List<BudgetRecord>();
            _expenseList = new List<BudgetRecord>();

            InitializeGame();
        }

        #endregion

        #region Public methods

        public void InitializeGame()
        {
            _gameTime = DateTime.Now;
            _gameSpeed = GameSpeed.Normal;
            _population = 0;
            _money = 1000;
            _happiness = 0;
        }

        public void AdvanceTime() { }

        public void MakeZone(int x, int y, FieldType newFieldType)
        {
            if (_fields[x, y].Type == FieldType.GeneralField && _fields[x, y].Building == null)
            {
                _fields[x, y].Type = newFieldType;
                OnMatrixChanged((x, y));
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
                        if (_fields[coords.x, coords.y].Type != FieldType.GeneralField || _fields[coords.x, coords.y].Building != null)
                        {
                            throw new CannotBuildException("Ide nem építhetsz ilyen épületet.");
                        }
                    }

                    foreach ((int x, int y) coords in building.Coordinates)
                    {
                        _fields[coords.x, coords.y].Building = building;
                        OnMatrixChanged((coords.x, coords.y));
                    }
                    break;
            }
        }

        public void Destroy(int x, int y)
        {

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
        /// Invoking GameOver event.
        /// </summary>
        private void OnGameOver()
        {
            GameOver?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}