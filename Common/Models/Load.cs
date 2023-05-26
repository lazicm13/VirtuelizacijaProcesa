using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using Common.Exceptions;
using System.ServiceModel;

namespace Common
{
    [DataContract]

    public class Load
    {
        #region 
        /// brojaci za dodelu ID
        public static int counter = 0;
        public static int forecastCounterID = 0;
        public static int measuredCounterID = 0;
        public static int errorIDcounter = 0;
        

        private int Id = 0;
        private string Timestamp;
        private double ForecastValue;
        private double MeasuredValue;
        private double AbsolutePercentageDeviation;
        private double SquaredDeviation;
        private int ForecastFileId = 0;
        private int MeasuredFileId = 0;

        [DataMember]
        public int errorID { get; set; }
        [DataMember]
        public int importedID { get; set; }

        
        

        // Liste za dodelu ID
        public static List<string> loadedMeasuredCSVFiles = new List<string>();
        public static List<string> loadedForecastCSVFiles = new List<string>();
        public static Dictionary<int, ImportedFile> importedList = new Dictionary<int, ImportedFile>();

        #endregion

        #region Properties
        [DataMember]
        public int ID { get => Id; set => Id = value; }
        [DataMember]
        public string TIMESTAMP { get => Timestamp; set => Timestamp = value; }
        [DataMember]
        public double FORECAST_VALUE { get => ForecastValue; set => ForecastValue = value; }
        [DataMember]
        public double MEASURED_VALUE { get => MeasuredValue; set => MeasuredValue = value; }
        [DataMember]
        public double ABSOLUTE_PERCENTAGE_DEVIATION { get => AbsolutePercentageDeviation; set => AbsolutePercentageDeviation = value; }
        [DataMember]
        public double SQUARED_DEVIATION { get => SquaredDeviation; set => SquaredDeviation = value; }
        [DataMember]
        public int FORECAST_FILE_ID { get => ForecastFileId; set => ForecastFileId = value; }
        [DataMember]
        public int MEASURED_FILE_ID { get => MeasuredFileId; set => MeasuredFileId = value; }


        #endregion

        #region Constructors

        public Load(int id, string timestamp, double forecastvalue, double measuredvalue, double percentage, double square, int forecastID, int measuredID)
        {
            this.ID = id;
            this.TIMESTAMP = timestamp;
            this.FORECAST_VALUE = forecastvalue;
            this.MEASURED_VALUE = measuredvalue;
            this.ABSOLUTE_PERCENTAGE_DEVIATION = percentage;
            this.SQUARED_DEVIATION = square;
            this.FORECAST_FILE_ID = forecastID;
            this.MEASURED_FILE_ID = measuredID;
        }
        public Load() : this(0, "", 0, 0, 0, 0, 0, 0) { }
        #endregion

        #region OverrideMethods
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return $"{ID} {TIMESTAMP} {FORECAST_VALUE} {MEASURED_VALUE} {ABSOLUTE_PERCENTAGE_DEVIATION} {SQUARED_DEVIATION} {FORECAST_FILE_ID} {MEASURED_FILE_ID}";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region Methods

        public Dictionary<string, Load> LoadMeasuredDataFromCSV(string filePathMeasured)
        {
            Dictionary<string, Load> measuredValue = new Dictionary<string, Load>();
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
                        measuredValue.Add(obj.TIMESTAMP, obj);
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
                Audit auditFile = new Audit(errorID, TIMESTAMP, MessageType.Error, ex.Razlog);
                // upisati audit u bazu
                throw new FaultException<InvalidFileException>(ex); // premesteno dole jer se nista iza ovog nece izvrsiti

            }

            return measuredValue;
        }
        public Dictionary<string, Load> LoadForecastDataFromCSV(string filePathForecast)
        {
            Dictionary<string, Load> forecastValue = new Dictionary<string, Load>();
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
                            forecastValue.Add(obj.TIMESTAMP, obj);
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
                Audit auditFile = new Audit(errorID, TIMESTAMP, MessageType.Error, ex.Razlog);
                // upisati audit u bazu
                throw new FaultException<InvalidFileException>(ex);
            }
            return forecastValue;
        }

       

    

    #endregion

    }
}
