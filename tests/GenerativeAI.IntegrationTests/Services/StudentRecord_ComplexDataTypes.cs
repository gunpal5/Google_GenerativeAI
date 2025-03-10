using System.ComponentModel;

namespace GenerativeAI.IntegrationTests;

public enum GradeLevel
{
    Freshman,
    Sophomore,
    Junior,
    Senior,
    Graduate
}
    
public class StudentRecord
{

    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public GradeLevel Level { get; set; } = GradeLevel.Freshman;
    public List<string> EnrolledCourses { get; set; } = new List<string>();
    public Dictionary<string, double> Grades { get; set; } = new Dictionary<string, double>();
    public DateTime EnrollmentDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;

    public double CalculateGPA()
    {
        if (Grades.Count == 0) return 0.0;
        return Grades.Values.Average();
    }
}

[Description("Request class containing filters for querying student records.")]
public class QueryStudentRecordRequest
{
    [Description("The student's full name.")]
    public string FullName { get; set; } = string.Empty;
    
    [Description("Grade filters for querying specific grades, e.g., Freshman or Senior.")]
    public List<GradeLevel> GradeFilters { get; set; } = new();
    
    [Description("The start date for the enrollment date range. ISO 8601 standard date")]
    public DateTime EnrollmentStartDate { get; set; }
        
    [Description("The end date for the enrollment date range. ISO 8601 standard date")]
    public DateTime EnrollmentEndDate { get; set; }
    
    [Description("The flag indicating whether to include only active students.")]
    public bool? IsActive { get; set; } = true;
}