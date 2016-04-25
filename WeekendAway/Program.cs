using Common;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolkit;

namespace WeekendAway
{                
    public partial class Program
    {        
        static void Main(string[] args)
        {
            var file = "visit.json";
            if (!File.Exists(file)) { throw new InvalidOperationException(string.Format("Error: File does not exist! {0}", file)); }
            var visit = JsonConvert.DeserializeObject<Visit>(FileIO.ReadAll(file));
            var start = visit.Start;
            var index = start.Id - 1;
            var s = visit.Places[index];
            if (!string.IsNullOrWhiteSpace(start.Name))
            {
                if (!s.Name.ToLower().Equals(start.Name.ToLower()))
                {
                    throw new InvalidOperationException(string.Format("Error: Start Id='{0}' does not match element in Places at index {1}, which is '{2}'", start.Id, index, s.Name));
                }
            }           
            var others = visit.Places;
            var matrix = MapToolkit.DistanceMatrix(s, others);
        }               
    }    
}
