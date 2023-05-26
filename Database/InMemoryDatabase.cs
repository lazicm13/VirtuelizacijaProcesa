using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Database
{
    public class InMemoryDatabase
    {
        public static Dictionary<int, Load> db = new Dictionary<int, Load>();
        public static Dictionary<int, ImportedFile> importedFiles = new Dictionary<int, ImportedFile>();
    }
}
