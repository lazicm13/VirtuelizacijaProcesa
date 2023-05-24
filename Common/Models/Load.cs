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
        private int Id;
        private string Timestamp;
        private double ForecastValue;
        private double MeasuredValue;
        private double AbsolutePercentageDeviation;
        private double SquaredDeviation;
        private string ForecastFileId;
        private string MeasuredFileId;
        #endregion

        #region Properties
        [DataMember]
        public int ID { get=>Id; set=> Id = value; }
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
        public static Dictionary<int, Load> LoadMeasuredDataFromCSV(string filePathMeasured)
        {
            Dictionary<int, Load> measuredValue = new Dictionary<int, Load>();

            using (var reader = new StreamReader(filePathMeasured))
            {
                reader.ReadLine();
                

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                   
                    
                    Load obj = new Load
                    { 
                        TIMESTAMP = values[0],
                        MEASURED_VALUE = Convert.ToDouble(values[1])
                    };

                    measuredValue.Add(obj.ID, obj);
                }
            } 

            return measuredValue;
        }
        public static Dictionary<int, Load> LoadForecastDataFromCSV(string filePathForecast)
        {
            Dictionary<int, Load> forecastValue = new Dictionary<int, Load>();

            using (var reader = new StreamReader(filePathForecast))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    Load obj = new Load
                    {
                        TIMESTAMP = values[0], // jer ima zarez izmedju
                        FORECAST_VALUE = Convert.ToDouble(values[2])
                    };

                    
                    forecastValue.Add(obj.ID, obj);
                    
                }
            } // Nakon ovog bloka, metoda Dispose() će biti automatski pozvana na objektu reader

            return forecastValue;
        }

        public static Dictionary<int, Load> LoadData(string filePathMeasured, string filePathForecast)
        {
            Dictionary<int, Load> loadData = new Dictionary<int, Load>();
            Dictionary<int, Load> loadForecast = new Dictionary<int, Load>();

            loadData = LoadMeasuredDataFromCSV(filePathMeasured); //uzimamo measured value i timestamp. Fali nam forecast value
            loadForecast = LoadForecastDataFromCSV(filePathForecast);

            foreach(Load l in loadData.Values)
            { 
                foreach(Load l1 in loadForecast.Values)
                {
                    if(l1.Timestamp.Trim().Equals(l.Timestamp.Trim()))
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
