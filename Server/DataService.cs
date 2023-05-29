using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Server
{
    public class DataService : IDataService
    {
        public Stream GetData(string path)
        {
            if (!File.Exists(path))
            {
                // baci gresku da ne postoji fajl
            }

            byte[] fileBytes = File.ReadAllBytes(path);
            MemoryStream stream = new MemoryStream(fileBytes);
            return stream;
        }
    }
}
