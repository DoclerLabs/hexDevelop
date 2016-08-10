using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using PluginCore;
using PluginCore.Managers;
using UnitTestSessionsPanel.Helpers;
using UnitTestSessionsPanel.Model;
using UnitTestSessionsPanel.Runners.HexUnit;

namespace UnitTestSessionsPanel
{
    public partial class TestsExplorerPanel : DockPanelControl
    {
        private ImageList imageList;

        private TestTreeModel model;

        private List<TestInformation> sessionTests;
        private Dictionary<string, int> sessionTestsMap;
        private readonly Dictionary<ToolStripMenuItem, TestTreeModel.TestGroupMethod> groupMethodMap;
        
/*        private Settings settings;
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

                    RepopulateTree();

                    selectFirstFailedTestToolStripMenuItem.Checked = settings.SelectFirstFailingTest;
                    showTimeToolStripMenuItem.Checked = settings.ShowTime;
                    sortToolButton.Checked = settings.SortTests;
                    trackActiveTestToolStripMenuItem.Checked = settings.TrackActiveTest;
                }
            }
        }*/
        
        public TestsExplorerPanel()
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
            testsTreeView.SelectionMode = TreeSelectionMode.Multi; //test

            imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                TransparentColor = Color.Transparent
            };

            nameNodeField.DrawText += NameNodeField_DrawText;
            childCountNodeField.DrawText += ChildCountNodeField_DrawText;

            iconNodeField.ValueNeeded += IconNodeField_ValueNeeded;
            nameNodeField.ValueNeeded += NameNodeField_ValueNeeded;
            childCountNodeField.ValueNeeded += ChildCountNodeField_ValueNeeded;

            if (PluginBase.MainForm == null) return;

            imageList.Images.Add(PluginBase.MainForm.FindImage("222")); // Unkown
            imageList.Images.Add(PluginBase.MainForm.FindImage("198")); // Ignored
            imageList.Images.Add(PluginBase.MainForm.FindImage("32")); // Passed
            imageList.Images.Add(PluginBase.MainForm.FindImage("196")); // Failed
            imageList.Images.Add(PluginBase.MainForm.FindImage("197")); // Error
            runToolButton.Image = PluginBase.MainForm.FindImage("351");
            refreshToolButton.Image = PluginBase.MainForm.FindImage("66");
            collapseAllToolButton.Image = PluginBase.MainForm.FindImage("488");
            expandAllToolButton.Image = PluginBase.MainForm.FindImage("489");
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

        private void NameNodeField_DrawText(object sender, DrawEventArgs e)
        {
            TestNodeBase testNode = (TestNodeBase)e.Node.Tag;

            if (testNode.State == TestResult.Ignored)
            {
                e.TextColor = Color.FromArgb(105, 105, 105);
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

        private void IconNodeField_ValueNeeded(object sender, NodeControlValueEventArgs e)
        {
            TestResult result = ((TestNodeBase)e.Node.Tag).State;
            e.Value = imageList.Images[(int)result];
        }

        private void NameNodeField_ValueNeeded(object sender, NodeControlValueEventArgs e)
        {
            e.Value = ((TestNodeBase)e.Node.Tag).Name;
        }

        private void TestsTreeView_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
        {
            TreeNodeAdv clickedNode = e.Node;
            TestNodeBase testNode;

            if (clickedNode != null && (testNode = (TestNodeBase)clickedNode.Tag) != null && testNode is TestNode)
            {
                TestInformation info = ((TestNode) testNode).TestInformation;

                ScintillaHelper.SelectTextOnFileLine(info.Path, info.FunctionName);
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

                //if (settings != null) settings.TestGroupByMethod = model.GroupMethod;
            }
        }

        private void SortToolButton_CheckedChanged(object sender, EventArgs e)
        {
            ((SortedTreeModel)testsTreeView.Model).Comparer = sortToolButton.Checked ? new NameNodeComparer() : null;
            //if (settings != null) settings.SortTests = sortToolButton.Checked;
        }

        #endregion

        #region Update Information

        public void ClearStats()
        {
            testsTreeView.BeginUpdate();
            model.Clear();
            sessionTests.Clear();
            sessionTestsMap.Clear();
        }

        public void EndUpdate()
        {
            testsTreeView.EndUpdate();
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

        #endregion

        private class NameNodeComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return ((TestNodeBase) x).Name.CompareTo(((TestNodeBase) y).Name);
            }
        }

        private void RunToolButton_Click(object sender, EventArgs e)
        {
            if (testsTreeView.SelectedNodes == null || testsTreeView.SelectedNodes.Count == 0) return;

            var hexUnitRunner = new HexUnitRunner();
            foreach (TreeNodeAdv node in testsTreeView.SelectedNodes)
            {
                AddRunNode(hexUnitRunner, node);
            }
            hexUnitRunner.Run();
            
        }

        private void RefreshToolButton_Click(object sender, EventArgs e)
        {
            this.ClearStats();
            if (ASCompletion.Context.ASContext.Context.IsFileValid && ASCompletion.Context.ASContext.Context.CurrentModel.haXe)
            {
                foreach (var classItem in ASCompletion.Context.ASContext.Context.GetAllProjectClasses().Items)
                {
                    ASCompletion.Model.ClassModel classModel = classItem as ASCompletion.Model.ClassModel ??
                                                               ASCompletion.Context.ASContext.Context.ResolveType(classItem.Type, null);

                    if (classModel == null) continue;

                    foreach (var member in classModel.Members.Items)
                    {
                        if (member.MetaDatas != null)
                        {
                            foreach (var meta in member.MetaDatas)
                            {
                                if (meta.Name == "Test" || meta.Name == "Suite" || meta.Name == "Ignore")
                                {
                                    AddTest(new TestInformation
                                    {
                                        ClassName = classModel.QualifiedName,
                                        Name = member.Name,
                                        FunctionName = member.Name,
                                        Path = classModel.InFile.FileName
                                    });

                                    break;
                                }
                            }
                        }
                    }
                }
            }
            this.testsTreeView.EndUpdate();
        }

        private static void AddRunNode(HexUnitRunner runner, TreeNodeAdv node)
        {
            var selected = node.Tag as TestNode;
            //var group = node.Tag as TestGroupNode;

            if (selected != null)
            {
                var inFile = new ASCompletion.Model.FileModel();
                runner.AddTest(new ASCompletion.Model.MemberModel { Name = selected.TestInformation.FunctionName },
                    new ASCompletion.Model.ClassModel { Name = selected.TestInformation.ClassName, InFile = inFile });

            }
            else
            {
                foreach (var child in node.Children)
                {
                    AddRunNode(runner, child);
                }

            }
        }
    }
}