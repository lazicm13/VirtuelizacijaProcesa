using Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
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
        Dictionary<int, Load> LoadDataFromCsv(string forecastPath, string measuredPath);

        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        void LoadDatabaseEntry(List<Load> list);


        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        void ImportedFileDatabaseEntry(List<ImportedFile> list);


        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        void AuditDatabaseEntry(Audit a);


        [OperationContract]
        Dictionary<int, Load> LoadMeasuredDataFromCSV(string filePathMeasured);



        [OperationContract]
        Dictionary<int, Load> LoadForecastDataFromCSV(string filePathForecast);

        [OperationContract]
        [FaultContract(typeof(CalculationException))]
        void CalculateDeviation();


        [OperationContract]
        void SendMessage(byte[] message);

        [OperationContract]
        Dictionary<int, Load> ReadXmlFile(string filePath);
    }
}
