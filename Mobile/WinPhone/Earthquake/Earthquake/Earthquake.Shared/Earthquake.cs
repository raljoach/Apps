using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Earthquake
{
    public class Earthquake
    {
        public string eqid { get; set; }
        public double magnitude { get; set; }
        public double lng { get; set; }
        public string src { get; set; }
        public string datetime { get; set; }
        public double depth { get; set; }
        public double lat { get; set; }
    }

    [DataContract]
    public class RootObject
    {
        [DataMember]
        public List<Earthquake> earthquakes { get; set; }
    }
}
