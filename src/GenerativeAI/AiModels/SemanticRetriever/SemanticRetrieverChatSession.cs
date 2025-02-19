using GenerativeAI.Core;
using GenerativeAI.Types;
using Microsoft.Extensions.Logging;

namespace GenerativeAI;

/// <summary>
/// Represents a chat session that interacts with a semantic retriever model to generate answers
/// based on user queries. Includes functionality for maintaining session history, managing configurations,
/// and handling responses from the model.
/// </summary>
public class SemanticRetrieverChatSession
{
    private readonly SemanticRetrieverModel _model;
    private readonly string _corpusId;
    private readonly List<SafetySetting>? _safetySettings;
    private readonly AnswerStyle _answerStyle;

    /// <summary>
    /// Represents a chat session that utilizes the semantic retriever model to generate answers.
    /// </summary>
    public SemanticRetrieverChatSession(SemanticRetrieverModel model, string corpusId,
        AnswerStyle? answerStyle = AnswerStyle.VERBOSE,
        List<Content>? history = null, List<SafetySetting>? safetySettings = null)
    {
        _model = model;
        _corpusId = corpusId;
        _safetySettings = safetySettings;
        _answerStyle = answerStyle ?? AnswerStyle.VERBOSE;
        this.History = history ?? new();
    }


    #region Properties

    private List<Content> _history = new List<Content>();

    /// <summary>
    /// Gets the content of the last request made in the current chat session,
    /// allowing access to the most recently sent message or query.
    /// This property is updated each time a request is successfully made within the session.
    /// </summary>
    /// <remarks>
    /// The value of this property is `null` if no request has been made or if the session
    /// was reset by assigning a new history to the <see cref="History"/> property.
    /// </remarks>
    public Content? LastRequestContent { get; private set; }

    /// <summary>
    /// Gets the content of the last response received in the current chat session,
    /// providing access to the most recently generated reply or output from the model.
    /// This property is updated each time a response is successfully received within the session.
    /// </summary>
    /// <remarks>
    /// The value of this property is `null` if no response has been received or if the session
    /// was reset by assigning a new history to the <see cref="History"/> property.
    /// </remarks>
    public Content? LastResponseContent { get; private set; }

    /// <summary>
    /// Gets or sets the history of the current chat session, representing a collection of
    /// user and model interactions recorded throughout the session.
    /// Updates to this property will reset both <see cref="LastRequestContent"/> and <see cref="LastResponseContent"/>.
    /// </summary>
    /// <remarks>
    /// Assigning a new value to this property effectively resets the session, clearing any previously tracked
    /// request or response content. The history contains instances of <see cref="Content"/> that represent
    /// messages exchanged between the user and the model.
    /// </remarks>
    public List<Content> History
    {
        get => this._history;
        set
        {
            this._history = value;
            this.LastRequestContent = null;
            this.LastResponseContent = null;
        }
    }

    #endregion


    /// <summary>
    /// Asynchronously generates an answer for the specified user query using the semantic retriever model.
    /// Updates the session history with the request and response contents.
    /// </summary>
    /// <param name="query">The user's query for which an answer is to be generated.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="GenerateAnswerResponse"/> containing the model's response to the query.</returns>
    public async Task<GenerateAnswerResponse> GenerateAnswerAsync(string query,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateAnswerRequest();
        var content = new Content(query, Roles.User);
        if (this.History.Count > 0)
            request.Contents.AddRange(this.History);
        request.AddContent(content);

        request.SemanticRetriever = new SemanticRetrieverConfig()
        {
            Source = _corpusId,
            Query = content
        };

        request.AnswerStyle = _answerStyle;
        request.SafetySettings = _safetySettings;
        var response = await _model.GenerateAnswerAsync(request, cancellationToken);
        UpdateHistory(request, response);
        return response;
    }

    /// <summary>
    /// Updates the session history with the most recent request and response contents.
    /// </summary>
    /// <param name="request">The request object containing the user's query content.</param>
    /// <param name="response">The response object containing the model's generated answer.</param>
    private void UpdateHistory(GenerateAnswerRequest request, GenerateAnswerResponse response)
    {
        if (response.Answer != null)
        {
            var lastRequestContent = request.Contents.Last();
            var lastResponseContent = response.Answer.Content;
            if (lastResponseContent != null)
            {
                UpdateHistory(lastRequestContent, lastResponseContent);
            }
        }
    }

    /// <summary>
    /// Internal helper method to add a new request and response pair to the session history.
    /// Assigns roles for both request and response before updating the history.
    /// </summary>
    /// <param name="lastRequestContent">The content of the user's most recent request.</param>
    /// <param name="lastResponseContent">The content of the model's most recent response.</param>
    private void UpdateHistory(Content lastRequestContent, Content lastResponseContent)
    {
        lastRequestContent.Role ??= Roles.User;
        lastResponseContent.Role ??= Roles.Model;
        History.Add(lastRequestContent);
        History.Add(lastResponseContent);
        this.LastRequestContent = lastRequestContent;
        this.LastResponseContent = lastResponseContent;
    }
}