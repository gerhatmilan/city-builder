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
    internal class TextureManager
    {
        #region variables
        private ImageBrush[] _floorTextures = new ImageBrush[20];
        private BitmapImage[] _buildingTextures = new BitmapImage[20];
        private SimCityModel _model;
        private int _currX = 0;
        private int _currY = 0;
        private int _modelSize;

        #endregion
        #region constructor

        public TextureManager(SimCityModel model)
        {
            _model = model;
            _modelSize = _model.Fields.GetLength(0);
            fillFloorTextures();
            fillBuildingTextures();
        }

        #endregion

        #region functions
        #region private functions

        private ImageBrush UriToImageBrush(string s)
        {
            return new ImageBrush(UriToBitmapImage(s));
        }
        private BitmapImage UriToBitmapImage(string s)
        {
            return new BitmapImage(new Uri(s, UriKind.Relative));
        }

        private void fillFloorTextures()
        {
            
            _floorTextures[0] = UriToImageBrush(@"~\..\View\Textures\missing_texture.png");
            _floorTextures[1] = UriToImageBrush(@"~\..\View\Textures\ground_grass.png");
            _floorTextures[2] = UriToImageBrush(@"~\..\View\Textures\ground_dirt.png");
            _floorTextures[3] = UriToImageBrush(@"~\..\View\Textures\ground_asphalt.png");
            //2
            _floorTextures[4] = UriToImageBrush(@"~\..\View\Textures\parking_asphalt.png");
            _floorTextures[5] = UriToImageBrush(@"~\..\View\Textures\street_straight_upDown.png");
            _floorTextures[6] = UriToImageBrush(@"~\..\View\Textures\street_straight_leftRight.png");
            //2
            _floorTextures[7] = UriToImageBrush(@"~\..\View\Textures\street_corner_leftUp.png");
            _floorTextures[8] = UriToImageBrush(@"~\..\View\Textures\street_corner_rightUp.png");
            _floorTextures[9] = UriToImageBrush(@"~\..\View\Textures\street_corner_leftDown.png");
            _floorTextures[10] = UriToImageBrush(@"~\..\View\Textures\street_corner_rightDown.png");
            //3
            _floorTextures[11] = UriToImageBrush(@"~\..\View\Textures\street_t_down.png");
            _floorTextures[12] = UriToImageBrush(@"~\..\View\Textures\street_t_up.png");
            //1
            _floorTextures[13] = UriToImageBrush(@"~\..\View\Textures\street_end_up.png");
            _floorTextures[14] = UriToImageBrush(@"~\..\View\Textures\street_end_right.png");
            _floorTextures[15] = UriToImageBrush(@"~\..\View\Textures\street_end_down.png");
            _floorTextures[16] = UriToImageBrush(@"~\..\View\Textures\street_end_left.png");
            //4
            _floorTextures[17] = UriToImageBrush(@"~\..\View\Textures\street_xing.png");









        }

        private void fillBuildingTextures()
        {

            _buildingTextures[0] = UriToBitmapImage(@"~\..\Textures\missing_texture.png");
            _buildingTextures[1] = UriToBitmapImage(@"~\..\Textures\nothing.png");
            
            _buildingTextures[2] = UriToBitmapImage(@"~\..\Textures\weed_medium.png");
            _buildingTextures[3] = UriToBitmapImage(@"~\..\Textures\house_small_yellow_a.png");
            _buildingTextures[4] = UriToBitmapImage(@"~\..\Textures\house_large_green_a.png");

            _buildingTextures[5] = UriToBitmapImage(@"~\..\Textures\rocks.png");
            _buildingTextures[6] = UriToBitmapImage(@"~\..\Textures\barn_red_a.png");
            _buildingTextures[7] = UriToBitmapImage(@"~\..\Textures\warehouse_orange_a.png");

            _buildingTextures[8] = UriToBitmapImage(@"~\..\Textures\road_sign_b.png");
            _buildingTextures[9] = UriToBitmapImage(@"~\..\Textures\building_medium_blue_a.png");
            _buildingTextures[10] = UriToBitmapImage(@"~\..\Textures\building_tall_yellow_a.png");

            _buildingTextures[11] = UriToBitmapImage(@"~\..\Textures\building_tall_blue_a.png");
            _buildingTextures[12] = UriToBitmapImage(@"~\..\Textures\building_tall_blue_a.png");
            _buildingTextures[13] = UriToBitmapImage(@"~\..\Textures\building_tall_blue_a.png");
            _buildingTextures[14] = UriToBitmapImage(@"~\..\Textures\building_tall_blue_a.png");

            _buildingTextures[15] = UriToBitmapImage(@"~\..\Textures\police_station_a.png");
            _buildingTextures[16] = UriToBitmapImage(@"~\..\Textures\fire_station_a.png");

            _buildingTextures[17] = UriToBitmapImage(@"~\..\Textures\tree_pine_02.png");
        }

        

        private int residentalBuildingHelper(BuildingType? buildT, int peopleNum, int cap)
        {
            switch (buildT)
            {
                case BuildingType.Home:
                    if (peopleNum < cap/2)
                    {
                        return 3;
                    }
                    return 4;
                case null: return 2;
                default: return 1;
            }
        }
        private int industrialBuildingHelper(BuildingType? buildT, int peopleNum, int cap)
        {
            switch (buildT)
            {
                case BuildingType.Industry:
                    if (peopleNum < cap/4)
                    {
                        return 6;
                    }
                    return 7;
                case null:return 5;
                default: return 1;
            }
        }
        private int officeBuildingHelper(BuildingType? buildT, int peopleNum, int cap)
        {
           
            switch (buildT)
            {
                case BuildingType.Industry:
                    if (peopleNum <cap/3)
                    {
                        return 9;
                    }
                    return 10;
                case null: return 8;
                default: return 1;
            }
        }
        private int generalFloorHelper(BuildingType? buildT, int variation) 
        {
            switch (buildT)
            {
                case BuildingType.Stadium:
                case BuildingType.PoliceStation:
                case BuildingType.FireStation: return 3;
                case BuildingType.Road: return roadHelper(variation);
                case null: return 1;
                default: return 0;
            }  
        }
        private bool isValidCoord(int x, int y)
        {
            return -1 < x && -1 < y && x < _modelSize && y < _modelSize;
        }


        private int roadHelper(int variation)
        {
            int neighborCount = 0;
            bool[] dirs = { false, false, false, false }; //-1,-1;-1,1;1,-1;1,1
            int ind = 0;
            for(int x = -1; x<=1; x += 2)
            {
                
                for(int y = -1; y<=1; y += 2)
                {
                    if (isValidCoord(x, y))
                    {
                        Building? f = _model.Fields[x, y].Building;
                        if (f != null && f.Type==BuildingType.Road)
                        {
                            dirs[ind] = true;
                            neighborCount++;
                        }
                    }
                    ind++;
                }
                ind++;
            }
            

            return 4;
        }

        private int generalBuildingHelper(BuildingType? buildT, int variation)
        {
            switch (buildT)
            {
                case BuildingType.Road: return carHelper(variation);
                case BuildingType.Stadium: return stadiumHelper(variation);
                case BuildingType.PoliceStation: return 15;
                case BuildingType.FireStation: return 16;
                case null: return 1;
                default: return 0;
            }
        }

        private int carHelper(int variation)
        {
            return 1;
        }

        private int stadiumHelper(int variation)
        {
            return 11;
        }

        #endregion
        #region public functions

        public (int floor, int building) getStarterTextures()
        {
            return (1, 17);
        }

        public (int floor,int building) GetTextureFromInformation(int x, int y)
        {
            (int floor, int building) sendBack = (0,0);
            _currX = x;
            _currY = y;
            Field f = _model.Fields[x, y];
            FieldType zone = f.Type;
            BuildingType? buildT;
            if (f.Building == null)
            {
                buildT = null;
            }
            else
            {
                buildT = f.Building.Type;
            }
            int variation = 0;
            switch (zone)
            {
                case FieldType.ResidentalZone: sendBack.floor = 1; sendBack.building    =  residentalBuildingHelper(buildT, f.NumberOfPeople,f.Capacity); break;
                case FieldType.IndustrialZone: sendBack.floor = 2; sendBack.building    =  industrialBuildingHelper(buildT, f.NumberOfPeople,f.Capacity); break;
                case FieldType.OfficeZone: sendBack.floor = 3; sendBack.building        =  officeBuildingHelper(buildT, f.NumberOfPeople,f.Capacity); break;
                case FieldType.GeneralField: sendBack.floor = generalFloorHelper(buildT,variation); sendBack.building = generalBuildingHelper(buildT, variation); break;
            }
            return sendBack;

        }
        public ImageBrush getFloorTexture(int index)
        {
            return _floorTextures[index];
        }

        public BitmapImage getBuildingTexture(int index)
        {
            return _buildingTextures[index];
        }


        #endregion
        #endregion

    }
}
