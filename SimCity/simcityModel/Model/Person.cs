using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public class Person
    {
        private const int INITIAL_HAPPYNESS = 50;
        private const int MAX_HAPPYNESS = 100;

        public Person()
        {
            happyness = INITIAL_HAPPYNESS;
            distanceToWork = 0;
        }

        public int happyness;
        public PeopleBuilding? home;
        public PeopleBuilding? work;
        public int distanceToWork;

    }
}
