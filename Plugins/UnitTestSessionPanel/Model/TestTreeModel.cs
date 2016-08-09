using System;
using System.Collections.Generic;
using System.Linq;
using Aga.Controls.Tree;

namespace UnitTestSessionsPanel.Model
{
    internal class TestTreeModel : TreeModelBase
    {
        public enum TestGroupMethod
        {
            None,
            Package,
            Category,
            CategoryAndPackage,
            Suite,
            PackageAndSuite
        }

        // TODO: Replace
        public List<TestInformation> TestSessionModel;

        private Dictionary<string, List<TestNodeBase>> _cache;

        private TestResult? _filterMethod;
        public TestResult? FilterMethod
        {
            get { return _filterMethod; }
            set
            {
                if (_filterMethod != value)
                {
                    _filterMethod = value;
                    OnStructureChanged(new TreePathEventArgs());
                }
            }
        }

        private TestGroupMethod _groupMethod = TestGroupMethod.Category;
        public TestGroupMethod GroupMethod
        {
            get { return _groupMethod; }
            set
            {
                if (_groupMethod != value)
                {
                    _groupMethod = value;
                    RepopulateTree();
                }
            }
        }

        public TestTreeModel()
        {
            _cache = new Dictionary<string, List<TestNodeBase>> {{"", new List<TestNodeBase>()}};
        }

        public override System.Collections.IEnumerable GetChildren(TreePath treePath)
        {
            List<TestNodeBase> retVal = null;

            if (treePath.IsEmpty())
            {
                retVal = _cache[""];
            }
            else
            {
                TestNodeBase parent = treePath.LastNode as TestNodeBase;
                if (parent != null)
                {
                    _cache.TryGetValue(parent.Path, out retVal);
                }
            }

            if (retVal == null || _filterMethod == null)
            {
                return retVal;
            }

            int newState = _filterMethod == TestResult.Failed ? 4 : ((int) _filterMethod + 1);
            
            return retVal.Where(x => x is TestGroupNode && ((TestGroupNode)x).TestChildren[newState] > 0 || x.State.IsSimilarLevel(_filterMethod.Value));
        }

        public override bool IsLeaf(TreePath treePath)
        {
            return treePath.LastNode is TestNode;
        }

        public TestNodeBase AddNode(TestInformation info)
        {
            string nodePath = GetTestPath(info);
            TestNodeBase newNode = null;
            TestNodeBase lastNode = null;
            bool canBreak = false;
            int index = 0;
            var stack = new Stack<object>();
            int pos = nodePath.Length - 1;
            int lastPos = pos;
            do
            {
                pos = nodePath.LastIndexOf(".", pos, StringComparison.OrdinalIgnoreCase);

                string path = pos > -1 ? nodePath.Substring(0, pos) : "";
                string name = nodePath.Substring(pos + 1, lastPos - pos);
                lastPos = --pos;

                List<TestNodeBase> nodes;
                if (!_cache.TryGetValue(path, out nodes))
                {
                    nodes = new List<TestNodeBase>();
                    _cache[path] = nodes;
                }
                else canBreak = true;

                if (newNode == null)
                {
                    lastNode = newNode = new TestNode(info) {RunTime = info.RunTime};
                }
                else
                {
                    var groupNode = new TestGroupNode {RunTime = newNode.RunTime};
                    groupNode.TestChildren[0] = 1;
                    groupNode.TestChildren[(int) newNode.State + 1] = 1;
                    lastNode = lastNode.Parent = groupNode;
                }

                lastNode.Name = name;
                lastNode.Path = path != "" ? path + "." + name : name;
                lastNode.State = info.Result;

                index = nodes.Count;

                nodes.Add(lastNode);
            } while (pos > -1 && !canBreak);

            if (pos > -1)
            {
                int parentPos = lastNode.Path.LastIndexOf('.');

                if (parentPos > -1)
                {
                    int pathEndPos = lastNode.Path.LastIndexOf('.', parentPos - 1);
                    string parentPath = pathEndPos > -1 ? lastNode.Path.Substring(0, pathEndPos) : "";
                    lastNode.Parent =
                        _cache[parentPath].FirstOrDefault(
                            x => x.Name == lastNode.Path.Substring(pathEndPos + 1, parentPos - pathEndPos - 1));
                }

                var tmpNode = lastNode;
                while ((tmpNode = tmpNode.Parent) != null)
                {
                    if (tmpNode.State < info.Result)
                    {
                        tmpNode.State = info.Result;
                    }
                    var groupNode = (TestGroupNode)tmpNode;
                    int state = newNode.State == TestResult.Error ? 4 : ((int) newNode.State + 1);
                    groupNode.TestChildren[0]++;
                    groupNode.TestChildren[state]++;
                    groupNode.RunTime += newNode.RunTime;

                    stack.Push(tmpNode);
                }
            }

            OnNodesInserted(new TreeModelEventArgs(new TreePath(stack.ToArray()), new[] {index}, new object[] {lastNode}));

            return newNode;
        }

        public void Clear()
        {
            _cache.Clear();
            _cache[""] = new List<TestNodeBase>();
            OnStructureChanged(new TreePathEventArgs());
        }

        public TestNodeBase GetNode(TestInformation info)
        {
            string path = GetTestPath(info);
            path = path.Substring(0, path.LastIndexOf('.') - 1);

            List<TestNodeBase> nodes;

            if (!_cache.TryGetValue(path, out nodes))
            {
                return null;
            }

            return nodes.FirstOrDefault(x => x.Name == info.Name);
        }

        private string GetTestPath(TestInformation info)
        {
            string name;

            switch (_groupMethod)
            {
                case TestGroupMethod.Package:
                    name = info.ClassName + "." + info.Name;
                    if (info.ClassName.IndexOf('.') < 0) name = "<Default Package>." + name;
                    break;
                case TestGroupMethod.Suite:
                    name = (info.OwnerTestSuite ?? "<No Suite>") + "." + info.Name;
                    break;
                case TestGroupMethod.PackageAndSuite:
                    // TODO: Get suite class path
                    name = (!string.IsNullOrEmpty(info.OwnerTestSuite)
                        ? info.OwnerTestSuite
                        : info.ClassName) + "." + info.Name;
                    break;
                case TestGroupMethod.Category:
                    name = (info.CategoryName ?? "<Uncategorized>") + "." + info.ClassName.Substring(info.ClassName.LastIndexOf('.') + 1) + "." + info.Name;
                    break;
                case TestGroupMethod.CategoryAndPackage:
                    name = (info.CategoryName ?? "<Uncategorized>") + "." + info.ClassName + "." + info.Name;
                    break;
                default:
                    name = info.ClassName.Substring(info.ClassName.LastIndexOf('.') + 1) + "." + info.Name;
                    break;
            }

            return name;
        }

        private void RepopulateTree()
        {
            Clear();
            for (int i = 0, count = TestSessionModel.Count; i < count; i++)
            {
                AddNode(TestSessionModel[i]);
            }
        }
    }
}
