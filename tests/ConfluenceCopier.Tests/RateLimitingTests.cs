using System;
using NUnit.Framework;

namespace ConfluenceCopierTests
{
    [TestFixture]
    public class RateLimitingTests : HeadlessTestBase
    {
        private MockMainWindow _mockMainWindow = null!;

        [SetUp]
        public void Setup()
        {
            _mockMainWindow = TestHelper.CreateMockMainWindow();
        }

        [Test]
        public void RateLimit_MultipleQuickRequests_DelaysSubsequentRequests()
        {
            // Arrange
            _mockMainWindow.LastRequestTime = DateTime.Now.AddMilliseconds(-100); // 100ms ago

            // Act
            var isRateLimited = _mockMainWindow.IsRateLimited;

            // Assert
            Assert.That(isRateLimited, Is.True);
        }

        [Test]
        public void RateLimit_RequestAfterDelay_AllowsRequest()
        {
            // Arrange
            _mockMainWindow.LastRequestTime = DateTime.Now.AddSeconds(-5); // 5 seconds ago

            // Act
            var isRateLimited = _mockMainWindow.IsRateLimited;

            // Assert
            Assert.That(isRateLimited, Is.False);
        }

        [Test]
        public void ApplyRateLimit_WhenNeeded_DelaysExecution()
        {
            // Arrange
            _mockMainWindow.LastRequestTime = DateTime.Now.AddMilliseconds(-500); // 500ms ago

            // Act
            var startTime = DateTime.Now;
            _mockMainWindow.ApplyRateLimit();
            var endTime = DateTime.Now;

            var delayDuration = (int)(endTime - startTime).TotalMilliseconds;

            // Assert - Should have delayed for approximately 500ms (1000 - 500)
            Assert.That(delayDuration, Is.GreaterThanOrEqualTo(400)); // Allow some tolerance
            Assert.That(delayDuration, Is.LessThan(1000)); // Should not delay more than 1 second
        }

        [Test]
        public void ApplyRateLimit_WhenNotNeeded_DoesNotDelay()
        {
            // Arrange
            _mockMainWindow.LastRequestTime = DateTime.Now.AddSeconds(-2); // 2 seconds ago

            // Act
            var startTime = DateTime.Now;
            _mockMainWindow.ApplyRateLimit();
            var endTime = DateTime.Now;

            var delayDuration = (int)(endTime - startTime).TotalMilliseconds;

            // Assert - Should not have delayed
            Assert.That(delayDuration, Is.LessThan(100)); // Should be nearly instant
        }
    }
}