using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KazmonWatcher.Test.Common;
using KazmonWatcher.Test.Common.Parameters;
using System.Collections.Specialized;
using Test.Common;
using Test.Harness;
using System.IO;
using Common.IO;

namespace KazmonWatcher.Test
{
    public class Demo : Driver
    {
        private TestInput Input;
        private StringDictionary ArgumentsDictionary = new StringDictionary();
        private string ExecutablePath;

        public Demo()
        {
            this.ExecutablePath = Path.Combine(DirectoryIO.GetProjectDir(), @"..\..\KazmonWatcher\bin\debug\KazmonWatcher.exe");
        }

        public override void SetUp(Input input)
        {
            this.Input = (TestInput)input;
            switch (Input.KazmonUrl)
            {
                case KazmonUrl.UnusedParameter:
                    break;
                default:
                    throw new InvalidOperationException(string.Format(@"Error: Unhandled \kazmonUrl type '{0}'", Input.KazmonUrl));
            }
        }

        public override Result Run()
        {            
            var p = new Prototype(ExecutablePath, ArgumentsDictionary);
            Result r = p.Run();
            return r;
        }
    }
}
