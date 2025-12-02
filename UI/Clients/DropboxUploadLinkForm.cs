using System;

using System.Drawing;

using System.IO;

using System.Threading.Tasks;

using System.Windows.Forms;

using ReactCRM.Database;

using ReactCRM.Services;
using System.Linq;



namespace ReactCRM.UI.Clients

{

    /// <summary>

    /// Formulario para generar links de subida via Dropbox File Requests

    /// </summary>

    public class DropboxUploadLinkForm : Form

    {

        private readonly int _clientId;

        private readonly string _clientName;

        private readonly int _userId;

        private readonly DropboxUploadRepository _repository;



        // UI Controls

        private Label lblTitle;

        private Label lblClientName;

        private Label lblStatus;

        private TextBox txtUploadLink;

        private Label lblFolder;

        private TextBox txtDropboxFolder;

        private Button btnGenerate;

        private Button btnCopy;

        private Button btnSync;

        private Button btnRevoke;

        private Button btnOpenFolder;

        private Button btnClose;

        private LinkLabel linkPreview;

        private ProgressBar progressBar;

        private Label lblProgress;



        public DropboxUploadLinkForm(int clientId, string clientName, int userId)

        {

            _clientId = clientId;

            _clientName = clientName;

            _userId = userId;

            _repository = new DropboxUploadRepository();



            InitializeComponent();

            LoadExistingRequest();

        }



        private void InitializeComponent()

