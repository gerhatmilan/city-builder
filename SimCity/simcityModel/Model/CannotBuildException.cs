using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    /// <summary>
    /// Custom exception class for build actions.
    /// </summary>
    public class CannotBuildException : Exception
    {
        /// <summary>
        /// Initializes a new instance of CannotBuildException class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public CannotBuildException(string message) : base(message) { }
    }
}
