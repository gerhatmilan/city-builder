using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public class CannotSendFireTruckException : Exception
    {
        public CannotSendFireTruckException() : base() { }
        public CannotSendFireTruckException(string message) : base(message) { }
    }
}