using Dapplo.Confluence;

namespace ConfluenceCopier;

public partial class SettingsForm : Form
{
    private TextBox txtConfluenceUrl = null!;
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private TextBox txtApiKey = null!;
    private Button btnShowHidePassword = null!;
    private Button btnShowHideApiKey = null!;
    private Button btnTestConnection = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private Label lblConfluenceUrl = null!;
    private Label lblUsername = null!;
    private Label lblPassword = null!;
    private Label lblApiKey = null!;
    private RadioButton rbPassword = null!;
    private RadioButton rbApiKey = null!;
    private Label lblAuthType = null!;

    private AppSettings settings;
    private bool isLoadingSettings = false;

    public SettingsForm(AppSettings currentSettings)
    {
        settings = currentSettings;
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        Text = "Confluence Settings";
        Size = new Size(580, 290);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Confluence URL
        lblConfluenceUrl = new Label
        {
            Text = "Confluence URL:",
            Location = new Point(20, 20),
            Size = new Size(120, 23)
        };
        Controls.Add(lblConfluenceUrl);

        txtConfluenceUrl = new TextBox
        {
            Location = new Point(150, 20),
            Size = new Size(400, 23),
            PlaceholderText = "https://yourcompany.atlassian.net"
        };
        Controls.Add(txtConfluenceUrl);

        // Username
        lblUsername = new Label
        {
            Text = "Username/Email:",
            Location = new Point(20, 60),
            Size = new Size(120, 23)
        };
        Controls.Add(lblUsername);

        txtUsername = new TextBox
        {
            Location = new Point(150, 60),
            Size = new Size(400, 23),
            PlaceholderText = "your.email@company.com"
        };
        Controls.Add(txtUsername);

        // Authentication Type
        lblAuthType = new Label
        {
            Text = "Authentication:",
            Location = new Point(20, 100),
            Size = new Size(120, 23),
            Font = new Font(Font, FontStyle.Bold)
        };
        Controls.Add(lblAuthType);

        rbPassword = new RadioButton
        {
            Text = "Username + Password",
            Location = new Point(150, 100),
            Size = new Size(180, 23),
            Checked = true
        };
        rbPassword.CheckedChanged += RbAuthType_CheckedChanged;
        Controls.Add(rbPassword);

        rbApiKey = new RadioButton
        {
            Text = "API Key",
            Location = new Point(350, 100),
            Size = new Size(120, 23)
        };
        rbApiKey.CheckedChanged += RbAuthType_CheckedChanged;
        Controls.Add(rbApiKey);

        // Password Section
        lblPassword = new Label
        {
            Text = "Password:",
            Location = new Point(20, 130),
            Size = new Size(120, 23)
        };
        Controls.Add(lblPassword);

        txtPassword = new TextBox
        {
            Location = new Point(150, 130),
            Size = new Size(360, 23),
            UseSystemPasswordChar = true,
            PlaceholderText = "Your Confluence password/API token"
        };
        Controls.Add(txtPassword);

        btnShowHidePassword = new Button
        {
            Text = "👁",
            Location = new Point(520, 130),
            Size = new Size(30, 23),
            FlatStyle = FlatStyle.Flat
        };
        btnShowHidePassword.Click += BtnShowHidePassword_Click;
        Controls.Add(btnShowHidePassword);

        // API Key Section
        lblApiKey = new Label
        {
            Text = "API Key:",
            Location = new Point(20, 130),
            Size = new Size(120, 23),
            Visible = false
        };
        Controls.Add(lblApiKey);

        txtApiKey = new TextBox
        {
            Location = new Point(150, 130),
            Size = new Size(360, 23),
            UseSystemPasswordChar = true,
            PlaceholderText = "Your API key",
            Visible = false
        };
        Controls.Add(txtApiKey);

        btnShowHideApiKey = new Button
        {
            Text = "👁",
            Location = new Point(520, 130),
            Size = new Size(30, 23),
            FlatStyle = FlatStyle.Flat,
            Visible = false
        };
        btnShowHideApiKey.Click += BtnShowHideApiKey_Click;
        Controls.Add(btnShowHideApiKey);

        // Test Connection Button
        btnTestConnection = new Button
        {
            Text = "Test Connection",
            Location = new Point(150, 170),
            Size = new Size(120, 30)
        };
        btnTestConnection.Click += BtnTestConnection_Click;
        Controls.Add(btnTestConnection);

        // Buttons
        btnSave = new Button
        {
            Text = "Save",
            Location = new Point(380, 210),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK
        };
        btnSave.Click += BtnSave_Click;
        Controls.Add(btnSave);

        btnCancel = new Button
        {
            Text = "Cancel",
            Location = new Point(470, 210),
            Size = new Size(80, 30),
            DialogResult = DialogResult.Cancel
        };
        Controls.Add(btnCancel);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    private void LoadSettings()
    {
        isLoadingSettings = true;

        txtConfluenceUrl.Text = settings.ConfluenceUrl;
        txtUsername.Text = settings.Username;
        txtPassword.Text = settings.Password;
        txtApiKey.Text = settings.ApiKey;

        // Set authentication type
        if (settings.AuthType == AuthenticationType.ApiKey)
            rbApiKey.Checked = true;
        else
            rbPassword.Checked = true;

        UpdateAuthFieldsVisibility();

        isLoadingSettings = false;
    }

    private void SaveSettings()
    {
        if (isLoadingSettings) return;

        settings.ConfluenceUrl = txtConfluenceUrl.Text;
        settings.Username = txtUsername.Text;
        settings.Password = txtPassword.Text;
        settings.ApiKey = txtApiKey.Text;
        settings.AuthType = rbApiKey.Checked ? AuthenticationType.ApiKey : AuthenticationType.Password;
        settings.Save();
    }

    private void BtnShowHidePassword_Click(object? sender, EventArgs e)
    {
        txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
        btnShowHidePassword.Text = txtPassword.UseSystemPasswordChar ? "👁" : "🙈";
    }

    private void BtnShowHideApiKey_Click(object? sender, EventArgs e)
    {
        txtApiKey.UseSystemPasswordChar = !txtApiKey.UseSystemPasswordChar;
        btnShowHideApiKey.Text = txtApiKey.UseSystemPasswordChar ? "👁" : "🙈";
    }

    private void RbAuthType_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateAuthFieldsVisibility();
    }

