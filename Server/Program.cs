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
            Dictionary<int, Load> loadMeasured = new Dictionary<int, Load>();
            loadMeasured = Load.LoadData("csv/measured/measured_2023_01_17.csv", "csv/forecast/forecast_2023_01_17.csv");
           
            for(int i=0;i<loadMeasured.Count;i++)
            {
                Console.WriteLine(loadMeasured[i].ToString());
            }

            Console.ReadLine();
        }

        
    }
}
