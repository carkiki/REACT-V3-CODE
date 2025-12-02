using System;
using System.Drawing;
using System.Windows.Forms;

namespace ReactCRM.Services
{
    /// <summary>
    /// Modern and professional UI theme system for REACT CRM
    /// Provides consistent color scheme and styling across the entire application
    /// </summary>
    public static class UITheme
    {
        // Primary Colors - Modern Professional Palette (Purple & Green Theme)

        public static class Colors
        {
            // ===== PRIMARY COLORS - Professional Elegance =====
            public static Color SoftBlue => Color.FromArgb(74, 144, 226);          // #4A90E2 - Primary actions
            public static Color DeepNavy => Color.FromArgb(44, 62, 80);            // #2C3E50 - Headers, text
            public static Color SlateBlue => Color.FromArgb(93, 109, 126);         // #5D6D7E - Secondary elements

            // ===== NEUTRAL COLORS =====
            public static Color Charcoal => Color.FromArgb(45, 52, 54);            // #2D3436 - Primary text
            public static Color SlateGrey => Color.FromArgb(99, 110, 114);         // #636E72 - Secondary text
            public static Color LightGrey => Color.FromArgb(149, 165, 166);        // #95A5A6 - Tertiary text
            public static Color Silver => Color.FromArgb(223, 230, 233);           // #DFE6E9 - Borders

            // ===== BACKGROUND COLORS =====
            public static Color PureWhite => Color.FromArgb(255, 255, 255);        // #FFFFFF - Cards, surfaces
            public static Color OffWhite => Color.FromArgb(248, 249, 250);         // #F8F9FA - Main background
            public static Color BackgroundLight => Color.FromArgb(241, 243, 245);  // #F1F3F5 - Secondary BG
            public static Color BackgroundSoft => Color.FromArgb(233, 236, 239);   // #E9ECEF - Tertiary BG

            // ===== ACCENT COLORS =====
            public static Color SuccessGreen => Color.FromArgb(0, 184, 148);       // #00B894 - Success states
            public static Color WarningAmber => Color.FromArgb(253, 203, 110);     // #FDCB6E - Warnings
            public static Color ErrorRed => Color.FromArgb(225, 112, 85);          // #E17055 - Errors, alerts
            public static Color InfoBlue => Color.FromArgb(116, 185, 255);         // #74B9FF - Information

            // ===== SIDEBAR & NAVIGATION =====
            public static Color SidebarDark => Color.FromArgb(30, 39, 46);         // #1E272E - Sidebar background
            public static Color SidebarHover => Color.FromArgb(47, 54, 64);        // #2F3640 - Hover state
            public static Color SidebarActive => Color.FromArgb(74, 144, 226);     // #4A90E2 - Active state
            public static Color SidebarText => Color.FromArgb(241, 243, 245);      // #F1F3F5 - Sidebar text

            // ===== LEGACY COMPATIBILITY (mapped to new colors) =====
            public static Color HeaderPrimary => SoftBlue;
            public static Color HeaderSecondary => DeepNavy;
            public static Color HeaderHover => InfoBlue;
            public static Color SidebarBackground => SidebarDark;
            public static Color ButtonPrimary => SoftBlue;
            public static Color ButtonSuccess => SuccessGreen;
            public static Color ButtonWarning => WarningAmber;
            public static Color ButtonDanger => ErrorRed;
            public static Color ButtonSecondary => BackgroundTertiary;
            public static Color ButtonDisabled => Silver;
            public static Color StatusSuccess => SuccessGreen;
            public static Color StatusWarning => WarningAmber;
            public static Color StatusError => ErrorRed;
            public static Color StatusInfo => InfoBlue;
            public static Color BackgroundPrimary => OffWhite;
            public static Color BackgroundSecondary => BackgroundLight;
            public static Color BackgroundTertiary => BackgroundSoft;
            public static Color SurfaceWhite => PureWhite;
            public static Color TextPrimary => Charcoal;
            public static Color TextSecondary => SlateGrey;
            public static Color TextTertiary => LightGrey;
            public static Color TextInverse => PureWhite;
            public static Color BorderPrimary => Silver;
            public static Color BorderSecondary => SlateGrey;
            public static Color BorderDark => DeepNavy;
            public static Color CardBackground => PureWhite;
            public static Color CardBorder => Silver;
            public static Color CardShadow => Color.FromArgb(20, 0, 0, 0);         // 8% opacity black

