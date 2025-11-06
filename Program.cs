// See https://aka.ms/new-console-template for more information

using OpenAI;
using System.Text.Json;
using System.ClientModel;
using System.ComponentModel;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

var alias = "qwen2.5-7b";

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
    AIFunctionFactory.Create(WeatherService.GetCurrentWeather)];

var messages = new ChatMessage[]
{
    new ChatMessage(ChatRole.System, "You are a helpful assistant with some tools."),
    new ChatMessage(ChatRole.User, "How is it in Sydney?")
};


ChatOptions options = new()
{
    Tools = tools,
    ToolMode = ChatToolMode.Auto,
    MaxOutputTokens = 2048
};

// This section formats and writes the output to a JSON file. It is for clarity and debugging purposes.
//Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText("messages.json", JsonSerializer.Serialize(messages, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText("tools.json", JsonSerializer.Serialize(options.Tools, new JsonSerializerOptions { WriteIndented = true }));

var completion = await chatClient.GetResponseAsync(messages, options);

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

public class WeatherService
{
    [Description("Get the current weather for a given city")]
    public static string GetCurrentWeather([Description("The city to get the weather for")] string city)
    {
        return $"The current weather in {city} is sunny with a temperature of 75°F.";
    }
}



