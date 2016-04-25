using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Place
    {
        public Dictionary<string, string> Hours { get; set; }
        public Place() { }
        public Place(int id, string name, string address, Dictionary<string, string> hours, GeoLocation geoLocation)
        {
            this.Id = id;
            this.Name = name;
            this.Address = address;
            this.Hours = hours;
            this.GeoLocation = geoLocation;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Open { get; set; }
        public string Close { get; set; }
        public string Days { get; set; }
        public GeoLocation GeoLocation { get; set; }  
        public string Url { get; set; }      
    }
}
