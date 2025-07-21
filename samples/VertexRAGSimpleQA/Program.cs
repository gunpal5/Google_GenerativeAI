using GenerativeAI;

var projectId = EnvironmentVariables.GOOGLE_PROJECT_ID ?? throw new InvalidOperationException("GOOGLE_PROJECT_ID environment variable is required");
var region = EnvironmentVariables.GOOGLE_REGION ?? throw new InvalidOperationException("GOOGLE_REGION environment variable is required");
var serviceAccountPath = Environment.GetEnvironmentVariable("Google_Service_Account_Json", EnvironmentVariableTarget.User) ?? throw new InvalidOperationException("Google_Service_Account_Json environment variable is required");

var demo = new VertexRagDemo(projectId, region, serviceAccountPath);

await demo.StartDemo("https://cloud.google.com/vertex-ai/generative-ai/docs/overview","3602879701896396800", "Vertex RAG Simple QA");

