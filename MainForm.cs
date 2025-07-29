using System.Diagnostics;
using System.Text.RegularExpressions;
using Dapplo.Confluence;
using Dapplo.Confluence.Entities;
using Dapplo.Confluence.Query;
using Label = System.Windows.Forms.Label;

namespace ConfluenceCopier;

public partial class MainForm : Form
{
    private TextBox txtCopyFromPage = null!;
    private TextBox txtCopyToPage = null!;
    private Button btnCopy = null!;
    private Button btnSettings = null!;
    private Label lblCopyFromPage = null!;
    private Label lblCopyToPage = null!;
    private Label lblCopyFromPath = null!;
    private Label lblCopyToPath = null!;
    private Label lblStatusLabel = null!;
    private Label lblStatus = null!;

    private AppSettings settings;
    private IConfluenceClient? confluenceClient;
    private System.Windows.Forms.Timer? debounceTimer;
    private DateTime lastApiCall = DateTime.MinValue;
    private readonly TimeSpan apiRateLimit = TimeSpan.FromSeconds(1);

    public MainForm()
    {
        settings = AppSettings.Load();
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Confluence Page Copier";
        Size = new Size(600, 350);
        StartPosition = FormStartPosition.CenterScreen;

        // Settings Button
        btnSettings = new Button
        {
            Text = "⚙ Settings",
            Location = new Point(480, 20),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Standard
        };
        btnSettings.Click += BtnSettings_Click;
        Controls.Add(btnSettings);

        // Copy From Section
        lblCopyFromPage = new Label
        {
            Text = "Copy From:",
            Location = new Point(20, 70),
            Size = new Size(120, 23),
            Font = new Font(Font, FontStyle.Bold)
        };
        Controls.Add(lblCopyFromPage);

        lblCopyFromPath = new Label
        {
            Text = "Select a page to see its path",
            Location = new Point(150, 70),
            Size = new Size(430, 20),
            ForeColor = Color.Gray,
            Font = new Font(Font.FontFamily, Font.Size - 1)
        };
        Controls.Add(lblCopyFromPath);

        txtCopyFromPage = new TextBox
        {
            Location = new Point(150, 95),
            Size = new Size(430, 23),
            PlaceholderText = "Enter page ID or URL (e.g., 123456 or https://company.atlassian.net/wiki/spaces/...)"
        };
        txtCopyFromPage.TextChanged += TxtCopyFromPage_TextChanged;
        Controls.Add(txtCopyFromPage);

        // Copy To Section
        lblCopyToPage = new Label
        {
            Text = "Copy To:",
            Location = new Point(20, 135),
            Size = new Size(120, 23),
            Font = new Font(Font, FontStyle.Bold)
        };
        Controls.Add(lblCopyToPage);

        lblCopyToPath = new Label
        {
            Text = "Select a page to see its path",
            Location = new Point(150, 135),
            Size = new Size(430, 20),
            ForeColor = Color.Gray,
            Font = new Font(Font.FontFamily, Font.Size - 1)
        };
        Controls.Add(lblCopyToPath);

        txtCopyToPage = new TextBox
        {
            Location = new Point(150, 160),
            Size = new Size(430, 23),
            PlaceholderText = "Enter page ID or URL (e.g., 123456 or https://company.atlassian.net/wiki/spaces/...)"
        };
        txtCopyToPage.TextChanged += TxtCopyToPage_TextChanged;
        Controls.Add(txtCopyToPage);

        // Copy Button
        btnCopy = new Button
        {
            Text = "Copy From → Copy To",
            Location = new Point(200, 200),
            Size = new Size(200, 35)
        };
        btnCopy.Click += BtnCopy_Click;
        Controls.Add(btnCopy);

        // Status Label
        lblStatusLabel = new Label
        {
            Text = "Status:",
            Location = new Point(20, 250),
            Size = new Size(50, 20),
            ForeColor = Color.Black
        };
        Controls.Add(lblStatusLabel);

        lblStatus = new Label
        {
            Text = "Ready",
            Location = new Point(75, 250),
            Size = new Size(500, 40),
            ForeColor = Color.Blue
        };
        Controls.Add(lblStatus);
        // Initialize debounce timer
        debounceTimer = new System.Windows.Forms.Timer();
        debounceTimer.Interval = 1000; // 1 second delay
        debounceTimer.Tick += DebounceTimer_Tick;
    }

    private void TxtCopyFromPage_TextChanged(object? sender, EventArgs e)
    {
        RestartDebounceTimer();
    }

    private void TxtCopyToPage_TextChanged(object? sender, EventArgs e)
    {
        RestartDebounceTimer();
    }

    private void RestartDebounceTimer()
    {
        debounceTimer?.Stop();
        debounceTimer?.Start();
    }

    private async void DebounceTimer_Tick(object? sender, EventArgs e)
    {
        debounceTimer?.Stop();
        await FetchPageInformation();
    }

