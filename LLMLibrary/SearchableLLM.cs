using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMLibrary
{
    public abstract class SearchableLLM : ILLMClient
    {
        ISearchEngine searchEngine;
        public SearchableLLM(ISearchEngine searchEngine)
        {
            this.searchEngine = searchEngine;
            if (searchEngine != null && !searchEngine.initialized)
            {
                searchEngine.Initialize();
            }
        }
        public abstract ModelInfo.ModelName modelName { get; }

        public abstract Task<bool> isConnected { get; }
        public abstract Task<IReadOnlyDictionary<string, Context>> contexts { get; }

        public abstract Task Connect();
        public abstract Task Disconnect();
        public abstract Task SetSystemMessage(string chatID, string message);
        public abstract Task<string> SendUserMessage(string chatID, string message, int retry = 0);
        public abstract Task DisposeChat(string chatID);
        public Task<string> Search(string query, int maxResults = 5)
        {
            return searchEngine?.Search(query, maxResults) ?? Task.FromResult(string.Empty);
        }

    }
}
