using Dapplo.Confluence;

namespace ConfluenceCopier;

public partial class SettingsForm : Form
{
    private TextBox txtConfluenceUrl = null!;
    private TextBox txtUsername = null!;
    private TextBox txtApiToken = null!;
    private Button btnShowHideToken = null!;
    private Button btnTestConnection = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private Label lblConfluenceUrl = null!;
    private Label lblUsername = null!;
    private Label lblApiToken = null!;

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
        Size = new Size(580, 260);
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

        // API Token
        lblApiToken = new Label
        {
            Text = "API Token:",
            Location = new Point(20, 100),
            Size = new Size(120, 23)
        };
        Controls.Add(lblApiToken);

        txtApiToken = new TextBox
        {
            Location = new Point(150, 100),
            Size = new Size(360, 23),
            UseSystemPasswordChar = true,
            PlaceholderText = "Your Confluence API token"
        };
        Controls.Add(txtApiToken);

        btnShowHideToken = new Button
        {
            Text = "👁",
            Location = new Point(520, 100),
            Size = new Size(30, 23),
            FlatStyle = FlatStyle.Flat
        };
        btnShowHideToken.Click += BtnShowHideToken_Click;
        Controls.Add(btnShowHideToken);

        // Test Connection Button
        btnTestConnection = new Button
        {
            Text = "Test Connection",
            Location = new Point(150, 140),
            Size = new Size(120, 30)
        };
        btnTestConnection.Click += BtnTestConnection_Click;
        Controls.Add(btnTestConnection);

        // Buttons
        btnSave = new Button
        {
            Text = "Save",
            Location = new Point(380, 180),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK
        };
        btnSave.Click += BtnSave_Click;
        Controls.Add(btnSave);

        btnCancel = new Button
        {
            Text = "Cancel",
            Location = new Point(470, 180),
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
        txtApiToken.Text = settings.ApiToken;

        isLoadingSettings = false;
    }

    private void SaveSettings()
    {
        if (isLoadingSettings) return;

        settings.ConfluenceUrl = txtConfluenceUrl.Text;
        settings.Username = txtUsername.Text;
        settings.ApiToken = txtApiToken.Text;
        settings.Save();
    }

    private void BtnShowHideToken_Click(object? sender, EventArgs e)
    {
        txtApiToken.UseSystemPasswordChar = !txtApiToken.UseSystemPasswordChar;
        btnShowHideToken.Text = txtApiToken.UseSystemPasswordChar ? "👁" : "🙈";
    }

    private async void BtnTestConnection_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtConfluenceUrl.Text) ||
            string.IsNullOrWhiteSpace(txtUsername.Text) ||
            string.IsNullOrWhiteSpace(txtApiToken.Text))
        {
            MessageBox.Show("Please fill in all fields before testing the connection.",
                "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
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
        var username = txtUsername.Text;
        var apiToken = txtApiToken.Text;

        // Validate URL format
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var confluenceUri))
            throw new ArgumentException(
                "Invalid Confluence URL format. Please ensure it starts with https:// or http://");

        // Create Confluence client
        var confluenceClient = ConfluenceClient.Create(confluenceUri);
        confluenceClient.SetBasicAuthentication(username, apiToken);

        // Test connection by getting user info
        try
        {
            var currentUser = await confluenceClient.User.GetCurrentUserAsync();

            if (currentUser == null)
                throw new Exception("Authentication failed. Please check your username and API token.");

            // Optionally test a simple API call to ensure we have basic permissions
            await confluenceClient.Space.GetAllAsync();
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.Message.Contains("401"))
                throw new Exception("Authentication failed. Please verify your username and API token are correct.");
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
        if (string.IsNullOrWhiteSpace(txtConfluenceUrl.Text) ||
            string.IsNullOrWhiteSpace(txtUsername.Text) ||
            string.IsNullOrWhiteSpace(txtApiToken.Text))
        {
            MessageBox.Show("Please fill in all fields.", "Missing Information",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SaveSettings();
        Close();
    }

    public AppSettings GetSettings()
    {
        return settings;
    }
}