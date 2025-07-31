using System;
using System.IO;
using System.Text.Json;
using System.Reflection;
using NUnit.Framework;
using ConfluenceCopier;

namespace ConfluenceCopierTests
{
    /// <summary>
    /// Comprehensive tests for AppSettings Load and Save functionality
    /// Uses a different approach that works around the readonly SettingsPath limitation
    /// </summary>
    [TestFixture]
    public class SettingsLoadSaveTests
    {
        private string _realSettingsPath = null!;
        private string _backupPath = null!;
        private bool _settingsFileExisted = false;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Get the actual settings path using reflection
            var settingsPathField = typeof(AppSettings).GetField("SettingsPath", 
                BindingFlags.NonPublic | BindingFlags.Static);
            _realSettingsPath = (string)settingsPathField!.GetValue(null)!;
            _backupPath = _realSettingsPath + ".test-backup";
        }

        [SetUp]
        public void SetUp()
        {
            // Backup existing settings file if it exists
            _settingsFileExisted = File.Exists(_realSettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(_realSettingsPath, _backupPath, true);
                File.Delete(_realSettingsPath);
            }

            // Ensure the directory doesn't exist for clean testing
            var directory = Path.GetDirectoryName(_realSettingsPath);
            if (Directory.Exists(directory))
            {
                // Only delete if it's empty or only contains our test file
                try
                {
                    var files = Directory.GetFiles(directory!);
                    if (files.Length == 0)
                    {
                        Directory.Delete(directory, false);
                    }
                }
                catch
                {
                    // Directory not empty or in use, that's fine
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test files
            if (File.Exists(_realSettingsPath))
            {
                File.Delete(_realSettingsPath);
            }

            // Restore backup if it existed
            if (_settingsFileExisted && File.Exists(_backupPath))
            {
                File.Copy(_backupPath, _realSettingsPath, true);
                File.Delete(_backupPath);
            }
        }

        [Test]
        public void Load_NoSettingsFile_ReturnsDefaultSettings()
        {
            // Arrange - ensure no settings file exists
            Assert.That(File.Exists(_realSettingsPath), Is.False);

            // Act
            var settings = AppSettings.Load();

            // Assert
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ConfluenceUrl, Is.EqualTo(""));
            Assert.That(settings.Username, Is.EqualTo(""));
            Assert.That(settings.Password, Is.EqualTo(""));
            Assert.That(settings.AuthType, Is.EqualTo(AuthenticationType.Password));
            Assert.That(settings.ApiKey, Is.EqualTo(""));
        }

        [Test]
        public void Save_NewSettings_CreatesDirectoryAndFile()
        {
            // Arrange
            var directory = Path.GetDirectoryName(_realSettingsPath);
            var directoryExisted = Directory.Exists(directory);
            
            var settings = new AppSettings
            {
                ConfluenceUrl = "https://save.test.net",
                Username = "save@test.com",
                Password = "savepassword",
                AuthType = AuthenticationType.Password,
                ApiKey = ""
            };

            // Act
            settings.Save();

            // Assert
            Assert.That(Directory.Exists(directory), Is.True);
            Assert.That(File.Exists(_realSettingsPath), Is.True);
            
            // Verify content by loading it back
            var loadedSettings = AppSettings.Load();
            Assert.That(loadedSettings.ConfluenceUrl, Is.EqualTo(settings.ConfluenceUrl));
            Assert.That(loadedSettings.Username, Is.EqualTo(settings.Username));
            Assert.That(loadedSettings.Password, Is.EqualTo(settings.Password));
            Assert.That(loadedSettings.AuthType, Is.EqualTo(settings.AuthType));
            Assert.That(loadedSettings.ApiKey, Is.EqualTo(settings.ApiKey));
        }

        [Test]
        public void SaveAndLoad_ValidSettings_PreservesAllData()
        {
            // Arrange
            var originalSettings = new AppSettings
            {
                ConfluenceUrl = "https://fullcycle.test.net/wiki",
                Username = "fullcycle@example.com",
                Password = "complex!Password123",
                AuthType = AuthenticationType.ApiKey,
                ApiKey = "ATATT3xFfGF0123456789abcdef"
            };

            // Act - Save and Load
            originalSettings.Save();
            var loadedSettings = AppSettings.Load();

            // Assert
            Assert.That(loadedSettings, Is.Not.Null);
            Assert.That(loadedSettings.ConfluenceUrl, Is.EqualTo(originalSettings.ConfluenceUrl));
            Assert.That(loadedSettings.Username, Is.EqualTo(originalSettings.Username));
            Assert.That(loadedSettings.Password, Is.EqualTo(originalSettings.Password));
            Assert.That(loadedSettings.AuthType, Is.EqualTo(originalSettings.AuthType));
            Assert.That(loadedSettings.ApiKey, Is.EqualTo(originalSettings.ApiKey));
        }

        [Test]
        public void SaveAndLoad_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange - settings with special characters
            var settings = new AppSettings
            {
                ConfluenceUrl = "https://special-chars.test.net/wiki/",
                Username = "user+test@domain.co.uk",
                Password = "password!@#$%^&*(){}[]|\\:;\"'<>,.?/~`",
                AuthType = AuthenticationType.Password,
                ApiKey = ""
            };

            // Act
            settings.Save();
            var loadedSettings = AppSettings.Load();

            // Assert
            Assert.That(loadedSettings.ConfluenceUrl, Is.EqualTo(settings.ConfluenceUrl));
            Assert.That(loadedSettings.Username, Is.EqualTo(settings.Username));
            Assert.That(loadedSettings.Password, Is.EqualTo(settings.Password));
        }

