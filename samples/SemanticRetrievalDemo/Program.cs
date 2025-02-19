using DocumentChunker.Chunkers;
using DocumentChunker.Core;
using DocumentChunker.Enum;
using GenerativeAI;
using GenerativeAI.Authenticators;
using GenerativeAI.Clients;
using GenerativeAI.Types;

// This demo require a Google Service Account to run
// Please set the Google_Service_Account_Json environment variable to the path of the service account json file.
// Please refer to this link https://cloud.google.com/iam/docs/service-account-overview

async Task AddBooksToCorpus(CorporaManager corporaManager, string corpusName, string contentUrl, string bookName,
    string authorName)
{
    //Document Chunker
    var chunker = new PlainTextDocumentChunker(new ChunkerConfig(500, ChunkType.Paragraph));

    //Add Documents
    var doc1 = await corporaManager.AddDocumentAsync(corpusName, bookName,
        new List<CustomMetadata> { new CustomMetadata() { Key = "Author", StringValue = authorName } });

    await foreach (var parts in chunker.ExtractChunksInPartsFromUrlAsync(contentUrl, 100))
    {
        var chunks = parts.Select(s => new Chunk() { Data = new ChunkData() { StringValue = s } }).ToList();

        chunks = await corporaManager.AddChunksAsync(doc1.Name, chunks);
    }
}

//Initialize Retriever Model
var serviceAccountConfigFile =
    Environment.GetEnvironmentVariable("Google_Service_Account_Json");
if (string.IsNullOrEmpty(serviceAccountConfigFile) || !File.Exists(serviceAccountConfigFile))
{
    Console.WriteLine("Please set the \"Google_Service_Account_Json\" environment variable to the path of the service account json file.");
    Console.WriteLine("You can create a service account at https://console.cloud.google.com/iam-admin/serviceaccounts/project");
    Console.WriteLine("Press any key to exit.");
    Console.ReadKey();
    return;
}
var authenticator = new GoogleServiceAccountAuthenticator(serviceAccountConfigFile);
var retrieverModel = new SemanticRetrieverModel(GoogleAIModels.Aqa, EnvironmentVariables.GOOGLE_API_KEY,
    authenticator: authenticator);


//Create Corpus if doesn't exist
var corporaManager = retrieverModel.CorporaManager;

var corpora = await corporaManager.ListCorporaAsync();

Corpus? corpus = null;
if (corpora == null || corpora.All(c => c.DisplayName != "Generative AI Demo"))
{
    //Create Corpus
    corpus = await corporaManager.CreateCorpusAsync("Generative AI Demo");

    //Add Documents
    await AddBooksToCorpus(corporaManager, corpus.Name, "https://www.gutenberg.org/cache/epub/1184/pg1184.txt",
        "The Count of Monte Cristo", "Alexandre Dumas");

    await AddBooksToCorpus(corporaManager, corpus.Name, "https://www.gutenberg.org/cache/epub/75400/pg75400.txt",
        "The boys of Columbia High on the diamond or, Winning out by pluck", "Graham B. Forbes");
    
    Console.WriteLine("Corpus was created for books. listed below:\r\n1) The Count of Monte Cristo \r\n2) \"The boys of Columbia High on the diamond or, Winning out by pluck\"");
}
else
{
    corpus = corpora.First(c => c.DisplayName == "Generative AI Demo");
    Console.WriteLine("Corpus already exists.");
}

//Generate answer
Console.WriteLine("type 'exit' to exit");
Console.WriteLine("Please enter a question:");
Console.WriteLine("e.g. tell me something about The Count of Monte Cristo.");
var chatSession = retrieverModel.CreateChatSession(corpus.Name,AnswerStyle.VERBOSE);
do
{
    Console.WriteLine();
    Console.Write("Question: ");
    var question = Console.ReadLine();
    if (question.ToLower() == "exit")
        break;
    var response = await chatSession.GenerateAnswerAsync(question);
    
    Console.WriteLine();
    Console.WriteLine("Answer:");
    Console.WriteLine(response.GetAnswer());
    Console.WriteLine($"Answerable Probablity: {response.AnswerableProbability}");
    Console.WriteLine();
} while (true);