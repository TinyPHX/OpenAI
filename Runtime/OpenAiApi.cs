using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using OpenAI;
using UnityEngine;
using UnityEngine.Networking;

namespace OpenAi
{
    [Serializable]
    public class Configuration
    {
        public static string AuthFileDir => "/.openai";
        public static string AuthFilePath => "/.openai/auth.json";
        public static Configuration GlobalConfig = new Configuration("", "");
        public static bool SaveTempImages => true;
        public class GlobalConfigFormat
        {
            public string private_api_key;
            public string organization;
        }
        
        //Specific key - this is if you want to support multiple api keys or anything like that.
        [SerializeField] private string apiKey;
        [SerializeField] private string organization;

        public Configuration(string apiKey, string organization)
        {
            this.apiKey = apiKey;
            this.organization = organization;
        }

        public string ApiKey => apiKey;
        public string Organization => organization;
    }

    public class OpenAiApi
    {
        public enum Model
        {
            CHAT_GPT,
            ADA,
            ADA_CODE_SEARCH_CODE,
            ADA_CODE_SEARCH_TEXT,
            ADA_SEARCH_DOCUMENT,
            ADA_SEARCH_QUERY,
            ADA_SIMILARITY,
            ADA_2020_05_03,
            AUDIO_TRANSCRIBE_DEPRECATED,
            BABBAGE,
            BABBAGE_CODE_SEARCH_CODE,
            BABBAGE_CODE_SEARCH_TEXT,
            BABBAGE_SEARCH_DOCUMENT,
            BABBAGE_SEARCH_QUERY,
            BABBAGE_SIMILARITY,
            BABBAGE_2020_05_03,
            CODE_CUSHMAN_001,
            CODE_DAVINCI_002,
            CODE_DAVINCI_EDIT_001,
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
            IF_CURIE_V2,
            IF_DAVINCI_V2,
            IF_DAVINCI_3_0_0,
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
            TEXT_SIMILARITY_DAVINCI_001
        }

        public enum Size
        {
            SMALL,
            MEDIUM,
            LARGE
        }

