using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityPersistance.Persistance
{
    public class SimCityPersistance
    {
        public (int, bool, float)[,] _fields;
        public ((int, int), (int, int))[] _people;
        public int _gameSize;
        public int _happyness;
        public int _money;
        public int _income;
        public int _population;
    }
}
