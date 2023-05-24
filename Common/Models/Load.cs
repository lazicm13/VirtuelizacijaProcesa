using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;

namespace Common
{
    [DataContract]
    public class Load
    {
        #region Fields
        private static int Id = 0;
        private string Timestamp;
        private double ForecastValue;
        private double MeasuredValue;
        private double AbsolutePercentageDeviation;
        private double SquaredDeviation;
        private string ForecastFileId;
        private string MeasuredFileId;

        private static int rowsCounter = 0;

        public static Dictionary<string, Load> loadedObjects = new Dictionary<string, Load>(); // recnik koji pamti dodate Load objekte
        public static List<string> loadedTimestamp = new List<string>(); // pamtimo timestamp za koji smo dodali Load objekat
                                                                        // mogli smo sve da izvlacimo iz 1 ali nam je ovako laksa provera?

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
        public string FORECAST_FILE_ID { get => ForecastFileId; set => ForecastFileId = value; }
        [DataMember]
        public string MEASURED_FILE_ID { get => MeasuredFileId; set => MeasuredFileId = value; }


        #endregion

        #region Constructors

        public Load(int id, string timestamp, double forecastvalue, double measuredvalue, double percentage, double square, string forecastID, string measuredID)
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
        public Load() : this(0, "", 0, 0, 0, 0, "", "") { }
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
        public static Dictionary<string, Load> LoadMeasuredDataFromCSV(string filePathMeasured)
        {
            Dictionary<string, Load> measuredValue = new Dictionary<string, Load>();

            rowsCounter = 0;
            using (var reader = new StreamReader(filePathMeasured))
            {
            //    if (reader.ReadLine().Contains("TIME_STAMP")) /// u koliko prvi red sadrzi imena kolona preskoci ga
            //    { NE RAD NE ZNAM ZASTO
                    reader.ReadLine();
             //   }
                int rowNum = 0;

                while (!reader.EndOfStream)
                {
                    rowsCounter++;
                    
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    rowNum++;

                    Load obj = new Load
                    {
                        TIMESTAMP = values[0],
                        MEASURED_VALUE = Convert.ToDouble(values[1])
                    };
                                                                /// Poslednji pasus u tacki 1
                    if (!loadedTimestamp.Contains(values[0]))   /// Ako u recniku nemamo objekat napravi ga, u suprotnom ga azuriraj                                                    
                    {                                           /// Fali deo dodavanja u bazu podataka
                        loadedObjects.Add(obj.TIMESTAMP, obj);
                        loadedTimestamp.Add(obj.TIMESTAMP);
                    }
                    else
                    {
                        loadedObjects[values[0]] = obj;
                    }
                    measuredValue.Add(obj.TIMESTAMP, obj);

                }
                if (rowsCounter < 23 || rowsCounter > 25)
                {
                    /// Osnovna provera treba dodati jos za bazu i sta vec fali
                    Console.WriteLine("Ova datoteka nije validna RN" + rowsCounter);
                    measuredValue.Clear();
                }
            }

            return measuredValue;
        }
        public static Dictionary<string, Load> LoadForecastDataFromCSV(string filePathForecast)
        {
            Dictionary<string, Load> forecastValue = new Dictionary<string, Load>();

            using (var reader = new StreamReader(filePathForecast))
            {
            rowsCounter = 0;
              //  if (reader.ReadLine().Contains("TIME_STAMP")) /// u koliko prvi red sadrzi imena kolona preskoci ga
             //   {     NE RADI NE ZNAM ZASTO
                    reader.ReadLine();
             //   }

                while (reader.EndOfStream)
                {
                    rowsCounter++;
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    Load obj = new Load
                    {
                        TIMESTAMP = values[0], // PREPRAVLJENO JER SU CSV FAJLOVI PREPRAVLJENI
                        FORECAST_VALUE = Convert.ToDouble(values[1])
                    };
                    if (!loadedTimestamp.Contains(values[0]))   /// Ako u recniku nemamo objekat napravi ga, u suprotnom ga azuriraj                                                    
                    {                                           /// Fali deo dodavanja u bazu podataka
                        loadedObjects.Add(obj.TIMESTAMP, obj);
                        loadedTimestamp.Add(obj.TIMESTAMP);
                    }
                    else
                    {
                        loadedObjects[values[0]] = obj;
                    }
                    forecastValue.Add(obj.TIMESTAMP, obj); 

                }
                if (rowsCounter < 23 || rowsCounter > 25)
                {
                    /// Osnovna provera treba dodati jos za bazu i sta vec fali
                    Console.WriteLine("Ova datoteka nije validna RN "+ rowsCounter);
                    forecastValue.Clear();
                }
            } // Nakon ovog bloka, metoda Dispose() će biti automatski pozvana na objektu reader

            return forecastValue;
        }

        public static Dictionary<string, Load> LoadData(string filePathMeasured, string filePathForecast)
        {
            Dictionary<string, Load> loadData = new Dictionary<string, Load>();
            Dictionary<string, Load> loadForecast = new Dictionary<string, Load>();
           
            loadData = LoadMeasuredDataFromCSV(filePathMeasured); // uzimamo measured value i timestamp. Fali nam forecast value
            loadForecast = LoadForecastDataFromCSV(filePathForecast); // koji dodajemo odavde

            foreach (Load l in loadData.Values)
            {
                foreach (Load l1 in loadForecast.Values)
                {
                    if (l1.Timestamp.Trim().Equals(l.Timestamp.Trim()))
                    {
                        l.FORECAST_VALUE = l1.FORECAST_VALUE;
                    }
                }
            }
            return loadData;
        }




        #endregion


    }
}
