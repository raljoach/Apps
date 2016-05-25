using Common;
using Common.IO;
using HouseFlipper.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseFlipper
{
    class Program
    {
        private static string dataRelFolder = @"data";
        static void Main(string[] args)
        {
            string dataFolder = null;
            if (args != null && args.Length > 0)
            {
                dataFolder = args[0];
                if (!Directory.Exists(dataFolder))
                {
                    Logger.Debug("Missing data folder: {0}", dataFolder);
                    return;
                }
            }
            else
            {
                dataFolder = GetDefaultFolder();
            }
            var indivSubDivResults = new Dictionary<string, FlippedHouse>();
            var activeMultiKeyHash = new Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>>();
            //var currentActiveHash = new Dictionary<string, MlsRow>();
            var zipResults = DataRead(indivSubDivResults, /*currentActiveHash,*/ activeMultiKeyHash, dataFolder);
            PrintZipProfitabilityRanking(zipResults);
            
            PrintSubDivProfitabilityRanking(indivSubDivResults);
            var commonSubDivResults = PrintSubDivAlphabetical(indivSubDivResults);

            PrintActiveWithinZip(zipResults, indivSubDivResults, activeMultiKeyHash);
            PrintActiveWithinCommonSubDiv(zipResults, commonSubDivResults, activeMultiKeyHash);

            Console.WriteLine();
            Console.WriteLine("Program has ended. Hit any key to exit.");
            Console.ReadKey();
        }

        private static void PrintActiveWithinCommonSubDiv(Dictionary<string, FlippedHouse> zipResults, Dictionary<string, FlippedHouse> commonSubDivResults, Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> activeMultiKeyHash)
        {
            throw new NotImplementedException();
        }

        private static void PrintActiveWithinZip(Dictionary<string, FlippedHouse> zipResults, Dictionary<string, FlippedHouse> indivSubDivResults, Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> activeMultiKeyHash)
        {
            var activeZipHouses = new Dictionary<string, List<MlsRow>>();
            var activeSubDivHouses = new Dictionary<string, List<MlsRow>>();
            foreach(var zip in zipResults.Keys)
            {                
                var eligibleZipHouses = new List<MlsRow>();
                activeZipHouses.Add(zip, eligibleZipHouses);
                var flipResults = zipResults[zip];
                var subDivHash = activeMultiKeyHash[zip];
                foreach(var subDiv in subDivHash.Keys)
                {
                    var eligibleSubDivHouses = new List<MlsRow>();
                    activeSubDivHouses.Add(zip+"::"+subDiv,eligibleSubDivHouses);
                    var houseHash = subDivHash[subDiv];
                    foreach(var houseId in houseHash.Keys)
                    {
                        var records = houseHash[houseId];
                        if(records.Count!=1)
                        {
                            throw new InvalidOperationException("Error: Don't know what to do with multiple active listings for the same house id!");
                        }
                        var house = records[0];
                        if(house.CurrentPriceValue() <= 0.2 * flipResults.PreFlip.Price)
                        {
                            eligibleZipHouses.Add(house);
                            eligibleSubDivHouses.Add(house);
                        }
                    }
                }
            }

            foreach (var zip in activeZipHouses.Keys)
            {
                var flipResults = zipResults[zip];
                var houses = activeZipHouses[zip];
                var numHouses = houses.Count;
                double newPrice = flipResults.PostFlip.Price;
                Logger.Debug("Zipcode: {0} >> {1} Eligible houses within 20% of preflip price ${2}", zip, numHouses, ToString(newPrice));
                Logger.Border();
                var count = 0;
                foreach (var house in houses)
                {
                    ++count;
                    var homeAddress = house.Address + " " + house.City + " " + house.PostalCode;
                    //var flipResults = zipResults[zip];
                    double currentPrice = house.CurrentPriceValue();                    
                    double profit = newPrice - currentPrice;
                    Console.WriteLine(
                        "{0}) {1}: {2} " +
                        "in zip {3} has current price {4} can be sold for ${5} " +
                        "Potential Profit: ${6}", 
                        count, house.MLNumber, homeAddress, 
                        zip, house.CurrentPrice, ToString(newPrice), 
                        ToString(profit));
                }
            }
        }

        private static void PrintZipProfitabilityRanking(Dictionary<string, FlippedHouse> results)
        {
            var list = results.Values.ToList();
            list.Sort();
            Console.WriteLine("Zipcodes ordered by profitability:");
            Logger.Border();
            for (int k = 0; k < list.Count; k++)
            {
                var r = list[k];
                Console.WriteLine("{0}) {1} | ${2}", k + 1, r.Zipcode, ToString(r.Profit));
            }
            Logger.Border();
        }

        private static void PrintSubDivProfitabilityRanking(Dictionary<string, FlippedHouse> results)
        {
            var list = results.Values.ToList();
            list.Sort();
            Console.WriteLine("Subdivisions ordered by profitability:");
            Logger.Border();
            for (int k = 0; k < list.Count; k++)
            {
                var r = list[k];
                //Console.WriteLine("{0}) {1} | {2} |${3}", k + 1, r.Zipcode, r.SubDivision, ToString(r.Profit));
                Console.WriteLine("{0}) ({1}) {2} => ${3}", k + 1, r.City, r.Zipcode, ToString(r.Profit));
            }
            Logger.Border();
        }

        private static Dictionary<string, FlippedHouse> PrintSubDivAlphabetical(Dictionary<string, FlippedHouse> indivSubDivResults)
        {
            var commonDivResults = new Dictionary<string, FlippedHouse>(StringComparer.OrdinalIgnoreCase);
            var list = indivSubDivResults.Values.ToList();
            list.Sort(
                (x, y) =>
                {
                    if (x == null && y == null) { return 0; }
                    if (x == null && y != null) { return 1; }
                    if (x != null && y == null) { return -1; }
                    var cityComp = x.City.CompareTo(y.City);
                    if (cityComp == 0)
                    {
                        return x.SubDivision.CompareTo(y.SubDivision);
                    }
                    return cityComp;
                }
                );
            Console.WriteLine("Subdivisions ordered alphabetically by city then subdivision:");
            Logger.Border();

            for (int k = 0; k < list.Count; k++)
            {
                var next = list[k];

                //Console.WriteLine("{0}) {1} | {2} |${3}", k + 1, r.Zipcode, r.SubDivision, ToString(r.Profit));
                Console.WriteLine("{0}) ({1}) {2} PreFlip:[Sum={3},Count={4},AvgPrice={5}] PostFlip:[Sum={6},Count={7},AvgPrice={8}] => Profit:${9}", k + 1, next.City, next.Zipcode, next.PreFlip.Sum, next.PreFlip.Price, next.PreFlip.NumHouses, next.PostFlip.Sum, next.PostFlip.NumHouses, next.PostFlip.Price, ToString(next.Profit));

                if (k == 0) { continue; }
                AggregateByCommonSubDivision(commonDivResults, list, k, next);
            }

            Logger.Border();
            var bigSubDivResults = PrintCommonSubDiv(commonDivResults);
            PrintCommonByProfit(bigSubDivResults);
            PrintCommonByCityThenProfit(bigSubDivResults);
            return commonDivResults;
        }

        private static void PrintCommonByProfit(Dictionary<string, FlippedHouse> finalHash)
        {
            var list = finalHash.Values.ToList();
            list.Sort();
            Console.WriteLine("Common subdivisions ordered by profitability:");
            Logger.Border();

            for (int k = 0; k < list.Count; k++)
            {
                var r = list[k];
                //Console.WriteLine("{0}) {1} | {2} |${3}", k + 1, r.Zipcode, r.SubDivision, ToString(r.Profit));
                Console.WriteLine("{0}) ({1}) {2} => ${3}", k + 1, r.City, r.SubDivision, ToString(r.Profit));
            }
            Logger.Border();
        }

        private static void PrintCommonByCityThenProfit(Dictionary<string, FlippedHouse> finalHash)
        {
            var list = finalHash.Values.ToList();
            list.Sort(
                (x, y) =>
                {
                    if (x == null && y == null) { return 0; }
                    if (x == null && y != null) { return 1; }
                    if (x != null && y == null) { return -1; }
                    var cityComp = x.City.CompareTo(y.City);
                    if (cityComp == 0)
                    {
                        return x.Profit.CompareTo(y.Profit);
                    }
                    return cityComp;
                }
                );
            Console.WriteLine("Common subdivisions ordered by city, then profitability:");
            Logger.Border();

            for (int k = 0; k < list.Count; k++)
            {
                var r = list[k];
                //Console.WriteLine("{0}) {1} | {2} |${3}", k + 1, r.Zipcode, r.SubDivision, ToString(r.Profit));
                Console.WriteLine("{0}) ({1}) {2} => ${3}", k + 1, r.City, r.SubDivision, ToString(r.Profit));
            }
            Logger.Border();
        }

        private static Dictionary<string, FlippedHouse> PrintCommonSubDiv(Dictionary<string, FlippedHouse> commonSubDivHash)
        {
            var bigSubDivResults = new Dictionary<string, FlippedHouse>();
            var list = commonSubDivHash.Values.ToList();
            list.Sort(
                (x, y) =>
                {
                    if (x == null && y == null) { return 0; }
                    if (x == null && y != null) { return 1; }
                    if (x != null && y == null) { return -1; }
                    var cityComp = x.City.CompareTo(y.City);
                    if (cityComp == 0)
                    {
                        return x.SubDivision.CompareTo(y.SubDivision);
                    }
                    return cityComp;
                }
                );
            Console.WriteLine("Common subdivisions ordered alphabetically by city then subdivision:");
            Logger.Border();

            var count = 0;
            for (int k = 0; k < list.Count; k++)
            {
                var next = list[k];
                if (next.Aggregation.Count == 1) { continue; }
                ++count;

                var subDiv = next.SubDivision.Trim();
                var updated = true;
                while (updated)
                {
                    updated = false;
                    var end = subDiv.Length - 1;
                    var w = end;
                    var start = 0;
                    for (; w >= start; w--)
                    {
                        var ch = subDiv[w];
                        if (Char.IsLetter(ch))
                        {
                            break;
                        }
                    }

                    if (w != end)
                    {
                        if (w >= start)
                        {
                            subDiv = subDiv.Substring(start, w - start + 1).Trim();
                            updated = true;
                        }
                    }

                    if (subDiv.ToLower().EndsWith(" unit"))
                    {
                        subDiv = subDiv.Substring(start, subDiv.Length - " unit".Length - start).Trim();
                        updated = true;
                    }

                    if (subDiv.ToLower().EndsWith(" ph"))
                    {
                        subDiv = subDiv.Substring(start, subDiv.Length - " ph".Length - start).Trim();
                        updated = true;
                    }

                    if (subDiv.ToLower().EndsWith(" sec"))
                    {
                        subDiv = subDiv.Substring(start, subDiv.Length - " sec".Length - start).Trim();
                        updated = true;
                    }
                }

                //Console.WriteLine("{0}) {1} | {2} |${3}", k + 1, r.Zipcode, r.SubDivision, ToString(r.Profit));
                Console.WriteLine(
                    "{0}) ({1}) {2} ***{3}*** SubDivCount={4} " +
                    "PreFlip:[Sum={5},Count={6},AvgPrice={7}] " +
                    "PostFlip:[Sum={8},Count={9},AvgPrice={10}] => " +
                    "Profit:${11}", 
                    count, next.City, subDiv, next.Zipcode, next.Aggregation.Count,
                    next.PreFlip.Sum, next.PreFlip.NumHouses, next.PreFlip.Price, 
                    next.PostFlip.Sum, next.PostFlip.NumHouses, next.PostFlip.Price, 
                    ToString(next.Profit));

                var flipHouse = new FlippedHouse()
                {
                    City = next.City,
                    PostFlip = next.PostFlip,
                    PreFlip = next.PreFlip,
                    SubDivision = subDiv,
                    Zipcode = next.Zipcode
                };
                flipHouse.Aggregation = new List<FlippedHouse>();
                flipHouse.Aggregation.AddRange(next.Aggregation);
                bigSubDivResults.Add(subDiv, flipHouse);

                /*
                Console.WriteLine("\t [{0}]", next.SubDivision.Trim());
                Console.WriteLine("\t ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ ");
                foreach (var thisSubDiv in next.Aggregation)
                {
                    Console.WriteLine("\t+ {0}", thisSubDiv.SubDivision);
                }
                */
            }

            Logger.Border();
            return bigSubDivResults;
        }

        private static void AggregateByCommonSubDivision(Dictionary<string, FlippedHouse> commonSubDivHash, List<FlippedHouse> list, int k, FlippedHouse next)
        {
            var currentName = next.SubDivision;
            bool found = false;
            //See if you have something in common with any of the keys
            foreach (var key in commonSubDivHash.Keys)
            {
                var prevName = key;
                string commonSubDivName = GetCommonSubDivName(prevName, currentName).Trim();

                if (commonSubDivName != string.Empty)
                /*{
                    //Nothing in common
                    // Store prev and next separately
                    var data = new FlippedHouse()
                    {
                        City = next.City,
                        Zipcode = next.Zipcode,
                        PreFlip = new HouseCharacteristics()
                        {
                            Sum = next.PreFlip.Sum,
                            NumHouses = next.PreFlip.NumHouses

                        },
                        PostFlip = new HouseCharacteristics()
                        {
                            Sum = next.PostFlip.Sum,
                            NumHouses = next.PostFlip.NumHouses
                        },
                    };
                    commonSubDivHash.Add(currentName, data);

                    //averages
                    data.PreFlip.Price = data.PreFlip.Sum / data.PreFlip.NumHouses;
                    data.PostFlip.Price = data.PostFlip.Sum / data.PostFlip.NumHouses;

                    break;
                }
                else */ //Something in common
                {
                    found = true;
                    /*if (commonSubDivHash.ContainsKey(commonSubDivName))
                    {
                        var data = commonSubDivHash[commonSubDivName];
                        data.PreFlip.Sum += next.PreFlip.Sum;
                        data.PreFlip.NumHouses += next.PreFlip.NumHouses;
                        data.PreFlip.Price = data.PreFlip.Sum / data.PreFlip.NumHouses;
                        data.PostFlip.Sum += next.PostFlip.Sum;
                        data.PostFlip.NumHouses += next.PostFlip.NumHouses;
                        data.PostFlip.Price = data.PostFlip.Sum / data.PostFlip.NumHouses;
                    }
                    else
                    {*/

                    var prev = commonSubDivHash[key];
                    string newZip = CombineZips(prev, next);

                    FlippedHouse totalData = new FlippedHouse()
                    {
                        SubDivision = commonSubDivName,

                        City = next.City,
                        Zipcode = newZip, // prev.Zipcode + (prevZip.ToLower().Trim() != nextZip.ToLower().Trim() ? ("," + nextZip) : string.Empty),
                        PreFlip = new HouseCharacteristics()
                        {
                            Sum = prev.PreFlip.Sum + next.PreFlip.Sum,
                            NumHouses = prev.PreFlip.NumHouses + next.PreFlip.NumHouses
                        },
                        PostFlip = new HouseCharacteristics()
                        {
                            Sum = prev.PostFlip.Sum + next.PostFlip.Sum,
                            NumHouses = prev.PostFlip.NumHouses + next.PostFlip.NumHouses
                        }
                    };
                    totalData.PreFlip.Price = totalData.PreFlip.Sum / totalData.PreFlip.NumHouses;
                    totalData.PostFlip.Price = totalData.PostFlip.Sum / totalData.PostFlip.NumHouses;
                    totalData.Aggregation = new List<FlippedHouse>();
                    totalData.Aggregation.AddRange(prev.Aggregation);
                    totalData.Aggregation.Add(next);
                    var newName = next.SubDivision;
                    commonSubDivHash.Remove(key);
                    commonSubDivHash.Add(commonSubDivName, totalData);

                    //}

                    break;
                }

            }

            if (!found) // if nothing found common with any of the keys, let's look at previous
            {
                string prevName = list[k - 1].SubDivision;
                if (prevName != null)
                {
                    string commonSubDivName = GetCommonSubDivName(prevName, currentName).Trim();
                    if (commonSubDivName == string.Empty)
                    {
                        //Nothing in common with previous
                        // so add next to commonSubDiv
                        var data = new FlippedHouse()
                        {
                            SubDivision = next.SubDivision,
                            City = next.City,
                            Zipcode = next.Zipcode,
                            PreFlip = new HouseCharacteristics()
                            {
                                Sum = next.PreFlip.Sum,
                                NumHouses = next.PreFlip.NumHouses

                            },
                            PostFlip = new HouseCharacteristics()
                            {
                                Sum = next.PostFlip.Sum,
                                NumHouses = next.PostFlip.NumHouses
                            },
                            Aggregation = new List<FlippedHouse>() { next }
                        };
                        commonSubDivHash.Add(currentName, data);

                        //averages
                        data.PreFlip.Price = data.PreFlip.Sum / data.PreFlip.NumHouses;
                        data.PostFlip.Price = data.PostFlip.Sum / data.PostFlip.NumHouses;

                        if (k == 1)  //unique case where, for the first list item, we need to store it in common sub div as well if it doesn't match
                        {
                            var prev = list[k - 1];
                            var prevData = new FlippedHouse()
                            {
                                SubDivision = prev.SubDivision,
                                City = prev.City,
                                Zipcode = prev.Zipcode,
                                PreFlip = new HouseCharacteristics()
                                {
                                    Sum = prev.PreFlip.Sum,
                                    NumHouses = prev.PreFlip.NumHouses

                                },
                                PostFlip = new HouseCharacteristics()
                                {
                                    Sum = prev.PostFlip.Sum,
                                    NumHouses = prev.PostFlip.NumHouses
                                },
                                Aggregation = new List<FlippedHouse>() { prev }
                            };
                            commonSubDivHash.Add(prevName, prevData);
                        }
                    }
                    else //Something in common with previous  (We assume, previous has already been taken care of: aggregated into common already)
                    {
                        if (commonSubDivHash.ContainsKey(commonSubDivName)) //Look for that root common name
                        {
                            var data = commonSubDivHash[commonSubDivName];  // then just add the next data
                            data.PreFlip.Sum += next.PreFlip.Sum;
                            data.PreFlip.NumHouses += next.PreFlip.NumHouses;
                            data.PreFlip.Price = data.PreFlip.Sum / data.PreFlip.NumHouses;
                            data.PostFlip.Sum += next.PostFlip.Sum;
                            data.PostFlip.NumHouses += next.PostFlip.NumHouses;
                            data.PostFlip.Price = data.PostFlip.Sum / data.PostFlip.NumHouses;
                            data.Aggregation.Add(next);
                        }
                        else
                        {
                            var prev = list[k - 1];
                            string newZip = CombineZips(prev, next);

                            FlippedHouse totalData = new FlippedHouse()
                            {
                                SubDivision = commonSubDivName,
                                City = next.City,
                                Zipcode = newZip, //prev.Zipcode + (prev.Zipcode.ToLower().Trim() != next.Zipcode.ToLower().Trim() ? ("," + next.Zipcode) : string.Empty),
                                PreFlip = new HouseCharacteristics()
                                {
                                    Sum = prev.PreFlip.Sum + next.PreFlip.Sum,
                                    NumHouses = prev.PreFlip.NumHouses + next.PreFlip.NumHouses
                                },
                                PostFlip = new HouseCharacteristics()
                                {
                                    Sum = prev.PostFlip.Sum + next.PostFlip.Sum,
                                    NumHouses = prev.PostFlip.NumHouses + next.PostFlip.NumHouses
                                },
                                Aggregation = new List<FlippedHouse>() { prev, next }
                            };
                            totalData.PreFlip.Price = totalData.PreFlip.Sum / totalData.PreFlip.NumHouses;
                            totalData.PostFlip.Price = totalData.PostFlip.Sum / totalData.PostFlip.NumHouses;
                            commonSubDivHash.Add(commonSubDivName, totalData);
                        }
                    }
                }
            }
        }

        private static string CombineZips(FlippedHouse prev, FlippedHouse next)
        {
            var prevZip = prev.Zipcode;
            string[] zipList = prevZip.Split(',');
            if (zipList == null || zipList.Count() > 0)
            {
                prevZip = StripZip(prevZip);
            }
            
            var nextZip = next.Zipcode;
            nextZip = StripZip(nextZip).ToLower().Trim();

            var alreadyMatch = false;
            if (zipList != null && zipList.Count() > 0)
            {
                foreach (var thisZip in zipList)
                {
                    var tmp = StripZip(thisZip).ToLower().Trim();
                    if (tmp.ToLower().Trim().Equals(nextZip))
                    {
                        alreadyMatch = true;
                        break;
                    }
                }
            }

            var newZip = prevZip;
            if (!alreadyMatch)
            {
                newZip = prevZip.Trim() + ", " + nextZip.Trim();
            }

            return newZip;
        }

        private static string StripZip(string prevZip)
        {
            var colonIndex = prevZip.IndexOf(':');

            if (colonIndex >= 0)
            {
                prevZip = prevZip.Substring(0, colonIndex);
            }

            return prevZip;
        }

        private static string GetCommonSubDivName(string origPrevName, string origCurrentName)
        {
            origPrevName = origPrevName.Trim();
            origCurrentName = origCurrentName.Trim();
            var prevName = origPrevName.ToLower().Trim();
            var currentName = origCurrentName.ToLower().Trim();
            var commonName = string.Empty;
            var x = 0;
            for (; x < currentName.Length; x++)
            {
                if (x == prevName.Length || x == currentName.Length) { break; }
                var ch = prevName[x];
                var thisCh = currentName[x];
                if (ch == thisCh)
                {
                    //commonName += ch;
                    commonName += origPrevName[x];
                }
                else
                {
                    break;
                }
            }
            if (commonName.Length > 0)
            {
                //************************************************
                //??????? REMOVE
                //************************************************
                bool reject;
                if (commonName.Length < 0.5 * origPrevName.Length)
                {//old logic: i would reject common name here
                    //commonName = string.Empty;
                    reject = true;
                    commonName = commonName + "";
                }
                else
                {// old logic: i would return common name here
                    // I want to see if I get hits for subdivisions that match over 50% of their name
                    reject = false;
                    commonName = commonName + "";
                }
                //************************************************

                if (x == prevName.Length || x == currentName.Length) {
                    return commonName;
                }
                var remPrev = prevName.Substring(x).Trim();
                var remNext = currentName.Substring(x).Trim();

                double num1, num2;
                if (double.TryParse(remPrev, out num1) && double.TryParse(remNext, out num2)) {
                    return commonName;
                }

                var directions = new string[] { "north", "south", "east", "west" };
                bool prevMatch = false, nextMatch = false;
                foreach(var d in directions)
                {
                    if(!prevMatch && remPrev.Equals(d))
                    {
                        prevMatch = true;

                        if(nextMatch)
                        {
                            break;
                        }
                    }
                    if(!nextMatch && remNext.Equals(d))
                    {
                        nextMatch = true;

                        if(prevMatch)
                        {
                            break;
                        }
                    }
                }

                if(prevMatch && nextMatch) {
                    return commonName;
                }

                int start = 0;
                var a = start;
                for (; a < remPrev.Length; a++)
                {
                    var ch = remPrev[a];
                    if (!Char.IsNumber(ch)) // skip symbols, characters (non-numbers)
                    {
                        break;
                    }
                }

                bool prevBeginsWithNumbers;
                if (a == start)
                {
                    prevBeginsWithNumbers = false;
                }
                else
                {
                    prevBeginsWithNumbers = true;
                    // a represents the first index in which encountered a digit
                    //bool prevEndsWithNumbers = a<remPrev.Length && double.TryParse(remPrev.Substring(a), out num1);
                }

                // OK let's see if we begin with numbers
                var end = remPrev.Length - 1;
                var b = end;
                for (; b >= x; b--)
                {
                    var ch = remPrev[b];
                    if (!Char.IsNumber(ch)) // skip numbers
                    {
                        break;
                    }
                }

                bool prevEndsWithNumbers;
                // b+1...end should be numbers
                if (b == end)
                {
                    prevEndsWithNumbers = false;
                }
                else
                {
                    prevEndsWithNumbers = true;
                    //prevBeginsWithNumbers = b < (remPrev.Length - 1) && double.TryParse(remPrev.Substring(b + 1), out num1);
                }


                start = 0;
                a = start;
                for (; a < remNext.Length; a++)
                {
                    var ch = remNext[a];
                    if (!Char.IsNumber(ch))
                    {
                        break;
                    }
                }

                bool nextBeginsWithNumbers;
                if (a == start)
                {
                    nextBeginsWithNumbers = false;
                }
                else
                {
                    nextBeginsWithNumbers = true;

                    // a represents the first index in which encountered a digit
                    //bool nextEndsWithNumbers = a < remNext.Length && double.TryParse(remNext.Substring(a), out num1);
                }

                // OK let's see if we begin with numbers
                end = remNext.Length - 1;
                b = end;
                for (; b >= x; b--)
                {
                    var ch = remNext[b];
                    if (!Char.IsNumber(ch)) // skip numbers
                    {
                        break;
                    }
                }


                bool nextEndsWithNumbers;
                // b+1...end should be numbers
                if (b == end)
                {
                    nextEndsWithNumbers = false;
                }
                else
                {
                    nextEndsWithNumbers = true;
                    // a...b should be numbers
                    //bool nextBeginsWithNumbers = b < (remNext.Length - 1) && double.TryParse(remNext.Substring(b+1), out num1);
                }


                if (prevBeginsWithNumbers && nextBeginsWithNumbers) {
                    return commonName;
                }
                if (prevEndsWithNumbers && nextEndsWithNumbers) {
                    //old logic: (incorrect) accept if two strings have a number in the back
                    //return commonName;
                    commonName = commonName + "";
                }

                // I reject common name
                return string.Empty;                
            }
            return commonName;
        }

        private static Dictionary<string, FlippedHouse> DataRead(Dictionary<string, FlippedHouse> subDivResults,/* Dictionary<string, MlsRow> currentActiveHash,*/ Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> activeMultiKeyHash, string dataFolder)
        {
            Logger.Debug("Using data folder: {0}", dataFolder);
            var subDirs = Directory.GetDirectories(dataFolder);
            var files = Directory.GetFiles(dataFolder, "*.csv", SearchOption.AllDirectories);
            if (files == null || files.Count() == 0)
            {
                Logger.Debug("No files in data folder");
            }
            List<MlsRow> mlsRows = new List<MlsRow>();
            var soldHash = new Dictionary<string, MlsRow>();
            //var currentActiveHash = new Dictionary<string, MlsRow>();
            var flippedMultiKeyHash = new Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>>();
            //var activeMultiKeyHash = new Dictionary< string, Dictionary<string, Dictionary<string, List<MlsRow>>>>();
            ReadFiles(files, mlsRows, soldHash, /*currentActiveHash,*/ flippedMultiKeyHash, activeMultiKeyHash);
            var mlsData = new MlsData() { Data = mlsRows };
            var json = JsonConvert.SerializeObject(mlsData);
            FileIO.Write("mls.json", json);
            //Dictionary<string, FlippedHouse> subDivResults = new Dictionary<string, FlippedHouse>();
            return PrintZipSummary(subDivResults, flippedMultiKeyHash);
        }

        private static Dictionary<string, FlippedHouse> PrintZipSummary(Dictionary<string, FlippedHouse> subDivResults, Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> flippedMultiKeyHash)
        {
            var zipResults = new Dictionary<string, FlippedHouse>();
            Console.WriteLine("Number of zipcodes with flipped houses: {0}", flippedMultiKeyHash.Keys.Count);
            Logger.Border();
            var zipCount = 0;
            //Dictionary<string, FlippedHouse> subDivResults = new Dictionary<string, FlippedHouse>();
            foreach (var zip in flippedMultiKeyHash.Keys)
            {
                ++zipCount;
                PrintZip(subDivResults, flippedMultiKeyHash, zipResults, zipCount, zip);
            }

            return zipResults;
        }

        private static Dictionary<string, FlippedHouse> PrintZip(Dictionary<string, FlippedHouse> subDivResults, Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> flippedHash, Dictionary<string, FlippedHouse> zipResults, int zipCount, string zip)
        {
            var preFlip = new HouseCharacteristics() { Price = 0 };
            var postFlip = new HouseCharacteristics() { Price = 0 };
            var subDivHash = flippedHash[zip];
            double numSubDiv = subDivHash.Keys.Count;
            double zipHousesCount = 0;
            Console.WriteLine("#{0} Zip code: {1} has {2} subdivisions", zipCount, zip, numSubDiv);
            var subDivCount = 0;

            foreach (var subDiv in subDivHash.Keys)
            {
                ++subDivCount;
                zipHousesCount += PrintSubDivision(subDivResults, zip, preFlip, postFlip, subDivHash, subDivCount, subDiv);
            }
            Console.WriteLine("#{0} Zip code: {1} has {2} subdivisions {3} houses in total", zipCount, zip, numSubDiv, zipHousesCount);
            PrintAverageFlipInZip(zipResults, zip, preFlip, postFlip, zipHousesCount);
            return subDivResults;
        }

        private static void PrintAverageFlipInZip(Dictionary<string, FlippedHouse> results, string zip, HouseCharacteristics preFlip, HouseCharacteristics postFlip, double numHouses)
        {
            ComputeAverageFlip(results, zip, preFlip, postFlip, numHouses);

            Logger.Border();
            Console.WriteLine("Zipcode Average price prior to flip: ${0}", ToString(preFlip.Price));
            Console.WriteLine("Zipcode Average price after flip: ${0}", ToString(postFlip.Price));
            Console.WriteLine("Net Profit: ${0}", ToString(postFlip.Price - preFlip.Price));
            Logger.Border();
        }

        private static FlippedHouse ComputeAverageFlip(Dictionary<string, FlippedHouse> results, string zip, HouseCharacteristics preFlip, HouseCharacteristics postFlip, double numHouses)
        {
            FlippedHouse fh = null;
            preFlip.Sum = preFlip.Price;
            preFlip.NumHouses = numHouses;
            postFlip.Sum = postFlip.Price;
            postFlip.NumHouses = numHouses;
            preFlip.Price = preFlip.Price / numHouses;
            postFlip.Price = postFlip.Price / numHouses;
            results.Add(zip, fh = new FlippedHouse(zip, preFlip, postFlip)/* { PreFlip = preFlip, PostFlip = postFlip }*/);
            return fh;
        }

        private static int PrintSubDivision(
            Dictionary<string, FlippedHouse> subDivResults,
            string zip,
            HouseCharacteristics preFlip,
            HouseCharacteristics postFlip,
            Dictionary<string, Dictionary<string, List<MlsRow>>> subDivHash,
            int subDivCount,
            string subDiv)
        {
            var houseHash = subDivHash[subDiv];
            var numHouses = houseHash.Keys.Count;
            Console.WriteLine("#{0} Subdivision: {1} => {2} flipped houses", subDivCount, subDiv, numHouses);
            var houseCount = 0;
            HouseCharacteristics preSubDivFlip = new HouseCharacteristics() { Price = 0 };
            HouseCharacteristics postSubDivFlip = new HouseCharacteristics() { Price = 0 };
            var city = string.Empty;
            foreach (var houseId in houseHash.Keys)
            {
                ++houseCount;
                Logger.Border();
                List<MlsRow> soldRecords = GetSortedSoldRecords(houseHash, houseId);
                MlsRow lastSold, firstSold;
                FindFirstLastSold(soldRecords, out lastSold, out firstSold);
                UpdateSoldRecords(soldRecords, lastSold, firstSold);

                double firstPrice, lastPrice;
                AddToSum(preFlip, postFlip, preSubDivFlip, postSubDivFlip, lastSold, firstSold, out firstPrice, out lastPrice);
                double profit = DetermineProfit(firstPrice, lastPrice);
                PrintHouseFlip(zip, houseCount, lastSold, firstSold, profit);
                city = lastSold.City;
            }
            var fh = ComputeAverageFlip(subDivResults, zip + "::" + subDiv, preSubDivFlip, postSubDivFlip, numHouses);
            fh.SubDivision = subDiv;
            fh.City = city;
            Logger.Border();
            Console.WriteLine("Subdivision Average price prior to flip: ${0}", ToString(preSubDivFlip.Price));
            Console.WriteLine("Subdivision Average price after flip: ${0}", ToString(postSubDivFlip.Price));
            Console.WriteLine("Net Profit: ${0}", ToString(postSubDivFlip.Price - preSubDivFlip.Price));
            Logger.Border();
            return numHouses;
        }

        private static void PrintHouseFlip(string zip, int count, MlsRow lastSold, MlsRow firstSold, double profit)
        {
            var homeAddress = firstSold.Address + " " + firstSold.City + " " + firstSold.PostalCode;
            Console.WriteLine("{0}) {1}: {2} in zip {3} sold for {4} in {5}, sold again for {6} in {7}, Profit: ${8}", count, firstSold.MLNumber, homeAddress, zip, firstSold.CurrentPrice, firstSold.CloseDate, lastSold.CurrentPrice, lastSold.CloseDate, ToString(profit));
        }

        private static double DetermineProfit(double firstPrice, double lastPrice)
        {
            return lastPrice - firstPrice;
        }

        private static void AddToSum(
            HouseCharacteristics preFlip,
            HouseCharacteristics postFlip,
            HouseCharacteristics preSubDivFlip,
            HouseCharacteristics postSubDivFlip,
            MlsRow lastSold,
            MlsRow firstSold,
            out double firstPrice,
            out double lastPrice)
        {
            firstPrice = firstSold.CurrentPriceValue();
            lastPrice = lastSold.CurrentPriceValue();
            preFlip.Price += firstPrice;
            preSubDivFlip.Price += firstPrice;
            postFlip.Price += lastPrice;
            postSubDivFlip.Price += lastPrice;
        }

        private static void UpdateSoldRecords(List<MlsRow> soldRecords, MlsRow lastSold, MlsRow firstSold)
        {
            soldRecords.Clear();
            soldRecords.Add(firstSold);
            soldRecords.Add(lastSold);
        }

        private static void FindFirstLastSold(List<MlsRow> soldRecords, out MlsRow lastSold, out MlsRow firstSold)
        {
            lastSold = soldRecords[soldRecords.Count - 1];
            firstSold = null;
            for (var k = soldRecords.Count - 2; k >= 0; k--)
            {
                var prevSold = soldRecords[k];
                if (lastSold.CloseDateValue() < prevSold.CloseDateValue())
                {
                    throw new InvalidOperationException();
                }
                var time = lastSold.CloseDateValue().Subtract(prevSold.CloseDateValue());
                var valid = time <= TimeSpan.FromDays(366);
                if (valid)
                {
                    firstSold = prevSold;
                }
            }
        }

        private static List<MlsRow> GetSortedSoldRecords(Dictionary<string, List<MlsRow>> zipHash, string houseId)
        {
            var soldRecords = zipHash[houseId];
            soldRecords.Sort();
            return soldRecords;
        }

        private static void ReadFiles(
            string[] files,
            List<MlsRow> mlsRows,
            Dictionary<string, MlsRow> soldHash,
            //Dictionary<string, MlsRow> currentActiveHash,
            Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> flippedMultiKeyHash,
            Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> activeMultiKeyHash)
        {
            foreach (var file in files)
            {
                FileProcess(mlsRows, soldHash, /*currentActiveHash,*/ flippedMultiKeyHash, activeMultiKeyHash, file);
            }
        }

        private static void FileProcess(
            List<MlsRow> mlsRows,
            Dictionary<string, MlsRow> soldHash,
            //Dictionary<string, MlsRow> currentActiveHash,
            Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> flippedMultiKeyHash,
            Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> activeMultiKeyHash,
            string file)
        {
            bool isFirstLine = true;

            string[] colNames = null;
            var rowNum = 0;
            foreach (var line in FileIO.ReadFrom(file))
            {
                ++rowNum;
                string[] fields = GetFields(line);
                if (isFirstLine)
                {
                    isFirstLine = false;
                    colNames = GetHeaderColumns(fields);
                }
                else
                {
                    AggregateByZipCode(mlsRows, soldHash, /*currentActiveHash,*/ flippedMultiKeyHash, activeMultiKeyHash, colNames, fields);
                }
            }
            if (rowNum == 0)
            {
                Logger.Debug("Empty file: '{0}'", file);
            }
            else if (rowNum == 1)
            {
                Logger.Debug("Header row found, but no data rows exist in file:'{0}'", file);
            }
        }

        private static string[] GetFields(string line)
        {
            return line.Split(new string[] { "\",\"" }, StringSplitOptions.None);
        }

        private static string[] GetHeaderColumns(string[] fields)
        {
            var colNames = new string[fields.Length];
            for (var j = 0; j < fields.Length; j++)
            {
                var field = fields[j].Replace("\"", string.Empty);
                colNames[j] = field;
            }
            return colNames;
        }

        private static void AggregateByZipCode(
            List<MlsRow> mlsRows,            
            Dictionary<string, MlsRow> soldHash,
            //Dictionary<string, MlsRow> currentActiveHash,
            Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> flippedMultiKeyHash,
            Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> activeMultiKeyHash,
            string[] colNames,
            string[] fields)
        {
            MlsRow record = AddRecord(mlsRows, colNames, fields);
            AggregateBySold(soldHash, flippedMultiKeyHash, record);
            AggregateByActive(activeMultiKeyHash, /*currentActiveHash,*/ record);
        }

        

        private static MlsRow AddRecord(List<MlsRow> mlsRows, string[] colNames, string[] fields)
        {
            var data = new StringDictionary();
            for (var j = 0; j < fields.Length; j++)
            {
                var field = fields[j].Replace("\"", string.Empty);
                data.Add(colNames[j], field);
            }
            var record = new MlsRow(data);
            mlsRows.Add(record);
            return record;
        }

        private static void AggregateBySold(Dictionary<string, MlsRow> soldHash, Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> flippedMultiKeyHash, MlsRow record)
        {
            if (Sold(record))
            {
                string houseID = PropertyAddress(record);
                if (Contains(soldHash, houseID))
                {
                    AggregateFlips(soldHash, flippedMultiKeyHash, record, houseID);
                }
                else
                {
                    AddFirstSold(soldHash, record, houseID);
                }
            }
        }
                                                                 //          zip =>           subDiv=>   activeList
        private static void AggregateByActive(Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> activeMultiKeyHash, /*Dictionary<string, MlsRow> currentActiveHash,*/ MlsRow record)
        {
            if(Active(record))
            {
                var zip = ZipCode(record);
                var subDiv = SubDivision(record);
                string houseID = PropertyAddress(record);
                AddActive(activeMultiKeyHash, /*currentActiveHash,*/ zip, subDiv, houseID, record);
            }
        }

        private static void AddActive(
            Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> activeMultiKeyHash,
            //Dictionary<string, MlsRow> currentActiveHash,
            string zip, 
            string subDiv, 
            string houseID, 
            MlsRow record)
        {
            zip = zip.ToLower().Trim();
            subDiv = subDiv.ToLower().Trim();
            houseID = houseID.ToLower().Trim();

            Dictionary<string, Dictionary<string, List<MlsRow>>> subDivHash;
            if (activeMultiKeyHash.ContainsKey(zip))
            {
                subDivHash = activeMultiKeyHash[zip];
            }
            else
            {
                subDivHash = new Dictionary<string, Dictionary<string, List<MlsRow>>>();
                activeMultiKeyHash.Add(zip, subDivHash);
            }

            Dictionary<string, List<MlsRow>> houseHash;
            if (subDivHash.ContainsKey(subDiv))
            {
                houseHash = subDivHash[subDiv];
            }
            else
            {
                houseHash = new Dictionary<string, List<MlsRow>>();
                subDivHash.Add(subDiv, houseHash);
            }

            List<MlsRow> houseList;
            if (houseHash.ContainsKey(houseID))
            {
                houseList = houseHash[subDiv];
                throw new InvalidOperationException("Error: Not sure why  2 active records would exist for the same house id");
            }
            else
            {
                houseList = new List<MlsRow>();
                houseHash.Add(houseID, houseList);
            }

            houseList.Add(record);

            /*
            if(currentActiveHash.ContainsKey(houseID))
            {
                throw new InvalidOperationException("Error: Not sure why  2 active records would exist for the same house id");
            }
            else
            {
                currentActiveHash.Add(houseID, record);
            }*/
        }

        private static string SubDivision(MlsRow record)
        {
            return record.LegalSubdivisionName;
        }

        private static void AddFirstSold(Dictionary<string, MlsRow> soldHash, MlsRow record, string houseID)
        {
            soldHash.Add(houseID, record);
        }

        private static void AggregateFlips(
            //          zip,              subdiv,           houseId,  soldList
            Dictionary<string, MlsRow> soldHash, Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> flippedMultiKeyHash, MlsRow record, string houseID)
        {
            //         subDiv,            house,   soldList
            Dictionary<string, Dictionary<string, List<MlsRow>>> subDivHash = AddSubDivsion(flippedMultiKeyHash, ZipCode(record));
            //         house,      
            Dictionary<string, List<MlsRow>> houseFlippedHash = AddHouseHash(record, subDivHash);
            AddFlippedHouse(soldHash, record, houseID, houseFlippedHash);
        }

        private static void AddFlippedHouse(Dictionary<string, MlsRow> soldHash, MlsRow record, string houseID, Dictionary<string, List<MlsRow>> houseFlippedHash)
        {
            if (houseFlippedHash.ContainsKey(houseID))
            {
                var soldRecords = houseFlippedHash[houseID];
                soldRecords.Add(record);
            }
            else
            {
                houseFlippedHash.Add(houseID, new List<MlsRow>() { soldHash[houseID], record });
            }
        }

        private static Dictionary<string, List<MlsRow>> AddHouseHash(MlsRow record, Dictionary<string, Dictionary<string, List<MlsRow>>> subDivHash)
        {
            return GetHouseHash(subDivHash, SubDivision(record));
        }

        private static Dictionary<string, List<MlsRow>> GetHouseHash(Dictionary<string, Dictionary<string, List<MlsRow>>> subDivHash, string subDiv)
        {
            Dictionary<string, List<MlsRow>> houseHash;
            if (subDivHash.ContainsKey(subDiv))
            {
                houseHash = subDivHash[subDiv];
            }
            else
            {
                houseHash = new Dictionary<string, List<MlsRow>>();
                subDivHash.Add(subDiv, houseHash);
            }

            return houseHash;
        }

        private static void AddZipHash(Dictionary<string, MlsRow> soldHash, MlsRow record, string houseID, Dictionary<string, List<MlsRow>> zipHash)
        {
            if (zipHash.ContainsKey(houseID))
            {
                var soldRecords = zipHash[houseID];
                soldRecords.Add(record);
            }
            else
            {
                zipHash.Add(houseID, new List<MlsRow>() { soldHash[houseID], record });
            }
        }

        private static Dictionary<string, Dictionary<string, List<MlsRow>>> AddSubDivsion(
            Dictionary<string, Dictionary<string, Dictionary<string, List<MlsRow>>>> flippedMultiKeyHash, string zip)
        {
            Dictionary<string, Dictionary<string, List<MlsRow>>> subDivHash;
            if (flippedMultiKeyHash.ContainsKey(zip))
            {
                subDivHash = flippedMultiKeyHash[zip];
            }
            else
            {
                subDivHash = new Dictionary<string, Dictionary<string, List<MlsRow>>>();
                flippedMultiKeyHash.Add(zip, subDivHash);
            }

            return subDivHash;
        }

        private static string ZipCode(MlsRow record)
        {
            return record.PostalCode;
        }

        private static bool Contains(Dictionary<string, MlsRow> soldHash, string houseID)
        {
            return soldHash.ContainsKey(houseID);
        }

        private static string PropertyAddress(MlsRow record)
        {
            var temp = record.Address.Trim() + record.City.Trim() + record.PostalCode.Trim();
            var houseID = temp.ToLower();
            return houseID;
        }

        private static bool Sold(MlsRow record)
        {
            return record.StatusValue == MlsStatus.Sold;
        }

        private static bool Active(MlsRow record)
        {
            return record.StatusValue == MlsStatus.Active;
        }


        private static string ToString(double num)
        {
            var str = num.ToString();
            var index = str.IndexOf('.');
            var f = "";
            if (index >= 0)
            {
                f = str.Substring(index);
                str = str.Substring(0, index);
            }
            var k = 0;
            for (int j = str.Length - 1; j >= 0; j--)
            {
                if (k > 0 && k % 3 == 0)
                {
                    f = "," + f;
                }
                var d = str[j];
                f = d + f;
                ++k;
            }
            return f;
        }

        public class FlippedHouse : IComparable
        {
            public FlippedHouse(string zip, HouseCharacteristics preFlip, HouseCharacteristics postFlip)
            {
                Zipcode = zip;
                PreFlip = preFlip;
                PostFlip = postFlip;
            }
            public FlippedHouse() { }

            public string Zipcode { get; set; }
            public HouseCharacteristics PreFlip { get; set; }
            public HouseCharacteristics PostFlip { get; set; }

            public double Profit
            {
                get
                {
                    var firstPrice = PreFlip.Price;
                    var lastPrice = PostFlip.Price;
                    var profit = lastPrice - firstPrice;
                    return profit;
                }
            }

            public string SubDivision { get; set; }
            public string City { get; set; }
            public List<FlippedHouse> Aggregation { get; internal set; }

            public int CompareTo(object obj)
            {
                var other = obj as FlippedHouse;
                if (other == null) { return 1; }
                return this.Profit.CompareTo(other.Profit);
            }
        }

        public class HouseCharacteristics
        {
            public HouseCharacteristics() { this.Price = 0; }
            public double Sum { get; set; }
            public double NumHouses { get; set; }
            public double Price { get; set; }
        }

        private static string GetDefaultFolder()
        {
            var exeLoc = typeof(Program).Assembly.Location;
            var projDir = exeLoc;
            while (!projDir.EndsWith("bin"))
            {
                projDir = Path.GetDirectoryName(projDir);
            }
            projDir = Path.GetDirectoryName(projDir);
            var dataFolder = Path.Combine(projDir, dataRelFolder);
            return dataFolder;
        }
    }
}