        {

            this.Text = "Dropbox Upload Link";

            this.Size = new Size(600, 420);

            this.StartPosition = FormStartPosition.CenterParent;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            this.MaximizeBox = false;

            this.MinimizeBox = false;

            this.BackColor = Color.FromArgb(245, 245, 250);



            int y = 20;



            // Title

            lblTitle = new Label

            {

                Text = "📁 Dropbox Upload Link",

                Font = new Font("Segoe UI", 14, FontStyle.Bold),

                Location = new Point(20, y),

                Size = new Size(400, 30),

                ForeColor = Color.FromArgb(33, 37, 41)

            };

            this.Controls.Add(lblTitle);

            y += 40;



            // Client Name

            lblClientName = new Label

            {

                Text = $"Client: {_clientName}",

                Font = new Font("Segoe UI", 10),

                Location = new Point(20, y),

                Size = new Size(400, 25),

                ForeColor = Color.FromArgb(73, 80, 87)

            };

            this.Controls.Add(lblClientName);

            y += 30;



            // Status

            lblStatus = new Label

            {

                Text = "Status: No upload link created",

                Font = new Font("Segoe UI", 9),

                Location = new Point(20, y),

                Size = new Size(400, 20),

                ForeColor = Color.FromArgb(108, 117, 125)

            };

            this.Controls.Add(lblStatus);

            y += 30;



            // Upload Link TextBox

            var lblLink = new Label

            {

                Text = "Upload Link (send to client):",

                Font = new Font("Segoe UI", 9),

                Location = new Point(20, y),

                Size = new Size(200, 20)

            };

            this.Controls.Add(lblLink);

            y += 22;



            txtUploadLink = new TextBox

            {

                Location = new Point(20, y),

                Size = new Size(450, 28),

                Font = new Font("Consolas", 9),

                ReadOnly = true,

                BackColor = Color.White

            };

            this.Controls.Add(txtUploadLink);



            btnCopy = new Button

            {

                Text = "Copy",

                Location = new Point(480, y - 2),

                Size = new Size(80, 30),

                BackColor = Color.FromArgb(0, 123, 255),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand

            };

            btnCopy.FlatAppearance.BorderSize = 0;

            btnCopy.Click += BtnCopy_Click;

            this.Controls.Add(btnCopy);

            y += 35;



            // Preview Link

            linkPreview = new LinkLabel

            {

                Text = "Open link in browser",

                Location = new Point(20, y),

                Size = new Size(200, 20),

                Visible = false

            };

            linkPreview.LinkClicked += LinkPreview_LinkClicked;

            this.Controls.Add(linkPreview);

            y += 30;



            // Dropbox Folder

            lblFolder = new Label

            {

                Text = "Dropbox folder:",

                Font = new Font("Segoe UI", 9),

                Location = new Point(20, y),

                Size = new Size(100, 20)

            };

            this.Controls.Add(lblFolder);

            y += 22;



            txtDropboxFolder = new TextBox

            {

                Location = new Point(20, y),

                Size = new Size(540, 25),

                Font = new Font("Consolas", 8),

                ReadOnly = true,

                BackColor = Color.FromArgb(248, 249, 250)

            };

            this.Controls.Add(txtDropboxFolder);

            y += 35;



            // Progress

            progressBar = new ProgressBar

            {

                Location = new Point(20, y),

                Size = new Size(540, 20),

                Visible = false,

                Style = ProgressBarStyle.Marquee

            };

            this.Controls.Add(progressBar);



            lblProgress = new Label

            {

                Text = "",

                Font = new Font("Segoe UI", 8),

                Location = new Point(20, y + 22),

                Size = new Size(540, 18),

                ForeColor = Color.FromArgb(108, 117, 125),

                Visible = false

            };

            this.Controls.Add(lblProgress);

            y += 50;



            // Buttons Panel

            var panelButtons = new Panel

            {

                Location = new Point(20, y),

                Size = new Size(560, 80)

            };

            this.Controls.Add(panelButtons);



            // Row 1

            btnGenerate = new Button

            {

                Text = "Generate Upload Link",

                Location = new Point(0, 0),

                Size = new Size(160, 35),

                BackColor = Color.FromArgb(0, 99, 177), // Dropbox blue

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand

            };

            btnGenerate.FlatAppearance.BorderSize = 0;

            btnGenerate.Click += BtnGenerate_Click;

            panelButtons.Controls.Add(btnGenerate);



            btnSync = new Button

            {

                Text = "Sync Files",

                Location = new Point(170, 0),

                Size = new Size(100, 35),

                BackColor = Color.FromArgb(40, 167, 69),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand,

                Visible = false

            };

            btnSync.FlatAppearance.BorderSize = 0;

            btnSync.Click += BtnSync_Click;

            panelButtons.Controls.Add(btnSync);



            btnOpenFolder = new Button

            {

                Text = "Open Folder",

                Location = new Point(280, 0),

                Size = new Size(100, 35),

                BackColor = Color.FromArgb(108, 117, 125),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand,

                Visible = false

            };

            btnOpenFolder.FlatAppearance.BorderSize = 0;

            btnOpenFolder.Click += BtnOpenFolder_Click;

            panelButtons.Controls.Add(btnOpenFolder);



            // Row 2

            btnRevoke = new Button

            {

                Text = "Revoke Link",

                Location = new Point(0, 40),

                Size = new Size(100, 32),

                BackColor = Color.FromArgb(220, 53, 69),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand,

                Visible = false

            };

            btnRevoke.FlatAppearance.BorderSize = 0;

            btnRevoke.Click += BtnRevoke_Click;

            panelButtons.Controls.Add(btnRevoke);



            btnClose = new Button

            {

                Text = "Close",

                Location = new Point(460, 40),

                Size = new Size(90, 32),

                BackColor = Color.FromArgb(108, 117, 125),

                ForeColor = Color.White,

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand

            };

            btnClose.FlatAppearance.BorderSize = 0;

            btnClose.Click += (s, e) => this.Close();

            panelButtons.Controls.Add(btnClose);

        }



        private void LoadExistingRequest()

