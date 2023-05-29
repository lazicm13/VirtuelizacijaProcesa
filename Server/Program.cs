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


            Load load = new Load();
            Dictionary<int, Load> loadData = new Dictionary<int, Load>();
            Dictionary<int, Load> loadData1 = new Dictionary<int, Load>();

            ServiceLoad sl = new ServiceLoad();
            string path = "csv/";

         //  loadData = sl.LoadDataFromCsv("csv/measured/measured_2023_01_17.csv", "csv/forecast/forecast_2023_01_17.csv");
           //sl.LoadDatabaseEntry(loadData.Values.ToList());
            //  sl.ImportedFileDatabaseEntry(InMemoryDatabase.importedFiles.Values.ToList());
            //  sl.AuditDatabaseEntry(InMemoryDatabase.auditFiles.Values.ToList());
          //  loadData1 = sl.ReadXML("LoadDB.xml");

            using (ServiceHost host = new ServiceHost(typeof(ServiceLoad)))
            {
                host.Open();
                Console.WriteLine("Server je pokrenut...");
                Console.WriteLine("Pritisnite [Enter] za zaustavljanje servera!");
                Console.ReadKey();
                if (sl.receivedMessage.Contains("forecast"))
                {
                    path += "forecast/" + sl.receivedMessage;
                    loadData = sl.LoadForecastDataFromCSV(path);
                    sl.LoadDatabaseEntry(loadData.Values.ToList());
                    Console.WriteLine(path);

                }
                else if (sl.receivedMessage.Contains("measured"))
                {
                    path += "measured/" + sl.receivedMessage;
                    loadData = sl.LoadMeasuredDataFromCSV(path);
                    sl.LoadDatabaseEntry(loadData.Values.ToList());
                    Console.WriteLine(path);
                }
                host.Close();
            }

            

           



        }
    }
}
