// See https://aka.ms/new-console-template for more information

using OpenAI;
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

Console.WriteLine($"Prompting model with {messages[1].Contents[0]}");

var completion = await chatClient.GetResponseAsync(messages, options);

Console.WriteLine(completion.Messages[2].Contents[0]);

public class WeatherService
{
    [Description("Get the current weather for a given city")]
    public static string GetCurrentWeather([Description("The city to get the weather for")] string city)
    {
        return $"The current weather in {city} is sunny with a temperature of 75°F.";
    }
}



