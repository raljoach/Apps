using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Demo
    {
        private Input input;
        private string DataFolderPath;

        public Demo(Input input)
        {
            DataFolderPath = string.Empty;
            switch(input.DataFolder)
            {
                case DataFolder.NotExistent:
                    var folder = Path.Combine(DirectoryIO.GetProjectDir(),"foo");
                    if(Directory.Exists(folder))
                    {
                        Directory.Delete(folder);
                    }
                    DataFolderPath = folder;
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Error: Unhandled data folder type {0}", input.DataFolder));
            }


        }
        
        public Result Run()
        {
            Prototype p = new Prototype(this.DataFolderPath);
            Result r = p.Run();
            return r;
        }

        
    }
}
