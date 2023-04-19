using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using OpenAi;
using OpenAi.AiUtils;
using Image = OpenAi.AiUtils.Image;

namespace OpenAI.AiModels
{
    public static class Endpoints
    {
        public const string Completion = "https://api.openai.com/v1/completions";
        public const string ChatCompletion = "https://api.openai.com/v1/chat/completions";
        public const string ImageGenerations = "https://api.openai.com/v1/images/generations";
        public const string ImageEdits = "https://api.openai.com/v1/images/edits";
        public const string ImageVariations = "https://api.openai.com/v1/images/variations";
    }

    public static class Models
    {
        public enum Image
        {
            DALL_E = Int32.MaxValue-1000
        }
        
        public enum ImageEdits
        {
            DALL_E_EDIT = Int32.MaxValue-1000
        }
        
        public enum ImageVariations
        {
            DALL_E_VARIATION = Int32.MaxValue-1000
        }

        public enum Chat
        {
            GPT_3_5_TURBO = Int32.MaxValue-1000,
            GPT_3_5_TURBO_0301,
            GPT_4,
            GPT_4_0314,
        }

        public static readonly Dictionary<Enum, string> ChatToString = new Dictionary<Enum, string>()
        {
            { Chat.GPT_3_5_TURBO, "gpt-3.5-turbo" },
            { Chat.GPT_3_5_TURBO_0301, "gpt-3.5-turbo-0301" },
            { Chat.GPT_4, "gpt-4" },
            { Chat.GPT_4_0314, "gpt-4-0314" }
        };

        public enum Text
        {
            GPT_3 = Int32.MaxValue-1000, 
            ADA,
            ADA_CODE_SEARCH_CODE,
            ADA_CODE_SEARCH_TEXT,
            ADA_SEARCH_DOCUMENT,
            ADA_SEARCH_QUERY,
            ADA_SIMILARITY,
            ADA_2020_05_03,
            BABBAGE,
            BABBAGE_CODE_SEARCH_CODE,
            BABBAGE_CODE_SEARCH_TEXT,
            BABBAGE_SEARCH_DOCUMENT,
            BABBAGE_SEARCH_QUERY,
            BABBAGE_SIMILARITY,
            BABBAGE_2020_05_03,
            CODE_SEARCH_ADA_CODE_001,
            CODE_SEARCH_ADA_TEXT_001,
            CODE_SEARCH_BABBAGE_CODE_001,
            CODE_SEARCH_BABBAGE_TEXT_001,
            CURIE,
            CURIE_INSTRUCT_BETA,
            CURIE_SEARCH_DOCUMENT,
            CURIE_SEARCH_QUERY,
            CURIE_SIMILARITY,
            CURIE_2020_05_03,
            CUSHMAN_2020_05_03,
            DAVINCI,
            DAVINCI_IF_3_0_0,
            DAVINCI_INSTRUCT_BETA,
            DAVINCI_INSTRUCT_BETA_2_0_0,
            DAVINCI_SEARCH_DOCUMENT,
            DAVINCI_SEARCH_QUERY,
            DAVINCI_SIMILARITY,
            DAVINCI_2020_05_03,
            TEXT_ADA_001,
            TEXT_ADA__001,
            TEXT_BABBAGE_001,
            TEXT_BABBAGE__001,
            TEXT_CURIE_001,
            TEXT_CURIE__001,
            TEXT_DAVINCI_001,
            TEXT_DAVINCI_002,
            TEXT_DAVINCI_003,
            TEXT_DAVINCI_EDIT_001,
            TEXT_DAVINCI_INSERT_001,
            TEXT_DAVINCI_INSERT_002,
            TEXT_DAVINCI__001,
            TEXT_EMBEDDING_ADA_002,
            TEXT_SEARCH_ADA_DOC_001,
            TEXT_SEARCH_ADA_QUERY_001,
            TEXT_SEARCH_BABBAGE_DOC_001,
            TEXT_SEARCH_BABBAGE_QUERY_001,
            TEXT_SEARCH_CURIE_DOC_001,
            TEXT_SEARCH_CURIE_QUERY_001,
            TEXT_SEARCH_DAVINCI_DOC_001,
            TEXT_SEARCH_DAVINCI_QUERY_001,
            TEXT_SIMILARITY_ADA_001,
            TEXT_SIMILARITY_BABBAGE_001,
            TEXT_SIMILARITY_CURIE_001,
            TEXT_SIMILARITY_DAVINCI_001,
        }
        