        public static readonly Dictionary<Model, string> ModelToString = new Dictionary<Model, string>()
        {
            { Model.CHAT_GPT, "text-davinci-003" },
            { Model.ADA, "ada" },
            { Model.ADA_CODE_SEARCH_CODE, "ada-code-search-code" },
            { Model.ADA_CODE_SEARCH_TEXT, "ada-code-search-text" },
            { Model.ADA_SEARCH_DOCUMENT, "ada-search-document" },
            { Model.ADA_SEARCH_QUERY, "ada-search-query" },
            { Model.ADA_SIMILARITY, "ada-similarity" },
            { Model.ADA_2020_05_03, "ada:2020-05-03" },
            { Model.AUDIO_TRANSCRIBE_DEPRECATED, "audio-transcribe-deprecated" },
            { Model.BABBAGE, "babbage" },
            { Model.BABBAGE_CODE_SEARCH_CODE, "babbage-code-search-code" },
            { Model.BABBAGE_CODE_SEARCH_TEXT, "babbage-code-search-text" },
            { Model.BABBAGE_SEARCH_DOCUMENT, "babbage-search-document" },
            { Model.BABBAGE_SEARCH_QUERY, "babbage-search-query" },
            { Model.BABBAGE_SIMILARITY, "babbage-similarity" },
            { Model.BABBAGE_2020_05_03, "babbage:2020-05-03" },
            { Model.CODE_CUSHMAN_001, "code-cushman-001" },
            { Model.CODE_DAVINCI_002, "code-davinci-002" },
            { Model.CODE_DAVINCI_EDIT_001, "code-davinci-edit-001" },
            { Model.CODE_SEARCH_ADA_CODE_001, "code-search-ada-code-001" },
            { Model.CODE_SEARCH_ADA_TEXT_001, "code-search-ada-text-001" },
            { Model.CODE_SEARCH_BABBAGE_CODE_001, "code-search-babbage-code-001" },
            { Model.CODE_SEARCH_BABBAGE_TEXT_001, "code-search-babbage-text-001" },
            { Model.CURIE, "curie" },
            { Model.CURIE_INSTRUCT_BETA, "curie-instruct-beta" },
            { Model.CURIE_SEARCH_DOCUMENT, "curie-search-document" },
            { Model.CURIE_SEARCH_QUERY, "curie-search-query" },
            { Model.CURIE_SIMILARITY, "curie-similarity" },
            { Model.CURIE_2020_05_03, "curie:2020-05-03" },
            { Model.CUSHMAN_2020_05_03, "cushman:2020-05-03" },
            { Model.DAVINCI, "davinci" },
            { Model.DAVINCI_IF_3_0_0, "davinci-if:3.0.0" },
            { Model.DAVINCI_INSTRUCT_BETA, "davinci-instruct-beta" },
            { Model.DAVINCI_INSTRUCT_BETA_2_0_0, "davinci-instruct-beta:2.0.0" },
            { Model.DAVINCI_SEARCH_DOCUMENT, "davinci-search-document" },
            { Model.DAVINCI_SEARCH_QUERY, "davinci-search-query" },
            { Model.DAVINCI_SIMILARITY, "davinci-similarity" },
            { Model.DAVINCI_2020_05_03, "davinci:2020-05-03" },
            { Model.IF_CURIE_V2, "if-curie-v2" },
            { Model.IF_DAVINCI_V2, "if-davinci-v2" },
            { Model.IF_DAVINCI_3_0_0, "if-davinci:3.0.0" },
            { Model.TEXT_ADA_001, "text-ada-001" },
            { Model.TEXT_ADA__001, "text-ada:001" },
            { Model.TEXT_BABBAGE_001, "text-babbage-001" },
            { Model.TEXT_BABBAGE__001, "text-babbage:001" },
            { Model.TEXT_CURIE_001, "text-curie-001" },
            { Model.TEXT_CURIE__001, "text-curie:001" },
            { Model.TEXT_DAVINCI_001, "text-davinci-001" },
            { Model.TEXT_DAVINCI_002, "text-davinci-002" },
            { Model.TEXT_DAVINCI_003, "text-davinci-003" },
            { Model.TEXT_DAVINCI_EDIT_001, "text-davinci-edit-001" },
            { Model.TEXT_DAVINCI_INSERT_001, "text-davinci-insert-001" },
            { Model.TEXT_DAVINCI_INSERT_002, "text-davinci-insert-002" },
            { Model.TEXT_DAVINCI__001, "text-davinci:001" },
            { Model.TEXT_EMBEDDING_ADA_002, "text-embedding-ada-002" },
            { Model.TEXT_SEARCH_ADA_DOC_001, "text-search-ada-doc-001" },
            { Model.TEXT_SEARCH_ADA_QUERY_001, "text-search-ada-query-001" },
            { Model.TEXT_SEARCH_BABBAGE_DOC_001, "text-search-babbage-doc-001" },
            { Model.TEXT_SEARCH_BABBAGE_QUERY_001, "text-search-babbage-query-001" },
            { Model.TEXT_SEARCH_CURIE_DOC_001, "text-search-curie-doc-001" },
            { Model.TEXT_SEARCH_CURIE_QUERY_001, "text-search-curie-query-001" },
            { Model.TEXT_SEARCH_DAVINCI_DOC_001, "text-search-davinci-doc-001" },
            { Model.TEXT_SEARCH_DAVINCI_QUERY_001, "text-search-davinci-query-001" },
            { Model.TEXT_SIMILARITY_ADA_001, "text-similarity-ada-001" },
            { Model.TEXT_SIMILARITY_BABBAGE_001, "text-similarity-babbage-001" },
            { Model.TEXT_SIMILARITY_CURIE_001, "text-similarity-curie-001" },
            { Model.TEXT_SIMILARITY_DAVINCI_001, "text-similarity-davinci-001" }
        };

