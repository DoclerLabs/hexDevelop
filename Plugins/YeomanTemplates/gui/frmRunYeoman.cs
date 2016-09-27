using System;
using System.Windows.Forms;
using YeomanTemplates.gui.controls;

namespace YeomanTemplates
{
    public partial class frmRunYeoman : Form
    {
        private PageControl[] pages;
        private int currentPageIndex = 0;
        private PageControl currentPage;

        public frmRunYeoman(string workingDir, string yoCmd)
        {
            InitializeComponent();

            pages = new PageControl[] {
                new ctrlChoose(yoCmd),
                new ctrlCmd(workingDir, yoCmd)
            };

            setPage(0);
        }

        public void DisableBack()
        {
            btnBack.Enabled = false;
        }

        private void setPage(int page)
        {
            if (page >= pages.Length)
                return;

            if (currentPage != null)
                pnlPage.Controls.Remove((UserControl)currentPage);

            currentPageIndex = page;
            currentPage = pages[page];
            currentPage.Ready += currentPage_Ready;

            pnlPage.Controls.Add((UserControl)currentPage);

            btnBack.Enabled = true;
            if (page == 0)
                btnBack.Enabled = false;

            btnNext.Enabled = false;
        }

        private void currentPage_Ready(object sender, EventArgs e)
        {
            btnNext.Enabled = true;

            if (currentPageIndex == pages.Length - 1)
            {
                Close();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            var data = currentPage.GetOutput();
            setPage(currentPageIndex + 1);

            currentPage.setInput(data);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            setPage(currentPageIndex - 1);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            currentPage.OnCancel();
            this.Close();
        }
    }
}
