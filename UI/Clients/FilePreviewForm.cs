using System;

using System.Drawing;

using System.IO;

using System.Windows.Forms;

using Microsoft.Web.WebView2.WinForms;

using ReactCRM.Services;



namespace ReactCRM.UI.Clients

{

    /// <summary>

    /// Form for previewing file attachments (images, PDFs, text, etc.)

    /// </summary>

    public class FilePreviewForm : Form

    {

        private string filePath;

        private string fileName;

        private Label lblFileName;

        private Label lblFileType;

        private Panel panelPreview;

        private PictureBox pictureBox;

        private RichTextBox richTextBox;

        private WebView2 webView;

        private Label lblInfo;



        public FilePreviewForm(string filePath, string fileName)

        {

            this.filePath = filePath;

            this.fileName = fileName;

            InitializeComponent();

            LoadPreview();

        }



        private void InitializeComponent()

        {

            this.Text = $"File Preview - {fileName}";

            this.Size = new Size(900, 700);

            this.StartPosition = FormStartPosition.CenterParent;

            this.BackColor = UITheme.Colors.BackgroundPrimary;

            this.Font = UITheme.Fonts.BodyRegular;



            // Header Panel

            var panelHeader = new Panel

            {

                Dock = DockStyle.Top,

                Height = UITheme.Spacing.HeaderHeight + 20,

                BackColor = UITheme.Colors.HeaderPrimary,

                Padding = new Padding(UITheme.Spacing.Medium)

            };



            lblFileName = new Label

            {

                Text = $"Preview: {fileName}",

                Font = UITheme.Fonts.HeaderMedium,

                ForeColor = UITheme.Colors.TextInverse,

                Location = new Point(UITheme.Spacing.Medium, UITheme.Spacing.Small),

                AutoSize = true

            };



            string fileExtension = Path.GetExtension(filePath).ToLower();

            string fileType = GetFileTypeDescription(fileExtension);



            lblFileType = new Label

            {

                Text = $"Type: {fileType}",

                Font = UITheme.Fonts.BodySmall,

                ForeColor = UITheme.Colors.TextSecondary,

                Location = new Point(UITheme.Spacing.Medium, UITheme.Spacing.HeaderHeight - 20),

                AutoSize = true

            };



            panelHeader.Controls.Add(lblFileName);

            panelHeader.Controls.Add(lblFileType);



            // Preview Panel

            panelPreview = new Panel

            {

                Dock = DockStyle.Fill,

                BackColor = UITheme.Colors.BackgroundSecondary,

                Padding = new Padding(UITheme.Spacing.Medium)

            };



            // PictureBox for images

            pictureBox = new PictureBox

            {

                Dock = DockStyle.Fill,

                SizeMode = PictureBoxSizeMode.Zoom,

                BackColor = UITheme.Colors.SurfaceWhite,

                Visible = false

            };



            // RichTextBox for text/code

            richTextBox = new RichTextBox

            {

                Dock = DockStyle.Fill,

                ReadOnly = true,

                BackColor = UITheme.Colors.SurfaceWhite,

                Font = UITheme.Fonts.BodySmall,

                Visible = false

            };



            // WebView2 for PDFs

            webView = new WebView2

            {

                Dock = DockStyle.Fill,

                Visible = false

            };



            // Info Label for unsupported formats

            lblInfo = new Label

            {

                Text = "File preview not available for this format.\nSupported formats: Images (jpg, png, bmp, gif), Text files (txt, log), PDFs",

                Font = UITheme.Fonts.BodyRegular,

                ForeColor = UITheme.Colors.TextSecondary,

                TextAlign = ContentAlignment.MiddleCenter,

                Dock = DockStyle.Fill,

                Visible = false

            };



            panelPreview.Controls.Add(lblInfo);

            panelPreview.Controls.Add(richTextBox);

            panelPreview.Controls.Add(pictureBox);

            panelPreview.Controls.Add(webView);



            // Footer Panel

            var panelFooter = new Panel

            {

                Dock = DockStyle.Bottom,

                Height = 60,

                BackColor = UITheme.Colors.BackgroundSecondary,

                Padding = new Padding(UITheme.Spacing.Medium)

            };



            var btnClose = new Button

            {

                Text = "Close",

                Location = new Point(UITheme.Spacing.Medium, UITheme.Spacing.Small),

                Size = new Size(100, UITheme.Spacing.ButtonHeight),

                Cursor = Cursors.Hand

            };

            UITheme.ApplyButtonStyle(btnClose, ButtonType.Secondary);

            btnClose.Click += (s, e) => this.Close();



            panelFooter.Controls.Add(btnClose);



            this.Controls.Add(panelPreview);

            this.Controls.Add(panelFooter);

            this.Controls.Add(panelHeader);

        }



        private async void LoadPreview()

