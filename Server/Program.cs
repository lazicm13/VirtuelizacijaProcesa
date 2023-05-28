using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common;
using System.IO;
using Database;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(ServiceLoad));
            host.Open();

            Load load = new Load();
            Dictionary<int, Load> loadData = new Dictionary<int, Load>();
            Dictionary<int, Load> loadData1 = new Dictionary<int, Load>();

            ServiceLoad sl = new ServiceLoad();

            loadData = sl.LoadDataFromCsv("csv/measured/measured_2023_01_17.csv", "csv/forecast/forecast_2023_01_17.csv");
            // sl.LoadDatabaseEntry(loadData.Values.ToList());
            //  sl.ImportedFileDatabaseEntry(InMemoryDatabase.importedFiles.Values.ToList());
            //  sl.AuditDatabaseEntry(InMemoryDatabase.auditFiles.Values.ToList());
            loadData1 = sl.ReadXML("csv/measured/measured_2023_01_17.csv");

            foreach(Load l in loadData1.Values)
            {
                Console.WriteLine(l);
            }

            Console.WriteLine("Pritisnite [Enter] za zaustavljanje servera!");
            Console.ReadLine();
        }

    }
}
