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
            Load load = new Load();
            Dictionary<string, Load> loadMeasured = new Dictionary<string, Load>();
            Dictionary<string, Load> loadMeasured1 = new Dictionary<string, Load>();
            loadMeasured = load.LoadData("csv/measured/measured_2023_01_23.csv", "csv/forecast/forecast_2023_01_23.csv");
            loadMeasured1 = load.LoadData("csv/measured/measured_2023_01_24.csv", "csv/forecast/forecast_2023_01_24.csv");

           
            foreach (Load l in loadMeasured.Values)
            {    
                Console.WriteLine(l);
            }
            foreach (Load l in loadMeasured1.Values)
            {
                Console.WriteLine(l);
            }


            Console.ReadLine();
        }

        
    }
}
