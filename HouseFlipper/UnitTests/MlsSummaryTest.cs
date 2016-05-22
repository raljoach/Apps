using Common.IO;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace  HouseFlipper.UnitTests
{
	public partial class SUT : Oracle
	{// USES SUT for Real!!
		public override Dictionary<string,AverageResult> Average(List<PropertyInfo> properties)
		{
             throw new NotImplementedException();
		}
	}
	public partial class Oracle
	{// ORACLE
		public virtual Dictionary<string,AverageResult> Average(List<PropertyInfo> properties)
		{
			/*
			string postalCode = null;
		    double priceBefore=0, priceAfter=0;
		    double sqftBefore=0, sqftAfter=0;
		    double bedsBefore=0, bedsAfter=0;
		    double bathsBefore=0, bathsAfter=0;	
		    double count=0;		
			foreach(var p in properties)
			{
				++count;
				priceBefore+=p.BeforePrice; priceAfter+=p.AfterPrice;
				sqftBefore+=p.BeforeSqft; sqftAfter+=p.AfterSqft;
		        bedsBefore+=p.BeforeBeds; bedsAfter+=p.AfterBeds;
		        bathsBefore+=p.BeforeBaths; bathsAfter+=p.AfterBaths;		
			}

			double BeforePriceAvg=priceBefore/count, AfterPriceeAvg=priceAfter/count;
		    double BeforeSqfteAvg=sqftBefore/count, AfterSqfteAvg=sqftAfter/count;
		    double BeforeBedseAvg=bedsBefore/count, AfterBedseAvg=bedsAfter/count;
		    double BeforeBathseAvg=bathsBefore/count, AfterBathseAvg=bathsAfter/count;

		    return new AverageResult(
		    	 postalCode,
                 BeforePriceAvg, AfterPriceeAvg,
		         BeforeSqfteAvg, AfterSqfteAvg,
		         BeforeBedseAvg, AfterBedseAvg,
		         BeforeBathseAvg, AfterBathseAvg
		    	);
		   */
		    var hash = new Dictionary<string,AverageResult>();
		  	foreach(var p in properties)
			{
				var postalCode = p.PostalCode;
		    	AverageResult avgRes = null;
		    	if(hash.ContainsKey(postalCode)) {
		    		avgRes = hash[postalCode];		    		
		    	} 	
		    	else {
		    		avgRes = new AverageResult(postalCode) { Count=0 };
		    		hash.Add(postalCode,avgRes);
		        } 
		        avgRes.Count++; Console.WriteLine("{0} Count: {1}",postalCode,avgRes.Count);
		        avgRes.BeforePrice+=p.BeforePrice; avgRes.AfterPrice+=p.AfterPrice;
				avgRes.BeforeSqft+=p.BeforeSqft; avgRes.AfterSqft+=p.AfterSqft;
		        avgRes.BeforeBeds+=p.BeforeBeds; avgRes.AfterBeds+=p.AfterBeds;
		        avgRes.BeforeBaths+=p.BeforeBaths; avgRes.AfterBaths+=p.AfterBaths;	
			}

            foreach(AverageResult avgRes in hash.Values)
            {
			    avgRes.BeforePrice/=avgRes.Count; avgRes.AfterPrice/=avgRes.Count;
			    avgRes.BeforeSqft/=avgRes.Count; avgRes.AfterSqft/=avgRes.Count;
			    avgRes.BeforeBeds/=avgRes.Count; avgRes.AfterBeds/=avgRes.Count;
			    avgRes.BeforeBaths/=avgRes.Count; avgRes.AfterBaths/=avgRes.Count;
			}
		    return hash;
		}
	}
	public class PropertyInfo
	{
		public int Id; 
		public string PostalCode;
		public double BeforePrice;
		public double AfterPrice;
		public double BeforeMonth;
		public double AfterMonth;
        public double BeforeYear;
		public double AfterYear;


		public double BeforeSqft;
		public double AfterSqft;
		public double BeforeBeds;
		public double AfterBeds;

		public double BeforeBaths;
		public double AfterBaths;
	}
	public class AverageResult
	{
		public AverageResult(string postalCode){PostalCode = postalCode;}
		public AverageResult(
			   string postalCode,
               double beforePrice, double afterPrice,
		       double beforeSqft, double afterSqft,
		       double beforeBeds, double afterBeds,
		       double beforeBaths, double afterBaths
			) : this(postalCode)
		{
			   
               BeforePrice=beforePrice; AfterPrice=afterPrice;
		       BeforeSqft=beforeSqft; AfterSqft=afterSqft;
		       BeforeBeds=beforeBeds; AfterBeds=afterBeds;
		       BeforeBaths=beforeBaths; AfterBaths=afterBaths;
		}
		public string PostalCode;
		public double BeforePrice, AfterPrice;
		public double BeforeSqft, AfterSqft;
		public double BeforeBeds, AfterBeds;
		public double BeforeBaths, AfterBaths;
		public double Count;

		public override bool Equals(object obj)
		{
			var ar = obj as AverageResult;
			if(ar==null) { return false; }
			return
				this.PostalCode == ar.PostalCode &&
				this.BeforePrice == ar.BeforePrice &&
				this.BeforeSqft==ar.BeforeSqft &&
				this.BeforeBeds == ar.BeforeBeds &&
				this.BeforeBaths == ar.BeforeBaths &&

				this.AfterPrice == ar.AfterPrice &&
				this.AfterSqft==ar.AfterSqft &&
				this.AfterBeds == ar.AfterBeds &&
				this.AfterBaths == ar.AfterBaths &&
				this.Count == ar.Count;
		}

		public override string ToString()
		{

			return string.Format( @"
			PostalCode: {0}
			BeforePrice: {1}
			BeforeSqft: {2}
			BeforeBeds: {3}
			BeforeBaths: {4}

			AfterPrice:	{5}
			AfterSqft:	{6}
			AfterBeds:	{7}
			AfterBaths:	{8}
			Count: {9}",
			this.PostalCode,
			this.BeforePrice,
				this.BeforeSqft,
				this.BeforeBeds,
				this.BeforeBaths,

				this.AfterPrice,
				this.AfterSqft,
				this.AfterBeds,
				this.AfterBaths,
				this.Count
			);
		}
	}
    [TestFixture]
    public partial class MlsSummaryTest 
    {
    	private static Oracle sut = new Oracle();
        [Test]
        public void SoldAverage___Example()
        {
           var expectedHash = new Dictionary<string,AverageResult>();
           var props = new List<PropertyInfo>();

           var postalCode = string.Empty;
           var postalCount = new Dictionary<string,double>();
           //var expectedOutputSection = false;
           foreach(var l in FileIO.ReadFrom(@"data\soldaverage_example.txt"))
           {
           	   var line = l.Trim();
           	   var upper = line.ToUpper();
           	   var upperNoSpace = upper.Replace(" ",string.Empty);
           	   if(upperNoSpace.StartsWith("HOUSE#"))
           	   {
           	   	   var prop = ExtractPropInfo(line);
                   props.Add(prop);
                   if(postalCount.ContainsKey(prop.PostalCode))
                   {
                   	 postalCount[prop.PostalCode]++;
                   }
                   else {
                     postalCount.Add(prop.PostalCode,1);
                   }
           	   }
           	   else if(upperNoSpace.StartsWith("ZIPCODE:"))
           	   {
           	   		var index = line.IndexOf(':');
           	   		postalCode = line.Substring(index+1).Trim();
           	   	    //expectedOutputSection = true;

           	   	    var expected = new AverageResult(postalCode);
           	   	    expectedHash.Add(postalCode,expected);
           	   	    if(postalCount.ContainsKey(postalCode))
           	   	    {
           	   	      expected.Count = postalCount[postalCode];
           	   	    }           	   	    
           	   }
           	   else if(upperNoSpace.StartsWith("AVERAGEPRICEPRIORTOFLIP:"))
           	   {
           	   		var index = line.IndexOf(':');
           	   		var priceBefore = line.Substring(index+1).Trim();
           	   		var expected = expectedHash[postalCode];
           	   		expected.BeforePrice = ConvertPrice(priceBefore);
           	   		Console.WriteLine("FOUND#1:"+ expected.BeforePrice);
           	   }
           	   else if(upperNoSpace.StartsWith("AVERAGEPRICEAFTERFLIP:"))
           	   {
           	   		var index = line.IndexOf(':');
           	   		var priceAfter = line.Substring(index+1).Trim();
           	   		var expected = expectedHash[postalCode];
           	   		expected.AfterPrice = ConvertPrice(priceAfter);
           	   		Console.WriteLine("FOUND#2:"+expected.AfterPrice);
           	   }
           }
           
           var hash = sut.Average(props);
           foreach(var k in expectedHash.Keys)
           {
           	   postalCode = k;
           	   Console.WriteLine("For postal code: '{0}'",postalCode);

           	   var expected = expectedHash[k];
           	   var result = hash[k];
           	   
	           Console.WriteLine("Expected: \r\n {0}",expected);
	           Console.WriteLine("Actual: \r\n {0}",result);
	           Assert.AreEqual(expected, result);
	           Console.WriteLine("(PASSED)");
	           Console.WriteLine("--------------------------------------------------");
       	   }
           
        }
        private static PropertyInfo ExtractPropInfo(string line)
        {
        	var index = line.IndexOf('#');        	
        	var str = line.Substring(index+1).Trim();
        	index = str.IndexOf(' ');
        	var id = int.Parse(str.Substring(0,index));
            index = str.IndexOf("zip ");
            str = str.Substring(index + "zip ".Length);
            index = str.IndexOf(' ');
            var postalCode = str.Substring(0,index);
            str = str.Substring(index+1);   //sold for $60,0000 [CONFIRMED_1]

            var marker = "sold for ";
            if(!str.StartsWith(marker)) { 
            	var msg = string.Format("Error#1: '{0}' does not start with '{1}'",str,marker);    
            	Console.WriteLine(msg);        	
            	throw new InvalidOperationException(msg);
            }
            Console.WriteLine(str); //[CONFIRMED_1]
            index = str.IndexOf(marker);
            str = str.Substring(index + marker.Length);  //$60,000 in January 2016
            index = str.IndexOf(' ');
            var priceBefore  = string.Empty;
            if(index>=0)
            {
              priceBefore = str.Substring(0,index);
              str = str.Substring(index+1);  //in January 2016 [CONFIRMED_2]
            }
            else
            {
            	priceBefore = str;
            	if(priceBefore.EndsWith("."))
            	{
            		priceBefore.Remove(priceBefore.Length-1);
            	}

            	return new PropertyInfo()
	            {
	            	Id = id,
			        PostalCode = postalCode,
			        BeforePrice = ConvertPrice(priceBefore)			        
	            };
            }

            marker = "in ";
            if(!str.StartsWith(marker)) { 
            	var msg = string.Format("Error#2: '{0}' does not start with '{1}'",str,marker);    
            	Console.WriteLine(msg);        	
            	throw new InvalidOperationException(msg);
            }
            Console.WriteLine(str); //[CONFIRMED_2]
            index = str.IndexOf(marker);
            str = str.Substring(index + marker.Length);  //January 2016, sold again
            index = str.IndexOf(' ');
            var monthBefore = str.Substring(0,index);
            str = str.Substring(index+1);  //2016, sold again for [CONFIRMED_3]

            marker = ",";
            /*if(!str.StartsWith(marker)) { 
            	var msg = string.Format("Error#3: '{0}' does not start with '{1}'",str,marker);    
            	Console.WriteLine(msg);        	
            	throw new InvalidOperationException(msg);
            }*/
            Console.WriteLine(str); //[CONFIRMED_3]
            index = str.IndexOf(marker);
            var yearBefore = str.Substring(0,index);
            str = str.Substring(index+1).Trim(); //sold again for $120,000 in May 2016 [CONFIRMED_4]

            marker = "sold again for";
            if(!str.StartsWith(marker)) { 
            	var msg = string.Format("Error#4: '{0}' does not start with '{1}'",str,marker);    
            	Console.WriteLine(msg);        	
            	throw new InvalidOperationException(msg);
            }
            Console.WriteLine(str); //[CONFIRMED_4]
            index = str.IndexOf(marker);
            str = str.Substring(index + marker.Length).Trim();  //$120,000 in May 2016
            index = str.IndexOf(' ');
            var priceAfter = str.Substring(0,index);
            str = str.Substring(index+1);  //in May 2016  [CONFIRMED_5]

            marker = "in ";
            if(!str.StartsWith(marker)) { 
            	var msg = string.Format("Error#5: '{0}' does not start with '{1}'",str,marker);    ///[BLOW UP!!!!!]
            	Console.WriteLine(msg);        	
            	throw new InvalidOperationException(msg);
            }
            Console.WriteLine(str); // [CONFIRMED_5]
            index = str.IndexOf(marker);
            str = str.Substring(index + marker.Length);  //May 2016
            index = str.IndexOf(' ');
            var monthAfter = str.Substring(0,index);
            var yearAfter = str.Substring(index+1);  //2016
            Console.WriteLine("--------------------------------");
            return new PropertyInfo()
            {
            	Id = id,
		        PostalCode = postalCode,
		        BeforePrice = ConvertPrice(priceBefore),
		        AfterPrice=ConvertPrice(priceAfter),
		        BeforeMonth=ConvertMonth(monthBefore),
		        AfterMonth=ConvertMonth(monthAfter),
                BeforeYear=double.Parse(yearBefore),
		        AfterYear=double.Parse(yearAfter)
            };

        }

        private static double ConvertMonth(string month)
        {
        	var tmp = month.ToLower();
        	double count = 0;
            foreach(var m in CultureInfo.CurrentCulture.DateTimeFormat.MonthNames)
            {
            	++count;
            	var thisMonth = m.ToLower();
            	if(tmp==thisMonth.ToLower() || thisMonth.StartsWith(tmp))
            	{
            		return count;
            	}
            }
        	throw new InvalidOperationException();
        }

        private static double ConvertPrice(string priceStr)
        {
        	var tmp = priceStr.Replace("$",string.Empty).Replace(",",string.Empty);
        	var price = double.Parse(tmp);
        	return price;
        }
        [Test]
        public void SoldAverage___Simple()
        {
           Assert.Ignore();
        }
        [Test]
        public void SoldAverage__Error()
        {
           Assert.Ignore();
        }        
        [Test]
        public void SoldAverage_Boundary()
        {
           Assert.Ignore();
        }
        [Test]
        public void SoldAverage_Large()
        {
           Assert.Ignore();
        }
        [Test]
        public void SoldAverage_Unique()
        {
           Assert.Ignore();
        }
    }

    public partial class SUT
    {
    	public override SoldCountResult SoldCount(List<Listing> listings)
    	{
    		throw new NotImplementedException();
    	}
    }
    public partial class Oracle
	{
		public virtual SoldCountResult SoldCount(List<Listing> listings)
		{
			var soldHash = new Dictionary<HouseId,List<Listing>>();
			foreach(var prop in listings)
			{
				if(prop.Status.ToLower()=="sld")
				{
					List<Listing> countSold = null;
					if(soldHash.ContainsKey(prop.HouseId))
					{
						countSold = soldHash[prop.HouseId];
					}
					else
					{
						countSold = new List<Listing>();
						soldHash.Add(prop.HouseId,countSold);
				    }
				    countSold.Add(prop);
				}
			}

            var result = new Dictionary<HouseId,double>();
			foreach(var k in soldHash.Keys)
			{
				double count = soldHash[k].Count;
				result.Add(k,count);
			}
			return new SoldCountResult(result);
		}
	}
	public class HouseId
	{
		public string Value;
	}
	public class SoldCountResult
	{
		public SoldCountResult(Dictionary<HouseId,double> result)
		{
			this.Result = result;
		}
		public Dictionary<HouseId,double> Result;
	}
	public class Listing 
	{
		public string Status;
		public HouseId HouseId {get;}
	}

    [TestFixture]
    public partial class MlsSummaryTest 
    {
    	[Test]
        public void SoldCount___Simple()
        {
           Assert.Ignore();
        }
        [Test]
        public void SoldCount__Error()
        {
           Assert.Ignore();
        }        
        [Test]
        public void SoldCount_Boundary()
        {
           Assert.Ignore();
        }
        [Test]
        public void SoldCount_Large()
        {
           Assert.Ignore();
        }
        [Test]
        public void SoldCount_Unique()
        {
           Assert.Ignore();
        }
    }
}