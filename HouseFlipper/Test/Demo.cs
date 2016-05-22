using Common;
using Common.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;
using Test.Harness;

namespace HouseFlipper.Test
{
    public class Demo : Driver
    {
        private TestInput Input;
        private string DataFolderPath;

        public override Result Run()
        {
            Program p = new Program(this.DataFolderPath);
            p.Timeout = Input.Timeout;
            Result r = p.Run();
            return r;
        }

        public override void SetUp(Input input)
        {
            this.Input = (TestInput)input;
            FolderPath folderPath = new DataFolderFactory().Create(Input.DataFolder);
            if (folderPath == null) { DataFolderPath = null; }
            else {
                DataFolderPath = folderPath.Quoted;
            }

            var files = Input.Files;
            new FilesFactory().Create(folderPath, files);

            var filesCount = Input.FileCount;
            new FilesCountFactory().Create(folderPath, filesCount);

            var headerCount = Input.HeaderCount;
            new HeaderCountFactory().Create(folderPath, headerCount);

            var dataRowCount = Input.DataRowCount;
            new DataRowCountFactory().Create(dataRowCount);
        }
    }
}
