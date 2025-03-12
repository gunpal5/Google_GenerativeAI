namespace AotTest;

/// <summary>
/// A complex sample class with nested classes and collections used for testing JSON deserialization.
/// </summary>
internal class ComplexJsonClass
{
    public string? Description { get; set; }
    public Detail? Details { get; set; }
    public List<Child2>? Children { get; set; }

    public class Detail
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
    }

    public class Child2
    {
        public string? Name { get; set; }
        public List<int>? Values { get; set; }
    }
}