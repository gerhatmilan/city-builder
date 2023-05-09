using simcityModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace simcityView.ViewModel
{
    internal interface TextureLogic
    {
        //Idea: Use theese classes from the TextureManager, where we give an enum a texturing logic!
        //These classes modify each Block in the ViewModel according to the given logic.
        
        #region Props
        public ImageBrush[] FloorTextures { get; set;  }
        public BitmapImage[] BuildingTextures { get; set; }
        public SimCityModel Model { get; set; }
        #endregion
        #region Texturing functions
        public void SetLogicalAllTextures();
        public void SetLogicalFloorTextures();
        public void SetLogicalBuildingTextures();
        public void UpdateWithLogicalTexture();
        #endregion
    }
}
