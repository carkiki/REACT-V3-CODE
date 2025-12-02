using System;

using System.Drawing;

using System.Windows.Forms;

using ReactCRM.Services;



namespace ReactCRM.UI.Login

{

    /// <summary>

    /// Loading screen form with spinner animation

    /// </summary>

    public class LoadingForm : Form

    {

        private Label lblLoading;

        private ProgressBar progressBar;

        private System.Windows.Forms.Timer animationTimer;

        private int dotCount = 0;



        public LoadingForm()

        {

            InitializeComponent();

            StartAnimation();

        }



        private void InitializeComponent()

        {

            this.Text = "";

            this.Size = new Size(400, 200);

            this.StartPosition = FormStartPosition.CenterParent;

            this.FormBorderStyle = FormBorderStyle.None;

            this.BackColor = UITheme.Colors.BackgroundPrimary;

            this.ShowInTaskbar = false;

            this.ControlBox = false;



            // Add border

            this.Paint += (s, e) =>

            {

                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle,

                    UITheme.Colors.HeaderPrimary, ButtonBorderStyle.Solid);

            };



            // Loading Label

            lblLoading = new Label

            {

                Text = "Loading",

                Font = new Font(UITheme.Fonts.PrimaryFont, 14, FontStyle.Bold),

                ForeColor = UITheme.Colors.TextPrimary,

                AutoSize = true,

                Location = new Point(155, 60)

            };



            // Progress Bar

            progressBar = new ProgressBar

            {

                Location = new Point(80, 110),

                Size = new Size(240, 20),

                Style = ProgressBarStyle.Marquee,

                MarqueeAnimationSpeed = 30,

                ForeColor = UITheme.Colors.HeaderPrimary

            };



            this.Controls.Add(lblLoading);

            this.Controls.Add(progressBar);

        }



        private void StartAnimation()

        {

            animationTimer = new System.Windows.Forms.Timer();

            animationTimer.Interval = 500; // Update every 500ms

            animationTimer.Tick += (s, e) =>

            {

                dotCount = (dotCount + 1) % 4;

                lblLoading.Text = "Loading" + new string('.', dotCount);

            };

            animationTimer.Start();

        }



        protected override void OnFormClosing(FormClosingEventArgs e)

        {

            if (animationTimer != null)

            {

                animationTimer.Stop();

                animationTimer.Dispose();

            }

            base.OnFormClosing(e);

        }

    }

}