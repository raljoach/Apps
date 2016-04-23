﻿using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeekendAway
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
            Console.WriteLine(message, args);
        }
    }
}
