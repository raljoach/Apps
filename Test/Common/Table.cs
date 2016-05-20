using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common
{
    public abstract class Table
    {
        public List<TableColumn> Columns;
        public List<TableRow> Rows;
    }

    public abstract class TableColumn
    {

    }

    public abstract class TableRow
    {
        public List<TableField> Columns;
        public List<TableField> GetColumns(int start, int secondToLast)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class TableField
    {

    }
}
