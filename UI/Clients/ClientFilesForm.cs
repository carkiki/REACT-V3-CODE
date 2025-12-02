using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using ReactCRM.Database;
using ReactCRM.Models;
using ReactCRM.Services;
using ReactCRM.UI.Templates;
using File = System.IO.File;
using Directory = System.IO.Directory;
using Path = System.IO.Path;

namespace ReactCRM.UI.Clients
{
    public partial class ClientFilesForm : Form
    {
        private int clientId;
        private Client client;
        private DataGridView gridFiles;
        private Button btnUpload;
        private Button btnDownload;
        private Button btnPreview;
        private Button btnDelete;
        private Button btnClose;
        private Label lblClientInfo;
        private Label lblTotal;

        public ClientFilesForm(int clientId, Client client)
        {
            this.clientId = clientId;
            this.client = client;
            InitializeComponent();
            LoadFiles();
        }

        private void InitializeComponent()
        {
            this.Text = $"File Attachments - {client.Name}";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(900, 600);
            this.BackColor = UITheme.Colors.BackgroundPrimary;

            // Header Panel
            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = UITheme.Spacing.HeaderHeight + 20,
                BackColor = UITheme.Colors.HeaderPrimary,
                Padding = new Padding(UITheme.Spacing.Medium)
            };

            var lblTitle = new Label
            {
                Text = $"File Attachments",
                Font = UITheme.Fonts.HeaderMedium,
                ForeColor = UITheme.Colors.TextInverse,
                Location = new Point(UITheme.Spacing.Medium, UITheme.Spacing.Small),
                AutoSize = true
            };

            lblClientInfo = new Label
            {
                Text = $"Client: {client.Name} (SSN: {client.SSN})",
                Font = UITheme.Fonts.BodySmall,
                ForeColor = UITheme.Colors.TextTertiary,
                Location = new Point(UITheme.Spacing.Medium, UITheme.Spacing.HeaderHeight - 25),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblTitle);
            panelHeader.Controls.Add(lblClientInfo);

            // Top Panel - Action Buttons
            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = UITheme.Colors.BackgroundSecondary,
                Padding = new Padding(UITheme.Spacing.Medium)
            };

