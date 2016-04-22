using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeFinder
{
    public class GeoLocation
    {
        public double Latitude;
        public double Longitude;
    }
    public class Address
    {
        public string Number;
        public string Apt;
        public string Street;
        public string City;
        public string State;
        public string Zipcode;

    }
    public class Home
    {
        public Address Address;
        public GeoLocation Location;
    }
    public class Listing
    {
        public Home Home;
        public double Price;
        public string TypeOfResidence;
        public string Builder;
        //public double CurrentPrice;
    }

    enum Period
    {
        Days,
        Months,
        Years
    }

    class Program
    {
        static string state = "Washington";
        static string city = "Issaquah";
        static int bedrooms = 4;
        static int baths = 2;
        static List<int> periodsYears = new List<int>() { 1, 5 };
        static List<int> periodsMonths = new List<int>() { 1, 3, 5 };
        static List<int> periodsDays = new List<int> { 1, 7 };

        static void Main(string[] args)
        {
            var loc = GetLatLong(state, city);
            var listings = GetListings(loc, bedrooms, baths, 10);
            //var listings2 = GetListings(state, city, bedrooms, baths);
            double sum = 0.0;
            foreach(var listing in listings)
            {
                sum += listing.Price;
            }
            double avg = sum / (double)listings.Count;
            var history = GetHistoricalData(listings, Period.Months, 3);
            var histAvg = history.Average();
            var histStd = history.StandardDeviation();
            foreach(var listing in listings)
            {
                var z = history.Zscore(listing);
                if(z<=-2)
                {
                    //cheaper
                    //buy
                    //or sell (losing value)

                   //If I'm a buyer, then I would buy this house
                   //  because it's cheaper than what it was paid for in the past


                   //If I'm a speculator (looking at making profits), then this house is selling at a loss
                   // If I could have predicted this, I would have never bought this house yesterday
                }
                else if(z>=2)
                {
                    //more expensive
                    //sell
                    //or buy (if you wait, it might get more expensive? how do you know unless you predict)

                    //If I'm a buyer, then I wouldn't buy this house
                    //  because it's more expensive than it was in the past

                    //If I'm a speculator (looking at making profits), then this house would earn me money
                    //   If I could ahve predicted this, I would have bought this house yesterday
                }
            }

            var current = GetHistoricalData(listings, Period.Days, 1);
            var startDate = current.Data.GetOldest();
            var baseline = GetHistoricalData(listings, Period.Months, 3, startDate);
            //SO I'm a speculator
            //I need to gather the history of the houses in the area
            //I need to watch them, (sliding window)
            //I need to predict what these houses are going to be worth (tomorrow)
            //I'm going to buy only if the house goes up in value, according to my prediction
            //If it goes down in value, I will skip it entirely
            foreach (var data in GetNewData())
            {                
                baseline.Data.RemoveOldest();
                baseline.Data.Add(current.Data.RemoveOldest());
                var baseAvg = history.Average();
                var baseStd = history.StandardDeviation();
                var currentAvg = current.Averag();

                //How does the present market compare to yesterday's market
                var z = history.Zscore(currentAvg);
                if (z <= -2)
                {
                    //cheaper => my gut tells me the market will predict a drop for the future (once I use the baseline+current=history)
                }
                else if (z >= 2)
                {
                    //more expensive => my gut tells me the market will predict an increase for the future                  
                }              
            }

            var housesToBuy = new List<Listing>();
            //LOOP: listing in listings
            // Predict then slide
            // Predict tomorrow's price for all listings
            // Now slide history forward: change detection
            //  => anomaly up? z>2 (for currentAvg z-score in baseline distribution?)
            //      => Buyer: buy a house quick today (profit make)!
            //      => Seller: hold my house (will make more money if i sell it tomorrow)!
            /*housesToBuy.Add(listing);*/

            //  => anomaly down? z<-2 (for currentAvg z-score in baseline distribution?)
            //      => Seller: sell my property quick (prevent profit loss)!
            //      => Buyer: wait on buying a house (will be cheaper later, be patient)!

            //  => still the same? (-2<=z<2)
            //END LOOP

            var housesShouldBuy = new List<Listing>();
            var housesAvoid = new List<int>();
            for (int j=0; j<housesToBuy.Count; j++)
            {
                var h = housesToBuy[j];
                // Do I like it?
                var like = LikeIt(h); //uses similarity formula
                if(like)
                {
                    var afford = CanAfford(h);
                    if(afford)
                    {
                        housesShouldBuy.Add(h);
                    }
                    else { housesAvoid.Add(j); /*"Reason: Can't afford"*/ }
                } else { housesAvoid.Add(j); /*"Reason: Will not like (predict)"*/ }
            }

            //Go for these houses first (make an offer)
            Listing myHouse = null;
            foreach(var thisHouse in housesShouldBuy)
            {
                var accepted = MakeOffer(thisHouse);
                if(accepted)
                {
                    myHouse = thisHouse;
                    break;
                }
            }

            //Go for these houses next (if the first set of houses, you don't get)
            for(int j=0; myHouse!=null && j<housesToBuy.Count; j++)
            {
                if(housesAvoid.Contains(j)) { continue; }
                var accepted = MakeOffer(housesToBuy(j));
                if(accepted)
                {
                    myHouse = housesToBuy[j];
                    break;
                }
            }

            //return myHouse;
        }

        public enum Role { None, Seller, Buyer }
        public enum Action { None, Buy, Sell }

        private static List<Listing> GetListings(string state, string city, int bedrooms, int baths, int? miles=null)
        {
            throw new NotImplementedException();
        }


        private static List<Listing> GetListings(GeoLocation loc, int bedrooms, int baths, int miles)
        {
            throw new NotImplementedException();
        }

        private static GeoLocation GetLatLong(string state, string city)
        {
            throw new NotImplementedException();
        }
    }
}
