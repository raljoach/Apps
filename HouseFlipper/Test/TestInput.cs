using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;

namespace HouseFlipper.Test
{
    public class TestInput : Input
    {
        public DataFolder DataFolder;
        public TimeSpan Timeout;
        public Files Files;
        public FileCount FileCount;        
        public HeaderCount HeaderCount;
        public DataRowCount DataRowCount;

        public TimeSpan DefaultTimeout { get { return TimeSpan.FromSeconds(1); } }

        public TestInput(DataFolder datafolder, Files files = Files.UnusedParameter, FileCount fileCount = FileCount.UnusedParameter, HeaderCount headerCount = HeaderCount.UnusedParameter, DataRowCount dataRowCount = DataRowCount.UnusedParameter)
        {
            this.Timeout = DefaultTimeout;
            this.DataFolder = datafolder;
            this.Files = files;
            this.FileCount = fileCount;            
            this.HeaderCount = headerCount;
            this.DataRowCount = dataRowCount;
        }

        public override string ToString()
        {
            string output = "\r\n";
            output += "\r\n";
            var cols = new List<Column>();
            cols.Add(new Column(@"\data", this.DataFolder));
            cols.Add(new Column(@"\files", this.Files));
            cols.Add(new Column(@"\fileCount", this.FileCount));
            cols.Add(new Column(@"\headerCount", this.HeaderCount));
            cols.Add(new Column(@"\dataRowCount", this.DataRowCount));

            var grid = new Grid(cols);
            output += grid.ToString();
            output += "\r\n";
            return output;
        }
    }
}
