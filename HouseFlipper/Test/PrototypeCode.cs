using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using Common.IO;
using Test.Common;
using Automata;
using Test.ModelBased;

namespace HouseFlipper.Test.ProtoTypeCode
{
    public class Result
    {
        public string Error;
        public string Output;

        public Result(string output, string error)
        {
            Output = output;
            Error = error;
        }
    }

    public class Program
    {
        private string exePath;
        private string arguments;
        public Program(string exePath, string arguments)
        {
            this.exePath = exePath;
            this.arguments = arguments;
        }

        public TimeSpan Timeout { get; set; }
        public string Arguments { get { return arguments; } set { arguments = value; } }

        public Result Run()
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(exePath, arguments);
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.UseShellExecute = false;
            var started = p.Start();
            if (!started)
            {
                throw new InvalidOperationException(string.Format("Process did not start '{0}'", exePath));
            }
            p.WaitForExit((int)this.Timeout.TotalMilliseconds);
            string error = null;
            string output = null;
            if (!p.HasExited)
            {
                //error = "Program stuck runnning!\r\n";                
                p.Kill();
                error += FileIO.ReadAll(p.StandardError);
                output = FileIO.ReadAll(p.StandardOutput);
            }
            else
            {
                error = FileIO.ReadAll(p.StandardError);
                output = FileIO.ReadAll(p.StandardOutput);
            }
            return new Result(output, error);
        }

