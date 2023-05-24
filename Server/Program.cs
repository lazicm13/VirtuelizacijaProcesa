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
            Dictionary<string, Load> loadMeasured = new Dictionary<string, Load>();
            loadMeasured = Load.LoadData("csv/measured/measured_2023_01_28.csv", "csv/forecast/forecast_2023_01_28.csv");

            foreach(Load l in loadMeasured.Values)
            {
                l.ID++;     /// ovo moramo da resimo da se pri dodavanju poveca id...
                Console.WriteLine(l);
            }

            Console.ReadLine();
        }

        
    }
}