    private void UpdateAuthFieldsVisibility()
    {
        var isPassword = rbPassword.Checked;
        var isApiKey = rbApiKey.Checked;

        if (isPassword)
        {
            // Show Username + Password fields
            lblUsername.Visible = true;
            txtUsername.Visible = true;
            lblPassword.Visible = true;
            txtPassword.Visible = true;
            btnShowHidePassword.Visible = true;

            // Hide API Key fields
            lblApiKey.Visible = false;
            txtApiKey.Visible = false;
            btnShowHideApiKey.Visible = false;

            // Position Username fields at Y=60
            lblUsername.Location = new Point(20, 60);
            txtUsername.Location = new Point(150, 60);

            // Position Password fields at Y=130
            lblPassword.Location = new Point(20, 130);
            txtPassword.Location = new Point(150, 130);
            btnShowHidePassword.Location = new Point(520, 130);
        }
        else if (rbApiKey.Checked)
        {
            // Hide Username + Password fields
            lblUsername.Visible = false;
            txtUsername.Visible = false;
            lblPassword.Visible = false;
            txtPassword.Visible = false;
            btnShowHidePassword.Visible = false;

            // Show API Key fields and position them at Y=130 (where password was)
            lblApiKey.Visible = true;
            txtApiKey.Visible = true;
            btnShowHideApiKey.Visible = true;

            lblApiKey.Location = new Point(20, 130);
            txtApiKey.Location = new Point(150, 130);
            btnShowHideApiKey.Location = new Point(520, 130);
        }
    }

