using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public abstract class Building
    {
        protected BuildingType _type;

        public BuildingType Type { get => _type; }

        public Building(BuildingType type)
        {
            _type = type;     
        }
    }

    public class PeopleBuilding : Building
    {
        private List<Person> _people;
        private bool _onFire;
        private float _fireProb;

        public List<Person> People { get => _people; }
        public bool OnFire { get => OnFire; }

        public PeopleBuilding(BuildingType type) : base(type)
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
        private (int x, int y) _coordinates;

        public int Price { get => _price; }
        public int MaintenceCost { get => _maintenanceCost; }
        public bool Onfire { get => _onFire; }
        public List<(int, int)> Coordinates
        {
            get
            {
                List<(int, int)> returnList = new List<(int, int)>();

                switch (_type)
                {
                    case BuildingType.PoliceStation:
                    case BuildingType.FireStation:
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

        public ServiceBuilding((int x, int y) coordinates, BuildingType type) : base(type)
        {
            _coordinates = coordinates;
        }

        public void CalculateFire() { }
    }

    public class Road : Building
    {
        private Vehicle _vehicle;
        private int _price;
        private int _maintenanceCost;

        public Vehicle Vehicle { get => _vehicle; }
        public int Price { get => _price; }
        public int MaintenceCost { get => _maintenanceCost; }

        public Road() : base(BuildingType.Road)
        {
            
        }
    }
}
