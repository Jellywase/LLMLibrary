namespace LLMLibrary
{
    public interface ISearchEngine
    {
        public bool initialized { get; }
        public Task Initialize();
        public Task Close();
        public Task<string> Search(string query, int maxResults = 5);
    }
}
