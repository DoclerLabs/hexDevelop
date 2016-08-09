namespace UnitTestSessionsPanel
{
    internal abstract class TestNodeBase
    {
        public string Path { get; set; }

        public string Name { get; set; }

        public TestResult State { get; set; }

        public double RunTime { get; set; }

        public TestNodeBase Parent { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
