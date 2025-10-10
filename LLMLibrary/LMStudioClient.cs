using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LLMLibrary
{
    public class LMStudioClient : SearchableLLM
    {
        readonly object lockObj = new object();
        public override Task<bool> isConnected => CheckChattable();
        bool manuallyAllowed = false;
        HttpClient httpClient = new HttpClient();

        public override ModelInfo.ModelName modelName => modelName_I;
        ModelInfo.ModelName modelName_I;
        public string modelID => ModelInfo.modelIDs[modelName];
        public override Task<IReadOnlyDictionary<string, Context>> contexts => Task.FromResult((IReadOnlyDictionary<string, Context>)_contexts);

        Dictionary<string, Context> _contexts;

        Task currentCall = Task.CompletedTask;

        readonly string modelsEndpoint = "http://192.168.0.2:1234/api/v0/models";
        readonly string chatEndpoint = "http://192.168.0.2:1234/api/v0/chat/completions";
        public LMStudioClient(ModelInfo.ModelName modelName, ISearchEngine searchEngine) : base(searchEngine)
        {
            modelName_I = modelName;
            _contexts = new Dictionary<string, Context>();
        }

        public override Task Connect()
        {
            manuallyAllowed = true;
            return Task.CompletedTask;
        }
        public override Task Disconnect()
        {
            manuallyAllowed = false;
            return Task.CompletedTask;
        }
        async Task<bool> CheckChattable()
        {
            // 인터페이스 공통성을 위해 connect하지 않으면 채팅 불가한 것으로 간주
            if (!manuallyAllowed)
            { return false; }
            try
            {
                using var response = await httpClient.GetStreamAsync(modelsEndpoint);
                using var json = await JsonDocument.ParseAsync(response);
                var models = json?.RootElement.GetProperty("data").EnumerateArray().ToList();
                if (models == null || models.Count == 0)
                { throw new Exception("No models loaded"); }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot connect to LLM: " + ex.Message);
                return false;
            }
            return true;
        }
        public override async Task<string> SendUserMessage(string chatID, string message)
        {
            if (!await isConnected)
            { return string.Empty; }
            Task<string> nextTask;
            lock (lockObj)
            {
                nextTask = currentCall.ContinueWith(async _ =>
                {
                    try
                    {
                        return await SendUserMessageInner(chatID, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in LMStudioClient: " + ex.Message);
                        throw;
                    }
                }).Unwrap();
                currentCall = nextTask;
            }
            return await nextTask;
        }

        private async Task<string> SendUserMessageInner(string chatID, string message)
        {
            Context context = RegisterOrGetContext(chatID);
            context.Add(Context.Sayer.user, message);

            var payload = BuildContextPayload(context);
            var response = await httpClient.PostAsJsonAsync(chatEndpoint, payload);
            var stream = await response.Content.ReadAsStreamAsync();
            using var responseJson = JsonDocument.Parse(stream);
            if (responseJson.RootElement.TryGetProperty("error", out var error))
            {
                throw new InvalidOperationException("LLM server error: " + error.GetString());
            }
            string reply = string.Empty;
            if (responseJson.RootElement.TryGetProperty("choices", out var choices))
            {
                reply = choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
                RecordToContext(Context.Sayer.assistant, context, reply);
            }

            return reply;
        }

        public override async Task SetSystemMessage(string chatID, string message)
        {
            if (!await isConnected)
            { return; }
            Context context = RegisterOrGetContext(chatID);
            RecordToContext(Context.Sayer.system, context, message);
        }
        object BuildContextPayload(Context context)
        {
            Dictionary<string, object> payload = new();
            payload["model"] = modelID;
            payload["temperature"] = 0.7;
            payload["max_tokens"] = 8192;
            List<Dictionary<string, string>> messages = new();
            foreach (var (sayer, message) in context.record)
            {
                messages.Add(new Dictionary<string, string>
                {
                    ["role"] = sayer.ToString().ToLower(),
                    ["content"] = message
                });
            }
            payload["messages"] = messages;
            return payload;
        }
        Context RegisterOrGetContext(string chatID)
        {
            lock (lockObj)
            {
                if (!_contexts.TryGetValue(chatID, out var context))
                {
                    _contexts[chatID] = context = new Context();
                }
                return context;
            }
        }
        void RecordToContext(Context.Sayer sayer, Context context, string response)
        {
            lock (lockObj)
            {
                context.Add(sayer, response);
            }
        }

        public override Task DisposeChat(string chatID)
        {
            _contexts.Remove(chatID);
            return Task.CompletedTask;
        }

    }
}
