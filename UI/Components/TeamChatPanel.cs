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
    /// Team chat panel for internal communication
    /// </summary>
    public class TeamChatPanel : UserControl
    {
        private Panel panelHeader;
        private Label lblTitle;
        private ComboBox cmbChannel;
        private Panel panelMessages;
        private Panel panelInput;
        private TextBox txtMessage;
        private Button btnSend;
        private ChatMessageRepository chatRepo;
        private int currentUserId;
        private string currentChannel = "general";
        private System.Windows.Forms.Timer refreshTimer;
        private Label lblOnlineCount;

        public event EventHandler<ChatMessage> MessageSent;

        public TeamChatPanel()
        {
            InitializeComponent();
            chatRepo = new ChatMessageRepository();
            currentUserId = AuthService.Instance.GetCurrentUserId();
            LoadMessages();

            // Auto-refresh every 5 seconds for real-time feel
            refreshTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            refreshTimer.Tick += (s, e) => LoadMessages();
            refreshTimer.Start();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(450, 600);
            this.BackColor = UITheme.Colors.BackgroundPrimary;
            this.BorderStyle = BorderStyle.FixedSingle;

            // Header
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = UITheme.Colors.HeaderPrimary,
                Padding = new Padding(15, 10, 15, 10)
            };

            lblTitle = new Label
            {
                Text = "💬 Team Chat",
                Font = UITheme.Fonts.HeaderSmall,
                ForeColor = Color.White,
                Location = new Point(15, 8),
                AutoSize = true
            };

            lblOnlineCount = new Label
            {
                Text = "🟢 Online",
                Font = UITheme.Fonts.BodySmall,
                ForeColor = Color.FromArgb(76, 175, 80),
                Location = new Point(15, 32),
                AutoSize = true
            };

            cmbChannel = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(panelHeader.Width - 150, 15),
                Width = 120,
                Font = UITheme.Fonts.BodySmall,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            cmbChannel.Items.AddRange(new object[] { "general", "support", "sales", "development" });
            cmbChannel.SelectedIndex = 0;
            cmbChannel.SelectedIndexChanged += (s, e) =>
            {
                currentChannel = cmbChannel.SelectedItem.ToString();
                LoadMessages();
            };

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblOnlineCount);
            panelHeader.Controls.Add(cmbChannel);

            // Messages panel
            panelMessages = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            // Input panel
            panelInput = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = UITheme.Colors.BackgroundSecondary,
                Padding = new Padding(15, 10, 15, 10)
            };

            txtMessage = new TextBox
            {
                Location = new Point(15, 15),
                Width = 330,
                Height = 40,
                Multiline = true,
                Font = UITheme.Fonts.BodyRegular,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            txtMessage.KeyDown += TxtMessage_KeyDown;
            btnSend = new Button
            {
                Text = "Send",
                Location = new Point(355, 15),
                Size = new Size(70, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = UITheme.Colors.ButtonPrimary,
                ForeColor = Color.White,
                Font = UITheme.Fonts.ButtonFont,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += BtnSend_Click;

            panelInput.Controls.Add(txtMessage);
            panelInput.Controls.Add(btnSend);

            this.Controls.Add(panelMessages);
            this.Controls.Add(panelInput);
            this.Controls.Add(panelHeader);
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            // Send on Enter (use Shift+Enter for new line)

            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                SendMessage();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
                return;

            var message = new ChatMessage
            {
                SenderId = currentUserId,
                Message = txtMessage.Text.Trim(),
                Channel = currentChannel,
                MessageType = "Text",
                SentDate = DateTime.Now
            };

            chatRepo.SendMessage(message);
            MessageSent?.Invoke(this, message);

            txtMessage.Clear();
            LoadMessages();

            // Scroll to bottom
            panelMessages.AutoScrollPosition = new Point(0, panelMessages.DisplayRectangle.Height);
        }

        public void LoadMessages()
        {
            // Clear existing messages
            panelMessages.Controls.Clear();

            var messages = chatRepo.GetChannelMessages(currentChannel, limit: 50);

            if (messages.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = $"No messages in #{currentChannel} yet.\nBe the first to say something!",
                    Font = UITheme.Fonts.BodyRegular,
                    ForeColor = UITheme.Colors.TextSecondary,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                panelMessages.Controls.Add(lblEmpty);
                return;
            }

            int yPos = 10;
            ChatMessage previousMessage = null;

            foreach (var message in messages)
            {
                // Add date separator if day changed
                if (previousMessage == null || message.SentDate.Date != previousMessage.SentDate.Date)
                {
                    var dateSeparator = CreateDateSeparator(message.SentDate, yPos);
                    panelMessages.Controls.Add(dateSeparator);
                    yPos += dateSeparator.Height + 10;
                }

                var messageCard = CreateMessageCard(message, yPos);
                panelMessages.Controls.Add(messageCard);
                yPos += messageCard.Height + 10;

                previousMessage = message;
            }

            // Auto scroll to bottom on first load
            if (panelMessages.Controls.Count > 0)
            {
                this.BeginInvoke(new Action(() =>
                {
                    panelMessages.AutoScrollPosition = new Point(0, panelMessages.DisplayRectangle.Height);
                }));
            }
        }

        private Panel CreateDateSeparator(DateTime date, int yPos)
        {
            var separator = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(panelMessages.Width - 40, 30),
                BackColor = Color.Transparent
            };

            var lblDate = new Label
            {
                Text = GetDateLabel(date),
                Font = new Font(UITheme.Fonts.PrimaryFont, 8, FontStyle.Bold),
                ForeColor = UITheme.Colors.TextTertiary,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            separator.Controls.Add(lblDate);
            return separator;
        }

        private string GetDateLabel(DateTime date)
        {
            if (date.Date == DateTime.Today)
                return "Today";
            if (date.Date == DateTime.Today.AddDays(-1))
                return "Yesterday";
            if ((DateTime.Today - date.Date).Days < 7)
                return date.ToString("dddd");
            return date.ToString("MMMM dd, yyyy");
        }

        private Panel CreateMessageCard(ChatMessage message, int yPos)
        {
            bool isCurrentUser = message.SenderId == currentUserId;

            var card = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(panelMessages.Width - 40, 80),
                BackColor = isCurrentUser ? Color.FromArgb(227, 242, 253) : UITheme.Colors.BackgroundSecondary,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10)
            };

            // Pinned indicator
            if (message.IsPinned)
            {
                var lblPinned = new Label
                {
                    Text = "📌",
                    Font = new Font(UITheme.Fonts.PrimaryFont, 10),
                    Location = new Point(card.Width - 30, 5),
                    Size = new Size(20, 20)
                };
                card.Controls.Add(lblPinned);
            }

            // Sender name
            var lblSender = new Label
            {
                Text = message.SenderName,
                Font = new Font(UITheme.Fonts.PrimaryFont, 9, FontStyle.Bold),
                ForeColor = isCurrentUser ? UITheme.Colors.ButtonPrimary : UITheme.Colors.TextPrimary,
                Location = new Point(10, 8),
                AutoSize = true
            };
            card.Controls.Add(lblSender);

            // Time
            var lblTime = new Label
            {
                Text = message.GetFormattedTime() + (message.IsEdited ? " (edited)" : ""),
                Font = UITheme.Fonts.BodySmall,
                ForeColor = UITheme.Colors.TextTertiary,
                Location = new Point(lblSender.Right + 10, 10),
                AutoSize = true
            };
            card.Controls.Add(lblTime);

            // Message content
            var lblMessage = new Label
            {
                Text = message.Message,
                Font = UITheme.Fonts.BodyRegular,
                ForeColor = UITheme.Colors.TextPrimary,
                Location = new Point(10, 28),
                Size = new Size(card.Width - 25, 45),
                AutoEllipsis = false
            };

            // Calculate required height for message
            using (Graphics g = lblMessage.CreateGraphics())
            {
                SizeF size = g.MeasureString(message.Message, lblMessage.Font, lblMessage.Width);
                int requiredHeight = (int)Math.Ceiling(size.Height);
                card.Height = Math.Max(80, requiredHeight + 40);
                lblMessage.Height = requiredHeight;
            }

            card.Controls.Add(lblMessage);

            // Context menu for message options
            if (isCurrentUser)
            {
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Edit", null, (s, e) => EditMessage(message));
                contextMenu.Items.Add("Delete", null, (s, e) => DeleteMessage(message));
                contextMenu.Items.Add("Pin/Unpin", null, (s, e) => TogglePinMessage(message));
                card.ContextMenuStrip = contextMenu;
            }

            return card;
        }

        private void EditMessage(ChatMessage message)
        {
            var editForm = new Form
            {
                Text = "Edit Message",
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var txtEdit = new TextBox
            {
                Text = message.Message,
                Multiline = true,
                Location = new Point(20, 20),
                Size = new Size(340, 80),
                Font = UITheme.Fonts.BodyRegular
            };

            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(200, 120),
                Size = new Size(80, 30)
            };
            btnSave.Click += (s, e) =>
            {
                chatRepo.EditMessage(message.Id, txtEdit.Text);
                editForm.Close();
                LoadMessages();
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(290, 120),
                Size = new Size(70, 30)
            };
            btnCancel.Click += (s, e) => editForm.Close();

            editForm.Controls.Add(txtEdit);
            editForm.Controls.Add(btnSave);
            editForm.Controls.Add(btnCancel);
            editForm.ShowDialog();
        }

        private void DeleteMessage(ChatMessage message)
        {
            if (MessageBox.Show("Delete this message?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                chatRepo.DeleteMessage(message.Id);
                LoadMessages();
            }
        }

        private void TogglePinMessage(ChatMessage message)
        {
            chatRepo.TogglePin(message.Id);
            LoadMessages();
        }

        public void SetChannel(string channel)
        {
            currentChannel = channel;
            cmbChannel.SelectedItem = channel;
            LoadMessages();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                LoadMessages();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                refreshTimer?.Stop();
                refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
