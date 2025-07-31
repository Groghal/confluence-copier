using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Dapplo.Confluence;

namespace ConfluenceCopier;

public partial class SettingsWindow : Window
{


    private AppSettings settings;
    private bool isLoadingSettings = false;

    public SettingsWindow(AppSettings currentSettings)
    {
        InitializeComponent();
        settings = currentSettings;
        InitializeControls();
        LoadSettings();
    }

    private void InitializeControls()
    {
        // Avalonia automatically generates fields for named controls
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
            rbApiKey.IsChecked = true;
        else
            rbPassword.IsChecked = true;

        UpdateAuthFieldsVisibility();

        isLoadingSettings = false;
    }

    private void SaveSettings()
    {
        if (isLoadingSettings) return;

        settings.ConfluenceUrl = txtConfluenceUrl.Text ?? "";
        settings.Username = txtUsername.Text ?? "";
        settings.Password = txtPassword.Text ?? "";
        settings.ApiKey = txtApiKey.Text ?? "";
        settings.AuthType = rbApiKey.IsChecked == true ? AuthenticationType.ApiKey : AuthenticationType.Password;
        settings.Save();
    }

    private void BtnShowHidePassword_Click(object? sender, RoutedEventArgs e)
    {
        if (txtPassword.PasswordChar == '\0')
        {
            txtPassword.PasswordChar = '*';
            btnShowHidePassword.Content = "👁";
        }
        else
        {
            txtPassword.PasswordChar = '\0';
            btnShowHidePassword.Content = "🙈";
        }
    }

    private void BtnShowHideApiKey_Click(object? sender, RoutedEventArgs e)
    {
        if (txtApiKey.PasswordChar == '\0')
        {
            txtApiKey.PasswordChar = '*';
            btnShowHideApiKey.Content = "👁";
        }
        else
        {
            txtApiKey.PasswordChar = '\0';
            btnShowHideApiKey.Content = "🙈";
        }
    }

    private void RbAuthType_CheckedChanged(object? sender, RoutedEventArgs e)
    {
        UpdateAuthFieldsVisibility();
    }

    private void UpdateAuthFieldsVisibility()
    {
        var isPassword = rbPassword.IsChecked == true;
        var isApiKey = rbApiKey.IsChecked == true;

        if (isPassword)
        {
            // Show Username + Password fields
            lblUsername.IsVisible = true;
            txtUsername.IsVisible = true;
            lblPassword.IsVisible = true;
            txtPassword.IsVisible = true;
            btnShowHidePassword.IsVisible = true;

            // Hide API Key fields
            lblApiKey.IsVisible = false;
            txtApiKey.IsVisible = false;
            btnShowHideApiKey.IsVisible = false;
        }
        else if (isApiKey)
        {
            // Hide Username + Password fields
            lblUsername.IsVisible = false;
            txtUsername.IsVisible = false;
            lblPassword.IsVisible = false;
            txtPassword.IsVisible = false;
            btnShowHidePassword.IsVisible = false;

            // Show API Key fields
            lblApiKey.IsVisible = true;
            txtApiKey.IsVisible = true;
            btnShowHideApiKey.IsVisible = true;
        }
    }

    private async void BtnTestConnection_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtConfluenceUrl.Text))
        {
            await ShowMessageBox("Please enter the Confluence URL before testing the connection.",
                "Missing Information");
            return;
        }

        if (rbPassword.IsChecked == true)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                await ShowMessageBox("Please fill in both username and password before testing the connection.",
                    "Missing Information");
                return;
            }
        }
        else if (rbApiKey.IsChecked == true)
        {
            if (string.IsNullOrWhiteSpace(txtApiKey.Text))
            {
                await ShowMessageBox("Please enter the API key before testing the connection.",
                    "Missing Information");
                return;
            }
        }

        // Disable button and show testing state
        btnTestConnection.IsEnabled = false;
        var originalText = btnTestConnection.Content?.ToString() ?? "Test Connection";
        btnTestConnection.Content = "Testing...";

        try
        {
            await TestConfluenceConnection();

            // Success
            btnTestConnection.Content = "✓ Success";
            btnTestConnection.Background = Brushes.LightGreen;

            await ShowMessageBox("Connection successful! Your Confluence settings are valid.",
                "Test Successful");
        }
        catch (Exception ex)
        {
            // Error
            btnTestConnection.Content = "✗ Failed";
            btnTestConnection.Background = Brushes.LightCoral;

            await ShowMessageBox($"Connection failed: {ex.Message}\n\nPlease check your settings and try again.",
                "Test Failed");
        }
        finally
        {
            // Reset button after 2 seconds
            var resetTimer = new DispatcherTimer();
            resetTimer.Interval = TimeSpan.FromSeconds(2);
            resetTimer.Tick += (s, args) =>
            {
                btnTestConnection.Content = originalText;
                btnTestConnection.Background = Brushes.Transparent;
                btnTestConnection.IsEnabled = true;
                resetTimer.Stop();
            };
            resetTimer.Start();
        }
    }

    private async Task TestConfluenceConnection()
    {
        var baseUrl = txtConfluenceUrl.Text?.TrimEnd('/') ?? "";

        // Validate URL format
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var confluenceUri))
            throw new ArgumentException(
                "Invalid Confluence URL format. Please ensure it starts with https:// or http://");

        // Create Confluence client
        var confluenceClient = ConfluenceClient.Create(confluenceUri);

        // Set authentication based on selected type
        if (rbPassword.IsChecked == true)
        {
            var username = txtUsername.Text ?? "";
            var password = txtPassword.Text ?? "";
            confluenceClient.SetBasicAuthentication(username, password);
        }
        else if (rbApiKey.IsChecked == true)
        {
            var apiKey = txtApiKey.Text ?? "";
            confluenceClient.SetBearerAuthentication(apiKey);
        }

        // Test connection by getting user info
        try
        {
            var currentUser = await confluenceClient.User.GetCurrentUserAsync();

            if (currentUser == null)
            {
                var authType = rbApiKey.IsChecked == true ? "API key" : "username and password";
                throw new Exception($"Authentication failed. Please check your {authType}.");
            }

            // Optionally test a simple API call to ensure we have basic permissions
            await confluenceClient.Space.GetAllAsync();
        }
        catch (HttpRequestException httpEx)
        {
            var authType = rbApiKey.IsChecked == true ? "API key" : "username and password";

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

    private async void BtnSave_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtConfluenceUrl.Text))
        {
            await ShowMessageBox("Please enter the Confluence URL.", "Missing Information");
            return;
        }

        if (rbPassword.IsChecked == true)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                await ShowMessageBox("Please fill in both username and password.", "Missing Information");
                return;
            }
        }
        else if (rbApiKey.IsChecked == true)
        {
            if (string.IsNullOrWhiteSpace(txtApiKey.Text))
            {
                await ShowMessageBox("Please enter the API key.", "Missing Information");
                return;
            }
        }

        SaveSettings();
        Close(true);
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
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

    public AppSettings GetSettings()
    {
        return settings;
    }
}