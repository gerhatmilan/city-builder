using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public interface Building
    {
        public BuildingType type;
    }

    public class PeopleBuilding : Building
    {
        public Person[] people;
        public bool onFire;
        public float fireProb;
        public void calculateFire();
    }

    public class ServiceBuilding : Building
    {
        public int price;
        public int maintanenceCost;
        public int Size;
    }

    public class Road : Building
    {
        public Vehicle? vehicle;
    }
}
