using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VehicleType { Car, Firecar, None }
    public class Vehicle
    {
        private Field position;
        public Queue<(int x, int y)> route;
    }
}
