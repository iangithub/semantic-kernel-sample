using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


public class NewsAgent
{

    private readonly Kernel _kernel;
    private readonly Kernel _kernel_4omini;
    public NewsAgent()
    {
        _kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    AppConfig.AzureOpenAIChatDeploymentName_gpt4o,
                    AppConfig.AzureOpenAIChatEndpoint,
                    AppConfig.AzureOpenAIChatApiKey
                ).Build();

        _kernel_4omini = Kernel.CreateBuilder()
               .AddAzureOpenAIChatCompletion(
                   AppConfig.AzureOpenAIChatDeploymentName_gpt4omini,
                   AppConfig.AzureOpenAIChatEndpoint,
                   AppConfig.AzureOpenAIChatApiKey
               ).Build();
    }

    public async Task ChatCompletionAgentAsync()
    {

        // News agent (uses OpenAI GPT-4 model)
        string newsAgentName = "NewsAgent";
        /*
        你是一個新聞代理，專門負責處理與新聞相關的任務。
        你的目標是提供新聞摘要。
        你將專注於這些任務，並且不會執行任何無關的任務。
        */
        string newsAgentNameInstructions =
                """
                    You are a news agent, specializing in handling news-related tasks.
                    Your goal is to provide news summaries.
                    You will focus solely on these tasks and will not perform any unrelated tasks.
                    """;

        ChatCompletionAgent newsAgent = new()
        {
            Name = newsAgentName,
            Instructions = newsAgentNameInstructions,
            Kernel = _kernel,
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions })
        };
        KernelPlugin newsPlugin = KernelPluginFactory.CreateFromType<NewsPlugin>();
        newsAgent.Kernel.Plugins.Add(newsPlugin);


        // Translate agent (uses 4omini model)
        string translateAgentName = "TranslateAgent";
        string translateAgentInstructions =
                """
                    You are a translation agent, specializing in translating text between en-us and zh-tw languages.
                    Your goal is to provide accurate translations .
                    You will focus solely on these tasks and will not perform any unrelated tasks.
                    """;
        ChatCompletionAgent translateAgent = new()
        {
            Name = translateAgentName,
            Instructions = translateAgentInstructions,
            Kernel = _kernel_4omini,
        };


        //Ai Agent Group Chat (sequence agents)
        AgentGroupChat chat = new(newsAgent, translateAgent);
        //AgentGroupChat chat = new(translateAgent, newsAgent); (change the order of the agents,note that the order of the agents in the group chat matters)

        ChatMessageContent message = new(AuthorRole.User, "今日國際上有哪些重要的事?");
        chat.AddChatMessage(message);
        Console.WriteLine(message);

        await foreach (ChatMessageContent responese in chat.InvokeAsync())
        {
            Console.WriteLine(responese);
        }
    }
}
