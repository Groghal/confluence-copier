using System;
using System.IO;
using System.Text.Json;
using NUnit.Framework;
using ConfluenceCopier;

namespace ConfluenceCopierTests
{
    /// <summary>
    /// Tests for business logic that doesn't require UI components
    /// These tests run without Avalonia initialization
    /// </summary>
    [TestFixture]
    public class SimpleLogicTests
    {
        [Test]
        public void AppSettings_DefaultConstruction_HasExpectedDefaults()
        {
            // Act
            var settings = new AppSettings();

            // Assert
            Assert.That(settings.ConfluenceUrl, Is.EqualTo(""));
            Assert.That(settings.Username, Is.EqualTo(""));
            Assert.That(settings.Password, Is.EqualTo(""));
            Assert.That(settings.AuthType, Is.EqualTo(AuthenticationType.Password));
            Assert.That(settings.ApiKey, Is.EqualTo(""));
        }

        [Test]
        public void AppSettings_JsonSerialization_PreservesAllProperties()
        {
            // Arrange
            var original = new AppSettings
            {
                ConfluenceUrl = "https://test.atlassian.net",
                Username = "test@example.com",
                Password = "test-password",
                AuthType = AuthenticationType.ApiKey,
                ApiKey = "test-api-key-123"
            };

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

        [Test]
        public void AppSettings_FileOperations_WorkWithTempFiles()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var settings = new AppSettings
            {
                ConfluenceUrl = "https://filetest.atlassian.net",
                Username = "filetest@example.com",
                Password = "file-password",
                AuthType = AuthenticationType.Password,
                ApiKey = ""
            };

            try
            {
                // Act - Save
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(tempFile, json);

                // Act - Load
                var loadedJson = File.ReadAllText(tempFile);
                var loadedSettings = JsonSerializer.Deserialize<AppSettings>(loadedJson);

                // Assert
                Assert.That(loadedSettings, Is.Not.Null);
                Assert.That(loadedSettings.ConfluenceUrl, Is.EqualTo(settings.ConfluenceUrl));
                Assert.That(loadedSettings.Username, Is.EqualTo(settings.Username));
                Assert.That(loadedSettings.Password, Is.EqualTo(settings.Password));
                Assert.That(loadedSettings.AuthType, Is.EqualTo(settings.AuthType));
                Assert.That(loadedSettings.ApiKey, Is.EqualTo(settings.ApiKey));
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Test]
        public void AuthenticationType_EnumValues_AreCorrect()
        {
            // Test that enum values are as expected
            Assert.That((int)AuthenticationType.Password, Is.EqualTo(0));
            Assert.That((int)AuthenticationType.ApiKey, Is.EqualTo(1));
        }

        [Test]
        public void MockConfluenceClient_BasicOperations_Work()
        {
            // Arrange
            var mockClient = new MockConfluenceClient();
            var testContent = new Dapplo.Confluence.Entities.Content
            {
                Id = 12345,
                Title = "Test Page",
                Space = new Dapplo.Confluence.Entities.Space { Key = "TEST", Name = "Test Space" }
            };

            // Act
            mockClient.AddMockPage(testContent);
            var retrievedPage = mockClient.MockPages[12345];

            // Assert
            Assert.That(retrievedPage, Is.Not.Null);
            Assert.That(retrievedPage.Id, Is.EqualTo(12345));
            Assert.That(retrievedPage.Title, Is.EqualTo("Test Page"));
            Assert.That(retrievedPage.Space.Key, Is.EqualTo("TEST"));
        }

        [Test]
        public void MockConfluenceClient_ApiCallTracking_Works()
        {
            // Arrange
            var mockClient = new MockConfluenceClient();
            var initialCount = mockClient.ApiCallTimes.Count;

            // Act
            mockClient.ApiCallTimes.Add(DateTime.Now);
            mockClient.ApiCallTimes.Add(DateTime.Now.AddSeconds(1));

            // Assert
            Assert.That(mockClient.ApiCallTimes.Count, Is.EqualTo(initialCount + 2));
        }

        [Test]
        public void MockConfluenceClient_ErrorStates_ConfigureCorrectly()
        {
            // Arrange
            var mockClient = new MockConfluenceClient();

            // Act & Assert - Default state
            Assert.That(mockClient.AuthenticationFailed, Is.False);
            Assert.That(mockClient.NetworkError, Is.False);

            // Act & Assert - Error states
            mockClient.AuthenticationFailed = true;
            mockClient.NetworkError = true;
            Assert.That(mockClient.AuthenticationFailed, Is.True);
            Assert.That(mockClient.NetworkError, Is.True);
        }
    }
}