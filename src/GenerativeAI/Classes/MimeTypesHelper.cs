using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeAI.Classes
{
    public class MimeTypeHelper
    {
        private static readonly Dictionary<string, string> MimeTypeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".bmp", "image/bmp" },
            { ".gif", "image/gif" },
            { ".jpeg", "image/jpeg" },
            { ".jpg", "image/jpeg" },
            { ".png", "image/png" },
            { ".tiff", "image/tiff" },
            { ".tif", "image/tiff" },
        };

        public static string GetMimeType(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var extension = Path.GetExtension(fileName);

            if (extension != null && MimeTypeMappings.TryGetValue(extension.ToLower(), out var mimeType))
            {
                return mimeType;
            }

            // Default to application/octet-stream if the mapping is not found
            return "application/octet-stream";
        }
    }
}
