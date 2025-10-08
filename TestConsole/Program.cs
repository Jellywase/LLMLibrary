using LLMLibrary;
public class Program
{

    public static async Task Main(string[] args)
    {
        //try
        {
            LMStudioClient llm = new(ModelInfo.ModelName.gpt_oss_20b, null);
            await llm.Connect();
            await llm.SetSystemMessage("MyChat", "You are a helpful assistant.");
            var reply = await llm.SendUserMessage("MyChat", "Hello, llm!. How are you today? Today's such a great nice day. isn't it?");
            await llm.DisposeChat("MyChat");
        }
        //catch (Exception ex)
        //{
        //    Console.WriteLine("LLM 오류 발생: " + ex.Message);
        //}
    }
}