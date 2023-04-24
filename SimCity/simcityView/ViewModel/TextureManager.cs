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

        #endregion
        #region constructor
        public TextureManager(SimCityModel model)
        {
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
            _floorTextures[4] = UriToImageBrush(@"~\..\View\Textures\parking_asphalt.png");


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


            /*
            _buildingTextures[1] = UriToBitmapImage(@"~\..\Textures\tree_pine_02.png");
            _buildingTextures[2] = UriToBitmapImage(@"~\..\Textures\building_medium_blue_a.png");
            _buildingTextures[3] = UriToBitmapImage(@"~\..\Textures\building_tall_yellow_a.png");
            _buildingTextures[4] = UriToBitmapImage(@"~\..\Textures\fire_station_a.png");
            _buildingTextures[5] = UriToBitmapImage(@"~\..\Textures\light_pole_b.png");
            _buildingTextures[6] = UriToBitmapImage(@"~\..\Textures\building_tall_blue_a.png");
            _buildingTextures[7] = 
            _buildingTextures[8] = UriToBitmapImage(@"~\..\Textures\police_station_a.png");
            _buildingTextures[9] = UriToBitmapImage(@"~\..\Textures\warehouse_orange_a.png");
            
            _buildingTextures[11] = UriToBitmapImage(@"~\..\Textures\road_sign_b.png");
            _buildingTextures[12] = UriToBitmapImage(@"~\..\Textures\rocks.png");
            */




        }

        private int residentalBuildingHelper(BuildingType? buildT, int lvl)
        {
            if (buildT == null)
            {
                return 2;
            }
            switch (buildT)
            {
                case BuildingType.Home:
                    if (lvl == 1)
                    {
                        return 3;
                    }
                    return 4;
                default: return 1;
            }
        }
        private int industrialBuildingHelper(BuildingType? buildT, int lvl)
        {
            if (buildT == null)
            {
                return 5;
            }
            switch (buildT)
            {
                case BuildingType.Industry:
                    if (lvl == 1)
                    {
                        return 6;
                    }
                    return 7;
                default: return 1;
            }
        }
        private int officeBuildingHelper(BuildingType? buildT, int lvl)
        {
            if (buildT == null)
            {
                return 8;
            }
            switch (buildT)
            {
                case BuildingType.Industry:
                    if (lvl == 1)
                    {
                        return 9;
                    }
                    return 10;
                default: return 1;
            }
        }
        private int generalFloorHelper(BuildingType? buildT) 
        {

            return -1;
        }

        private int generalBuildingHelper(BuildingType? buildT, int variation)
        {

            return -1;
        }

        #endregion
        #region public functions

        public (int floor,int building) GetTextureFromInformation(FieldType zone, BuildingType? buildT, int variation)
        {
            (int floor, int building) sendBack = (0,0);
            switch (zone)
            {
                case FieldType.ResidentalZone: sendBack.floor = 1; sendBack.building    =  residentalBuildingHelper(buildT, variation); break;
                case FieldType.IndustrialZone: sendBack.floor = 2; sendBack.building    =  industrialBuildingHelper(buildT, variation); break;
                case FieldType.OfficeZone: sendBack.floor = 3; sendBack.building        =  officeBuildingHelper(buildT, variation); break;
                case FieldType.GeneralField: sendBack.floor = generalFloorHelper(buildT); sendBack.building = generalBuildingHelper(buildT, variation); break;
            }
            return sendBack;

        }

        #endregion
        #endregion
    
    }
}
