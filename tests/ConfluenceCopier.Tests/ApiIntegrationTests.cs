using System;
using System.Threading.Tasks;
using Dapplo.Confluence.Entities;
using NUnit.Framework;
using ConfluenceCopier;

namespace ConfluenceCopierTests
{
    [TestFixture]
    public class ApiIntegrationTests : HeadlessTestBase
    {
        private MockConfluenceClient _mockClient = null!;

        [SetUp]
        public void Setup()
        {
            _mockClient = new MockConfluenceClient();

            // Set up mock pages with minimal data
            _mockClient.AddMockPage(new Content
            {
                Id = 123456,
                Title = "Source Page",
                Space = new Space { Key = "SRC", Name = "Source Space" }
            });

            _mockClient.AddMockPage(new Content
            {
                Id = 654321,
                Title = "Destination Page", 
                Space = new Space { Key = "DST", Name = "Destination Space" }
            });
        }

        [Test]
        public async Task MockClient_GetContent_ReturnsValidContent()
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
        public async Task MockClient_UpdateContent_ModifiesContent()
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
        public async Task MockClient_CreateContent_AddsNewContent()
        {
            // Arrange
            var newContent = new Content
            {
                Id = 0, // Will be assigned by mock
                Title = "New Page",
                Space = new Space { Key = "NEW", Name = "New Space" }
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
        public async Task MockClient_MultipleOperations_TracksApiCalls()
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
}