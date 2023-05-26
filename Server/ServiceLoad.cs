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
using System.Xml;
using Database;
using System.Configuration;


namespace Server
{
    public class ServiceLoad : ILoad
    {
        /// brojaci za dodelu ID
        public static int counter = 0;
        public static int forecastCounterID = 0;
        public static int measuredCounterID = 0;
        public static int errorIDcounter = 0;
        private string tempTimestamp;

        public static int importedIDcounter = 0;
        public static List<string> loadedTimestamp = new List<string>(); // pamtimo timestamp za koji smo dodali Load objekat
        public int errorID { get; set; }
        public int importedID { get; set; }


        // Liste za dodelu ID
        public static List<string> loadedMeasuredCSVFiles = new List<string>();
        public static List<string> loadedForecastCSVFiles = new List<string>();
        public static Dictionary<int, ImportedFile> importedList = new Dictionary<int, ImportedFile>();

        public Dictionary<int, Load> LoadMeasuredDataFromCSV(string filePathMeasured)
        {
            Dictionary<int, Load> measuredValue = new Dictionary<int, Load>();
            int rowCounter = 0; // brojac koji proverava da li je broj dana 23 - 25

            // dodajemo u listu svaki otvoreni CSV fajl i povecavamo brojac na osnovu kog mu dodeljujemo ID
            if (!loadedMeasuredCSVFiles.Contains(filePathMeasured))
            {
                loadedMeasuredCSVFiles.Add(filePathMeasured);
                measuredCounterID++;
            }


            using (var reader = new StreamReader(filePathMeasured))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    //Provera da li u prvom redu imamo "header"
                    if (line.Contains("TIME_STAMP"))
                    {
                        continue;
                    }
                    else
                    {
                        Load obj = new Load
                        {
                            TIMESTAMP = values[0],
                            MEASURED_VALUE = Convert.ToDouble(values[1])
                        };
                        tempTimestamp = obj.TIMESTAMP;
                        counter++;
                        obj.ID = counter;

                        measuredValue.Add(obj.ID, obj);
                        obj.MEASURED_FILE_ID = measuredCounterID;
                    }
                    rowCounter++;
                }
            }
            /// Odradjeno je otp proveriti lepo
            if (rowCounter < 23 || rowCounter > 25)
            {
                Console.WriteLine("NE VALJA");
                InvalidFileException ex = new InvalidFileException()
                {
                    Razlog = "Datoteka nije validna"
                };

                errorIDcounter++;
                errorID = errorIDcounter;
                Audit auditFile = new Audit(errorID, tempTimestamp, MessageType.Error, ex.Razlog);
                AuditDatabaseEntry(auditFile);
                throw new FaultException<InvalidFileException>(ex); // premesteno dole jer se nista iza ovog nece izvrsiti

            }

            return measuredValue;
        }
        public Dictionary<int, Load> LoadForecastDataFromCSV(string filePathForecast)
        {
            Dictionary<int, Load> forecastValue = new Dictionary<int, Load>();
            int rowCounter = 0;// brojac koji proverava da li je broj dana 23 - 25

            // dodajemo u listu svaki otvoreni CSV fajl i povecavamo brojac na osnovu kog mu dodeljujemo ID
            if (!loadedForecastCSVFiles.Contains(filePathForecast))
            {
                loadedForecastCSVFiles.Add(filePathForecast);
                forecastCounterID++;
            }

            using (var reader = new StreamReader(filePathForecast))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    //Provera da li u prvom redu imamo "header"
                    if (line.Contains("TIME_STAMP"))
                    {
                        continue;
                    }
                    else
                    {
                        Load obj = new Load
                        {
                            TIMESTAMP = values[0], // PREPRAVLJENO JER SU CSV FAJLOVI PREPRAVLJENI
                            FORECAST_VALUE = Convert.ToDouble(values[1])
                        };
                        tempTimestamp = obj.TIMESTAMP;
                        counter++;
                        obj.ID = counter;
                        forecastValue.Add(obj.ID, obj);
                        obj.FORECAST_FILE_ID = forecastCounterID;
                    }
                    rowCounter++;
                }
            } // Nakon ovog bloka, metoda Dispose() će biti automatski pozvana na objektu reader

            /// Odradjeno je otp proveriti lepo
            if (rowCounter < 23 || rowCounter > 25)
            {
                Console.WriteLine("Greska! U fajlu ne sme biti vise od ");
                InvalidFileException ex = new InvalidFileException()
                {
                    Razlog = "Datoteka nije validna"
                };
                errorIDcounter++;
                errorID = errorIDcounter;
                Audit auditFile = new Audit(errorID, tempTimestamp, MessageType.Error, ex.Razlog);
                AuditDatabaseEntry(auditFile);
                throw new FaultException<InvalidFileException>(ex);
            }
            return forecastValue;
        }

        public Dictionary<int, Load> LoadDataFromCsv(string filePathMeasured, string filePathForecast)
        {    
            Dictionary<int, Load> loadData = LoadMeasuredDataFromCSV(filePathMeasured); // uzimamo measured value, timestamp i FORECAST_ID. Fali nam forecast value i ID
            Dictionary<int, Load> loadForecast = LoadForecastDataFromCSV(filePathForecast); // koji dodajemo odavde

            foreach (Load l in loadData.Values)
            {
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
                importedID = importedIDcounter;
                importedFile.ID = importedID;
                importedFile.FILENAME = path;
                InMemoryDatabase.importedFiles.Add(importedID, importedFile); // dodajemo u InMemory bazu podataka
            }
        }


        public void CalculateDeviation() // Funkcija za racunanje odstupanja
        {
            string odstupanjeMetoda = ConfigurationManager.AppSettings["OdstupanjeMetoda"];

            foreach (Load l in InMemoryDatabase.db.Values)
            {
                if (l.FORECAST_VALUE > 0 && l.MEASURED_VALUE > 0)
                {
                    if (odstupanjeMetoda == "ApOdstupanje")
                    {
                            l.ABSOLUTE_PERCENTAGE_DEVIATION = Math.Round(((Math.Abs(l.MEASURED_VALUE - l.FORECAST_VALUE)) / l.MEASURED_VALUE) * 100, 4);
                    }
                    else if (odstupanjeMetoda == "KvOdstupanje")
                    {
                            l.SQUARED_DEVIATION = Math.Round(Math.Pow(((l.MEASURED_VALUE - l.FORECAST_VALUE) / l.MEASURED_VALUE), 2), 4);
                    }
                    else
                    {
                        CalculationException ce = new CalculationException()
                        {
                            Razlog = "Niste odabrali nacin racunanja"
                        };
                        throw new FaultException<CalculationException>(ce, ce.Razlog);
                    }
                }
            }
        }

        public void AuditDatabaseEntry(Audit audit)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Audit));

            using (StreamWriter writer = new StreamWriter("AuditDB.xml"))
            {
                serializer.Serialize(writer, audit);
            }
        }

       public void ImportedFileDatabaseEntry(List<ImportedFile> dict)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportedFile>));

            using (StreamWriter writer = new StreamWriter("ImportedFileDB.xml"))
            {
                serializer.Serialize(writer, dict);
            }
        }
        public void LoadDatabaseEntry(List<Load> dict)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Load>));

            using (StreamWriter writer = new StreamWriter("LoadDB.xml"))
            {
                serializer.Serialize(writer, dict);
            }
    }

}
}
