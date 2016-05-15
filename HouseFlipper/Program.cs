using Common;
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
            string dataFolder = GetDataFolder();
            var subDirs = Directory.GetDirectories(dataFolder);
            var files = Directory.GetFiles(dataFolder, "*.csv", SearchOption.AllDirectories);
            List<MlsRow> mlsRows = new List<MlsRow>();
            var soldHash = new Dictionary<string, MlsRow>();
            var flippedHash = new Dictionary<string, Dictionary<string, List<MlsRow>>>();
            foreach (var f in files)
            {
                bool isFirstLine = true;

                string[] colNames = null;
                var rowNum = 0;
                foreach (var line in FileIO.ReadFrom(f))
                {
                    ++rowNum;
                    var cols = line.Split(new string[] { "\",\"" }, StringSplitOptions.None);
                    if (isFirstLine)
                    {
                        isFirstLine = false;                        
                        var data = new StringDictionary();
                        colNames = new string[cols.Length];
                        for (var j = 0; j < cols.Length; j++)
                        {
                            var field = cols[j].Replace("\"", string.Empty);
                            colNames[j] = field;
                        }                                             
                    }
                    else
                    {
                        var data = new StringDictionary();
                        for (var j = 0; j < cols.Length; j++)
                        {
                            var field = cols[j].Replace("\"",string.Empty);
                            data.Add(colNames[j], field);
                        }
                        var mlsRow = new MlsRow(data);
                        mlsRows.Add(mlsRow);
                        if (mlsRow.StatusValue == MlsStatus.Sold)
                        {
                            //var houseID = mlsRow.MLNumber.Trim().ToLower();
                            var temp = mlsRow.Address.Trim() + mlsRow.City.Trim() + mlsRow.PostalCode.Trim();
                            var houseID = temp.ToLower();
                            if (soldHash.ContainsKey(houseID))
                            {
                                var zip = mlsRow.PostalCode;
                                Dictionary<string, List<MlsRow>> zipHash;
                                if(flippedHash.ContainsKey(zip))
                                {
                                    zipHash = flippedHash[zip];
                                }
                                else
                                {
                                    zipHash = new Dictionary<string, List<MlsRow>>();
                                    flippedHash.Add(zip, zipHash);
                                }
                                if(zipHash.ContainsKey(houseID))
                                {
                                    var soldRecords = zipHash[houseID];
                                    soldRecords.Add(mlsRow);
                                }
                                else
                                {
                                    zipHash.Add(houseID, new List<MlsRow>() { soldHash[houseID], mlsRow });
                                }
                            }
                            else
                            {
                                soldHash.Add(houseID, mlsRow);
                            }                            
                        }
                    }
                }
            }
            var mlsData = new MlsData() { Data = mlsRows };
            var json = JsonConvert.SerializeObject(mlsData);
            FileIO.Write("mls.json", json);

            var results = new Dictionary<string, FlippedHouse>();
            Console.WriteLine("Number of zipcodes with flipped houses: {0}", flippedHash.Keys.Count);
            Logger.Border();
            var zipCount = 0;
            foreach(var zip in flippedHash.Keys)
            {
                ++zipCount;                
                var preFlip = new HouseCharacteristics();
                var postFlip = new HouseCharacteristics();
                var zipHash = flippedHash[zip];
                double numHouses = zipHash.Keys.Count;
             
                Console.WriteLine("#{0} Zip code: {1} => {2} flipped houses", zipCount, zip, numHouses);
                var count = 0;
                foreach (var houseId in zipHash.Keys)
                {
                    ++count;
                    Logger.Border();
                    var soldRecords = zipHash[houseId];
                    soldRecords.Sort();
                    var lastSold = soldRecords[soldRecords.Count - 1];
                    MlsRow firstSold = null;
                    for(var k=soldRecords.Count-2; k>=0; k--)
                    {
                        var prevSold = soldRecords[k];
                        if (lastSold.CloseDateValue() < prevSold.CloseDateValue()) {
                            throw new InvalidOperationException();
                        }
                        var time = lastSold.CloseDateValue().Subtract(prevSold.CloseDateValue());
                        var valid = time <= TimeSpan.FromDays(366);
                        if (valid)
                        {
                            firstSold = prevSold;
                        }
                    }

                    soldRecords.Clear();
                    soldRecords.Add(firstSold);
                    soldRecords.Add(lastSold);
                    var homeAddress = firstSold.Address + " " + firstSold.City + " " + firstSold.PostalCode;
                    var firstPrice = firstSold.CurrentPriceValue();
                    var lastPrice = lastSold.CurrentPriceValue();
                    preFlip.Price += firstPrice;
                    postFlip.Price += lastPrice;
                    var profit = lastPrice - firstPrice;
                    Console.WriteLine("{0}) {1}: {2} in zip {3} sold for {4} in {5}, sold again for {6} in {7}, Profit: ${8}", count, firstSold.MLNumber, homeAddress, zip, firstSold.CurrentPrice, firstSold.CloseDate, lastSold.CurrentPrice, lastSold.CloseDate, ToString(profit));
                    
                }

                preFlip.Price = preFlip.Price / numHouses;
                postFlip.Price = postFlip.Price / numHouses;
                results.Add(zip, new FlippedHouse(zip,preFlip,postFlip)/* { PreFlip = preFlip, PostFlip = postFlip }*/);

                Logger.Border();
                Console.WriteLine("Average price prior to flip: ${0}", ToString(preFlip.Price));
                Console.WriteLine("Average price after flip: ${0}", ToString(postFlip.Price));
                Console.WriteLine("Net Profit: ${0}", ToString(postFlip.Price - preFlip.Price));
                Logger.Border();
            }
            var list = results.Values.ToList();
            list.Sort();
            Console.WriteLine("Zipcodes ordered by profitability:");
            Logger.Border();
            for(int k=0; k<list.Count; k++)
            {
                var r = list[k];
                Console.WriteLine("{0}) {1} | ${2}", k+1, r.Zipcode, ToString(r.Profit));
            }
            Logger.Border();
            Console.WriteLine();
            Console.WriteLine("Program has ended. Hit any key to exit.");
            Console.ReadKey();
        }

        private static string ToString(double num)
        {           
            var str = num.ToString();
            var index = str.IndexOf('.');
            var f = "";
            if(index>=0)
            {
                f = str.Substring(index);
                str = str.Substring(0, index);
            }
            var k = 0;
            for(int j=str.Length-1; j>=0; j--)
            {              
                if(k>0 && k%3==0)
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

            public int CompareTo(object obj)
            {
                var other = obj as FlippedHouse;
                if(other==null) { return 1; }
                return this.Profit.CompareTo(other.Profit);
            }
        }

        public class HouseCharacteristics
        {
            public HouseCharacteristics() { this.Price = 0; }
            public double Price { get; set; }
        }

        private static string GetDataFolder()
        {
            var exeLoc = Environment.CurrentDirectory;
            var projDir = exeLoc;
            while (!projDir.EndsWith("bin"))
            {
                projDir = Path.GetDirectoryName(exeLoc);
            }
            projDir = Path.GetDirectoryName(projDir);
            var dataFolder = Path.Combine(projDir, dataRelFolder);
            return dataFolder;
        }
    }
}
