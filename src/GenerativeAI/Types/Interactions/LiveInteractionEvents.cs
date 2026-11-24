using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using GenerativeAI.Types.Converters;

namespace GenerativeAI.Types;

// ---------------------------------------------------------------------------------------------------------------
// Server -> client events of live (bidirectional) interactions. They share the InteractionSseEvent base and its
// converter with the streaming (SSE) events: step.start / step.delta / step.stop / error are the same types, and
// interaction.created / interaction.completed are read as the live variants when they carry no "interaction" object.
// ---------------------------------------------------------------------------------------------------------------

/// <summary>
/// The server's response when a client (re)connects to a live interaction (<c>interaction.connected</c>).
/// </summary>
public sealed class InteractionConnectedEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionConnectedEvent"/> class.</summary>
    public InteractionConnectedEvent() : base(InteractionEventTypes.InteractionConnected)
    {
    }
}

/// <summary>
/// The live interaction was cancelled, e.g. in response to a <see cref="CancelInteractionEvent"/>
/// (<c>interaction.cancelled</c>). Work in flight was aborted and no more input can be sent.
/// </summary>
public sealed class InteractionCancelledEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionCancelledEvent"/> class.</summary>
    public InteractionCancelledEvent() : base(InteractionEventTypes.InteractionCancelled)
    {
    }

    /// <summary>Statistics on the interaction's token usage.</summary>
    [JsonPropertyName("usage")]
    public InteractionUsage? Usage { get; set; }
}

/// <summary>
/// The server confirms that a new live interaction was created (<c>interaction.created</c> without an embedded
/// <c>interaction</c> object; the streaming variant is <see cref="InteractionCreatedEvent"/>).
/// </summary>
public sealed class LiveInteractionCreatedEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="LiveInteractionCreatedEvent"/> class.</summary>
    public LiveInteractionCreatedEvent() : base(InteractionEventTypes.InteractionCreated)
    {
    }

    /// <summary>The ID of the created interaction.</summary>
    [JsonPropertyName("interaction_id")]
    public string? InteractionId { get; set; }
}

/// <summary>
/// The live interaction completed (<c>interaction.completed</c> without an embedded <c>interaction</c> object; the
/// streaming variant is <see cref="InteractionCompletedEvent"/>). Sent when the interaction receives Complete/Cancel
/// or terminates naturally; no more input can be sent afterwards.
/// </summary>
public sealed class LiveInteractionCompletedEvent : InteractionSseEvent
{
    /// <summary>Initializes a new instance of the <see cref="LiveInteractionCompletedEvent"/> class.</summary>
    public LiveInteractionCompletedEvent() : base(InteractionEventTypes.InteractionCompleted)
    {
    }

    /// <summary>Creation time in ISO 8601 format.</summary>
    [JsonPropertyName("created")]
    public string? Created { get; set; }

    /// <summary>Last update time in ISO 8601 format.</summary>
    [JsonPropertyName("updated")]
    public string? Updated { get; set; }

    /// <summary>The final status of the interaction, as a <c>google.rpc.Status</c> (set when it ended with an error).</summary>
    [JsonPropertyName("status")]
    public InteractionRpcStatus? Status { get; set; }

    /// <summary>Statistics on the interaction's token usage.</summary>
    [JsonPropertyName("usage")]
    public InteractionUsage? Usage { get; set; }
}

// ---------------------------------------------------------------------------------------------------------------
// Client -> server events of live interactions.
// ---------------------------------------------------------------------------------------------------------------

/// <summary>
/// A client event of a live interaction, discriminated by <c>event_type</c> (python-genai <c>ClientEvent</c>).
/// Unknown event types are read as <see cref="UnknownInteractionClientEvent"/>.
/// </summary>
[JsonConverter(typeof(InteractionClientEventConverter))]
public abstract class InteractionClientEvent
{
    /// <summary>
    /// Initializes a new client event with the given discriminator.
    /// </summary>
    /// <param name="eventType">The <c>event_type</c> discriminator.</param>
    protected InteractionClientEvent(string eventType)
    {
        EventType = eventType;
    }

    /// <summary>The event type (see <see cref="InteractionClientEventTypes"/>).</summary>
    [JsonPropertyName("event_type")]
    public string EventType { get; }