        [Test]
        public void SaveAndLoad_WithUnicodeCharacters_HandlesCorrectly()
        {
            // Arrange - settings with Unicode characters
            var settings = new AppSettings
            {
                ConfluenceUrl = "https://unicode.test.net",
                Username = "用户@测试.com",
                Password = "密码123",
                AuthType = AuthenticationType.Password,
                ApiKey = ""
            };

            // Act
            settings.Save();
            var loadedSettings = AppSettings.Load();

            // Assert
            Assert.That(loadedSettings.Username, Is.EqualTo(settings.Username));
            Assert.That(loadedSettings.Password, Is.EqualTo(settings.Password));
        }

        [Test]
        public void Save_OverwriteExisting_UpdatesFile()
        {
            // Arrange - create initial settings
            var initialSettings = new AppSettings
            {
                ConfluenceUrl = "https://initial.test.net",
                Username = "initial@test.com"
            };
            initialSettings.Save();

            // Verify initial save
            Assert.That(File.Exists(_realSettingsPath), Is.True);
            var firstLoad = AppSettings.Load();
            Assert.That(firstLoad.ConfluenceUrl, Is.EqualTo("https://initial.test.net"));

            // Act - save updated settings
            var updatedSettings = new AppSettings
            {
                ConfluenceUrl = "https://updated.test.net",
                Username = "updated@test.com",
                Password = "newpassword",
                AuthType = AuthenticationType.ApiKey,
                ApiKey = "new-api-key"
            };
            updatedSettings.Save();

            // Assert
            var loadedSettings = AppSettings.Load();
            Assert.That(loadedSettings.ConfluenceUrl, Is.EqualTo("https://updated.test.net"));
            Assert.That(loadedSettings.Username, Is.EqualTo("updated@test.com"));
            Assert.That(loadedSettings.Password, Is.EqualTo("newpassword"));
            Assert.That(loadedSettings.AuthType, Is.EqualTo(AuthenticationType.ApiKey));
            Assert.That(loadedSettings.ApiKey, Is.EqualTo("new-api-key"));
        }

