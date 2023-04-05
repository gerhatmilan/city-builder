using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityPersistance.Persistance
{
    public class FileDataAccess : IDataAcces
    {
        public override Task<SimCityPersistance> LoadAsync(string path);
        public override Task SaveAsync(string path, SimCityPersistance persistance);
    }
}
