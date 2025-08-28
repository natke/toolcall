// See https://aka.ms/new-console-template for more information

using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using Microsoft.AI.Foundry.Local;


var alias = "deepseek-r1-distill-qwen-7b-generic-gpu:3";

var manager = await FoundryLocalManager.StartModelAsync(aliasOrModelId: alias);

var model = await manager.GetModelInfoAsync(aliasOrModelId: alias);
ApiKeyCredential key = new ApiKeyCredential(manager.ApiKey);
OpenAIClient client = new OpenAIClient(key, new OpenAIClientOptions
{
    Endpoint = manager.Endpoint
});

var chatClient = client.GetChatClient(model?.ModelId).AsIChatClient();

var messages = new ChatMessage[]
{
    new ChatMessage(ChatRole.System, "You are help desk assistant with some tools. Output the tool calls only in response to the user prompt"),
    new ChatMessage(ChatRole.User, "'I'd like to order 10 'Clean Code' books' to 666-111-222")
};

ChatOptions options = new()
{
   Tools = [ AIFunctionFactory.Create(SmsService.SendSms) ],
   MaxOutputTokens = 2048
};

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



