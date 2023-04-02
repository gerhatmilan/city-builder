using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace simcityView.ViewModel
{
    internal class Block : ViewModelBase
    {

        #region variables
        
        public int X;
        public int Y;
        private string _floorTexture;
        private string _buildingTexture;



        #endregion

        #region props
        public string FloorTexture
        {
            get { return _floorTexture; }
            set { _floorTexture = value; OnPropertyChanged(nameof(FloorTexture)); }
        }
        public string BuildingTexture
        {
            get { return _buildingTexture; }
            set { _buildingTexture = value; OnPropertyChanged(nameof(BuildingTexture)); }
        }



        public DelegateCommand? ClickCom { get; set; }

        #endregion

        #region contructor

        public Block()
        {
            FloorTexture = @"~\..\View\Textures\missing_texture.png";
            BuildingTexture = @"~\..\Textures\missing_texture.png";
        }

        #endregion

        #region functions

       
        

        #endregion

    }
}
