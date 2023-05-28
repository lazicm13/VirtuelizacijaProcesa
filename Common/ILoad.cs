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
        Dictionary<int, Load> LoadDataFromCsv(string filePathMeasured, string filePathForecast);

        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        void LoadDatabaseEntry(Load load);
       
        
        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        void ImportedFileDatabaseEntry(ImportedFile dict);
       
        
        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        void AuditDatabaseEntry(Audit audit);

        
        [OperationContract]
        Dictionary<int, Load> LoadMeasuredDataFromCSV(string filePathMeasured);
       
        
        
        [OperationContract]
        Dictionary<int, Load> LoadForecastDataFromCSV(string filePathForecast);

        [OperationContract]
        [FaultContract(typeof(CalculationException))]
        void CalculateDeviation(Load load);

        [OperationContract]
        Dictionary<int, Load> ReadXML(string filepath);

    }
}
