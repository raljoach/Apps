using Common;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolkit;

namespace WeekendAway
{                
    public partial class Program
    {        
        static void Main(string[] args)
        {
            new Prototype2();            
            Logger.Debug("Program ended. Hit any key to exit....");
            Console.ReadKey();
        }        
    }    
}
