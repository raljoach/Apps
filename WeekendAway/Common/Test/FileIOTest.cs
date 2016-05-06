using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common;
using System.IO;

namespace Test
{
    [TestClass]
    public class FileIOTest
    {
        [TestMethod]
        public void ReadAllError()
        {
            if (File.Exists("foo"))
            {
                File.Delete("foo");
            }
            var exThrown = false;
            try
            {
                FileIO.ReadAll("foo");
            }
            catch(InvalidOperationException)
            {
                exThrown = true;
            }
            Assert.IsTrue(exThrown, "Error: Nonexistent file should have thrown exception");
        }
        
        [TestMethod]
        public void ReadAllEmpty()
        {
            if(File.Exists("foo"))
            {
                File.Delete("foo");
            }
            using (var sw = new StreamWriter(new FileStream("foo", FileMode.CreateNew, FileAccess.Write)))
            {
            }
            var str = FileIO.ReadAll("foo");
            Assert.IsTrue(string.IsNullOrEmpty(str), "Error: ReadAll from Empty file should have returned null or empty string!");
        } 
    }
}
