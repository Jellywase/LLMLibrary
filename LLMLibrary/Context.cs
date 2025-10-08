using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMLibrary
{
    public class Context
    {
        public enum Sayer { system, user, assistant }
        public IEnumerable<(Sayer, string)> record => recordList;
        readonly List<(Sayer, string)> recordList = new();

        public void Add(Sayer sayer, string message)
        {
            recordList.Add((sayer, message));
        }
    }
}
