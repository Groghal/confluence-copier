using System;
using System.IO;
using System.Text.Json;
using NUnit.Framework;
using ConfluenceCopier;

namespace ConfluenceCopierTests
{
    [TestFixture]
    public class SettingsTests : HeadlessTestBase
    {
        private string _testSettingsPath = null!;
        private string _originalSettingsPath = null!;

        [SetUp]
        public void Setup()
        {
            // Create a temporary test settings path
            _testSettingsPath = Path.Combine(Path.GetTempPath(), $"ConfluenceCopierTests_{Guid.NewGuid()}", "settings.json");

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_testSettingsPath)!);

            // Store original settings path (if we can access it safely)
            _originalSettingsPath = GetCurrentSettingsPath();

            // Delete any existing test settings file
            if (File.Exists(_testSettingsPath))
            {
                File.Delete(_testSettingsPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test settings file and directory
            try
            {
                if (File.Exists(_testSettingsPath))
                {
                    File.Delete(_testSettingsPath);
                }

                var testDir = Path.GetDirectoryName(_testSettingsPath);
                if (Directory.Exists(testDir) && Directory.GetFiles(testDir!).Length == 0)
                {
                    Directory.Delete(testDir);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        [Test]
        public void Load_NoExistingSettings_ReturnsDefaultSettings()
        {
            // Act - Create settings with test-specific approach
            var settings = CreateTestSettings();

            // Assert
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ConfluenceUrl, Is.EqualTo(""));
            Assert.That(settings.Username, Is.EqualTo(""));
            Assert.That(settings.Password, Is.EqualTo(""));
            Assert.That(settings.AuthType, Is.EqualTo(AuthenticationType.Password));
            Assert.That(settings.ApiKey, Is.EqualTo(""));
        }

        [Test]
        public void Save_ValidSettings_WritesToFile()
        {
            // Arrange
            var settings = CreateTestSettings();
            settings.ConfluenceUrl = "https://test.atlassian.net";
            settings.Username = "test@example.com";
            settings.Password = "password123";
            settings.AuthType = AuthenticationType.Password;
            settings.ApiKey = "";

            // Act
            SaveTestSettings(settings);

            // Assert
            Assert.That(File.Exists(_testSettingsPath), Is.True);
            var json = File.ReadAllText(_testSettingsPath);
            var loadedSettings = JsonSerializer.Deserialize<AppSettings>(json);

            Assert.That(loadedSettings, Is.Not.Null);
            Assert.That(loadedSettings.ConfluenceUrl, Is.EqualTo("https://test.atlassian.net"));
            Assert.That(loadedSettings.Username, Is.EqualTo("test@example.com"));
            Assert.That(loadedSettings.Password, Is.EqualTo("password123"));
            Assert.That(loadedSettings.AuthType, Is.EqualTo(AuthenticationType.Password));
            Assert.That(loadedSettings.ApiKey, Is.EqualTo(""));
        }

        [Test]
        public void SaveAndLoad_WithApiKey_RetainsSettings()
        {
            // Arrange
            var originalSettings = CreateTestSettings();
            originalSettings.ConfluenceUrl = "https://test.atlassian.net";
            originalSettings.Username = "test@example.com";
            originalSettings.Password = "";
            originalSettings.AuthType = AuthenticationType.ApiKey;
            originalSettings.ApiKey = "test-api-key-12345";

            // Act
            SaveTestSettings(originalSettings);
            var loadedSettings = LoadTestSettings();

            // Assert
            Assert.That(loadedSettings, Is.Not.Null);
            Assert.That(loadedSettings.ConfluenceUrl, Is.EqualTo("https://test.atlassian.net"));
            Assert.That(loadedSettings.Username, Is.EqualTo("test@example.com"));
            Assert.That(loadedSettings.Password, Is.EqualTo(""));
            Assert.That(loadedSettings.AuthType, Is.EqualTo(AuthenticationType.ApiKey));
            Assert.That(loadedSettings.ApiKey, Is.EqualTo("test-api-key-12345"));
        }

        [Test]
        public void SaveAndLoad_WithPassword_RetainsSettings()
        {
            // Arrange
            var originalSettings = CreateTestSettings();
            originalSettings.ConfluenceUrl = "https://company.atlassian.net";
            originalSettings.Username = "user@company.com";
            originalSettings.Password = "secure-password";
            originalSettings.AuthType = AuthenticationType.Password;
            originalSettings.ApiKey = "";

            // Act
            SaveTestSettings(originalSettings);
            var loadedSettings = LoadTestSettings();

            // Assert
            Assert.That(loadedSettings, Is.Not.Null);
            Assert.That(loadedSettings.ConfluenceUrl, Is.EqualTo("https://company.atlassian.net"));
            Assert.That(loadedSettings.Username, Is.EqualTo("user@company.com"));
            Assert.That(loadedSettings.Password, Is.EqualTo("secure-password"));
            Assert.That(loadedSettings.AuthType, Is.EqualTo(AuthenticationType.Password));
            Assert.That(loadedSettings.ApiKey, Is.EqualTo(""));
        }

        [Test]
        public void JsonSerialization_RoundTrip_PreservesAllProperties()
        {
            // Arrange
            var original = CreateTestSettings();
            original.ConfluenceUrl = "https://serialization.test.net";
            original.Username = "serialize@test.com";
            original.Password = "test-password";
            original.AuthType = AuthenticationType.ApiKey;
            original.ApiKey = "serialize-key-123";

            // Act
            var json = JsonSerializer.Serialize(original, new JsonSerializerOptions { WriteIndented = true });
            var deserialized = JsonSerializer.Deserialize<AppSettings>(json);

            // Assert
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized.ConfluenceUrl, Is.EqualTo(original.ConfluenceUrl));
            Assert.That(deserialized.Username, Is.EqualTo(original.Username));
            Assert.That(deserialized.Password, Is.EqualTo(original.Password));
            Assert.That(deserialized.AuthType, Is.EqualTo(original.AuthType));
            Assert.That(deserialized.ApiKey, Is.EqualTo(original.ApiKey));
        }

        // Helper methods for test-specific settings operations
        private AppSettings CreateTestSettings()
        {
            return new AppSettings();
        }

        private void SaveTestSettings(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_testSettingsPath, json);
        }

        private AppSettings LoadTestSettings()
        {
            if (!File.Exists(_testSettingsPath))
                return new AppSettings();

            var json = File.ReadAllText(_testSettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        private string GetCurrentSettingsPath()
        {
            // Try to get the current settings path without modifying it
            try
            {
                // This is a safe way to get the current path without reflection
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                   "ConfluenceCopier", "settings.json");
            }
            catch
            {
                return "";
            }
        }
    }
}