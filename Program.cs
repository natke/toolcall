// See https://aka.ms/new-console-template for more information

using OpenAI;
using System.Text.Json;
using System.Text;
using System.ClientModel;
using System.ComponentModel;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

var alias = "qwen2.5-coder-7b-instruct-generic-gpu:3";

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

Console.WriteLine(JsonSerializer.Serialize(completion));

// This section formats and writes the output to a JSON file. It is for clarity and debugging purposes.
// It prints the conversation output to a file named output.json. This section can be omitted if not needed.
var modelOutput = new StringBuilder();
foreach (var m in completion.Messages)
{
    var msg = new { MessageRole = m.Role, Content = m.Contents?.First() };
    modelOutput.Append($"{JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true })}");
};

File.WriteAllText("output.json", modelOutput.ToString());


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



