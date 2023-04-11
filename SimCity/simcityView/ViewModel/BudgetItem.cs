using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityView.ViewModel
{
    internal class BudgetItem : ViewModelBase
    {

        private string _moneyText = "alma";
        public string MoneyText { get { return _moneyText; } 
                                  set { _moneyText = value; OnPropertyChanged(nameof(MoneyText)); } }
    }
}
