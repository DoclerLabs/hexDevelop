using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using PluginCore;
using PluginCore.Managers;
using UnitTestSessionsPanel.Helpers;
using UnitTestSessionsPanel.Model;

namespace UnitTestSessionsPanel
{
    public partial class TestsSessionsPanel : DockPanelControl
    {
        private ImageList imageList;

        private int[] counters = new int[5];

        private TestTreeModel model;

        private List<TestInformation> sessionTests;
        private Dictionary<string, int> sessionTestsMap;
        private readonly Dictionary<ToolStripMenuItem, TestTreeModel.TestGroupMethod> groupMethodMap;
        
        private Settings settings;
        public Settings Settings
        {
            get { return settings; }
            set
            {
                settings = value;

                if (settings != null)
                {
                    splitContainer.SplitterMoved -= SplitContainer_SplitterMoved;
                    if (settings.OutputSide == 0)
                    {
                        rightOutputToolStripMenuItem.Checked = true;
                        bottomOutputToolStripMenuItem.Checked = false;
                        splitContainer.Orientation = Orientation.Vertical;
                        splitContainer.SplitterDistance = (int)(splitContainer.Width * settings.HorizontalTestResultSize / 100);
                    }
                    else
                    {
                        bottomOutputToolStripMenuItem.Checked = true;
                        rightOutputToolStripMenuItem.Checked = false;
                        splitContainer.Orientation = Orientation.Horizontal;
                        splitContainer.SplitterDistance = (int)(splitContainer.Height * settings.VerticalTestResultSize / 100);
                    }
                    splitContainer.SplitterMoved += SplitContainer_SplitterMoved;
                    outputToolStripSpltButton.Checked = settings.ShowOutput;

                    model.GroupMethod = settings.TestGroupByMethod;

                    foreach (var groupMapEntry in groupMethodMap)
                    {
                        groupMapEntry.Key.Checked = groupMapEntry.Value == settings.TestGroupByMethod;
                    }

                    selectFirstFailedTestToolStripMenuItem.Checked = settings.SelectFirstFailingTest;
                    showTimeToolStripMenuItem.Checked = settings.ShowTime;
                    sortToolButton.Checked = settings.SortTests;
                    trackActiveTestToolStripMenuItem.Checked = settings.TrackActiveTest;
                }
            }
        }
        
        public TestsSessionsPanel()
        {
            InitializeComponent();

            groupMethodMap = new Dictionary<ToolStripMenuItem, TestTreeModel.TestGroupMethod>
            {
                {noGroupToolMenuItem, TestTreeModel.TestGroupMethod.None},
                {categoryGroupToolMenuItem, TestTreeModel.TestGroupMethod.Category},
                {categoryAndPackageGroupToolMenuItem, TestTreeModel.TestGroupMethod.CategoryAndPackage},
                {packageGroupToolMenuItem, TestTreeModel.TestGroupMethod.Package},
                {packageAndSuiteGroupToolMenuItem, TestTreeModel.TestGroupMethod.PackageAndSuite},
                {suiteGroupToolMenuItem, TestTreeModel.TestGroupMethod.Suite}
            };

            sessionTests = new List<TestInformation>();
            sessionTestsMap = new Dictionary<string, int>();

            model = new TestTreeModel {TestSessionModel = sessionTests};
            testsTreeView.Model = new SortedTreeModel(model);

            imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                TransparentColor = Color.Transparent
            };

            nameNodeField.DrawText += NameNodeField_DrawText;
            childCountNodeField.DrawText += ChildCountNodeField_DrawText;
            stateNodeField.DrawText += StateNodeField_DrawText;
            timeNodeField.DrawText += TimeNodeField_DrawText;

            iconNodeField.ValueNeeded += IconNodeField_ValueNeeded;
            nameNodeField.ValueNeeded += NameNodeField_ValueNeeded;
            childCountNodeField.ValueNeeded += ChildCountNodeField_ValueNeeded;
            stateNodeField.ValueNeeded += StateNodeField_ValueNeeded;
            timeNodeField.ValueNeeded += TimeNodeField_ValueNeeded;

            if (PluginBase.MainForm == null) return;