            // ===== DATA GRID COLORS =====
            public static Color DataGridHeaderBackground => DeepNavy;              // #2C3E50
            public static Color DataGridHeaderText => PureWhite;
            public static Color DataGridRowAlternate => OffWhite;                  // #F8F9FA
            public static Color DataGridRowHover => BackgroundLight;               // #F1F3F5
            public static Color DataGridRowSelected => Color.FromArgb(25, 74, 144, 226); // Soft Blue 10% opacity
            public static Color DataGridBorder => Silver;                          // #DFE6E9
            public static Color DataGridRowSelectedBorder => SoftBlue;             // #4A90E2
        }

        // Typography
        public static class Fonts
        {
            public static string PrimaryFont => "Segoe UI";
            public static string MonoFont => "Consolas";

            // Display Fonts (Page titles, major headers)
            public static Font DisplayLarge => new Font(PrimaryFont, 24, FontStyle.Bold);       // Page titles
            public static Font DisplayMedium => new Font(PrimaryFont, 20, FontStyle.Bold);     // Section headers

            // Heading Fonts (Card titles, subsections)
            public static Font HeadingLarge => new Font(PrimaryFont, 18, FontStyle.Bold);      // Card titles
            public static Font HeadingMedium => new Font(PrimaryFont, 16, FontStyle.Bold);     // Subsection headers
            public static Font HeadingSmall => new Font(PrimaryFont, 14, FontStyle.Regular);   // Widget titles

            // Body Fonts (Content, standard text)
            public static Font BodyLarge => new Font(PrimaryFont, 14, FontStyle.Regular);      // Primary content
            public static Font BodyRegular => new Font(PrimaryFont, 13, FontStyle.Regular);    // Standard text
            public static Font BodySmall => new Font(PrimaryFont, 12, FontStyle.Regular);      // Secondary text

            // Caption Fonts (Labels, metadata)
            public static Font Caption => new Font(PrimaryFont, 11, FontStyle.Regular);        // Labels, hints
            public static Font Tiny => new Font(PrimaryFont, 10, FontStyle.Regular);           // Metadata, timestamps

            // Specialized Fonts
            public static Font ButtonFont => new Font(PrimaryFont, 13, FontStyle.Regular);     // Button text
            public static Font LabelFont => new Font(PrimaryFont, 12, FontStyle.Regular);      // Form labels
            public static Font MonoRegular => new Font(MonoFont, 13, FontStyle.Regular);       // Code, data
            public static Font MonoLarge => new Font(MonoFont, 16, FontStyle.Regular);         // Large data display

            // Legacy Compatibility
            public static Font HeaderLarge => HeadingLarge;
            public static Font HeaderMedium => HeadingMedium;
            public static Font HeaderSmall => HeadingSmall;
        }

        // Sizing and Spacing (8px base unit)
        public static class Spacing
        {
            // Base Spacing Units
            public static int XXSmall => 4;      // Tight spacing (icon-text gaps)
            public static int XSmall => 8;       // Minimal spacing (inline elements)
            public static int Small => 12;       // Small spacing (compact layouts)
            public static int Medium => 16;      // Standard spacing (default gaps)
            public static int Large => 24;       // Large spacing (section separation)
            public static int XLarge => 32;      // Extra large (major sections)
            public static int XXLarge => 48;     // Maximum spacing (page sections)

            // Component Dimensions
            public static int HeaderHeight => 60;              // Top navigation bar
            public static int SidebarWidth => 240;             // Expanded sidebar
            public static int SidebarCollapsedWidth => 60;     // Collapsed sidebar
            public static int ButtonHeight => 40;              // Standard button
            public static int ButtonHeightLarge => 48;         // Large button
            public static int InputHeight => 38;               // Form inputs
            public static int CardPadding => 20;               // Card internal padding
            public static int SectionGap => 16;                // Gap between sections
        }

        // Border Radius
        public static class CornerRadius
        {
            public static int None => 0;         // Sharp edges (rare use)
            public static int Small => 4;        // Buttons, inputs, tags
            public static int Medium => 6;       // Cards, panels
            public static int Large => 8;        // Modals, large containers
            public static int XLarge => 12;      // Special elements
            public static int Pill => 999;       // Badges, status indicators
        }

        // Animation and Transitions
        public static class Animations
        {
            public static int FastDuration => 150;        // Hover effects, button states
            public static int StandardDuration => 250;    // Panel slides, dropdowns
            public static int SmoothDuration => 350;      // Sidebar collapse, modal open
            public static int SlowDuration => 500;        // Page transitions, complex animations

