#define UP 

using simcityModel.Model;
using simcityView.ViewModel.TexturingLogics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace simcityView.ViewModel
{

    internal class TextureManager
    {

        #region variables
        private ImageBrush[] _floorTextures = new ImageBrush[21];
        private BitmapImage[] _buildingTextures = new BitmapImage[23];
        private SimCityModel _model;
        private SimCityViewModel _vm;
        private ITextureLogic[] _textureLogics = new ITextureLogic[5];
        
        private int _modelSize;
        private int _stadiumOffset = 0;

        #endregion
        #region constructor

        public TextureManager(SimCityModel model, SimCityViewModel view)
        {
            _model = model;
            _modelSize = _model.Fields.GetLength(0);
            fillFloorTextures();
            fillBuildingTextures();
            _vm = view;
        }

        #endregion
        #region functions
        #region private functions

        private int CoordsToListIndex(int x, int y)
        {
            return (x + y * _model.GameSize);
        }
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
            
            //0
            _floorTextures[4] = UriToImageBrush(@"~\..\View\Textures\parking_asphalt.png"); // 0,0,0,0
             //1
            _floorTextures[12] = UriToImageBrush(@"~\..\View\Textures\street_end_up.png");     // 0 0 0 1
            _floorTextures[8] = UriToImageBrush(@"~\..\View\Textures\street_end_left.png");  // 0 0 1 0
            _floorTextures[6] = UriToImageBrush(@"~\..\View\Textures\street_end_down.png");  // 0 1 0 0
            _floorTextures[5] = UriToImageBrush(@"~\..\View\Textures\street_end_right.png"); // 1 0 0 0
            //2
            _floorTextures[14] = UriToImageBrush(@"~\..\View\Textures\street_straight_upDown.png");    // 0 1 0 1
            _floorTextures[9] = UriToImageBrush(@"~\..\View\Textures\street_straight_leftRight.png"); // 1 0 1 0
            
            _floorTextures[13] = UriToImageBrush(@"~\..\View\Textures\street_corner_leftDown.png");       // 1 0 0 1
            _floorTextures[16] = UriToImageBrush(@"~\..\View\Textures\street_corner_rightDown.png");     // 0 0 1 1
            _floorTextures[7] = UriToImageBrush(@"~\..\View\Textures\street_corner_leftUp.png");    // 1 1 0 0
            _floorTextures[10] = UriToImageBrush(@"~\..\View\Textures\street_corner_rightUp.png"); // 0 1 1 0
            
            //3
            _floorTextures[17] = UriToImageBrush(@"~\..\View\Textures\street_t_up.png");     // 1 0 1 1
            _floorTextures[11] = UriToImageBrush(@"~\..\View\Textures\street_t_down.png");      // 1 1 1 0
            _floorTextures[15] = UriToImageBrush(@"~\..\View\Textures\street_t_right.png");  // 1 1 0 1
            _floorTextures[18] = UriToImageBrush(@"~\..\View\Textures\street_t_left.png");  // 0 1 1 1

            //4
            _floorTextures[19] = UriToImageBrush(@"~\..\View\Textures\street_xing.png"); // 1 1 1 1

            _floorTextures[20] = UriToImageBrush(@"~\..\View\Textures\ground_grass_dark.png"); // 1 1 1 1







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

            _buildingTextures[11] = UriToBitmapImage(@"~\..\Textures\stadium1.png");
            _buildingTextures[12] = UriToBitmapImage(@"~\..\Textures\stadium2.png");
            _buildingTextures[13] = UriToBitmapImage(@"~\..\Textures\stadium3.png");
            _buildingTextures[14] = UriToBitmapImage(@"~\..\Textures\stadium4.png");

            _buildingTextures[15] = UriToBitmapImage(@"~\..\Textures\police_station_a.png");
            _buildingTextures[16] = UriToBitmapImage(@"~\..\Textures\fire_station_a.png");

            _buildingTextures[17] = UriToBitmapImage(@"~\..\Textures\tree_pine_02.png");

            _buildingTextures[18] = UriToBitmapImage(@"~\..\Textures\car_white_down.png");
            _buildingTextures[19] = UriToBitmapImage(@"~\..\Textures\car_white_up.png");
            _buildingTextures[20] = UriToBitmapImage(@"~\..\Textures\car_white_left.png");
            _buildingTextures[21] = UriToBitmapImage(@"~\..\Textures\car_white_right.png");

            _buildingTextures[22] = UriToBitmapImage(@"~\..\Textures\stadium_thumbnail");

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
                case BuildingType.OfficeBuilding:
                    if (peopleNum <cap/3)
                    {
                        return 9;
                    }
                    return 10;
                case null: return 8;
                default: return 1;
            }
        }
        private int generalFloorHelper(BuildingType? buildT, int x, int y) 
        {
            switch (buildT)
            {
                case BuildingType.Stadium: return 20;
                case BuildingType.PoliceStation:
                case BuildingType.FireStation: return 3;
                case BuildingType.Road: return roadHelper(x,y,true);
                case null: return 1;
                default: return 0;
            }  
        }
        private bool isValidCoord(int x, int y)
        {
            return -1 < x && -1 < y && x < _modelSize && y < _modelSize;
        }


        private int roadHelper(int centerX, int centerY, bool isMiddle)
        {
            //              left   down   right  up
            int[] dirs =    { 0    , 0,     0,    0 }; //(-1,0);(0,-1);(1,0);(0,1)
            int x = -1;
            int y = 0;
            for(int ind = 0; ind<4; ind++)
            {
                int cellX = centerX + x;
                int cellY = centerY + y;
                if (isValidCoord(cellX,cellY))
                {
                    Building? f = _model.Fields[cellX, cellY].Building;
                    if (f != null && f.Type==BuildingType.Road)
                    {
                        dirs[ind] = 1;
                        if (isMiddle)
                        {
                           
                            int neighbourIndex = roadHelper(cellX, cellY, false);
                            _vm.Cells[CoordsToListIndex(cellX,cellY)].FloorTexture = _floorTextures[neighbourIndex];
                        }
                    }
                }
                
                int z = x;
                x = y;
                y = z;
                x *=-1;
            }
            int roadIndex = dirs[0] * 1 + dirs[1] * 2 + dirs[2] * 4 + dirs[3] * 8;
            return 4 + roadIndex;
        }

        private int generalBuildingHelper(BuildingType? buildT, int x, int y)
        {
            switch (buildT)
            {
                case BuildingType.Road: return carHelper(x,y);
                case BuildingType.Stadium: return stadiumHelper(x,y);
                case BuildingType.PoliceStation: return 15;
                case BuildingType.FireStation: return 16;
                case null: return 1;
                default: return 0;
            }
        }

        private int carHelper(int x, int y)
        {
            //Road r = (Road)_model.Fields[x, y].Building;
            
            return 1;
        }

        private int stadiumHelper(int x, int y)
        {
            if(_stadiumOffset == 4)
            {
                _stadiumOffset = 0;
            }
            int index = 11 + _stadiumOffset;
            _stadiumOffset++;
            return index;
        }

        #endregion
        #region public functions

        #region updating in progress
        public void Init()
        {
            //0 -- generalfield
            //1 -- home
            //2 -- office
            //3 -- industry
            //4 -- road
            //5 -- firestation
            //6 -- policestation
            //7 -- stadium
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures = generalFieldTextures();
            _textureLogics[0] = new OneByOneTextureLogic(_model, _vm,textures.FloorTextures,textures.BuildingTextures);



        }
        private (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) generalFieldTextures()
        {
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures;
            textures.FloorTextures = new ImageBrush[1];
            textures.FloorTextures[0] = _floorTextures[0];
            textures.BuildingTextures = new BitmapImage[1];
            textures.BuildingTextures[0] = _buildingTextures[1];
            return textures;
            
        }
        

        public void SetTexture(int x, int y)
        {
            Field f = _model.Fields[x, y];
            if(f.Building == null)
            {
                _textureLogics[(int)f.Type].SetLogicalBuildingTextures(x, y);
            }
            else
            {
                _textureLogics[(int)f.Building.Type].SetLogicalBuildingTextures(x, y);
            }
        }
        public void UpdteTextureAround(int centerX, int centerY)
        {
            int x = -1;
            int y = 0;
            for (int ind = 0; ind < 4; ind++)
            {
                int cellX = centerX + x;
                int cellY = centerY + y;
                if (isValidCoord(cellX, cellY))
                {
                    Field f = _model.Fields[x, y];
                    if (f.Building == null)
                    {
                        _textureLogics[(int)f.Type].UpdateWithLogicalTexture(x, y);
                    }
                    else
                    {
                        _textureLogics[(int)f.Building.Type].UpdateWithLogicalTexture(x, y);
                    }
                }
                int z = x;
                x = y;
                y = z;
                x *= -1;
            }
        }
        #endregion
        
        public (ImageBrush floor, BitmapImage building) getStarterTextures()
        {
            return (_floorTextures[1], _buildingTextures[17]);
        }

        public void SetTextureFromInformation(int x, int y)
        {
            (int floor, int building) whatToSet = (0,0);
            
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
            switch (zone)
            {
                case FieldType.ResidentalZone: whatToSet.floor = 1; whatToSet.building    =  residentalBuildingHelper(buildT, f.NumberOfPeople,f.Capacity); break;
                case FieldType.IndustrialZone: whatToSet.floor = 2; whatToSet.building    =  industrialBuildingHelper(buildT, f.NumberOfPeople,f.Capacity); break;
                case FieldType.OfficeZone: whatToSet.floor = 3; whatToSet.building        =  officeBuildingHelper(buildT, f.NumberOfPeople,f.Capacity); break;
                case FieldType.GeneralField: whatToSet.floor = generalFloorHelper(buildT,x,y); whatToSet.building = generalBuildingHelper(buildT, x,y); break;
            }
            _vm.Cells[CoordsToListIndex(x,y)].BuildingTexture = _buildingTextures[whatToSet.building];
            _vm.Cells[CoordsToListIndex(x,y)].FloorTexture = _floorTextures[whatToSet.floor];


        }
       


        #endregion
        #endregion

    }
}
