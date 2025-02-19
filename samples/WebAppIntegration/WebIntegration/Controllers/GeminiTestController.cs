using GenerativeAI;
using GenerativeAI.Web;
using Microsoft.AspNetCore.Mvc;

namespace WebIntegration.Controllers;

[ApiController]
[Route("gemini-test")]
public class GeminiTestController : Controller
{
    private readonly IGenerativeAiService _generativeAiService;
    public GeminiTestController(IGenerativeAiService generativeAiService)
    {
        this._generativeAiService = generativeAiService;
    }
    // GET
    [HttpGet(template:"generate")]
    public async Task<string> GenerateAsync(string prompt)
    {
        var model = _generativeAiService.CreateInstance(GoogleAIModels.Gemini2Flash);

        var response = await model.GenerateContentAsync(prompt).ConfigureAwait(false);
        
        return response.Text();
    }
}