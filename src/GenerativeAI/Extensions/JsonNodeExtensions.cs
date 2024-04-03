using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GenerativeAI.Types;

namespace GenerativeAI.Extensions
{
    public static class JsonNodeExtensions
    {
        public static Content ToFunctionCallContent(this JsonNode? responseContent,string name)
        {
            var content = new Content()
            {
                Role = Roles.Function,
                Parts = new[]
                {
                    new Part()
                    {
                        FunctionResponse = new ChatFunctionResponse()
                        {
                            Name = name,
                            Response = new FunctionResponse() { Name = name, Content = responseContent }
                        }
                    }
                }
            };
        }
    }
}
