using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseFlipper.PrototypeCode
{
    public class MlsDetails
    {

    }
    public class MlsSummary
    {
        public MlsSummary(string[] mlsFiles)
        {
            foreach (var mlsFile in mlsFiles)
            {
                foreach (var mlsRecord in FileIO.ReadFrom(mlsFile))
                {
                    var mlsDetails = GetDetails(mlsRecord);
                }
            }

        }

        private MlsDetails GetDetails(string mlsRecord)
        {
            throw new NotImplementedException();
        }
    }
    public class Prototype
    {
        public static void _Main(string[] args)
        {
            //var summary = new MlsSummary(files);
        }
    }
}