        public static readonly Dictionary<Enum, string> TextToString = new Dictionary<Enum, string>()
        {            
            { Text.GPT_3, "text-davinci-003" },
            { Text.ADA, "ada" },
            { Text.ADA_CODE_SEARCH_CODE, "ada-code-search-code" },
            { Text.ADA_CODE_SEARCH_TEXT, "ada-code-search-text" },
            { Text.ADA_SEARCH_DOCUMENT, "ada-search-document" },
            { Text.ADA_SEARCH_QUERY, "ada-search-query" },
            { Text.ADA_SIMILARITY, "ada-similarity" },
            { Text.ADA_2020_05_03, "ada:2020-05-03" },
            { Text.BABBAGE, "babbage" },
            { Text.BABBAGE_CODE_SEARCH_CODE, "babbage-code-search-code" },
            { Text.BABBAGE_CODE_SEARCH_TEXT, "babbage-code-search-text" },
            { Text.BABBAGE_SEARCH_DOCUMENT, "babbage-search-document" },
            { Text.BABBAGE_SEARCH_QUERY, "babbage-search-query" },
            { Text.BABBAGE_SIMILARITY, "babbage-similarity" },
            { Text.BABBAGE_2020_05_03, "babbage:2020-05-03" },
            { Text.CODE_SEARCH_ADA_CODE_001, "code-search-ada-code-001" },
            { Text.CODE_SEARCH_ADA_TEXT_001, "code-search-ada-text-001" },
            { Text.CODE_SEARCH_BABBAGE_CODE_001, "code-search-babbage-code-001" },
            { Text.CODE_SEARCH_BABBAGE_TEXT_001, "code-search-babbage-text-001" },
            { Text.CURIE, "curie" },
            { Text.CURIE_INSTRUCT_BETA, "curie-instruct-beta" },
            { Text.CURIE_SEARCH_DOCUMENT, "curie-search-document" },
            { Text.CURIE_SEARCH_QUERY, "curie-search-query" },
            { Text.CURIE_SIMILARITY, "curie-similarity" },
            { Text.CURIE_2020_05_03, "curie:2020-05-03" },
            { Text.CUSHMAN_2020_05_03, "cushman:2020-05-03" },
            { Text.DAVINCI, "davinci" },
            { Text.DAVINCI_IF_3_0_0, "davinci-if:3.0.0" },
            { Text.DAVINCI_INSTRUCT_BETA, "davinci-instruct-beta" },
            { Text.DAVINCI_INSTRUCT_BETA_2_0_0, "davinci-instruct-beta:2.0.0" },
            { Text.DAVINCI_SEARCH_DOCUMENT, "davinci-search-document" },
            { Text.DAVINCI_SEARCH_QUERY, "davinci-search-query" },
            { Text.DAVINCI_SIMILARITY, "davinci-similarity" },
            { Text.DAVINCI_2020_05_03, "davinci:2020-05-03" },
            { Text.TEXT_ADA_001, "text-ada-001" },
            { Text.TEXT_ADA__001, "text-ada:001" },
            { Text.TEXT_BABBAGE_001, "text-babbage-001" },
            { Text.TEXT_BABBAGE__001, "text-babbage:001" },
            { Text.TEXT_CURIE_001, "text-curie-001" },
            { Text.TEXT_CURIE__001, "text-curie:001" },
            { Text.TEXT_DAVINCI_001, "text-davinci-001" },
            { Text.TEXT_DAVINCI_002, "text-davinci-002" },
            { Text.TEXT_DAVINCI_003, "text-davinci-003" },
            { Text.TEXT_DAVINCI_EDIT_001, "text-davinci-edit-001" },
            { Text.TEXT_DAVINCI_INSERT_001, "text-davinci-insert-001" },
            { Text.TEXT_DAVINCI_INSERT_002, "text-davinci-insert-002" },
            { Text.TEXT_DAVINCI__001, "text-davinci:001" },
            { Text.TEXT_EMBEDDING_ADA_002, "text-embedding-ada-002" },
            { Text.TEXT_SEARCH_ADA_DOC_001, "text-search-ada-doc-001" },
            { Text.TEXT_SEARCH_ADA_QUERY_001, "text-search-ada-query-001" },
            { Text.TEXT_SEARCH_BABBAGE_DOC_001, "text-search-babbage-doc-001" },
            { Text.TEXT_SEARCH_BABBAGE_QUERY_001, "text-search-babbage-query-001" },
            { Text.TEXT_SEARCH_CURIE_DOC_001, "text-search-curie-doc-001" },
            { Text.TEXT_SEARCH_CURIE_QUERY_001, "text-search-curie-query-001" },
            { Text.TEXT_SEARCH_DAVINCI_DOC_001, "text-search-davinci-doc-001" },
            { Text.TEXT_SEARCH_DAVINCI_QUERY_001, "text-search-davinci-query-001" },
            { Text.TEXT_SIMILARITY_ADA_001, "text-similarity-ada-001" },
            { Text.TEXT_SIMILARITY_BABBAGE_001, "text-similarity-babbage-001" },
            { Text.TEXT_SIMILARITY_CURIE_001, "text-similarity-curie-001" },
            { Text.TEXT_SIMILARITY_DAVINCI_001, "text-similarity-davinci-001" },
        };

