using System.Web;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for string-related model manipulations.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a model name string into a standardized model identifier.
    /// </summary>
    /// <param name="modelName">
    /// The input model name. If the string contains a forward slash ('/'), it must
    /// start with "models/" (case-insensitive). Otherwise, the method will prefix
    /// the provided name with "models/".
    /// </param>
    /// <returns>
    /// A string formatted as a valid model identifier. If the input string already starts
    /// with "models/" and contains a '/', it will be returned in lowercase.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the input string contains a '/' but does not start with "models/".
    /// </exception>
    public static string ToModelId(this string modelName)
    {
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (modelName.Contains("/"))
#else
        if (modelName.Contains("/", StringComparison.InvariantCulture))
#endif
        {
            if (modelName.StartsWith("models/", StringComparison.InvariantCultureIgnoreCase))
            {
                return modelName;
            }
            else
            {
                throw new ArgumentException($"Invalid model name. {modelName}");
            }
        }

        return $"models/{modelName}";
    }

    /// <summary>
    /// Converts a corpus name string into a standardized corpus identifier.
    /// </summary>
    /// <param name="corpusName">
    /// The input corpus name. If the string contains a forward slash ('/'), it must
    /// start with "corpora/" (case-insensitive). Otherwise, the method will prefix
    /// the provided name with "corpora/".
    /// </param>
    /// <returns>
    /// A string formatted as a valid corpus identifier. If the input string already starts
    /// with "corpora/" and contains a '/', it will be returned unchanged.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the input string contains a '/' but does not start with "corpora/".
    /// </exception>
    public static string ToRagCorpusId(this string corpusName)
    {
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (corpusName.Contains("/"))
#else
        if (corpusName.Contains("/", StringComparison.InvariantCulture))
#endif
        {
            if (corpusName.StartsWith("ragCorpora/", StringComparison.InvariantCultureIgnoreCase))
            {
                return corpusName;
            }
            else
            {
                if (corpusName.Contains("ragCorpora"))
                {
                    return $"ragCorpora/{corpusName.Substring(corpusName.LastIndexOf('/') + 1)}";
                }

                throw new ArgumentException($"Invalid corpus name. {corpusName}");
            }
        }

        return $"ragCorpora/{corpusName}";
    }

    /// <summary>
    /// Converts a file name into a standardized RAG file identifier.
    /// </summary>
    /// <param name="fileName">
    /// The input file name. If the string contains a forward slash ('/'), it must
    /// start with "corpora/" (case-insensitive). Otherwise, the method will prefix
    /// the file name with "corpora/".
    /// </param>
    /// <returns>
    /// A string formatted as a valid RAG file identifier. If the input string
    /// already starts with "corpora/" and contains a '/', it will be returned in its original form.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the input string contains a '/' but does not start with "corpora/".
    /// </exception>
    public static string ToRagFileId(this string fileName)
    {
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (fileName.Contains("/"))
#else
        if (fileName.Contains("/", StringComparison.InvariantCulture))
#endif
        {
            if (fileName.StartsWith("ragCorpora/", StringComparison.InvariantCultureIgnoreCase))
            {
                return fileName;
            }
            else
            {
                if (fileName.Contains("ragCorpora"))
                {
                    var l = fileName.Substring(fileName.LastIndexOf("ragCorpora/"));
                    return l;
                }
                throw new ArgumentException($"Invalid rag file name. {fileName}");
            }
        }
        else throw new ArgumentException($"Invalid rag file name. {fileName}");
    }

    /// <summary>
    /// Converts a given model name into a formatted tuned model identifier.
    /// </summary>
    /// <param name="modelName">The name of the model to convert into a tuned model identifier.</param>
    /// <returns>
    /// A string representing the tuned model identifier, prefixed with "tunedModels/" if it is not already formatted correctly.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided model name is invalid, such as when it contains a "/" but does not start with "tunedModels/".
    /// </exception>
    public static string ToTunedModelId(this string modelName)
    {
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (modelName.Contains("/"))
#else
        if (modelName.Contains("/", StringComparison.InvariantCulture))
#endif
        {
            if (modelName.StartsWith("tunedModels/", StringComparison.InvariantCulture))
            {
                return modelName;
            }
            else
            {
                throw new ArgumentException($"Invalid model name. {modelName}");
            }
        }

        return $"tunedModels/{modelName}";
    }

    /// <summary>
    /// Converts a file name or path string into a standardized file identifier.
    /// </summary>
    /// <param name="fileName">
    /// The input file name or path. If the string contains a forward slash ('/'), it must
    /// start with "files/" (case-insensitive). Otherwise, the method will prefix
    /// the provided name with "files/".
    /// </param>
    /// <returns>
    /// A string formatted as a valid file identifier. If the input string already starts
    /// with "files/" and contains a '/', it will be returned in lowercase.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the input string contains a '/' but does not start with "files/".
    /// </exception>
    public static string ToFileId(this string fileName)
    {
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (fileName.Contains("/"))
#else
        if (fileName.Contains("/", StringComparison.InvariantCulture))
#endif
        {
            if (fileName.StartsWith("files/", StringComparison.InvariantCultureIgnoreCase))
            {
                return fileName;
            }
            else
            {
                throw new ArgumentException($"Invalid file name. {fileName}");
            }
        }

        return $"files/{fileName}";
    }

    public static string RecoverOperationId(this string operationId)
    {
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (operationId.Contains("/"))
#else
        if (operationId.Contains("/", StringComparison.InvariantCulture))
#endif
        {
            if (operationId.StartsWith("operations/", StringComparison.InvariantCultureIgnoreCase))
            {
                return operationId;
            }
            else
            {
                var opId = operationId.Substring(operationId.LastIndexOf('/') + 1);
                return $"operations/{opId}";
            }
        }

        return $"operations/{operationId}";
    }


    /// <summary>
    /// Converts a content name or path string into a standardized cached content identifier.
    /// </summary>
    /// <param name="contentName">
    /// The input content name or path. If the string contains a forward slash ('/'), it must
    /// start with "cachedContent/" (case-insensitive). Otherwise, the method will prefix
    /// the provided name with "cachedContent/".
    /// </param>
    /// <returns>
    /// A string formatted as a valid cached content identifier. If the input string already starts
    /// with "cachedContent/" and contains a '/', it will be returned in lowercase.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the input string contains a '/' but does not start with "cachedContent/".
    /// </exception>
    public static string ToCachedContentId(this string contentName)
    {
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (contentName.Contains("/"))
#else
        if (contentName.Contains("/", StringComparison.InvariantCulture))
#endif
        {
            if (contentName.StartsWith("cachedContents/", StringComparison.InvariantCultureIgnoreCase))
            {
                return contentName;
            }
            else
            {
                throw new ArgumentException($"Invalid content name. {contentName}");
            }
        }

        return $"cachedContents/{contentName}";
    }


    /// <summary>
    /// Converts a content name or path string into a standardized corpora content identifier.
    /// </summary>
    /// <param name="corporaName">
    /// The input corpora content name or path. If the string contains a forward slash ('/'), it must
    /// start with "corpora/" (case-insensitive). Otherwise, the method will prefix
    /// the provided name with "corpora/".
    /// </param>
    /// <returns>
    /// A string formatted as a valid corpora content identifier. If the input string already starts
    /// with "corpora/" and contains a '/', it will be returned in lowercase.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the input string contains a '/' but does not start with "corpora/".
    /// </exception>
    public static string ToCorpusId(this string corporaName)
    {
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (corporaName.Contains("/"))
#else
        if (corporaName.Contains("/", StringComparison.InvariantCulture))
#endif
        {
            if (corporaName.StartsWith("corpora/", StringComparison.InvariantCultureIgnoreCase))
            {
                return corporaName;
            }
            else
            {
                throw new ArgumentException($"Invalid corpora name. {corporaName}");
            }
        }

        return $"corpora/{corporaName}";
    }

    /// <summary>
    /// Masks the API key present in the query string of a URL.
    /// </summary>
    /// <param name="url">
    /// The input URL containing an API key as part of its query parameters. If the key exists, it will be replaced with "Google_API_Key".
    /// </param>
    /// <returns>
    /// A new URL string with the API key masked. If the input URL does not contain a key or the input is null/whitespace, it returns the original URL.
    /// </returns>
    public static string MaskApiKey(this string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        var uri = new Uri(url);
        var query = HttpUtility.ParseQueryString(uri.Query);

        if (query["key"] != null)
        {
            query["key"] = "Google_API_Key";
        }

        var uriBuilder = new UriBuilder(uri)
        {
            Query = query.ToString()
        };

        return uriBuilder.ToString();
    }
}