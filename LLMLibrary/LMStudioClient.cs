using System;
using System.Collections.Generic;
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
        public override Task<IReadOnlyDictionary<string, Context>> contexts => Task.FromResult((IReadOnlyDictionary<string, Context>)_contexts);
        Dictionary<string, Context> _contexts;

        readonly string modelsEndpoint = "http://192.168.0.2:1234/api/v0/models";
        readonly string chatEndpoint = "http://192.168.0.2:1234/api/v0/chat/completions";
        public LMStudioClient(ISearchEngine searchEngine) : base(searchEngine)
        {
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

            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(modelsEndpoint);
            var models = JsonSerializer.Deserialize<Dictionary<string, string>>(response);
            if (models == null || models.Count == 0)
            { return false; }
            return true;
        }
        public override async Task<string> SendUserMessage(string chatID, string message)
        {
            Context context = RegisterOrGetContext(chatID);
            context.Add(Context.Sayer.user, message);

            HttpClient client = new HttpClient();
            var payload = BuildContextPayload(context);
            var response = await client.PostAsJsonAsync(chatEndpoint, payload);
            var stream = await response.Content.ReadAsStreamAsync();
            using var responseJson = JsonDocument.Parse(stream);
            var choices = responseJson.RootElement.GetProperty("choices");
            string reply = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "No reply..";

            RecordToContext(Context.Sayer.assistant, context, reply);
            return reply;
        }
        public override Task SetSystemMessage(string chatID, string message)
        {
            Context context = RegisterOrGetContext(chatID);
            RecordToContext(Context.Sayer.system, context, message);
            return Task.CompletedTask;
        }
        object BuildContextPayload(Context context)
        {
            Dictionary<string, object> payload = new();
            payload["model"] = "your-model-name";
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
