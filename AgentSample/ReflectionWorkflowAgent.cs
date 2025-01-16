using System;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.History;
using Microsoft.SemanticKernel.ChatCompletion;

public class ReflectionWorkflowAgent
{

    private readonly Kernel _kernel;
    public ReflectionWorkflowAgent()
    {
        _kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion(modelId: AppConfig.Openai_ModelId_gpt4o, apiKey: AppConfig.Openai_ApiKey)
                        .Build();

    }

    //Define the Translate Agent
    private ChatCompletionAgent TranslationAgent(string agentName)
    {
        /*
        您是語言專家，專門從事{英文}到{中文}的翻譯。

        您將翻譯以下內容。
        除了翻譯之外，不要提供任何解釋或文字。

        只是直接輸出翻譯內容，不能做其他的事情。
        只是直接輸出翻譯內容，不能做其他的事情。
        只是直接輸出翻譯內容，不能做其他的事情。
        */
        string agentInstructions =
        """
        You are an expert linguist, specializing in translation from {English} to {Traditional Chinese}.
        
        You will translate the following content. 
        Do not provide any explanations or text apart from the translation.
        
        Just output the translation content directly and can't do anything else.
        Just output the translation content directly and can't do anything else.
        Just output the translation content directly and can't do anything else.
        """;

        return new ChatCompletionAgent()
        {
            Name = agentName,
            Instructions = agentInstructions,
            Kernel = _kernel
        };
    }

    private ChatCompletionAgent ReviewTranslationAgent(string agentName)
    {
        /*
        你的任務是仔細閱讀原文和從{英文}到{正體中文}的翻譯，
        然後提出建設性的批評和有益的建議，以改進翻譯。
        譯文的最終風格和語氣應符合台灣用語的風格。

        寫建議時，注意是否有改進翻譯的方法
        - 準確度（透過修正新增、誤譯、遺漏或未翻譯文字的錯誤）
        - 流暢度（透過應用{正體中文}語法、拼字和標點規則，並確保沒有不必要的重複）
        - 風格（確保翻譯反映源文本的風格並考慮任何文化背景）
        - 術語（確保術語的使用一致並反映原文本領域；並且僅確保使用等效的用語{正體中文}）


        列出具體的、有幫助的和建設性的建議以改進翻譯。
        每個建議都應針對翻譯的一個特定部分。

        僅輸出來源內容和建議，不輸出其他內容。
        */
        string agentInstructions =
        """
        Your task is to carefully read a source text and a translation from {English} to {Traditional Chinese}, 
        and then give constructive criticism and helpful suggestions to improve the translation. 
        The final style and tone of the translation should match the style of {Traditional Chinese} colloquially spoken in {Taiwan}.

        When writing suggestions, pay attention to whether there are ways to improve the translation's
        - accuracy (by correcting errors of addition, mistranslation, omission, or untranslated text)
        - fluency (by applying {Traditional Chinese} grammar, spelling and punctuation rules, and ensuring there are no unnecessary repetitions)
        - style (by ensuring the translations reflect the style of the source text and take into account any cultural context)
        - terminology (by ensuring terminology use is consistent and reflects the source text domain; and by only ensuring you use equivalent idioms {Traditional Chinese})


        Write a list of specific, helpful and constructive suggestions for improving the translation.
        Each suggestion should address one specific part of the translation.
        
        Output only the source content and suggestions and nothing else.
        Output only the source content and suggestions and nothing else.
        Output only the source content and suggestions and nothing else.
        """;

        return new ChatCompletionAgent()
        {
            Name = agentName,
            Instructions = agentInstructions,
            Kernel = _kernel
        };
    }

    private ChatCompletionAgent ImproveTranslationAgent(string agentName)
    {
        /*
            您的任務是仔細閱讀，然後編輯從{英文}到{中文}的翻譯，並考慮到專家列出的建議和建設性批評。
            您將獲得原始文字及其翻譯，您的目標是改進翻譯。
            您只能輸出新翻譯，不能輸出其他內容
        */
        string agentInstructions =
        """
        Your task is to carefully read, then edit, a translation from {English} to {Traditional Chinese}, taking into 
        account a list of expert suggestions and constructive criticisms.
        
        You will be provided with a source text and its translation and your goal is to improve the translation.

        only the output new translation and nothing else
        only the output new translation and nothing else
        only the output new translation and nothing else
        """;

        return new ChatCompletionAgent()
        {
            Name = agentName,
            Instructions = agentInstructions,
            Kernel = _kernel
        };
    }

    public async Task ChatCompletionAgentAsync()
    {
        ChatCompletionAgent translationAgent = TranslationAgent("translationAgent");
        ChatCompletionAgent reviewTranslationAgent = ReviewTranslationAgent("reviewTranslationAgent");
        ChatCompletionAgent improveTranslationAgent = ImproveTranslationAgent("improveTranslationAgent");

        //Ai Agent Group Chat (sequence agents)
        AgentGroupChat chat = new(translationAgent, reviewTranslationAgent, improveTranslationAgent);
        //AgentGroupChat chat = new(translateAgent, newsAgent); (change the order of the agents,note that the order of the agents in the group chat matters)

        var userPrompt =
            """
            Sora is OpenAI’s video generation model, designed to take text, image, and video inputs and generate a new video as an output. Users can create videos up to 1080p resolution (20 seconds max) in various formats, generate new content from text, or enhance, remix, and blend their own assets. Users will be able to explore the Featured and Recent feeds which showcase community creations and offer inspiration for new ideas. Sora builds on learnings from DALL·E and GPT models, and is designed to give people expanded tools for storytelling and creative expression. 

            Sora is a diffusion model, which generates a video by starting off with a base video that looks like static noise and gradually transforms it by removing the noise over many steps. By giving the model foresight of many frames at a time, we’ve solved a challenging problem of making sure a subject stays the same even when it goes out of view temporarily. Similar to GPT models, Sora uses a transformer architecture, unlocking superior scaling performance. 

            Sora uses the recaptioning technique from DALL·E 3, which involves generating highly descriptive captions for the visual training data. As a result, the model is able to follow the user’s text instructions in the generated video more faithfully.

            In addition to being able to generate a video solely from text instructions, the model is able to take an existing still image and generate a video from it, animating the image’s contents with accuracy and attention to small detail. The model can also take an existing video and extend it or fill in missing frames⁠. Sora serves as a foundation for models that can understand and simulate the real world, a capability we believe will be an important milestone for achieving AGI.

            Sora’s capabilities may also introduce novel risks, such as the potential for misuse of likeness or the generation of misleading or explicit video content. In order to safely deploy Sora in a product, we built on learnings from safety work for DALL·E’s deployment in ChatGPT and the API and safety mitigations for other OpenAI products such as ChatGPT. This system card outlines the resulting mitigation stack, external red teaming efforts, evaluations, and ongoing research to refine these safeguards further.
            """;


        ChatMessageContent message = new(AuthorRole.User, userPrompt);
        chat.AddChatMessage(message);

        await foreach (ChatMessageContent responese in chat.InvokeAsync())
        {
            Console.WriteLine(responese);
        }
    }

}