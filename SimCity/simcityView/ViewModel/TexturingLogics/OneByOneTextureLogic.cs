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
    internal class OneByOneTextureLogic : ITextureLogic
    {
        #region Props
        public ImageBrush[] FloorTextures { get; private set; }
        public BitmapImage[] BuildingTextures { get; private set; }
        public SimCityModel Model { get; set; }
        public SimCityViewModel ViewModel { get; set; }
        #endregion
        #region Contructor
        public OneByOneTextureLogic(SimCityModel Model, SimCityViewModel ViewModel, ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures)
        {
            this.ViewModel = ViewModel;
            this.Model = Model;
            if (FloorTextures.Length != 1)
            {
                throw new ArgumentException("FloorTexture size must be 1!");
            }
            if (BuildingTextures.Length != 2)
            {
                throw new ArgumentException("BuildingTexture size must be 2!");
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
            
            if (Model.Fields[x, y].Building != null && Model.Fields[x, y].Building!.OnFire)
            {
                ViewModel.Cells[ViewModel.CoordsToListIndex(x, y)].BuildingTexture = BuildingTextures[1];
            }
            else
            {
                ViewModel.Cells[ViewModel.CoordsToListIndex(x, y)].BuildingTexture = BuildingTextures[0];
            }
            
            
        }
        public void UpdateWithLogicalTexture(int x, int y)
        {

        }
        #endregion
    }
}
