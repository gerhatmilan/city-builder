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

        #endregion

        #region props

        public ObservableCollection<Block> Cells { get; set; }

        #endregion

        #region constructor
        public SimCityViewModel(SimCityModel model)
        {
            _model= model;
            Cells = new ObservableCollection<Block>();
            fillCells(10);
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

        #endregion
    }
}
