using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {

            ChannelFactory<ILoad> factory = new ChannelFactory<ILoad>("ServiceLoad");

            ILoad proxy = factory.CreateChannel();

            Console.WriteLine("Unesite naziv datoteke iz koje zelite da iščitate podatke.\nNaziv se sastoji od tipa datoteke i datuma, npr \"prog_2023_01_27.csv\"");
            Console.WriteLine("Tip datoteke može biti\n\tprog - za prognoziranu potrošnju i \n\tostv - za ostvarenu potrošnju");
            Console.Write("Unesite naziv datoteke: ");
            string datoteka;
            datoteka = Console.ReadLine().Trim();

            Console.WriteLine("Pritisnite [Enter] za zaustavljanje klijenta!");
            Console.ReadLine();
        }
    }
}
