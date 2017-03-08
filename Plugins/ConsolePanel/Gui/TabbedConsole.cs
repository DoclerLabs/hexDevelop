using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ConsolePanel.Gui
{
    public partial class TabbedConsole : UserControl
    {
        //private List<CmdPanel> consoles;
        private PluginMain main;

        public ICollection<ConsoleControl.ConsoleProvider> Consoles
        {
            get
            {
                return consoleTabMap.Keys;
            }
        }

        public Dictionary<ConsoleControl.ConsoleProvider, TabPage> consoleTabMap;
        public Dictionary<TabPage, ConsoleControl.ConsoleProvider> tabConsoleMap;

        public TabbedConsole(PluginMain plugin)
        {
            InitializeComponent();

            main = plugin;
            consoleTabMap = new Dictionary<ConsoleControl.ConsoleProvider, TabPage>();
            tabConsoleMap = new Dictionary<TabPage, ConsoleControl.ConsoleProvider>();

            btnNew.Image = PluginCore.PluginBase.MainForm.FindImage16("33");
        }

        public void AddConsole(ConsoleControl.ConsoleProvider console)
        {
            if (!(console is Control))
                throw new Exception("ConsoleControl needs to be a System.Windows.Forms.Control");

            var tabPage = new TabPage("Console");
            
            tabPage.Controls.Add((Control)console);

            tabConsoles.TabPages.Add(tabPage);
            tabConsoles.SelectTab(tabPage);
            consoleTabMap.Add(console, tabConsoles.SelectedTab);
            tabConsoleMap.Add(tabConsoles.SelectedTab, console);
        }

        public void RemoveConsole(ConsoleControl.ConsoleProvider console)
        {
            if (consoleTabMap.ContainsKey(console))
            {
                console.Cancel();

                var page = consoleTabMap[console];
                tabConsoles.TabPages.Remove(page);
                consoleTabMap.Remove(console);
                tabConsoleMap.Remove(page);
            }
        }

        private void tabConsoles_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < tabConsoles.TabCount; i++)
                {
                    if (tabConsoles.GetTabRect(i).Contains(e.Location))
                    {
                        RemoveConsole(tabConsoleMap[tabConsoles.TabPages[i]]);
                    }
                }
                
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            main.CreateConsolePanel(null);
        }
    }
}