        public static readonly Dictionary<Size, string> SizeToString = new Dictionary<Size, string>()
        {
            { Size.SMALL, "256x256" },
            { Size.MEDIUM, "512x512" },
            { Size.LARGE, "1024x1024" }
        };

        private Configuration config;
        private static CoroutineRunner runner;
        public static CoroutineRunner Runner {
            get
            {
                if (!runner)
                {
                    runner = GameObject.FindObjectOfType<CoroutineRunner>();
                }
                
                if (!runner)
                {
                    GameObject gameObject = new GameObject("Open AI Request Runner");
                    gameObject.AddComponent<CoroutineRunner>();
                    gameObject.hideFlags = HideFlags.HideInHierarchy;
                    runner = gameObject.GetComponent<CoroutineRunner>();
                }

                if (Application.isPlaying && runner)
                {
                    UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
                }

                return runner;
            }
        }
        
        public OpenAiApi()
        {
            config = null;
        }

        public OpenAiApi(Configuration config)
        {
            this.config = config;
        }

        public Configuration ActiveConfig => config ?? OpenAi.Configuration.GlobalConfig;

        public string ApiKey
        {
            get
            {
                if (ActiveConfig == null)
                {
                    OpenAi.Configuration.GlobalConfig = ReadConfigFromUserDirectory();
                }
                return ActiveConfig.ApiKey;
            }
        }

        public string Organization  
        {
            get
            {
                if (ActiveConfig == null)
                {
                    OpenAi.Configuration.GlobalConfig = ReadConfigFromUserDirectory();
                }
                return ActiveConfig.Organization;
            }
        }
        
        // public static Configuration
        
        public static string ConfigFileDir => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + OpenAi.Configuration.AuthFileDir;
        public static string ConfigFilePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + OpenAi.Configuration.AuthFilePath;

        public static Configuration ReadConfigFromUserDirectory()
        {
            try
            {
                string jsonConfig = File.ReadAllText(ConfigFilePath);
                var config = JsonUtility.FromJson<Configuration.GlobalConfigFormat>(jsonConfig);
                return new Configuration(config.private_api_key, config.organization);
            }
            catch (Exception exception) when (exception is DirectoryNotFoundException || exception is FileNotFoundException)
            {
                return new Configuration("", "");
            }
        }

        public static void SaveConfigToUserDirectory(Configuration config)
        {
            if (!Directory.Exists(ConfigFileDir)) { Directory.CreateDirectory(ConfigFileDir); }
            Configuration.GlobalConfigFormat globalConfig = new Configuration.GlobalConfigFormat
            {
                private_api_key = config.ApiKey,
                organization = config.Organization
            };
            string jsonConfig = JsonUtility.ToJson(globalConfig, true);
            File.WriteAllText(ConfigFilePath, jsonConfig);
        }
        
        public static void Configuration(string globalApiKey, string globalOrganization)
        {
            OpenAi.Configuration.GlobalConfig = new Configuration(globalApiKey, globalOrganization);
        }
        
        public delegate void Callback<T>(T response=default);

        #region Completions

        public Task<AiText> CreateCompletion(string prompt, Callback<AiText> callback=null)
        {
            return CreateCompletion(prompt, Model.TEXT_DAVINCI_003, callback);
        }

        public Task<AiText> CreateCompletion(string prompt, Model model, Callback<AiText> callback=null)
        {
            string modelString = ModelToString[model];
            return CreateCompletion(prompt, modelString, callback);
        }

        public Task<AiText> CreateCompletion(string prompt, string model, Callback<AiText> callback=null)
        {
            
            AiText.Request request = new AiText.Request(prompt, model);
            return CreateCompletion(request, callback);
        }

        public Task<AiText> CreateCompletion(AiText.Request request, Callback<AiText> callback=null)
        {
            return Post(request, callback);
        }
        
        #endregion

        #region Images

        public Task<AiImage> CreateImage(string prompt, Callback<AiImage> callback=null)
        {
            return CreateImage(prompt, Size.SMALL, callback);
        }

