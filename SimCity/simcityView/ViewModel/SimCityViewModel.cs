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
        private ImageBrush[] _floorTextures = new ImageBrush[20];
        private BitmapImage[] _buildingTextures = new BitmapImage[20];
        private int _playFieldSize = 10;
        private float _playFieldX = 250f;
        private float _playFieldY = 250f;
        private float _playFieldZoom = 1f;
        private float _zoomSpeed = 0.1f;
        private float _camSpeed = 25f;
        private string _mouseStateText = "";
        private string _currentAction = "Build";
        private string _currentFieldType = "Re";
        private string _currentBuildingType = "Ro";
        private int _selectedTab = 0;
        private bool _flipBuldozeMode = false;

        #endregion

        #region props

        public ObservableCollection<BudgetItem> Income { get; set; }
        public ObservableCollection<BudgetItem> Expense { get; set; }
        public ObservableCollection<Block> Cells { get; set; }
        public string InfoText
        {
            get { return _infoText; }
            set { _infoText = value; OnPropertyChanged(nameof(InfoText)); }
        }
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
            set { _playFieldZoom = value; OnPropertyChanged(nameof(PlayFieldZoom)); }
        }
        public string MouseStateText
        {
            get { return _mouseStateText; }
            set { _mouseStateText = value; OnPropertyChanged(nameof(MouseStateText)); }
        }
        public int SelectedTab
        {
            get { return _selectedTab; }
            set { _selectedTab = value; OnPropertyChanged(nameof(SelectedTab)); UpdateMouseStateText(); }
        }
        


        public DelegateCommand MovePlayFieldUp { get; set; }
        public DelegateCommand MovePlayFieldDown { get; set; }
        public DelegateCommand MovePlayFieldLeft { get; set; }
        public DelegateCommand MovePlayFieldRight { get; set; }
        public DelegateCommand ZoomPlayField { get; set; }
        public DelegateCommand MinimizePlayField { get; set; }
        public DelegateCommand SelectedBuildable { get; set; }
        public DelegateCommand FlipBuldozeMode { get; set; }

        #endregion

        #region constructor
        public SimCityViewModel(SimCityModel model)
        {
            _model= model;
            Cells = new ObservableCollection<Block>();
            Income = new ObservableCollection<BudgetItem>();
            Expense = new ObservableCollection<BudgetItem>();

            MovePlayFieldUp = new DelegateCommand(param => PlayFieldY+= _camSpeed * (1/PlayFieldZoom));
            MovePlayFieldDown = new DelegateCommand(param => PlayFieldY -= _camSpeed * (1 / PlayFieldZoom));
            MovePlayFieldLeft = new DelegateCommand(param => PlayFieldX += _camSpeed * (1 / PlayFieldZoom));
            MovePlayFieldRight = new DelegateCommand(param => PlayFieldX -= _camSpeed * (1 / PlayFieldZoom));
            
            ZoomPlayField = new DelegateCommand(param => PlayFieldZoom += _zoomSpeed);
            MinimizePlayField = new DelegateCommand(param => PlayFieldZoom -= _zoomSpeed);
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

            }); ;

            fillFloorTextures();
            fillBuildingTextures();

            UpdateMouseStateText();
            updateInfoText();
            fillCells();
            fillBudgets();
        }

        #endregion

        #region functions

        private int CoordsToListIndex(int x, int y)
        {
            return (x + y * _playFieldSize);
        }

        private void fillCells()
        {
            Cells.Clear();
            for(int x = 0; x<_playFieldSize; x++)
            {
                
                for(int y = 0; y< _playFieldSize; y++)
                {
                    Block b = new Block(_floorTextures[1], _buildingTextures[1]);
                    b.X = x;
                    b.Y= y;
                    b.UpdateToolTipText = new DelegateCommand(param => b.ToolTipText = _playFieldX.ToString());
                    b.ClickCom = new DelegateCommand(param => { 
                        
                    
                    
                    
                    });
                    Cells.Add(b);
                }
                
            }
           
            Cells[CoordsToListIndex(2,2)].BuildingTexture = _buildingTextures[2];
            Cells[CoordsToListIndex(3, 2)].BuildingTexture = _buildingTextures[2];
            Cells[CoordsToListIndex(4, 2)].BuildingTexture = _buildingTextures[2];

        }

        private void fillBudgets()
        {
            Income.Clear();
            Expense.Clear();
            
            BudgetItem incomeHeader = new BudgetItem();
            incomeHeader.MoneyText = "Bevétel:";
            Income.Add(incomeHeader);

            BudgetItem expenseHeader = new BudgetItem();
            expenseHeader.MoneyText = "Kiadások:";
            Expense.Add(expenseHeader);
        }

        private void updateInfoText()
        {
            InfoText = "Dátum: 2023/04/02\t|\tPénz: 99999💸\t|\tLakosság: 100 fő";
        }

        private void fillFloorTextures()
        {
            
            _floorTextures[0] = UriToImageBrush(@"~\..\View\Textures\missing_texture.png");
            _floorTextures[1] = UriToImageBrush(@"~\..\View\Textures\ground_grass.png");
            _floorTextures[2] = UriToImageBrush(@"~\..\View\Textures\street_straight");


        }

        private void fillBuildingTextures()
        {
            
            _buildingTextures[0] = UriToBitmapImage(@"~\..\Textures\missing_texture.png");
            _buildingTextures[1] = UriToBitmapImage(@"~\..\Textures\tree_pine_02.png");
            _buildingTextures[2] = UriToBitmapImage(@"~\..\Textures\building_medium_blue_a.png");
            _buildingTextures[3] = UriToBitmapImage(@"~\..\Textures\building_tall_yellow_a.png");
        }

        private ImageBrush UriToImageBrush(string s)
        {
            return new ImageBrush(UriToBitmapImage(s));
        }
        private BitmapImage UriToBitmapImage(string s)
        {
            return new BitmapImage(new Uri(s, UriKind.Relative));
        }
        private void UpdateMouseStateText()
        {
            switch (SelectedTab)
            {
                case 0: MouseStateText = _currentAction + " " + SelectedTab.ToString() + " " + _currentFieldType;    break;
                case 1: MouseStateText = _currentAction + " " + SelectedTab.ToString() + " " + _currentBuildingType; break;
                case 2: MouseStateText = _currentAction + " " + SelectedTab.ToString();                              break;
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
    }
}
