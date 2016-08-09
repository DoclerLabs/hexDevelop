namespace UnitTestSessionsPanel
{
    internal class TestNode : TestNodeBase
    {
        public TestInformation TestInformation { get; private set; }

        public TestNode(TestInformation testInformation)
        {
            TestInformation = testInformation;
        }
    }
}
