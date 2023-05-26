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
        void LoadDatabaseEntry(List<Load> dict);
       
        
        [OperationContract]
        void ImportedFileDatabaseEntry(List<ImportedFile> dict);
       
        
        [OperationContract]
        void AuditDatabaseEntry(Audit audit);

        
        [OperationContract]
        [FaultContract(typeof(CalculationException))]

        Dictionary<int, Load> LoadMeasuredDataFromCSV(string filePathMeasured);
       
        
        
        [OperationContract]
        Dictionary<int, Load> LoadForecastDataFromCSV(string filePathForecast);

        [OperationContract]
        void CalculateDeviation();

    }
}
