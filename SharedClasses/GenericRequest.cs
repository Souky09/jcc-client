using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    public class GenericRequest : IGenericRequest
    {
        public Info Info { get; set; }
        public string OperationType { get; set; }
        public PayLoad PayLoad { get; set; }

    }
}
