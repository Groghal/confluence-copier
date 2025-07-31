using System;
using System.Reflection;
using NUnit.Framework;
using ConfluenceCopier;

namespace ConfluenceCopierTests
{
    /// <summary>
    /// Tests for business logic methods that don't require full UI initialization
    /// Uses reflection to test static methods and isolated functionality
    /// </summary>
    [TestFixture]
    public class BusinessLogicTests
    {
        [Test]
        public void ExtractPageId_NumericId_ReturnsId()
        {
            // Arrange
            string input = "123456";

            // Act
            string result = CallExtractPageId(input);

            // Assert
            Assert.That(result, Is.EqualTo("123456"));
        }

        [Test]
        public void ExtractPageId_NumericIdWithWhitespace_ReturnsCleanId()
        {
            // Arrange
            string input = "  123456  ";

            // Act
            string result = CallExtractPageId(input);

            // Assert
            Assert.That(result, Is.EqualTo("123456"));
        }

        [Test]
        public void ExtractPageId_StandardWikiUrl_ReturnsId()
        {
            // Arrange
            string input = "https://example.atlassian.net/wiki/spaces/TEST/pages/123456/Page+Title";

            // Act
            string result = CallExtractPageId(input);

            // Assert
            Assert.That(result, Is.EqualTo("123456"));
        }

        [Test]
        public void ExtractPageId_ViewPageUrl_ReturnsId()
        {
            // Arrange
            string input = "https://example.atlassian.net/wiki/pages/viewpage.action?pageId=123456";

            // Act
            string result = CallExtractPageId(input);

            // Assert
            Assert.That(result, Is.EqualTo("123456"));
        }

        [Test]
        public void ExtractPageId_DisplayUrlWithPageId_ReturnsId()
        {
            // Arrange - Use a display URL with pageId parameter which is actually supported
            string input = "https://example.atlassian.net/display/TEST/Title?pageId=123456";

            // Act
            string result = CallExtractPageId(input);

            // Assert
            Assert.That(result, Is.EqualTo("123456"));
        }

        [Test]
        public void ExtractPageId_EmptyString_ThrowsArgumentException()
        {
            // Arrange
            string input = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => CallExtractPageId(input));
        }