        public Task<AiImage> CreateImage(string prompt, Size size, Callback<AiImage> callback=null)
        {
            string sizeString = SizeToString[size];
            return CreateImage(prompt, sizeString, callback);
        }

        public Task<AiImage> CreateImage(string prompt, string size, Callback<AiImage> callback=null)
        {
            
            AiImage.Request request = new AiImage.Request(prompt, size);
            return CreateImage(request, callback);
        }

        public Task<AiImage> CreateImage(AiImage.Request request, Callback<AiImage> callback=null)
        {
            callback ??= value => {  };
            var taskCompletion = new TaskCompletionSource<AiImage>();
            Callback<AiImage> callbackIntercept = async image =>
            {
                Texture2D[] textures = await GetAllImages(image);
                for (int i = 0; i < textures.Length; i++)
                {
                    AiImage.Data data = image.data[i];

                    Texture2D texture = textures[i];
                    if (OpenAi.Configuration.SaveTempImages)
                    {
                        string num = i > 0 ? (" " + i) : "";
                        texture = Utils.Image.SaveToFile(request.prompt + num, texture, false, Utils.Image.TempDirectory);
                    }
                    
                    data.texture = texture;
                }
                callback(image);
                taskCompletion.SetResult(image);
            };
            Post(request, callbackIntercept);
            return taskCompletion.Task;
        }
        
        #endregion
        
        private Task<Texture2D[]> GetAllImages(AiImage aiImage)
        {
            List<Task<Texture2D>> getImageTasks = new List<Task<Texture2D>>{};
            
            for (int i = 0; i < aiImage.data.Length; i++)
            {
                AiImage.Data data = aiImage.data[i];
                if (data.url != "")
                {
                    getImageTasks.Add(GetImageFromUrl(data.url));
                }
            }

            return Task.WhenAll(getImageTasks);
        }

        private Task<Texture2D> GetImageFromUrl(string url)
        {
            (Task<Texture2D> task, Callback<Texture2D> callback) = CallbackToTask<Texture2D>();
            Runner.StartCoroutine(GetImageFromUrl(url, callback));
            return task;
        }

        static IEnumerator GetImageFromUrl(string url, Callback<Texture2D> callback) {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
            yield return webRequest.SendWebRequest();
            Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
            callback(texture);
        }

        private static Tuple<Task<T>, Callback<T>> CallbackToTask<T>(Callback<T> callback=null)
        {
            var taskCompletion = new TaskCompletionSource<T>();
            callback ??= value => {  };
            Callback<T> wrappedCallback = value =>
            {
                taskCompletion.SetResult(value);
                callback(value);
            };

            return new Tuple<Task<T>, Callback<T>>(taskCompletion.Task, wrappedCallback);
        }
        
        private Task<T> Post<T,R>(R requestBody, Callback<T> completionCallback=null) where T : IRequestable<T>, new()
        {
            string url = new T().URL;
            string bodyString = JsonUtility.ToJson(requestBody);
            completionCallback ??= value => {  };

            (Task<T> task, Callback<T> taskCallback) = CallbackToTask(completionCallback);
            Runner.StartCoroutine(Post(url, bodyString, taskCallback));

            return task;
        }

        private IEnumerator Post<T>(string url, string body, Callback<T> completionCallback) where T : IRequestable<T>, new()
        {
            UnityWebRequest webRequest = new UnityWebRequest(url);
                
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "Authorization", "Bearer " + ApiKey },
                { "Content-Type", "application/json" },
                { "OpenAI-Organization", Organization },
            };
            foreach (var entry in headers)
            {
                webRequest.SetRequestHeader(entry.Key, entry.Value);
            }
            
            byte[] bodyByteArray = System.Text.Encoding.UTF8.GetBytes(body);
            
            webRequest.uploadHandler = new UploadHandlerRaw(bodyByteArray);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.disposeUploadHandlerOnDispose = true;
            webRequest.disposeDownloadHandlerOnDispose = true;
            webRequest.method = UnityWebRequest.kHttpVerbPOST;
            
