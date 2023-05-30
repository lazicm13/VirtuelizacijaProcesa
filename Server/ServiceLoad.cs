using Common;
using Common.Exceptions;
using Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;


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

        public delegate bool DatabaseSelection();
        public event DatabaseSelection SelectDatabase;

        public bool DatabaseSelectionLogic()
        {
            string VrstaUpisa = ConfigurationManager.AppSettings["VrstaUpisa"];
            if (VrstaUpisa == "XMLDatabase")
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public static int importedIDcounter = 0;
        public int errorID { get; set; }
        public int importedID { get; set; }
        public string receivedMessage;

        #region Pomocne liste
        public static Dictionary<int, Load> loadList = new Dictionary<int, Load>();
        public static List<ImportedFile> importedFileList = new List<ImportedFile>();
        public static List<string> loadedTimestamp = new List<string>();
        public static List<string> loadedMeasuredCSVFiles = new List<string>();
        public static List<string> loadedForecastCSVFiles = new List<string>();
        public static Dictionary<int, ImportedFile> importedList = new Dictionary<int, ImportedFile>();
        public static Dictionary<int, Load> data = new Dictionary<int, Load>();
        #endregion

        public bool IsXmlDatabase()
        {
            SelectDatabase = DatabaseSelectionLogic;

            bool useXML = SelectDatabase();

            return useXML;
        }

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
                        if (values.Length > 2)
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
                throw new FaultException<InvalidFileException>(ex);
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
                        if (values.Length > 2)
                        {
                            Load obj = new Load
                            {
                                TIMESTAMP = values[0] + " " + values[1],
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
                                TIMESTAMP = values[0],
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
                throw new FaultException<InvalidFileException>(ex);
            }
            return forecastValue;
        }

        public Dictionary<int, Load> LoadDataFromCsv(string forecastPath, string measuredPath)
        {
            data = ReadXmlFile("LoadDB.xml");
            Dictionary<int, Load> tempData = LoadMeasuredDataFromCSV(measuredPath);
            Dictionary<int, Load> loadForecast = LoadForecastDataFromCSV(forecastPath);
    
            foreach (Load l in tempData.Values)
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
                /// Ako u recniku nemamo objekat napravi ga, u suprotnom ga azuriraj
                if (!loadedTimestamp.Contains(l.TIMESTAMP))
                {
                    loadList.Add(l.ID, l);
                    loadedTimestamp.Add(l.TIMESTAMP);
                }
                else
                {
                    loadList[l.ID] = l;
                }
            }

            ImportFile(forecastPath);
            ImportFile(measuredPath);
            return tempData;
        }



        public void ImportFile(string path)
        {
            Load load = new Load();
            ImportedFile importedFile = new ImportedFile();
            bool valid = false;
            string fileName = "";

            foreach (ImportedFile i in importedFileList)
            {
                if (i.FILENAME == path)
                {
                    var parts = path.Split('/');
                    fileName = parts[2];
                    importedFile.ID = i.ID;
                    importedFile.FILENAME = fileName;
                    valid = true;
                    break;
                }
                valid = false;
            }
            if (!valid)
            {
                var parts = path.Split('/');
                fileName = parts[2];
                importedIDcounter++;
                importedID = importedIDcounter;
                importedFile.ID = importedID;
                importedFile.FILENAME = fileName;
                importedFileList.Add(importedFile);
            }
            ImportedFileDatabaseEntry(importedFileList);
        }
        public void CalculateDeviation() // Funkcija za racunanje odstupanja
        {
            string odstupanjeMetoda = ConfigurationManager.AppSettings["OdstupanjeMetoda"];

            foreach (Load l in data.Values)
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
            LoadDatabaseEntry(data.Values.ToList());
        }
        public void AuditDatabaseEntry(Audit audit)
        {

            if (IsXmlDatabase())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Audit));

                using (StreamWriter writer = new StreamWriter("AuditDB.xml", true))
                {
                    serializer.Serialize(writer, audit);
                }
            }
            else
            {
                foreach (Audit a in InMemoryDatabase.auditFiles.Values)
                {
                    InMemoryDatabase.auditFiles[a.ID] = a;
                }
            }
        }

        public void ImportedFileDatabaseEntry(List<ImportedFile> dict)
        {
            if (IsXmlDatabase())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ImportedFile>));

                using (StreamWriter writer = new StreamWriter("ImportedFileDB.xml"))
                {
                    serializer.Serialize(writer, dict);
                }
            }
            else
            {
                foreach (ImportedFile im in InMemoryDatabase.importedFiles.Values)
                {
                    InMemoryDatabase.importedFiles[im.ID] = im;
                }
            }
        }

        public void LoadDatabaseEntry(List<Load> dict)
        {
            if (IsXmlDatabase())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Load>));

                using (StreamWriter writer = new StreamWriter("LoadDB.xml"))
                {
                    serializer.Serialize(writer, dict);
                }
            }
            else
            {
                foreach (Load l in InMemoryDatabase.db.Values)
                {
                    InMemoryDatabase.db[l.ID] = l;
                }
            }
        }

        public void SendMessage(byte[] message)
        {

            string path1 = "csv/";
            string path2 = "csv/";
            using (MemoryStream memoryStream = new MemoryStream(message))
            {
                try
                {
                    receivedMessage = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());

                    var lines = receivedMessage.Split('#');
                    string forecastPath = lines[0];
                    string measuredPath = lines[1];
                    Console.WriteLine("Server received message: " + receivedMessage);

                    path1 += "forecast/" + forecastPath;

                    path2 += "measured/" + measuredPath;



                    data = LoadDataFromCsv(path1, path2);
                    CalculateDeviation();
                    LoadDatabaseEntry(data.Values.ToList());
                }
                catch (FaultException<CalculationException> e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (FaultException<InvalidFileException> e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }


            }
        }

        public Dictionary<int, Load> ReadXmlFile(string filePath)
        {
            Dictionary<int, Load> loads = new Dictionary<int, Load>();
            XmlDocument xmlDoc = new XmlDocument();

            // Učitavanje XML datoteke
            xmlDoc.Load(filePath);

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

    }
}
