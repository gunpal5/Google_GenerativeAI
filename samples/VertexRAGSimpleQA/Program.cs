using GenerativeAI;

 var demo = new VertexRagDemo(EnvironmentVariables.GOOGLE_PROJECT_ID, EnvironmentVariables.GOOGLE_REGION,
     Environment.GetEnvironmentVariable("Google_Service_Account_Json", EnvironmentVariableTarget.User));

await demo.StartDemo("https://cloud.google.com/vertex-ai/generative-ai/docs/overview","3602879701896396800", "Vertex RAG Simple QA");