            yield return webRequest.SendWebRequest();
            
            LogRequestResult(body, webRequest);
            
            T response;
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                response = new T().FromJson(webRequest.downloadHandler.text);
                response.Result = webRequest.result;
            }
            else
            {
                response = new T
                {
                    Result = webRequest.result
                };
            }
            
            webRequest.Dispose();
            
            completionCallback(response);
        }

        private void LogRequestResult(string body, UnityWebRequest request)
        {
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError: Log(); break;
                case UnityWebRequest.Result.DataProcessingError: Log(); break;
                case UnityWebRequest.Result.ProtocolError: Log(); break;
            }
            
            void Log()
            {
                Debug.LogError(
                    "Method: " + request.method + "\n" + 
                    "URL: " + request.uri + ": \n" +
                    "body: " + body.Take(10000) + "..." + ": \n\n" +
                    "result: " + request.result + ": \n\n" +
                    "response: " + request.downloadHandler.text);
            }
        }
    }
    
    
    public interface IRequestable<T>
    {
        string URL { get; }
        UnityWebRequest.Result Result { set; get; }
        T FromJson(string jsonString);
    }
    
    [Serializable]
    public class AiText : IRequestable<AiText>
    {
        public string URL => "https://api.openai.com/v1/completions";

        public string Text => choices.Length > 0 ? choices[0].text : default;
            
        [Serializable]
        public class Request
        {
            public string prompt;
            public string model;
            public int n;
            public float temperature;
            public int max_tokens;
            
            private const int defaultMaxTokens = 1000;

            public Request(string prompt, string model, int n=1, float temperature=.8f, int max_tokens=defaultMaxTokens)
            {
                this.prompt = prompt;
                this.model = model;
                this.temperature = temperature;
                this.n = n;
                this.max_tokens = max_tokens;
            }

            public Request(string prompt, OpenAiApi.Model model, int n=1, float temperature=.8f, int max_tokens=defaultMaxTokens)
            {
                this.prompt = prompt;
                this.model = OpenAiApi.ModelToString[model];
                this.temperature = temperature;
                this.n = n;
                this.max_tokens = max_tokens;
            }
        }
            
        [Serializable]
        public class Choice
        {
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
            
        public string id;
        public string obj;
        public int created;
        public string model;
        public Choice[] choices = new Choice[] {};
        public Usage usage;

        public UnityWebRequest.Result Result { get; set; }

        public AiText()
        {
            choices = new [] { new Choice() };
        }
        
        public AiText FromJson(string jsonString)
        {
            jsonString = jsonString.Replace("\"object\":", "\"obj\":"); //Have to replace object since it's a reserved word. 
            AiText aiText = JsonUtility.FromJson<AiText>(jsonString);
            foreach (var choice in aiText.choices)
            {
                choice.text = choice.text.Trim();
            }
            return aiText;
        }
    }
    
    [Serializable]
    public class AiImage : IRequestable<AiImage>
    {
        public string URL => "https://api.openai.com/v1/images/generations";

        public Texture2D Texture => data.Length > 0 ? data[0].texture : default;

            [Serializable]
        public class Request
        {
            public string prompt;
            public string size;
            public int n;
        
            public Request(string prompt, string size="256x256", int n=1)
            {
                this.prompt = prompt;
                this.size = size;
                this.n = n;
            }
        
            public Request(string prompt, OpenAiApi.Size size=OpenAiApi.Size.SMALL, int n=1)
            {
                this.prompt = prompt;
                this.size = OpenAiApi.SizeToString[size];
                this.n = n;
            }
        }
        
        [Serializable]
        public class Data
        {
            public string url;
            public Texture2D texture;
        }
        
        public int created;
        public Data[] data = new Data[]{};

        public UnityWebRequest.Result Result { get; set; }

        public AiImage()
        {
            data = new [] { new Data
                { url = "", texture = null } 
            };
        }
            
        public AiImage FromJson(string jsonString)
        {
            return JsonUtility.FromJson<AiImage>(jsonString);
        }
    }
}
