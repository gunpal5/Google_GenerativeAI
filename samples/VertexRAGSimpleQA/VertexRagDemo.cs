using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenerativeAI;
using GenerativeAI.Authenticators;
using GenerativeAI.Exceptions;
using GenerativeAI.Types;
using GenerativeAI.Types.RagEngine;

public class VertexRagDemo
{
    private readonly VertexAI _vertexAi;
    private readonly VertexRagManager _ragManager;
    private RagCorpus _corpus;
    private GenerativeModel _model;
    private readonly string _projectId;
    private readonly string _region;
    
    private string _documentationUrl;

    public VertexRagDemo(string projectId, string region, string serviceAccountFilePath)
    {
        _projectId = projectId;
        _region = region;
        var authenticator = new GoogleServiceAccountAuthenticator(serviceAccountFilePath);
        _vertexAi = new VertexAI(projectId, region, authenticator: authenticator);
        _ragManager = _vertexAi.CreateRagManager();
    }

    public async Task StartDemo(string documentationsUrl, string corpusName, string corpusDescription)
    {
        _documentationUrl = documentationsUrl;
        // Check if corpus exists, create if not
        _corpus = await GetOrCreateCorpus(corpusName, corpusDescription);

        if (!_corpus.Name.EndsWith(corpusName, StringComparison.OrdinalIgnoreCase))
        {
            // Scrape and import data
            await ScrapeAndImportData(_documentationUrl);
        }
        // Create generative model
        _model = _vertexAi.CreateGenerativeModel(VertexAIModels.Gemini.Gemini2Flash, corpusIdForRag: _corpus.Name);

        // Start QA chat
        await StartQaChat();
    }

    private async Task<RagCorpus> GetOrCreateCorpus(string corpusName, string corpusDescription)
    {
        try
        {
            var existingCorpus = await _ragManager.GetCorpusAsync(corpusName);
            if (existingCorpus != null)
            {
                Console.WriteLine($"Corpus '{corpusName}' already exists.");
                this._corpus = existingCorpus;
                return existingCorpus;
            }

            // If corpus doesn't exist, create a new one
            var newCorpus = await _ragManager.CreateCorpusAsync(corpusName, corpusDescription);
            if (newCorpus == null)
                throw new InvalidOperationException($"Failed to create corpus '{corpusName}'.");
            this._corpus = newCorpus;
            Console.WriteLine($"Corpus '{newCorpus.Name}' created.");
            return newCorpus;
        }
        catch (Exception ex)
        {
            var newCorpus = await _ragManager.CreateCorpusAsync(corpusName, corpusDescription);
            if (newCorpus == null)
                throw new InvalidOperationException($"Failed to create corpus '{corpusName}'.");
            this._corpus = newCorpus;
            Console.WriteLine($"Corpus '{_corpus.Name}' created.");
           

            return newCorpus;
        }
    }

    private async Task ScrapeAndImportData(string url)
    {
        Console.WriteLine("Crawling documentation...");
        var crawler = new ParallelWebCrawler(url);
        var textList = await crawler.CrawlUrlsParallel(url);

        int count = 0;
        await Parallel.ForEachAsync(textList, new ParallelOptions() { MaxDegreeOfParallelism = 50 }, async (text,ct) =>
        {
            try
            {
                count++;
                Console.WriteLine($"Uploading file {count}/{textList.Count} data...");
                var tmp = Path.GetTempFileName() + ".html";
                await File.WriteAllTextAsync(tmp, text,ct);
                await _ragManager.UploadLocalFileAsync(_corpus.Name, tmp,cancellationToken:ct);
            }catch(Exception ex)
            {
                Console.WriteLine($"Error importing file {count}/{textList.Count}: {ex.Message}");
            }
            count++;
        });

        Console.WriteLine("Data import completed.");
    }

    private async Task StartQaChat()
    {
        var chat = _model.StartChat();
        
        while (true)
        {
            Console.Write("Ask a question (or 'exit'): ");
            string question = Console.ReadLine();

            if (question.ToLower() == "exit")
            {
                break;
            }

            try
            {
                var result = await chat.GenerateContentAsync(question);
                Console.WriteLine($"Answer: {result.Text}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}