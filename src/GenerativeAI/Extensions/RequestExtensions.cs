using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeAI.Types;

namespace GenerativeAI.Helpers
{
    public class RequestExtensions
    {
        public static Content FormatGenerateContentInput(string @params)
        {
            var parts = new[]{new Part(){Text = @params}};
            return new Content(parts, Roles.User);
        }

        public static Content FormatGenerateContentInput( IEnumerable<string> request)
        {
            var parts = request.Select(part => new Part() { Text = part }).ToArray();

            return new Content(parts, Roles.User);
        }

        public static Content FormatGenerateContentInput(IEnumerable<Part> request)
        {
            return new Content(request.ToArray(), Roles.User);
        }
    }
}
