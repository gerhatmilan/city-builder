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
            if (FloorTextures.Length != 16)
            {
                throw new ArgumentException("FloorTexture size must be 16!");
            }
            if (BuildingTextures.Length != 21)
            {
                throw new ArgumentException("BuildingTexture size must be 21!");
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
            //return;
            Road r = (Road)Model.Fields[x, y].Building!;
            int offset = -1; //0 -- car | 1 -- firetruck | 2 --  (car - car) | 3 -- (car - firetruck) | 4 -- (firetruck - firetruck)
            int dir = -1; // 0 - 1 - 2 - 3
            if(r.Vehicles.Count == 0)
            {
                ViewModel.Cells[ViewModel.CoordsToListIndex(x, y)].BuildingTexture = BuildingTextures[0];
                return;
            }else if(r.Vehicles.Count == 1)
            {
                dir = (int)(r.Vehicles[0].CurrentDirection);
                offset = (int)(r.Vehicles[0].Type);
            }
            else //vehicles count is 2
            {
                int zeroV = (int)(r.Vehicles[0].Type);
                int zeroD = (int)(r.Vehicles[0].CurrentDirection);
                int oneV = (int)(r.Vehicles[1].Type);
                int oneD = (int)(r.Vehicles[1].CurrentDirection);

                offset = 2 + zeroV + oneV;
                if (offset == 3)
                {
                    if (zeroV == 1) //fst is a firetruck
                    {
                        dir = zeroD;
                    }
                    else //snd is a firetruck
                    {
                        dir = oneD;
                    }
                }
                else
                {
                    dir = zeroD;
                }
                //vehicles list count max is 2
                //firetruck direction is strong, other vehicle's dir is always opposite
                //2 firectruck case!!!!!
                //two car case: directions are always opposite;

            }
            ViewModel.Cells[ViewModel.CoordsToListIndex(x, y)].BuildingTexture = BuildingTextures[1 + offset*4 + dir];
        }
        public void UpdateWithLogicalTexture(int x, int y)
        {
           SetLogicalAllTextures(x, y);
        }
        #endregion
    }
}
