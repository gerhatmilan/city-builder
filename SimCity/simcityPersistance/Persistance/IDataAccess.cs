using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityPersistance.Persistance
{
    public interface IDataAccess
    {
        public Task<String> LoadAsync(string path);
        public Task SaveAsync(string path, string data);
    }
}
