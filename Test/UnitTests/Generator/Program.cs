using Common;
using Common.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{
    class Program
    {
        static string file = "oo.txt";
        static bool createSubFolder = true;
        static void Main(string[] args)
        {
            var result = Read();
            var rootNameSpace = result.NameSpace;
            Dictionary<string, List<NounVerbDetail>> nvdHash = result.nvdHash;
            var outDir = @".\out";
            
var usingStmts =
@"using NUnit.Framework;
";
//{0} rootNameSpace
var nameSpaceTemp =
@"namespace {0}UnitTests";
var nameSpaceBegin= 
@"
{
";

// {0} noun
var className = TrimFirstLine(
@"
    [TestFixture]
    public class {0}Test 
");

var classBegin = TrimFirstLine(
@"
    {
");

// {0} verb_detail
var methodSigTemp = TrimFirstLine(
@"
        [Test]
        public void {0}()
");

var methodImpl = TrimFirstLine(
@"
        {
           Assert.Ignore();
        }
");

var classEnd = TrimFirstLine(
@"
    }
");

var nameSpaceEnd = TrimFirstLine(
@"
}
");
            foreach (var noun in nvdHash.Keys)
            {
                string methods = string.Empty;
                foreach (var nvd in nvdHash[noun])
                {
                    var methodName = nvd.verb;
                    if (!string.IsNullOrEmpty(nvd.detail))
                    {
                        methodName += "_" + nvd.detail;
                        var methodBody =
                        string.Format(methodSigTemp, methodName) +
                        methodImpl;
                        methods += methodBody;
                    }
                }
                string nameSpace = string.Format(nameSpaceTemp, string.IsNullOrWhiteSpace(rootNameSpace) ? string.Empty : rootNameSpace + ".");
                var code =
                usingStmts +
                nameSpace+
                nameSpaceBegin +
                string.Format(className, noun) +
                classBegin +
                methods +
                classEnd +
                nameSpaceEnd;

                if (createSubFolder)
                {
                    var subFolder = nameSpace.Replace("namespace ",string.Empty);
                    while (subFolder.Contains("."))
                    {
                        var index = subFolder.IndexOf('.');
                        var tmp = subFolder.Substring(0, index);
                        outDir = Path.Combine(outDir, tmp);
                        if (!Directory.Exists(outDir))
                        {
                            Directory.CreateDirectory(outDir);
                        }
                        subFolder = subFolder.Substring(index+1);
                    }
                    outDir = Path.Combine(outDir, subFolder);
                }
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }

                var csFile = string.Format("{0}Test.cs", noun);
                var path = Path.Combine(outDir, csFile);
                FileIO.Write(path, code);
                Logger.Debug(path);
                Logger.Border();
                Logger.Explicit(code);
            }
            Logger.Border();
            Logger.Debug("Program has ended. Hit any key to exit!");
            Console.ReadKey();
        }

        private static string TrimFirstLine(string str)
        {
            var index = str.IndexOf('\n');
            str = str.Substring(index + 1);
            return str;
        }

        public class ReadResult
        {
            public string NameSpace;
            public Dictionary<string, List<NounVerbDetail>> nvdHash;

            public ReadResult(string nameSpace, Dictionary<string, List<NounVerbDetail>> nvdHash)
            {
                this.NameSpace = nameSpace;
                this.nvdHash = nvdHash;
            }
        }

        private static ReadResult Read()
        {
            Logger.Debug("Reading {0}....", file);
            Logger.Border();
            var skipSection = false;
            var inDetailsSection = true;
            string noun = null;
            string verb = null;
            var nvdHash = new Dictionary<string, List<NounVerbDetail>>(StringComparer.OrdinalIgnoreCase);
            var lineCount = 0;
            var created = new List<NounVerbDetail>();
            string nameSpace = null;
            foreach (var l in FileIO.ReadFrom(file))
            {
                ++lineCount;
                var line = l.Trim();
                if (string.IsNullOrWhiteSpace(line)) { continue; }
                if (line.ToUpper().StartsWith("NAMESPACE:"))
                {
                    var index = line.IndexOf(':');
                    nameSpace = line.Substring(index + 1);
                    continue;
                }
                else if (IsSectionSeparator(line))
                {
                    //COMPLETED current section
                    //Entering New section
                    noun = null;
                    verb = null;

                    if (skipSection) { skipSection = false; }
                    if (inDetailsSection) { inDetailsSection = false; }
                    continue;
                }
                else if (IsLineComment(ref line))
                {
                    var comment = line.Replace(" ", string.Empty).ToUpper();
                    if (comment.StartsWith("TESTCLASS:SKIP"))
                    {
                        skipSection = true;
                    }
                    continue;
                }
                else if (IsPrimaryNoun(ref line) || IsNoun(ref line))
                {
                    noun = line;
                }
                else if (IsVerb(ref line))
                {
                    verb = line;
                }
                else if (IsDetails(line))
                {
                    inDetailsSection = true;
                }
                else if (inDetailsSection)
                {
                    if (!skipSection)
                    {
                        var tmp = line.ToUpper().Replace(" ", string.Empty);
                        if (tmp.StartsWith("-D:"))
                        {
                            var index = line.IndexOf(":");
                            var detail = line.Substring(index + 1);
                            List<NounVerbDetail> list;
                            if (nvdHash.ContainsKey(noun))
                            {
                                list = nvdHash[noun];
                            }
                            else
                            {
                                list = new List<NounVerbDetail>();
                                nvdHash.Add(noun, list);
                            }
                            list.Add(new NounVerbDetail(noun, verb, detail));
                        }
                    }
                }
            }

            return new ReadResult(nameSpace,nvdHash);
        }

        private static bool IsDetails(string line)
        {
            var result = false;
            var tmp = line.Replace(" ", string.Empty).ToUpper();
            result = tmp.StartsWith("DETAILS:");
            return result;
        }

        private static bool IsNoun(ref string line)
        {
            var key = "NOUN:";
            return HandleNoun(ref line, key);
        }

        private static bool IsVerb(ref string line)
        {
            var key = "VERB:";
            var tag = "V:";
            return HandleProperty(ref line, key, tag);
        }

        private static bool HandleNoun(ref string line, string key)
        {
            var tag = "X:";
            return HandleProperty(ref line, key, tag);
        }

        private static bool HandleProperty(ref string line, string key, string tag)
        {
            var result = false;
            var tmp = line.ToUpper().Replace(" ", string.Empty);
            if (result = tmp.StartsWith(key))
            {
                var index = line.IndexOf(':');
                line = line.Substring(index + 1).Trim();
                if (line.ToUpper().StartsWith(tag))
                {
                    line = line.Remove(0, 2).Trim();
                }
            }
            return result;
        }

        private static bool IsPrimaryNoun(ref string line)
        {
            var key = "PRIMARYNOUN:";
            return HandleNoun(ref line, key);
        }

        private static bool IsLineComment(ref string line)
        {
            var result = line.StartsWith("//");
            if (result)
            {
                line = line.Remove(0, 2);
            }
            return result;
        }

        private static bool IsSectionSeparator(string line)
        {
            var isDashedLine = false;
            var tmp = line.Replace("-", string.Empty);
            if (string.IsNullOrWhiteSpace(tmp))
            {
                isDashedLine = true;
            }

            return isDashedLine;
        }
    }
}
