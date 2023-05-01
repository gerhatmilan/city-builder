using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityPersistance.Persistance
{
    public class SimCityPersistance
    {
        public (int fieldType, int buildingType, bool onFire, float fireProb, int happyness)[,] _fields;
        public ((int homeX, int homeY), (int workX, int workY), int happyness)[] _people;
        public int _gameSize;
        public int _happyness;
        public int _money;
        public int _income;
        public int _population;
    }
}