        public enum TextEdit
        {
            TEXT_DAVINCI_EDIT_001 = Int32.MaxValue-1000,
            CODE_DAVINCI_EDIT_001
        }

        public static readonly Dictionary<Enum, string> TextEditToString = new Dictionary<Enum, string>
        {
            { TextEdit.TEXT_DAVINCI_EDIT_001, "text-davinci-edit-001" },
            { TextEdit.CODE_DAVINCI_EDIT_001, "code-davinci-edit-001" }
        };

        public enum Audio
        {
            WHISPER_1 = Int32.MaxValue-1000,
        }

        public static readonly Dictionary<Enum, string> AudioToString = new Dictionary<Enum, string>
        {
            { Audio.WHISPER_1, "whisper-1" },
        };

        public enum Other
        {
            IF_CURIE_V2 = Int32.MaxValue-1000,
            IF_DAVINCI_V2,
            IF_DAVINCI_3_0_0
        }
    }
    
    public static class AiModelJson
    {
        public static string ReplaceEnum<TEnum>(string json, string name, Dictionary<Enum, string> mapping) where TEnum : struct, Enum
        {
            Regex regex = new Regex($"\"{name}\": ([\\d]+)");
            MatchCollection matches = regex.Matches(json);
            List<Match> matchList = matches.Cast<Match>().OrderByDescending(m => m.Index).ToList();
            
            foreach (Match match in matchList)
            {
                Group group = match.Groups[1];
                if (Enum.TryParse(group.Value, out TEnum valueEnum) && mapping.ContainsKey(valueEnum))
                {
                    string before = json.Substring(0, group.Index);
                    string newValue = $"\"{mapping[valueEnum]}\"";
                    string after = json.Substring(group.Index + group.Length, json.Length - group.Index - group.Length);

                    json = before + newValue + after;
                }
            }
            
            return json;
        }
        
        public static readonly Dictionary<Enum, string> ImageSizeToString = new Dictionary<Enum, string>()
        {
            { ImageSize.SMALL, "256x256" }, 
            { ImageSize.MEDIUM, "512x512" }, 
            { ImageSize.LARGE, "1024x1024" }
        };
        
