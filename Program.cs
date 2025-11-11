// See https://aka.ms/new-console-template for more information

using OpenAI;
using System.ClientModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

// Parse command line arguments for model alias
var alias = args.Length > 0 ? args[0] : "qwen2.5-0.5b";

// Display usage information if help is requested
if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h"))
{
    Console.WriteLine("Usage: toolcall [model-alias]");
    Console.WriteLine("  model-alias: The model alias to use (default: qwen2.5-0.5b)");
    Console.WriteLine("  Examples:");
    Console.WriteLine("    toolcall");
    Console.WriteLine("    toolcall qwen2.5-14b");
    Console.WriteLine("    toolcall qwen2.5-0.5b-instruct-generic-cpu:3");
    return;
}

Console.WriteLine($"Starting model: {alias}...");

// Create service collection and configure logging
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
});

// Build service provider
var serviceProvider = services.BuildServiceProvider();
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

var manager = await FoundryLocalManager.StartModelAsync(aliasOrModelId: alias);

var model = await manager.GetModelInfoAsync(aliasOrModelId: alias);
ApiKeyCredential key = new ApiKeyCredential(manager.ApiKey);
OpenAIClient client = new OpenAIClient(key, new OpenAIClientOptions
{
    Endpoint = manager.Endpoint
});

var chatClient = client.GetChatClient(model?.ModelId).AsIChatClient().AsBuilder().UseFunctionInvocation().UseLogging(loggerFactory).Build();

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
    ToolMode = ChatToolMode.RequireAny,
    MaxOutputTokens = 2048
};

Console.WriteLine($"Prompting model with {messages[1].Contents[0]}");

var completion = await chatClient.GetResponseAsync(messages, options);

Console.WriteLine(completion.Messages[2].Contents[0]);

// Clean up
serviceProvider.Dispose();

public class WeatherService
{
    [Description("Get the current weather for a given city")]
    public static string GetCurrentWeather([Description("The city to get the weather for")] string city)
    {
        return $"The current weather in {city} is sunny with a temperature of 75°F.";
    }
}



