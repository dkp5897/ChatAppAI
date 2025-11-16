
using ChatAppAI;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;

namespace ChatAppAi;

public class Program
{ 
    public static async Task Main()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        var apiKey = config["GoogleAPIKey1"] ?? string.Empty;
        var modelName = config["GoogleAIModal"] ?? "gemini-2.0-flash";
        if (string.IsNullOrWhiteSpace(apiKey))
        {
          Console.WriteLine("API Key is missing. Please set it in User Secrets.");
          return;
        }

        var client = new Client(apiKey: apiKey);
        string systemPrompt = "You are a helpful assistant in an interactive console app. Keep answers concise unless asked for more.";

        Console.WriteLine("Welcome to Gemini Chat! Type your question, or 'exit' to quit. Type '/help' for commands.");
        Console.WriteLine("Your name please: ");
        string? userName = null;
        while (true)
        {
          userName = Console.ReadLine();
          if (!string.IsNullOrWhiteSpace(userName))
          {
            Console.WriteLine($"Hello, {userName}! please ask your question.");
            break;
          }
          Console.WriteLine("Your name please: ");
        }

        var chat = new ChatService(client, modelName, systemPrompt, userName);
        while (true)
        {
          Console.ForegroundColor = ConsoleColor.Cyan;
          Console.Write($"{userName}: ");
          var userInput = Console.ReadLine();
          Console.ResetColor();
          if (string.IsNullOrWhiteSpace(userInput))
          {
            Console.WriteLine("Please enter a question or command.");
            continue;
          }
          var trimmedInput = userInput.Trim();

          if (trimmedInput.Equals("exit", StringComparison.OrdinalIgnoreCase) || trimmedInput.Equals("/quit", StringComparison.OrdinalIgnoreCase))
            break;

          if (trimmedInput.Equals("/help", StringComparison.OrdinalIgnoreCase))
          {
            Console.WriteLine("Commands:");
            Console.WriteLine("/help - show help");
            Console.WriteLine("/clear - clear conversation history");
            Console.WriteLine("/history - print conversation history");
            Console.WriteLine("/exit or /quit - exit");
            continue;
          }

          if (trimmedInput.Equals("/clear", StringComparison.OrdinalIgnoreCase))
          {
            chat.ClearHistory();
            Console.WriteLine("Conversation history cleared.");
            continue;
          }

          if (trimmedInput.Equals("/history", StringComparison.OrdinalIgnoreCase))
          {
            var history = chat.History;
            if (history.Count == 0)
            {
              Console.WriteLine("No conversation history.");
            }
            else
            {
              Console.WriteLine("Conversation History:");
              foreach (var message in history)
              {
                //skip system prompt in history display
                if (message.Role != "system")
                  Console.WriteLine($"{message?.Role?.ToUpper()}: {message?.Content}\n");
              }
              continue;
            }
          }

          //allow user to cancel the request with Ctrl+C
          using var cts = new CancellationTokenSource(60 * 1000); //60 seconds timeout

          try
          {
            var response = await chat.SendAsync(userInput, cts.Token);
            //AI response colur 
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"DK-AI: {response}");
            Console.ResetColor();

          }
          catch (OperationCanceledException)
          {
            Console.WriteLine("Request was cancelled or failed. Please try again.");
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Error: {ex.Message}");
          }

        }
        Console.WriteLine("Goodbye!");
    }
}