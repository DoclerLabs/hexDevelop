namespace UnitTestSessionsPanel
{
    public enum TestResult
    {
        Unknown,
        Ignored,
        Passed,
        Failed,
        Error
    }

    public class TestInformation
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string ClassName { get; set; }
        public string FunctionName { get; set; }
        public string CategoryName { get; set; }
        public int Line { get; set; }
        public string Reason { get; set; }
        public string ShortReason { get; set; }
        public string OwnerTestSuite { get; set; }
        public double RunTime { get; set; }
        public TestResult Result { get; set; }

        public TestInformation()
        {
            this.RunTime = double.NaN;
        }
    }

    internal static class TestResultExtensions
    {
        public static bool IsSimilarLevel(this TestResult src, TestResult other)
        {
            return other == TestResult.Failed || other == TestResult.Error
                ? src == TestResult.Failed || src == TestResult.Error
                : src == other;
        }
    }
}
