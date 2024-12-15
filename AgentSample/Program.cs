// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.SetMinimumLevel(LogLevel.Trace); // Set the minimum log level to Trace
        });
    })
    .Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

Console.WriteLine("\n\n Hello, .NET Conf 2024 ! \n\n");

//Platform Agent
// var agent = new PlatformAgent();
// await agent.OpenAIAssistantAgentAsync();

//NewsAgent agent
// var agent = new NewsAgent();
// await agent.ChatCompletionAgentAsync();

//Reflection Workflow Agent
// var agent = new ReflectionWorkflowAgent();
// await agent.ChatCompletionAgentAsync();

//Reflection agent
// var agent = new ReflectionAgent();
// await agent.ChatCompletionAgentAsync();

//Delegate agent
var agent = new DelegateAgent();
await agent.ChatCompletionAgentAsync();

//complex agent
// var agent = new ComplexAgent();
// await agent.ChatCompletionAgentAsync();
