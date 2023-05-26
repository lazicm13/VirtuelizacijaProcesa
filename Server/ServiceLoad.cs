using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.IO;
using Common.Exceptions;
using System.ServiceModel;
using System.Xml.Serialization;
using Database;


namespace Server
{
    public class ServiceLoad : ILoad
    {
        public static int counter = 0;
        public static int importedIDcounter = 0;
        public static List<string> loadedTimestamp = new List<string>(); // pamtimo timestamp za koji smo dodali Load objekat
        

        public Dictionary<string, Load> LoadDataFromCsv(string filePathMeasured, string filePathForecast)
        {
            Load load = new Load();
        
            Dictionary<string, Load> loadData = load.LoadMeasuredDataFromCSV(filePathMeasured); // uzimamo measured value, timestamp i FORECAST_ID. Fali nam forecast value i ID
            Dictionary<string, Load> loadForecast = load.LoadForecastDataFromCSV(filePathForecast); // koji dodajemo odavde

            foreach (Load l in loadData.Values)
            {
                // brojac na osnovu kog povecavamo ID po satu u Load objektu u jednoj datoteci
                counter++;
                l.ID = counter;
                foreach (Load l1 in loadForecast.Values)
                {
                    if (l1.TIMESTAMP.Trim().Equals(l.TIMESTAMP.Trim()))
                    {
                        l.FORECAST_VALUE = l1.FORECAST_VALUE;
                        l.FORECAST_FILE_ID = l1.FORECAST_FILE_ID;
                    }
                }

                /// Kreiranje LOAD objekata po vremenu, sat po sat 
                if (!loadedTimestamp.Contains(l.TIMESTAMP))   /// Ako u recniku nemamo objekat napravi ga, u suprotnom ga azuriraj                                                       
                {
                    InMemoryDatabase.db.Add(l.ID, l);
                    loadedTimestamp.Add(l.TIMESTAMP);
                }
                else
                {
                    InMemoryDatabase.db[l.ID] = l;
                }
                /// Nakon kreiranja/azuriranja dodati objekat u bazu / taj deo fali

            }
            ImportFile(filePathForecast); // nisam siguran oko prosledjivanja ovog parametra load
            ImportFile(filePathMeasured);
            return loadData;
        }

        
        public void ImportFile(string path)
        {
            Load load = new Load();
            ImportedFile importedFile = new ImportedFile();
            bool valid = false;
            
            foreach (ImportedFile i in InMemoryDatabase.importedFiles.Values)
            {
                if (i.FILENAME == path)
                {
                    importedFile.ID = i.ID;
                    importedFile.FILENAME = i.FILENAME;
                    valid = true;
                    break;
                }
                valid = false;
            }
            if (!valid)
            {
                importedIDcounter++;
                load.importedID = importedIDcounter;
                importedFile.ID = load.importedID;
                importedFile.FILENAME = path;
                InMemoryDatabase.importedFiles.Add(load.importedID, importedFile); // dodajemo u InMemory bazu podataka
            }

        }
        public void DatabaseEntry()
        {
        /*
            XmlSerializer serializer = new XmlSerializer((typeof(Dictionary<int, Load>)));

            // Open a FileStream to the XML file
            using (FileStream fileStream = new FileStream("Baza.xml", FileMode.Create))
            {
                // Use the serializer to write the dictionary to the file
                serializer.Serialize(fileStream, InMemoryDatabase.db);
            }
        */
        }

        public void CalculateDeviation() // Funkcija za racunanje odstupanja
        {
            foreach(Load l in InMemoryDatabase.db.Values)
            {
                l.ABSOLUTE_PERCENTAGE_DEVIATION = Math.Round(((Math.Abs(l.MEASURED_VALUE - l.FORECAST_VALUE)) / l.MEASURED_VALUE)*100, 4);
                l.SQUARED_DEVIATION = Math.Round(Math.Pow(((l.MEASURED_VALUE - l.FORECAST_VALUE) / l.MEASURED_VALUE), 2), 4);
            }
        }

    }
}
