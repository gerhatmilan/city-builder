using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public class CannotDestroyException : Exception
    {
        public CannotDestroyException(string message) : base(message) { }
    }
}