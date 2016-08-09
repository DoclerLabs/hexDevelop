using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTestSessionsPanel
{
    internal class TestGroupNode : TestNodeBase
    {
        public enum TestGroupType
        {
            Package,
            Category,
            Suite,
            Class
        }

        public TestGroupType Type { get; private set; }

        private int[] testChildren;
        public int[] TestChildren
        {
            get { return testChildren; }
            set
            {
                if (testChildren != value)
                {
                    testChildren = value;
                }
            }
        }

        public int failedChildren;
        public int FailedChildren
        {
            get { return failedChildren; }
            set
            {
                if (failedChildren != value)
                {
                    failedChildren = value;
                }
            }
        }

        public TestGroupNode()
        {
            testChildren = new int[5];
        }
    }
}
