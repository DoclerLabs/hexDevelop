using System.Drawing;
using System.Windows.Forms;

namespace UnitTestSessionsPanel.Controls
{
    public partial class TestResultPanel : UserControl
    {
        private TestInformation selectedTest;

        public TestInformation SelectedTest
        {
            get { return selectedTest; }
            set
            {
                selectedTest = value;

                if (value != null)
                {
                    string txt = "";
                    Color color;

                    switch (value.Result)
                    {
                        case TestResult.Error:
                        case TestResult.Failed:
                            txt = string.Format("{0} failed", value.Name);
                            color = Color.FromArgb(255, 120, 120);
                            break;
                        case TestResult.Passed:
                            txt = string.Format("{0} passed", value.Name);
                            color = Color.PaleGreen;
                            break;
                        case TestResult.Ignored:
                            txt = string.Format("{0} ignored", value.Name);
                            color = Color.FromArgb(255, 176, 98);
                            break;
                        default:
                            txt = value.Name;
                            color = Color.FromArgb(200, 200, 200);
                            break;
                    }

                    if (!string.IsNullOrEmpty(value.Reason))
                    {
                        richTextBox1.Text = value.Reason;
                        richTextBox1.Visible = true;
                    }
                    else
                    {
                        richTextBox1.Visible = false;
                        richTextBox1.Clear();
                    }

                    label1.Text = txt;
                    panel1.BackColor = color;
                }
            }
        }
        
        public TestResultPanel()
        {
            InitializeComponent();
        }
    }
}