            // Easing function approximation (for reference, actual implementation would use Timer)
            // cubic-bezier(0.4, 0.0, 0.2, 1) - Material Design standard
        }

        // Shadow Effects (5 elevation levels)
        public static class Shadows
        {
            // Shadow Level 1: Subtle (cards at rest)
            public static Color Shadow1Color => Color.FromArgb(20, 0, 0, 0);  // 8% opacity
            public static int Shadow1Blur => 3;
            public static int Shadow1OffsetY => 1;

            // Shadow Level 2: Card (elevated cards)
            public static Color Shadow2Color => Color.FromArgb(26, 0, 0, 0);  // 10% opacity
            public static int Shadow2Blur => 8;
            public static int Shadow2OffsetY => 2;

            // Shadow Level 3: Elevated (hover states)
            public static Color Shadow3Color => Color.FromArgb(31, 0, 0, 0);  // 12% opacity
            public static int Shadow3Blur => 16;
            public static int Shadow3OffsetY => 4;

            // Shadow Level 4: Modal (dialogs, overlays)
            public static Color Shadow4Color => Color.FromArgb(38, 0, 0, 0);  // 15% opacity
            public static int Shadow4Blur => 24;
            public static int Shadow4OffsetY => 8;

            // Shadow Level 5: Dropdown (menus, dropdowns)
            public static Color Shadow5Color => Color.FromArgb(46, 0, 0, 0);  // 18% opacity
            public static int Shadow5Blur => 32;
            public static int Shadow5OffsetY => 12;

            // Legacy compatibility
            public static Color ShadowColor => Shadow2Color;
            public static int ShadowElevation => 2;
        }

        /// <summary>
        /// Applies a modern button style with smooth appearance
        /// </summary>
        public static void ApplyButtonStyle(Button button, ButtonType buttonType = ButtonType.Primary)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = Fonts.ButtonFont;
            button.Height = Spacing.ButtonHeight;
            button.Cursor = Cursors.Hand;

            // Apply color based on button type
            var (backColor, foreColor) = buttonType switch
            {
                ButtonType.Primary => (Colors.ButtonPrimary, Colors.TextInverse),
                ButtonType.Success => (Colors.ButtonSuccess, Colors.TextInverse),
                ButtonType.Warning => (Colors.ButtonWarning, Colors.TextInverse),
                ButtonType.Danger => (Colors.ButtonDanger, Colors.TextInverse),
                ButtonType.Secondary => (Colors.BackgroundTertiary, Colors.TextPrimary),
                ButtonType.Disabled => (Colors.ButtonDisabled, Colors.TextTertiary),
                _ => (Colors.ButtonPrimary, Colors.TextInverse)
            };

            button.BackColor = backColor;
            button.ForeColor = foreColor;

            // Add hover effect
            button.MouseEnter += (s, e) =>
            {
                if (buttonType != ButtonType.Disabled)
                {
                    button.BackColor = DarkenColor(button.BackColor, 0.2);
                }
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = (buttonType) switch
                {
                    ButtonType.Primary => Colors.ButtonPrimary,
                    ButtonType.Success => Colors.ButtonSuccess,
                    ButtonType.Warning => Colors.ButtonWarning,
                    ButtonType.Danger => Colors.ButtonDanger,
                    ButtonType.Secondary => Colors.BackgroundTertiary,
                    ButtonType.Disabled => Colors.ButtonDisabled,
                    _ => Colors.ButtonPrimary
                };
            };
        }

        /// <summary>
        /// Applies header panel style
        /// </summary>
        public static void ApplyHeaderStyle(Panel headerPanel)
        {
            headerPanel.BackColor = Colors.HeaderPrimary;
            headerPanel.Height = Spacing.HeaderHeight;
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Padding = new Padding(Spacing.Medium);
        }

        /// <summary>
        /// Applies sidebar panel style
        /// </summary>
        public static void ApplySidebarStyle(Panel sidebarPanel, bool collapsed = false)
        {
            sidebarPanel.BackColor = Colors.SidebarBackground;
            sidebarPanel.Width = collapsed ? Spacing.SidebarCollapsedWidth : Spacing.SidebarWidth;
            sidebarPanel.Dock = DockStyle.Left;
        }

