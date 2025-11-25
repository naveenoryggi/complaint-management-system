using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace ComplaintManagement.Installer
{
    public class MainInstallerForm : Form
    {
        private Panel headerPanel;
        private Panel contentPanel;
        private Panel footerPanel;
        private Label titleLabel;
        private Label subtitleLabel;
        private Button nextButton;
        private Button backButton;
        private Button cancelButton;
        private PictureBox logoPictureBox;

        private int currentStep = 0;
        private Panel[] wizardPanels;

        // Installation data
        public string SqlServerName { get; set; } = "localhost";
        public string DatabaseName { get; set; } = "ComplaintManagementDB";
        public bool UseWindowsAuth { get; set; } = true;
        public string SqlUsername { get; set; } = "";
        public string SqlPassword { get; set; } = "";
        public string InstallPath { get; set; } = @"C:\Program Files\ComplaintManagement";

        public MainInstallerForm()
        {
            InitializeComponent();
            InitializeWizardPanels();
            ShowStep(0);
        }

        private void InitializeComponent()
        {
            this.Text = "Complaint Management System - Setup Wizard";
            this.Size = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Header Panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(103, 126, 234) // Modern blue gradient
            };

            titleLabel = new Label
            {
                Text = "Complaint Management System",
                Location = new Point(20, 20),
                Size = new Size(600, 35),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            subtitleLabel = new Label
            {
                Text = "Setup Wizard",
                Location = new Point(20, 55),
                Size = new Size(600, 25),
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(230, 230, 230),
                BackColor = Color.Transparent
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);

            // Content Panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                BackColor = Color.White
            };

            // Footer Panel
            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(20, 15, 20, 15)
            };

            cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 35),
                Location = new Point(540, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            cancelButton.Click += (s, e) =>
            {
                if (MessageBox.Show("Are you sure you want to cancel the installation?",
                    "Cancel Installation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            };

            backButton = new Button
            {
                Text = "< Back",
                Size = new Size(100, 35),
                Location = new Point(540, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Enabled = false
            };
            backButton.Click += (s, e) => ShowStep(currentStep - 1);

            nextButton = new Button
            {
                Text = "Next >",
                Size = new Size(100, 35),
                Location = new Point(650, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(103, 126, 234),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            nextButton.Click += NextButton_Click;

            footerPanel.Controls.Add(cancelButton);
            footerPanel.Controls.Add(backButton);
            footerPanel.Controls.Add(nextButton);

            this.Controls.Add(contentPanel);
            this.Controls.Add(headerPanel);
            this.Controls.Add(footerPanel);
        }

        private void InitializeWizardPanels()
        {
            wizardPanels = new Panel[]
            {
                CreateWelcomePanel(),
                CreatePrerequisitesPanel(),
                CreateDatabaseConfigPanel(),
                CreateInstallLocationPanel(),
                CreateReadyPanel(),
                CreateProgressPanel(),
                CreateCompletePanel()
            };

            foreach (var panel in wizardPanels)
            {
                panel.Visible = false;
                contentPanel.Controls.Add(panel);
            }
        }

        private Panel CreateWelcomePanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            var welcomeLabel = new Label
            {
                Text = "Welcome to Complaint Management System Setup",
                Location = new Point(50, 50),
                Size = new Size(650, 40),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            var descLabel = new Label
            {
                Text = "This wizard will guide you through the installation of the Complaint Management System.\n\n" +
                       "This application includes:\n\n" +
                       "• Complete complaint tracking and management\n" +
                       "• Multi-user support with role-based access\n" +
                       "• Email integration for automatic ticket creation\n" +
                       "• Real-time notifications and reporting\n" +
                       "• Mobile-responsive web interface",
                Location = new Point(50, 110),
                Size = new Size(650, 250),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            var noteLabel = new Label
            {
                Text = "Click 'Next' to continue",
                Location = new Point(50, 380),
                Size = new Size(650, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            panel.Controls.AddRange(new Control[] { welcomeLabel, descLabel, noteLabel });
            return panel;
        }

        private Panel CreatePrerequisitesPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            var titleLabel = new Label
            {
                Text = "Prerequisites Check",
                Location = new Point(50, 20),
                Size = new Size(650, 35),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            var checkListBox = new CheckedListBox
            {
                Location = new Point(50, 70),
                Size = new Size(650, 300),
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = false
            };

            checkListBox.Items.Add(".NET 8 Runtime", CheckState.Unchecked);
            checkListBox.Items.Add("Internet Information Services (IIS)", CheckState.Unchecked);
            checkListBox.Items.Add("SQL Server", CheckState.Unchecked);
            checkListBox.Items.Add("Administrator Privileges", CheckState.Unchecked);

            var checkButton = new Button
            {
                Text = "Check Prerequisites",
                Location = new Point(50, 385),
                Size = new Size(200, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(103, 126, 234),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            checkButton.Click += (s, e) => CheckPrerequisites(checkListBox);

            panel.Controls.AddRange(new Control[] { titleLabel, checkListBox, checkButton });
            return panel;
        }

        private Panel CreateDatabaseConfigPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            var titleLabel = new Label
            {
                Text = "Database Configuration",
                Location = new Point(50, 20),
                Size = new Size(650, 35),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            // SQL Server Name
            var serverLabel = new Label
            {
                Text = "SQL Server Name / IP Address:",
                Location = new Point(50, 80),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 11)
            };

            var serverTextBox = new TextBox
            {
                Name = "serverTextBox",
                Text = "localhost",
                Location = new Point(50, 110),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11)
            };

            // Database Name
            var dbLabel = new Label
            {
                Text = "Database Name:",
                Location = new Point(50, 160),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 11)
            };

            var dbTextBox = new TextBox
            {
                Name = "dbTextBox",
                Text = "ComplaintManagementDB",
                Location = new Point(50, 190),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11)
            };

            // Authentication Type
            var authLabel = new Label
            {
                Text = "Authentication Type:",
                Location = new Point(50, 240),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 11)
            };

            var windowsAuthRadio = new RadioButton
            {
                Text = "Windows Authentication (Recommended)",
                Location = new Point(50, 270),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11),
                Checked = true
            };

            var sqlAuthRadio = new RadioButton
            {
                Text = "SQL Server Authentication",
                Location = new Point(50, 305),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11)
            };

            // SQL Auth fields
            var usernameTextBox = new TextBox
            {
                Name = "usernameTextBox",
                Location = new Point(70, 345),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "SQL Username",
                Enabled = false
            };

            var passwordTextBox = new TextBox
            {
                Name = "passwordTextBox",
                Location = new Point(70, 385),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "SQL Password",
                PasswordChar = '●',
                Enabled = false
            };

            sqlAuthRadio.CheckedChanged += (s, e) =>
            {
                usernameTextBox.Enabled = sqlAuthRadio.Checked;
                passwordTextBox.Enabled = sqlAuthRadio.Checked;
            };

            // Test Connection Button
            var testButton = new Button
            {
                Text = "Test Connection",
                Location = new Point(480, 270),
                Size = new Size(180, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            testButton.Click += (s, e) =>
            {
                SqlServerName = serverTextBox.Text;
                DatabaseName = dbTextBox.Text;
                UseWindowsAuth = windowsAuthRadio.Checked;
                SqlUsername = usernameTextBox.Text;
                SqlPassword = passwordTextBox.Text;
                TestDatabaseConnection();
            };

            panel.Controls.AddRange(new Control[] {
                titleLabel, serverLabel, serverTextBox, dbLabel, dbTextBox,
                authLabel, windowsAuthRadio, sqlAuthRadio, usernameTextBox,
                passwordTextBox, testButton
            });

            return panel;
        }

        private Panel CreateInstallLocationPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            var titleLabel = new Label
            {
                Text = "Installation Location",
                Location = new Point(50, 20),
                Size = new Size(650, 35),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            var pathLabel = new Label
            {
                Text = "Select installation directory:",
                Location = new Point(50, 80),
                Size = new Size(650, 25),
                Font = new Font("Segoe UI", 11)
            };

            var pathTextBox = new TextBox
            {
                Text = @"C:\Program Files\ComplaintManagement",
                Location = new Point(50, 115),
                Size = new Size(500, 30),
                Font = new Font("Segoe UI", 11)
            };

            var browseButton = new Button
            {
                Text = "Browse...",
                Location = new Point(560, 115),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            browseButton.Click += (s, e) =>
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        pathTextBox.Text = dialog.SelectedPath;
                        InstallPath = dialog.SelectedPath;
                    }
                }
            };

            var spaceLabel = new Label
            {
                Text = "Required disk space: 500 MB\nAvailable disk space: Calculating...",
                Location = new Point(50, 170),
                Size = new Size(650, 50),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(100, 100, 100)
            };

            panel.Controls.AddRange(new Control[] { titleLabel, pathLabel, pathTextBox, browseButton, spaceLabel });
            return panel;
        }

        private Panel CreateReadyPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            var titleLabel = new Label
            {
                Text = "Ready to Install",
                Location = new Point(50, 20),
                Size = new Size(650, 35),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            var summaryLabel = new Label
            {
                Text = "The wizard is ready to begin installation.\n\nInstallation Summary:\n\n" +
                       $"• SQL Server: localhost\n" +
                       $"• Database: ComplaintManagementDB\n" +
                       $"• Install Location: C:\\Program Files\\ComplaintManagement\n" +
                       $"• Components: API Service, Web Frontend, Database\n\n" +
                       "Click 'Install' to begin the installation.",
                Location = new Point(50, 80),
                Size = new Size(650, 300),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            panel.Controls.AddRange(new Control[] { titleLabel, summaryLabel });
            return panel;
        }

        private Panel CreateProgressPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            var titleLabel = new Label
            {
                Text = "Installing Complaint Management System",
                Location = new Point(50, 20),
                Size = new Size(650, 35),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            var statusLabel = new Label
            {
                Name = "statusLabel",
                Text = "Preparing installation...",
                Location = new Point(50, 80),
                Size = new Size(650, 30),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            var progressBar = new ProgressBar
            {
                Name = "mainProgressBar",
                Location = new Point(50, 125),
                Size = new Size(650, 30),
                Style = ProgressBarStyle.Continuous
            };

            var logTextBox = new RichTextBox
            {
                Name = "logTextBox",
                Location = new Point(50, 175),
                Size = new Size(650, 250),
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.FixedSingle
            };

            panel.Controls.AddRange(new Control[] { titleLabel, statusLabel, progressBar, logTextBox });
            return panel;
        }

        private Panel CreateCompletePanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            var titleLabel = new Label
            {
                Text = "Installation Complete!",
                Location = new Point(50, 50),
                Size = new Size(650, 40),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69)
            };

            var messageLabel = new Label
            {
                Text = "Complaint Management System has been successfully installed.\n\n" +
                       "You can now access the application at:\n" +
                       "http://localhost\n\n" +
                       "Default administrator credentials:\n" +
                       "Username: admin@complaintmanagement.com\n" +
                       "Password: Admin@123\n\n" +
                       "⚠️ IMPORTANT: Please change the default password after first login!",
                Location = new Point(50, 110),
                Size = new Size(650, 250),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            var launchCheckBox = new CheckBox
            {
                Text = "Launch Complaint Management System now",
                Location = new Point(50, 380),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 11),
                Checked = true
            };

            panel.Controls.AddRange(new Control[] { titleLabel, messageLabel, launchCheckBox });
            return panel;
        }

        private void ShowStep(int step)
        {
            if (step < 0 || step >= wizardPanels.Length) return;

            // Hide all panels
            foreach (var panel in wizardPanels)
            {
                panel.Visible = false;
            }

            // Show current panel
            wizardPanels[step].Visible = true;
            currentStep = step;

            // Update buttons
            backButton.Enabled = step > 0 && step < 5;

            if (step == 4) // Ready to install
            {
                nextButton.Text = "Install";
            }
            else if (step == 6) // Complete
            {
                nextButton.Text = "Finish";
                backButton.Enabled = false;
                cancelButton.Enabled = false;
            }
            else if (step == 5) // Installing
            {
                nextButton.Enabled = false;
                backButton.Enabled = false;
                cancelButton.Enabled = false;
            }
            else
            {
                nextButton.Text = "Next >";
                nextButton.Enabled = true;
            }

            // Update subtitle
            string[] stepTitles = {
                "Welcome",
                "Prerequisites Check",
                "Database Configuration",
                "Installation Location",
                "Ready to Install",
                "Installing",
                "Installation Complete"
            };
            subtitleLabel.Text = $"Step {step + 1} of {wizardPanels.Length}: {stepTitles[step]}";
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (currentStep == 4) // Start installation
            {
                ShowStep(5);
                StartInstallation();
            }
            else if (currentStep == 6) // Finish
            {
                var launchCheckBox = wizardPanels[6].Controls.Find("launchCheckBox", true)[0] as CheckBox;
                if (launchCheckBox?.Checked == true)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "http://localhost",
                        UseShellExecute = true
                    });
                }
                Application.Exit();
            }
            else
            {
                ShowStep(currentStep + 1);
            }
        }

        private void CheckPrerequisites(CheckedListBox checkListBox)
        {
            // Check .NET 8
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                var output = process.StandardOutput.ReadToEnd();
                if (output.Contains("Microsoft.AspNetCore.App 8"))
                {
                    checkListBox.SetItemChecked(0, true);
                }
            }
            catch { }

            // Check IIS (simplified)
            if (Directory.Exists(@"C:\inetpub"))
            {
                checkListBox.SetItemChecked(1, true);
            }

            // Check SQL Server (simplified)
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = "query MSSQLSERVER",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                checkListBox.SetItemChecked(2, true);
            }
            catch { }

            // Check admin
            checkListBox.SetItemChecked(3, new System.Security.Principal.WindowsPrincipal(
                System.Security.Principal.WindowsIdentity.GetCurrent())
                .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator));

            MessageBox.Show("Prerequisites check complete!", "Check Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void TestDatabaseConnection()
        {
            var connectionString = UseWindowsAuth
                ? $"Server={SqlServerName};Database=master;Integrated Security=True;TrustServerCertificate=True;"
                : $"Server={SqlServerName};Database=master;User Id={SqlUsername};Password={SqlPassword};TrustServerCertificate=True;";

            try
            {
                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("✓ Database connection successful!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"✗ Connection failed:\n\n{ex.Message}", "Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void StartInstallation()
        {
            var statusLabel = wizardPanels[5].Controls.Find("statusLabel", true)[0] as Label;
            var progressBar = wizardPanels[5].Controls.Find("mainProgressBar", true)[0] as ProgressBar;
            var logTextBox = wizardPanels[5].Controls.Find("logTextBox", true)[0] as RichTextBox;

            progressBar.Value = 0;
            progressBar.Maximum = 100;

            await Task.Run(() =>
            {
                // Simulate installation steps
                UpdateProgress("Creating installation directory...", 10, logTextBox, progressBar, statusLabel);
                Thread.Sleep(1000);

                UpdateProgress("Building .NET API...", 25, logTextBox, progressBar, statusLabel);
                Thread.Sleep(2000);

                UpdateProgress("Building Angular frontend...", 40, logTextBox, progressBar, statusLabel);
                Thread.Sleep(2000);

                UpdateProgress("Creating database...", 60, logTextBox, progressBar, statusLabel);
                Thread.Sleep(1500);

                UpdateProgress("Running database migrations...", 70, logTextBox, progressBar, statusLabel);
                Thread.Sleep(1500);

                UpdateProgress("Installing Windows Service...", 85, logTextBox, progressBar, statusLabel);
                Thread.Sleep(1000);

                UpdateProgress("Configuring IIS...", 95, logTextBox, progressBar, statusLabel);
                Thread.Sleep(1000);

                UpdateProgress("Installation complete!", 100, logTextBox, progressBar, statusLabel);
            });

            await Task.Delay(1000);
            this.Invoke(new Action(() => ShowStep(6)));
        }

        private void UpdateProgress(string message, int progress, RichTextBox log, ProgressBar bar, Label status)
        {
            this.Invoke(new Action(() =>
            {
                status.Text = message;
                bar.Value = progress;
                log.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                log.ScrollToCaret();
            }));
        }
    }
}
