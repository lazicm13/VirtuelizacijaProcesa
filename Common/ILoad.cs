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
        Dictionary<int, Load> LoadDataFromCsv(string recievedMessage);

        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        void LoadDatabaseEntry(List<Load> list);

        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        void ImportedFileDatabaseEntry(List<ImportedFile> list);

        [OperationContract]
        [FaultContract(typeof(InvalidFileException))]
        void AuditDatabaseEntry(List<Audit> audits);

        [OperationContract]
        [FaultContract(typeof(CalculationException))]
        void CalculateDeviation();

        [OperationContract]
        bool SendFiles(Stream stream);

        [OperationContract]
        Dictionary<int, Load> ReadXmlFile(string filePath);

        [OperationContract]
        Dictionary<int, ImportedFile> ReadImportFile(string filePath);

        [OperationContract]
        Dictionary<int, Audit> ReadAuditFile(string filePath);
    }
}
