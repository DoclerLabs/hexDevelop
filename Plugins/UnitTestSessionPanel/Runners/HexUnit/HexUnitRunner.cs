using System.IO;
using System.Threading;
using ASCompletion.Context;
using ASCompletion.Model;
using PluginCore;
using PluginCore.Managers;
using ProjectManager;
using ProjectManager.Projects.Haxe;
using ProjectManager.Projects;

namespace UnitTestSessionsPanel.Runners.HexUnit
{
    class HexUnitRunner
    {
        private static string PREVIOUS_TEST_CLASS;

        private string origDocumentPath;

        private string tempDocumentClass = "src/TestMainTmp.hx";

        private string testDocumentTemplate =
@"package;

class TestMainTmp
{
    static public function main() : Void
    {
        var emu : hex.unittest.runner.ExMachinaUnitCore = new hex.unittest.runner.ExMachinaUnitCore();
        
        #if js
            js.Browser.document.getElementById('console').style.display = 'block';
            emu.addListener( new hex.unittest.notifier.BrowserUnitTestNotifier('console') );
            emu.addListener( new hex.unittest.notifier.WebSocketNotifier('ws://localhost:6660') );
        #elseif flash
            emu.addListener( new hex.unittest.notifier.FlashUnitTestNotifier(flash.Lib.current) );
        #end
        
        emu.$(testToRun);
        emu.run();
    }	
}";


        public void Run(MemberModel testMember, ClassModel ownerClass)
        {
            this.NotifyTestStart();

            HaxeProject project = (HaxeProject)PluginBase.CurrentProject;

            this.GenerateDocumentClassFromTemplate(testMember, ownerClass);

            this.StoreOrigDocumentClass(project);

            this.SetDocumentClass(project, Path.Combine(Path.GetDirectoryName(project.ProjectPath), this.tempDocumentClass));


            this.Build();

            //TODO: figure out the build finished event
            Thread.Sleep(1500);

            this.SetDocumentClass(project, this.origDocumentPath);

            this.Cleanup();
        }

        private void NotifyTestStart()
        {
            TraceManager.Add("Running unit test for " + this.GetTestToRun());
        }

        private void GenerateDocumentClassFromTemplate(MemberModel testMember, ClassModel ownerClass)
        {
            string testToRun = testMember != null 
                                ? "addTestMethod(" + ownerClass.QualifiedName + ", \"" + testMember.Name + "\")"
                                : "addTest(" + ownerClass.QualifiedName + ")";

            string text = this.testDocumentTemplate;
            text = text.Replace("$(testToRun)", testToRun);
            File.WriteAllText(this.tempDocumentClass, text);
        }

        private string GetTestToRun()
        {
            /*string testToRun = ASContext.Context.CurrentMember != null 
                                ? "addTestMethod(" + ASContext.Context.CurrentClass.QualifiedName + ", \"" + ASContext.Context.CurrentMember.Name + "\")"
                                : "addTest(" + ASContext.Context.CurrentClass.QualifiedName + ")";*/

            string testToRun = ASContext.Context.CurrentClass.QualifiedName;

            if (!testToRun.EndsWith("Test") && !testToRun.EndsWith("Suite") && PREVIOUS_TEST_CLASS != null)
            {
                testToRun = PREVIOUS_TEST_CLASS;
            }
            else
            {
                PREVIOUS_TEST_CLASS = testToRun;
            }

            return testToRun;
        }


        private void StoreOrigDocumentClass(HaxeProject project)
        {
            string origMain = project.CompilerOptions.MainClass.Replace(".", "\\");
            string projectPath = Path.GetDirectoryName(project.ProjectPath);

            foreach (string cp in project.AbsoluteClasspaths)
            {
                if (File.Exists(Path.Combine(cp, origMain + ".hx")))
                {
                    this.origDocumentPath = Path.Combine(cp, origMain + ".hx");
                    break;
                }
            }
        }

        private void SetDocumentClass(Project project, string path)
        {
            project.SetDocumentClass(path, true);
            project.Save();
        }

        private void Build()
        {
            DataEvent de1 = new DataEvent(EventType.Command, ProjectManagerCommands.BuildProject, null);
            EventManager.DispatchEvent(null, de1);

            /*if (!buildActions.Build(project, true, noTrace))
            {
                BroadcastBuildFailed(project);
            }*/

            DataEvent de2 = new DataEvent(EventType.Command, ProjectManagerCommands.TestMovie, null);
            EventManager.DispatchEvent(null, de2);

            //EventManager.AddEventHandler(null, "ProjectManager.BuildComplete")
        }

        private void Cleanup()
        {
            File.Delete(this.tempDocumentClass);
        }

        /*public void HandleEvent(Object sender, NotifyEvent e, HandlingPriority priority)
        {
            switch (e.Type)
            {
                case EventType.Command:
                {
                    DataEvent de = (DataEvent)e;
                    if (de.Action == "ProjectManager.BuildComplete") 
                    {
                        TraceManager.Add("HELLOOOO");
                    }
                    break;
                }
            }
        }*/
    }
}



