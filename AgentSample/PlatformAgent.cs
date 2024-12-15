using System.ClientModel;
using System.Reflection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.History;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;

public class PlatformAgent
{
    public async Task OpenAIAssistantAgentAsync()
    {
        // Create the OpenAI provider
        OpenAIClientProvider provider = OpenAIClientProvider.ForOpenAI(new ApiKeyCredential(AppConfig.Openai_ApiKey));

        // Define the agents
        OpenAIAssistantAgent assistantAgent =
            await OpenAIAssistantAgent.CreateAsync(
                provider,
                definition: new OpenAIAssistantDefinition(AppConfig.Openai_ModelId_gpt4o)
                {
                    Name = nameof(OpenAIAssistantAgent),
                    Instructions =
                    $"""
                    你是一名具有10年經驗的 Power point 簡報I. 專家,你會根據使用者提供的資料,生成簡報檔案

                    簡報的內容並且要包含以下幾個部分:
                    1.敘述要有結構性
                    2.每一頁簡報的內容要有標題及內容
                    3.簡報的內容要有說服力
                    4.簡報的內容要有創意

                    Make the file id available to download                    
                    """,
                    EnableCodeInterpreter = true
                },
                kernel: new Kernel());

        AgentGroupChat chat = new();
        await InvokeAgentAsync("我要準備一場關於生成式AI的講座,大約15分鐘");

        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            chat.AddChatMessage(message);

            // Invoke the agent
            await foreach (ChatMessageContent response in chat.InvokeAsync(assistantAgent))
            {
                Console.WriteLine($"Input:{input}");
                Console.WriteLine($"{response.AuthorName}: {response.Content}");
                Console.WriteLine($"response: {JsonConvert.SerializeObject(response)}");
                Console.WriteLine($"\n=====================================\n");

                //從 OpenAI Assistant API response.Items 集合中找到
                //第一個具有 fileid 屬性的項目，並返回該屬性的值。
                var fileId = response.Items
                    .Select(item => item.GetType().GetProperty("fileid",
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                        ?.GetValue(item))
                    .FirstOrDefault(id => id != null);

                //如果 fileid 不為 null，則以 fileid 為參數調用 OpenAI 文件服務，
                //並從 OpenAI 文件服務中獲取文件內容。
                if (fileId != null)
                {
                    string filePath = Path.Combine(".", $"{fileId}.pptx");
                    OpenAIFileService fileService = new OpenAIFileService(AppConfig.Openai_ApiKey);
                    Microsoft.SemanticKernel.BinaryContent content = await fileService.GetFileContentAsync(fileId.ToString());
                    if (content != null)
                    {
                        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            await fileStream.WriteAsync(content.Data.Value);
                        }
                        Console.WriteLine($"File {fileId} is available for download at {content.Uri}");
                    }
                }

            }
        }
    }
}