        public void SetArguments(List<Parameter> parameters)
        {
            var arguments = string.Empty;
            var isFirst = true;
            foreach (var parameter in parameters)
            {
                if (isFirst) { isFirst = false; }
                else
                {
                    arguments += " ";
                }
                var argName = parameter.Name;
                var argVal = parameter.Value;
                var argStr = string.Format("-{0} {1}", argName, argVal);
                arguments += argStr;
            }
            this.Arguments = arguments;
        }
        public void SetCreateArguments(List<IParameter> parameters)
        {
            var arguments = string.Empty;
            var isFirst = true;
            foreach (Parameter parameter in parameters)
            {
                if (isFirst) { isFirst = false; }
                else
                {
                    arguments += " ";
                }
                var argName = parameter.Name;
                var argVal = parameter.Value;
                var argStr = string.Format("-{0} {1}", argName, argVal);
                arguments += argStr;
            }
            this.Arguments = arguments;
        }
    }
    public class Parameter : IParameter
    {
        private Enum enumVal;
        public Parameter(Enum enumVal)
        {
            this.enumVal = enumVal;
            this.Name = enumVal.GetType().Name;
        }
        public Parameter(Parameter copy)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> AllValues { get; internal set; }
        public Enum EnumValue { get { return enumVal; } set { enumVal = value; } }
        public string Name { get; private set; }

        public string Value
        {
            set
            {
                enumVal = (Enum)Enum.Parse(enumVal.GetType(), value, true);
            }
            get
            {
                return this.enumVal.ToString();
            }
        }
    }
    public class ParameterModel
    {
        ParameterGroup group;
        public ParameterModel(ParameterGroup group)
        {
            this.group = group;
        }

        public ParameterTable CreateTable()
        {
            string pictExe = @"C:\Program Files (x86)\PICT\pict.exe";
            string inputFile = CreatePictFile();
            var pict = new Program(pictExe, inputFile);
            var output = pict.Run().Output;
            var rows = output.Split('\n');
            return new ParameterTable(group, rows);
        }

        private string CreatePictFile()
        {
            string file = "input.txt";
            List<string> specs = new List<string>();
            foreach (var parameter in group)
            {
                string spec = string.Format("{0}: ", parameter.Name);
                bool isFirst = true;
                foreach (string val in parameter.AllValues)
                {
                    if (isFirst) { isFirst = false; }
                    else
                    {
                        spec += ", ";
                    }
                    spec += val;
                }

                specs.Add(spec);
            }

            foreach (var spec in specs)
            {
                FileIO.Append(file, spec);
            }
            return file;
        }
    }
    public class ParameterTable : IEnumerable<List<Parameter>>
    {
        private ParameterGroup signature;
        private List<List<Parameter>> rows;
        public ParameterTable(ParameterGroup signature, string[] rows)
        {
            this.signature = signature;
            this.rows = Parse(rows, this.signature);
        }
        public IEnumerable<List<Parameter>> GetRows()
        {
            return rows;
        }
        private static List<List<Parameter>> Parse(string[] list, ParameterGroup signature)
        {
            var table = new List<List<Parameter>>();
            foreach (var r in list)
            {
                var row = new List<Parameter>();
                var valTokens = r.Split('\t', ' ');
                for (var k = 0; k < valTokens.Length; k++)
                {
                    var val = valTokens[k];
                    var parameter = new Parameter(signature[k]);
                    parameter.Value = val;
                    row.Add(parameter);
                }
                table.Add(row);
            }

            return table;
        }

        public IEnumerator<List<Parameter>> GetEnumerator()
        {
            return ((IEnumerable<List<Parameter>>)rows).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<List<Parameter>>)rows).GetEnumerator();
        }
    }
    public class ParameterGroup : IEnumerable<Parameter>
    {
        private Enum[] enums;
        private List<Parameter> parameters;

        public ParameterGroup(params Type[] enumTypes)
        {
            enums = Convert(enumTypes);
            for (var k = 0; k < enums.Length; k++)
            {
                parameters.Add(new Parameter(enums[k]));
            }
        }

        public Parameter this[int index]
        {
            get
            {
                return new Parameter(enums[index]);
            }
        }

        public Parameter this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<Parameter> Parameters
        {
            get
            {
                return parameters;
            }
        }

        private static Enum[] Convert(Type[] enumTypes)
        {
            Enum[] enums = new Enum[enumTypes.Length];
            if (enumTypes == null || enumTypes.Length == 0) { throw new InvalidOperationException("Error: One or more Enum types must be specified"); }
            for (var k = 0; k < enumTypes.Length; k++)
            {
                var t = enumTypes[k];
                if (!t.IsEnum) { throw new InvalidOperationException(string.Format("Error: '{0}' is not an Enum type", t.Name)); }
                //var defaultVal = ((Enum[])t.GetEnumValues())[0];
                //enums[k] = defaultVal;

                var defaultVal = (Enum)t.GetFields().First().GetValue(null);
                enums[k] = defaultVal;
            }

            return enums;
        }

        public IEnumerator<Parameter> GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Parameters.GetEnumerator();
        }
    }
    public interface IMethodTester
    {
        //IEnumerable<string> Methods { get; }
        ParameterGroup GetSignature(string method);
        void Run(string method, List<Parameter> args);

    }
    public interface IParameterTester
    {
        ParameterGroup GetParameters();

        void Run(List<Parameter> args);
    }

    public class ParameterHarness
    {
        public void Run(IMethodTester tester, string method)
        {
            var signature = tester.GetSignature(method);
            var table = CreateModel(signature);
            foreach (var args in table)
            {
                tester.Run(method, args);
            }
        }

        public void Run(IParameterTester tester)
        {
            var parameters = tester.GetParameters();
            var table = CreateModel(parameters);
            foreach (var args in table)
            {
                tester.Run(args);
            }
        }

        private ParameterTable CreateModel(ParameterGroup signature)
        {
            var model = new ParameterModel(signature);
            var table = model.CreateTable();
            return table;
        }
    }

    public class Test : IEnumerable<TestStep>
    {
        private List<TestStep> steps;
        public Test(string testName, TestInput input, ExpectedResult result)
        {
            this.TestName = testName;
            steps = new List<TestStep>();
            var singleStep = new TestStep();
            steps.Add(singleStep);
            singleStep.Arguments = input.Arguments;
            singleStep.Result = result;
        }
        public Test(params TestStep[] steps)
        {
            if (steps == null || steps.Length == 0) { throw new ArgumentException("Error: Steps cannot be null or empty!"); }
            this.steps = new List<TestStep>();
            this.steps.AddRange(steps);
        }
        public IEnumerable<TestStep> TestSteps { get { return steps; } }

        public string TestName { get; private set; }

        public IEnumerator<TestStep> GetEnumerator()
        {
            return TestSteps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return TestSteps.GetEnumerator();
        }
    }

    public class TestStep
    {
        public string Description { get; set; }
        public Action Action { get; set; }
        public List<Parameter> Arguments { get; set; }
        public ExpectedResult Result { get; set; }
    }

    public class TestInput : Input
    {
        private List<Enum> input;

        public List<Parameter> Arguments
        {
            get
            {
                var args = new List<Parameter>();
                foreach (var enumVal in input)
                {
                    args.Add(new Parameter(enumVal));
                }
                return args;
            }
            set
            {
                this.input = new List<Enum>();
                foreach (var parameter in value)
                {
                    this.input.Add(parameter.EnumValue);
                }
            }
        }

        public TestInput(params Enum[] input)
        {
            if (input == null || input.Length == 0) { throw new ArgumentException("Error: Input cannot be empty or null!"); }
            this.input = new List<Enum>();
            this.input.AddRange(input);
        }
    }

    public interface IRegressionTester
    {
        IEnumerable<Test> GetTests();
        void Run(Test test);
    }
    public class RegressionHarness
    {
        private IRegressionTester tester;
        public RegressionHarness(IRegressionTester tester)
        {
            this.tester = tester;
        }
        public void Run()
        {
            var tests = tester.GetTests();
            foreach (var test in tests)
            {
                tester.Run(test);
            }
        }
    }

    public interface IStateBasedTester
    {
        TestMachine StateMachine { get; }
    }

    public class TestStateMachine : StateMachine
    {
        public TestStateMachine(TestState[] states) : base(states)
        {
            this.StateChanged += HandleStateChange;
        }
        private void HandleStateChange(State newState)
        {
            throw new InvalidOperationException();
        }
    }

    public class StateBasedHarness
    {
        private IStateBasedTester tester;
        public StateBasedHarness(IStateBasedTester tester)
        {
            this.tester = tester;
        }
        public void Run()
        {
            TestMachine stateMachine = tester.StateMachine;
            stateMachine.Run();
        }
    }

    public class TestTable : IEnumerable<Test>
    {
        private List<Test> tests;
        public TestTable(params Test[] tests)
        {
            if (tests == null || tests.Length == 0) { throw new ArgumentException("Error: Tests cannot be null or empty!"); }
            this.tests = new List<Test>();
            this.tests.AddRange(tests);
        }

        public IEnumerable<Test> Tests { get { return tests; } }

        public IEnumerator<Test> GetEnumerator()
        {
            return Tests.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Tests.GetEnumerator();
        }
    }

    public interface IUnitTestDriver
    {

    }

    public interface IUnitTester
    {

    }

    public class UnitTestHarness
    {
        private IUnitTester tester;

        public UnitTestHarness(IUnitTester tester)
        {
            this.tester = tester;
        }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }

    public interface IExploratoryTester
    {

    }
    public class ExploratoryHarness
    {
        private Tester tester;

        public ExploratoryHarness(Tester tester)
        {
            this.tester = tester;
        }

        internal void Run()
        {
            throw new NotImplementedException();
        }
    }
    public interface IChaosActionTester
    {

    }
    public class ChaosActionHarness
    {
        private IChaosActionTester tester;

        public ChaosActionHarness(IChaosActionTester tester)
        {
            this.tester = tester;
        }

        internal void Run()
        {
            throw new NotImplementedException();
        }
    }

    public interface IChaosStateTester
    {

    }
    public class ChaosStateHarness
    {
        private IChaosStateTester tester;

        public ChaosStateHarness(IChaosStateTester tester)
        {
            this.tester = tester;
        }

        internal void Run()
        {
            throw new NotImplementedException();
        }
    }

    public interface IChaosSystemTester
    {

    }
    public class ChaosSystemHarness
    {
        private IChaosSystemTester tester;

        public ChaosSystemHarness(IChaosSystemTester tester)
        {
            this.tester = tester;
        }

        internal void Run()
        {
            throw new NotImplementedException();
        }
    }

    public class TestException : Exception
    {
        public TestException(string exMsg) : base(exMsg) { }
        public TestException(string exMsg, params object[] args) : base(string.Format(exMsg, args))
        {

        }
    }
    public class Tester : IParameterTester, IRegressionTester, IStateBasedTester, ITransitionDriver, IUnitTester, IUnitTestDriver, IExploratoryTester, IChaosActionTester, IChaosStateTester, IChaosSystemTester
    {
        private Program program;

        public Tester(Program program)
        {
            this.program = program;
            try
            {
                RunIt();
                BeatItUp();
                AnalyzeIt();
                UnitTestIt();
                Explore();
                Chaos();
            }
            catch (TestException te)
            {
                Logger.Debug(te.Message);
            }
        }
        private void RunIt()
        {
            //no data setup
            var response = program.Run();
            if (!string.IsNullOrWhiteSpace(response.Error))
            {
                throw new TestException(string.Format("Error: HouseFlipper.exe failed during run - '{0}'", response.Error));
            }
        }

        private void BeatItUp()
        {
            new ParameterHarness().Run(this);
        }

        private void AnalyzeIt()
        {
            new RegressionHarness(this).Run();
            new StateBasedHarness(this).Run();
        }

        private void UnitTestIt()
        {
            new UnitTestHarness(this).Run();
        }

        private void Explore()
        {
            new ExploratoryHarness(this).Run();
        }

        private void Chaos()
        {
            new ChaosActionHarness(this).Run();
            new ChaosStateHarness(this).Run();
            new ChaosSystemHarness(this).Run();
        }

        ParameterGroup IParameterTester.GetParameters()
        {
            return new ParameterGroup(typeof(DataFolder), typeof(Files), typeof(FileCount), typeof(HeaderCount), typeof(DataRowCount));
        }

        void IParameterTester.Run(List<Parameter> parameters)
        {
            DataSetup(parameters.ToArray());
            var response = program.Run();
            if (!string.IsNullOrWhiteSpace(response.Error))
            {
                throw new TestException(string.Format("Error: HouseFlipper.exe failed during run - '{0}'", response.Error));
            }
        }        

        IEnumerable<Test> IRegressionTester.GetTests()
        {
            var tests = DefineTests();
            foreach (var test in tests)
            {
                yield return test;
            }
        }

        void IRegressionTester.Run(Test test)
        {
            var stepCount = 0;
            foreach (var step in test)
            {
                ++stepCount;
                Result result = null;
                step.Action =
                    () =>
                    {
                        DataSetup(step.Arguments.ToArray());                        
                        result = program.Run();
                    };
                Logger.Debug("{0}. {1}", step);
                step.Action.Invoke();
                if (!step.Result.Equals(result))
                {
                    throw new TestException("{0} Test Failed: Expected {1}, Actual {2}", test.TestName, step.Result, result);
                }
            }
        }

        private TestTable DefineTests()
        {
            string testName = "HouseFlipper";
            var table = new TestTable(
                new Test(testName, new TestInput(DataFolder.Default), new ExpectedResult(new PartialMatch(@"Using data folder: C:\Users\ralph.joachim\Documents\Visual Studio 2015\Projects\HouseFlipper\data"))),
                new Test(testName, new TestInput(DataFolder.NotExistent), new ExpectedResult(new PartialMatch("Missing data folder"))),
                new Test(testName, new TestInput(DataFolder.Exists, Files.NotExistent), new ExpectedResult(new PartialMatch("No files in data folder"))),
                new Test(testName, new TestInput(DataFolder.Exists, Files.Exists, FileCount.Single, HeaderCount.None), new ExpectedResult(new PartialMatch("Empty file"))),
                new Test(testName, new TestInput(DataFolder.Exists, Files.Exists, FileCount.Single, HeaderCount.Single, DataRowCount.None), new ExpectedResult(new PartialMatch("Header row found, but no data rows exist in file")))
            );

            return table;
        }

        TestMachine IStateBasedTester.StateMachine
        {
            get
            {
                TestMachine stateMachine = DataMachine();
                return stateMachine;
            }
        }

        private TestMachine DataMachine()
        {
            var states = DataStates();
            var stateMachine = new TestMachine(states.ToArray(), this);
            stateMachine.StateChanged += DataSetup;
            return stateMachine;
        }

        private List<TestState> DataStates()
        {
            var states = new List<TestState>();
            var table = DataModel();
            TestState prev = null;
            var regressionTable = DefineTests();
            foreach (var row in table)
            {
                var next = new TestState(row.ToArray(), new Transition[] { }, this);
                SetExpectedResult(regressionTable, next);
                states.Add(next);
                if(prev!=null)
                {
                    prev.Transitions = new Transition[1] { new Transition() };
                    prev.Transitions[0].End = next;
                }

                prev = next;
            }
            states[0].IsStart = true;
            return states;
        }

        private void SetExpectedResult(TestTable table, TestState state)
        {
            bool match = false;
            foreach (var row in table)
            {
                foreach(var expectation in row)
                {
                    var allArgsMatch = true;              
                    for(var k=0; k<expectation.Arguments.Count; k++)
                    {
                        var thisArg = expectation.Arguments[k];
                        var thatArg = (Parameter)state.Parameters[k];
                        var unused = thisArg.Value.ToLower().Trim().Equals("unusedparameter");
                        if ( !unused &&  thisArg.EnumValue != thatArg.EnumValue)
                        {
                            allArgsMatch = false;
                            break;
                        }
                    }

                    if(allArgsMatch)
                    {
                        state.Expectation = expectation.Result;
                        match = true;
                        break;
                    }
                }
                if(match)
                {
                    break;
                }
            }
        }

        private ParameterTable DataModel()
        {
            var stateParams = ((IParameterTester)this).GetParameters();
            var model = new ParameterModel(stateParams);
            var table = model.CreateTable();
            return table;
        }
        
        void DataSetup(State newDataState)
        {
            var parameters = newDataState.Parameters;
            DataSetup(parameters);
        }

        private void DataSetup(IParameter[] parameters)
        {
            FolderPath folderPath = null;
            foreach (Parameter parameter in parameters)
            {
                if (parameter.EnumValue is DataFolder)
                {
                    folderPath = new DataFolderFactory().Create((DataFolder)parameter.EnumValue);
                    if (folderPath == null) { program.Arguments = null; }
                    else
                    {
                        program.Arguments = folderPath.Quoted;
                    }
                }
                else if (parameter.EnumValue is Files)
                {
                    var files = (Files)parameter.EnumValue;
                    new FilesFactory().Create(folderPath, files);
                }
                else if (parameter.EnumValue is FileCount)
                {
                    var filesCount = (FileCount)parameter.EnumValue;
                    new FilesCountFactory().Create(folderPath, filesCount);
                }
                else if (parameter.EnumValue is HeaderCount)
                {
                    var headerCount = (HeaderCount)parameter.EnumValue;
                    new HeaderCountFactory().Create(folderPath, headerCount);
                }
                else if (parameter.EnumValue is HeaderCount)
                {
                    var dataRowCount = (DataRowCount)parameter.EnumValue;
                    new DataRowCountFactory().Create(dataRowCount);
                }
            }
        }

        void ITransitionDriver.Execute(Transition transition)
        {            
            var result = program.Run();
            Validate((TestState)transition.Start, result);
        }

        private static void Validate(TestState state, Result result)
        {
            var expectedResult = state.Expectation as ExpectedResult;
            if (expectedResult != null)
            {
                if (!expectedResult.Equals(result))
                {
                    throw new TestException("{0} Test Failed: Expected {1}, Actual {2}", "Data Machine", expectedResult, result);
                }
            }
        }
    }

    public class Prototype
    {
        public static void Main(string[] args)
        {
            Program program = null;
            new Tester(program);
        }
    }
}
