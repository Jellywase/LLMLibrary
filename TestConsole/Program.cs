using LLMLibrary;
public class Program
{
    static object lockObj = new object();
    public static async Task Main(string[] args)
    {
        //try
        {
            LMStudioClient llm = new(ModelInfo.ModelName.gpt_oss_20b, null);
            await llm.Connect();
            var a = await llm.isConnected;
            await llm.SetSystemMessage("MyChat", "You are a helpful assistant1.");
            Task tsk1 = llm.SendUserMessage("MyChat", "Hello, llm!. How are you today? Today's such a great nice day. isn't it?");
            Task tsk2 = llm.SendUserMessage("MyChat", "Hello, llm!. How are you today? Today's such a great nice day. isn't it?");
            Task tsk3 = llm.SendUserMessage("MyChat", "Hello, llm!. How are you today? Today's such a great nice day. isn't it?");


            Task.WaitAll(tsk1, tsk2, tsk3);

            await llm.DisposeChat("MyChat");
        }
        //catch (Exception ex)
        //{
        //    Console.WriteLine("LLM 오류 발생: " + ex.Message);
        //}
    }
}