        public static readonly Dictionary<Enum, string> ImageResponseFormatToString = new Dictionary<Enum, string>()
        {
            { ImageResponseFormat.URL, "url" }, 
            { ImageResponseFormat.B64_JSON, "b64_json" }, 
        };
    }
    
    public static class AiModelDefaults
    {
        public static readonly string prompt = "";
        public static readonly Message[] messages = new [] { new Message("") };
        public static readonly int n = 1;
        public static readonly int max_tokens = 100;
        public static readonly float temperature = .8f;
        public static readonly ImageSize size = ImageSize.SMALL;
        public static readonly bool stream = false;
        public static readonly ImageResponseFormat response_format = ImageResponseFormat.B64_JSON;
    }
    
    [Serializable]
    public class ModelRequestResponse<T> where T : class
    {
        public virtual string Url { get; }
        
        public virtual string ToJson()
        {
            return JsonUtility.ToJson(this as T, true);
        }
        
        public virtual T FromJson(string jsonString)
        {
            return JsonUtility.FromJson<T>(jsonString);
        }
        
        public UnityWebRequest.Result Result { get; set; }
    }
    
    #region Request Types
    
    [Serializable]
    public class ModelRequest<T> where T : class
    {
        public virtual string Url { get; }
        public virtual bool Stream => false;
        public virtual bool UseForm => false;
        
        public virtual string ToJson()
        {
            return JsonUtility.ToJson(this as T, true);
        }

        public virtual WWWForm ToForm()
        {
            return default;
        }
    }
    
    // https://platform.openai.com/docs/api-reference/completions/create
    [Serializable]
    public class AiTextRequest : ModelRequest<AiTextRequest>
    {
        public override string Url => Endpoints.Completion;
        public override bool Stream => stream;
        
        public Models.Text model = Models.Text.GPT_3;
        [TextArea(1,20)]
        public string prompt = AiModelDefaults.prompt;
        public int n = AiModelDefaults.n;
        public int max_tokens = AiModelDefaults.max_tokens;
        public float temperature = AiModelDefaults.temperature;
        public bool stream = AiModelDefaults.stream;
        
        public override string ToJson()
        {
            string json = JsonUtility.ToJson(this, true);
            json = AiModelJson.ReplaceEnum<Models.Text>(json, nameof(model), Models.TextToString);
            return json;
        }
    }

    [Serializable]
    public class FullAiTextRequest : ModelRequest<FullAiTextRequest>
    {
        public string prompt;
        public string suffix;
        public int n;
        public int max_tokens;
        public float temperature;
        public int top_p;
        public bool stream;
        public int logprobs;
        public bool echo;
        public string stop;
        public float presence_penalty;
        public float frequency_penalty;
        public int best_of;
        public string logit_bias; //json
        public string user;
    }
    
    // https://platform.openai.com/docs/api-reference/completions/create
    [Serializable]
    public class AiTextEditRequest : ModelRequest<AiTextEditRequest>
    {
        public override string Url => Endpoints.Completion;
        public override bool Stream => stream;
        
        public Models.Text model = Models.Text.GPT_3;
        [TextArea(1,20)]
        public string prompt = AiModelDefaults.prompt;
        public int n = AiModelDefaults.n;
        public int max_tokens = AiModelDefaults.max_tokens;
        public float temperature = AiModelDefaults.temperature;
        public bool stream = AiModelDefaults.stream;
        
        public override string ToJson()
        {
            string json = JsonUtility.ToJson(this, true);
            json = AiModelJson.ReplaceEnum<Models.Text>(json, nameof(model), Models.TextToString);
            return json;
        }
    }
    
    [Serializable]
    public class AiChatRequest : ModelRequest<AiChatRequest> 
    {
        public override string Url => Endpoints.ChatCompletion;
        public override bool Stream => stream;

        public Models.Chat model = Models.Chat.GPT_4;
        public Message[] messages = AiModelDefaults.messages;
        public int n = AiModelDefaults.n;
        public float temperature = AiModelDefaults.temperature;
        public int max_tokens = AiModelDefaults.max_tokens;
        public bool stream = AiModelDefaults.stream;
        
