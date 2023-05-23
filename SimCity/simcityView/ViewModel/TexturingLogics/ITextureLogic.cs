using simcityModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace simcityView.ViewModel.TexturingLogics
{
    internal interface ITextureLogic
    {
        //Idea: Use theese classes from the TextureManager, where we give an enum a texturing logic!
        //These classes modify each Block in the ViewModel according to the given logic.

        #region Props
        public ImageBrush[] FloorTextures { get;}
        public BitmapImage[] BuildingTextures { get;}
        public SimCityModel Model { get; set; }
        public SimCityViewModel ViewModel { get; set; }
        #endregion
        #region Texturing functions
        public void SetLogicalAllTextures(int x, int y);
        public void SetLogicalFloorTextures(int x, int y);
        public void SetLogicalBuildingTextures(int x, int y);
        public void UpdateWithLogicalTexture(int x, int y);
        #endregion
    }
}
