using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;

namespace Test.ModelBased
{
    public class ParameterModel
    {
        public ParameterGroup Parameters;
        public ParameterModel(ParameterGroup parameters)
        {
            this.Parameters = parameters;
        }

        public Table Generate()
        {
            // TODO: Create a SymbolicValue table of ParameterInfo rows
            // TODO: Make each cell of table, a ParameterInfo with an actual ParameterFactoryValue instance (if possible)
            throw new NotImplementedException();
        }
    }
}
