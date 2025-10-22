using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpToken;

namespace LLMLibrary
{
    public static class LLMUtility
    {
        public static List<string> ChunkByTokens(string text, int maxTokens = 4096, string modelName = "gpt-4")
        {
            var encoder = GptEncoding.GetEncodingForModel(modelName);
            List<int> tokens = encoder.Encode(text);
            List<string> chunks = new();

            int start = 0;

            while (start < tokens.Count)
            {
                int end = Math.Min(start + maxTokens, tokens.Count);

                // 토큰 슬라이스
                List<int> slice = tokens.GetRange(start, end - start);

                // 토큰을 다시 문자열로 복원
                string chunkText = encoder.Decode(slice);

                chunks.Add(chunkText);
                start = end;
            }

            return chunks;
        }
    }
}
