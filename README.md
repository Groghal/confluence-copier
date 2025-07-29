# Confluence Page Copier

A simple WinForms application for copying content between Confluence pages with automatic version history tracking and
real-time page information display.

## 🚀 Features

### **Core Functionality**

- **Easy Content Copying**: Copy content from one page to another with one click
- **Flexible Input**: Supports both page IDs and full Confluence URLs
- **Real-Time Page Info**: Shows full page hierarchy as you type (Space > Parent > Child)
- **Version History**: Automatically opens version history in browser after copying

### **User Interface**

- **Clean Design**: Separate settings dialog for one-time configuration
- **Path Display**: See complete page hierarchy before copying
- **Visual Feedback**: Color-coded status messages and path validation
- **Intuitive Labels**: "Copy From" and "Copy To" for clear direction

### **Technical Features**

- **Secure Authentication**: Uses Confluence API tokens with show/hide functionality
- **Settings Persistence**: Automatically saves and restores your Confluence settings
- **Rate Limiting**: Respects Confluence API limits (max 1 call per second)
- **Error Handling**: Robust error handling that won't crash the application
- **Self-Contained**: Can be built as a standalone executable (no .NET runtime required)

## 📋 Requirements

### To Run the Application:

- Windows 7/8/10/11
- .NET 8.0 Runtime (or use self-contained build)

### To Build from Source:

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code (optional)

## 🔧 Building

### Option 1: Build Self-Contained Executable (Recommended)

```batch
# Run the provided batch script
build.bat
```

This creates a fully portable executable in the `publish` folder that doesn't require .NET runtime.

### Option 2: Standard Build

```bash
# Build for development/testing
dotnet build

# Run directly
dotnet run
```

## 📖 Usage

### 1. Initial Setup

1. Click **"⚙ Settings"** button
2. **Confluence URL**: Enter your Confluence instance URL (e.g., `https://yourcompany.atlassian.net`)
3. **Username**: Your Confluence email address
4. **API Token**: Generate from Atlassian Account Settings → Security → API tokens
5. Click **"Save"** to store your settings

### 2. Page Input Formats

The application supports multiple input formats for pages:

**Page IDs:**

```
123456
```

**Full URLs:**

```
https://company.atlassian.net/wiki/spaces/DEV/pages/123456/My-Page-Title
https://company.atlassian.net/pages/viewpage.action?pageId=123456
https://company.atlassian.net/display/SPACE/Title?pageId=123456
```

### 3. Real-Time Page Information

As you type page IDs or URLs, the application will automatically:

- Display the full page hierarchy (e.g., "Development Space > API Docs > Authentication")
- Validate the page exists and is accessible
- Show error messages for invalid pages

### 4. Copying Content

1. Enter **Copy From** page (source of the content)
2. Enter **Copy To** page (destination for the content)
3. Verify the page paths shown above each text box
4. Click **"Copy From → Copy To"**
5. Version history will automatically open in your browser

## ⚙️ Configuration

### Settings Storage

Settings are automatically saved to:

```
%APPDATA%\ConfluenceCopier\settings.json
```

### Settings Dialog

- Access via **"⚙ Settings"** button on main window
- One-time configuration keeps main interface clean
- Settings persist between application restarts

### API Token Security

- API tokens are stored locally in the settings file
- Use the eye button (👁/🙈) to show/hide the token while typing
- The token field is masked by default for security
- Validation ensures all fields are filled before saving

## 🔒 Security Notes

- **API tokens are stored in plain text** in the local settings file
- Only store tokens on trusted computers
- Consider using dedicated API tokens with limited permissions
- Regularly rotate your API tokens for security

## 🎯 Use Cases

- **Content Synchronization**: Keep development and production pages in sync
- **Template Deployment**: Copy approved templates to new pages
- **Backup/Recovery**: Restore page content from backups
- **Content Migration**: Move content between spaces or instances
- **Version Control**: Create content snapshots before major changes

## 🐛 Troubleshooting

### Common Issues

**"Failed to fetch page content"**

- Verify the page ID/URL is correct
- Check API token permissions
- Ensure the page exists and is accessible

**"Authentication failed"**

- Verify Confluence URL is correct (include https://)
- Check username (should be email address)
- Regenerate API token if needed

**"Could not extract page ID from URL"**

- Ensure the URL contains a valid pageId parameter
- Try using the numeric page ID directly
- Check URL format matches supported patterns

### Getting Page IDs

If you need to find a page ID:

1. Go to the page in Confluence
2. Click **"⋯" → "Page Information"**
3. The URL will show: `...pageId=123456`

## 📁 Project Structure

```
confluence-copier/
├── ConfluenceCopier.csproj    # Project file (.NET 8)
├── Program.cs                 # Application entry point
├── MainForm.cs               # Main UI and logic
├── SettingsForm.cs           # Settings dialog
├── Settings.cs               # Settings persistence
├── build.bat                 # Self-contained build script
├── LICENSE                   # MIT License
└── README.md                 # This file
```

## 🔗 Dependencies

- **[Dapplo.Confluence](https://github.com/dapplo/Dapplo.Confluence)** - Confluence API client library
- **.NET 8.0** - Application framework
- **WinForms** - User interface framework

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### MIT License Summary

- ✅ Commercial use
- ✅ Modification
- ✅ Distribution
- ✅ Private use
- ❌ Liability
- ❌ Warranty

Please ensure compliance with your organization's security policies when using API tokens.

## 🤝 Contributing

Feel free to submit issues, feature requests, or pull requests to improve the application.

## 🎯 Screenshots

### Main Interface

```
┌─────────────────────────────────────────────────┐
│ Confluence Page Copier             [⚙ Settings] │
│                                                 │
│ Copy From:    Development > API > Authentication │
│ [enter page ID or URL here____________________ ] │
│                                                 │
│ Copy To:      Production > Docs > User Guide    │
│ [enter page ID or URL here____________________ ] │
│                                                 │
│              [Copy From → Copy To]              │
│                                                 │
│ Status: Page information updated                │
└─────────────────────────────────────────────────┘
```

### Settings Dialog

```
┌──────────────────────────────────────┐
│ Confluence Settings                  │
│                                      │
│ Confluence URL: [__________________ ] │
│ Username/Email: [__________________ ] │
│ API Token:      [______________] [👁] │
│                                      │
│                    [Save] [Cancel]   │
└──────────────────────────────────────┘
```

## ⚠️ Disclaimer

- Always test on non-critical pages first
- This tool modifies Confluence content - use with caution
- Ensure you have proper permissions before copying content
- The authors are not responsible for any data loss or unauthorized content changes 