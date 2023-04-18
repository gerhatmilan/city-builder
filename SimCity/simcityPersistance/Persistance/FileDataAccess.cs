using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityPersistance.Persistance
{
    public class FileDataAccess : IDataAccess
    {
        public async Task<SimCityPersistance> LoadAsync(string path) { return new SimCityPersistance(); }
        public async Task SaveAsync(string path, SimCityPersistance persistance) { }
    }
}
