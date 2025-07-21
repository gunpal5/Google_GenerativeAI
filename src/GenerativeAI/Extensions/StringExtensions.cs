﻿using System.Web;

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
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(modelName);
#else
        if (modelName == null) throw new ArgumentNullException(nameof(modelName));
#endif
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
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(corpusName);
#else
        if (corpusName == null) throw new ArgumentNullException(nameof(corpusName));
#endif
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
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fileName);
#else
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));
#endif
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
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(modelName);
#else
        if (modelName == null) throw new ArgumentNullException(nameof(modelName));
#endif
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
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fileName);
#else
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));
#endif
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

    /// <summary>
    /// Ensures an operation ID is in the correct format with "operations/" prefix.
    /// </summary>
    /// <param name="operationId">The operation ID to format.</param>
    /// <returns>The properly formatted operation ID.</returns>
    public static string RecoverOperationId(this string operationId)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(operationId);
#else
        if (operationId == null) throw new ArgumentNullException(nameof(operationId));
#endif
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
    /// Extracts the model ID from an operation ID string.
    /// </summary>
    /// <param name="operationId">The operation ID containing the model information.</param>
    /// <returns>The extracted model ID.</returns>
    public static string RecoverModelIdFromOperationId(this string operationId)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(operationId);
#else
        if (operationId == null) throw new ArgumentNullException(nameof(operationId));
#endif
#if NETSTANDARD2_0 || NET462_OR_GREATER
        if (operationId.Contains("/"))
#else
        if (operationId.Contains("/", StringComparison.InvariantCulture))
#endif
        {
            if (operationId.StartsWith("publishers/", StringComparison.InvariantCultureIgnoreCase))
            {
                return operationId;
            }
            else
            {
                var opId = operationId.Substring(operationId.LastIndexOf("/publishers") + 1);
                opId = opId.Remove(opId.IndexOf("/operations"));
                return $"{opId}";
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
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(contentName);
#else
        if (contentName == null) throw new ArgumentNullException(nameof(contentName));
#endif
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
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(corporaName);
#else
        if (corporaName == null) throw new ArgumentNullException(nameof(corporaName));
#endif
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
    
    
    /// <summary>
    /// Converts a string into camel case format.
    /// </summary>
    /// <param name="input">The input string to be converted.</param>
    /// <returns>
    /// A camel case representation of the input string, or an empty string if the input is null or whitespace.
    /// </returns>
    public static string ToCamelCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var words = input.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < words.Length; i++)
        {
            words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1);
        }

        return char.ToLowerInvariant(words[0][0]) + words[0].Substring(1) + string.Join(string.Empty, words.Skip(1));
    }
}