        {

            try

            {

                if (!File.Exists(filePath))

                {

                    lblInfo.Text = "File not found";

                    lblInfo.Visible = true;

                    return;

                }



                string extension = Path.GetExtension(filePath).ToLower();



                // Image preview

                if (IsImageFile(extension))

                {

                    try

                    {

                        pictureBox.Image = Image.FromFile(filePath);

                        pictureBox.Visible = true;

                    }

                    catch (Exception ex)

                    {

                        lblInfo.Text = $"Error loading image: {ex.Message}";

                        lblInfo.Visible = true;

                    }

                }

                // Text preview

                else if (IsTextFile(extension))

                {

                    try

                    {

                        string content = File.ReadAllText(filePath);

                        richTextBox.Text = content.Length > 50000

                            ? content.Substring(0, 50000) + "\n\n[Content truncated...]"

                            : content;

                        richTextBox.Visible = true;

                    }

                    catch (Exception ex)

                    {

                        lblInfo.Text = $"Error loading text: {ex.Message}";

                        lblInfo.Visible = true;

                    }

                }

                // PDF preview using WebView2

                else if (extension == ".pdf")

                {

                    try

                    {

                        await webView.EnsureCoreWebView2Async(null);

                        string absolutePath = Path.GetFullPath(filePath);

                        webView.CoreWebView2.Navigate(absolutePath);

                        webView.Visible = true;

                    }

                    catch (Exception ex)

                    {

                        lblInfo.Text = $"Error loading PDF: {ex.Message}\n\nMake sure WebView2 Runtime is installed.";

                        lblInfo.Visible = true;

                        AddExternalViewerButton();

                    }

                }

                // Office documents - offer to open externally

                else if (IsOfficeFile(extension))

                {

                    ShowExternalViewerOption(extension);

                }

                // Unsupported format

                else

                {

                    lblInfo.Text = $"Preview not available for {extension} files.\n\nClick 'Open with Default Program' to view this file.";

                    lblInfo.Visible = true;

                    AddExternalViewerButton();

                }

            }

            catch (Exception ex)

            {

                lblInfo.Text = $"Error: {ex.Message}";

                lblInfo.Visible = true;

            }

        }



        private bool IsImageFile(string extension)

        {

            return extension == ".jpg" || extension == ".jpeg" ||

                   extension == ".png" || extension == ".bmp" ||

                   extension == ".gif" || extension == ".tiff";

        }



        private bool IsTextFile(string extension)

        {

            return extension == ".txt" || extension == ".log" ||

                   extension == ".csv" || extension == ".xml" ||

                   extension == ".json" || extension == ".html" ||

                   extension == ".css" || extension == ".js" ||

                   extension == ".cs" || extension == ".sql";

        }



        private bool IsOfficeFile(string extension)

        {

            return extension == ".doc" || extension == ".docx" ||

                   extension == ".xls" || extension == ".xlsx" ||

                   extension == ".ppt" || extension == ".pptx" ||

                   extension == ".odt" || extension == ".ods";

        }



        private void ShowExternalViewerOption(string extension)

        {

            string fileTypeDesc = GetFileTypeDescription(extension);

            lblInfo.Text = $"{fileTypeDesc} preview not available in this viewer.\n\nClick 'Open with Default Program' to view this file.";

            lblInfo.Visible = true;

            AddExternalViewerButton();

        }



        private void AddExternalViewerButton()

        {

            var btnOpenExternal = new Button

            {

                Text = "Open with Default Program",

                Size = new Size(200, 40),

                Location = new Point(

                    (panelPreview.Width - 200) / 2,

                    lblInfo.Bottom + 20

                ),

                BackColor = UITheme.Colors.ButtonPrimary,

                ForeColor = UITheme.Colors.TextInverse,

                FlatStyle = FlatStyle.Flat,

                Font = UITheme.Fonts.BodyRegular,

                Cursor = Cursors.Hand

            };

            btnOpenExternal.FlatAppearance.BorderSize = 0;

            btnOpenExternal.Click += (s, e) => OpenFileExternally();



            panelPreview.Controls.Add(btnOpenExternal);

        }



        private void OpenFileExternally()

        {

            try

            {

                if (!File.Exists(filePath))

                {

                    MessageBox.Show("File not found.", "Error",

                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;

                }



                var processStartInfo = new System.Diagnostics.ProcessStartInfo

                {

                    FileName = filePath,

                    UseShellExecute = true

                };

                System.Diagnostics.Process.Start(processStartInfo);

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error opening file: {ex.Message}\n\nYou may need to download the file first.",

                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }



        private string GetFileTypeDescription(string extension)

        {

            return extension.ToLower() switch

            {

                ".pdf" => "PDF Document",

                ".doc" => "Word Document",

                ".docx" => "Word Document",

                ".xls" => "Excel Spreadsheet",

                ".xlsx" => "Excel Spreadsheet",

                ".ppt" => "PowerPoint Presentation",

                ".pptx" => "PowerPoint Presentation",

                ".jpg" => "JPEG Image",

                ".jpeg" => "JPEG Image",

                ".png" => "PNG Image",

                ".gif" => "GIF Image",

                ".bmp" => "Bitmap Image",

                ".txt" => "Text File",

                ".log" => "Log File",

                ".csv" => "CSV File",

                ".zip" => "ZIP Archive",

                ".rar" => "RAR Archive",

                _ => "Unknown File"

            };

        }



        protected override void Dispose(bool disposing)

        {

            if (disposing)

            {

                pictureBox?.Image?.Dispose();

                webView?.Dispose();

            }

            base.Dispose(disposing);

        }

    }

}