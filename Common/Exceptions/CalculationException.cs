using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    [DataContract]
   public class CalculationException
    {
        private string razlog;
        [DataMember]
        public string Razlog { get => razlog; set => razlog = value; }

        public CalculationException(string razlog)
        {
            Razlog = razlog;
        }

        public CalculationException()
        {
        }
    }
}