        public override string ToJson()
        {
            string json = JsonUtility.ToJson(this, true);
            json = AiModelJson.ReplaceEnum<Models.Chat>(json, nameof(model), Models.ChatToString);
            json = AiModelJson.ReplaceEnum<Message.Role>(json, nameof(Message.role), Message.RoleToString);
            return json;
        }
    }
    
    [Serializable]
    public class AiImageRequest : ModelRequest<AiImageRequest>
    {
        public override string Url => Endpoints.ImageGenerations;
        
        public string prompt = AiModelDefaults.prompt;
        public ImageSize size = AiModelDefaults.size;
        public int n = AiModelDefaults.n;
        [HideInInspector] public ImageResponseFormat response_format = AiModelDefaults.response_format;
        
        public override string ToJson()
        {
            string json = JsonUtility.ToJson(this, true);
            json = AiModelJson.ReplaceEnum<ImageSize>(json, nameof(size), AiModelJson.ImageSizeToString);
            json = AiModelJson.ReplaceEnum<ImageResponseFormat>(json, nameof(response_format), AiModelJson.ImageResponseFormatToString);
            return json;
        }
    }
    
    [Serializable]
    public class AiImageEditRequest : ModelRequest<AiImageEditRequest>
    {
        public override string Url => Endpoints.ImageEdits;
        public override bool UseForm => true;

        public Texture2D image;
        public Texture2D mask;
        public string prompt = AiModelDefaults.prompt;
        public ImageSize size = AiModelDefaults.size;
        public int n = AiModelDefaults.n;
        [HideInInspector] public ImageResponseFormat response_format = AiModelDefaults.response_format;

