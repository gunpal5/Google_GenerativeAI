using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeAI.Exceptions
{
    public class GenerativeAIException : Exception
    {
        public string Details { get; private set; }
        public GenerativeAIException(string message, string details) : base(message)
        {
            Details = details;
        }
    }
}
