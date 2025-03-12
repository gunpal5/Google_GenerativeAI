using System.Text.Json.Serialization;

namespace AotTest;

/// <summary>
/// A sample class used to test serialization and deserialization with various data types.
/// </summary>
internal class ComplexDataTypeClass
{
    public string? Title { get; set; }
    [JsonIgnore] public Dictionary<string, string>? Metadata { get; set; }
    public int[]? Numbers { get; set; }
    public List<Child>? Children { get; set; }
    public string? OptionalField { get; set; }

    public class Child
    {
        public string? Name { get; set; }
        public List<int>? Values { get; set; }
    }
}