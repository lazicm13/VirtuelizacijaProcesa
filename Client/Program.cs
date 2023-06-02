using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using Common;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Dopunite naziv datoteke.\nNaziv se sastoji od tipa datoteke i datuma, npr 'forecast_2023_01_27.csv'");
            string forecastPath;
            string measuredPath;

            using (var client = new ChannelFactory<ILoad>("ServiceLoad"))
            {
                ILoad proxy = client.CreateChannel();

                while (true)
                {
                    Console.Write("Unos forecast datoteke: ");
                    forecastPath = "csv/forecast/" + Console.ReadLine().Trim();

                    Console.Write("Unos measured datoteke: ");
                    measuredPath = "csv/measured/" + Console.ReadLine().Trim();

                    // Read forecast file into a byte array
                    byte[] forecastBytes = File.ReadAllBytes(forecastPath);

                    // Read measured file into a byte array
                    byte[] measuredBytes = File.ReadAllBytes(measuredPath);

                    using (MemoryStream stream = new MemoryStream())
                    {
                        // Write the measured file bytes to the stream
                        stream.Write(forecastBytes, 0, forecastBytes.Length);

                        stream.Write(measuredBytes, 0, measuredBytes.Length);
                        
                        // Set the stream position back to the beginning
                        stream.Position = 0;

                        if (proxy.SendFiles(stream))
                        {
                            Console.WriteLine("Datoteke su uspešno obradjene!");
                        }
                        else
                        {
                            Console.WriteLine("Datoteke nisu uspešno obradjene!");
                        }
                    }
                }
            }
        }
    }
}
