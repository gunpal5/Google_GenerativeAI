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