namespace LLMLibrary
{
    public interface ILLMClient
    {
        public Task<bool> isConnected { get; }
        public Task<IReadOnlyDictionary<string, Context>> contexts { get; }
        public Task Connect();
        public Task Disconnect();
        public Task<string> SendUserMessage(string chatID, string message);
        public Task SetSystemMessage(string chatID, string message);
        public Task DisposeChat(string chatID);
    }
}
