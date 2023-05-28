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
        public int errorID { get; set; }
        public int importedID { get; set; }


        // Liste za dodelu ID
        public static List<string> loadedTimestamp = new List<string>(); // pamtimo timestamp za koji smo dodali Load objekat
        public static List<string> loadedMeasuredCSVFiles = new List<string>();
        public static List<string> loadedForecastCSVFiles = new List<string>();
        public static Dictionary<int, ImportedFile> importedList = new Dictionary<int, ImportedFile>();

        public Dictionary<int, Load> LoadMeasuredDataFromCSV(string filePathMeasured)
        {
            Dictionary<int, Load> measuredValue = new Dictionary<int, Load>();
            int rowCounter = 0; /// brojac koji proverava da li je broj dana 23 - 25

            /// dodajemo u listu svaki otvoreni CSV fajl i povecavamo brojac na osnovu kog mu dodeljujemo ID
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

                    ///Provera da li u prvom redu imamo "header"
                    if (line.Contains("TIME_STAMP"))
                    {
                        continue;
                    }
                    else
                    {
                        if(values.Length > 2)
                        {
                            Load obj = new Load
                            {
                                TIMESTAMP = values[0] + " " + values[1],
                                MEASURED_VALUE = Convert.ToDouble(values[2])
                            };
                            tempTimestamp = obj.TIMESTAMP;
                            counter++;
                            obj.ID = counter;

                            measuredValue.Add(obj.ID, obj);
                            obj.MEASURED_FILE_ID = measuredCounterID;
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
                    }
                    rowCounter++;
                }
            }
            /// Odradjeno je otp proveriti lepo
            if (rowCounter < 23 || rowCounter > 25)
            {
                InvalidFileException ex = new InvalidFileException()
                {
                    Razlog = "Datoteka nije validna"
                };

                errorIDcounter++;
                errorID = errorIDcounter;
                Audit auditFile = new Audit(errorID, tempTimestamp, MessageType.Error, ex.Razlog);
                AuditDatabaseEntry(auditFile);
                // throw new FaultException<InvalidFileException>(ex); // premesteno dole jer se nista iza ovog nece izvrsiti

            }

            return measuredValue;
        }

        public Dictionary<int, Load> ReadXML(string filepath)
        {
            Dictionary<int, Load> loads = new Dictionary<int, Load>();
            XmlDocument xmlDoc = new XmlDocument();

            // Učitavanje XML datoteke
            xmlDoc.Load(filepath);

            // Čitanje vrednosti iz XML datoteke
            XmlNodeList loadNodes = xmlDoc.SelectNodes("/ArrayOfLoad/Load");
            foreach (XmlNode loadNode in loadNodes)
            {
                int id = Convert.ToInt32(loadNode.SelectSingleNode("ID").InnerText);
                string timestamp = loadNode.SelectSingleNode("TIMESTAMP").InnerText;
                double forecastValue = Convert.ToDouble(loadNode.SelectSingleNode("FORECAST_VALUE").InnerText);
                double measuredValue = Convert.ToDouble(loadNode.SelectSingleNode("MEASURED_VALUE").InnerText);
                double absDeviation = Convert.ToDouble(loadNode.SelectSingleNode("ABSOLUTE_PERCENTAGE_DEVIATION").InnerText);
                double squaredDeviation = Convert.ToDouble(loadNode.SelectSingleNode("SQUARED_DEVIATION").InnerText);
                int forecastFileId = Convert.ToInt32(loadNode.SelectSingleNode("FORECAST_FILE_ID").InnerText);
                int measuerdFileId = Convert.ToInt32(loadNode.SelectSingleNode("MEASURED_FILE_ID").InnerText);
                Load load = new Load(id, timestamp, forecastValue, measuredValue, absDeviation, squaredDeviation, forecastFileId, measuerdFileId);

                loads.Add(load.ID, load);
            }

            return loads;
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
                        if (values.Length > 2)
                        {
                            Load obj = new Load
                            {
                                TIMESTAMP = values[0] + " " + values[1], // PREPRAVLJENO JER SU CSV FAJLOVI PREPRAVLJENI
                                FORECAST_VALUE = Convert.ToDouble(values[2])
                            };
                            tempTimestamp = obj.TIMESTAMP;
                            counter++;
                            obj.ID = counter;
                            forecastValue.Add(obj.ID, obj);
                            obj.FORECAST_FILE_ID = forecastCounterID;
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
               // throw new FaultException<InvalidFileException>(ex);
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
                    LoadDatabaseEntry(l);
                    loadedTimestamp.Add(l.TIMESTAMP);
                }
                else
                {
                    LoadDatabaseEntry(l);
                }
                /// Nakon kreiranja/azuriranja dodati objekat u bazu / taj deo fali
                CalculateDeviation(l);
            }
            ImportFile(filePathMeasured);
            ImportFile(filePathForecast); // nisam siguran oko prosledjivanja ovog parametra load
            return loadData;
        } 
        
        public void ImportFile(string path)
        {
            ImportedFile importedFile = new ImportedFile();
            string fileName = "";
           
            var parts = path.Split('/');
            fileName = parts[2];
            importedIDcounter++;
            importedID = importedIDcounter;
            importedFile.ID = importedID;
            importedFile.FILENAME = fileName;
            ImportedFileDatabaseEntry(importedFile);
        }
        public void CalculateDeviation(Load l) // Funkcija za racunanje odstupanja
        {
            string odstupanjeMetoda = ConfigurationManager.AppSettings["OdstupanjeMetoda"];

            
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
            LoadDatabaseEntry(l);

           
        }
        public void AuditDatabaseEntry(Audit audit)
        {
            string VrstaUpisa = ConfigurationManager.AppSettings["VrstaUpisa"];

            if (VrstaUpisa == "XMLDatabase")
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Audit));

                using (StreamWriter writer = new StreamWriter("AuditDB.xml", true))
                {
                    serializer.Serialize(writer, audit);
                }
            }
            else if (VrstaUpisa == "InMemoryDatabase")
            {
                InMemoryDatabase.auditFiles[audit.ID] = audit;
            }
        }
        public void ImportedFileDatabaseEntry(ImportedFile impFile)
        {
           
            string VrstaUpisa = ConfigurationManager.AppSettings["VrstaUpisa"];

            if (VrstaUpisa == "XMLDatabase")
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ImportedFile));

                using (StreamWriter writer = new StreamWriter("ImportedFileDB.xml", true))
                {
                    serializer.Serialize(writer, impFile);
                }
            }
            else if (VrstaUpisa == "InMemoryDatabase")
            {
                InMemoryDatabase.importedFiles[impFile.ID] = impFile;
            }
        }
        public void LoadDatabaseEntry(Load load)
        {
            string VrstaUpisa = ConfigurationManager.AppSettings["VrstaUpisa"];

            if (VrstaUpisa == "XMLDatabase")
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Load));
                using (StreamWriter writer = new StreamWriter("LoadDB.xml", true))
                {
                    serializer.Serialize(writer, load);
                }
            }
            else if (VrstaUpisa == "InMemoryDatabase")
            {
                InMemoryDatabase.db[load.ID] = load;
            }
        }

    
    }
}
