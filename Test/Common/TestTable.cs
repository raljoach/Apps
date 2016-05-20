using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common
{
    public class TestTable : Dictionary<Input, ExpectedResult>
    {
        public TestTable() { }

        public TestTable(Table table)
        {
            int start = 0, last = table.Columns.Count-1, secondToLast = last - 1;
            foreach(var row in table.Rows)
            {
                Input input = Input.Create(row.GetColumns(start, secondToLast));
                ExpectedResult er = ExpectedResult.Create(row.Columns[last]);
                this.Add(input, er);
            }
        }
    }
}
