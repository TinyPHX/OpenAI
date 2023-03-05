using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace OpenAi
{
    public class OpenAiApiExample : MonoBehaviour
    {
        public Configuration configuration;

        [Header("Text Completion")] public AiText.Request completionRequest;
        public AiText aiTextResponse;

        [Header("Image Generation")] public AiImage.Request imageRequest;
        public AiImage aiImageResponse;

        public async void SendCompletionRequest()
        {
            OpenAiApi openai = new OpenAiApi();
            aiTextResponse = await openai.CreateCompletion(completionRequest);
        }

        public async void SendImageRequest()
        {
            OpenAiApi openai = new OpenAiApi();
            aiImageResponse = await openai.CreateImage(imageRequest);
        }

        public void ReloadAuth()
        {
            Configuration.GlobalConfig = OpenAiApi.ReadConfigFromUserDirectory();
        }
    }
}