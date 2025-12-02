using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ReactCRM.Services;

namespace ReactCRM.UI.Components
{
    public class StatCard : Panel
    {
        private string _title;
        private string _value;
        private string _trend;
        private Color _accentColor;
        private string _icon;
        private bool _isHovered;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value
        {
            get => _value;
            set { _value = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Trend
        {
            get => _trend;
            set { _trend = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Icon
        {
            get => _icon;
            set { _icon = value; Invalidate(); }
        }

        public StatCard()
        {
            this.Size = new Size(220, 140);
            this.BackColor = UITheme.Colors.PureWhite;
            this.Padding = new Padding(20);
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Hand;

            // Default values
            _accentColor = UITheme.Colors.SoftBlue;
            _title = "Stat Title";
            _value = "0";
            _trend = "+0%";
            _icon = "📊";

            // Hover effects
            this.MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            this.MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw border
            using (var pen = new Pen(UITheme.Colors.Silver, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }

            // Draw top accent border
            using (var brush = new SolidBrush(_accentColor))
            {
                e.Graphics.FillRectangle(brush, 0, 0, this.Width, 4);
            }

            // Draw Icon
            using (var font = new Font("Segoe UI Emoji", 20, FontStyle.Regular))
            using (var brush = new SolidBrush(UITheme.Colors.SlateGrey))
            {
                e.Graphics.DrawString(_icon, font, brush, 20, 20);
            }

            // Draw Title
            using (var font = UITheme.Fonts.HeadingSmall)
            using (var brush = new SolidBrush(UITheme.Colors.SlateGrey))
            {
                e.Graphics.DrawString(_title, font, brush, 60, 24);
            }

            // Draw Value
            using (var font = new Font(UITheme.Fonts.PrimaryFont, 32, FontStyle.Bold))
            using (var brush = new SolidBrush(UITheme.Colors.Charcoal))
            {
                e.Graphics.DrawString(_value, font, brush, 20, 60);
            }

            // Draw Trend
            if (!string.IsNullOrEmpty(_trend))
            {
                Color trendColor = _trend.StartsWith("+") ? UITheme.Colors.SuccessGreen :
                                   _trend.StartsWith("-") ? UITheme.Colors.ErrorRed :
                                   UITheme.Colors.SlateGrey;

                using (var font = UITheme.Fonts.BodySmall)
                using (var brush = new SolidBrush(trendColor))
                {
                    e.Graphics.DrawString(_trend, font, brush, 20, 110);
                }
            }

            // Draw Hover Shadow (Simulated)
            if (_isHovered)
            {
                // In a real scenario, we'd draw a shadow outside the bounds or use a parent container
                // For now, we can darken the border slightly to indicate focus
                using (var pen = new Pen(UITheme.Colors.SoftBlue, 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            }
        }
    }
}
