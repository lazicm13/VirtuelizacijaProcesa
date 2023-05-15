using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]

    public enum MessageType {[EnumMember] Info,[EnumMember] Warning,[EnumMember] Error};
    public class Audit
    {
        #region Fields
        private string Id;
        private string Timestamp;
        private MessageType messageType;
        private string Message;
        #endregion

        #region Properties
        [DataMember]
        public string ID { get => Id; set => Id = value; }
        [DataMember]
        public string TIMESTAMP { get => Timestamp; set => Timestamp = value; }
        [DataMember]
        public MessageType MESSAGE_TYPE { get => messageType; set => messageType = value; }
        [DataMember]
        public string MESSAGE { get => Message; set => Message = value; }
        #endregion

        #region Constructors
        public Audit(string id, string timestamp, MessageType m, string message)
        {
            this.ID = id;
            this.TIMESTAMP = timestamp;
            this.MESSAGE_TYPE = m;
            this.MESSAGE = message;
        }

        public Audit() : this("", "", 0, "") { }
        #endregion

        #region OverrideMethods
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return $"{ID} {TIMESTAMP} {MESSAGE_TYPE} {MESSAGE}";
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
