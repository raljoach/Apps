using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Place : IItem
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
            this.DistanceValue = double.MaxValue;
            this.DurationValue = double.MaxValue;
        }
        public bool Visited { get; set; }
        public bool Preferred { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Open { get; set; }
        public string Close { get; set; }
        public string Days { get; set; }
        public GeoLocation GeoLocation { get; set; }  
        public string Url { get; set; }
        public string DistanceText { get; set; }
        public string DurationText { get; set; }
        public double DistanceValue { get; set; }
        public double DurationValue { get; set; }
        public int DistanceRank { get; set; }
        public int DurationRank { get; set; }
        public int OriginId { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Place;
            if (other == null) return false;
            return this.Id == other.Id /*|| this.Name == other.Name*/;
        }
    }
}
