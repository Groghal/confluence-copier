using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapplo.Confluence.Entities;

namespace ConfluenceCopierTests
{
    /// <summary>
    /// Simplified mock for testing basic Confluence operations
    /// </summary>
    public class MockConfluenceClient
    {
        public Dictionary<long, Content> MockPages { get; } = new Dictionary<long, Content>();
        public bool AuthenticationFailed { get; set; } = false;
        public bool NetworkError { get; set; } = false;
        public List<DateTime> ApiCallTimes { get; } = new List<DateTime>();

        // Helper method for tests
        public void AddMockPage(Content content)
        {
            MockPages[content.Id] = content;
        }

        // Simple mock methods for basic testing
        public Task<Content> GetAsync(long id)
        {
            ApiCallTimes.Add(DateTime.Now);

            if (AuthenticationFailed)
                throw new UnauthorizedAccessException("Authentication failed");

            if (NetworkError)
                throw new Exception("Network error");

            if (MockPages.TryGetValue(id, out Content? content))
                return Task.FromResult(content);

            throw new KeyNotFoundException($"Content with ID {id} not found");
        }

        public Task<Content> UpdateAsync(Content content)
        {
            ApiCallTimes.Add(DateTime.Now);

            if (AuthenticationFailed)
                throw new UnauthorizedAccessException("Authentication failed");

            if (NetworkError)
                throw new Exception("Network error");

            if (MockPages.TryGetValue(content.Id, out Content? existingContent))
            {
                existingContent.Title = content.Title;
                if (content.Body != null)
                    existingContent.Body = content.Body;
                MockPages[content.Id] = existingContent;
                return Task.FromResult(existingContent);
            }
            
            throw new Exception($"Content with ID {content.Id} not found");
        }

        public Task<Content> CreateAsync(Content content)
        {
            ApiCallTimes.Add(DateTime.Now);

            if (AuthenticationFailed)
                throw new UnauthorizedAccessException("Authentication failed");

            if (NetworkError)
                throw new Exception("Network error");

            if (content.Id == 0)
                content.Id = MockPages.Count + 1;
            
            MockPages[content.Id] = content;
            return Task.FromResult(content);
        }
    }
}