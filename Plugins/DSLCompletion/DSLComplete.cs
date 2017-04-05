using PluginCore;
using ScintillaNet;
using System;
using ProjectManager.Projects.Haxe;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using PluginCore.Controls;
using ASCompletion.Context;
using ASCompletion;
using XMLCompletion;

namespace DSLCompletion
{
    delegate void Fun();
    delegate bool Check(char p);

    class DSLComplete
    {
        ICompletionHandler completionHandler;

        public DSLComplete(CompletionMethod method)
        {
            SetCompletionMethod(method);
        }

        public bool GetCompletion()
        {
            if (!IsFileValid()) return false;

            var selection = FindSelection(IsAllowedChar, false);
            string word = null;

            if (selection == null || selection == ".") return false;

            if (!selection.EndsWith('.'))
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

            ThreadPool.QueueUserWorkItem(delegate
            {
                completionHandler.GetCompletion(selection, delegate(List<string> list)
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

        public bool FindClass()
        {
            if (!IsFileValid()) return false;

            var clazz = FindSelection(IsAllowedChar);
            if (clazz.Contains("."))
                return false;

            ThreadPool.QueueUserWorkItem(delegate
            {
                completionHandler.GetCompletePath(clazz, delegate (List<string> list)
                {
                    if (list == null || list.Count == 0) return;

                    InvokeSci(delegate
                    {
                        ShowCompletionList(list, clazz);
                    });
                });
            });

            return true;
        }

        /// <summary>
        /// Opens the haxe file that contains the class specified under the caret.
        /// </summary>
        /// <returns></returns>
        public bool GotoDeclaration()
        {
            var validHaxe = IsFileValid("haxe");
            var validXml = IsFileValid();

            if (validHaxe || validXml)
            {
                string fileSelection = FindSelection(delegate (char c)
                {
                    foreach (char inv in Path.GetInvalidPathChars())
                    {
                        if (inv == c) return false;
                    }
                    return true;
                });

                if (fileSelection != null && fileSelection.EndsWith(".xml") && OpenFile(fileSelection))
                {
                    return true;
                }
            }

            if (!validXml) return false;

            var selection = FindSelection(IsAllowedChar);

            var sci = PluginBase.MainForm.CurrentDocument.SciControl;
            var f = XMLComplete.GetXMLContextTag(sci, sci.CurrentPos);

            if (f.Name == "method-call" || f.Name == "property")
            {
                var p = XMLComplete.GetParentTag(sci, f);
                var parent = CloseTag(p);
                var field = CloseTag(f);

                var parentTag = System.Xml.XmlReader.Create(new StringReader(parent));
                var fieldTag = System.Xml.XmlReader.Create(new StringReader(field));

                string fieldAttr;
                string typeAttr = null;
                var type = selection;
                try
                {
                    parentTag.Read();
                    typeAttr = parentTag.GetAttribute("type");

                    fieldTag.Read(); //this will fail if caret is not at the end of the tag
                    fieldAttr = fieldTag.GetAttribute("name");
                }
                catch
                {
                    fieldAttr = type; //if fieldTag.Read failed, use selection as fallback
                }

                if (typeAttr != "Object")
                {
                    type = typeAttr + "." + fieldAttr;
                }

                parentTag.Close();
                fieldTag.Close();

                Goto(type, delegate  //fails if caret is not on field name
                {
                    Goto(selection, null);
                });
                    
            }
            else
            {
                if (selection == null || selection == "") return false;

                Goto(selection, null);
            }

            return true;
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
            }
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

        private void FixPath(PositionResult pos)
        {
            if (!Path.IsPathRooted(pos.file))
            {
                var hxproj = (PluginBase.CurrentProject as HaxeProject);
                pos.file = Path.GetFullPath(Path.Combine(hxproj.Directory, pos.file));
            }
        }

        /// <summary>
        /// Tries to figure out what haxe package the caret is on.
        /// </summary>
        /// <param name="checkRight">If is false, the caret is interpreted as the end of the selection</param>
        /// <returns>the current selection as a string</returns>
        private string FindSelection(Check isAllowedChar, bool checkRight = true)
        {
            ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
            
            var text = sci.GetLine(sci.CurrentLine);

            if (!text.Contains("\"")) return null;

            var lineStart = sci.PositionFromLine(sci.CurrentLine);
            var lineEnd = lineStart + sci.LineLength(sci.CurrentLine);
            int left = sci.CurrentPos - 1;
            int right = sci.CurrentPos - 1;
            if (right < 0) right = 0;
            if (left < 0) left = 0;

            if (!isAllowedChar((char)sci.CharAt(left))) //if char left of caret is not allowed
            {
                return null;
            }

            //find left-most allowed char
            for (int i = left; i >= lineStart; i--)
            {
                char c = (char)sci.CharAt(i);
                if (isAllowedChar(c))
                {
                    left = i;
                }
                else
                {
                    break;
                }
            }

            if (checkRight)
            {
                //find right-most allowed char
                for (int i = right; i <= lineEnd; i++)
                {
                    char c = (char)sci.CharAt(i);
                    if (isAllowedChar(c))
                    {
                        right = i;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (sci.GetTextRange(left - 1, left) != "\"")
                return null;

            try
            {
                var selection = sci.GetTextRange(left, right + 1);
                return selection;
            } catch (Exception e) { return null; }
            
        }

        /// <summary>
        /// Helper method to work with ScintillaControl from another thread.
        /// </summary>
        /// <param name="onResult">function that interacts with SciControl</param>
        private void InvokeSci(Fun onResult)
        {
            ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;

            if (sci.InvokeRequired)
            {
                sci.BeginInvoke((MethodInvoker)delegate
                {
                    onResult();
                });
            }
            else onResult();
        }

        /// <summary>
        /// Checks if completion should be used for this file.
        /// </summary>
        private bool IsFileValid(string lang = "xml")
        {
            var hxproj = (PluginBase.CurrentProject as HaxeProject);
            if (hxproj == null) return false;

            ScintillaControl sci = PluginBase.MainForm.CurrentDocument.SciControl;
            if (sci == null) return false;
            if (sci.ConfigurationLanguage != lang) return false;

            return true;
        }

        /// <summary>
        /// Checks if the specified char is a letter, digit, dot or underscore.
        /// </summary>
        /// <remarks>
        /// Only those characters are allowed in Haxe package names.
        /// </remarks>
        /// <param name="c">the char to check</param>
        public static bool IsAllowedChar(char c)
        {
            return Char.IsLetterOrDigit(c) || c == '.' || c == '_';
        }

        /// <summary>
        /// Helper function to add a closing tag to the given tag
        /// </summary>
        private string CloseTag(XMLContextTag tag)
        {
            var result = tag.Tag;
            if (!tag.Closed)
            {
                if (tag.Closing)
                {
                    result += ">";
                }
                else
                {
                    result += "</" + tag.Name + ">";
                }
            }

            return result;
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
        
        private void GotoPosition(PositionResult result)
        {
            if (result == null) return;

            PluginBase.MainForm.OpenEditableDocument(result.file, false);
            if (result.lines)
            {
                PluginBase.MainForm.CurrentDocument.SciControl.GotoLine(result.pos);
            }
            else
            {
                PluginBase.MainForm.CurrentDocument.SciControl.GotoPos(result.pos);
            }
            
        }
    }

    /// <summary>
    /// Package member completion list item.
    /// Can either be a package or a Module.
    /// </summary>
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
