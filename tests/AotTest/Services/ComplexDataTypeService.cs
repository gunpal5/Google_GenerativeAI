namespace AotTest;

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