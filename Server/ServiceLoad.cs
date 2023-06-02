using Common;
using Common.Exceptions;
using Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace Server
{

    public class ServiceLoad : ILoad
    {
        #region Pomocne promenljive
        public static int counter = 0;
        public static int counter2 = 0;
        public static int forecastCounterID = 0;
        public static int measuredCounterID = 0;
        public static int errorIDcounter = 0;
        private string tempTimestamp;
        public static bool xmlEmpty = false;

        public static int importedIDcounter = 0;
        public int errorID { get; set; }
        public int importedID { get; set; }
        public string receivedMessage;

        public static Dictionary<int, Load> loadDict = new Dictionary<int, Load>();
        public static List<string> loadedTimestamp = new List<string>();
        public static List<string> loadedMeasuredCSVFiles = new List<string>();
        public static List<string> loadedForecastCSVFiles = new List<string>();
        #endregion
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
       
        public bool IsXmlDatabase()
        {
            SelectDatabase = DatabaseSelectionLogic;

            bool useXML = SelectDatabase();

            return useXML;
        }
        
        public Dictionary<int, Load> LoadForecastDataFromCSV(string file)
        {
            Dictionary<int, Load> forecastValue = new Dictionary<int, Load>();

            var lines = file.Split('\n');
            string fileName = "forecast_";
            var valuesArray = lines[2].Split(',');
            fileName += valuesArray[0].Substring(0, 10); 

            if (!loadedForecastCSVFiles.Contains(fileName))
            {
                loadedForecastCSVFiles.Add(fileName);
                forecastCounterID++;
            }

            for (int i=0;i<lines.Length;i++)
            {

                var values = lines[i].Split(',');
                
                if (values.Length >= 2)
                {
                    if (lines[i].Contains("TIME_STAMP"))
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
                            counter2++;
                            obj.ID = counter2;

                            obj.FORECAST_FILE_ID = forecastCounterID;
                            forecastValue.Add(obj.ID, obj);
                        }
                        else
                        {
                            Load obj = new Load
                            {
                                TIMESTAMP = values[0],
                                FORECAST_VALUE = Convert.ToDouble(values[1])
                            };
                            tempTimestamp = obj.TIMESTAMP;
                            counter2++;
                            obj.ID = counter2;

                            obj.FORECAST_FILE_ID = forecastCounterID;
                            forecastValue.Add(obj.ID, obj);
                        }
                    }
                }
            }
            if (forecastValue.Count < 23 || forecastValue.Count > 25)
            {
                InvalidFileException ex = new InvalidFileException()
                {
                    Razlog = "Datoteka nije validna"
                };

                errorIDcounter++;
                errorID = errorIDcounter;
                Audit auditFile = new Audit(errorID, tempTimestamp, MessageType.Error, ex.Razlog);
                ImportAudit(auditFile);
                throw new FaultException<InvalidFileException>(ex);
            }
            ImportFile(fileName);
            return forecastValue;
        }

        public Dictionary<int, Load> LoadMeasuredDataFromCSV(string file)
        {
            Dictionary<int, Load> measuredValue = new Dictionary<int, Load>();

            var lines = file.Split('\n');
            string fileName = "measured_";
            var valuesArray = lines[2].Split(',');
            fileName += valuesArray[0].Substring(0, 10); 

            if (!loadedMeasuredCSVFiles.Contains(fileName))
            {
                loadedMeasuredCSVFiles.Add(fileName);
                measuredCounterID++;
            }
            
            for (int i = 0; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');

                if (values.Length >= 2)
                {
                    if (lines[i].Contains("TIME_STAMP"))
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

                            obj.MEASURED_FILE_ID = measuredCounterID;
                            measuredValue.Add(obj.ID, obj);
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

                            obj.MEASURED_FILE_ID = measuredCounterID;
                            measuredValue.Add(obj.ID, obj);
                        }
                    }
                }
            }
            if (measuredValue.Count < 23 || measuredValue.Count > 25)
            {
                InvalidFileException ex = new InvalidFileException()
                {
                    Razlog = "Datoteka nije validna"
                };

                errorIDcounter++;
                errorID = errorIDcounter;
                Audit auditFile = new Audit(errorID, tempTimestamp, MessageType.Error, ex.Razlog);
                ImportAudit(auditFile);
                throw new FaultException<InvalidFileException>(ex);
            }
            ImportFile(fileName);
            return measuredValue;
        }

        public Dictionary<int, Load> LoadDataFromCsv(string recievedMessage)
        {
            if (File.Exists("LoadDB.xml"))
            {
                loadDict = ReadXmlFile("LoadDB.xml");
                foreach (Load load in loadDict.Values)
                {
                    loadedTimestamp.Add(load.TIMESTAMP);
                }
                measuredCounterID = loadDict[loadDict.Count].MEASURED_FILE_ID;
                forecastCounterID = loadDict[loadDict.Count].FORECAST_FILE_ID;
                importedID = forecastCounterID;
            }
            Dictionary<int, Load> tempTimestamp = new Dictionary<int, Load>();
            var files = recievedMessage.Split('\uFEFF');
            int startId = loadDict.Count + 1;
            Dictionary<int, Load> loadForecast = LoadForecastDataFromCSV(files[0]);
            Dictionary<int, Load> tempData = LoadMeasuredDataFromCSV(files[1]);

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
                if (!loadedTimestamp.Contains(l.TIMESTAMP))
                {
                    loadDict.Add(startId, l);
                    loadedTimestamp.Add(l.TIMESTAMP);
                    startId++;
                }
                else
                {
                    tempTimestamp.Add(l.ID, l);
                }
            }
            int id = 1;

            foreach (Load load in loadDict.Values)
            {
                load.ID = id;
                id++;
            }
            foreach (Load load in loadDict.Values)
            {
                foreach(Load l in tempTimestamp.Values)
                {
                    if(load.TIMESTAMP.Trim().Equals(l.TIMESTAMP))
                    {
                        load.MEASURED_VALUE = l.MEASURED_VALUE;
                        load.FORECAST_VALUE = l.FORECAST_VALUE;
                        load.SQUARED_DEVIATION = l.SQUARED_DEVIATION;
                        load.ABSOLUTE_PERCENTAGE_DEVIATION = l.ABSOLUTE_PERCENTAGE_DEVIATION;
                    }
                }
            }
            return loadDict; 
        }

        public void ImportFile(string fileName)
        {
            Dictionary<int, ImportedFile> impList = new Dictionary<int, ImportedFile>();
            if (File.Exists("ImportedFileDB.xml"))
            {
                impList = ReadImportFile("ImportedFileDB.xml");
            }
            int startID = impList.Count + 1;
            ImportedFile impFile = new ImportedFile(startID,  fileName);
            impList.Add(startID, impFile);
            ImportedFileDatabaseEntry(impList.Values.ToList());
        }

        public void ImportAudit(Audit a)
        {
            Dictionary<int, Audit> loadAudit = new Dictionary<int, Audit>();
            if (File.Exists("AuditDB.xml"))
            {
                loadAudit = ReadAuditFile("AuditDB.xml");
            }
            int startID = loadAudit.Count + 1;
            a.ID = startID;
            loadAudit.Add(startID, a);
            AuditDatabaseEntry(loadAudit.Values.ToList());
        }
      
        public void CalculateDeviation() 
        {
            string odstupanjeMetoda = ConfigurationManager.AppSettings["OdstupanjeMetoda"];

            foreach (Load l in loadDict.Values)
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
      
        public void AuditDatabaseEntry(List<Audit> audits)
        {
            if (IsXmlDatabase())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Audit>));

                using (StreamWriter writer = new StreamWriter("AuditDB.xml"))
                {
                    serializer.Serialize(writer, audits);
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

        public bool SendFiles(Stream stream)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);

                    memoryStream.Position = 0;
                    string recievedMessage;

                    using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8))
                    {
                        recievedMessage = reader.ReadToEnd();
                    }

                    loadDict = LoadDataFromCsv(recievedMessage);
                    CalculateDeviation();
                    LoadDatabaseEntry(loadDict.Values.ToList());
                    return true;
                }
            }
            catch (FaultException<CalculationException> e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            catch (FaultException<InvalidFileException> e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine($"Stack Trace:\n{e.StackTrace}");
                return false;
            }
        }

        public Dictionary<int, Load> ReadXmlFile(string filePath)
        {
            Dictionary<int, Load> loads = new Dictionary<int, Load>();
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Load")
                    {
                        int id = 0;
                        string timestamp = string.Empty;
                        double forecastValue = 0.0;
                        double measuredValue = 0.0;
                        double absDeviation = 0.0;
                        double squaredDeviation = 0.0;
                        int forecastFileId = 0;
                        int measuerdFileId = 0;

                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                switch (reader.Name)
                                {
                                    case "ID":
                                        id = Convert.ToInt32(reader.ReadInnerXml());
                                        break;
                                    case "TIMESTAMP":
                                        timestamp = reader.ReadInnerXml();
                                        break;
                                    case "FORECAST_VALUE":
                                        forecastValue = Convert.ToDouble(reader.ReadInnerXml());
                                        break;
                                    case "MEASURED_VALUE":
                                        measuredValue = Convert.ToDouble(reader.ReadInnerXml());
                                        break;
                                    case "ABSOLUTE_PERCENTAGE_DEVIATION":
                                        absDeviation = Convert.ToDouble(reader.ReadInnerXml());
                                        break;
                                    case "SQUARED_DEVIATION":
                                        squaredDeviation = Convert.ToDouble(reader.ReadInnerXml());
                                        break;
                                    case "FORECAST_FILE_ID":
                                        forecastFileId = Convert.ToInt32(reader.ReadInnerXml());
                                        break;
                                    case "MEASURED_FILE_ID":
                                        measuerdFileId = Convert.ToInt32(reader.ReadInnerXml());
                                        break;
                                }
                            }
                            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Load")
                            {
                                Load load = new Load(id, timestamp, forecastValue, measuredValue, absDeviation, squaredDeviation, forecastFileId, measuerdFileId);
                                loads.Add(id, load);
                                break;
                            }
                        }
                    }
                }
            }
            return loads;
        }
    
        public Dictionary<int, ImportedFile> ReadImportFile(string filePath)
        {
            Dictionary<int, ImportedFile> imports = new Dictionary<int, ImportedFile>();
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "ImportedFile")
                    {
                        int importedId = 0;
                        string filename = string.Empty;

                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                switch (reader.Name)
                                {
                                    case "ID":
                                        importedId = Convert.ToInt32(reader.ReadInnerXml());
                                        break;
                                    case "FILENAME":
                                        filename = reader.ReadInnerXml();
                                        break;
                                }
                            }
                            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "ImportedFile")
                            {
                                ImportedFile imp = new ImportedFile(importedId, filename);
                                imports.Add(importedId, imp);
                                break;
                            }
                        }
                    }
                }
            }
            return imports;
        }

        public Dictionary<int, Audit> ReadAuditFile(string filePath)
        {
            Dictionary<int, Audit> audits = new Dictionary<int, Audit>();
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Audit")
                    {
                        int auditID = 0;
                        string timestamp = string.Empty;
                        string messageType = string.Empty;
                        string message = string.Empty;

                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                switch (reader.Name)
                                {
                                    case "ID":
                                        auditID = Convert.ToInt32(reader.ReadInnerXml());
                                        break;
                                    case "FILENAME":
                                        timestamp = reader.ReadInnerXml();
                                        break;
                                    case "MESSAGE_TYPE":
                                        messageType = reader.ReadInnerXml();
                                        break;
                                    case "MESSAGE":
                                        message = reader.ReadInnerXml();
                                        break;
                                }
                            }
                            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Audit")
                            {
                                Audit auditFile = new Audit(auditID, timestamp, MessageType.Error, message);
                                audits.Add(auditID, auditFile);
                                break;

                            }
                        }
                    }
                }
            }
            return audits;
        }
    }
}

