using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public abstract class Building
    {
        private BuildingType _type;
        private (int x, int y) _coordinates;

        public BuildingType Type { get => _type; }
        public List<(int, int)> Coordinates { 
            get
            {
                List<(int, int)> returnList = new List<(int, int)>();

                switch (_type)
                {
                    case BuildingType.Industry:
                    case BuildingType.OfficeBuilding:
                    case BuildingType.Home:
                    case BuildingType.PoliceStation:
                    case BuildingType.FireStation:
                    case BuildingType.Road:                     
                        returnList.Add((_coordinates.x, _coordinates.y));
                        break;
                    case BuildingType.Stadium:
                        returnList.Add((_coordinates.x, _coordinates.y));
                        returnList.Add((_coordinates.x, _coordinates.y + 1));
                        returnList.Add((_coordinates.x + 1, _coordinates.y));
                        returnList.Add((_coordinates.x + 1, _coordinates.y + 1));
                        break;
                }

                return returnList;
            }
        }

        public Building((int x, int y) coordinates, BuildingType type)
        {
            _type = type;
            _coordinates = coordinates;        
        }
    }

    public class PeopleBuilding : Building
    {
        private List<Person> _people;
        private bool _onFire;
        private float _fireProb;

        public List<Person> People { get => _people; }
        public bool OnFire { get => OnFire; }

        public PeopleBuilding((int x, int y) coordinates, BuildingType type) : base(coordinates, type)
        {
            _people = new List<Person>();
        }

        public void CalculateFire() { }
    }

    public class ServiceBuilding : Building
    {
        private int _price;
        private int _maintenanceCost;
        private bool _onFire;
        private float _fireProb;

        public int Price { get => _price; }
        public int MaintenceCost { get => _maintenanceCost; }
        public bool Onfire { get => _onFire; }

        public ServiceBuilding((int x, int y) coordinates,  BuildingType type) : base(coordinates, type)
        {

        }

        public void CalculateFire() { }
    }

    public class Road : Building
    {
        private Vehicle? _vehicle;
        private int _price;
        private int _maintenanceCost;

        public Vehicle? Vehicle { get => _vehicle; }
        public int Price { get => _price; }
        public int MaintenceCost { get => _maintenanceCost; }

        public Road((int x, int y) coordinates) : base(coordinates, BuildingType.Road)
        {
            
        }
    }
}