        /// <summary>
        /// Applies modern DataGridView style
        /// </summary>
        public static void ApplyDataGridStyle(DataGridView grid)
        {
            grid.BackgroundColor = Colors.BackgroundPrimary;
            grid.GridColor = Colors.DataGridBorder;
            grid.BorderStyle = BorderStyle.None;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.Font = Fonts.BodyRegular;

            // Header style
            grid.ColumnHeadersDefaultCellStyle.BackColor = Colors.DataGridHeaderBackground;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Colors.DataGridHeaderText;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(Fonts.PrimaryFont, 10, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Row style
            grid.DefaultCellStyle.BackColor = Colors.SurfaceWhite;
            grid.DefaultCellStyle.ForeColor = Colors.TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = Colors.HeaderSecondary;
            grid.DefaultCellStyle.SelectionForeColor = Colors.TextInverse;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Colors.DataGridRowAlternate;

            grid.RowTemplate.Height = 32;
        }

        /// <summary>
        /// Applies panel card style
        /// </summary>
        public static void ApplyPanelCardStyle(Panel panel)
        {
            panel.BackColor = Colors.CardBackground;
            panel.BorderStyle = BorderStyle.FixedSingle;
        }

        /// <summary>
        /// Darkens a color by a specified factor
        /// </summary>
        public static Color DarkenColor(Color color, double factor)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, (int)(color.R * (1 - factor))),
                Math.Max(0, (int)(color.G * (1 - factor))),
                Math.Max(0, (int)(color.B * (1 - factor)))
            );
        }

        /// <summary>
        /// Lightens a color by a specified factor
        /// </summary>
        public static Color LightenColor(Color color, double factor)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, (int)(color.R + (255 - color.R) * factor)),
                Math.Min(255, (int)(color.G + (255 - color.G) * factor)),
                Math.Min(255, (int)(color.B + (255 - color.B) * factor))
            );
        }

        /// <summary>
        /// Creates a color with specified opacity
        /// </summary>
        public static Color WithOpacity(Color color, double opacity)
        {
            int alpha = (int)(255 * Math.Max(0, Math.Min(1, opacity)));
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        /// <summary>
        /// Creates a semi-transparent overlay color
        /// </summary>
        public static Color CreateOverlay(double opacity = 0.4)
        {
            return Color.FromArgb((int)(255 * opacity), 0, 0, 0);
        }

        /// <summary>
        /// Applies modern card style with shadow effect
        /// </summary>
        public static void ApplyCardStyle(Panel panel, int shadowLevel = 2)
        {
            panel.BackColor = Colors.CardBackground;
            panel.BorderStyle = BorderStyle.None;
            panel.Padding = new Padding(Spacing.CardPadding);
            // Note: WinForms doesn't support box-shadow directly
            // Shadow would need to be implemented via custom painting
        }

        /// <summary>
        /// Applies modern input/textbox style
        /// </summary>
        public static void ApplyInputStyle(TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Colors.OffWhite;
            textBox.ForeColor = Colors.TextPrimary;
            textBox.Font = Fonts.BodyRegular;
            textBox.Height = Spacing.InputHeight;
        }

        /// <summary>
        /// Applies modern label style
        /// </summary>
        public static void ApplyLabelStyle(Label label, bool isSecondary = false)
        {
            label.Font = Fonts.LabelFont;
            label.ForeColor = isSecondary ? Colors.TextSecondary : Colors.TextPrimary;
            label.BackColor = Color.Transparent;
        }

        /// <summary>
        /// Creates a gradient brush for backgrounds (requires System.Drawing)
        /// </summary>
        public static System.Drawing.Drawing2D.LinearGradientBrush CreateGradientBrush(
            Rectangle bounds, Color startColor, Color endColor, float angle = 90f)
        {
            return new System.Drawing.Drawing2D.LinearGradientBrush(
                bounds, startColor, endColor, angle);
        }

        /// <summary>
        /// Applies hover effect to a control
        /// </summary>
        public static void AddHoverEffect(Control control, Color normalColor, Color hoverColor)
        {
            control.MouseEnter += (s, e) => control.BackColor = hoverColor;
            control.MouseLeave += (s, e) => control.BackColor = normalColor;
        }

        /// <summary>
        /// Enables smooth double buffering for a control to reduce flicker
        /// </summary>
        public static void EnableDoubleBuffering(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
        }
    }

    /// <summary>
    /// Button type enumeration for styling
    /// </summary>
    public enum ButtonType
    {
        Primary,
        Success,
        Warning,
        Danger,
        Secondary,
        Disabled
    }
}
