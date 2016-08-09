using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace UnitTestSessionsPanel.Controls
{
    internal class CustomProgressBar : Control
    {
        private int maximum = 100;
        [DefaultValue(100)]
        public int Maximum
        {
            get { return maximum; }
            set
            {
                if (this.maximum != value)
                {
                    this.maximum = value;
                    Invalidate();
                }
            }
        }

        private int value = 0;
        [DefaultValue(0)]
        public int Value
        {
            get { return value; }
            set
            {
                if (this.value != value)
                {
                    if (value < 0)
                    {
                        this.value = 0;
                    }
                    else if (value > maximum)
                    {
                        this.value = maximum;
                    }
                    else
                    {
                        this.value = value;
                    }
                    Invalidate();
                }
            }
        }

        private int radius = 20;
        [DefaultValue(20)]
        public int Radius
        {
            get { return radius; }
            set
            {
                if (this.radius != value)
                {
                    this.radius = value;
                    Invalidate();
                }
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, 23);
            }
        }

        public CustomProgressBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            var baseRect = new Rectangle(Padding.Left, Padding.Top, ClientRectangle.Width - Padding.Horizontal, ClientRectangle.Height - Padding.Vertical);

            var oldSmoothingMode = e.Graphics.SmoothingMode;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var foregroundBrush = new SolidBrush(Color.Black))
            {
                using (var gp = GenerateRoundedRectangle(baseRect, radius))
                {
                    e.Graphics.FillPath(foregroundBrush, gp);
                }
            }
            using (var foregroundBrush = new SolidBrush(Color.FromArgb(80, ForeColor)))
            {
                using (var gp = GenerateRoundedRectangle(baseRect, radius))
                {
                    e.Graphics.FillPath(foregroundBrush, gp);
                }
            }
            e.Graphics.SmoothingMode = oldSmoothingMode;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var oldSmoothingMode = e.Graphics.SmoothingMode;
            var baseRect = new Rectangle(Padding.Left, Padding.Top, ClientRectangle.Width - Padding.Horizontal, ClientRectangle.Height - Padding.Vertical);
            baseRect.Inflate(-1, -1);
            float fillWidth = Value * (baseRect.Width - 2) / 100;

            if (fillWidth <= 0) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var foregroundBrush = new LinearGradientBrush(baseRect, ControlPaint.Light(ForeColor, 0.8f), ForeColor, LinearGradientMode.Vertical))
            {
                using (var gp = GenerateRoundedRectangle(baseRect, radius))
                {
                    e.Graphics.SetClip(new RectangleF(baseRect.X, baseRect.Y, fillWidth, baseRect.Height));
                    e.Graphics.FillPath(foregroundBrush, gp);
                }
            }
            e.Graphics.SmoothingMode = oldSmoothingMode;
        }

        private static GraphicsPath GenerateRoundedRectangle(RectangleF rectangle, float radius)
        {
            var gp = new GraphicsPath();

            gp.StartFigure();

            if (radius <= 0)
            {
                gp.AddRectangle(rectangle);
            }
            else if (radius >= Math.Min(rectangle.Width, rectangle.Height))
            {
                if (rectangle.Width > rectangle.Height)
                {
                    var size = new SizeF(rectangle.Height - 1, rectangle.Height - 1);
                    var rect = new RectangleF(rectangle.Location, size);
                    rect.X += 1;
                    gp.AddArc(rect, 90, 180);
                    rect.X = rectangle.Right - rectangle.Height - 1;
                    gp.AddArc(rect, 270, 180);
                }
                else if (rectangle.Width < rectangle.Height)
                {
                    var size = new SizeF(rectangle.Width, rectangle.Width);
                    var rect = new RectangleF(rectangle.Location, size);
                    gp.AddArc(rect, 180, 180);
                    rect.Y = rectangle.Bottom - rectangle.Width;
                    gp.AddArc(rect, 0, 180);
                }
                else gp.AddEllipse(rectangle);
            }
            else
            {
                gp.AddArc(rectangle.X + 1, rectangle.Y, radius, radius, 180, 90);
                gp.AddArc(rectangle.X + rectangle.Width - radius - 2, rectangle.Y, radius, radius, 270, 90);
                gp.AddArc(rectangle.X + rectangle.Width - radius - 2, rectangle.Y + rectangle.Height - radius - 1, radius, radius, 0, 90);
                gp.AddArc(rectangle.X + 1, rectangle.Y + rectangle.Height - radius - 1, radius, radius, 90, 90);
            }

            gp.CloseFigure();

            return gp;
        }
    }

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
    public class ToolStripCustomProgressBar : ToolStripControlHost
    {
        private CustomProgressBar progressBar;

        [DefaultValue(100)]
        public int Maximum
        {
            get { return progressBar.Maximum; }
            set
            {
                progressBar.Maximum = value;
            }
        }

        private int value;
        [DefaultValue(0)]
        public int Value
        {
            get { return progressBar.Value; }
            set
            {
                progressBar.Value = value;
            }
        }

        private int radius;
        [DefaultValue(20)]
        public int Radius
        {
            get { return progressBar.Radius; }
            set
            {
                progressBar.Radius = value;
            }
        }

        public ToolStripCustomProgressBar()
            : base(new CustomProgressBar())
        {
            this.progressBar = (CustomProgressBar)this.Control;
        }
    }
}