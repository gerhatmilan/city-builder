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
    internal class ConnectedRoadTextureLogic : ITextureLogic
    {
        #region Props
        public ImageBrush[] FloorTextures { get; private set; }
        public BitmapImage[] BuildingTextures { get; private set; }
        public SimCityModel Model { get; set; }
        public SimCityViewModel ViewModel { get; set; }
        #endregion
        #region Contructor
        public ConnectedRoadTextureLogic(SimCityModel Model, SimCityViewModel ViewModel, ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures)
        {
            this.ViewModel = ViewModel;
            this.Model = Model;
            if (FloorTextures.Length != 1)
            {
                throw new ArgumentException("FloorTexture size must be 1!");
            }
            if (BuildingTextures.Length != 16)
            {
                throw new ArgumentException("BuildingTexture size must be 16!");
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
            int[] dirs = { 0, 0, 0, 0 }; //(-1,0);(0,-1);(1,0);(0,1)
            int rotationX = -1;
            int rotationY = 0;
            for (int ind = 0; ind < 4; ind++)
            {
                int cellX = x + rotationX;
                int cellY = y + rotationY;
                if (ViewModel.isValidCoord(cellX, cellY))
                {
                    Building? f = Model.Fields[cellX, cellY].Building;
                    if (f != null && f.Type == BuildingType.Road)
                    {
                        dirs[ind] = 1;
                    }
                }

                int rotationZ = rotationX;
                rotationX = rotationY;
                rotationY = rotationZ;
                rotationX *= -1;
            }
            int roadIndex = dirs[0] * 1 + dirs[1] * 2 + dirs[2] * 4 + dirs[3] * 8;
            ViewModel.Cells[ViewModel.CoordsToListIndex(x, y)].FloorTexture = FloorTextures[roadIndex];

        }
        public void SetLogicalBuildingTextures(int x, int y)
        {
            ViewModel.Cells[ViewModel.CoordsToListIndex(x, y)].BuildingTexture = BuildingTextures[0];
        }
        public void UpdateWithLogicalTexture(int x, int y)
        {
           SetLogicalAllTextures(x, y);
        }
        #endregion
    }
}
