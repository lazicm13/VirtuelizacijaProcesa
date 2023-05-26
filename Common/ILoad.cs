using Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface ILoad
    {
        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        Dictionary<string, Load> LoadDataFromCsv(string filePathMeasured, string filePathForecast);

        [OperationContract]
        void DatabaseEntry();

        [OperationContract]
        void CalculateDeviation();

    }
}