    private async Task FetchPageInformation()
    {
        // Check if we have valid settings
        if (string.IsNullOrWhiteSpace(settings.ConfluenceUrl) ||
            string.IsNullOrWhiteSpace(settings.Username) ||
            string.IsNullOrWhiteSpace(settings.ApiToken))
            return; // Don't fetch if settings aren't configured

        // Rate limiting
        var timeSinceLastCall = DateTime.Now - lastApiCall;
        if (timeSinceLastCall < apiRateLimit) return;

        try
        {
            lastApiCall = DateTime.Now;

            // Initialize client if needed
            if (confluenceClient == null)
            {
                var confluenceUri = new Uri(settings.ConfluenceUrl.TrimEnd('/'));
                confluenceClient = ConfluenceClient.Create(confluenceUri);
                confluenceClient.SetBasicAuthentication(settings.Username, settings.ApiToken);
            }

            // Fetch copy from page info
            if (!string.IsNullOrWhiteSpace(txtCopyFromPage.Text))
            {
                try
                {
                    var copyFromPageId = ExtractPageId(txtCopyFromPage.Text);
                    var copyFromPage = await confluenceClient.Content.GetAsync(long.Parse(copyFromPageId),
                        ConfluenceClientConfig.ExpandGetContent);

                    if (copyFromPage != null)
                    {
                        var copyFromHierarchy = await GetPageHierarchy(copyFromPage);
                        lblCopyFromPath.Text = copyFromHierarchy;
                        lblCopyFromPath.ForeColor = Color.DarkBlue;
                    }
                }
                catch
                {
                    lblCopyFromPath.Text = "Invalid page or page not found";
                    lblCopyFromPath.ForeColor = Color.Red;
                }
            }
            else
            {
                lblCopyFromPath.Text = "Select a page to see its path";
                lblCopyFromPath.ForeColor = Color.Gray;
            }

            // Fetch copy to page info
            if (!string.IsNullOrWhiteSpace(txtCopyToPage.Text))
            {
                try
                {
                    var copyToPageId = ExtractPageId(txtCopyToPage.Text);
                    var copyToPage = await confluenceClient.Content.GetAsync(long.Parse(copyToPageId),
                        ConfluenceClientConfig.ExpandGetContent);

                    if (copyToPage != null)
                    {
                        var copyToHierarchy = await GetPageHierarchy(copyToPage);
                        lblCopyToPath.Text = copyToHierarchy;
                        lblCopyToPath.ForeColor = Color.DarkBlue;
                    }
                }
                catch
                {
                    lblCopyToPath.Text = "Invalid page or page not found";
                    lblCopyToPath.ForeColor = Color.Red;
                }
            }
            else
            {
                lblCopyToPath.Text = "Select a page to see its path";
                lblCopyToPath.ForeColor = Color.Gray;
            }

            lblStatus.Text = "Page information updated";
            lblStatus.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            // Don't crash the app, just show a brief error
            lblStatus.Text = $"Error fetching page info: {ex.Message}";
            lblStatus.ForeColor = Color.Orange;

            // Reset path labels on error
            if (string.IsNullOrWhiteSpace(txtCopyFromPage.Text))
            {
                lblCopyFromPath.Text = "Select a page to see its path";
                lblCopyFromPath.ForeColor = Color.Gray;
            }

            if (string.IsNullOrWhiteSpace(txtCopyToPage.Text))
            {
                lblCopyToPath.Text = "Select a page to see its path";
                lblCopyToPath.ForeColor = Color.Gray;
            }

            // Clear error after 3 seconds
            var errorTimer = new System.Windows.Forms.Timer();
            errorTimer.Interval = 3000;
            errorTimer.Tick += (s, args) =>
            {
                lblStatus.Text = "Ready";
                lblStatus.ForeColor = Color.Blue;
                errorTimer.Stop();
                errorTimer.Dispose();
            };
            errorTimer.Start();
        }
    }

    private async Task<string> GetPageHierarchy(Content page)
    {
        try
        {
            var hierarchy = new List<string>();

            // Get space info
            if (page.Space != null) hierarchy.Add(page.Space.Name ?? page.Space.Key ?? "Unknown Space");

            // Get parent pages
            var currentPage = page;
            var ancestors = new List<string>();

            // Try to get ancestors (parent pages)
            try
            {
                var pageWithAncestors = await confluenceClient!.Content.GetAsync(page.Id,
                    new[] { "ancestors" });
                if (pageWithAncestors?.Ancestors != null)
                    foreach (var ancestor in pageWithAncestors.Ancestors.Reverse())
                        if (!string.IsNullOrEmpty(ancestor.Title))
                            ancestors.Add(ancestor.Title);
            }
            catch
            {
                // If we can't get ancestors, just continue
            }

            // Build hierarchy string
            hierarchy.AddRange(ancestors);
            hierarchy.Add(page.Title ?? "Untitled Page");

            return string.Join(" > ", hierarchy);
        }
        catch
        {
            return page.Title ?? "Untitled Page";
        }
    }

