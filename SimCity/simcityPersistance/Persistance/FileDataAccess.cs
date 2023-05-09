using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace simcityPersistance.Persistance
{
    public class FileDataAccess : IDataAccess
    {
        public async Task<String> LoadAsync(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    byte[] decryptedBytes = new byte[stream.Length];
                    await stream.ReadAsync(decryptedBytes, 0, (int)stream.Length);

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
                        aes.IV = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

                        using (MemoryStream memoryStream = new MemoryStream(decryptedBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                            {
                                using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                                {
                                    return streamReader.ReadToEnd();
                                }
                            }
                        }
                    }
                }
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
                byte[] jsonBytes = Encoding.UTF8.GetBytes(data);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }; ;
                    aes.IV = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            await cs.WriteAsync(jsonBytes, 0, jsonBytes.Length);
                        }

                        byte[] encryptedBytes = ms.ToArray();

                        using (FileStream fs = new FileStream(path, FileMode.Create))
                        {
                            await fs.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new GameException();
            }
        }
    }
}
