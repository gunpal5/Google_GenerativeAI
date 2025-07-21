using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class ParallelWebCrawler
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly ConcurrentBag<string> _crawledUrls = new ConcurrentBag<string>();
    private readonly ConcurrentBag<string> _allText = new ConcurrentBag<string>();

    private readonly ConcurrentDictionary<string, bool>
        _visitedUrls = new ConcurrentDictionary<string, bool>(); //Track visited urls.

    private readonly string _baseUrlPattern;

    public ParallelWebCrawler(string baseUrl)
    {
        _baseUrlPattern = baseUrl.Substring(0, baseUrl.LastIndexOf('/'));
    }

    public Task<List<string>> CrawlUrlsParallel(string startUrl)
    {
        var urlsToCrawl = new ConcurrentQueue<string>();
        urlsToCrawl.Enqueue(startUrl);

        var tasks = new List<Task>();

        while (!urlsToCrawl.IsEmpty)
        {
            Parallel.For(0, urlsToCrawl.Count, new ParallelOptions(){MaxDegreeOfParallelism = 50}, i =>
            {
                if (urlsToCrawl.TryDequeue(out var url))
                {
                    if(url.Contains("#"))
                        url = url.Substring(0, url.IndexOf('#'));
                    if (_visitedUrls.TryAdd(url, true))
                    {
                        try
                        {
                            if(url.Contains("reference") || url.Contains("samples"))
                                return;
                            var html = _httpClient.GetStringAsync(url).Result;
                            var doc = new HtmlDocument();
                            doc.LoadHtml(html);

                            var article = doc.DocumentNode.Descendants("article").FirstOrDefault();

                            string text = article != null ? article.OuterHtml : html;
                            _allText.Add(text);

                            var links = doc.DocumentNode.SelectNodes("//a[@href]")
                                ?.Select(node => node.Attributes["href"].Value).ToList();
                            if (links != null)
                            {
                                foreach (var link in links)
                                {
                                    var absoluteLink = GetAbsoluteUrl(url, link);
                                    if (absoluteLink != null && absoluteLink.StartsWith(_baseUrlPattern) &&
                                        !_visitedUrls.ContainsKey(absoluteLink))
                                    {
                                        urlsToCrawl.Enqueue(absoluteLink);
                                    }
                                }
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            Console.WriteLine($"Error crawling {url}: {ex.Message}");
                        }
                        catch (HtmlAgilityPack.HtmlWebException ex)
                        {
                            Console.WriteLine($"HTML Parse error crawling {url}: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error crawling {url}: {ex.Message}");
                        }
                    }
                }
            });
        }
       
        return Task.FromResult(_allText.ToList());
    }

    private string? GetAbsoluteUrl(string baseUrl, string relativeUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUrl))
            {
                return null;
            }

            if (relativeUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                relativeUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return relativeUrl;
            }

            if (relativeUrl.StartsWith("//"))
            {
                return new Uri(new Uri(baseUrl), relativeUrl).AbsoluteUri;
            }

            if (relativeUrl.StartsWith("/"))
            {
                return new Uri(new Uri(baseUrl).GetLeftPart(UriPartial.Authority) + relativeUrl).AbsoluteUri;
            }

            return new Uri(new Uri(baseUrl), relativeUrl).AbsoluteUri;
        }
        catch
        {
            return null;
        }
    }
}