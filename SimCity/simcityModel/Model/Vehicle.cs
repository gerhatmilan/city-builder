using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public enum VehicleType { Car, Firecar, None }

    public class Vehicle
    {
        private Field position;
        public Queue<(int x, int y)> route;
    }
}
