using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;

namespace GenerativeAI.Microsoft.Extensions
{
    /// <summary>
    /// Comprehensive date/time transformer that handles all nested structures and edge cases.
    /// </summary>
    internal static class ComprehensiveDateTimeTransformer
    {
        private enum TransformationType
        {
            None,
            DateOnly,
            TimeOnly,
            Object,
            Array
        }

        private class TransformationMetadata
        {
            public TransformationType Type { get; set; } = TransformationType.None;
            public JsonNode? Schema { get; set; }
            public Dictionary<string, TransformationMetadata>? Properties { get; set; }
            public TransformationMetadata? ItemsMetadata { get; set; }
        }

        /// <summary>
        /// Main entry point for transforming function call arguments.
        /// </summary>
        public static IDictionary<string, object?>? TransformFunctionCallArguments(
            JsonNode? functionCallArgs,
            string? functionName,
            ChatOptions? options)
        {
            if (functionCallArgs == null)
                return null;

            var obj = functionCallArgs.AsObject();
            if (obj == null)
                return null;

            if (string.IsNullOrEmpty(functionName) || options?.Tools == null)
            {
                return obj.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value?.DeepClone());
            }

            // Find the function schema
            var function = options.Tools.OfType<AIFunction>().FirstOrDefault(f => f.Name == functionName);
            if (function?.JsonSchema == null)
            {
                return obj.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value?.DeepClone());
            }

