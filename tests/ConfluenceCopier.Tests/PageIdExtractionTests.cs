using NUnit.Framework;
using ConfluenceCopier;

namespace ConfluenceCopierTests
{
    [TestFixture]
    public class PageIdExtractionTests : HeadlessTestBase
    {
        private MockMainWindow _mockMainWindow = null!;

        [SetUp]
        public void Setup()
        {
            _mockMainWindow = TestHelper.CreateMockMainWindow();
        }

        [Test]
        public void ExtractPageId_NumericId_ReturnsId()
        {
            // Arrange
            string input = "123456";

            // Act
            string result = _mockMainWindow.ExtractPageId(input);

            // Assert
            Assert.That(result, Is.EqualTo("123456"));
        }

        [Test]
        public void ExtractPageId_NumericIdWithWhitespace_ReturnsCleanId()
        {
            // Arrange
            string input = "  123456  ";

            // Act
            string result = _mockMainWindow.ExtractPageId(input);

            // Assert
            Assert.That(result, Is.EqualTo("123456"));
        }

        [Test]
        public void ExtractPageId_StandardWikiUrl_ReturnsId()
        {
            // Arrange
            string input = "https://example.atlassian.net/wiki/spaces/TEST/pages/123456/Page+Title";

            // Act
            string result = _mockMainWindow.ExtractPageId(input);

            // Assert
            Assert.That(result, Is.EqualTo("123456"));
        }

        [Test]
        public void ExtractPageId_ViewPageUrl_ReturnsId()
        {
            // Arrange
            string input = "https://example.atlassian.net/wiki/pages/viewpage.action?pageId=123456";

            // Act
            string result = _mockMainWindow.ExtractPageId(input);

            // Assert
            Assert.That(result, Is.EqualTo("123456"));
        }

        [Test]
        public void ExtractPageId_DisplayUrlWithPageId_ReturnsId()
        {
            // Arrange - Use a display URL with pageId parameter which is actually supported
            string input = "https://example.atlassian.net/display/TEST/Title?pageId=123456";

            // Act
            string result = _mockMainWindow.ExtractPageId(input);

            // Assert
            Assert.That(result, Is.EqualTo("123456"));
        }

        [Test]
        public void ExtractPageId_EmptyString_ThrowsArgumentException()
        {
            // Arrange
            string input = "";

            // Act & Assert
            Assert.Throws<System.ArgumentException>(() => _mockMainWindow.ExtractPageId(input));
        }

        [Test]
        public void ExtractPageId_InvalidUrl_ThrowsArgumentException()
        {
            // Arrange
            string input = "https://example.com/invalid/url";

            // Act & Assert
            Assert.Throws<System.ArgumentException>(() => _mockMainWindow.ExtractPageId(input));
        }
    }
}