
using Xunit.Sdk;
using Xunit.v3;

namespace GenerativeAI.Tests.Base;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestPriorityAttribute : Attribute,ITraitAttribute
{
    public int Priority { get; private set; }

    public TestPriorityAttribute(int priority) => Priority = priority;
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
    {
        return new[] { new KeyValuePair<string, string>("TestPriority", Priority.ToString()) };
    }
}
public class PriorityOrderer : ITestCaseOrderer
{
    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase
    {
        var sortedMethods = new SortedDictionary<int, List<TTestCase>>();
        foreach (TTestCase testCase in testCases)
        {
            
            if (testCase.TestMethod.Traits.TryGetValue("TestPriority", out var values))
            {
                var priority = values.Select(int.Parse).FirstOrDefault();
                
                List<TTestCase>? cases = null;
                if(sortedMethods.TryGetValue(priority,out cases))
                    cases.Add(testCase);
                else
                    sortedMethods.Add(priority,new List<TTestCase>{ testCase });
            }
            else
            {
                List<TTestCase>? cases = null;
                if (sortedMethods.TryGetValue(int.MaxValue, out cases))
                    cases.Add(testCase);
                else
                    sortedMethods.Add(int.MaxValue, new List<TTestCase> { testCase });
            }
           
        }

        return sortedMethods.Keys.SelectMany(
            priority => sortedMethods[priority].OrderBy(
                testCase => testCase.TestMethod.MethodName)).ToList();
    }
}