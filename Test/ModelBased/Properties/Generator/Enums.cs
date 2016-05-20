using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{
    [DataType(typeof(String))]
    public enum MlsNumber { Null, Blank, NonBlank }
    [DataType(typeof(String))]
    public enum Status { Null, ACT, SLD }
    [DataType(typeof(String))]
    public enum PostalCode { Null, Blank, NonBlank }
    [DataType(typeof(String))]
    public enum Address { Null, Blank, NonBlank }
    [DataType(typeof(String))]
    public enum City { Null, Blank, NonBlank }
    [DataType(typeof(DateTime))]
    public enum CloseDate { Null, Blank, NonBlank }

    public class DataTypeAttribute : Attribute
    {
        public Type Type;
        public DataTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
