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
        Dictionary<string, Load> LoadMeasuredDataFromCSV(string filePathMeasured);

        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        Dictionary<string, Load> LoadForecastDataFromCSV(string filePathForecast);

        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        Dictionary<string, Load> LoadData(string filePathMeasured, string filePathForecast);
    }
}
