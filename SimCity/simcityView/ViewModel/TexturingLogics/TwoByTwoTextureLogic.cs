using simcityModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace simcityView.ViewModel.TexturingLogics
{
    internal class TwoByTwoTextureLogic : ITextureLogic
    {
        #region variables
        private int _stadiumOffset = 0;
        #endregion
        #region Props
        public ImageBrush[] FloorTextures { get; private set; }
        public BitmapImage[] BuildingTextures { get; private set; }
        public SimCityModel Model { get; set; }
        public SimCityViewModel ViewModel { get; set; }
        #endregion
        #region Contructor
        public TwoByTwoTextureLogic(SimCityModel Model, SimCityViewModel ViewModel, ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures)
        {
            this.ViewModel = ViewModel;
            this.Model = Model;
            if (FloorTextures.Length != 1)
            {
                throw new ArgumentException("FloorTexture size must be 1!");
            }
            if (BuildingTextures.Length != 8)
            {
                throw new ArgumentException("BuildingTexture size must be 8!");
            }
            this.FloorTextures = FloorTextures;
            this.BuildingTextures = BuildingTextures;

        }
        #endregion
        #region Texturing functions
        public void SetLogicalAllTextures(int x, int y)
        {
            SetLogicalFloorTextures(x, y);
            SetLogicalBuildingTextures(x, y);
        }
        public void SetLogicalFloorTextures(int x, int y)
        {
            ViewModel.Cells[ViewModel.CoordsToListIndex(x, y)].FloorTexture = FloorTextures[0];
        }
        public void SetLogicalBuildingTextures(int x, int y)
        {
            if (_stadiumOffset == 4)
            {
                _stadiumOffset = 0;
            }
            int _fireOffset = 0;
            if (Model.Fields[x, y].Building.OnFire)
            {
                _fireOffset = 4;
            }
            ViewModel.Cells[ViewModel.CoordsToListIndex(x, y)].BuildingTexture = BuildingTextures[_stadiumOffset + _fireOffset];
            _stadiumOffset++;
        }
        public void UpdateWithLogicalTexture(int x, int y)
        {

        }
        #endregion
    }
}
