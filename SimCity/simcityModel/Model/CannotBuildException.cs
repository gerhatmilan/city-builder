﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public class CannotBuildException : Exception
    {
        public CannotBuildException(string message) : base(message) { }
    }
}
