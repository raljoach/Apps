using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.ModelBased;

namespace HouseFlipper.Test.Toolkit
{
    class Program
    {
        static void Main(string[] args)
        {
            var group = new ParameterGroup(typeof(DataFolder),typeof(FileCount), typeof(HeaderCount), typeof(DataRowCount));
        }
    }
}
