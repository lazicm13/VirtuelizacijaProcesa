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
        #region Fields
     
        private int Id = 0;
        private string Timestamp;
        private double ForecastValue;
        private double MeasuredValue;
        private double AbsolutePercentageDeviation;
        private double SquaredDeviation;
        private int ForecastFileId = 0;
        private int MeasuredFileId = 0;

     

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


    }
}
