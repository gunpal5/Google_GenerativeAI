namespace GenerativeAI.Types;

/// <summary>
/// Represents a request containing a collection of contents.
/// </summary>
/// <remarks>
/// This interface defines a property for managing multiple instances of <see cref="Content"/>.
/// Implementations of this interface can be used to interact with and manipulate content-related requests.
/// </remarks>
public interface IContentsRequest
{
     /// <summary>
     /// Represents a collection of content items within an object that implements the IContentsRequest interface.
     /// </summary>
     /// <remarks>
     /// The <c>Contents</c> property provides access to a list of <see cref="Content"/> objects.
     /// Each <see cref="Content"/> represents multi-part content with a specified role and data parts.
     /// </remarks>
     /// <example>
     /// This property can be used to store and manipulate a collection of content items,
     /// such as messages or data associated with requests or responses.
     /// </example>
     List<Content> Contents { get; set; }
}