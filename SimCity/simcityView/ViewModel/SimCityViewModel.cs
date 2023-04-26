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
        private float _playFieldX = 250f;
        private float _playFieldY = 250f;
        private float _playFieldZoom = 1f;
        private string _mouseStateText = "";
        private string _currentAction = "Build";
        private string _currentFieldType = "Re";
        private string _currentBuildingType = "Ro";
        private int _selectedTab = 0;
        private bool _flipBuldozeMode = false;
        private int _time = 0;

        #endregion

        #region Props
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

        #endregion
        #region DelegateCommands
        public DelegateCommand MovePlayFieldUpDown { get; set; }
        public DelegateCommand MovePlayFieldLeftRight { get; set; }
        public DelegateCommand ZoomPlayField { get; set; }
        public DelegateCommand SelectedBuildable { get; set; }
        public DelegateCommand FlipBuldozeMode { get; set; }
        public DelegateCommand TimeSet { get; set; }
        
        #endregion
        #endregion

        #region Constructor
        public SimCityViewModel(SimCityModel model)
        {
            _model= model;
            _model.IncomeListChanged += new EventHandler<List<BudgetRecord>>(model_UpdateIncomeList);
            _model.ExpenseListChanged += new EventHandler<List<BudgetRecord>>(model_UpdateExpenseList);
            _model.GameInfoChanged += new EventHandler<GameEventArgs>(model_UpdateInfoText);
            _model.MatrixChanged += new EventHandler<(int, int)>(model_MatrixChanged);

            _textureManager = new TextureManager(_model);

            Cells = new ObservableCollection<Block>();
            Income = new ObservableCollection<BudgetItem>();
            Expense = new ObservableCollection<BudgetItem>();

            MovePlayFieldUpDown = new DelegateCommand(param => PlayFieldY += (float)param! * (1/PlayFieldZoom)); //Up is positive, down is negative param
            MovePlayFieldLeftRight = new DelegateCommand(param => PlayFieldX += (float)param! * (1 / PlayFieldZoom)); //Right is positive, left is negative param
            ZoomPlayField = new DelegateCommand(param => PlayFieldZoom += (float)param!);  //Zoom is positive, minimize is negative param


            SelectedBuildable = new DelegateCommand(param => SelectedBuildableSorter((string)param!));
            FlipBuldozeMode = new DelegateCommand(param =>
            {
                if (_flipBuldozeMode)
                {
                    _flipBuldozeMode = false;
                    _currentAction = "Build";
                }
                else
                {
                    _currentAction = "Buldoze";
                    _flipBuldozeMode= true;
                }
                UpdateMouseStateText();

            });

            TimeSet = new DelegateCommand(param => {
                int num = int.Parse((string)param!);
                if (num != _time)
                {
                    switch (num)
                    {
                        case 0: 
                        case 1: 
                        case 2:
                        case 3: _time = num; UpdateMouseStateText(); break;
                        default: break;
                    }
                }
            
            });
            
            
            

            UpdateMouseStateText();
            fillCells();
            fillIncome();
            fillExpense();
                
        }

        #endregion

        #region ViewModel functions
        #region Cell functions
        
        private int CoordsToListIndex(int x, int y)
        {
            return (x + y * _model.GameSize);
        }

        private void fillCells()
        {
            Cells.Clear();
            (int floor, int building) info = _textureManager.getStarterTextures();
            ImageBrush floor = _textureManager.getFloorTexture(info.floor);
            BitmapImage building = _textureManager.getBuildingTexture(info.building);
            

            for(int y = 0; y< _model.GameSize; y++)
            {
                
                for(int x = 0; x< _model.GameSize; x++)
                {
                    
                    Block b = new Block(floor, building);
                    b.X = x;
                    b.Y= y;
                    b.UpdateToolTipText = new DelegateCommand(param =>
                    {
                        b.ToolTipText = "X: " + b.X + " " +
                                        "Y: " + b.Y + "\n" +
                                        "Kapacitás: " + _model.Fields[b.X,b.Y].NumberOfPeople + "/" + _model.Fields[b.X, b.Y].Capacity;
                    });
                    b.UpdateToolTipText.Execute(this);
                    b.ClickCom = new DelegateCommand(param => {
                        try
                        {
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

        private void model_UpdateIncomeList(object? s, List<BudgetRecord> e)
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

        private void model_UpdateExpenseList(object? s, List<BudgetRecord> e)
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

        private void model_UpdateInfoText(object? s, GameEventArgs e )
        {
            InfoText = "Dátum: " + e.GameTime.ToString() + "\t|\tPénz: " + e.Money + "💸\t|\tLakosság: " + e.Population + " fő";
        }

        private void model_MatrixChanged(object? s, (int X, int Y) e)
        {
            (int floor, int building) info = _textureManager.GetTextureFromInformation(e.X, e.Y);
            Cells[CoordsToListIndex(e.X, e.Y)].FloorTexture = _textureManager.getFloorTexture(info.floor);
            Cells[CoordsToListIndex(e.X, e.Y)].BuildingTexture = _textureManager.getBuildingTexture(info.building);

        }







        #endregion
    }
}
