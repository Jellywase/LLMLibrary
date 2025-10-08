using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMLibrary
{
    public static class ModelInfo
    {
        public enum ModelName { gpt_oss_20b}
        public static Dictionary<ModelName, string> modelIDs = new()
        {
            { ModelName.gpt_oss_20b, "openai/gpt-oss-20b" }
        };
        public const int contextLimit = 8192;
    }
}