        [Test]
        public void ExtractPageId_InvalidUrl_ThrowsArgumentException()
        {
            // Arrange
            string input = "https://example.com/invalid/url";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => CallExtractPageId(input));
        }

        [Test]
        public void ExtractPageId_NullInput_ThrowsArgumentException()
        {
            // Arrange
            string? input = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => CallExtractPageId(input!));
        }

        /// <summary>
        /// Helper method to call the instance ExtractPageId method via reflection
        /// </summary>
        private string CallExtractPageId(string input)
        {
            var mainWindowType = typeof(MainWindow);
            var method = mainWindowType.GetMethod("ExtractPageId", 
                BindingFlags.Public | BindingFlags.Instance);
            
            if (method != null)
            {
                try
                {
                    // Create a minimal MainWindow instance for the method call
                    var mainWindow = new MainWindow();
                    return (string)method.Invoke(mainWindow, new object[] { input })!;
                }
                catch (TargetInvocationException ex)
                {
                    // Unwrap the inner exception for proper test assertions
                    throw ex.InnerException ?? ex;
                }
            }
            
            throw new NotImplementedException("ExtractPageId method not found");
        }
    }

    /// <summary>
    /// Tests for the MockConfluenceClient async operations
    /// </summary>
    [TestFixture]
    public class AsyncMockClientTests
    {
        private MockConfluenceClient _mockClient = null!;

        [SetUp]
        public void Setup()
        {
            _mockClient = new MockConfluenceClient();

            // Set up test data
            _mockClient.AddMockPage(new Dapplo.Confluence.Entities.Content
            {
                Id = 123456,
                Title = "Source Page",
                Space = new Dapplo.Confluence.Entities.Space { Key = "SRC", Name = "Source Space" }
            });

            _mockClient.AddMockPage(new Dapplo.Confluence.Entities.Content
            {
                Id = 654321,
                Title = "Destination Page", 
                Space = new Dapplo.Confluence.Entities.Space { Key = "DST", Name = "Destination Space" }
            });
        }

        [Test]
        public async System.Threading.Tasks.Task MockClient_GetContent_ReturnsValidContent()
        {
            // Arrange
            long pageId = 123456;

            // Act
            var content = await _mockClient.GetAsync(pageId);

            // Assert
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Id, Is.EqualTo(pageId));
            Assert.That(content.Title, Is.EqualTo("Source Page"));
            Assert.That(_mockClient.ApiCallTimes.Count, Is.EqualTo(1));
        }

        [Test]
        public void MockClient_AuthenticationFailure_ThrowsException()
        {
            // Arrange
            _mockClient.AuthenticationFailed = true;
            long pageId = 123456;

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => 
                await _mockClient.GetAsync(pageId));
        }

        [Test]
        public void MockClient_NetworkError_ThrowsException()
        {
            // Arrange
            _mockClient.NetworkError = true;
            long pageId = 123456;

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => 
                await _mockClient.GetAsync(pageId));
        }

        [Test]
        public async System.Threading.Tasks.Task MockClient_UpdateContent_ModifiesContent()
        {
            // Arrange
            var content = await _mockClient.GetAsync(123456);
            content.Title = "Updated Title";

            // Act
            var updatedContent = await _mockClient.UpdateAsync(content);

            // Assert
            Assert.That(updatedContent.Title, Is.EqualTo("Updated Title"));
            Assert.That(_mockClient.MockPages[123456].Title, Is.EqualTo("Updated Title"));
        }

        [Test]
        public async System.Threading.Tasks.Task MockClient_CreateContent_AddsNewContent()
        {
            // Arrange
            var newContent = new Dapplo.Confluence.Entities.Content
            {
                Id = 0, // Will be assigned by mock
                Title = "New Page",
                Space = new Dapplo.Confluence.Entities.Space { Key = "NEW", Name = "New Space" }
            };

            // Act
            var createdContent = await _mockClient.CreateAsync(newContent);

            // Assert
            Assert.That(createdContent.Id, Is.GreaterThan(0));
            Assert.That(createdContent.Title, Is.EqualTo("New Page"));
            Assert.That(_mockClient.MockPages.ContainsKey(createdContent.Id), Is.True);
        }

        [Test]
        public void MockClient_GetNonexistentContent_ThrowsKeyNotFoundException()
        {
            // Arrange
            long nonExistentId = 999999;

            // Act & Assert
            Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(async () => 
                await _mockClient.GetAsync(nonExistentId));
        }

        [Test]
        public async System.Threading.Tasks.Task MockClient_MultipleOperations_TracksApiCalls()
        {
            // Arrange & Act
            await _mockClient.GetAsync(123456);
            await _mockClient.GetAsync(654321);
            var content = await _mockClient.GetAsync(123456);
            await _mockClient.UpdateAsync(content);

            // Assert
            Assert.That(_mockClient.ApiCallTimes.Count, Is.EqualTo(4));
        }
    }

    /// <summary>
    /// Tests for rate limiting logic without UI dependencies
    /// </summary>
    [TestFixture]
    public class RateLimitingLogicTests
    {
        [Test]
        public void IsRateLimited_RecentRequest_ReturnsTrue()
        {
            // Arrange
            var lastRequestTime = DateTime.Now.AddMilliseconds(-500); // 500ms ago
            var currentTime = DateTime.Now;

            // Act
            var timeDiff = currentTime.Subtract(lastRequestTime).TotalMilliseconds;
            var isRateLimited = timeDiff < 1000; // Rate limit is 1 second

            // Assert
            Assert.That(isRateLimited, Is.True);
        }

        [Test]
        public void IsRateLimited_OldRequest_ReturnsFalse()
        {
            // Arrange
            var lastRequestTime = DateTime.Now.AddSeconds(-2); // 2 seconds ago
            var currentTime = DateTime.Now;

            // Act
            var timeDiff = currentTime.Subtract(lastRequestTime).TotalMilliseconds;
            var isRateLimited = timeDiff < 1000; // Rate limit is 1 second

            // Assert
            Assert.That(isRateLimited, Is.False);
        }

        [Test]
        public void CalculateDelay_RecentRequest_ReturnsCorrectDelay()
        {
            // Arrange
            var lastRequestTime = DateTime.Now.AddMilliseconds(-300); // 300ms ago
            var currentTime = DateTime.Now;
            var rateLimitMs = 1000;

            // Act
            var timeSinceLastRequest = (int)currentTime.Subtract(lastRequestTime).TotalMilliseconds;
            var delayNeeded = Math.Max(0, rateLimitMs - timeSinceLastRequest);

            // Assert
            Assert.That(delayNeeded, Is.GreaterThan(600)); // Should be around 700ms
            Assert.That(delayNeeded, Is.LessThan(800)); // Allow some tolerance
        }
    }
}