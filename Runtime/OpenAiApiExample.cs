using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace OpenAi
{
    public class OpenAiApiExample : MonoBehaviour
    {
        public Configuration configuration;

        [Header("Text Completion")] public Completion.Request completionRequest;
        public Completion completionResponse;

        [Header("Image Generation")] public Image.Request imageRequest;
        public Image imageResponse;

        public async void SendCompletionRequest()
        {
            OpenAiApi openai = new OpenAiApi(this);

            completionResponse = await openai.CreateCompletion(completionRequest);
            
            // openai.CreateCompletion(completionRequest, completion => { completionResponse = completion; });
        }

        public async void SendImageRequest()
        {
            OpenAiApi openai = new OpenAiApi(this);

            imageResponse = await openai.CreateImage(imageRequest);

            // openai.CreateImage(imageRequest, image => { imageResponse = image; });
        }

        public void ReloadAuth()
        {
            Configuration.GlobalConfig = OpenAiApi.ReadConfigFromUserDirectory();
        }
    }
}