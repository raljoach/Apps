using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{
    public class Parameter
    {
        public string Name;
        public string Type;
        public string Values;
        public List<Enum> Enumeration;
        public Type EnumType;

        public Parameter(string name, string type, string values)
        {
            this.Name = name;
            this.Type = type;
            this.Values = values;

            var types = System.Reflection.Assembly.GetAssembly(this.GetType()).GetTypes();
            foreach (var t in types)
            {
                if (t.IsEnum)
                {
                    if (t.Name.ToLower().Equals(name.ToLower()))
                    {
                        DataTypeAttribute attrib = GetDataType(t);
                        if (attrib != null && attrib.Type.Name.ToLower() != this.Type.ToLower())
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                  "Error: For parameter {0}, Found enumeration {1}," +
                                  "but the enumeration's associated DataType {2} " +
                                  "does not match the specified parameter type {3}",
                                  name, t.Name,
                                  attrib.Type.Name,
                                  type));
                        }
                        this.EnumType = t;
                        var enumValues = Enum.GetValues(t);
                        if (string.IsNullOrWhiteSpace(Values))
                        {
                            Enumeration.AddRange((Enum[])enumValues);
                        }
                        else
                        {
                            var valTokens = this.Values.Trim().Split(',');
                            foreach (var val in valTokens)
                            {
                                var tmp = val.ToLower().Trim();
                                var found = false;
                                foreach (Enum e in enumValues)
                                {
                                    if (e.ToString().ToLower() == tmp)
                                    {
                                        if (Enumeration == null) { Enumeration = new List<Enum>(); }
                                        Enumeration.Add(e);
                                        found = true;
                                        break;
                                    }
                                }
                                if(!found)
                                {
                                    throw new InvalidOperationException(
                                                string.Format(
                                                    "Error: Parameter value '{0}' not found in Enum '{1}'",
                                                    val, t.Name 
                                                )
                                              );
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        private DataTypeAttribute GetDataType(Type t)
        {
            foreach(var a in t.GetCustomAttributes(true))
            {
                if(a is DataTypeAttribute)
                {
                    return (DataTypeAttribute)a;
                }
            }
            return null;
        }
    }
}