    /// <summary>Additional fields to send (for API features this SDK does not model yet).</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>Starts a new live interaction (<c>interaction.create</c>).</summary>
public sealed class CreateInteractionEvent : InteractionClientEvent
{
    /// <summary>Initializes a new instance of the <see cref="CreateInteractionEvent"/> class.</summary>
    public CreateInteractionEvent() : base(InteractionClientEventTypes.InteractionCreate)
    {
    }

    /// <summary>The model used for generating the completion (see <see cref="InteractionModels"/>).</summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// The ID of a previous interaction whose output is prepended to the input. That interaction must have been run
    /// with <c>store=true</c> and must be in a terminal state.
    /// </summary>
    [JsonPropertyName("previous_interaction_id")]
    public string? PreviousInteractionId { get; set; }

    /// <summary>
    /// Whether to store the interaction so it can be retrieved later (e.g. as a <see cref="PreviousInteractionId"/>).
    /// This does not affect the storage needed to run the interaction itself (reconnects, failure recovery).
    /// </summary>
    [JsonPropertyName("store")]
    public bool? Store { get; set; }

    /// <summary>System instruction for the interaction.</summary>
    [JsonPropertyName("system_instruction")]
    public string? SystemInstruction { get; set; }

    /// <summary>Tool declarations the model may call during the interaction.</summary>
    [JsonPropertyName("tools")]
    public List<InteractionTool>? Tools { get; set; }
}

/// <summary>
/// Reconnects to an existing live interaction (<c>interaction.connect</c>). The server answers with an
/// <see cref="InteractionConnectedEvent"/>; if the interaction is already complete, it replays the remaining events.
/// </summary>
public sealed class ConnectInteractionEvent : InteractionClientEvent
{
    /// <summary>Initializes a new instance of the <see cref="ConnectInteractionEvent"/> class.</summary>
    public ConnectInteractionEvent() : base(InteractionClientEventTypes.InteractionConnect)
    {
    }

    /// <summary>The ID of the interaction to connect to.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// When true, the client intends to disconnect after sending this message; the interaction keeps running in the
    /// background and the client can reconnect later to retrieve further output.
    /// </summary>
    [JsonPropertyName("background")]
    public bool? Background { get; set; }
}

/// <summary>
/// Permanently ends the live interaction, aborting any work in flight (<c>interaction.cancel</c>). Reconnecting
/// afterwards is supported to read the final status, but no further input may be sent.
/// </summary>
public sealed class CancelInteractionEvent : InteractionClientEvent
{
    /// <summary>Initializes a new instance of the <see cref="CancelInteractionEvent"/> class.</summary>
    public CancelInteractionEvent() : base(InteractionClientEventTypes.InteractionCancel)
    {
    }
}

/// <summary>
/// Ends the live interaction like a half-close (<c>interaction.complete</c>): the client sends no new input but waits
/// for the output to finish, and may reconnect.
/// </summary>
public sealed class CompleteInteractionEvent : InteractionClientEvent
{
    /// <summary>Initializes a new instance of the <see cref="CompleteInteractionEvent"/> class.</summary>
    public CompleteInteractionEvent() : base(InteractionClientEventTypes.InteractionComplete)
    {
    }
}

/// <summary>
/// Requests a graceful disconnection without ending the live interaction (<c>interaction.disconnect</c>).
/// python-genai: <c>DisconnectEvent</c>.
/// </summary>
public sealed class InteractionDisconnectEvent : InteractionClientEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionDisconnectEvent"/> class.</summary>
    public InteractionDisconnectEvent() : base(InteractionClientEventTypes.InteractionDisconnect)
    {
    }
}

/// <summary>
/// An atomic input payload of a live interaction (<c>input</c>); it replaces the <c>user_input</c> step on the
/// bidirectional wire. python-genai: <c>InputEvent</c>.
/// </summary>
public sealed class InteractionInputEvent : InteractionClientEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionInputEvent"/> class.</summary>
    public InteractionInputEvent() : base(InteractionClientEventTypes.Input)
    {
    }

    /// <summary>The content blocks making up the input (text, images, documents, ...).</summary>
    [JsonPropertyName("content")]
    public List<InteractionContent>? Content { get; set; }

    /// <summary>
    /// Creates an input event with a single text block.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The input event.</returns>
    public static InteractionInputEvent FromText(string text) =>
        new() { Content = new List<InteractionContent> { InteractionContent.FromText(text) } };
}

