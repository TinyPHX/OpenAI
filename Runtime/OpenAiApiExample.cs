using System.Threading.Tasks;
using MyBox;
using OpenAI.AiModels;
using UnityEngine;

namespace OpenAi
{
    public class OpenAiApiExample : MonoBehaviour
    {
        public Configuration configuration;

        [Separator("AI Text")] 
        public AiTextRequest aiTextRequest;
        public AiText aiText;

        [Separator("AI Chat")] 
        public AiChatRequest aiChatRequest;
        public AiChat aiChat;

        [Separator("AI Image")] 
        public AiImageRequest aiImageRequest;
        public AiImage aiImageResponse;

        [Separator("AI Image Edit")] 
        public AiImageEditRequest aiImageEditRequest;
        public AiImage aiImageEditResponse;

        [Separator("AI Image Variation")] 
        public AiImageVariationRequest aiImageVariationRequest;
        public AiImage aiImageVariationResponse;

        private Configuration ConfigOrNull => (configuration.ApiKey != "" || configuration.Organization != "") ? configuration : null;

        public async Task SendAiTextRequest()
        {
            OpenAiApi openai = new OpenAiApi(ConfigOrNull);
            aiText = await openai.Send(aiTextRequest, callback: streamResult =>
            {
                aiText = streamResult;
            });
            Debug.Log("complete");
        }

        public async Task SendAiChatRequest()
        {
            OpenAiApi openai = new OpenAiApi(ConfigOrNull);
            aiChat = await openai.Send(aiChatRequest, callback: streamResult =>
            {
                aiChat = streamResult;
            });
        }

        public async Task SendAiImageRequest()
        {
            OpenAiApi openai = new OpenAiApi(ConfigOrNull);
            aiImageResponse = await openai.Send(aiImageRequest);
        }

        public async Task SendAiImageEditRequest()
        {
            OpenAiApi openai = new OpenAiApi(ConfigOrNull);
            aiImageEditResponse = await openai.Send(aiImageEditRequest);
        }
        
        public async Task SendAiImageVariationRequest()
        {
            OpenAiApi openai = new OpenAiApi(ConfigOrNull);
            aiImageVariationResponse = await openai.Send(aiImageVariationRequest);
        }
    }
}