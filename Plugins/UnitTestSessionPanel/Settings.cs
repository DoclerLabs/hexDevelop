using System;
using System.ComponentModel;
using UnitTestSessionsPanel.Model;

namespace UnitTestSessionsPanel
{
    [Serializable]
    public class Settings
    {
        [Browsable(false)]
        public byte OutputSide { get; set; }

        private float horizontalTestResultSize = 75;
        [Browsable(false)]
        public float HorizontalTestResultSize
        {
            get { return horizontalTestResultSize; }
            set { horizontalTestResultSize = value; }
        }

        private float verticalTestResultSize = 70;
        [Browsable(false)]
        public float VerticalTestResultSize
        {
            get { return verticalTestResultSize; }
            set { verticalTestResultSize = value; }
        }

        [Browsable(false)]
        public bool SelectFirstFailingTest { get; set; }

        private bool showOutput = true;
        [Browsable(false)]
        public bool ShowOutput
        {
            get { return showOutput; }
            set { this.showOutput = value; }
        }

        [Browsable(false)]
        public bool ShowTime { get; set; }

        [Browsable(false)]
        public bool SortTests { get; set; }

        private TestTreeModel.TestGroupMethod testGroupByMethod = TestTreeModel.TestGroupMethod.Package;
        [Browsable(false)]
        internal TestTreeModel.TestGroupMethod TestGroupByMethod
        {
            get { return testGroupByMethod; }
            set { this.testGroupByMethod = value; }
        }

        [Browsable(false)]
        public bool TrackActiveTest { get; set; }
    }
}