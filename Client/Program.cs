using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common;
using System.IO;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Dopunite naziv datoteke.\nNaziv se sastoji od tipa datoteke i datuma, npr \"forecast_2023_01_27.csv\"");
            string forecastPath = "";
            string measuredPath = "";
            string path = "";

            using (var client = new ChannelFactory<ILoad>("ServiceLoad"))
            {
                ILoad proxy = client.CreateChannel();
               using(MemoryStream stream = new MemoryStream())
                {

                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        
                        while (true)
                        {
                            Console.Write("Unos forecast datoteke: ");
                            forecastPath = Console.ReadLine().Trim();


                            Console.Write("Unos measured datoteke: ");
                            measuredPath = Console.ReadLine().Trim();

                            path = forecastPath + "#" + measuredPath;
                            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(path);
                            proxy.SendMessage(messageBytes);
                        }
                    }
                }
            }
        }
    }
}