            imageList.Images.Add(PluginBase.MainForm.FindImage("222")); // Unkown
            imageList.Images.Add(PluginBase.MainForm.FindImage("198")); // Ignored
            imageList.Images.Add(PluginBase.MainForm.FindImage("32")); // Passed
            imageList.Images.Add(PluginBase.MainForm.FindImage("196")); // Failed
            imageList.Images.Add(PluginBase.MainForm.FindImage("197")); // Error
            testTotalToolButton.Image = PluginBase.MainForm.FindImage("340");
            testSuccessToolButton.Image = imageList.Images[(int)TestResult.Passed];
            testFailedToolButton.Image = imageList.Images[(int)TestResult.Error];
            testIgnoredToolButton.Image = imageList.Images[(int)TestResult.Ignored];
            testUnkownToolButton.Image = imageList.Images[(int)TestResult.Unknown];
            outputToolStripSpltButton.Image = PluginBase.MainForm.FindImage("50");
            collapseAllToolButton.Image = PluginBase.MainForm.FindImage("488");
            expandAllToolButton.Image = PluginBase.MainForm.FindImage("489");
            optionsToolDropDownButton.Image = PluginBase.MainForm.FindImage("127");
            showTimeToolStripMenuItem.Image = PluginBase.MainForm.FindImage("123");
            groupByToolDropDownButton.Image = PluginBase.MainForm.FindImage("444");
            sortToolButton.Image = PluginBase.MainForm.FindImage("445");
        }

        #region Event Handling

        private void ChildCountNodeField_DrawText(object sender, DrawEventArgs e)
        {
            var groupNode = e.Node.Tag as TestGroupNode;
            if (groupNode != null)
            {
                e.TextColor = Color.Gray;
                e.Font = new Font(e.Font, FontStyle.Italic);
            }
        }

        private void TimeNodeField_DrawText(object sender, DrawEventArgs e)
        {
            e.TextColor = Color.FromArgb(75, 75, 75);
        }

        private void NameNodeField_DrawText(object sender, DrawEventArgs e)
        {
            TestNodeBase testNode = (TestNodeBase)e.Node.Tag;

            if (testNode.State == TestResult.Ignored)
            {
                e.TextColor = Color.FromArgb(105, 105, 105);
            }
        }

        private void StateNodeField_DrawText(object sender, DrawEventArgs e)
        {
            TestNodeBase testNode = (TestNodeBase)e.Node.Tag;

            switch (testNode.State)
            {
                case TestResult.Error:
                case TestResult.Failed:
                    e.TextColor = Color.Red;
                    break;
                case TestResult.Passed:
                    e.TextColor = Color.Green;
                    break;
                case TestResult.Ignored:
                    e.TextColor = Color.DarkOrange;
                    break;
            }
        }

        private void ChildCountNodeField_ValueNeeded(object sender, NodeControlValueEventArgs e)
        {
            var groupNode = e.Node.Tag as TestGroupNode;
            if (groupNode != null)
            {
                int totalTest = groupNode.TestChildren[0];
                e.Value = totalTest == 1 ? "1 test" : totalTest + " tests";
            }
        }

        private void TimeNodeField_ValueNeeded(object sender, NodeControlValueEventArgs e)
        {
            e.Value = ((TestNodeBase)e.Node.Tag).RunTime + "ms";
        }

        private void IconNodeField_ValueNeeded(object sender, NodeControlValueEventArgs e)
        {
            TestResult result = ((TestNodeBase)e.Node.Tag).State;
            e.Value = imageList.Images[(int)result];
        }

        private void NameNodeField_ValueNeeded(object sender, NodeControlValueEventArgs e)
        {
            e.Value = ((TestNodeBase)e.Node.Tag).Name;
        }

        private void StateNodeField_ValueNeeded(object sender, NodeControlValueEventArgs e)
        {
            TestResult result = ((TestNodeBase) e.Node.Tag).State;

            switch (result)
            {
                case TestResult.Passed:
                    e.Value = "Success";
                    break;

                case TestResult.Ignored:
                    e.Value = "Ignored";
/*                    if (!string.IsNullOrEmpty(info.Reason))
                    {
                        node.State += ": " + (info.Reason.Length > 64 ? info.Reason.Substring(0, 60) + "..." : info.Reason);
                    }
*/                    break;

                case TestResult.Unknown:
                    e.Value = "";
                    break;

                case TestResult.Failed:
                    e.Value = "Failed";
/*                    if (!string.IsNullOrEmpty(info.ShortReason))
                    {
                        node.State += ": " + info.ShortReason;
                    }
*/
                    break;

                case TestResult.Error:
                    e.Value = "Failed";
/*                    if (!string.IsNullOrEmpty(info.ShortReason))
                    {
                        node.State += ": " + info.ShortReason;
                    }
*/
                    break;
            }
        }

