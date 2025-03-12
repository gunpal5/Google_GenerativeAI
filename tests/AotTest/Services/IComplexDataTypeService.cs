using System.ComponentModel;
using CSharpToJsonSchema;

namespace AotTest;

[GenerateJsonSchema()]
public interface IComplexDataTypeService
{
    [Description("Get student record for the year")]
    public Task<StudentRecord> GetStudentRecordAsync(QueryStudentRecordRequest query,
        CancellationToken cancellationToken = default);
}