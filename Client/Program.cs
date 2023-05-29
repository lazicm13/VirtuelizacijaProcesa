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

            Console.WriteLine("Unesite naziv datoteke iz koje zelite da iščitate podatke.\nNaziv se sastoji od tipa datoteke i datuma, npr \"forecast_2023_01_27.csv\"");
            Console.WriteLine("Tip datoteke može biti\n\tforecast - za prognoziranu potrošnju i \n\tmeasured - za ostvarenu potrošnju");
            Console.WriteLine("Unesite \"kraj\" za gasenje klijenta");
            string datoteka = "";
           
                using (var client = new ChannelFactory<ICommunication>("Communication"))
                {
                    ICommunication proxy = client.CreateChannel();
                    MemoryStream stream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(stream);

                     while(datoteka.ToLower() != "kraj")
                     { 
                         Console.Write("Unesite naziv datoteke: ");
                         datoteka = Console.ReadLine().Trim();

                         writer.Write(datoteka);

                         writer.Flush();
                         stream.Position = 0;

                         MemoryStream responseStream = proxy.SendMessage(stream);
                         StreamReader reader = new StreamReader(responseStream);
                         string response = reader.ReadToEnd();

                         Console.WriteLine("Odgovor od servera: " + response);
                     }


                }
        }
    }
}