        private void TestsTreeView_SelectionChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<object, EventArgs>(TestsTreeView_SelectionChanged), sender, e);
                return;
            }

            TreeNodeAdv clickedNode = testsTreeView.SelectedNode;
            TestNodeBase testNode;

            if (clickedNode == null || (testNode = (TestNodeBase)clickedNode.Tag) == null)
            {
                testResultPanel.Visible = false;
                testResultPanel.SelectedTest = null;
            }
            else if (testNode is TestNode)
            {
                TestInformation info = ((TestNode) testNode).TestInformation;

                testResultPanel.SelectedTest = info;
                testResultPanel.Visible = true;
            }
        }

        private void TestsTreeView_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
        {
            TreeNodeAdv clickedNode = e.Node;
            TestNodeBase testNode;

            if (clickedNode != null && (testNode = (TestNodeBase)clickedNode.Tag) != null && testNode is TestNode)
            {
                TestInformation info = ((TestNode) testNode).TestInformation;

                testResultPanel.SelectedTest = info;
                testResultPanel.Visible = true;
                ScintillaHelper.SelectTextOnFileLine(info.Path, info.FunctionName);
            }
        }

        private void SplitContainer_Panel2_BackColorChanged(object sender, EventArgs e)
        {
            if (splitContainer.Panel2.BackColor != testsTreeView.BackColor)
            {
                splitContainer.Panel2.BackColor = testsTreeView.BackColor;
            }
        }

        private void TestsTreeView_BackColorChanged(object sender, EventArgs e)
        {
            splitContainer.Panel2.BackColor = testsTreeView.BackColor;
        }

        private void BottomOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!bottomOutputToolStripMenuItem.Checked)
            {
                rightOutputToolStripMenuItem.Checked = false;
                bottomOutputToolStripMenuItem.Checked = true;
                splitContainer.SplitterMoved -= SplitContainer_SplitterMoved;
                splitContainer.Orientation = Orientation.Horizontal;
                splitContainer.SplitterMoved += SplitContainer_SplitterMoved;
                if (settings == null) return;
                splitContainer.SplitterDistance = (int)(splitContainer.Height * settings.VerticalTestResultSize / 100);
                settings.OutputSide = 1;
            }
        }

        private void RightOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!rightOutputToolStripMenuItem.Checked)
            {
                rightOutputToolStripMenuItem.Checked = true;
                bottomOutputToolStripMenuItem.Checked = false;
                splitContainer.SplitterMoved -= SplitContainer_SplitterMoved;
                splitContainer.Orientation = Orientation.Vertical;
                splitContainer.SplitterMoved += SplitContainer_SplitterMoved;
                if (settings == null) return;
                splitContainer.SplitterDistance = (int)(splitContainer.Width * settings.HorizontalTestResultSize / 100);
                settings.OutputSide = 0;
            }
        }

        private void OutputToolStripSpltButton_CheckedChanged(object sender, EventArgs e)
        {
            if (outputToolStripSpltButton.Checked)
            {
                splitContainer.Panel2Collapsed = false;
                splitContainer.Panel2.Show();
                settings.ShowOutput = true;
            }
            else
            {
                splitContainer.Panel2Collapsed = true;
                splitContainer.Panel2.Hide();
                settings.ShowOutput = false;
            }
        }

        private void TestSuccessToolButton_Click(object sender, EventArgs e)
        {
            testSuccessToolButton.Checked = true;
            testFailedToolButton.Checked = false;
            testUnkownToolButton.Checked = false;
            testTotalToolButton.Checked = false;
            testIgnoredToolButton.Checked = false;

            model.FilterMethod = TestResult.Passed;
        }

        private void TestTotalToolButton_Click(object sender, EventArgs e)
        {
            testSuccessToolButton.Checked = false;
            testFailedToolButton.Checked = false;
            testUnkownToolButton.Checked = false;
            testTotalToolButton.Checked = true;
            testIgnoredToolButton.Checked = false;

            model.FilterMethod = null;
        }

        private void TestFailedToolButton_Click(object sender, EventArgs e)
        {
            testSuccessToolButton.Checked = false;
            testFailedToolButton.Checked = true;
            testUnkownToolButton.Checked = false;
            testTotalToolButton.Checked = false;
            testIgnoredToolButton.Checked = false;

            model.FilterMethod = TestResult.Failed;
        }

        private void TestUnkownToolButton_Click(object sender, EventArgs e)
        {
            testSuccessToolButton.Checked = false;
            testFailedToolButton.Checked = false;
            testUnkownToolButton.Checked = true;
            testTotalToolButton.Checked = false;
            testIgnoredToolButton.Checked = false;

            model.FilterMethod = TestResult.Unknown;
        }

        private void TestIgnoredToolButton_Click(object sender, EventArgs e)
        {
            testSuccessToolButton.Checked = false;
            testFailedToolButton.Checked = false;
            testUnkownToolButton.Checked = false;
            testTotalToolButton.Checked = false;
            testIgnoredToolButton.Checked = true;

            model.FilterMethod = TestResult.Ignored;
        }

        private void SplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (settings != null)
            {
                if (splitContainer.Orientation == Orientation.Vertical)
                    settings.HorizontalTestResultSize = splitContainer.SplitterDistance * 100 / splitContainer.Width;
                else
                    settings.VerticalTestResultSize = splitContainer.SplitterDistance * 100 / splitContainer.Height;
            }
        }

        private void ExpandAllToolButton_Click(object sender, EventArgs e)
        {
            if (testsTreeView.Root.Children.Count > 0)
            {
                testsTreeView.ExpandAll();
            }
        }

        private void CollapseAllToolButton_Click(object sender, EventArgs e)
        {
            if (testsTreeView.Root.Children.Count > 0)
            {
                testsTreeView.CollapseAll();
            }
        }

        private void ShowTimeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            testTimeColumn.IsVisible = showTimeToolStripMenuItem.Checked;
            if (settings != null) settings.ShowTime = showTimeToolStripMenuItem.Checked;
        }

        private void SelectFirstFailedTestToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (settings != null) settings.SelectFirstFailingTest = selectFirstFailedTestToolStripMenuItem.Checked;
        }

        private void TrackActiveTestToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (settings != null) settings.TrackActiveTest = trackActiveTestToolStripMenuItem.Checked;
        }

        private void GroupByToolMenuItem_Click(object sender, EventArgs e)
        {
            var toolMenuItem = (ToolStripMenuItem)sender;
            if (!toolMenuItem.Checked)
            {
                foreach (ToolStripMenuItem menuItem in toolMenuItem.GetCurrentParent().Items)
                {
                    menuItem.Checked = menuItem == toolMenuItem;
                }

                model.GroupMethod = groupMethodMap[toolMenuItem];

                if (settings != null) settings.TestGroupByMethod = model.GroupMethod;
            }
        }

        private void SortToolButton_CheckedChanged(object sender, EventArgs e)
        {
            ((SortedTreeModel)testsTreeView.Model).Comparer = sortToolButton.Checked ? new NameNodeComparer() : null;
            if (settings != null) settings.SortTests = sortToolButton.Checked;
        }

        private void TestsToolStrip_Layout(object sender, LayoutEventArgs e)
        {
            const int minWidth = 75;
            const int maxWidth = 400;

            int left = testUnkownToolButton.Bounds.Right + testProgressBar.Margin.Left;
            int width = testsToolStrip.Width - left - testProgressBar.Padding.Horizontal * 2 - 5;

            if (width < minWidth)
            {
                if (minWidth > testsToolStrip.Width)
                {
                    left = 0;
                    width = testsToolStrip.Width;
                }
                else if (left + minWidth > testsToolStrip.Width)
                {
                    width = Math.Min(testsToolStrip.Width - testProgressBar.Padding.Horizontal * 2 - 5, maxWidth);
                    left = Math.Max(testsToolStrip.Width - testProgressBar.Padding.Horizontal * 2 - width - 5, 0);
                }
                else
                {
                    width = minWidth;
                }
            }
            else if (width > maxWidth)
            {
                width = maxWidth;
                left = testsToolStrip.Width - testProgressBar.Padding.Horizontal * 2 - width - 5;
            }

            testProgressBar.Width = width;
            testProgressBar.Control.Left = left;
        }

        #endregion

        #region Update Information

        public void ClearStats()
        {
            Array.Clear(counters, 0, counters.Length);
            testProgressBar.Value = 0;
            testProgressBar.ForeColor = Color.LimeGreen;
            testsTreeView.BeginUpdate();
            model.Clear();
            sessionTests.Clear();
            sessionTestsMap.Clear();
        }

        public void EndUpdate()
        {
            testsTreeView.EndUpdate();
            UpdateProgress();
        }

        public void UpdateProgress()
        {
            int totalErrors = counters[(int)TestResult.Error] + counters[(int)TestResult.Failed];
            int totalSuccess = counters[(int)TestResult.Passed];
            int totalIgnored = counters[(int)TestResult.Ignored];
            int totalUnkown = counters[(int)TestResult.Unknown];
            int totalTests = totalErrors + totalSuccess + totalIgnored + totalUnkown;

            testTotalToolButton.Text = totalTests.ToString();
            testSuccessToolButton.Text = totalSuccess.ToString();
            testFailedToolButton.Text = totalErrors.ToString();
            testIgnoredToolButton.Text = totalIgnored.ToString();
            testUnkownToolButton.Text = totalUnkown.ToString();

            if (totalTests == 0) testProgressBar.Value = 0;
            else
            {
                testProgressBar.Maximum = totalTests;
                testProgressBar.Value = totalSuccess;
            }
        }

        #endregion

        #region Test Manipulation

        public void AddTest(TestInformation info)
        {
            TestNodeBase node;
            string fullName = info.ClassName + "." + info.FunctionName;

            int existingPos;
            if (sessionTestsMap.TryGetValue(fullName, out existingPos))
            {
                sessionTests[existingPos] = info;
                node = model.GetNode(info);
                TraceManager.AddAsync(string.Format("Test {0} already run", fullName));
            }
            else
            {
                sessionTestsMap[fullName] = sessionTests.Count;
                sessionTests.Add(info);
                node = model.AddNode(info);
            }

            if (info.Result.IsSimilarLevel(TestResult.Failed))
            {
                var nodeView = testsTreeView.FindNodeByTag(node);
                var expandNode = nodeView;
                while (!expandNode.IsExpanded)
                {
                    expandNode.IsExpanded = true;
                    expandNode = expandNode.Parent;
                }
                testsTreeView.ScrollTo(nodeView);

                if (selectFirstFailedTestToolStripMenuItem.Checked &&
                    (testsTreeView.SelectedNode == null ||
                     !((TestNode) testsTreeView.SelectedNode.Tag).State.IsSimilarLevel(
                         TestResult.Failed)))
                {
                    testsTreeView.SelectedNode = nodeView;
                }

                testProgressBar.ForeColor = Color.Red;
            }
            else if (trackActiveTestToolStripMenuItem.Checked)
            {
                var nodeView = testsTreeView.FindNodeByTag(node);
                var expandNode = nodeView;
                while (!expandNode.IsExpanded)
                {
                    expandNode.IsExpanded = true;
                    expandNode = expandNode.Parent;
                }
                testsTreeView.ScrollTo(nodeView);
            }

            AddTestResultToStatistics(info.Result);
        }

        public bool IsTesting(TestInformation info)
        {
            return sessionTestsMap.ContainsKey(info.ClassName + "." + info.FunctionName);
        }

        public void SetTestPathAndLine(TestInformation testInfo)
        {
            testInfo.Name = testInfo.Name.Replace('/', '.');
            var testNode = model.GetNode(testInfo);
            /*var info = (TestInformation)testNode.Tag;
            info.FunctionName = testInfo.FunctionName;
            info.Path = testInfo.Path;
            info.Line = testInfo.Line;
            testNode.Tag = info;*/
        }

        private void AddTestResultToStatistics(TestResult result)
        {
            counters[(int)result]++;
        }

        #endregion

        private class NameNodeComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return ((TestNodeBase) x).Name.CompareTo(((TestNodeBase) y).Name);
            }
        }
    }
}