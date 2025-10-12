using LLMLibrary;
using System.Text;
using System.Text.Json.Nodes;
public class Program
{
    static object lockObj = new object();

    public static async Task Main(string[] args)
    {
        await Crawl();
    }

    private static async Task Crawl()
    {
        string url = "https://stackoverflow.com/questions/10646142/what-does-it-mean-to-escape-a-string";
        HttpClient httpClient = new();
        HttpResponseMessage response = await httpClient.GetAsync(url);
        string html = await response.Content.ReadAsStringAsync();
    }

    private static async Task CSE()
    {
        HttpClient httpClient = new();
        string apiKey = "AIzaSyDZfdGh8frPaD-N1iy-bDB_lfaEqCSQZoE";           // 발급받은 API Key
        string cseId = "90a2780d945314e2c";             // Custom Search Engine ID
        string query = "Apple";

        string url = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString(query)}&key={apiKey}&cx={cseId}";
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                JsonNode jsonNode = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

            }
            else
            {
                Console.WriteLine($"Error in response: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while GetAsync: {ex.Message}");
        }
    }
}