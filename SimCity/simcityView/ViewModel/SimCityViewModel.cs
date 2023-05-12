using simcityModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace simcityView.ViewModel
{
    internal class SimCityViewModel : ViewModelBase
    {
        #region variables

        private SimCityModel _model;
        private string _infoText = string.Empty;
        private TextureManager _textureManager;
        private float _playFieldX = 150;
        private float _playFieldY = 320f;
        private float _playFieldZoom = 1f;
        private string _mouseStateText = "";
        private string _currentAction = "Build";
        private string _currentFieldType = "Re";
        private string _currentBuildingType = "Ro";
        private int _selectedTab = 0;
        private bool _flipBuldozeMode = false;
        private int _time = 0;
        private bool _gameIsNotOver = true;
        private bool _timeReset = true;

        #endregion

        #region Props
        #region GameStateProps
        public bool GameIsNotOver { 
            get { return _gameIsNotOver; } 
            set { _gameIsNotOver = value; OnPropertyChanged(nameof(GameIsNotOver)); } 
        }
        public bool AntiBuldozer
        {
            get { return !_flipBuldozeMode; }
        }
        public bool Buldozer
        {
            get { return _flipBuldozeMode; }
            set { _flipBuldozeMode = value; OnPropertyChanged(nameof(Buldozer)); OnPropertyChanged(nameof(AntiBuldozer)); }
        }
        #endregion
        #region ObservableProps

        public ObservableCollection<BudgetItem> Income { get; set; }
        public ObservableCollection<BudgetItem> Expense { get; set; }
        public ObservableCollection<Block> Cells { get; set; }

        #endregion
        #region CamProps

        public float PlayFieldX
        {
            get { return _playFieldX; }
            set { _playFieldX = value; OnPropertyChanged(nameof(PlayFieldX)); }
        }
        public float PlayFieldY
        {
            get { return _playFieldY; }
            set { _playFieldY = value; OnPropertyChanged(nameof(PlayFieldY)); }
        }
        public float PlayFieldZoom
        {
            get { return _playFieldZoom; }
            set { _playFieldZoom = Math.Clamp(value,0.1f,10.0f); OnPropertyChanged(nameof(PlayFieldZoom)); }
        }
        public bool inFocus { get; set; } = true;

        #endregion
        #region DebugProps

        public string MouseStateText
        {
            get { return _mouseStateText; }
            set { _mouseStateText = value; OnPropertyChanged(nameof(MouseStateText)); }
        }
        public string InfoText
        {
            get { return _infoText; }
            set { _infoText = value; OnPropertyChanged(nameof(InfoText)); }
        }
        public int SelectedTab
        {
            get { return _selectedTab; }
            set { _selectedTab = value; OnPropertyChanged(nameof(SelectedTab)); UpdateMouseStateText(); }
        }
        public bool TimeReset
        {
            get { return _timeReset; }
            set { _timeReset = value; OnPropertyChanged(nameof(_timeReset)); UpdateMouseStateText(); }
        }

        #endregion
        #region DelegateCommands
        public DelegateCommand MovePlayFieldUpDown { get; set; }
        public DelegateCommand MovePlayFieldLeftRight { get; set; }
        public DelegateCommand ZoomPlayField { get; set; }
        public DelegateCommand SelectedBuildable { get; set; }
        public DelegateCommand FlipBuldozeMode { get; set; }
        public DelegateCommand TimeSet { get; set; }
        public DelegateCommand SaveGame { get; set; }
        public DelegateCommand LoadGame { get; set; }
        public DelegateCommand NewGame { get; set; }
        public DelegateCommand ShowHelp { get; set; }
        #endregion
        #region Events
        public event EventHandler? SaveGameEvent;
        public event EventHandler? LoadGameEvent;
        public event EventHandler? NewGameEvent;
        public event EventHandler? ShowHelpEvent;
        public event EventHandler<int>? ChangeTime;
        #endregion
        #endregion

        #region Constructor
        public SimCityViewModel(SimCityModel model)
        {
            _model= model;
            _model.IncomeListChanged += new EventHandler<ObservableCollection<BudgetRecord>>(model_UpdateIncomeList);
            _model.ExpenseListChanged += new EventHandler<ObservableCollection<BudgetRecord>>(model_UpdateExpenseList);
            _model.GameInfoChanged += new EventHandler(model_UpdateInfoText);
            _model.MatrixChanged += new EventHandler<(int, int)>(model_MatrixChanged);
            _model.NumberOfPeopleChanged += new EventHandler<(int, int)>(model_MatrixChanged);

            _textureManager = new TextureManager(_model,this);

            SaveGame = new DelegateCommand(param => SaveGameEvent?.Invoke(this, EventArgs.Empty));
            LoadGame = new DelegateCommand(param => LoadGameEvent?.Invoke(this, EventArgs.Empty));
            NewGame = new DelegateCommand(param => NewGameEvent?.Invoke(this, EventArgs.Empty));
            ShowHelp = new DelegateCommand(param => ShowHelpEvent?.Invoke(this, EventArgs.Empty));

            Cells = new ObservableCollection<Block>();
            Income = new ObservableCollection<BudgetItem>();
            Expense = new ObservableCollection<BudgetItem>();

            MovePlayFieldUpDown = new DelegateCommand(param =>
            {
                if (inFocus)
                {
                    PlayFieldY += (float)param! * (1 / PlayFieldZoom);
                }
            }); //Up is positive, down is negative param
            MovePlayFieldLeftRight = new DelegateCommand(param =>
            {
                if (inFocus)
                {
                    PlayFieldX += (float)param! * (1 / PlayFieldZoom);
                }
            }); //Right is positive, left is negative param
            ZoomPlayField = new DelegateCommand(param =>
            {
                if (inFocus)
                {
                    PlayFieldZoom += (float)param!;
                }
            });  //Zoom is positive, minimize is negative param


            SelectedBuildable = new DelegateCommand(param => SelectedBuildableSorter((string)param!));
            FlipBuldozeMode = new DelegateCommand(param =>
            {
                if (Buldozer)
                {
                    Buldozer = false;
                    _currentAction = "Build";
                }
                else
                {
                    _currentAction = "Buldoze";
                    Buldozer = true;
                }
                UpdateMouseStateText();

            });

            TimeSet = new DelegateCommand(param => {
                int num = int.Parse((string)param!);
                if (num != _time)
                {
                    _time = num; 
                    UpdateMouseStateText();
                    ChangeTime?.Invoke(this, num);
                }
            
            });
            
            
            

            UpdateMouseStateText();
            model_UpdateInfoText(this, EventArgs.Empty);
            fillCells();
            fillIncome();
            fillExpense();               
        }

        #endregion

        #region ViewModel functions
        #region Cell functions
        public int CoordsToListIndex(int x, int y)
        {
            return (x + y * _model.GameSize);
        }
        public bool isValidCoord(int x, int y)
        {
            return -1 < x && -1 < y && x < _model.GameSize && y < _model.GameSize;
        }
        private void fillCells()
        {
            Cells.Clear();
            (ImageBrush floor, BitmapImage building) starterTextures = _textureManager.getStarterTextures();
            

            for(int y = 0; y< _model.GameSize; y++)
            {
                
                for(int x = 0; x< _model.GameSize; x++)
                {
                    
                    Block b = new Block(starterTextures.floor, starterTextures.building);
                    b.X = x;
                    b.Y= y;
                    b.UpdateToolTipText = new DelegateCommand(param =>
                    {
                        b.ToolTipText = "X: " + b.X + " " +
                                        "Y: " + b.Y + "\n" +
                                        "Kapacitás: " + _model.Fields[b.X,b.Y].NumberOfPeople + "/" + _model.Fields[b.X, b.Y].Capacity + "\n" +
                                        "Mező boldogsága: " + (int)Math.Floor(_model.Fields[b.X,b.Y].PeopleHappiness);
                    });
                    b.UpdateToolTipText.Execute(this);
                    b.ClickCom = new DelegateCommand(param => {
                        try
                        {
                            if (_model.Fields[b.X, b.Y].Building != null && _model.Fields[b.X, b.Y].Building!.OnFire)
                            {
                                _model.SendFireTruck((b.X, b.Y));
                                return;
                            }

                            if (_flipBuldozeMode)
                            {
                                _model.Destroy(b.X, b.Y);
                            }
                            else
                            {

                                switch (SelectedTab)
                                {
                                    case 0:

                                        switch (_currentFieldType)
                                        {
                                            case "Re": _model.MakeZone(b.X, b.Y, FieldType.ResidentalZone); break;
                                            case "I": _model.MakeZone(b.X, b.Y, FieldType.IndustrialZone); break;
                                            case "O": _model.MakeZone(b.X, b.Y, FieldType.OfficeZone); break;
                                        }
                                        break;

                                    case 1:

                                        switch (_currentBuildingType)
                                        {
                                            case "Ro": _model.MakeBuilding(b.X, b.Y, BuildingType.Road); break;
                                            case "S": _model.MakeBuilding(b.X, b.Y, BuildingType.Stadium); break;
                                            case "F": _model.MakeBuilding(b.X, b.Y, BuildingType.FireStation); break;
                                            case "P": _model.MakeBuilding(b.X, b.Y, BuildingType.PoliceStation); break;
                                        }
                                        break;
                                    case 2: break;
                                    default: break;
                                }
                            }


                        }
                        catch(Exception e)
                        {

                        }
                        
                    
                    
                    
                    });
                    Cells.Add(b);
                }
                
            }
           

        }

        #endregion
        #region Budget functions

        private void fillIncome()
        {
            Income.Clear();
            BudgetItem incomeHeader = new BudgetItem();
            incomeHeader.MoneyText = "Bevétel:";
            Income.Add(incomeHeader);
        }

        private void fillExpense()
        {
            Expense.Clear();
            BudgetItem expenseHeader = new BudgetItem();
            expenseHeader.MoneyText = "Kiadások:";
            Expense.Add(expenseHeader);
        }

        #endregion
        #region MouseState functions
        
        private void UpdateMouseStateText()
        {
            switch (SelectedTab)
            {
                case 0: MouseStateText = _currentAction + " t:" + _time.ToString() + " " + SelectedTab.ToString() + " " + _currentFieldType;    break;
                case 1: MouseStateText = _currentAction + " t:" + _time.ToString() + " " + SelectedTab.ToString() + " " + _currentBuildingType; break;
                case 2: MouseStateText = _currentAction + " t:" + _time.ToString() + " " + SelectedTab.ToString();                              break;
                default: break;
            }
           
        }

        
        private void SelectedBuildableSorter(string s)
        {
            switch (s)
            {
                case "Re":
                case "I":
                case "O": _currentFieldType = s; UpdateMouseStateText(); break;
                case "Ro":
                case "S":
                case "F":
                case "P": _currentBuildingType= s; UpdateMouseStateText(); break;
                default: break;
            }
        }


        #endregion
        #endregion

        #region Model functions

        private void model_UpdateIncomeList(object? s, ObservableCollection<BudgetRecord> e)
        {


            fillIncome();
            for (int i = e.Count()-1; i>-1; i-- )
            {
                BudgetItem toAdd = new BudgetItem();
                toAdd.MoneyText = e[i].Text + " " + e[i].Amount.ToString() + "💸";
                Income.Add(toAdd);
            }
            OnPropertyChanged(nameof(Income));
        }

        private void model_UpdateExpenseList(object? s, ObservableCollection<BudgetRecord> e)
        {
            fillExpense();
            for (int i = e.Count() - 1; i > -1; i--)
            {
                BudgetItem toAdd = new BudgetItem();
                toAdd.MoneyText = e[i].Text + " " + e[i].Amount.ToString() + "💸";
                Expense.Add(toAdd);
            }
            OnPropertyChanged(nameof(Expense));
        }

        private void model_UpdateInfoText(object? s, EventArgs e)
        {
            int happines = (int)Math.Floor(_model.Happiness);
            string emoji = " 😐";
            if (happines < 25)
            {
                emoji = " 🙁";
            }
            if(happines > 74)
            {
                emoji = " 😁";
            }

            InfoText = "Dátum: " + _model.GameTime.ToString("yyyy. MM. dd.") + "\t|\tPénz: " + _model.Money + "💸\t|\tLakosság: " + _model.Population + " fő\t|\tBoldogság: " + happines + emoji;
        }

        private void model_MatrixChanged(object? s, (int X, int Y) e)
        {
            _textureManager.SetTexture(e.X, e.Y);
        }

        #endregion
    }
}
