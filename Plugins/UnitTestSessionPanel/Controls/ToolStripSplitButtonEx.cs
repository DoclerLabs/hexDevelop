using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UnitTestSessionsPanel.Controls
{
    public class ToolStripSplitButtonEx : ToolStripSplitButton
    {

        public event EventHandler CheckedChanged;

        private static ProfessionalColorTable _professionalColorTable;

        private bool _checked;
        public bool Checked
        {
            get { return _checked; }
            set
            {
                _checked = value;
                Invalidate();
                OnCheckedChanged(EventArgs.Empty);
            }
        }

        public bool CheckOnClick { get; set; }

        private void RenderCheckedButtonFill(Graphics g, Rectangle bounds)
        {
            if ((bounds.Width == 0) || (bounds.Height == 0))
            {
                return;
            }

            if (!UseSystemColors)
            {
                using (Brush b = new LinearGradientBrush(bounds, ColorTable.ButtonCheckedGradientBegin, ColorTable.ButtonCheckedGradientEnd, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(b, bounds);
                }
            }
            else
            {
                Color fillColor = ColorTable.ButtonCheckedHighlight;

                using (Brush b = new SolidBrush(fillColor))
                {
                    g.FillRectangle(b, bounds);
                }
            }
        }

        private bool UseSystemColors
        {
            get { return ColorTable.UseSystemColors || !ToolStripManager.VisualStylesEnabled; }
        }

        private static ProfessionalColorTable ColorTable
        {
            get { return _professionalColorTable ?? (_professionalColorTable = new ProfessionalColorTable()); }
        }

        protected override void OnButtonClick(System.EventArgs e)
        {
            if (CheckOnClick) this.Checked = !this.Checked;

            base.OnButtonClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_checked)
            {
                Graphics g = e.Graphics;
                Rectangle bounds = new Rectangle(Point.Empty, Size);

                RenderCheckedButtonFill(g, bounds);

                using (Pen p = new Pen(ColorTable.ButtonSelectedBorder))
                {
                    g.DrawRectangle(p, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                }
            }
            base.OnPaint(e);
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            if (CheckedChanged != null)
            {
                CheckedChanged(this, e);
            }
        }

    }
}
