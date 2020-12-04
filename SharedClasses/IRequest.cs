using System;
using System.Collections.Generic;
using System.Text;

namespace SharedClasses
{
    public interface IRequest
    {
        void SettLength(List<Field> formatting);
        string GetContent(Formatting formatting);
        string Serialize(List<Field> fields);

    }
}
