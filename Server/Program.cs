using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common;
using System.IO;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(ServiceLoad));


            host.Open();

            Load load = new Load();
            Dictionary<string, Load> loadData = new Dictionary<string, Load>();
            Dictionary<string, Load> loadData1 = new Dictionary<string, Load>();

            

            ServiceLoad sl = new ServiceLoad();

            loadData = sl.LoadDataFromCsv("csv/measured/measured_2023_01_23.csv", "csv/forecast/forecast_2023_01_23.csv");
            sl.CalculateDeviation();
            sl.DatabaseEntry();
           
            foreach (Load l in loadData.Values)
            {    
                Console.WriteLine(l);
            }


            Console.WriteLine("Pritisnite [Enter] za zaustavljanje servera!");
            Console.ReadLine();
        }

        
    }
}
