// See https://aka.ms/new-console-template for more information

using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using Microsoft.AI.Foundry.Local;

var alias = "deepseek-r1-distill-qwen-7b-generic-gpu:3";

var manager = await FoundryLocalManager.StartModelAsync(aliasOrModelId: alias);

var model = await manager.GetModelInfoAsync(aliasOrModelId: alias);
ApiKeyCredential key = new ApiKeyCredential(manager.ApiKey);
OpenAIClient client = new OpenAIClient(key, new OpenAIClientOptions
{
    Endpoint = manager.Endpoint
});

var chatClient = client.GetChatClient(model?.ModelId);

var messages = new ChatMessage[]
{
    ChatMessage.CreateSystemMessage("You are help desk assistant with some tools. Output the tool calls only in response to the user prompt"),
    ChatMessage.CreateUserMessage("'I'd like to order 10 'Clean Code' books' to 666-111-222")
};

var tool = SmsService.GetTool();

ChatCompletionOptions options = new()
{
   Tools = { tool },
   MaxOutputTokenCount = 2048
};

var completionUpdates = chatClient.CompleteChatStreaming(messages, options);

Console.Write($"[ASSISTANT]: ");
foreach (var completionUpdate in completionUpdates)
{
    if (completionUpdate.ContentUpdate.Count > 0)
    {
        Console.Write(completionUpdate.ContentUpdate[0].Text);
    }
}

public class SmsService
{
    static public ChatTool GetTool()
    {
        ChatTool tool = ChatTool.CreateFunctionTool(
            nameof(SendSms),
            "send SMS",
            BinaryData.FromString(
                @"
            {
              ""type"": ""object"",
              ""properties"": {
                ""message"": {
                  ""type"": ""string"",
                  ""description"": ""text of SMS""
                },
                ""phoneNumber"": {
                  ""type"": ""string"",
                  ""description"": ""phone number of recipient""
                }
              },
              ""required"": [""message"", ""phoneNumber""]
            }
        ")
        );

        return tool;
    }

    public static string SendSms(string message, string phoneNumber)
    {
        return "SMS sent!";
    }
}



