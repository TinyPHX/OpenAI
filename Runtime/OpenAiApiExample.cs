using System;
using System.Threading.Tasks;
using MyBox;
using OpenAI.AiModels;
using UnityEngine;

namespace OpenAi
{
    public class OpenAiApiExample : MonoBehaviour
    {
        public Configuration configuration;

        [Separator("Text Completion")] 
        public CompletionRequest completionRequest;
        public CompletionResponse completionResponse;

        [Separator("Chat Completion")] 
        public ChatCompletionRequest chatCompletionRequest;
        public ChatCompletionResponse chatCompletionResponse;

        [Separator("Image Generation")] 
        public ImageGenerationRequest imageRequest;
        public ImageGenerationResponse imageResponse;

        public async Task SendCompletionRequest()
        {
            OpenAiApi openai = new OpenAiApi();
            completionResponse = await openai.Send(completionRequest);
        }

        public async Task SendChatCompletionRequest()
        {
            OpenAiApi openai = new OpenAiApi();
            chatCompletionResponse = await openai.Send(chatCompletionRequest);
        }

        public async Task SendImageRequest()
        {
            OpenAiApi openai = new OpenAiApi();
            imageResponse = await openai.Send(imageRequest);
        }
    }
}