    private void BtnSettings_Click(object? sender, EventArgs e)
    {
        using var settingsForm = new SettingsForm(settings);
        if (settingsForm.ShowDialog() == DialogResult.OK)
        {
            settings = settingsForm.GetSettings();
            lblStatus.Text = "Settings saved successfully.";
            lblStatus.ForeColor = Color.Green;
        }
    }

    private string ExtractPageId(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // If it's already just a number, return it
        if (long.TryParse(input.Trim(), out _))
            return input.Trim();

        // Try to extract page ID from various URL formats
        var patterns = new[]
        {
            // Standard wiki URL: https://domain/wiki/spaces/SPACE/pages/123456/Title
            @"/pages/(\d+)(?:/|$)",
            // View page action: https://domain/pages/viewpage.action?pageId=123456
            @"pageId=(\d+)",
            // Direct page URL: https://domain/display/SPACE/Title?pageId=123456
            @"pageId=(\d+)",
            // Any other pattern with pageId parameter
            @"[?&]pageId=(\d+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1) return match.Groups[1].Value;
        }

        // If no pattern matches, assume it might be a malformed input
        throw new ArgumentException($"Could not extract page ID from: {input}. Please provide a valid page ID or URL.");
    }

    private async void BtnCopy_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(settings.ConfluenceUrl) ||
            string.IsNullOrWhiteSpace(settings.Username) ||
            string.IsNullOrWhiteSpace(settings.ApiToken))
        {
            MessageBox.Show("Please configure your Confluence settings first using the Settings button.",
                "Settings Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtCopyFromPage.Text) ||
            string.IsNullOrWhiteSpace(txtCopyToPage.Text))
        {
            MessageBox.Show("Please enter both copy from and copy to page information.",
                "Missing Page Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnCopy.Enabled = false;
        lblStatus.Text = "Copying content...";
        lblStatus.ForeColor = Color.Blue;

        try
        {
            await CopyPageContent();
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Error: {ex.Message}";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnCopy.Enabled = true;
        }
    }

    private async Task CopyPageContent()
    {
        var baseUrl = settings.ConfluenceUrl.TrimEnd('/');
        var username = settings.Username;
        var apiToken = settings.ApiToken;

        // Extract page IDs from input (supports both IDs and URLs)
        var copyFromPageId = ExtractPageId(txtCopyFromPage.Text);
        var copyToPageId = ExtractPageId(txtCopyToPage.Text);

        // Initialize Confluence client
        var confluenceUri = new Uri(baseUrl);
        confluenceClient = ConfluenceClient.Create(confluenceUri);
        confluenceClient.SetBasicAuthentication(username, apiToken);

        lblStatus.Text = "Fetching copy from page content...";

        // Get copy from page content with body expansion
        var copyFromPage = await confluenceClient.Content.GetAsync(long.Parse(copyFromPageId),
            ConfluenceClientConfig.ExpandGetContent);

        if (copyFromPage?.Body?.Storage?.Value == null)
            throw new Exception("Failed to fetch copy from page content or page not found.");

        var copyFromContent = copyFromPage.Body.Storage.Value;
        var copyFromTitle = copyFromPage.Title;

        lblStatus.Text = "Fetching copy to page info...";

        // Get copy to page info
        var copyToPage = await confluenceClient.Content.GetAsync(long.Parse(copyToPageId));

        if (copyToPage == null) throw new Exception("Failed to fetch copy to page or page not found.");

        var copyToTitle = copyToPage.Title;
        var currentVersion = copyToPage.Version.Number;

        lblStatus.Text = "Updating copy to page...";

        // Create updated content
        var updatedContent = new Content
        {
            Id = long.Parse(copyToPageId),
            Type = ContentTypes.Page,
            Title = copyToTitle, // Keep original title
            Body = new Body
            {
                Storage = new BodyContent
                {
                    Value = copyFromContent,
                    Representation = "storage"
                }
            },
            Version = new Dapplo.Confluence.Entities.Version
            {
                Number = currentVersion + 1
            }
        };

        // Update copy to page with copy from content
        var result = await confluenceClient.Content.UpdateAsync(updatedContent);

        if (result == null) throw new Exception("Failed to update copy to page.");

        lblStatus.Text = "Opening version history...";

        // Open version history in browser
        var historyUrl = $"{baseUrl}/pages/viewpage.action?pageId={copyToPageId}&showVersions=true";
        Process.Start(new ProcessStartInfo(historyUrl) { UseShellExecute = true });

        lblStatus.Text =
            $"Successfully copied content from '{copyFromTitle}' to '{copyToTitle}'. Version history opened in browser.";
        lblStatus.ForeColor = Color.Green;

        MessageBox.Show(
            $"Content successfully copied from '{copyFromTitle}' to '{copyToTitle}'!\n\nVersion history has been opened in your default browser.",
            "Copy Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            debounceTimer?.Stop();
            debounceTimer?.Dispose();
            // IConfluenceClient doesn't implement IDisposable in this version
            confluenceClient = null;
        }

        base.Dispose(disposing);
    }
}