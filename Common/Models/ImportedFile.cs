using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ImportedFile
    {
        #region Fields
        private string Id;
        private string FileName;
        #endregion

        #region Properties
        [DataMember]
        public string ID { get => Id; set => Id = value; }
        [DataMember]
        public string FILENAME { get => FileName; set => FileName = value; }
        #endregion

        #region Constructors
        public ImportedFile(string id, string file)
        {
            this.ID = id;
            this.FILENAME = file;
        }

        public ImportedFile() : this("", "") { }

        #endregion

        #region OverrideMethods
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return $"{ID} {FILENAME}";
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
