// See https://aka.ms/new-console-template for more information

using OpenAI;
using System.ClientModel;
using System.ComponentModel;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using System.Text.Json;

var alias = "qwen2.5-7b-instruct-generic-gpu";

var manager = await FoundryLocalManager.StartModelAsync(aliasOrModelId: alias);

var model = await manager.GetModelInfoAsync(aliasOrModelId: alias);
ApiKeyCredential key = new ApiKeyCredential(manager.ApiKey);
OpenAIClient client = new OpenAIClient(key, new OpenAIClientOptions
{
    Endpoint = manager.Endpoint
});

var chatClient = client.GetChatClient(model?.ModelId).AsIChatClient();

IList<AITool> tools = [AIFunctionFactory.Create(StringService.Reverse), AIFunctionFactory.Create(SmsService.SendSms)];

var messages = new ChatMessage[]
{
    new ChatMessage(ChatRole.System, "You are help assistant with some tools."),
    new ChatMessage(ChatRole.User, "Reverse the string 'Hello World'.")
};


ChatOptions options = new()
{
    Tools = tools,
    MaxOutputTokens = 2048
};


Console.WriteLine(JsonSerializer.Serialize(messages));
Console.WriteLine(JsonSerializer.Serialize(options));


var completionUpdates = chatClient.GetStreamingResponseAsync(messages, options);

Console.Write($"[ASSISTANT]: ");
await foreach (var completionUpdate in completionUpdates)
{
    Console.Write(completionUpdate.Text);
}

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



