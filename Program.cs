// See https://aka.ms/new-console-template for more information

using OpenAI;
using System.Text.Json;
using System.Text;
using System.ClientModel;
using System.ComponentModel;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

var alias = "qwen2.5-0.5b";

Console.WriteLine("Starting model...");

var manager = await FoundryLocalManager.StartModelAsync(aliasOrModelId: alias);

var model = await manager.GetModelInfoAsync(aliasOrModelId: alias);
ApiKeyCredential key = new ApiKeyCredential(manager.ApiKey);
OpenAIClient client = new OpenAIClient(key, new OpenAIClientOptions
{
    Endpoint = manager.Endpoint
});

var chatClient = client.GetChatClient(model?.ModelId).AsIChatClient().AsBuilder().UseFunctionInvocation().Build();

IList<AITool> tools = [
    AIFunctionFactory.Create(StringService.Reverse),
    AIFunctionFactory.Create(SmsService.SendSms),
    AIFunctionFactory.Create(HoroscopeService.GetHoroscope),
    AIFunctionFactory.Create(HoroscopeService.GetSun),
    AIFunctionFactory.Create(HoroscopeService.GetMoon)];

var messages = new ChatMessage[]
{
    new ChatMessage(ChatRole.System, "You are a helpful assistant with some tools."),
    new ChatMessage(ChatRole.User, "What is my sun? I am a Scorpio.")
};


ChatOptions options = new()
{
    Tools = tools,
    ToolMode = ChatToolMode.Auto,
    MaxOutputTokens = 2048
};


var completion = await chatClient.GetResponseAsync(messages, options);

// Print detailed information about each message
Console.WriteLine("=== CHAT COMPLETION MESSAGES ===");
for (int i = 0; i < completion.Messages.Count; i++)
{
    var message = completion.Messages[i];
    Console.WriteLine($"\n--- Message {i + 1} ---");
    Console.WriteLine($"Role: {message.Role}");
    Console.WriteLine($"Message ID: {message.MessageId}");
    Console.WriteLine($"Created At: {message.CreatedAt}");
    Console.WriteLine($"Contents Count: {message.Contents?.Count ?? 0}");
    
    if (message.Contents != null)
    {
        for (int j = 0; j < message.Contents.Count; j++)
        {
            var content = message.Contents[j];
            Console.WriteLine($"  Content {j + 1}:");
            Console.WriteLine($"    Type: {content.GetType().Name}");
            
            // Handle different content types
            switch (content)
            {
                case TextContent textContent:
                    Console.WriteLine($"    Text: {textContent.Text}");
                    break;
                case FunctionCallContent funcCall:
                    Console.WriteLine($"    Function Call: {funcCall.Name}");
                    Console.WriteLine($"    Call ID: {funcCall.CallId}");
                    Console.WriteLine($"    Arguments: {JsonSerializer.Serialize(funcCall.Arguments)}");
                    break;
                case FunctionResultContent funcResult:
                    Console.WriteLine($"    Function Result:");
                    Console.WriteLine($"    Call ID: {funcResult.CallId}");
                    Console.WriteLine($"    Result: {funcResult.Result}");
                    break;
                default:
                    Console.WriteLine($"    Content: {content}");
                    break;
            }
        }
    }
}

Console.WriteLine($"\n=== COMPLETION SUMMARY ===");
Console.WriteLine($"Response ID: {completion.ResponseId}");
Console.WriteLine($"Model ID: {completion.ModelId}");
Console.WriteLine($"Created At: {completion.CreatedAt}");
Console.WriteLine($"Finish Reason: {completion.FinishReason}");

// This section formats and writes the output to a JSON file. It is for clarity and debugging purposes.
//Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText("output.json", JsonSerializer.Serialize(completion, new JsonSerializerOptions { WriteIndented = true }));


public class SmsService
{
    [Description("Given a phone number and a message send an SMS")]
    public static string SendSms(string message, string phoneNumber)
    {
        return "SMS sent!";
    }
}

public class StringService
{
    [Description("Given a string, return the reverse of that string")]
    public static string Reverse(string input)
    {
        return "String reversed";
    }
}

public class HoroscopeService
{
    [Description("Get a horoscope reading for a zodiac sign")]
    public static string GetHoroscope(string sign)
    {
        return $"{sign}: Next Tuesday you will befriend a baby otter.";
    }

    [Description("Get sun information for a zodiac sign")]
    public static string GetSun(string sign)
    {
        return $"{sign}: The sun is shining bright today.";
    }

    [Description("Get moon information for a zodiac sign")]
    public static string GetMoon(string sign)
    {
        return $"{sign}: The moon is full tonight.";
    }
}



