using simcityModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityView.ViewModel
{
    internal class SimCityViewModel : ViewModelBase
    {
        #region variables


        private SimCityModel _model;
        private string _infoText = string.Empty;
        #endregion

        #region props

        public ObservableCollection<BudgetItem> Income { get; set; }
        public ObservableCollection<BudgetItem> Expense { get; set; }
        public ObservableCollection<Block> Cells { get; set; }
        public string InfoText { get { return _infoText; } 
                                 set { _infoText = value; OnPropertyChanged(nameof(InfoText)); } }
        #endregion

        #region constructor
        public SimCityViewModel(SimCityModel model)
        {
            _model= model;
            Cells = new ObservableCollection<Block>();
            Income = new ObservableCollection<BudgetItem>();
            Expense = new ObservableCollection<BudgetItem>();
            
            updateInfoText();
            fillCells(10);
            fillBudgets();
        }

        #endregion

        #region functions

        private void fillCells(int size)
        {
            Cells.Clear();
            for(int x = 0; x<size; x++)
            {
                for(int y = 0; y<size; y++)
                {
                    Block b = new Block();
                    b.X = x;
                    b.Y= y;
                    b.BuildingTexture = @"~\..\Textures\building_medium_blue_a.png";
                    b.ClickCom = new DelegateCommand(param => { });
                    Cells.Add(b);
                }
            }
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
            InfoText = "Dátum: 2023/04/02\t|\tPénz: 99999$\t|\tLakosság: 100 fő";
        }

        #endregion
    }
}
