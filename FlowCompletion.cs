using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using PluginCore;
using ProjectManager.Projects.Haxe;
using System.Windows.Forms;
using System.Threading;
using ASCompletion;
using ASCompletion.Context;
using PluginCore.Controls;

namespace FlowCompletion
{

    public enum CompletionType
    {
        None,
        NewDeclaration,
        StaticRef,
        MethodCall,
        Property
    };

    delegate void Fun();
    delegate bool Check(char p);

    public class FlowCompletion
    {
        ICompletionHandler completionHandler;
        private bool isNew;
        private Dictionary<string, string> savedDeclaration;

        public FlowCompletion(CompletionMethod method)
        {
            this.isNew = false;
            this.savedDeclaration = new Dictionary<string, string>();

            SetCompletionMethod(method);
        }

        public void SetCompletionMethod(CompletionMethod method)
        {
            switch (method)
            {
                case CompletionMethod.Compiler:
                    completionHandler = new CompilerCompletionHandler();
                    break;
                case CompletionMethod.Fallback:
                    completionHandler = new FallbackCompletionHandler();
                    break;
                case CompletionMethod.Combined:
                    completionHandler = new CombinedCompletionHandler();
                    break;
                default:
                    completionHandler = new CombinedCompletionHandler();
                    break;
            }
        }

        public void choseCompletion(bool checkNew, char c)
        {
            if (isNewDeclaration())
            {
                isNew = true;
                getCompletion(true);
            }
            else if (isNewProperty()) getCompletion(true);
        }

        public void AddNewDeclaration()
        {
            ScintillaNet.ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
 
            int i = 0;
            string text = null;
            string name;
            string value = null;
            int index;

            while ((text = sci.GetLine(i)) != "")
            {
                if (text.IndexOf(" ") > -1) text = text.Replace(" ", "");
                if (text.IndexOf("\t") > -1) text = text.Replace("\t", "");
                if ((index = text.IndexOf("=new")) > -1)
                {
                    value = "";
                    name = text.Substring(0, index);

                    for (int start = index + 4; start < text.Length; start++)
                    {
                        if (IsAllowedChar(text[start]))
                            value += text[start];
                        else break;
                    }

                    //MessageBox.Show("name -> (" + name + ")" + "\n value -> (" + value + ")");
                    if (savedDeclaration.ContainsKey(name)) savedDeclaration[name] = value;
                    else savedDeclaration.Add(name, value);
                }
                   i++;
            }


            //var text = sci.GetLine(sci.CurrentLine);

            //GoToLine()
            
            /*var text = sci.GetText(sci.Length);

            MessageBox.Show(text);

            string name;
            string value = null;
            int index;

            if (text.IndexOf(" ") > -1) text = text.Replace(" ", "");
            if (text.IndexOf("\t") > -1) text = text.Replace("\t", "");
            MessageBox.Show(text);
            while ((index = text.IndexOf("=new")) > -1)
            {
                name = text.Substring(0, index);
                
                for (int start = index + 4; start < text.Length; start++)
                {
                    if (IsAllowedChar(text[start]))
                        value += text[start];
                    else break;
                }
                MessageBox.Show("name -> ("+name+")" + "\n value -> ("+value+")");
                savedDeclaration.Add(name, value);
            }*/
        }

