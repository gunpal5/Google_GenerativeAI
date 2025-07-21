﻿using System.Text;
using System.Text.Json;
using GenerativeAI.Core;
using GenerativeAI.Types;

namespace GenerativeAI;

/// <summary>
/// Provides extension methods for the GenerateContentResponse type in the GenerativeAI.Types namespace.
/// </summary>
public static class GenerateContentResponseExtensions
{
    /// <summary>
    /// Returns the text of the first candidate’s first content part.
    /// </summary>
    /// <param name="response">The GenerateContentResponse to process.</param>
    /// <returns>The text if found; otherwise null.</returns>
    public static string? Text(this GenerateContentResponse response)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(response);
#else
        if (response == null)
            throw new ArgumentNullException(nameof(response));
#endif

        StringBuilder sb = new StringBuilder();
        if (response.Candidates != null)
        {
            foreach (var candidate in response.Candidates)
            {
                if (candidate.Content == null) continue;
                if (candidate.Content?.Parts == null) continue;

                foreach (var p in candidate.Content.Parts)
                {
                    sb.AppendLine(p.Text);
                }
            }
        }

        var text = sb.ToString();
        if (string.IsNullOrEmpty(text))
            return null;
        else return text;
    }

    /// <summary>
    /// Returns the first candidate’s first function call.
    /// </summary>
    /// <param name="response">The GenerateContentResponse to process.</param>
    /// <returns>The function call if found; otherwise null.</returns>
    public static FunctionCall? GetFunction(this GenerateContentResponse? response)
    {
        if (response == null || response.Candidates == null || response.Candidates.Length == 0)
            return null;

        var candidate = response.Candidates.FirstOrDefault();

        if (candidate == null || candidate.Content == null || candidate.Content.Parts == null ||
            candidate.Content.Parts.Count == 0)
        {
            return null;
        }

        var part = candidate.Content.Parts.FirstOrDefault();
        if (part == null || part.FunctionCall == null)
            return null;

        return part.FunctionCall;
    }

    /// <summary>
    /// Returns the first candidate’s first function call.
    /// </summary>
    /// <param name="response">The GenerateContentResponse to process.</param>
    /// <returns>The function call if found; otherwise null.</returns>
    public static List<FunctionCall>? GetFunctions(this GenerateContentResponse? response)
    {
        if (response == null || response.Candidates == null || response.Candidates.Length == 0)
            return null;

        var candidate = response.Candidates.FirstOrDefault();

        if (candidate == null || candidate.Content == null || candidate.Content.Parts == null ||
            candidate.Content.Parts.Count == 0)
        {
            return null;
        }

        var part = candidate.Content.Parts.FirstOrDefault();
        if (part == null || part.FunctionCall == null)
            return null;

        var funcs = candidate.Content.Parts.Where(p => p.FunctionCall != null).Select(p => p.FunctionCall!).ToList();
        
        return funcs.Count>0 ? funcs : null;
    }

    /// <summary>
    /// Extracts all code blocks from the provided GenerateContentResponse.
    /// </summary>
    /// <param name="response">The GenerateContentResponse containing potential code blocks.</param>
    /// <returns>A list of CodeBlock objects extracted from the response. Returns an empty list if no code blocks are found.</returns>
    public static List<CodeBlock> ExtractCodeBlocks(this GenerateContentResponse response)
    {
        List<CodeBlock> codeBlocks = new List<CodeBlock>();
        var candidates = response?.Candidates;
        if (candidates != null)
        {
            foreach (var candidate in candidates)
            {
                if (candidate.Content != null)
                {
                    var blocks = candidate.Content.ExtractCodeBlocks();
                    codeBlocks.AddRange(blocks);
                }
            }
        }

        return codeBlocks;
    }

    /// <summary>
    /// Extracts all JSON blocks from the provided GenerateContentResponse.
    /// </summary>
    /// <param name="response">The GenerateContentResponse containing potential JSON blocks.</param>
    /// <returns>A list of JsonBlock objects extracted from the response. Returns an empty list if no JSON blocks are found.</returns>
    public static List<JsonBlock> ExtractJsonBlocks(this GenerateContentResponse response)
    {
        List<JsonBlock> jsonBlocks = new List<JsonBlock>();
        var candidates = response?.Candidates;
        if (candidates != null)
        {
            foreach (var candidate in candidates)
            {
                if (candidate.Content != null)
                {
                    var blocks = candidate.Content.ExtractJsonBlocks();
                    jsonBlocks.AddRange(blocks);
                }
            }
        }

        return jsonBlocks;
    }

    /// <summary>
    /// Extracts all JSON blocks from the provided GenerateContentResponse.
    /// </summary>
    /// <param name="response">The GenerateContentResponse containing potential JSON blocks.</param>
    /// <param name="options">Optional JSON serializer options to use for deserialization.</param>
    /// <returns>A list of JsonBlock objects extracted from the response. Returns an empty list if no JSON blocks are found.</returns>
    public static T? ToObject<T>(this GenerateContentResponse response, JsonSerializerOptions? options = null)
        where T : class
    {
        var blocks = ExtractJsonBlocks(response);
        foreach (var block in blocks)
        {
            T? obj = block.ToObject<T>(options);
            if (obj != null)
                return obj;
        }

        return null;
    }

    /// <summary>
    /// Converts JSON blocks contained within the GenerateContentResponse into objects of the specified type.
    /// </summary>
    /// <param name="response">The GenerateContentResponse containing JSON blocks to convert.</param>
    /// <param name="options">Optional JSON serializer options to use for deserialization.</param>
    /// <typeparam name="T">The type to which the JSON blocks are converted.</typeparam>
    /// <returns>A list of objects of type T. Returns an empty list if no JSON blocks are found or successfully converted.</returns>
    public static List<T> ToObjects<T>(this GenerateContentResponse response, JsonSerializerOptions? options = null)
        where T : class
    {
        var blocks = ExtractJsonBlocks(response);
        List<T> objects = new List<T>();
        foreach (var block in blocks)
        {
            T? obj = block.ToObject<T>(options);
            if (obj != null)
                objects.Add(obj);
        }

        return objects;
    }

    /// <summary>
    /// Converts the response text to an enum value.
    /// </summary>
    /// <typeparam name="T">The enum type to convert to.</typeparam>
    /// <param name="response">The response containing the text to convert.</param>
    /// <param name="options">Optional JSON serializer options (not used in this implementation).</param>
    /// <returns>The parsed enum value, or the default value if parsing fails.</returns>
    public static T? ToEnum<T>(this GenerateContentResponse response, JsonSerializerOptions? options = null)
        where T : Enum
    {
        var text = Text(response);
        if (string.IsNullOrEmpty(text))
            return default;
        return (T)Enum.Parse(typeof(T), text, true);
    }
}