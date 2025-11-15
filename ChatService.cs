using System;
using Google.GenAI;
using Polly;
using Polly.Retry;

namespace ChatAppAI;

public record Message(string? Role, string Content);
public class ChatService (Client client, string model, string SystemPrompt, string? userName = "user")
{
    private readonly Client _client = client;
    private readonly string _model = model ?? "gemini-2.0-flash";
    private readonly string _systemPrompt = SystemPrompt ?? "You are a helpful assistant.";
    private readonly List<Message> _conversationHistory = [];

    // Polly retry: retry 3 times with exponential backoff for transient exceptions
    
    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    public IReadOnlyList<Message> History => _conversationHistory.AsReadOnly();

    // add system prompt and user message to history
    public void AddSystemPromptIfMissing()
    {
        if(_conversationHistory.Count == 0)
        {
            _conversationHistory.Add(new Message("system", _systemPrompt));
        }
    }

    public async Task<string> SendAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(userMessage))
            return string.Empty;
        
        AddSystemPromptIfMissing();
        _conversationHistory.Add(new Message(userName, userMessage));

        var formattedMessages = FormatMessages();

        string responseContent = string.Empty;

        //use Polly retry policy to send request

        await _retryPolicy.ExecuteAsync(async (ct) =>
        {
            ct.ThrowIfCancellationRequested();
            var response = await _client.Models.GenerateContentAsync(
                model: _model,
                contents: formattedMessages
            );

            responseContent = response?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? string.Empty;
            return Task.CompletedTask;
        }, cancellationToken);

        if(!string.IsNullOrWhiteSpace(responseContent))
        {
            _conversationHistory.Add(new Message("assistant", responseContent));
            return responseContent;
        }

        return "Sorry, I couldn't generate a response.";

    }

    //helper methods

    private string FormatMessages()
    {
        var formatted = string.Join("\n", _conversationHistory.Select(m => $"{m.Role}: {m.Content}"));
        return formatted;
    }

    public void ClearHistory()
    {
        _conversationHistory.Clear();
    }


}
