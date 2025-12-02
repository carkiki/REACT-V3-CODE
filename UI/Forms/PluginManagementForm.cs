using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReactCRM.Models;
using ReactCRM.Plugins;
using ReactCRM.Services;

namespace ReactCRM.UI.Forms
{
    /// <summary>
    /// Complete GUI for managing plugins:
    /// - Import plugins from DLL files
    /// - Enable/disable plugins
    /// - Configure auto-start
    /// - Execute plugins manually
    /// - Uninstall plugins
    /// </summary>
    public class PluginManagementForm : Form
    {
        private Panel panelHeader;
        private Label lblTitle;
        private Button btnImport;
        private Button btnRefresh;
        private Panel panelPlugins;
        private PluginManager pluginManager;

        public PluginManagementForm()
        {
            pluginManager = PluginManager.Instance;
            InitializeComponent();
            LoadPluginsList();
        }

        private void InitializeComponent()
        {
            this.Text = "Plugin Management";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = UITheme.Colors.BackgroundPrimary;

            // Header
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = UITheme.Colors.HeaderPrimary,
                Padding = new Padding(20)
            };

            lblTitle = new Label
            {
                Text = "🔌 Plugin Management",
                Font = UITheme.Fonts.HeaderLarge,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            btnImport = new Button
            {
                Text = "Import Plugin",
                Font = UITheme.Fonts.BodyRegular,
                Size = new Size(140, 40),
                Location = new Point(650, 20),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.Click += BtnImport_Click;

            btnRefresh = new Button
            {
                Text = "Reload",
                Font = UITheme.Fonts.BodyRegular,
                Size = new Size(100, 40),
                Location = new Point(800, 20),
                BackColor = UITheme.Colors.ButtonPrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 1;
            btnRefresh.FlatAppearance.BorderColor = Color.White;
            btnRefresh.Click += BtnRefresh_Click;

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(btnImport);
            panelHeader.Controls.Add(btnRefresh);

            // Plugins container
            panelPlugins = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = UITheme.Colors.BackgroundPrimary,
                Padding = new Padding(20)
            };

            this.Controls.Add(panelPlugins);
            this.Controls.Add(panelHeader);
        }

        private void LoadPluginsList()
        {
            panelPlugins.Controls.Clear();

            var plugins = pluginManager.GetPlugins();
            var allConfigs = new Database.PluginConfigRepository().GetAllPluginConfigs();

            if (allConfigs.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "No plugins found.\n\nClick 'Import Plugin' to add your first plugin.",
                    Font = UITheme.Fonts.BodyRegular,
                    ForeColor = UITheme.Colors.TextSecondary,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                panelPlugins.Controls.Add(lblEmpty);
                return;
            }

            int yPos = 20;
            foreach (var config in allConfigs)
            {
                var plugin = pluginManager.GetPluginByName(config.PluginName);
                var card = CreatePluginCard(plugin, config, yPos);
                panelPlugins.Controls.Add(card);
                yPos += card.Height + 15;
            }
        }

        private Panel CreatePluginCard(IReactCrmPlugin plugin, PluginConfig config, int yPos)
        {
            var card = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(panelPlugins.Width - 60, 140),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Plugin icon
            var lblIcon = new Label
            {
                Text = plugin?.Icon ?? "📦",
                Font = new Font(UITheme.Fonts.PrimaryFont, 32),
                Location = new Point(15, 20),
                Size = new Size(60, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblIcon);

            // Plugin name
            var lblName = new Label
            {
                Text = config.PluginName,
                Font = UITheme.Fonts.HeaderSmall,
                ForeColor = UITheme.Colors.TextPrimary,
                Location = new Point(90, 15),
                Size = new Size(300, 25)
            };
            card.Controls.Add(lblName);

            // Version and author
            var lblVersion = new Label
            {
                Text = plugin != null ? $"v{plugin.Version} by {plugin.Author}" : "Not loaded",
                Font = UITheme.Fonts.BodySmall,
                ForeColor = UITheme.Colors.TextSecondary,
                Location = new Point(90, 40),
                Size = new Size(300, 20)
            };
            card.Controls.Add(lblVersion);

            // Description
            var lblDescription = new Label
            {
                Text = plugin?.Description ?? "Plugin not loaded",
                Font = UITheme.Fonts.BodySmall,
                ForeColor = UITheme.Colors.TextSecondary,
                Location = new Point(90, 62),
                Size = new Size(500, 35),
                AutoEllipsis = true
            };
            card.Controls.Add(lblDescription);

            // DLL Path
            var lblPath = new Label
            {
                Text = $"DLL: {config.DllPath}",
                Font = new Font(UITheme.Fonts.PrimaryFont, 7),
                ForeColor = UITheme.Colors.TextTertiary,
                Location = new Point(90, 100),
                Size = new Size(500, 15),
                AutoEllipsis = true
            };
            card.Controls.Add(lblPath);

            // Last loaded date
            var lblLastLoaded = new Label
            {
                Text = config.LastLoaded.HasValue ? $"Last loaded: {config.LastLoaded.Value:yyyy-MM-dd HH:mm}" : "Never loaded",
                Font = new Font(UITheme.Fonts.PrimaryFont, 7),
                ForeColor = UITheme.Colors.TextTertiary,
                Location = new Point(90, 118),
                Size = new Size(300, 15)
            };
            card.Controls.Add(lblLastLoaded);

            // Enabled checkbox
            var chkEnabled = new CheckBox
            {
                Text = "Enabled",
                Font = UITheme.Fonts.BodySmall,
                Location = new Point(600, 15),
                Size = new Size(90, 25),
                Checked = config.IsEnabled,
                Tag = config.PluginName
            };
            chkEnabled.CheckedChanged += (s, e) =>
            {
                pluginManager.SetPluginEnabled((string)chkEnabled.Tag, chkEnabled.Checked);
            };
            card.Controls.Add(chkEnabled);

            // Auto-start checkbox
            var chkAutoStart = new CheckBox
            {
                Text = "Auto-Start",
                Font = UITheme.Fonts.BodySmall,
                Location = new Point(600, 45),
                Size = new Size(100, 25),
                Checked = config.AutoStart,
                Tag = config.PluginName,
                Enabled = config.IsEnabled
            };
            chkAutoStart.CheckedChanged += (s, e) =>
            {
                pluginManager.SetPluginAutoStart((string)chkAutoStart.Tag, chkAutoStart.Checked);
            };
            chkEnabled.CheckedChanged += (s, e) =>
            {
                chkAutoStart.Enabled = chkEnabled.Checked;
            };
            card.Controls.Add(chkAutoStart);

            // Execute button
            var btnExecute = new Button
            {
                Text = "Execute",
                Font = UITheme.Fonts.BodySmall,
                Size = new Size(90, 35),
                Location = new Point(710, 15),
                BackColor = UITheme.Colors.ButtonPrimary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = config.IsEnabled && plugin != null,
                Tag = config.PluginName
            };
            btnExecute.FlatAppearance.BorderSize = 0;
            btnExecute.Click += (s, e) =>
            {
                try
                {
                    pluginManager.ExecutePlugin((string)btnExecute.Tag, this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error executing plugin:\n{ex.Message}", "Plugin Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            chkEnabled.CheckedChanged += (s, e) =>
            {
                btnExecute.Enabled = chkEnabled.Checked && plugin != null;
            };
            card.Controls.Add(btnExecute);

            // Uninstall button
            var btnUninstall = new Button
            {
                Text = "Uninstall",
                Font = UITheme.Fonts.BodySmall,
                Size = new Size(90, 35),
                Location = new Point(710, 55),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = config.PluginName
            };
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.Click += (s, e) =>
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to uninstall '{config.PluginName}'?\n\nThis will delete the DLL file and cannot be undone.",
                    "Confirm Uninstall",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    if (pluginManager.UninstallPlugin((string)btnUninstall.Tag, out string errorMessage))
                    {
                        MessageBox.Show($"Plugin '{config.PluginName}' uninstalled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadPluginsList();
                    }
                    else
                    {
                        MessageBox.Show($"Error uninstalling plugin:\n{errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };
            card.Controls.Add(btnUninstall);

            return card;
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Title = "Select Plugin DLL";
                openDialog.Filter = "DLL Files (*.dll)|*.dll|All Files (*.*)|*.*";
                openDialog.CheckFileExists = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var sourcePath = openDialog.FileName;

                    if (pluginManager.ImportPlugin(sourcePath, out string errorMessage))
                    {
                        MessageBox.Show(
                            "Plugin imported successfully!\n\nThe plugin has been added to your plugins folder.",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        LoadPluginsList();
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Failed to import plugin:\n\n{errorMessage}",
                            "Import Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            pluginManager.ReloadPlugins(executeAutoStart: false);
            LoadPluginsList();
            MessageBox.Show("Plugins reloaded successfully.", "Reload Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}