        [Test]
        public void Load_CorruptedJsonFile_ReturnsDefaultSettings()
        {
            // Arrange - create corrupted JSON file
            var directory = Path.GetDirectoryName(_realSettingsPath);
            Directory.CreateDirectory(directory!);
            File.WriteAllText(_realSettingsPath, "{ invalid json content }");

            // Act
            var settings = AppSettings.Load();

            // Assert - should return default settings, not throw exception
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ConfluenceUrl, Is.EqualTo(""));
            Assert.That(settings.Username, Is.EqualTo(""));
            Assert.That(settings.Password, Is.EqualTo(""));
            Assert.That(settings.AuthType, Is.EqualTo(AuthenticationType.Password));
            Assert.That(settings.ApiKey, Is.EqualTo(""));
        }

        [Test]
        public void Load_EmptyJsonFile_ReturnsDefaultSettings()
        {
            // Arrange - create empty file
            var directory = Path.GetDirectoryName(_realSettingsPath);
            Directory.CreateDirectory(directory!);
            File.WriteAllText(_realSettingsPath, "");

            // Act
            var settings = AppSettings.Load();

            // Assert - should return default settings, not throw exception
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ConfluenceUrl, Is.EqualTo(""));
        }

        [Test]
        public void Load_PartialJsonFile_ReturnsSettingsWithDefaults()
        {
            // Arrange - create JSON with only some properties
            var directory = Path.GetDirectoryName(_realSettingsPath);
            Directory.CreateDirectory(directory!);
            var partialJson = @"{
                ""ConfluenceUrl"": ""https://partial.test.net"",
                ""Username"": ""partial@test.com""
            }";
            File.WriteAllText(_realSettingsPath, partialJson);

            // Act
            var settings = AppSettings.Load();

            // Assert - should load partial data and use defaults for missing properties
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ConfluenceUrl, Is.EqualTo("https://partial.test.net"));
            Assert.That(settings.Username, Is.EqualTo("partial@test.com"));
            Assert.That(settings.Password, Is.EqualTo("")); // Default
            Assert.That(settings.AuthType, Is.EqualTo(AuthenticationType.Password)); // Default
            Assert.That(settings.ApiKey, Is.EqualTo("")); // Default
        }

        [Test]
        public void Load_JsonWithNullValues_HandlesGracefully()
        {
            // Arrange - create JSON with null values
            var directory = Path.GetDirectoryName(_realSettingsPath);
            Directory.CreateDirectory(directory!);
            var jsonWithNulls = @"{
                ""ConfluenceUrl"": null,
                ""Username"": ""test@example.com"",
                ""Password"": null,
                ""AuthType"": 1,
                ""ApiKey"": null
            }";
            File.WriteAllText(_realSettingsPath, jsonWithNulls);

            // Act
            var settings = AppSettings.Load();

            // Assert - should handle nulls gracefully and load successfully
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.Username, Is.EqualTo("test@example.com"));
            Assert.That(settings.AuthType, Is.EqualTo(AuthenticationType.ApiKey));
            
            // Note: When JSON explicitly sets a property to null, 
            // the deserializer will set it to null, regardless of default values
            // The application should handle null values appropriately
        }

        [Test]
        public void Save_ToNonExistentDirectory_CreatesDirectoryAndSaves()
        {
            // Arrange - ensure directory doesn't exist
            var directory = Path.GetDirectoryName(_realSettingsPath);
            if (Directory.Exists(directory))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch
                {
                    // If we can't delete it, skip this test
                    Assert.Inconclusive("Cannot delete directory for test");
                }
            }

            var settings = new AppSettings
            {
                ConfluenceUrl = "https://newdir.test.net",
                Username = "newdir@test.com"
            };

            // Act
            settings.Save();

            // Assert
            Assert.That(Directory.Exists(directory), Is.True);
            Assert.That(File.Exists(_realSettingsPath), Is.True);

            var loadedSettings = AppSettings.Load();
            Assert.That(loadedSettings.ConfluenceUrl, Is.EqualTo("https://newdir.test.net"));
        }

        [Test]
        public void AuthenticationType_AllValues_SerializeAndDeserializeCorrectly()
        {
            // Test Password authentication type
            var passwordSettings = new AppSettings
            {
                ConfluenceUrl = "https://password.test.net",
                AuthType = AuthenticationType.Password
            };
            passwordSettings.Save();
            var loadedPasswordSettings = AppSettings.Load();
            Assert.That(loadedPasswordSettings.AuthType, Is.EqualTo(AuthenticationType.Password));

            // Test ApiKey authentication type
            var apiKeySettings = new AppSettings
            {
                ConfluenceUrl = "https://apikey.test.net",
                AuthType = AuthenticationType.ApiKey
            };
            apiKeySettings.Save();
            var loadedApiKeySettings = AppSettings.Load();
            Assert.That(loadedApiKeySettings.AuthType, Is.EqualTo(AuthenticationType.ApiKey));
        }
    }
}