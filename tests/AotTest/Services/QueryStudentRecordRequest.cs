using System.ComponentModel;

namespace AotTest;

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