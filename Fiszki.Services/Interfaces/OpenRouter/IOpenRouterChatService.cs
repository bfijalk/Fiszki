using Fiszki.Services.Models.OpenRouter;

namespace Fiszki.Services.Interfaces.OpenRouter;

/// <summary>
/// Simplified interface for OpenRouter chat completion service (MVP version)
/// </summary>
public interface IOpenRouterChatService
{
    /// <summary>
    /// Performs a non-streaming chat completion
    /// </summary>
    Task<ChatCompletionResult> CompleteAsync(
        ChatSession session, 
        ChatCompletionOptions? options = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a structured chat completion with JSON schema validation
    /// </summary>
    Task<StructuredResult<T>> CompleteStructuredAsync<T>(
        ChatSession session, 
        JsonSchemaDescriptor schema, 
        ChatCompletionOptions? options = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new chat session for the specified model
    /// </summary>
    ChatSession CreateSession(string model);

    /// <summary>
    /// Sets the system message for a chat session
    /// </summary>
    void SetSystemMessage(ChatSession session, string content);

    /// <summary>
    /// Adds a user message to the chat session
    /// </summary>
    void AddUserMessage(ChatSession session, string content, MessageMetadata? metadata = null);

    /// <summary>
    /// Adds an assistant message to the chat session
    /// </summary>
    void AddAssistantMessage(ChatSession session, string content, MessageMetadata? metadata = null);

    /// <summary>
    /// Creates chat completion options using a builder pattern
    /// </summary>
    ChatCompletionOptions CreateOptions(Action<ChatCompletionOptionsBuilder> build);
}