    private async void BtnTestConnection_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtConfluenceUrl.Text))
        {
            MessageBox.Show("Please enter the Confluence URL before testing the connection.",
                "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (rbPassword.Checked)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill in both username and password before testing the connection.",
                    "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        else if (rbApiKey.Checked)
        {
            if (string.IsNullOrWhiteSpace(txtApiKey.Text))
            {
                MessageBox.Show("Please enter the API key before testing the connection.",
                    "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        // Disable button and show testing state
        btnTestConnection.Enabled = false;
        var originalText = btnTestConnection.Text;
        btnTestConnection.Text = "Testing...";

        try
        {
            await TestConfluenceConnection();

            // Success
            btnTestConnection.Text = "✓ Success";
            btnTestConnection.BackColor = Color.LightGreen;

            MessageBox.Show("Connection successful! Your Confluence settings are valid.",
                "Test Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            // Error
            btnTestConnection.Text = "✗ Failed";
            btnTestConnection.BackColor = Color.LightCoral;

            MessageBox.Show($"Connection failed: {ex.Message}\n\nPlease check your settings and try again.",
                "Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Reset button after 2 seconds
            var resetTimer = new System.Windows.Forms.Timer();
            resetTimer.Interval = 2000;
            resetTimer.Tick += (s, args) =>
            {
                btnTestConnection.Text = originalText;
                btnTestConnection.BackColor = SystemColors.Control;
                btnTestConnection.Enabled = true;
                resetTimer.Stop();
                resetTimer.Dispose();
            };
            resetTimer.Start();
        }
    }

    private async Task TestConfluenceConnection()
    {
        var baseUrl = txtConfluenceUrl.Text.TrimEnd('/');

        // Validate URL format
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var confluenceUri))
            throw new ArgumentException(
                "Invalid Confluence URL format. Please ensure it starts with https:// or http://");

        // Create Confluence client
        var confluenceClient = ConfluenceClient.Create(confluenceUri);

        // Set authentication based on selected type
        if (rbPassword.Checked)
        {
            var username = txtUsername.Text;
            var password = txtPassword.Text;
            confluenceClient.SetBasicAuthentication(username, password);
        }
        else if (rbApiKey.Checked)
        {
            var apiKey = txtApiKey.Text;
            confluenceClient.SetBearerAuthentication(apiKey);
        }

        // Test connection by getting user info
        try
        {
            var currentUser = await confluenceClient.User.GetCurrentUserAsync();

            if (currentUser == null)
            {
                var authType = rbApiKey.Checked ? "API key" : "username and password";
                throw new Exception($"Authentication failed. Please check your {authType}.");
            }

            // Optionally test a simple API call to ensure we have basic permissions
            await confluenceClient.Space.GetAllAsync();
        }
        catch (HttpRequestException httpEx)
        {
            var authType = rbApiKey.Checked ? "API key" : "username and password";

            if (httpEx.Message.Contains("401"))
                throw new Exception($"Authentication failed. Please verify your {authType} is correct.");
            else if (httpEx.Message.Contains("403"))
                throw new Exception("Access denied. Your account may not have sufficient permissions.");
            else if (httpEx.Message.Contains("404"))
                throw new Exception("Confluence instance not found. Please verify the URL is correct.");
            else
                throw new Exception($"Network error: {httpEx.Message}");
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtConfluenceUrl.Text))
        {
            MessageBox.Show("Please enter the Confluence URL.", "Missing Information",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (rbPassword.Checked)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill in both username and password.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        else if (rbApiKey.Checked)
        {
            if (string.IsNullOrWhiteSpace(txtApiKey.Text))
            {
                MessageBox.Show("Please enter the API key.", "Missing Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        SaveSettings();
        Close();
    }

    public AppSettings GetSettings()
    {
        return settings;
    }
}