        public bool getCompletion(bool checkNew)
        { 
            if (!isFileValid()) return false;

            string word = null;
            var selection = findSelection(IsAllowedChar, false);

            AddNewDeclaration();

            if (selection != null && savedDeclaration.ContainsKey(selection.Substring(0, selection.IndexOf("."))))
                selection = selection.Replace(selection.Substring(0, selection.IndexOf(".")), savedDeclaration[selection.Substring(0, selection.IndexOf("."))]);
            if (selection == null && !checkNew) return false;
            else if (selection != null && !selection.EndsWith('.'))
            {
                var dot = selection.LastIndexOf('.');
                if (dot == -1)
                {
                    word = selection;
                    selection = "";
                }
                else
                {
                    word = selection.Substring(dot + 1);
                    selection = selection.Substring(0, dot + 1);
                }
                if (word == "") word = null;
            }
            else if (selection == null && checkNew)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    completionHandler.GetCompletion(".", delegate (List<string> list)
                    {
                        if (list == null || list.Count == 0) return;

                        InvokeSci(delegate
                        {
                            ShowCompletionList(list, "");
                        });
                    });
                });
                return true;
            }
            else
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    completionHandler.GetCompletion(selection, delegate (List<string> list)
                    {
                        if (list == null || list.Count == 0) return;

                        InvokeSci(delegate
                        {
                            ShowCompletionList(list, word);
                        });
                    });
                });
                return true;
            }
            return true;
        }

        public bool GoToDeclaration()
        {
            if (isFileValid("text"))
            {
                string fileSelection = findSelection(delegate (char c)
                {
                    foreach (char inv in Path.GetInvalidPathChars())
                    {
                        if (inv == c) return false;
                    }
                    return true;
                });
                if (fileSelection != null && fileSelection.EndsWith(".flow") && OpenFile(fileSelection))
                    return false;
                var selection = findSelection(IsAllowedChar, true, false);
                MessageBox.Show(selection);
                var sci = PluginBase.MainForm.CurrentDocument.SciControl;

                if (selection == null || selection == "") return false;

                if (savedDeclaration.Count == 0) AddNewDeclaration();
                foreach(KeyValuePair<string, string> decla in savedDeclaration)
                {
                    if (selection.Contains(decla.Key))
                    {
                        selection = selection.Replace(decla.Key, decla.Value);
                        break;
                    }
                }
                Goto(selection, null);
            }
            return true;
        }

        private void Goto(string type, Fun fail)
        {
            if (type == null || type == "")
            {
                fail?.Invoke();
            }

            ThreadPool.QueueUserWorkItem(delegate
            {
                completionHandler.GetPosition(type, delegate (PositionResult pos)
                {
                    if (pos == null)
                    {
                        fail?.Invoke();
                        return;
                    }

                    FixPath(pos);

                    InvokeSci(delegate
                    {
                        GotoPosition(pos);
                    });
                });

            });
        }

        private bool OpenFile(string file)
        {
            completionHandler.GetFile(file, delegate(string str)
            {
                if (str == null) return;

                var pos = new PositionResult(str, 0, false);
                FixPath(pos);

                InvokeSci(delegate
                {
                    GotoPosition(pos);
                });
            });
            return true;
        }

        private void FixPath(PositionResult pos)
        {
            if (!Path.IsPathRooted(pos.file))
            {
                var hxproj = (PluginBase.CurrentProject as HaxeProject);
                pos.file = Path.GetFullPath(Path.Combine(hxproj.Directory, pos.file));
            }
        }

        private void GotoPosition(PositionResult result)
        {
            if (result == null) return;

            PluginBase.MainForm.OpenEditableDocument(result.file, false);
            if (result.lines) PluginBase.MainForm.CurrentDocument.SciControl.GotoLine(result.pos);
            else PluginBase.MainForm.CurrentDocument.SciControl.GotoPos(result.pos);
        }

        private void ShowCompletionList(List<string> list, string word)
        {
            var items = new List<ICompletionListItem>();

            foreach (string item in list)
            {
                items.Add(new PackageMember(item));
            }

            var sci = PluginBase.MainForm.CurrentDocument.SciControl;

            CompletionList.Show(items, true, word);
        }

        private void InvokeSci(Fun onResult)
        {
            ScintillaNet.ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;

            if (sci.InvokeRequired)
            {
                sci.BeginInvoke((MethodInvoker)delegate
                {
                    onResult();
                });
            }
            else onResult();
        }

        private string findSelection(Check isCharAllowed, bool checkRight = true, bool selectAfterDot = true)
        {
            ScintillaNet.ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
            string text = sci.GetLine(sci.CurrentLine);

            var lineStart = sci.PositionFromLine(sci.CurrentLine);
            var lineEnd = lineStart + sci.LineLength(sci.CurrentLine);
            int left = sci.CurrentPos - 1;
            int right = sci.CurrentPos - 1;

            if (right < 0)
                right = 0;
            if (left < 0)
                left = 0;

            if (!IsAllowedChar((char)(sci.CharAt(left))))
                return null;

            for (int i = left; i >= lineStart; i--)
            {
                char c = (char)sci.CharAt(i);
                if (IsAllowedChar((char)sci.CharAt(i))) left = i;
                else break;
            }

            if (checkRight)
            {
                for (int i = right; i <= lineEnd; i++)
                {
                    char c = (char)sci.CharAt(i);
                    if (IsAllowedChar((char)sci.CharAt(i)))
                    {
                        if (!selectAfterDot)
                        {
                            if ((char)sci.CharAt(i) != '.') right = i;
                            else break;
                        }
                        else right = i;

                    }
                    else break;
                }
            }
            try
            {
                var selection = text.Substring(left - lineStart, (right - lineStart)  - (left - lineStart) + 1);
                return selection;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private bool isNewProperty()
        {
            ScintillaNet.ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
            var text = sci.GetLine(sci.CurrentLine);
            int currPos = sci.CurrentPos;
            int lineStart = sci.PositionFromLine(sci.CurrentLine);
            text = text.Substring(0, currPos - lineStart);

            if (text.EndsWith("property : ") || text.EndsWith("property: "))
                return true;
            else return false;
        }

        private bool isNewDeclaration()
        {
            ScintillaNet.ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
            var text = sci.GetLine(sci.CurrentLine);
            text = text.Substring(0, text.Length - 1);

            if (text.EndsWith("= new ") || text.EndsWith("=new "))
                return true;
            else return false;
        }

        private bool isFileValid(string lang = "text")
        {
            var hxproj = (PluginBase.CurrentProject as HaxeProject);

            if (hxproj == null) return false;

            ScintillaNet.ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;

            if (sci == null) return false;
            //MessageBox.Show("current lang = " + sci.ConfigurationLanguage.ToString());
            if (sci.ConfigurationLanguage != lang) return false;

            return true;
        }

        public bool IsAllowedChar(char c)
        {
            return (Char.IsLetterOrDigit(c) || c == '.' || c == '_');
            //return false;
        }

        public bool NewDeclaration
        {
            get { return this.isNew; }
            set { this.isNew = value; }
        }
    }


    public class PackageMember : ICompletionListItem
    {
        private int icon;
        private string name;
        //TODO: add description (documentation)

        public PackageMember(string name)
        {
            this.name = name;
            if (Char.IsLower(name[0])) //Package because it starts in lower case
            {
                this.icon = PluginUI.ICON_PACKAGE;
            }
            else //Module
            {
                this.icon = PluginUI.ICON_TYPE;
            }

            //TODO: functions / vars / enums / typedefs ...
        }

        public string Label
        {
            get { return name; }
        }

        public virtual string Description
        {
            get
            {
                return "";
            }
        }

        public Bitmap Icon
        {
            get { return (Bitmap)ASContext.Panel.GetIcon(icon); }
        }

        public string Value
        {
            get
            {
                return name;
            }
        }

        public override string ToString()
        {
            return Label;
        }
    }
}

