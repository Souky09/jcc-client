using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    public class GenericResponse
    {
        public PayLoad Payload { get; set; }
        public Info Info { get; set; }
        public Result  Result { get; set; }

    }
}