/// <summary>
/// An interactive user action (keyboard keys, mouse movements/clicks) for world simulation and interactive media
/// (<c>user_action</c>). python-genai: <c>UserActionInput</c>.
/// </summary>
public sealed class InteractionUserActionInput : InteractionClientEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionUserActionInput"/> class.</summary>
    public InteractionUserActionInput() : base(InteractionClientEventTypes.UserAction)
    {
    }

    /// <summary>The type of action performed by the client (see <see cref="InteractionUserActionTypes"/>).</summary>
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    /// <summary>The input associated with the action (see <see cref="InteractionUserActionKeys"/>).</summary>
    [JsonPropertyName("input")]
    public string? Input { get; set; }
}

/// <summary>Text prompt describing the world to simulate. python-genai: <c>WorldPromptContent</c>.</summary>
public sealed class InteractionWorldPromptContent
{
    /// <summary>Required. The text of the world prompt.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>Reference image for the initial world frame. python-genai: <c>WorldReferenceImage</c>.</summary>
public sealed class InteractionWorldReferenceImage
{
    /// <summary>Base64-encoded image bytes.</summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>The MIME type of the image (e.g. <c>image/jpeg</c>, <c>image/png</c>).</summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    /// <summary>The URI of the image.</summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
}

/// <summary>
/// The <c>world_prompt</c> of a <see cref="InteractionWorldSetupEvent"/> (python-genai <c>WorldPrompt</c>): either a
/// plain <see cref="Text"/> string or an <see cref="InteractionWorldPromptContent"/> object.
/// </summary>
[JsonConverter(typeof(InteractionWorldPromptConverter))]
public sealed class InteractionWorldPrompt
{
    /// <summary>Creates a world prompt from text (sent as a string).</summary>
    /// <param name="text">The prompt text.</param>
    public InteractionWorldPrompt(string text)
    {
        Text = text;
    }

    /// <summary>Creates a world prompt from a content object.</summary>
    /// <param name="content">The prompt content.</param>
    public InteractionWorldPrompt(InteractionWorldPromptContent content)
    {
        Content = content;
    }

    /// <summary>The prompt text, when the prompt is given as a string.</summary>
    public string? Text { get; }

    /// <summary>The prompt content, when the prompt is given as an object.</summary>
    public InteractionWorldPromptContent? Content { get; }

    /// <summary>Converts text to a world prompt.</summary>
    /// <param name="text">The prompt text.</param>
    public static implicit operator InteractionWorldPrompt(string text) => new(text);

    /// <summary>Converts a content object to a world prompt.</summary>
    /// <param name="content">The prompt content.</param>
    public static implicit operator InteractionWorldPrompt(InteractionWorldPromptContent content) => new(content);

    /// <summary>Creates a world prompt from text.</summary>
    /// <param name="text">The prompt text.</param>
    /// <returns>The world prompt.</returns>
    public static InteractionWorldPrompt FromString(string text) => new(text);

    /// <summary>Creates a world prompt from a content object.</summary>
    /// <param name="content">The prompt content.</param>
    /// <returns>The world prompt.</returns>
    public static InteractionWorldPrompt FromInteractionWorldPromptContent(InteractionWorldPromptContent content) =>
        new(content);
}

/// <summary>
/// World setup for world-simulation models (e.g. Genie models), sent by the client after <c>interaction.create</c> to
/// initialize the world state and start generation (<c>world_setup</c>). python-genai: <c>WorldSetupEvent</c>.
/// </summary>
public sealed class InteractionWorldSetupEvent : InteractionClientEvent
{
    /// <summary>Initializes a new instance of the <see cref="InteractionWorldSetupEvent"/> class.</summary>
    public InteractionWorldSetupEvent() : base(InteractionClientEventTypes.WorldSetup)
    {
    }

    /// <summary>Reference image for the initial world frame.</summary>
    [JsonPropertyName("reference_image")]
    public InteractionWorldReferenceImage? ReferenceImage { get; set; }

