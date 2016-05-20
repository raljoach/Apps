using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;

namespace Test.ModelBased
{
    public class ParameterGroup//<T> //where T : Enum
    {
        public List<ParameterInfo> Parameters = new List<ParameterInfo>();
        public ParameterGroup(params Type[] enums)
        {
            if(enums == null || enums.Length==0)
            {
                throw new InvalidOperationException();
            }
            foreach (var e in enums)
            {
                CheckIsEnum(e);
                this.Parameters.Add(new ParameterInfo(e));
            }
        }

        static void CheckIsEnum(Type t)
        {
            if (!t.IsEnum)
            {
                throw new ArgumentException(string.Format("{0} must be an enumerated type",t.Name));
            }
        }
    }

    public class ParameterInfo : Input
    {
        public Type EnumType;
        public Type ValueType;
        public Enum SymbolicValue;
        public object ActualValue;
        public ParameterInfo(Type enumType, Type valueType=null, ParameterValueFactory factory=null)
        {
            this.EnumType = enumType;
            this.ValueType = valueType;
        }
    }

    public abstract class ParameterValueFactory
    {

    }
}
