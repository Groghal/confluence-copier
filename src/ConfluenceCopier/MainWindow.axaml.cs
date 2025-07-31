using System.Diagnostics;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Dapplo.Confluence;
using Dapplo.Confluence.Entities;
using Dapplo.Confluence.Query;

namespace ConfluenceCopier;

public partial class MainWindow : Window
{


    private AppSettings settings;
    private IConfluenceClient? confluenceClient;
    private DispatcherTimer? debounceTimer;
    private DateTime lastApiCall = DateTime.MinValue;
    private readonly TimeSpan apiRateLimit = TimeSpan.FromSeconds(1);

    public MainWindow()
    {
        InitializeComponent();
        settings = AppSettings.Load();
        InitializeControls();
    }

    private void InitializeControls()
    {
        // Initialize debounce timer
        debounceTimer = new DispatcherTimer();
        debounceTimer.Interval = TimeSpan.FromSeconds(1);
        debounceTimer.Tick += DebounceTimer_Tick;
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void TitleBar_DoubleTapped(object? sender, TappedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void TxtCopyFromPage_TextChanged(object? sender, TextChangedEventArgs e)
    {
        RestartDebounceTimer();
    }

    private void TxtCopyToPage_TextChanged(object? sender, TextChangedEventArgs e)
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
        // Check if we have valid settings based on auth type
        if (string.IsNullOrWhiteSpace(settings.ConfluenceUrl)) return; // Don't fetch if URL isn't configured

        var hasValidAuth = false;
        if (settings.AuthType == AuthenticationType.ApiKey)
            hasValidAuth = !string.IsNullOrWhiteSpace(settings.ApiKey);
        else
            hasValidAuth = !string.IsNullOrWhiteSpace(settings.Username) &&
                           !string.IsNullOrWhiteSpace(settings.Password);

        if (!hasValidAuth) return; // Don't fetch if authentication isn't configured

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

                // Set authentication based on type
                if (settings.AuthType == AuthenticationType.ApiKey)
                    confluenceClient.SetBearerAuthentication(settings.ApiKey);
                else
                    confluenceClient.SetBasicAuthentication(settings.Username, settings.Password);
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
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            lblCopyFromPath.Text = copyFromHierarchy;
                            lblCopyFromPath.Foreground = (IBrush?)Application.Current?.FindResource("StatusSuccessBrush") ?? Brushes.DarkBlue;
                        });
                    }
                }
                catch
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        lblCopyFromPath.Text = "Invalid page or page not found";
                        lblCopyFromPath.Foreground = (IBrush?)Application.Current?.FindResource("StatusErrorBrush") ?? Brushes.Red;
                    });
                }
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    lblCopyFromPath.Text = "Select a page to see its path";
                    lblCopyFromPath.Foreground = (IBrush?)Application.Current?.FindResource("SubtitleTextBrush") ?? Brushes.Gray;
                });
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
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            lblCopyToPath.Text = copyToHierarchy;
                            lblCopyToPath.Foreground = (IBrush?)Application.Current?.FindResource("StatusSuccessBrush") ?? Brushes.DarkBlue;
                        });
                    }
                }
                catch
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        lblCopyToPath.Text = "Invalid page or page not found";
                        lblCopyToPath.Foreground = (IBrush?)Application.Current?.FindResource("StatusErrorBrush") ?? Brushes.Red;
                    });
                }
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    lblCopyToPath.Text = "Select a page to see its path";
                    lblCopyToPath.Foreground = (IBrush?)Application.Current?.FindResource("SubtitleTextBrush") ?? Brushes.Gray;
                });
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                lblStatus.Text = "Page information updated";
                lblStatus.Foreground = (IBrush?)Application.Current?.FindResource("StatusSuccessBrush") ?? Brushes.Green;
            });
        }
        catch (Exception ex)
        {
            // Don't crash the app, just show a brief error
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                lblStatus.Text = $"Error fetching page info: {ex.Message}";
                lblStatus.Foreground = (IBrush?)Application.Current?.FindResource("StatusWarningBrush") ?? Brushes.Orange;

                // Reset path labels on error
                if (string.IsNullOrWhiteSpace(txtCopyFromPage.Text))
                {
                    lblCopyFromPath.Text = "Select a page to see its path";
                    lblCopyFromPath.Foreground = (IBrush?)Application.Current?.FindResource("SubtitleTextBrush") ?? Brushes.Gray;
                }

                if (string.IsNullOrWhiteSpace(txtCopyToPage.Text))
                {
                    lblCopyToPath.Text = "Select a page to see its path";
                    lblCopyToPath.Foreground = (IBrush?)Application.Current?.FindResource("SubtitleTextBrush") ?? Brushes.Gray;
                }
            });

            // Clear error after 3 seconds
            var errorTimer = new DispatcherTimer();
            errorTimer.Interval = TimeSpan.FromSeconds(3);
            errorTimer.Tick += (s, args) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    lblStatus.Text = "Ready";
                    lblStatus.Foreground = (IBrush?)Application.Current?.FindResource("StatusReadyBrush") ?? Brushes.Blue;
                });
                errorTimer.Stop();
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

    private async void BtnSettings_Click(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(settings);
        var result = await settingsWindow.ShowDialog<bool?>(this);
        
        if (result == true)
        {
            settings = settingsWindow.GetSettings();
            lblStatus.Text = "Settings saved successfully.";
            lblStatus.Foreground = (IBrush?)Application.Current?.FindResource("StatusSuccessBrush") ?? Brushes.Green;
        }
    }

    public string ExtractPageId(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException($"Could not extract page ID from: {input}. Please provide a valid page ID or URL.");

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

    private async void BtnCopy_Click(object? sender, RoutedEventArgs e)
    {
        // Check if we have valid settings based on auth type
        if (string.IsNullOrWhiteSpace(settings.ConfluenceUrl))
        {
            var dialog = new Window()
            {
                Title = "Settings Required",
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            await ShowMessageBox("Please configure your Confluence settings first using the Settings button.",
                "Settings Required");
            return;
        }

        if (settings.AuthType == AuthenticationType.ApiKey)
        {
            if (string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                await ShowMessageBox("Please configure your API key in the Settings.",
                    "Settings Required");
                return;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.Password))
            {
                await ShowMessageBox("Please configure your username and password in the Settings.",
                    "Settings Required");
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(txtCopyFromPage.Text) ||
            string.IsNullOrWhiteSpace(txtCopyToPage.Text))
        {
            await ShowMessageBox("Please enter both copy from and copy to page information.",
                "Missing Page Information");
            return;
        }

        btnCopy.IsEnabled = false;
        lblStatus.Text = "Copying content...";
        lblStatus.Foreground = (IBrush?)Application.Current?.FindResource("StatusReadyBrush") ?? Brushes.Blue;

        try
        {
            await CopyPageContent();
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Error: {ex.Message}";
            lblStatus.Foreground = (IBrush?)Application.Current?.FindResource("StatusErrorBrush") ?? Brushes.Red;
            await ShowMessageBox($"An error occurred: {ex.Message}", "Error");
        }
        finally
        {
            btnCopy.IsEnabled = true;
        }
    }

    private async Task ShowMessageBox(string message, string title)
    {
        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        var messageWindow = new Window()
        {
            Title = title,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    okButton
                }
            }
        };

        okButton.Click += (s, e) => messageWindow.Close();
        
        await messageWindow.ShowDialog(this);
    }

    private async Task CopyPageContent()
    {
        var baseUrl = settings.ConfluenceUrl.TrimEnd('/');

        // Extract page IDs from input (supports both IDs and URLs)
        var copyFromPageId = ExtractPageId(txtCopyFromPage.Text ?? "");
        var copyToPageId = ExtractPageId(txtCopyToPage.Text ?? "");

        // Initialize Confluence client
        var confluenceUri = new Uri(baseUrl);
        confluenceClient = ConfluenceClient.Create(confluenceUri);

        // Set authentication based on type
        if (settings.AuthType == AuthenticationType.ApiKey)
            confluenceClient.SetBearerAuthentication(settings.ApiKey);
        else
            confluenceClient.SetBasicAuthentication(settings.Username, settings.Password);

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
        lblStatus.Foreground = (IBrush?)Application.Current?.FindResource("StatusSuccessBrush") ?? Brushes.Green;

        await ShowMessageBox(
            $"Content successfully copied from '{copyFromTitle}' to '{copyToTitle}'!\n\nVersion history has been opened in your default browser.",
            "Copy Successful");
    }
}