            btnUpload = new Button
            {
                Text = "+ Upload File",
                Location = new Point(UITheme.Spacing.Medium, UITheme.Spacing.Small),
                Size = new Size(130, UITheme.Spacing.ButtonHeight),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonStyle(btnUpload, ButtonType.Primary);
            btnUpload.Click += BtnUpload_Click;

            // Scan Button
            var btnScan = new Button
            {
                Text = "Scan",
                Location = new Point(UITheme.Spacing.Medium + 140, UITheme.Spacing.Small),
                Size = new Size(100, UITheme.Spacing.ButtonHeight),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonStyle(btnScan, ButtonType.Secondary);
            btnScan.Click += BtnScan_Click;

            // Generate from Template Button
            var btnGenerateFromTemplate = new Button
            {
                Text = "From Template",
                Location = new Point(UITheme.Spacing.Medium + 250, UITheme.Spacing.Small),
                Size = new Size(130, UITheme.Spacing.ButtonHeight),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonStyle(btnGenerateFromTemplate, ButtonType.Secondary);
            btnGenerateFromTemplate.Click += BtnGenerateFromTemplate_Click;

            // Upload Link Button (Dropbox) - positioned after From Template
            var btnUploadLink = new Button
            {
                Text = "Upload Link",
                Location = new Point(UITheme.Spacing.Medium + 390, UITheme.Spacing.Small),
                Size = new Size(120, UITheme.Spacing.ButtonHeight),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(0, 99, 177), // Dropbox blue
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUploadLink.FlatAppearance.BorderSize = 0;
            btnUploadLink.Click += BtnUploadLink_Click;

            lblTotal = new Label
            {
                Text = "Total: 0",
                AutoSize = true,
                Font = UITheme.Fonts.BodyRegular,
                Location = new Point(UITheme.Spacing.Medium + 530, UITheme.Spacing.Small + 10),
                ForeColor = UITheme.Colors.TextPrimary
            };

            panelTop.Controls.Add(btnUpload);
            panelTop.Controls.Add(btnScan);
            panelTop.Controls.Add(btnGenerateFromTemplate);
            panelTop.Controls.Add(btnUploadLink);
            panelTop.Controls.Add(lblTotal);

            // DataGridView - Modern Style
            gridFiles = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                Font = UITheme.Fonts.BodyRegular,
                Margin = new Padding(UITheme.Spacing.Small)
            };
            UITheme.ApplyDataGridStyle(gridFiles);
            gridFiles.CellDoubleClick += GridFiles_CellDoubleClick;

            // Bottom Panel - Action Buttons
            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = UITheme.Colors.BackgroundSecondary,
                Padding = new Padding(UITheme.Spacing.Medium)
            };

            btnDownload = new Button
            {
                Text = "Download",
                Location = new Point(UITheme.Spacing.Medium, UITheme.Spacing.Small),
                Size = new Size(120, UITheme.Spacing.ButtonHeight),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonStyle(btnDownload, ButtonType.Primary);
            btnDownload.Click += BtnDownload_Click;

            btnPreview = new Button
            {
                Text = "Preview",
                Location = new Point(UITheme.Spacing.Medium + 130, UITheme.Spacing.Small),
                Size = new Size(110, UITheme.Spacing.ButtonHeight),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonStyle(btnPreview, ButtonType.Secondary);
            btnPreview.Click += BtnPreview_Click;

            // Rename Button
            var btnRename = new Button
            {
                Text = "Rename",
                Location = new Point(UITheme.Spacing.Medium + 250, UITheme.Spacing.Small),
                Size = new Size(110, UITheme.Spacing.ButtonHeight),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonStyle(btnRename, ButtonType.Secondary);
            btnRename.Click += BtnRename_Click;

            btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(UITheme.Spacing.Medium + 370, UITheme.Spacing.Small),
                Size = new Size(110, UITheme.Spacing.ButtonHeight),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonStyle(btnDelete, ButtonType.Danger);
            btnDelete.Click += BtnDelete_Click;

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(UITheme.Spacing.Medium + 490, UITheme.Spacing.Small),
                Size = new Size(100, UITheme.Spacing.ButtonHeight),
                Cursor = Cursors.Hand
            };
            UITheme.ApplyButtonStyle(btnClose, ButtonType.Secondary);
            btnClose.Click += (s, e) => this.Close();

            panelBottom.Controls.Add(btnDownload);
            panelBottom.Controls.Add(btnPreview);
            panelBottom.Controls.Add(btnRename);
            panelBottom.Controls.Add(btnDelete);
            panelBottom.Controls.Add(btnClose);

            // Add controls to form
            this.Controls.Add(gridFiles);
            this.Controls.Add(panelTop);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelBottom);

            // Check permissions
            if (!AuthService.Instance.CanEditClients())
            {
                btnUpload.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void LoadFiles()
        {
            try
            {
                var files = GetClientFiles();
                gridFiles.DataSource = null;
                gridFiles.Columns.Clear();

                // Manually add columns
                gridFiles.Columns.Add("Id", "ID");
                gridFiles.Columns.Add("FileName", "File Name");
                gridFiles.Columns.Add("FileSize", "Size");
                gridFiles.Columns.Add("Description", "Description");
                gridFiles.Columns.Add("UploadedBy", "Uploaded By");
                gridFiles.Columns.Add("UploadedAt", "Uploaded At");

                // Add rows
                foreach (var file in files)
                {
                    gridFiles.Rows.Add(
                        file.Id,
                        file.FileName,
                        FormatFileSize(file.FileSize),
                        file.Description ?? "",
                        file.UploadedByUsername,
                        file.UploadedAt.ToString("yyyy-MM-dd HH:mm")
                    );
                }

                // Format columns
                gridFiles.Columns["Id"].Width = 50;
                gridFiles.Columns["FileName"].Width = 250;
                gridFiles.Columns["FileSize"].Width = 100;
                gridFiles.Columns["Description"].Width = 200;
                gridFiles.Columns["UploadedBy"].Width = 150;
                gridFiles.Columns["UploadedAt"].Width = 150;

                lblTotal.Text = $"Total: {files.Count} files";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<ClientFile> GetClientFiles()
        {
            var files = new List<ClientFile>();

            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT cf.*, w.Username as UploadedByUsername
                FROM ClientFiles cf
                LEFT JOIN Workers w ON cf.UploadedBy = w.Id
                WHERE cf.ClientId = @clientId
                ORDER BY cf.UploadedAt DESC";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                files.Add(new ClientFile
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ClientId = Convert.ToInt32(reader["ClientId"]),
                    FileName = reader["FileName"].ToString() ?? "",
                    FilePath = reader["FilePath"].ToString() ?? "",
                    FileSize = Convert.ToInt64(reader["FileSize"]),
                    Description = reader["Description"]?.ToString(),
                    UploadedBy = Convert.ToInt32(reader["UploadedBy"]),
                    UploadedByUsername = reader["UploadedByUsername"]?.ToString(),
                    UploadedAt = Convert.ToDateTime(reader["UploadedAt"])
                });
            }

            return files;
        }

        private void BtnGenerateFromTemplate_Click(object? sender, EventArgs e)
        {
            if (!AuthService.Instance.CanEditClients())
            {
                MessageBox.Show("You don't have permission to generate documents.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var generateForm = new TemplateGenerateForm(client);
            generateForm.DocumentGenerated += (s, args) =>
            {
                // Refresh file list when document is generated
                LoadFiles();
            };

            generateForm.ShowDialog();
        }

        /// <summary>
        /// Opens Dropbox Upload Link form for client
        /// </summary>
        private void BtnUploadLink_Click(object? sender, EventArgs e)
        {
            var form = new DropboxUploadLinkForm(clientId, client.Name, AuthService.Instance.CurrentUserId);
            form.ShowDialog();
            LoadFiles(); // Refresh in case files were synced
        }

        private void BtnUpload_Click(object? sender, EventArgs e)
        {
            if (!AuthService.Instance.CanEditClients())
            {
                MessageBox.Show("You don't have permission to upload files.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var openFileDialog = new OpenFileDialog
            {
                Title = "Select File to Upload",
                Filter = "All Files (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string sourceFile = openFileDialog.FileName;
                    string fileName = Path.GetFileName(sourceFile);
                    long fileSize = new System.IO.FileInfo(sourceFile).Length;

                    // Check file size (max 50MB)
                    if (fileSize > 50 * 1024 * 1024)
                    {
                        MessageBox.Show("File size exceeds 50MB limit.", "File Too Large",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Ask for description
                    string description = PromptForDescription();

                    // Create client files directory if it doesn't exist (with client name)
                    string clientFilesDir = FilePathService.GetOrMigrateClientFolderPath(clientId, client.Name);
                    Directory.CreateDirectory(clientFilesDir);

                    // Generate unique file name to avoid collisions
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string uniqueFileName = $"{timestamp}_{fileName}";
                    string destPath = Path.Combine(clientFilesDir, uniqueFileName);

                    // Copy file
                    File.Copy(sourceFile, destPath);

                    // Save to database
                    SaveFileToDatabase(fileName, destPath, fileSize, description);

                    MessageBox.Show("File uploaded successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadFiles();

                    AuditService.LogAction("Upload", "ClientFile", clientId,
                        $"Uploaded file '{fileName}' for client {client.Name}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error uploading file: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveFileToDatabase(string fileName, string filePath, long fileSize, string description)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                INSERT INTO ClientFiles (ClientId, FileName, FilePath, FileSize, Description, UploadedBy)
                VALUES (@clientId, @fileName, @filePath, @fileSize, @description, @uploadedBy)";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@clientId", clientId);
            cmd.Parameters.AddWithValue("@fileName", fileName);
            cmd.Parameters.AddWithValue("@filePath", filePath);
            cmd.Parameters.AddWithValue("@fileSize", fileSize);
            cmd.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@uploadedBy", AuthService.Instance.GetCurrentUserId());

            cmd.ExecuteNonQuery();
        }

        private void BtnDownload_Click(object? sender, EventArgs e)
        {
            if (gridFiles.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a file to download.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DownloadSelectedFile();
        }

        private void GridFiles_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DownloadSelectedFile();
            }
        }

        private void DownloadSelectedFile()
        {
            try
            {
                int fileId = (int)gridFiles.SelectedRows[0].Cells["Id"].Value;
                string fileName = gridFiles.SelectedRows[0].Cells["FileName"].Value?.ToString() ?? "";

                var file = GetClientFileById(fileId);
                if (file == null || !File.Exists(file.FilePath))
                {
                    MessageBox.Show("File not found on disk.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var saveFileDialog = new SaveFileDialog
                {
                    FileName = fileName,
                    Filter = "All Files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(file.FilePath, saveFileDialog.FileName, true);
                    MessageBox.Show("File downloaded successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    AuditService.LogAction("Download", "ClientFile", clientId,
                        $"Downloaded file '{fileName}' for client {client.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (!AuthService.Instance.CanEditClients())
            {
                MessageBox.Show("You don't have permission to delete files.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (gridFiles.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a file to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int fileId = (int)gridFiles.SelectedRows[0].Cells["Id"].Value;
            string fileName = gridFiles.SelectedRows[0].Cells["FileName"].Value?.ToString() ?? "";

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{fileName}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var file = GetClientFileById(fileId);
                    if (file != null)
                    {
                        // Delete from disk
                        if (File.Exists(file.FilePath))
                        {
                            File.Delete(file.FilePath);
                        }

                        // Delete from database
                        DeleteFileFromDatabase(fileId);

                        MessageBox.Show("File deleted successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadFiles();

                        AuditService.LogAction("Delete", "ClientFile", clientId,
                            $"Deleted file '{fileName}' for client {client.Name}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting file: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteFileFromDatabase(int fileId)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = "DELETE FROM ClientFiles WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", fileId);
            cmd.ExecuteNonQuery();
        }

        private ClientFile? GetClientFileById(int id)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                SELECT cf.*, w.Username as UploadedByUsername
                FROM ClientFiles cf
                LEFT JOIN Workers w ON cf.UploadedBy = w.Id
                WHERE cf.Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new ClientFile
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ClientId = Convert.ToInt32(reader["ClientId"]),
                    FileName = reader["FileName"].ToString() ?? "",
                    FilePath = reader["FilePath"].ToString() ?? "",
                    FileSize = Convert.ToInt64(reader["FileSize"]),
                    Description = reader["Description"]?.ToString(),
                    UploadedBy = Convert.ToInt32(reader["UploadedBy"]),
                    UploadedByUsername = reader["UploadedByUsername"]?.ToString(),
                    UploadedAt = Convert.ToDateTime(reader["UploadedAt"])
                };
            }

            return null;
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void BtnRename_Click(object? sender, EventArgs e)
        {
            if (!AuthService.Instance.CanEditClients())
            {
                MessageBox.Show("You don't have permission to rename files.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (gridFiles.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a file to rename.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int fileId = (int)gridFiles.SelectedRows[0].Cells["Id"].Value;
                string currentFileName = gridFiles.SelectedRows[0].Cells["FileName"].Value?.ToString() ?? "";

                string? newFileName = PromptForFileName("Rename File", "Enter new file name:", currentFileName);

                if (!string.IsNullOrWhiteSpace(newFileName) && newFileName != currentFileName)
                {
                    RenameFileInDatabase(fileId, newFileName);

                    MessageBox.Show("File renamed successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadFiles();

                    AuditService.LogAction("Rename", "ClientFile", clientId,
                        $"Renamed file from '{currentFileName}' to '{newFileName}'");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error renaming file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnScan_Click(object? sender, EventArgs e)
        {
            if (!AuthService.Instance.CanEditClients())
            {
                MessageBox.Show("You don't have permission to scan files.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ScanDocumentWithService();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during scanning: {ex.Message}\n\nMake sure a scanner is connected and turned on.",
                    "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Scan document using ScannerService (supports network scanners)
        /// </summary>
        private void ScanDocumentWithService()
        {
            try
            {
                // Get available scanners
                var scanners = ScannerService.GetAvailableScanners();

                if (scanners.Count == 0)
                {
                    MessageBox.Show("No scanners detected.\n\nPlease connect a scanner and try again.",
                        "No Scanners Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string selectedDeviceId = null;

                // If multiple scanners, let user select
                if (scanners.Count > 1)
                {
                    using (var selectionForm = new Form())
                    {
                        selectionForm.Text = "Select Scanner";
                        selectionForm.Size = new Size(450, 300);
                        selectionForm.StartPosition = FormStartPosition.CenterParent;
                        selectionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        selectionForm.MaximizeBox = false;
                        selectionForm.MinimizeBox = false;
                        selectionForm.BackColor = UITheme.Colors.BackgroundPrimary;

                        var label = new Label
                        {
                            Text = "Multiple scanners detected. Please select one:",
                            Location = new Point(10, 10),
                            Size = new Size(420, 40),
                            Font = UITheme.Fonts.BodyRegular
                        };

                        var listBox = new ListBox
                        {
                            Location = new Point(10, 50),
                            Size = new Size(420, 150),
                            Font = UITheme.Fonts.BodyRegular
                        };

                        foreach (var scanner in scanners)
                        {
                            listBox.Items.Add(scanner);
                        }
                        listBox.SelectedIndex = 0;
                        listBox.DisplayMember = "ToString";

                        var btnOK = new Button
                        {
                            Text = "OK",
                            DialogResult = DialogResult.OK,
                            Location = new Point(250, 220),
                            Size = new Size(80, 30)
                        };
                        UITheme.ApplyButtonStyle(btnOK, ButtonType.Primary);

                        var btnCancel = new Button
                        {
                            Text = "Cancel",
                            DialogResult = DialogResult.Cancel,
                            Location = new Point(340, 220),
                            Size = new Size(80, 30)
                        };
                        UITheme.ApplyButtonStyle(btnCancel, ButtonType.Secondary);

                        selectionForm.Controls.AddRange(new Control[] { label, listBox, btnOK, btnCancel });
                        selectionForm.AcceptButton = btnOK;
                        selectionForm.CancelButton = btnCancel;

                        if (selectionForm.ShowDialog() == DialogResult.OK && listBox.SelectedItem != null)
                        {
                            selectedDeviceId = ((ScannerDevice)listBox.SelectedItem).DeviceId;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    selectedDeviceId = scanners[0].DeviceId;
                }

                Cursor = Cursors.WaitCursor;

                // Configure scan settings
                var settings = new ScanSettings
                {
                    ResolutionDPI = 300,
                    ColorMode = ScanColorMode.Color
                };

                // Perform scan
                System.Drawing.Image scannedImage = ScannerService.ScanDocument(selectedDeviceId, settings);

                if (scannedImage == null)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("Scan was cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Prompt for document type
                int? documentTypeId = null;
                var documentTypes = DocumentTypeService.Instance.GetActiveDocumentTypes();
                if (documentTypes.Count > 0)
                {
                    using (var typeForm = new Form())
                    {
                        typeForm.Text = "Select Document Type";
                        typeForm.Size = new Size(400, 200);
                        typeForm.StartPosition = FormStartPosition.CenterParent;
                        typeForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        typeForm.MaximizeBox = false;
                        typeForm.MinimizeBox = false;
                        typeForm.BackColor = UITheme.Colors.BackgroundPrimary;

                        var label = new Label
                        {
                            Text = "Select the type of document being scanned:",
                            Location = new Point(10, 10),
                            Size = new Size(360, 20),
                            Font = UITheme.Fonts.BodyRegular
                        };

                        var comboBox = new ComboBox
                        {
                            Location = new Point(10, 40),
                            Size = new Size(360, 25),
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            Font = UITheme.Fonts.BodyRegular
                        };
                        comboBox.DisplayMember = "TypeName";
                        comboBox.ValueMember = "Id";
                        comboBox.DataSource = documentTypes;
                        comboBox.SelectedIndex = 0;

                        var btnOK = new Button
                        {
                            Text = "OK",
                            DialogResult = DialogResult.OK,
                            Location = new Point(200, 120),
                            Size = new Size(80, 30)
                        };
                        UITheme.ApplyButtonStyle(btnOK, ButtonType.Primary);

                        var btnSkip = new Button
                        {
                            Text = "Skip",
                            DialogResult = DialogResult.No,
                            Location = new Point(290, 120),
                            Size = new Size(80, 30)
                        };
                        UITheme.ApplyButtonStyle(btnSkip, ButtonType.Secondary);

                        typeForm.Controls.AddRange(new Control[] { label, comboBox, btnOK, btnSkip });
                        typeForm.AcceptButton = btnOK;

                        var result = typeForm.ShowDialog();
                        if (result == DialogResult.OK && comboBox.SelectedValue != null)
                        {
                            documentTypeId = (int)comboBox.SelectedValue;
                        }
                    }
                }

                // Save scanned image
                string clientFolder = FilePathService.GetOrMigrateClientFolderPath(clientId, client.Name);
                if (!Directory.Exists(clientFolder))
                {
                    Directory.CreateDirectory(clientFolder);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Scanned_Document_{timestamp}.png";
                string filePath = Path.Combine(clientFolder, fileName);

                ScannerService.SaveScannedImage(scannedImage, filePath);

                long fileSize = new System.IO.FileInfo(filePath).Length;

                // Save to database
                using (var connection = DbConnection.GetConnection())
                {
                    connection.Open();

                    string sql = @"
                        INSERT INTO ClientFiles (ClientId, FileName, FilePath, FileSize, Description, DocumentTypeId, UploadedAt, UploadedBy)
                        VALUES (@ClientId, @FileName, @FilePath, @FileSize, @Description, @DocumentTypeId, @UploadedAt, @UploadedBy)";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@ClientId", clientId);
                        command.Parameters.AddWithValue("@FileName", fileName);
                        command.Parameters.AddWithValue("@FilePath", filePath);
                        command.Parameters.AddWithValue("@FileSize", fileSize);
                        command.Parameters.AddWithValue("@Description", "Scanned document");
                        command.Parameters.AddWithValue("@DocumentTypeId", documentTypeId.HasValue ? (object)documentTypeId.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@UploadedAt", DateTime.Now);
                        command.Parameters.AddWithValue("@UploadedBy", AuthService.Instance.GetCurrentUserId());

                        command.ExecuteNonQuery();
                    }
                }

                AuditService.LogAction("SCAN_DOCUMENT", "ClientFiles", clientId,
                    $"Scanned document '{fileName}' for client {client?.Name ?? clientId.ToString()}");

                Cursor = Cursors.Default;

                MessageBox.Show($"Document scanned successfully!\n\nFile: {fileName}\nSize: {FormatFileSize(fileSize)}",
                    "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadFiles();

                // Dispose the image
                scannedImage.Dispose();
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                throw new Exception($"Scanner error: {ex.Message}", ex);
            }
        }

        private void ScanDocument()
        {
            try
            {
                Type? deviceManagerType = Type.GetTypeFromProgID("WIA.DeviceManager");
                if (deviceManagerType == null)
                {
                    MessageBox.Show("Windows Image Acquisition (WIA) is not available on this system.\n\n" +
                        "Please install WIA support or use the 'Upload File' button to manually import scanned documents.",
                        "WIA Not Available", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dynamic deviceManager = Activator.CreateInstance(deviceManagerType)!;
                dynamic deviceInfos = deviceManager.DeviceInfos;

                if (deviceInfos.Count == 0)
                {
                    MessageBox.Show("No scanners detected.\n\nPlease connect a scanner and try again.",
                        "No Scanners Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dynamic? selectedDevice = null;
                if (deviceInfos.Count > 1)
                {
                    List<string> scannerNames = new List<string>();
                    for (int i = 1; i <= deviceInfos.Count; i++)
                    {
                        var deviceInfo = deviceInfos[i];
                        string name = deviceInfo.Properties["Name"].Value.ToString();
                        scannerNames.Add(name);
                    }

                    using (var selectionForm = new Form())
                    {
                        selectionForm.Text = "Select Scanner";
                        selectionForm.Size = new Size(400, 300);
                        selectionForm.StartPosition = FormStartPosition.CenterParent;
                        selectionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        selectionForm.MaximizeBox = false;
                        selectionForm.MinimizeBox = false;

                        var label = new Label
                        {
                            Text = "Multiple scanners detected. Please select one:",
                            Location = new Point(10, 10),
                            Size = new Size(360, 40)
                        };

                        var listBox = new ListBox
                        {
                            Location = new Point(10, 50),
                            Size = new Size(360, 150)
                        };
                        listBox.Items.AddRange(scannerNames.ToArray());
                        listBox.SelectedIndex = 0;

                        var btnOK = new Button
                        {
                            Text = "OK",
                            DialogResult = DialogResult.OK,
                            Location = new Point(200, 210),
                            Size = new Size(80, 30)
                        };

                        var btnCancel = new Button
                        {
                            Text = "Cancel",
                            DialogResult = DialogResult.Cancel,
                            Location = new Point(290, 210),
                            Size = new Size(80, 30)
                        };

                        selectionForm.Controls.AddRange(new Control[] { label, listBox, btnOK, btnCancel });
                        selectionForm.AcceptButton = btnOK;
                        selectionForm.CancelButton = btnCancel;

                        if (selectionForm.ShowDialog() == DialogResult.OK && listBox.SelectedIndex >= 0)
                        {
                            selectedDevice = deviceInfos[listBox.SelectedIndex + 1].Connect();
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    selectedDevice = deviceInfos[1].Connect();
                }

                if (selectedDevice == null)
                {
                    return;
                }

                Cursor = Cursors.WaitCursor;

                Type? commonDialogType = Type.GetTypeFromProgID("WIA.CommonDialog");
                dynamic commonDialog = Activator.CreateInstance(commonDialogType!)!;

                dynamic image = commonDialog.ShowAcquireImage(
                    WiaDeviceType: 1,
                    Intent: 0,
                    Bias: 0,
                    FormatID: "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}",
                    AlwaysSelectDevice: false,
                    UseCommonUI: true,
                    CancelError: true
                );

                if (image == null)
                {
                    Cursor = Cursors.Default;
                    return;
                }

                int? documentTypeId = null;
                var documentTypes = DocumentTypeService.Instance.GetActiveDocumentTypes();
                if (documentTypes.Count > 0)
                {
                    using (var typeForm = new Form())
                    {
                        typeForm.Text = "Select Document Type";
                        typeForm.Size = new Size(400, 200);
                        typeForm.StartPosition = FormStartPosition.CenterParent;
                        typeForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        typeForm.MaximizeBox = false;
                        typeForm.MinimizeBox = false;

                        var label = new Label
                        {
                            Text = "Select the type of document being scanned:",
                            Location = new Point(10, 10),
                            Size = new Size(360, 20)
                        };

                        var comboBox = new ComboBox
                        {
                            Location = new Point(10, 40),
                            Size = new Size(360, 25),
                            DropDownStyle = ComboBoxStyle.DropDownList
                        };
                        comboBox.DisplayMember = "TypeName";
                        comboBox.ValueMember = "Id";
                        comboBox.DataSource = documentTypes;
                        comboBox.SelectedIndex = 0;

                        var btnOK = new Button
                        {
                            Text = "OK",
                            DialogResult = DialogResult.OK,
                            Location = new Point(200, 120),
                            Size = new Size(80, 30)
                        };

                        var btnSkip = new Button
                        {
                            Text = "Skip",
                            DialogResult = DialogResult.No,
                            Location = new Point(290, 120),
                            Size = new Size(80, 30)
                        };

                        typeForm.Controls.AddRange(new Control[] { label, comboBox, btnOK, btnSkip });
                        typeForm.AcceptButton = btnOK;

                        var result = typeForm.ShowDialog();
                        if (result == DialogResult.OK && comboBox.SelectedValue != null)
                        {
                            documentTypeId = (int)comboBox.SelectedValue;
                        }
                    }
                }

                string clientFolder = FilePathService.GetOrMigrateClientFolderPath(clientId, client.Name);
                if (!Directory.Exists(clientFolder))
                {
                    Directory.CreateDirectory(clientFolder);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Scanned_Document_{timestamp}.png";
                string filePath = Path.Combine(clientFolder, fileName);

                image.SaveFile(filePath);

                long fileSize = new System.IO.FileInfo(filePath).Length;

                using (var connection = DbConnection.GetConnection())
                {
                    connection.Open();

                    string sql = @"
                        INSERT INTO ClientFiles (ClientId, FileName, FilePath, FileSize, Description, DocumentTypeId, UploadedAt, UploadedBy)
                        VALUES (@ClientId, @FileName, @FilePath, @FileSize, @Description, @DocumentTypeId, @UploadedAt, @UploadedBy)";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@ClientId", clientId);
                        command.Parameters.AddWithValue("@FileName", fileName);
                        command.Parameters.AddWithValue("@FilePath", filePath);
                        command.Parameters.AddWithValue("@FileSize", fileSize);
                        command.Parameters.AddWithValue("@Description", "Scanned document");
                        command.Parameters.AddWithValue("@DocumentTypeId", documentTypeId.HasValue ? (object)documentTypeId.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@UploadedAt", DateTime.Now);
                        command.Parameters.AddWithValue("@UploadedBy", AuthService.Instance.GetCurrentUserId());

                        command.ExecuteNonQuery();
                    }
                }

                AuditService.LogAction("SCAN_DOCUMENT", "ClientFiles", clientId,
                    $"Scanned document '{fileName}' for client {client?.Name ?? clientId.ToString()}");

                Cursor = Cursors.Default;

                MessageBox.Show($"Document scanned successfully!\n\nFile: {fileName}\nSize: {FormatFileSize(fileSize)}",
                    "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadFiles();
            }
            catch (System.Runtime.InteropServices.COMException comEx)
            {
                Cursor = Cursors.Default;
                if (comEx.ErrorCode == unchecked((int)0x80210006))
                {
                    MessageBox.Show("Scanner is busy. Please wait and try again.",
                        "Scanner Busy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (comEx.ErrorCode == unchecked((int)0x80210001))
                {
                    MessageBox.Show("Scanner error. Please check the scanner connection and try again.",
                        "Scanner Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RenameFileInDatabase(int fileId, string newFileName)
        {
            using var connection = DbConnection.GetConnection();
            connection.Open();

            string sql = @"
                UPDATE ClientFiles
                SET FileName = @fileName
                WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@fileName", newFileName);
            cmd.Parameters.AddWithValue("@id", fileId);

            cmd.ExecuteNonQuery();
        }

        private string? PromptForFileName(string title, string prompt, string defaultValue)
        {
            using var form = new Form
            {
                Text = title,
                Size = new Size(400, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = UITheme.Colors.BackgroundPrimary
            };

            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(15, 15),
                AutoSize = true,
                Font = UITheme.Fonts.BodyRegular
            };

            var txtFileName = new TextBox
            {
                Location = new Point(15, 40),
                Width = 350,
                Height = 30,
                Text = defaultValue,
                Font = UITheme.Fonts.BodyRegular
            };
            txtFileName.SelectAll();

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(150, 80),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            UITheme.ApplyButtonStyle(btnOk, ButtonType.Primary);

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(240, 80),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };
            UITheme.ApplyButtonStyle(btnCancel, ButtonType.Secondary);

            form.Controls.Add(lblPrompt);
            form.Controls.Add(txtFileName);
            form.Controls.Add(btnOk);
            form.Controls.Add(btnCancel);

            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            return form.ShowDialog() == DialogResult.OK ? txtFileName.Text : null;
        }

        private string PromptForDescription()
        {
            using var form = new Form
            {
                Text = "File Description",
                Size = new Size(400, 180),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = UITheme.Colors.BackgroundPrimary
            };

            var lblPrompt = new Label
            {
                Text = "Enter a description for this file (optional):",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = UITheme.Fonts.BodyRegular
            };

            var txtDescription = new TextBox
            {
                Location = new Point(15, 40),
                Width = 350,
                Height = 60,
                Multiline = true,
                Font = UITheme.Fonts.BodySmall
            };

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(150, 110),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            UITheme.ApplyButtonStyle(btnOk, ButtonType.Primary);

            var btnCancel = new Button
            {
                Text = "Skip",
                Location = new Point(240, 110),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };
            UITheme.ApplyButtonStyle(btnCancel, ButtonType.Secondary);

            form.Controls.Add(lblPrompt);
            form.Controls.Add(txtDescription);
            form.Controls.Add(btnOk);
            form.Controls.Add(btnCancel);

            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            return form.ShowDialog() == DialogResult.OK ? txtDescription.Text : "";
        }

        private void BtnPreview_Click(object? sender, EventArgs e)
        {
            if (gridFiles.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a file to preview.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int fileId = (int)gridFiles.SelectedRows[0].Cells["Id"].Value;
                string fileName = gridFiles.SelectedRows[0].Cells["FileName"].Value?.ToString() ?? "";

                var file = GetClientFileById(fileId);
                if (file == null || !File.Exists(file.FilePath))
                {
                    MessageBox.Show("File not found on disk.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var previewForm = new FilePreviewForm(file.FilePath, fileName);
                previewForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file preview: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string? PromptForDocumentType()
        {
            using var form = new Form
            {
                Text = "Select Document Type",
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = UITheme.Colors.BackgroundPrimary
            };

            var lblPrompt = new Label
            {
                Text = "What type of document is this?",
                Location = new Point(15, 15),
                AutoSize = true,
                Font = UITheme.Fonts.BodyRegular
            };

            var docTypes = DocumentTypeService.Instance.GetAllDocumentTypes();
            var comboTypes = new ComboBox
            {
                Location = new Point(15, 40),
                Width = 350,
                Height = 30,
                Font = UITheme.Fonts.BodyRegular,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (var docType in docTypes)
            {
                comboTypes.Items.Add(docType.TypeName);
            }

            if (comboTypes.Items.Count > 0)
                comboTypes.SelectedIndex = 0;

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(150, 90),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            UITheme.ApplyButtonStyle(btnOk, ButtonType.Primary);

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(240, 90),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };
            UITheme.ApplyButtonStyle(btnCancel, ButtonType.Secondary);

            form.Controls.Add(lblPrompt);
            form.Controls.Add(comboTypes);
            form.Controls.Add(btnOk);
            form.Controls.Add(btnCancel);

            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog() == DialogResult.OK && comboTypes.SelectedIndex >= 0)
            {
                return comboTypes.SelectedItem?.ToString();
            }

            return null;
        }
    }

    public class ClientFile
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? Description { get; set; }
        public int UploadedBy { get; set; }
        public string? UploadedByUsername { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}