        //TODO TODO TODO TODO TODO TODO TODO  
        public List<IMultipartFormSection> ToFormTest()
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
            formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));
            return formData;
        }
        
        public override WWWForm ToForm()
        {
            WWWForm form = new WWWForm();
            AiAssets.MakeTextureReadable(image);
            AiAssets.MakeTextureReadable(mask);
            byte[] imageData = ImageConversion.EncodeToPNG(image);
            byte[] maskData = ImageConversion.EncodeToPNG(mask);
            form.AddBinaryData(nameof(image), imageData, "AiImageEditRequest-" + nameof(image) + ".png");
            form.AddBinaryData(nameof(mask), maskData, "AiImageEditRequest-" + nameof(mask) + ".png");
            form.AddField(nameof(prompt), prompt);
            form.AddField(nameof(size), AiModelJson.ImageSizeToString[size]);
            form.AddField(nameof(n), n);
            form.AddField(nameof(response_format), AiModelJson.ImageResponseFormatToString[response_format]);
            return form;
        }
    }
    
    [Serializable]
    public class AiImageVariationRequest : ModelRequest<AiImageVariationRequest>
    {
        public override string Url => Endpoints.ImageVariations;
        public override bool UseForm => true;

        public Texture2D image;
        public ImageSize size = AiModelDefaults.size;
        public int n = AiModelDefaults.n;
        [HideInInspector] public ImageResponseFormat response_format = AiModelDefaults.response_format;
        
        public override WWWForm ToForm()
        {
            WWWForm form = new WWWForm();
            AiAssets.MakeTextureReadable(image);
            byte[] imageData = ImageConversion.EncodeToPNG(image);
            form.AddBinaryData(nameof(image), imageData);
            form.AddField(nameof(size), AiModelJson.ImageSizeToString[size]);
            form.AddField(nameof(n), n);
            form.AddField(nameof(response_format), AiModelJson.ImageResponseFormatToString[response_format]);
            return form;
        }
    }
    
    #endregion Request Types

    #region Response Types

    [Serializable]
    public class ModelResponse<T>
    {
        public virtual T FromJson(string jsonString)
        {
            return JsonUtility.FromJson<T>(jsonString);
        }

        public virtual T AppendStreamResult(T streamResult)
        {
            return streamResult;
        }

        public virtual Task ResponseCallback<R>(ModelRequest<R> request)
            where R : class
        {
            return Task.CompletedTask;
        }
        
        public UnityWebRequest.Result Result { get; set; }
    }

    [Serializable]
    public class AiText : ModelResponse<AiText>
    {
        public string Text => choices.Length > 0 ? choices[0].text : "";
        
        public string id;
        public string obj;
        public int created;
        public Models.Text model;
        public Choice[] choices = new Choice[] {};
        public Usage usage;

        private int responseCallbackCount = 0;

        public override AiText AppendStreamResult(AiText streamResult)
        {
            for (var i = 0; i < streamResult.choices.Length; i++)
            {
                Choice newChoice = streamResult.choices[i];
                while (choices.Length < newChoice.index + 1)
                {
                    choices = choices.Append(new Choice()).ToArray();
                }

                string choice = choices[newChoice.index].text + newChoice.text;
                choices[newChoice.index] = newChoice;
                choices[newChoice.index].text = choice;
            }

            return this;
        }

        public override Task ResponseCallback<R>(ModelRequest<R> request)
        {
            for (int i = 0; i < choices.Length; i++)
            {
                if (responseCallbackCount == 0 || string.IsNullOrWhiteSpace(choices[i].text))
                {
                    choices[i].text = choices[i].text.TrimStart(); //Trim the response because it often has whitespace.
                }
            }

            responseCallbackCount++;

            return base.ResponseCallback(request);
        }
    }

    [Serializable]
    public class AiChat : ModelResponse<AiChat>
    {
        public string Text => choices.Length > 0 ? choices[0].message.content : "";
        public Message Message => choices.Length > 0 ? choices[0].message : new Message("");
        
        public string id;
        public string obj;
        public int created;
        public Models.Chat model;
        public MessageChoice[] choices = new MessageChoice[] {};
        public Usage usage;

        public override AiChat AppendStreamResult(AiChat streamResult)
        {
            for (var i = 0; i < streamResult.choices.Length; i++)
            {
                MessageChoice newChoice = streamResult.choices[i];
                while (choices.Length < newChoice.index + 1)
                {
                    choices = choices.Append(new MessageChoice()).ToArray();
                }

                string content = choices[newChoice.index].message.content + newChoice.delta.content;
                choices[newChoice.index] = newChoice;
                choices[newChoice.index].message.content = content;
            }

            return this;
        }
    }

    [Serializable]
    public class AiImage : ModelResponse<AiImage>
    {
        public Texture2D Texture => data.Length > 0 ? data[0].texture : default;

        public int created;
        public ImageData[] data = new ImageData[]{};

        public override async Task ResponseCallback<T>(ModelRequest<T> request)
        {
            string name = "";
            ImageResponseFormat response_format = AiModelDefaults.response_format;
            if (request is AiImageRequest)
            {
                AiImageRequest imageRequest = request as AiImageRequest;
                response_format = imageRequest.response_format;
                name = imageRequest.prompt;
            } 
            else if (request is AiImageEditRequest)
            {
                AiImageEditRequest imageRequest = request as AiImageEditRequest;
                response_format = imageRequest.response_format;
                name = imageRequest.prompt;
            }
            else if (request is AiImageVariationRequest)
            {
                AiImageVariationRequest imageRequest = request as AiImageVariationRequest;
                response_format = imageRequest.response_format;
                name = imageRequest.image.name + "_variant";
            }
            else
            {
                DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                int currentEpochTime = (int)(DateTime.UtcNow - epochStart).TotalSeconds;
                name = currentEpochTime.ToString();
            }

            if (response_format == ImageResponseFormat.B64_JSON)
            {
                for (var i = 0; i < data.Length; i++)
                {
                    var dataElement = data[i];
                    byte[] imageDate = Convert.FromBase64String(dataElement.b64_json);
                    dataElement.texture = new Texture2D(1, 1);
                    dataElement.texture.LoadImage(imageDate);

                    Texture2D texture = dataElement.texture;
                    if (Configuration.SaveTempImages)
                    {
                        string num = (i + 1).ToString();
                        if (name != "")
                        {
                            num = i == 0 ? "" : "_" + num;
                        }

                        texture = Image.SaveTempImageToFile(name + num, texture, false);
                    }

                    dataElement.texture = texture;
                }
            }
            else if (response_format == ImageResponseFormat.URL)
            {
                Texture2D[] textures = await GetAllImages(this);
                for (int i = 0; i < textures.Length; i++)
                {
                    ImageData dataElement = data[i];
                    Texture2D texture = textures[i];
                    if (Configuration.SaveTempImages)
                    {
                        string num = (i + 1).ToString();
                        if (name != "")
                        {
                            num = i == 0 ? "" : "_" + num;
                        }

                        texture = Image.SaveTempImageToFile(name + num, texture, false);
                    }

                    dataElement.texture = texture;
                }
            }
        }
        
        public static Task<Texture2D[]> GetAllImages(AiImage aiAiImage)
        {
            List<Task<Texture2D>> getImageTasks = new List<Task<Texture2D>>{};
            
            for (int i = 0; i < aiAiImage.data.Length; i++)
            {
                ImageData data = aiAiImage.data[i];
                if (data.url != "")
                {
                    getImageTasks.Add(GetImageFromUrl(data.url));
                }
            }

            return Task.WhenAll(getImageTasks);
        }

        public delegate void Callback<T>(T response=default);
        private static Task<Texture2D> GetImageFromUrl(string url)
        {
            (Task<Texture2D> task, Callback<Texture2D> callback) = CallbackToTask<Texture2D>();
            OpenAiApi.Runner.StartCoroutine(GetImageFromUrl(url, callback));
            return task;
        }

        private static IEnumerator GetImageFromUrl(string url, Callback<Texture2D> callback) {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
            webRequest.SetRequestHeader("Access-Control-Allow-Origin", "*");
            yield return webRequest.SendWebRequest();
            Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
            callback(texture);
        }

        private static Tuple<Task<T>, Callback<T>> CallbackToTask<T>(Callback<T> callback=null)
        {
            var taskCompletion = new TaskCompletionSource<T>();
            callback ??= (value) => {  };
            Callback<T> wrappedCallback = value =>
            {
                taskCompletion.SetResult(value);
                callback(value);
            };

            return new Tuple<Task<T>, Callback<T>>(taskCompletion.Task, wrappedCallback);
        }
    }

    #endregion Response Types

    #region Request/Response Dependency Types

    public enum ImageSize
    {
        SMALL = Int32.MaxValue-1000, 
        MEDIUM, 
        LARGE
    };

    public enum ImageResponseFormat
    {
        URL = Int32.MaxValue-1000,
        B64_JSON
    }

    [Serializable]
    public class Choice
    {
        [TextArea(1,20)]
        public string text = "";
        public int index = 0;
        public string logprobs;
        public string finish_reason;
    }

    [Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }
    
    [Serializable]
    public class Message
    {
        public enum Role
        {
            SYSTEM = Int32.MaxValue-1000,
            ASSISTANT, 
            USER
        }
        
        public static readonly Dictionary<Enum, string> RoleToString = new Dictionary<Enum, string>()
        {
            { Role.SYSTEM, "system" },
            { Role.ASSISTANT, "assistant" },
            { Role.USER, "user" }
        };

        public Role role = Role.USER;
        [TextArea(1,20)]
        public string content;

        public Message(string content)
        {
            this.content = content;
        }
                
        public Message(string content, Role role)
        {
            this.role = role;
            this.content = content;
        }
    }
    
    [Serializable]
    public class MessageChoice
    {
        public Message message = new Message("");
        public Message delta = new Message(""); //for streaming only
        public int index = 0;
        public string logprobs;
        public string finish_reason;
    }
    
    
    [Serializable]
    public class ImageData
    {
        public Texture2D texture;
        [HideInInspector] public string b64_json;
        [HideInInspector] public string url;
    }

    #endregion
}