using System.ComponentModel;
using CSharpToJsonSchema;

namespace AotTest;

public class StudentRecord
{
    public enum GradeLevel
    {
        Freshman,
        Sophomore,
        Junior,
        Senior,
        Graduate
    }

    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public GradeLevel Level { get; set; } = GradeLevel.Freshman;
    public List<string> EnrolledCourses { get; set; } = new List<string>();
    public Dictionary<string, double> Grades { get; set; } = new Dictionary<string, double>();
    public DateTime EnrollmentDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
   
}

[Description("Request class containing filters for querying student records.")]
public class QueryStudentRecordRequest
{
    [Description("The student's full name.")]
    public string FullName { get; set; } = string.Empty;

    [Description("Grade filters for querying specific grades, e.g., Freshman or Senior.")]
    public List<StudentRecord.GradeLevel> GradeFilters { get; set; } = new();

    [Description("The start date for the enrollment date range. ISO 8601 standard date")]
    public DateTime EnrollmentStartDate { get; set; }

    [Description("The end date for the enrollment date range. ISO 8601 standard date")]
    public DateTime EnrollmentEndDate { get; set; }

    [Description("The flag indicating whether to include only active students.")]
    public bool? IsActive { get; set; } = true;
}

public class ComplexDataTypeService : IComplexDataTypeService
{
    [System.ComponentModel.Description("Get student record for the year")]
    public async Task<StudentRecord> GetStudentRecordAsync(QueryStudentRecordRequest query,
        CancellationToken cancellationToken = default)
    {
        return new StudentRecord
        {
            StudentId = "12345",
            FullName = query.FullName,
            Level = StudentRecord.GradeLevel.Senior,
            EnrolledCourses = new List<string> { "Math 101", "Physics 202", "History 303" },
            Grades = new Dictionary<string, double>
            {
                { "Math 101", 3.5 },
                { "Physics 202", 3.8 },
                { "History 303", 3.9 }
            },
            EnrollmentDate = new DateTime(2020, 9, 1),
            IsActive = true
        };
    }
}

[GenerateJsonSchema()]
public interface IComplexDataTypeService
{
    [Description("Get student record for the year")]
    public Task<StudentRecord> GetStudentRecordAsync(QueryStudentRecordRequest query,
        CancellationToken cancellationToken = default);
}

