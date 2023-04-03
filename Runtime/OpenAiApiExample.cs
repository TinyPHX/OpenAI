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

        public string GetFullCode()
        {
            return GetConfigurationCode() + "\n\n" +
                   GetAiTextRequestCode() + "\n\n" + 
                   GetAiChatRequestCode() + "\n\n" + 
                   GetAiImageRequestCode();
        }

        public string GetConfigurationCode()
        {
            string code = "";
            if (!configuration.ApiKey.IsNullOrEmpty() || !configuration.Organization.IsNullOrEmpty())
            {
                code = $@"
// ------------ Configuration -----------

Configuration configuration = new Configuration(""{configuration.ApiKey}"", ""{configuration.Organization}"");
OpenAiApi openai = new OpenAiApi(configuration);
".Trim();
            }
            else
            {
                code = $@"
// ------------ Configuration -----------

//No configuration. Using config stored in Users/username/.openai/auth.json.
OpenAiApi openai = new OpenAiApi();
".Trim();
            }

            return code;
        }

        public string GetAiTextRequestCode()
        {
            string returnTab = "\n    ";
            string Prompt() => $"\"{aiTextRequest.prompt.Replace("\n", "\\n")}\"";
            string Model() => $", {returnTab}Models.Text.{aiTextRequest.model.ToString()}";
            string N() => aiTextRequest.n == 1 ? "" : $", {returnTab}n:{aiTextRequest.n}";
            string Temperature() => aiTextRequest.temperature == .8f ? "" : $", {returnTab}temperature:{aiTextRequest.temperature}";
            string MaxTokens() => aiTextRequest.max_tokens == 100 ? "" : $", {returnTab}max_tokens:{aiTextRequest.max_tokens}";
            string Stream() => !aiTextRequest.stream ? "" : $", {returnTab}stream:{aiTextRequest.stream}";
            string Callback() => $", {returnTab}" + (N() + Temperature() + MaxTokens() != "" ? "callback:" : "") + "aiText =>";
            
            return $@"
// ------------ AI Text -----------

openai.TextCompletion({Prompt()}{Model()}{N()}{Temperature()}{MaxTokens()}{Stream()}{Callback()}
{{
    Debug.Log(aiText.choices[0].text); // Do something with result!
}});
".Trim();
        }

        public string GetAiChatRequestCode()
        {
            string returnTab = "\n    ";
            string returnTabTab = "\n        ";
            string Messages() {
                string messages = "new []{ ";
                for (var index = 0; index < aiChatRequest.messages.Length; index++)
                {
                    if (index > 0)
                    {
                        messages += ", ";
                    }
                    if (aiChatRequest.messages.Length > 1)
                    {
                        messages += returnTabTab;
                    }
                    
                    var message = aiChatRequest.messages[index];
                    
                    string Content() => $"\"{message.content.Replace("\n", "\\n")}\"";
                    string Role() => message.role == Message.Role.USER ? "" : $", Message.Role.{message.role.ToString()}";

                    messages += $"new Message({Content()}{Role()})";
                }
                if (aiChatRequest.messages.Length > 1)
                {
                    messages += returnTab;
                }
                messages += " }";

                return messages;
            }
            string Model() => $", {returnTab}Models.Chat.{aiChatRequest.model.ToString()}";
            string N() => aiChatRequest.n == 1 ? "" : $", {returnTab}n:{aiChatRequest.n}";
            string Temperature() => aiChatRequest.temperature == .8f ? "" : $", {returnTab}temperature:{aiChatRequest.temperature}";
            string MaxTokens() => aiChatRequest.max_tokens == 100 ? "" : $", {returnTab}max_tokens:{aiChatRequest.max_tokens}";
            string Stream() => !aiChatRequest.stream ? "" : $", {returnTab}stream:{aiChatRequest.stream}";
            string Callback() => $", {returnTab}" + (N() + Temperature() + MaxTokens() != "" ? "callback:" : "") + "aiChat =>";
            
            return $@"
// ------------ AI Chat -----------

openai.ChatCompletion({Messages()}{Model()}{N()}{Temperature()}{MaxTokens()}{Stream()}{Callback()}
{{
    Debug.Log(aiChat.choices[0].message.content); // Do something with result!
}});
".Trim();
        }
        
        public string GetAiImageRequestCode()
        {
            string returnTab = "\n    ";
            string Prompt() => $"\"{aiImageRequest.prompt.Replace("\n", "\\n")}\"";
            string Size() => $", {returnTab}ImageSize.{aiImageRequest.size.ToString()}";
            string N() => aiImageRequest.n == 1 ? "" : $", {returnTab}n:{aiImageRequest.n}";
            string Callback() => ", aiImage =>";
            
            return $@"
// ------------ AI Image -----------

openai.CreateImage({Prompt()}{Size()}{N()}{Callback()}
{{
    Debug.Log(aiImage.data[0].texture); // Do something with result!
}});
".Trim();
        }
    }
}