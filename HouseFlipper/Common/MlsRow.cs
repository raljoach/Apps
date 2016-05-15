using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace HouseFlipper.Common
{
    public enum MlsStatus { Active, Sold }
    public class MlsRow : IComparable
    {
        public MlsRow(StringDictionary data)
        {
            this.Lookup = data;
            var hash = new Dictionary<string, PropertyInfo>();
            foreach (var p in typeof(MlsRow).GetProperties())
            {
                var thisName = p.Name.ToLower();
                hash.Add(thisName, p);
            }

            foreach (string k in data.Keys)
            {
                var name = k.ToLower();
                if (name == "#") { continue; }
                var propName = name.Replace(" ", string.Empty).Replace("/", string.Empty);

                if (hash.ContainsKey(propName))
                {
                    var p = hash[propName];
                    p.SetMethod.Invoke(this, new object[] { data[k] });
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            /*
            foreach (var p in typeof(MlsRow).GetProperties())
            {
                var name = p.Name.ToLower();
                if (data.ContainsKey(name))
                {
                    var val = data[name];
                    p.SetMethod.Invoke(this, new object[] { val });
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }*/
        }

        public MlsRow() { }

        public StringDictionary Lookup { get; private set; }

        public double GetNumericValue(string val)
        {
            val = val.Replace(",", string.Empty).Replace("$", string.Empty);
            return double.Parse(val);
        }

        public string MLNumber { get; set; }
        public string Status { get; set; }
        public MlsStatus StatusValue {
            get {
                var str = this.Status.ToLower();
                if(str=="sld")
                {
                    return MlsStatus.Sold;
                }
                else if(str=="act")
                {
                    return MlsStatus.Active;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string LegalSubdivisionName { get; set; }
        public string SqFtHeated { get; set; }

        public double SqFtHeatedValue {
            get
            {/*
                var val = this.SqFtHeated.Replace(",", string.Empty);
                return double.Parse(val);*/
                return GetNumericValue(this.SqFtHeated);
            }
         
        }

        public string CurrentPrice { get; set; }
        public double CurrentPriceValue()
        {
            
                return GetNumericValue(this.CurrentPrice);
            
        }
        public string Beds { get; set; }
        public double BedsValue()
        {
            
                return GetNumericValue(this.Beds);
            
        }
        public string FullBaths { get; set; }
        public double FullBathsValue()
        {
            
                return GetNumericValue(this.FullBaths);
            
        }
        public string HalfBaths { get; set; }
        public double HalfBathsValue()
        {
            
                return GetNumericValue(this.HalfBaths);
            
        }
        public string YearBuilt { get; set; }
        public double YearBuiltValue()
        {
            
                return GetNumericValue(this.HalfBaths);
            
        }
        public string Pool { get; set; }
        public string PropertyStyle { get; set; }
        public string Taxes { get; set; }
        public string CDOM { get; set; }
        public string ADOM { get; set; }
        public string DaystoContract { get; set; }
        public string SoldTerms { get; set; }
        public string CloseDate { get; set; }
        public string LPSqFt { get; set; }
        public string SPSqFt { get; set; }
        public string SPLP { get; set; }
        public string ListOfficeName { get; set; }
        public string ListAgentFullName { get; set; }
        public string ListAgentID { get; set; }
        public string SellingAgentName { get; set; }
        public string SellingOfficeID { get; set; }
        public string SellingAgentID { get; set; }
        public string LSCListSide { get; set; }
        public string OfficePrimaryBoardID { get; set; }

        public double PostalCodeValue() { return GetNumericValue(this.PostalCode);  }
       /* public double SqFtHeatedValue { return GetNumericValue(this.SqFtHeated); } }
        public double CurrentPriceValue { return GetNumericValue(this.CurrentPrice); } }
        public double FullBathsValue { return GetNumericValue(this.FullBaths); } }
        public double HalfBathsValue { return GetNumericValue(this.HalfBaths); } }
        public double YearBuiltValue { return GetNumericValue(this.YearBuilt); } }*/
        public double CDOMValue() { return GetNumericValue(this.CDOM);  }
        public double ADOMValue() { return GetNumericValue(this.ADOM);  }
        public double DaystoContractValue() { return GetNumericValue(this.DaystoContract);  }
        public double LPSqFtValue() { return GetNumericValue(this.LPSqFt);  }
        public double SPSqFtValue() { return GetNumericValue(this.SPSqFt);  }
        public double SPLPValue() { return GetNumericValue(this.SPLP);  }
        public double ListAgentIDValue() { return GetNumericValue(this.ListAgentID);  }
        public double SellingAgentIDValue() { return GetNumericValue(this.SellingAgentID);  }
        public DateTime CloseDateValue() { return DateTime.Parse(this.CloseDate);  }

        public int CompareTo(object obj)
        {
            var other = obj as MlsRow;
            return this.CloseDateValue().CompareTo(other.CloseDateValue());
        }
    }
}