            // Convert schema to JsonNode
            var schemaNode = JsonSerializer.SerializeToNode(function.JsonSchema);
            var propertiesSchema = schemaNode?["properties"];
            if (propertiesSchema == null)
            {
                return obj.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value?.DeepClone());
            }

            // Build transformation metadata from schema
            var metadata = AnalyzeSchema(propertiesSchema);

            // Transform the arguments
            var result = new Dictionary<string, object?>();
            foreach (var kvp in obj)
            {
                if (kvp.Value != null && metadata.Properties != null && metadata.Properties.TryGetValue(kvp.Key, out var propMetadata))
                {
                    // Special case: if schema expects object but value is array,
                    // and object has a single array property, treat the array as that property
                    if (propMetadata.Type == TransformationType.Object && kvp.Value is JsonArray arrayValue)
                    {
                        // Check if the object schema has a property that is an array
                        if (propMetadata.Properties != null)
                        {
                            // Find the array property (usually named like "Events", "Items", etc.)
                            foreach (var prop in propMetadata.Properties)
                            {
                                if (prop.Value.Type == TransformationType.Array)
                                {
                                    // Transform the array using the array property's schema
                                    var transformedArray = TransformValue(arrayValue, prop.Value);
                                    // Wrap it in an object with the property name
                                    var wrappedObj = new JsonObject { [prop.Key] = transformedArray as JsonNode ?? JsonValue.Create(transformedArray) };
                                    result[kvp.Key] = wrappedObj;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        var transformed = TransformValue(kvp.Value, propMetadata);
                        result[kvp.Key] = transformed;
                    }
                }
                else
                {
                    result[kvp.Key] = kvp.Value?.DeepClone();
                }
            }

            return result;
        }

        /// <summary>
        /// Analyzes a JSON schema and builds transformation metadata.
        /// </summary>
        private static TransformationMetadata AnalyzeSchema(JsonNode? schema)
        {
            var metadata = new TransformationMetadata { Schema = schema };

            if (schema == null)
                return metadata;

            // Handle object with properties
            if (schema is JsonObject schemaObj)
            {
                // Check if this is a properties object
                if (schemaObj.Count > 0 && !schemaObj.ContainsKey("type"))
                {
                    // This is a properties collection
                    metadata.Type = TransformationType.Object;
                    metadata.Properties = new Dictionary<string, TransformationMetadata>();
                    
                    foreach (var prop in schemaObj)
                    {
                        metadata.Properties[prop.Key] = AnalyzePropertySchema(prop.Value);
                    }
                }
                else
                {
                    // This is a single schema definition
                    return AnalyzePropertySchema(schema);
                }
            }

            return metadata;
        }

        /// <summary>
        /// Analyzes a single property schema.
        /// </summary>
        private static TransformationMetadata AnalyzePropertySchema(JsonNode? schema)
        {
            var metadata = new TransformationMetadata { Schema = schema };

            if (schema == null)
                return metadata;

            // Get type and format
            var typeNode = schema["type"];
            var formatNode = schema["format"];

            string? type = null;
            string? format = null;

            if (typeNode is JsonValue typeValue)
                typeValue.TryGetValue<string>(out type);
            if (formatNode is JsonValue formatValue)
                formatValue.TryGetValue<string>(out format);

            // Determine transformation type
            if (type == "string")
            {
                if (format == "date" || format == "date-time")
                {
                    metadata.Type = TransformationType.DateOnly;
                    System.Diagnostics.Debug.WriteLine($"[Analyze] Found DateOnly field with format: {format}");
                }
                else if (format == "time")
                {
                    metadata.Type = TransformationType.TimeOnly;
                    System.Diagnostics.Debug.WriteLine($"[Analyze] Found TimeOnly field with format: {format}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Analyze] String field with no special format: type='{type}', format='{format}'");
                }
            }
            else if (type == "object")
            {
                metadata.Type = TransformationType.Object;
                var properties = schema["properties"];
                if (properties is JsonObject propsObj)
                {
                    metadata.Properties = new Dictionary<string, TransformationMetadata>();
                    foreach (var prop in propsObj)
                    {
                        metadata.Properties[prop.Key] = AnalyzePropertySchema(prop.Value);
                    }
                }
            }
            else if (type == "array")
            {
                metadata.Type = TransformationType.Array;
                var items = schema["items"];
                if (items != null)
                {
                    metadata.ItemsMetadata = AnalyzePropertySchema(items);
                }
            }

            return metadata;
        }

        /// <summary>
        /// Transforms a value based on its metadata.
        /// </summary>
        private static object? TransformValue(JsonNode? value, TransformationMetadata? metadata)
        {
            if (value == null || metadata == null)
                return value?.DeepClone();

            // Debug removed

            switch (metadata.Type)
            {
                case TransformationType.DateOnly:
                    return TransformToDateOnly(value);
                    
                case TransformationType.TimeOnly:
                    return TransformToTimeOnly(value);
                    
                case TransformationType.Object:
                    return TransformObject(value, metadata);
                    
                case TransformationType.Array:
                    return TransformArray(value, metadata);
                    
                default:
                    return value.DeepClone();
            }
        }

        /// <summary>
        /// Transforms an object and its properties.
        /// </summary>
        private static object? TransformObject(JsonNode? value, TransformationMetadata metadata)
        {
            if (value is not JsonObject jsonObj)
                return value?.DeepClone();

            var result = new JsonObject();

            foreach (var kvp in jsonObj)
            {
                if (kvp.Value != null && metadata.Properties != null && 
                    metadata.Properties.TryGetValue(kvp.Key, out var propMetadata))
                {
                    var transformed = TransformValue(kvp.Value, propMetadata);
                    
                    // Special handling to ensure JsonNode values are preserved correctly
                    if (transformed is JsonNode node)
                    {
                        result[kvp.Key] = node;
                    }
                    else if (transformed is string stringValue)
                    {
                        result[kvp.Key] = JsonValue.Create(stringValue);
                    }
                    else if (transformed != null)
                    {
                        result[kvp.Key] = JsonValue.Create(transformed);
                    }
                    else
                    {
                        result[kvp.Key] = null;
                    }
                }
                else
                {
                    // Property not in schema - preserve as-is
                    result[kvp.Key] = kvp.Value?.DeepClone();
                }
            }

            return result;
        }

        /// <summary>
        /// Transforms an array and its items.
        /// </summary>
        private static object? TransformArray(JsonNode? value, TransformationMetadata metadata)
        {
            if (value is not JsonArray jsonArray)
                return value?.DeepClone();

            var result = new JsonArray();

            foreach (var item in jsonArray)
            {
                var transformed = TransformValue(item, metadata.ItemsMetadata);
                
                // Special handling to ensure JsonNode values are preserved correctly
                if (transformed is JsonNode node)
                {
                    result.Add(node);
                }
                else if (transformed is string stringValue)
                {
                    result.Add(JsonValue.Create(stringValue));
                }
                else if (transformed != null)
                {
                    result.Add(JsonValue.Create(transformed));
                }
                else
                {
                    result.Add(null);
                }
            }

            return result;
        }

        /// <summary>
        /// Transforms a value to DateOnly format (yyyy-MM-dd).
        /// </summary>
        private static object? TransformToDateOnly(JsonNode? value)
        {
            if (value is not JsonValue jsonValue)
                return value?.DeepClone();

            if (!jsonValue.TryGetValue<string>(out var stringValue) || string.IsNullOrEmpty(stringValue))
                return value.DeepClone();

            // Check if already in correct format
            if (Regex.IsMatch(stringValue, @"^\d{4}-\d{2}-\d{2}$"))
            {
                System.Diagnostics.Debug.WriteLine($"[DateOnly] Already in correct format: '{stringValue}'");
                return value.DeepClone();
            }

            // Try multiple parsing strategies using TryParse (much more flexible)
            DateTime parsedDate;
            
            // Strategy 1: Simple TryParse with InvariantCulture (handles ISO dates automatically)
            if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                var result = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                System.Diagnostics.Debug.WriteLine($"[DateOnly] Transformed '{stringValue}' to '{result}' using InvariantCulture TryParse");
                return JsonValue.Create(result);
            }
            
            // Strategy 2: Try with US culture for natural language formats like "January 15, 2024"
            if (DateTime.TryParse(stringValue, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out parsedDate))
            {
                var result = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                System.Diagnostics.Debug.WriteLine($"[DateOnly] Transformed '{stringValue}' to '{result}' using US Culture TryParse");
                return JsonValue.Create(result);
            }

            // If all parsing fails, return original
            return value.DeepClone();
        }

        /// <summary>
        /// Transforms a value to TimeOnly format (HH:mm:ss).
        /// </summary>
        private static object? TransformToTimeOnly(JsonNode? value)
        {
            if (value is not JsonValue jsonValue)
                return value?.DeepClone();

            if (!jsonValue.TryGetValue<string>(out var stringValue) || string.IsNullOrEmpty(stringValue))
                return value.DeepClone();

            // Check if already in correct format HH:mm:ss
            if (Regex.IsMatch(stringValue, @"^\d{2}:\d{2}:\d{2}$"))
                return value.DeepClone();

            // Check if in HH:mm format - preserve as-is (don't add seconds)
            if (Regex.IsMatch(stringValue, @"^\d{1,2}:\d{2}$"))
            {
                // Return the value as-is to preserve HH:mm format
                return value.DeepClone();
            }

            // Try parsing as DateTime first
            if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, 
                DateTimeStyles.None, out var dateTime))
            {
                return JsonValue.Create(dateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
            }

            // Try with dummy date for time-only strings
            if (DateTime.TryParse("2000-01-01 " + stringValue, CultureInfo.InvariantCulture, 
                DateTimeStyles.None, out dateTime))
            {
                return JsonValue.Create(dateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
            }

            // Try common time formats
            string[] formats = {
                "h:mm tt", "h:mm:ss tt", "hh:mm tt", "hh:mm:ss tt",
                "H:mm", "H:mm:ss",
                "HH:mm", "HH:mm:ss",
                "h:mmtt", "h:mm:sstt", "hh:mmtt", "hh:mm:sstt"
            };

            // First try parsing the time directly with formats
            if (DateTime.TryParseExact(stringValue, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                System.Diagnostics.Debug.WriteLine($"[TimeOnly] Direct format parse '{stringValue}' -> '{dateTime.ToString("HH:mm:ss")}'");
                return JsonValue.Create(dateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
            }

            // Then try with dummy date prefix
            if (DateTime.TryParseExact("2000-01-01 " + stringValue, 
                formats.Select(f => "yyyy-MM-dd " + f).ToArray(), 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                System.Diagnostics.Debug.WriteLine($"[TimeOnly] Prefixed format parse '{stringValue}' -> '{dateTime.ToString("HH:mm:ss")}'");
                return JsonValue.Create(dateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
            }

            // If all parsing fails, return original
            return value.DeepClone();
        }
    }
}