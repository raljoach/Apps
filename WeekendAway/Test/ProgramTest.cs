using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WeekendAway;
using System.Collections.Generic;

namespace Test
{
    [TestClass]
    public class ProgramTest
    {
        [TestMethod]
        public void GetDestinationsTest()
        {
            string file = @"C:\Users\ralph.joachim\Documents\Visual Studio 2015\Projects\WeekendAway\visit.json";
            var visit = Program.GetDestinations(file);
            Assert.IsNotNull(visit);
            Assert.IsNotNull(visit.Start);
            Assert.IsNotNull(visit.Start.Id);
            Assert.IsNotNull(visit.Places);
            Assert.IsTrue(visit.Places.Count > 0);

            var found = false;
            var hash = new HashSet<int>();
            foreach(var p in visit.Places)
            {
                Assert.IsNotNull(p.Id);
                if(p.Id==visit.Start.Id)
                {
                    found = true;
                    break;
                }
                Assert.IsFalse(string.IsNullOrWhiteSpace(p.Name));
                Assert.IsTrue(hash.Add(p.Id));
            }
            Assert.IsTrue(found);
        }
    }
}
