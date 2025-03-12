using System.Text.Json.Serialization;

namespace AotTest;

[JsonSerializable(typeof(SampleJsonClass))]
[JsonSerializable(typeof(ComplexDataTypeClass.Child))]
[JsonSerializable(typeof(ComplexJsonClass.Child2))]
[JsonSerializable(typeof(ComplexJsonClass.Detail))]
[JsonSerializable(typeof(ComplexDataTypeClass))]
[JsonSerializable(typeof(ComplexJsonClass))]
[JsonSerializable(typeof(QueryStudentRecordRequest))]
[JsonSerializable(typeof(Weather))]
[JsonSerializable(typeof(Unit))]
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
internal partial class TestJsonSerializerContext : JsonSerializerContext
{
    
}