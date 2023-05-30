using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common;
using System.IO;
using Database;
using System.Configuration;

namespace Server
{
    class Program
    {
        
        static void Main(string[] args)
        {

            ServiceLoad sl = new ServiceLoad();

            Dictionary<int, Load> dict = sl.ReadXmlFile("LoadDB.xml");
                Console.WriteLine("Ispis" + dict.Count);
            foreach(Load l in dict.Values)
            {
                Console.WriteLine(l);
            }

            using (ServiceHost host = new ServiceHost(typeof(ServiceLoad)))
            {
                host.Open();
                Console.WriteLine("Server je pokrenut...");
                Console.WriteLine("Pritisnite [Enter] za zaustavljanje servera!");
                Console.ReadKey();
                Console.WriteLine(sl.receivedMessage);


                host.Close();
            }

           







        }
    }
}