        {

            try

            {

                var info = _repository.GetClientDropboxInfo(_clientId);



                if (info != null && !string.IsNullOrEmpty(info.UploadUrl))

                {

                    txtUploadLink.Text = info.UploadUrl;

                    txtDropboxFolder.Text = info.DropboxFolder;



                    lblStatus.Text = $"Status: Link active (created {info.Created:yyyy-MM-dd})";

                    lblStatus.ForeColor = Color.FromArgb(40, 167, 69);



                    btnGenerate.Text = "Regenerate Link";

                    btnSync.Visible = true;

                    btnOpenFolder.Visible = true;

                    btnRevoke.Visible = true;

                    linkPreview.Visible = true;

                }

                else

                {

                    lblStatus.Text = "Status: No upload link created";

                    lblStatus.ForeColor = Color.FromArgb(108, 117, 125);

                    btnGenerate.Text = "Generate Upload Link";

                    btnSync.Visible = false;

                    btnOpenFolder.Visible = false;

                    btnRevoke.Visible = false;

                    linkPreview.Visible = false;

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error loading data: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        private async void BtnGenerate_Click(object sender, EventArgs e)

        {

            btnGenerate.Enabled = false;

            ShowProgress("Creating Dropbox File Request...");



            try

            {

                var request = await DropboxService.Instance.CreateFileRequest(_clientId, _clientName);



                _repository.SaveFileRequest(_clientId, request, _userId);



                txtUploadLink.Text = request.Url;

                txtDropboxFolder.Text = request.Destination;



                HideProgress();

                LoadExistingRequest();



                MessageBox.Show(

                    "Upload link created successfully!\n\n" +

                    "Send this link to your client so they can upload files.\n" +

                    "Use 'Sync Files' to download uploaded files to your computer.",

                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

            catch (Exception ex)

            {

                HideProgress();

                MessageBox.Show(

                    $"Error creating Dropbox link:\n\n{ex.Message}\n\n" +

                    "Make sure your Dropbox access token is configured correctly.",

                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            finally

            {

                btnGenerate.Enabled = true;

            }

        }



        private async void BtnSync_Click(object sender, EventArgs e)

        {

            var info = _repository.GetClientDropboxInfo(_clientId);

            if (info == null || string.IsNullOrEmpty(info.DropboxFolder))

            {

                MessageBox.Show("No Dropbox folder configured.", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;

            }



            btnSync.Enabled = false;

            ShowProgress($"Checking Dropbox folder: {info.DropboxFolder}");



            try

            {

                // Step 1: List files

                lblProgress.Text = "Step 1/3: Listing files in Dropbox...";

                Application.DoEvents();



                List<DropboxFile> dropboxFiles;

                try

                {

                    dropboxFiles = await DropboxService.Instance.ListFiles(info.DropboxFolder);

                }

                catch (Exception ex)

                {

                    HideProgress();

                    MessageBox.Show(

                        $"Failed to list files from Dropbox.\n\n" +

                        $"Dropbox folder: {info.DropboxFolder}\n\n" +

                        $"Error: {ex.Message}\n\n" +

                        "Make sure:\n" +

                        "1. Files have been uploaded to the Dropbox link\n" +

                        "2. Your access token has 'files.metadata.read' permission",

                        "List Files Error",

                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    btnSync.Enabled = true;

                    return;

                }



                lblProgress.Text = $"Step 2/3: Found {dropboxFiles.Count} file(s) in Dropbox. Downloading new files...";

                Application.DoEvents();



                if (dropboxFiles.Count == 0)

                {

                    // Offer to search recursively

                    var result = MessageBox.Show(

                        $"No files found in the direct folder path:\n" +

                        $"{info.DropboxFolder}\n\n" +

                        $"Files might be in a subfolder.\n\n" +

                        $"Would you like to search recursively (including subfolders)?\n" +

                        $"This will show all files and their exact locations.",

                        "No Files Found - Search Recursively?",

                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);



                    if (result == DialogResult.Yes)

                    {

                        ShowProgress("Searching recursively for files...");

                        try

                        {

                            var recursiveFiles = await DropboxService.Instance.ListFilesRecursive(info.DropboxFolder);



                            HideProgress();



                            if (recursiveFiles.Count == 0)

                            {

                                MessageBox.Show(

                                    $"No files found even with recursive search.\n\n" +

                                    $"Searched in: {info.DropboxFolder}\n\n" +

                                    "This means either:\n" +

                                    "1. No files have been uploaded to the Dropbox link yet\n" +

                                    "2. Files are in a completely different location\n\n" +

                                    "Please verify files were uploaded using the upload link.",

                                    "No Files Found",

                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            }

                            else

                            {

                                // Show where files were found

                                string fileList = string.Join("\n",

                                    recursiveFiles.Take(10).Select(f => $"  {f.PathDisplay}"));



                                if (recursiveFiles.Count > 10)

                                    fileList += $"\n  ... and {recursiveFiles.Count - 10} more file(s)";



                                var downloadResult = MessageBox.Show(

                                    $"Found {recursiveFiles.Count} file(s) in subfolders:\n\n" +

                                    fileList + "\n\n" +

                                    "These files are NOT in the expected folder!\n" +

                                    "They might be in a subfolder created by Dropbox.\n\n" +

                                    "Would you like to download them anyway?",

                                    $"Found {recursiveFiles.Count} Files in Subfolders",

                                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);



                                if (downloadResult == DialogResult.Yes)

                                {

                                    // Download all found files

                                    ShowProgress($"Downloading {recursiveFiles.Count} file(s)...");



                                    var clientFolder = Path.Combine(DropboxService.LocalDownloadFolder, _clientId.ToString());

                                    if (!Directory.Exists(clientFolder))

                                        Directory.CreateDirectory(clientFolder);



                                    int downloadCount = 0;

                                    foreach (var file in recursiveFiles)

                                    {

                                        try

                                        {

                                            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                                            string uniqueFileName = $"{timestamp}_{file.Name}";



                                            // Check if already exists

                                            var existingFiles = Directory.GetFiles(clientFolder, $"*{file.Name}");

                                            if (existingFiles.Length == 0)

                                            {

                                                var downloaded = await DropboxService.Instance.DownloadFile(file.PathDisplay, clientFolder);



                                                var fileInfo = new FileInfo(downloaded);

                                                _repository.SaveDownloadedFile(

                                                    _clientId,

                                                    fileInfo.Name,

                                                    downloaded,

                                                    fileInfo.Length,

                                                    file.PathDisplay);



                                                downloadCount++;

                                            }

                                        }

                                        catch (Exception ex)

                                        {

                                            System.Diagnostics.Debug.WriteLine($"Error downloading {file.Name}: {ex.Message}");

                                        }

                                    }



                                    HideProgress();

                                    MessageBox.Show(

                                        $"Successfully downloaded {downloadCount} file(s)!\n\n" +

                                        $"Files saved to: {FilePathService.GetClientFolderPath(_clientId, _clientName)}\\\n\n" +

                                        "You can now view them in 'File Attachments'.",

                                        "Download Complete",

                                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                                }

                            }

                        }

                        catch (Exception ex)

                        {

                            HideProgress();

                            MessageBox.Show(

                                $"Error during recursive search:\n\n{ex.Message}",

                                "Search Error",

                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                        }

                    }

                    else

                    {

                        // Copy path to clipboard

                        Clipboard.SetText(info.DropboxFolder);

                        MessageBox.Show(

                            $"Folder path copied to clipboard:\n{info.DropboxFolder}\n\n" +

                            "Check this exact path in Dropbox.com to see where files are.",

                            "Path Copied",

                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }

                    btnSync.Enabled = true;

                    return;

                }



                // Step 2: Download files

                List<string> downloadedFiles;

                try

                {

                    downloadedFiles = await DropboxService.Instance.SyncClientFiles(

                        _clientId, _clientName, info.DropboxFolder);

                }

                catch (Exception ex)

                {

                    HideProgress();

                    MessageBox.Show(

                        $"Failed to download files from Dropbox.\n\n" +

                        $"Error: {ex.Message}\n\n" +

                        "Make sure your access token has 'files.content.read' permission",

                        "Download Error",

                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    btnSync.Enabled = true;

                    return;

                }



                // Step 3: Save to database

                lblProgress.Text = $"Step 3/3: Saving {downloadedFiles.Count} file(s) to database...";

                Application.DoEvents();



                int savedCount = 0;

                List<string> saveErrors = new List<string>();



                foreach (var filePath in downloadedFiles)

                {

                    try

                    {

                        var fileInfo = new FileInfo(filePath);

                        _repository.SaveDownloadedFile(

                            _clientId,

                            fileInfo.Name,

                            filePath,

                            fileInfo.Length,

                            info.DropboxFolder);

                        savedCount++;

                    }

                    catch (Exception ex)

                    {

                        saveErrors.Add($"{Path.GetFileName(filePath)}: {ex.Message}");

                        System.Diagnostics.Debug.WriteLine($"Error saving file to database: {ex.Message}");

                    }

                }



                HideProgress();



                // Show results

                if (downloadedFiles.Count > 0)

                {

                    string message = $"Successfully synced!\n\n" +

                                   $"Total files in Dropbox: {dropboxFiles.Count}\n" +

                                   $"New files downloaded: {downloadedFiles.Count}\n" +

                                   $"Files saved to database: {savedCount}\n\n" +

                                   $"Files saved to: {FilePathService.GetClientFolderPath(_clientId, _clientName)}\\\n\n" +

                                   "You can now view these files in the 'File Attachments' window.";



                    if (saveErrors.Count > 0)

                    {

                        message += "\n\nWarning - Some files failed to save:\n" + string.Join("\n", saveErrors);

                    }



                    MessageBox.Show(message, "Sync Complete",

                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                }

                else

                {

                    MessageBox.Show(

                        $"No new files to download.\n\n" +

                        $"Total files in Dropbox: {dropboxFiles.Count}\n" +

                        $"All files have already been synced to:\n" +

                        $"{FilePathService.GetClientFolderPath(_clientId, _clientName)}\\",

                        "Sync Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }

            }

            catch (Exception ex)

            {

                HideProgress();

                MessageBox.Show(

                    $"Unexpected error during sync:\n\n{ex.Message}\n\n" +

                    $"Stack trace:\n{ex.StackTrace}");

            }

        }



        private void BtnOpenFolder_Click(object sender, EventArgs e)

        {

            // Use same folder structure as ClientFilesForm: clientfiles/{clientId}-{clientName}/

            var clientFolder = Path.Combine(

                DropboxService.LocalDownloadFolder,

                Path.GetFileName(FilePathService.GetOrMigrateClientFolderPath(_clientId, _clientName)));



            if (!Directory.Exists(clientFolder))

            {

                Directory.CreateDirectory(clientFolder);

            }



            System.Diagnostics.Process.Start("explorer.exe", clientFolder);

        }



        private async void BtnRevoke_Click(object sender, EventArgs e)

        {

            var result = MessageBox.Show(

                "Are you sure you want to revoke this link?\n\n" +

                "The client will no longer be able to upload files using this link.",

                "Confirm Revoke",

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Warning);



            if (result != DialogResult.Yes) return;



            try

            {

                var info = _repository.GetClientDropboxInfo(_clientId);

                if (info != null)

                {

                    await DropboxService.Instance.CloseFileRequest(info.RequestId);

                }



                _repository.ClearFileRequest(_clientId, _userId);



                txtUploadLink.Text = "";

                txtDropboxFolder.Text = "";

                LoadExistingRequest();



                MessageBox.Show("Upload link revoked.", "Success",

                    MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error revoking link: {ex.Message}", "Error",

                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        private void BtnCopy_Click(object sender, EventArgs e)

        {

            if (!string.IsNullOrEmpty(txtUploadLink.Text))

            {

                Clipboard.SetText(txtUploadLink.Text);



                var originalText = btnCopy.Text;

                btnCopy.Text = "Copied!";

                btnCopy.BackColor = Color.FromArgb(40, 167, 69);



                var timer = new System.Windows.Forms.Timer { Interval = 1500 };

                timer.Tick += (s, args) =>

                {

                    btnCopy.Text = originalText;

                    btnCopy.BackColor = Color.FromArgb(0, 123, 255);

                    timer.Stop();

                    timer.Dispose();

                };

                timer.Start();

            }

        }



        private void LinkPreview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)

        {

            if (!string.IsNullOrEmpty(txtUploadLink.Text))

            {

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo

                {

                    FileName = txtUploadLink.Text,

                    UseShellExecute = true

                });

            }

        }



        private void ShowProgress(string message)

        {

            progressBar.Visible = true;

            lblProgress.Text = message;

            lblProgress.Visible = true;

            Application.DoEvents();

        }



        private void HideProgress()

        {

            progressBar.Visible = false;

            lblProgress.Visible = false;

        }

    }

}