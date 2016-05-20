using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeekendAway.Common
{
    public class Visit
    {
        public Visit() { }
        public Visit(List<Place> places)
        {
            this.Places = places;
        }

        public Visit(Place[] places)
        {
            Places = new List<Place>();
            Places.AddRange(places);
        }

        public Place Start { get; set; }
        public List<Place> Places { get; set; }
    }
}
