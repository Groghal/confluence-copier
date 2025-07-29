namespace ConfluenceCopier;

public partial class SettingsForm : Form
{
    private TextBox txtConfluenceUrl = null!;
    private TextBox txtUsername = null!;
    private TextBox txtApiToken = null!;
    private Button btnShowHideToken = null!;
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
        Size = new Size(580, 220);
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

        // Buttons
        btnSave = new Button
        {
            Text = "Save",
            Location = new Point(380, 140),
            Size = new Size(80, 30),
            DialogResult = DialogResult.OK
        };
        btnSave.Click += BtnSave_Click;
        Controls.Add(btnSave);

        btnCancel = new Button
        {
            Text = "Cancel",
            Location = new Point(470, 140),
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