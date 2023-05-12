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
        private BitmapImage[] _buildingTextures = new BitmapImage[30];
        private SimCityModel _model;
        private SimCityViewModel _vm;
        private ITextureLogic[] _textureLogics;
        

        #endregion
        #region constructor

        public TextureManager(SimCityModel model, SimCityViewModel view)
        {
            _model = model;
            _vm = view;
            int a = Enum.GetNames(typeof(FieldType)).Length;
            int b = Enum.GetNames(typeof(BuildingType)).Length;
            int modelEnumMaxsSize;
            if (a > b)
            {
                modelEnumMaxsSize = a;
            }
            else
            {
                modelEnumMaxsSize = b;
                modelEnumMaxsSize++;
            }

            _textureLogics = new ITextureLogic[modelEnumMaxsSize];
            Init();
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
            _buildingTextures[13] = UriToBitmapImage(@"~\..\Textures\stadium4.png");
            _buildingTextures[14] = UriToBitmapImage(@"~\..\Textures\stadium3.png");

            _buildingTextures[15] = UriToBitmapImage(@"~\..\Textures\police_station_a.png");
            _buildingTextures[16] = UriToBitmapImage(@"~\..\Textures\fire_station_a.png");

            _buildingTextures[17] = UriToBitmapImage(@"~\..\Textures\tree_pine_02.png");

            _buildingTextures[18] = UriToBitmapImage(@"~\..\Textures\f_police_station_a.png");
            _buildingTextures[19] = UriToBitmapImage(@"~\..\Textures\f_fire_station_a.png");
            _buildingTextures[20] = UriToBitmapImage(@"~\..\Textures\f_house_small_yellow_a.png");
            _buildingTextures[21] = UriToBitmapImage(@"~\..\Textures\f_house_large_green_a.png");
            _buildingTextures[22] = UriToBitmapImage(@"~\..\Textures\f_barn_red_a.png");
            _buildingTextures[23] = UriToBitmapImage(@"~\..\Textures\f_warehouse_orange_a.png");
            _buildingTextures[24] = UriToBitmapImage(@"~\..\Textures\f_building_medium_blue_a.png");
            _buildingTextures[25] = UriToBitmapImage(@"~\..\Textures\f_building_tall_yellow_a.png");
            _buildingTextures[26] = UriToBitmapImage(@"~\..\Textures\f_stadium1.png");
            _buildingTextures[27] = UriToBitmapImage(@"~\..\Textures\f_stadium2.png");
            _buildingTextures[28] = UriToBitmapImage(@"~\..\Textures\f_stadium4.png");
            _buildingTextures[29] = UriToBitmapImage(@"~\..\Textures\f_stadium3.png");


        }

        #endregion
        #region public functions

       
        public void Init()
        {
            fillBuildingTextures();
            fillFloorTextures();
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
            textures = homeFieldTextures();
            _textureLogics[1] = new ZoneTextureLogic(_model, _vm, textures.FloorTextures, textures.BuildingTextures);
            textures = officeFieldTextures();
            _textureLogics[2] = new ZoneTextureLogic(_model, _vm, textures.FloorTextures, textures.BuildingTextures);
            textures = industryFieldTextures();
            _textureLogics[3] = new ZoneTextureLogic(_model, _vm, textures.FloorTextures, textures.BuildingTextures);
            textures = roadFieldTextures();
            _textureLogics[4] = new ConnectedRoadTextureLogic(_model, _vm, textures.FloorTextures, textures.BuildingTextures);
            textures = fireStationFieldTextures();
            _textureLogics[5] = new OneByOneTextureLogic(_model, _vm, textures.FloorTextures, textures.BuildingTextures);
            textures = policeStationFieldTextures();
            _textureLogics[6] = new OneByOneTextureLogic(_model, _vm, textures.FloorTextures, textures.BuildingTextures);
            textures = stadiumFieldTextures();
            _textureLogics[7] = new TwoByTwoTextureLogic(_model, _vm, textures.FloorTextures, textures.BuildingTextures);
            textures = undefinedFieldTextures();
            for(int i = 8; i<_textureLogics.Length; i++)
            {
                _textureLogics[i] = new OneByOneTextureLogic(_model, _vm, textures.FloorTextures, textures.BuildingTextures);
            }
        }
        private (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) undefinedFieldTextures()
        {
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures;
            textures.FloorTextures = new ImageBrush[1];
            textures.FloorTextures[0] = _floorTextures[0];
            textures.BuildingTextures = new BitmapImage[2];
            textures.BuildingTextures[0] = _buildingTextures[0];
            textures.BuildingTextures[1] = _buildingTextures[0];
            return textures;

        }
        private (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) generalFieldTextures()
        {
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures;
            textures.FloorTextures = new ImageBrush[1];
            textures.FloorTextures[0] = _floorTextures[1];
            textures.BuildingTextures = new BitmapImage[2];
            textures.BuildingTextures[0] = _buildingTextures[1];
            textures.BuildingTextures[1] = _buildingTextures[1];
            return textures;
            
        }
        private (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) homeFieldTextures()
        {
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures;
            textures.FloorTextures = new ImageBrush[1];
            textures.FloorTextures[0] = _floorTextures[1];
            textures.BuildingTextures = new BitmapImage[5];
            textures.BuildingTextures[0] = _buildingTextures[2];
            textures.BuildingTextures[1] = _buildingTextures[3];
            textures.BuildingTextures[2] = _buildingTextures[4];
            textures.BuildingTextures[3] = _buildingTextures[20];
            textures.BuildingTextures[4] = _buildingTextures[21];


            return textures;

        }
        private (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) industryFieldTextures()
        {
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures;
            textures.FloorTextures = new ImageBrush[1];
            textures.FloorTextures[0] = _floorTextures[2];
            textures.BuildingTextures = new BitmapImage[5];
            textures.BuildingTextures[0] = _buildingTextures[5];
            textures.BuildingTextures[1] = _buildingTextures[6];
            textures.BuildingTextures[2] = _buildingTextures[7];
            textures.BuildingTextures[3] = _buildingTextures[22];
            textures.BuildingTextures[4] = _buildingTextures[23];


            return textures;

        }
        private (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) officeFieldTextures()
        {
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures;
            textures.FloorTextures = new ImageBrush[1];
            textures.FloorTextures[0] = _floorTextures[3];
            textures.BuildingTextures = new BitmapImage[5];
            textures.BuildingTextures[0] = _buildingTextures[8];
            textures.BuildingTextures[1] = _buildingTextures[9];
            textures.BuildingTextures[2] = _buildingTextures[10];
            textures.BuildingTextures[3] = _buildingTextures[24];
            textures.BuildingTextures[4] = _buildingTextures[25];


            return textures;

        }
        private (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) roadFieldTextures()
        {
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures;
            textures.FloorTextures = new ImageBrush[16];
            //0
            textures.FloorTextures[0] = _floorTextures[4];
            //1
            textures.FloorTextures[8] = _floorTextures[12];
            textures.FloorTextures[4] = _floorTextures[8];
            textures.FloorTextures[2] = _floorTextures[6];
            textures.FloorTextures[1] = _floorTextures[5];
            //2
            textures.FloorTextures[10] = _floorTextures[14];
            textures.FloorTextures[5] = _floorTextures[9];

            textures.FloorTextures[9] = _floorTextures[13];
            textures.FloorTextures[12] = _floorTextures[16];
            textures.FloorTextures[3] = _floorTextures[7];
            textures.FloorTextures[6] = _floorTextures[10];

            //3
            textures.FloorTextures[13] = _floorTextures[17];
            textures.FloorTextures[7] = _floorTextures[11];
            textures.FloorTextures[11] = _floorTextures[15];
            textures.FloorTextures[14] = _floorTextures[18];

            //4
            textures.FloorTextures[15] = _floorTextures[19];
            
            textures.BuildingTextures = new BitmapImage[1];
            textures.BuildingTextures[0] = _buildingTextures[1];
           

            return textures;

        }
        private (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) fireStationFieldTextures()
        {
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures;
            textures.FloorTextures = new ImageBrush[1];
            textures.FloorTextures[0] = _floorTextures[3];
            textures.BuildingTextures = new BitmapImage[2];
            textures.BuildingTextures[0] = _buildingTextures[16];
            textures.BuildingTextures[1] = _buildingTextures[19];

            return textures;

        }
        private (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) policeStationFieldTextures()
        {
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures;
            textures.FloorTextures = new ImageBrush[1];
            textures.FloorTextures[0] = _floorTextures[3];
            textures.BuildingTextures = new BitmapImage[2];
            textures.BuildingTextures[0] = _buildingTextures[15];
            textures.BuildingTextures[1] = _buildingTextures[18];

            return textures;

        }
        private (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) stadiumFieldTextures()
        {
            (ImageBrush[] FloorTextures, BitmapImage[] BuildingTextures) textures;
            textures.FloorTextures = new ImageBrush[1];
            textures.FloorTextures[0] = _floorTextures[3];
            textures.BuildingTextures = new BitmapImage[8];
            textures.BuildingTextures[0] = _buildingTextures[11];
            textures.BuildingTextures[1] = _buildingTextures[12];
            textures.BuildingTextures[2] = _buildingTextures[13];
            textures.BuildingTextures[3] = _buildingTextures[14];
            textures.BuildingTextures[4] = _buildingTextures[26];
            textures.BuildingTextures[5] = _buildingTextures[27];
            textures.BuildingTextures[6] = _buildingTextures[28];
            textures.BuildingTextures[7] = _buildingTextures[29];



            return textures;

        }

        public void SetTexture(int x, int y)
        {
            Field f = _model.Fields[x, y];
            int logicNum=0;
            if(f.Building == null)
            {
                logicNum = (int)f.Type;
                _textureLogics[logicNum].SetLogicalAllTextures(x, y);
            }
            else
            {
                logicNum = (int)f.Building.Type;
                _textureLogics[logicNum].SetLogicalAllTextures(x, y);
            }
            UpdateTextureAround(x, y);
        }
        public void UpdateTextureAround(int centerX, int centerY)
        {
            int x = -1;
            int y = 0;
            for (int ind = 0; ind < 4; ind++)
            {
                int cellX = centerX + x;
                int cellY = centerY + y;
                if (_vm.isValidCoord(cellX, cellY))
                {
                    Field f = _model.Fields[cellX, cellY];
                    if (f.Building == null)
                    {
                        _textureLogics[(int)f.Type].UpdateWithLogicalTexture(cellX, cellY);
                    }
                    else
                    {
                        _textureLogics[(int)f.Building.Type].UpdateWithLogicalTexture(cellX, cellY);
                    }
                }
                int z = x;
                x = y;
                y = z;
                x *= -1;
            }
        }
        public (ImageBrush floor, BitmapImage building) getStarterTextures()
        {
            return (_floorTextures[1], _buildingTextures[17]);
        }
        
        #endregion
        #endregion

    }
}
