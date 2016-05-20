using Common;
using System;

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
