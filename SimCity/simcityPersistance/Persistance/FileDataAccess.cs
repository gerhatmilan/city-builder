using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace simcityPersistance.Persistance
{
    public class FileDataAccess : IDataAccess
    {
        public async Task<String> LoadAsync(string path)
        {
            try
            {
                FileStream stream = new FileStream(path, FileMode.Open);
                byte[] bytes = new byte[stream.Length];
                await stream.ReadAsync(bytes, 0, (int)stream.Length);
                string data = Encoding.UTF8.GetString(bytes);
                stream.Close();
                return data;
            }
            catch (Exception ex)
            {
                throw new GameException();
            }
        }
        public async Task SaveAsync(string path, string data)
        {
            try
            {
                FileStream stream = new FileStream(path, FileMode.Create);
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                stream.Close();
            }
            catch(Exception ex)
            {
                throw new GameException();
            }
        }
    }
}
