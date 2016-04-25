using Common;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolkit
{
    //Class PhantomJS driver extensions, additional methods, decorator
    //Name: PhantomJSDriverExt
    public partial class Program
    {
        private static PhantomJSDriver InitPhantomJS()
        {

            PhantomJSDriverService service = PhantomJSDriverService.CreateDefaultService();
            service.LogFile = @".\phantom.log";
            service.SuppressInitialDiagnosticInformation = true;
            int firstRow = Console.CursorTop;
            //var tmp = Console.Out;
            //Console.SetOut(new StringWriter(new StringBuilder()));
            PhantomJSDriver driver = new PhantomJSDriver(service);
            //Console.SetOut(tmp);
            ClearConsole(firstRow);
            return driver;
        }

        private static void ClearConsole(int firstRow)
        {
            int lastRow = Console.CursorTop;
            int count = lastRow - firstRow + 1;
            for (int i = 0; i < count; i++)
            //while(Console.CursorTop>=stop)
            {
                // Start at beginning of current row
                Console.SetCursorPosition(0, Console.CursorTop - i);

                // Blank out/Clear current row
                Console.Write(new string(' ', Console.BufferWidth));
                //Console.Write(new string(' ', Console.WindowWidth));


                Console.SetCursorPosition(0, lastRow);
            }
            Console.SetCursorPosition(0, Console.CursorTop - count + 1);
        }
    }

    //Class: Logger
    public partial class Program
    {
        private static void Debug(string message, params object[] args)
        {
            Logger.Debug(message, args);
        }

        private static void DebugWrite(string message, params object[] args)
        {
            Logger.DebugWrite(message, args);
        }
        private static void Debug()
        {
            Logger.Debug();
        }

        private static void Debug(Dictionary<string, string> table)
        {
            foreach (var key in table.Keys)
            {
                Debug("{0} => {1}", key, table[key]);
            }
        }
    }

    //Class: Input I/O
    public partial class Program
    {
        private static IEnumerable<string> ReadFrom(string file)
        {
            return FileIO.ReadFrom(file);
        }

        private static string ReadAll(string file)
        {
            return FileIO.ReadAll(file);
        }
    }

    public class Suggestion
    {        
        public Suggestion(Place place)
        {
            this.Place = place;
        }

        public Place Place { get; set; }
        public string SearchUrl { get; set; }
        public int SearchUrlId { get; set; }
    }
}