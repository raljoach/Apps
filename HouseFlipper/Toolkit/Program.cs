using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolkit
{
    class Program
    {
        static void Main(string[] args)
        {
            //CreateDataProperties();
            var list = new List<string>()
            {
                "#","ML Number","Status","Address","City","Postal Code","Legal Subdivision Name","Sq Ft Heated","Current Price","Beds","Full Baths","Half Baths","Year Built","Pool","Property Style","Taxes","CDOM","ADOM","Days to Contract","Sold Terms","Close Date","LP/SqFt","SP/SqFt","SP/LP","List Office Name","List Agent Full Name","List Agent ID","Selling Agent Name","Selling Office ID","Selling Agent ID","LSC List Side","Office Primary Board ID"
            };
            var numericPropCols = new List<string>()
            {
                "F","H","I","J-M","P-S","V-X","AA","AD"
            };
            var datePropCols = new List<string>()
            {
                "U"
            };
            var dataType = "double";
            foreach (var n in numericPropCols)
            {
                foreach (string prop in CreateProperty(list, dataType, n))
                {
                    ImplementProperty(prop, "GetNumericValue");
                }
            }
            dataType = "DateTime";
            foreach (var d in datePropCols)
            {
                foreach (string prop in CreateProperty(list, dataType, d))
                {
                    ImplementProperty(prop, "DateTime.Parse");
                }
            }
            Console.WriteLine("Program has ended. Hit any key to exit");
            Console.ReadKey();
        }

        private static void ImplementProperty(string prop, string method)
        {
            var temp = prop.Replace(" {", "Value {").Replace("set;",string.Empty);
            var tokens = prop.Split(' ');
            temp = temp.Replace("get;",
                "get { " + string.Format("return {0}(this.{1});", method, tokens[2]) + " }"
                );
            Console.WriteLine(temp);
        }

        private static IEnumerable<string> CreateProperty(List<string> list, string dataType, string n)
        {
            string prop = null;
            if (n.Contains("-"))
            {
                var tokens = n.Split('-');
                var start = tokens[0];
                var end = tokens[1];
                foreach (var letter in GetRange(start, end))
                {
                    prop = HandleCreateProperty(list, dataType, letter);
                    yield return prop;
                }
            }
            else
            {
                yield return HandleCreateProperty(list, dataType, n);
            }
        }

        private static IEnumerable<string> GetRange(string start, string end)
        {
            var current = start;
            while(current!=end)
            {
                current = HandleGetNext(current);
                yield return current;
            }           
        }

        private static string HandleGetNext(string current)
        {
            string nextStr = string.Empty;
            if (current.Length > 1)
            {
                string next = string.Empty;
                for (int j = 0; j < current.Length - 1; j++)
                {
                    next += current[j].ToString();
                }
                var last = current[current.Length - 1];
                nextStr += GetNextChar(last.ToString());
            }
            else
            {
                nextStr = GetNextChar(current);
            }

            return nextStr;
        }

        private static string GetNextChar(string current)
        {
            string nextStr;
            if (current == "Z")
            {
                nextStr = "AA";
            }
            else
            {
                var ch = current[0];
                char next = (char)((int)ch + 1);
                nextStr = next.ToString();
            }

            return nextStr;
        }

        private static string HandleCreateProperty(List<string> list, string dataType, string n)
        {
            string prop = null;
            
            if (n.Length > 1)
            {
                if (n.Length == 2)
                {
                    if (n[0] == 'A')
                    {
                        var ch = n[1];
                        var delta = ch - 'A';
                        var z = 25;
                        var i = z + delta;
                        var colName = list[i];
                        prop = GenerateProperty(colName, dataType);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                var i = n[0] - 'A';
                var colName = list[i];
                prop = GenerateProperty(colName, dataType);
            }

            return prop;
        }

        private static void CreateDataProperties()
        {
            var list = new List<string>()
            {
                "ML Number","Status","Address","City","Postal Code","Legal Subdivision Name","Sq Ft Heated","Current Price","Beds","Full Baths","Half Baths","Year Built","Pool","Property Style","Taxes","CDOM","ADOM","Days to Contract","Sold Terms","Close Date","LP/SqFt","SP/SqFt","SP/LP","List Office Name","List Agent Full Name","List Agent ID","Selling Agent Name","Selling Office ID","Selling Agent ID","LSC List Side","Office Primary Board ID"
            };

            foreach (string colName in list)
            {
                var dataType = "string";
                string prop = GenerateProperty(colName, dataType);
                Console.WriteLine(prop);
            }
        }

        private static string GenerateProperty(string colName, string dataType)
        {
            var propName = colName.Replace(" ", string.Empty).Replace("/", string.Empty);
            //var prop = string.Format(@"public string {0} { get; set; }", propName);                
            string prop = CreateProperty(propName, dataType);
            return prop;
        }

        private static string CreateProperty(string propName, string dataType)
        {
            var prop = string.Format(@"public {0} {1} ", dataType, propName);
            prop += "{ get; set; }";
            return prop;
        }
    }
}
