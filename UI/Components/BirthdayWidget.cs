using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;

namespace ReactCRM.UI.Components
{
    /// <summary>
    /// Widget to display upcoming client birthdays
    /// </summary>
    public class BirthdayWidget : UserControl
    {
        private Panel panelHeader;
        private Label lblTitle;
        private Panel panelBirthdays;
        private ClientRepository clientRepo;

        public event EventHandler<Client> ClientClicked;

        public BirthdayWidget()
        {
            InitializeComponent();
            clientRepo = new ClientRepository();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 250);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;

            // Header
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(142, 68, 173), // Purple theme
                Padding = new Padding(15, 8, 15, 8)
            };

            lblTitle = new Label
            {
                Text = "🎂 Upcoming Birthdays",
                Font = UITheme.Fonts.HeaderSmall,
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            panelHeader.Controls.Add(lblTitle);

            // Birthdays container
            panelBirthdays = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            this.Controls.Add(panelBirthdays);
            this.Controls.Add(panelHeader);
        }

        public void SetTitle(string title)
        {
            lblTitle.Text = title;
        }

        public void LoadBirthdays()
        {
            panelBirthdays.Controls.Clear();

            // Get all clients
            var allClients = clientRepo.GetAllClients();
            var today = DateTime.Today;

            // Filter clients with birthdays in the next 30 days
            var upcomingBirthdays = allClients
                .Where(c => c.DOB.HasValue)
                .Select(c => new
                {
                    Client = c,
                    DaysUntil = GetDaysUntilBirthday(c.DOB.Value, today),
                    NextBirthday = GetNextBirthday(c.DOB.Value, today)
                })
                .Where(x => x.DaysUntil >= 0 && x.DaysUntil <= 30)
                .OrderBy(x => x.DaysUntil)
                .Take(5)
                .ToList();

            if (upcomingBirthdays.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "No upcoming birthdays\nin the next 30 days.",
                    Font = UITheme.Fonts.BodyRegular,
                    ForeColor = UITheme.Colors.TextSecondary,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                panelBirthdays.Controls.Add(lblEmpty);
                return;
            }

            int yPos = 10;
            foreach (var item in upcomingBirthdays)
            {
                var card = CreateBirthdayCard(item.Client, item.DaysUntil, item.NextBirthday, yPos);
                panelBirthdays.Controls.Add(card);
                yPos += card.Height + 10;
            }
        }

        private Panel CreateBirthdayCard(Client client, int daysUntil, DateTime nextBirthday, int yPos)
        {
            var card = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(panelBirthdays.Width - 40, 60),
                BorderStyle = BorderStyle.None,
                BackColor = daysUntil == 0 ? Color.FromArgb(255, 243, 224) : Color.White,
                Cursor = Cursors.Hand,
                Tag = client
            };

            // Birthday icon/color bar
            var colorBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(4, card.Height),
                BackColor = daysUntil == 0 ? Color.FromArgb(255, 152, 0) : Color.FromArgb(142, 68, 173)
            };
            card.Controls.Add(colorBar);

            // Birthday cake emoji
            var lblIcon = new Label
            {
                Text = "🎂",
                Font = new Font(UITheme.Fonts.PrimaryFont, 20),
                Location = new Point(15, 15),
                Size = new Size(40, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblIcon);

            // Client name
            var lblName = new Label
            {
                Text = client.Name,
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextPrimary,
                Location = new Point(60, 10),
                Size = new Size(card.Width - 150, 20),
                AutoEllipsis = true
            };
            card.Controls.Add(lblName);

            // Age display (calculate age on next birthday)
            int age = nextBirthday.Year - client.DOB.Value.Year;
            var lblAge = new Label
            {
                Text = $"Turns {age}",
                Font = UITheme.Fonts.BodySmall,
                ForeColor = UITheme.Colors.TextSecondary,
                Location = new Point(60, 32),
                Size = new Size(100, 18)
            };
            card.Controls.Add(lblAge);

            // Days until birthday badge
            string daysText = daysUntil == 0 ? "Today!" :
                             daysUntil == 1 ? "Tomorrow" :
                             $"{daysUntil} days";

            var lblDays = new Label
            {
                Text = daysText,
                Font = UITheme.Fonts.BodySmall,
                ForeColor = Color.White,
                BackColor = daysUntil == 0 ? Color.FromArgb(255, 152, 0) : Color.FromArgb(142, 68, 173),
                Location = new Point(card.Width - 90, 18),
                Size = new Size(80, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblDays);

            // Click event
            card.Click += (s, e) => ClientClicked?.Invoke(this, client);
            foreach (Control ctrl in card.Controls.OfType<Label>())
            {
                ctrl.Click += (s, e) => ClientClicked?.Invoke(this, client);
            }

            // Hover effect
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = UITheme.Colors.BackgroundTertiary;
            };

            card.MouseLeave += (s, e) =>
            {
                card.BackColor = daysUntil == 0 ? Color.FromArgb(255, 243, 224) : Color.White;
            };

            return card;
        }

        private int GetDaysUntilBirthday(DateTime dob, DateTime today)
        {
            var nextBirthday = GetNextBirthday(dob, today);
            return (nextBirthday - today).Days;
        }

        private DateTime GetNextBirthday(DateTime dob, DateTime today)
        {
            int year = today.Year;

            int month = dob.Month;

            int day = dob.Day;



            // Handle leap year birthdays (Feb 29)

            // If born on Feb 29 but current year is not a leap year, use March 1

            if (month == 2 && day == 29 && !DateTime.IsLeapYear(year))

            {

                month = 3;

                day = 1;

            }



            var thisYearBirthday = new DateTime(year, month, day);



            if (thisYearBirthday < today)

            {

                // Birthday already passed this year, check next year

                int nextYear = year + 1;

                int nextMonth = dob.Month;

                int nextDay = dob.Day;



                // Handle leap year for next year too

                if (nextMonth == 2 && nextDay == 29 && !DateTime.IsLeapYear(nextYear))

                {

                    nextMonth = 3;

                    nextDay = 1;

                }



                return new DateTime(nextYear, nextMonth, nextDay);
            }

            return thisYearBirthday;
        }
    }
}