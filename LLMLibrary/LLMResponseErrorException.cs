using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMLibrary
{
    internal class LLMResponseErrorException : Exception
    {
        public LLMResponseErrorException()
        {
        }
        public LLMResponseErrorException(string message)
            : base(message)
        {
        }
        public LLMResponseErrorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