    /// <summary>Text prompt describing the world to simulate.</summary>
    [JsonPropertyName("world_prompt")]
    public InteractionWorldPrompt? WorldPrompt { get; set; }
}

/// <summary>A client event whose <c>event_type</c> this SDK does not know.</summary>
public sealed class UnknownInteractionClientEvent : InteractionClientEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnknownInteractionClientEvent"/> class.
    /// </summary>
    /// <param name="eventType">The discriminator value, if any.</param>
    /// <param name="raw">The raw JSON.</param>
    public UnknownInteractionClientEvent(string? eventType, JsonNode raw) : base(eventType ?? string.Empty)
    {
        Raw = raw;
    }

    /// <summary>The raw JSON of the event.</summary>
    [JsonIgnore]
    public JsonNode Raw { get; }
}

/// <summary>A list of client events (<c>{"events": [...]}</c>). python-genai: <c>ClientEventList</c>.</summary>
public sealed class InteractionClientEventList
{
    /// <summary>The client events.</summary>
    [JsonPropertyName("events")]
    public List<InteractionClientEvent>? Events { get; set; }
}

/// <summary>
/// A top-level client-to-server wire message of a live interaction (python-genai <c>ClientEventBatch</c>): either a
/// single <see cref="Event"/> or an <see cref="EventList"/>.
/// </summary>
[JsonConverter(typeof(InteractionClientEventBatchConverter))]
public sealed class InteractionClientEventBatch
{
    /// <summary>Creates a message carrying a single event.</summary>
    /// <param name="clientEvent">The event.</param>
    public InteractionClientEventBatch(InteractionClientEvent clientEvent)
    {
        Event = clientEvent;
    }

    /// <summary>Creates a message carrying a list of events.</summary>
    /// <param name="list">The event list.</param>
    public InteractionClientEventBatch(InteractionClientEventList list)
    {
        EventList = list;
    }

    /// <summary>The event, when the message carries a single event.</summary>
    public InteractionClientEvent? Event { get; }

    /// <summary>The event list, when the message carries several events.</summary>
    public InteractionClientEventList? EventList { get; }

    /// <summary>Converts a single event to a message.</summary>
    /// <param name="clientEvent">The event.</param>
    public static implicit operator InteractionClientEventBatch(InteractionClientEvent clientEvent) => new(clientEvent);

    /// <summary>Converts an event list to a message.</summary>
    /// <param name="list">The event list.</param>
    public static implicit operator InteractionClientEventBatch(InteractionClientEventList list) => new(list);

    /// <summary>Creates a message carrying a single event.</summary>
    /// <param name="clientEvent">The event.</param>
    /// <returns>The message.</returns>
    public static InteractionClientEventBatch FromInteractionClientEvent(InteractionClientEvent clientEvent) =>
        new(clientEvent);

    /// <summary>Creates a message carrying an event list.</summary>
    /// <param name="list">The event list.</param>
    /// <returns>The message.</returns>
    public static InteractionClientEventBatch FromInteractionClientEventList(InteractionClientEventList list) =>
        new(list);

    /// <summary>Creates a message carrying several events.</summary>
    /// <param name="events">The events.</param>
    /// <returns>The message.</returns>
    public static InteractionClientEventBatch FromEvents(IEnumerable<InteractionClientEvent> events) =>
        new(new InteractionClientEventList
        {
            Events = events?.ToList() ?? throw new ArgumentNullException(nameof(events))
        });
}

// ---------------------------------------------------------------------------------------------------------------
// Ephemeral auth tokens for live interactions.
// ---------------------------------------------------------------------------------------------------------------

/// <summary>Parameters for creating an ephemeral interaction auth token.</summary>
public sealed class InteractionAuthTokenCreateParams
{
    /// <summary>Hard deadline: messages are rejected after this time.</summary>
    [JsonPropertyName("expire_time")]
    public DateTimeOffset? ExpireTime { get; set; }

    /// <summary>Additional body fields to send (for API features this SDK does not model yet).</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}

/// <summary>An ephemeral auth token for live interactions.</summary>
public sealed class InteractionAuthToken
{
    /// <summary>The auth token.</summary>
    [JsonPropertyName("auth_token")]
    public string? AuthToken { get; set; }

    /// <summary>Expiration time of the auth token.</summary>
    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>Fields returned by the API that this SDK does not model yet.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
