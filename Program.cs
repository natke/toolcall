// See https://aka.ms/new-console-template for more information

using OpenAI;
using System.ClientModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

var alias = "qwen2.5-14b";

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

// This section formats and writes the input to a JSON file. It is for clarity and debugging purposes.
// It prints the conversation output to a file named output.json. This section can be omitted if not needed.
var modelInput = new StringBuilder();
foreach (var m in messages)
{
    var msg = new { MessageRole = m.Role, Content = m.Contents?.First() };
    modelInput.Append($"{JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true })}");
};
File.WriteAllText("output.json", modelInput.ToString());
// End of messages formatting section.


ChatOptions options = new()
{
    Tools = tools,
    ToolMode = ChatToolMode.RequireAny,
    MaxOutputTokens = 2048
};

// This section formats and writes the input to a JSON file. It is for clarity and debugging purposes.
// It prints the conversation output to a file named output.json. This section can be omitted if not needed.
var modelOptions = new StringBuilder();
modelOptions.Append($"{JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true })}");
File.WriteAllText("options.json", modelOptions.ToString());
// End of options formatting section.


Console.WriteLine($"Prompting model with {messages[1].Contents[0]}");

var completion = await chatClient.GetResponseAsync(messages, options);

// This section formats and writes the output to a JSON file. It is for clarity and debugging purposes.
// It prints the conversation output to a file named output.json. This section can be omitted if not needed.
var modelOutput = new StringBuilder();
foreach (var m in completion.Messages)
{
    var msg = new { MessageRole = m.Role, Content = m.Contents?.First() };
    modelOutput.Append($"{JsonSerializer.Serialize(msg, new JsonSerializerOptions { WriteIndented = true })}");
};
File.WriteAllText("output.json", modelOutput.ToString());
// End of output formatting section.


Console.WriteLine(completion.Messages[2].Contents[0]);

public class WeatherService
{
    [Description("Get the current weather for a given city")]
    public static string GetCurrentWeather([Description("The city to get the weather for")] string city)
    {
        return $"The current weather in {city} is sunny with a temperature of 75°F.";
    }
}



