using System.Text.RegularExpressions;

namespace UnitTestSessionsPanel.Handlers.MessageHandlers.FlexUnit
{
    class FlexUnitMessageHandler : ITraceMessageHandler
    {
        private readonly TestsSessionsPanel ui;
        private readonly Regex testResultPattern;
        private readonly Regex testErrorPattern;
        private readonly Regex testErrorFilePattern;
        private readonly Regex testTimePattern;

        public FlexUnitMessageHandler(TestsSessionsPanel pluginUi)
        {
            ui = pluginUi;

            testResultPattern = new Regex("([A-Z]{1}[A-Za-z0-9.]{5,}) ([.|F])$");
            testErrorPattern = new Regex("^[0-9]+ [a-zA-Z]*::([a-zA-Z0-9.]+) ([a-zA-Z0-9:<> ]+)");
            testErrorFilePattern = new Regex("([a-zA-Z0-9]*/([a-zA-Z0-9]*))[()]{2}.(.*.as):([0-9]+)]");
            testTimePattern = new Regex("Time: ([0-9/.]*)");
        }

        public void ProcessMessage(string message)
        {
            if (testResultPattern.IsMatch(message)) ProcessTestResult(message);
            if (testErrorPattern.IsMatch(message)) ProcessTestError(message);
            if (testErrorFilePattern.IsMatch(message)) ProcessFileError(message);
            if (testTimePattern.IsMatch(message)) ProcessTestTime(message);
        }

        private void ProcessFileError(string message)
        {
            Match match = testErrorFilePattern.Match(message);
            TestInformation info = new TestInformation
            {
                Name = match.Groups[1].Value,
                FunctionName = match.Groups[2].Value,
                Path = match.Groups[3].Value,
                Line = int.Parse(match.Groups[4].Value)
            };
            if (ui.IsTesting(info)) ui.SetTestPathAndLine(info);
        }

        private void ProcessTestResult(string message)
        {
            Match match = testResultPattern.Match(message);
            TestInformation info = new TestInformation
            {
                Name = match.Groups[1].Value,
                Result = TestResultFromString(match.Groups[2].Value)
            };
            if (info.Result == TestResult.Passed) ui.AddTest(info);
        }

        private void ProcessTestError(string message)
        {
            Match match = testErrorPattern.Match(message);
            string name = match.Groups[1].Value;
            string error = match.Groups[2].Value;
            TestResult result = TestResult.Failed;
            if (Regex.IsMatch(error, "Error:")) result = TestResult.Error;
            TestInformation info = new TestInformation
            {
                Name = name,
                Reason = error,
                Result = result
            };
            ui.AddTest(info);
        }

        private void ProcessTestTime(string message)
        {
            Match match = testTimePattern.Match(message);
            //ui.SetRunTime(match.Groups[1].Value);
        }

        private static TestResult TestResultFromString(string result)
        {
            return result == "." ? TestResult.Passed : TestResult.Failed;
        }
    }
}