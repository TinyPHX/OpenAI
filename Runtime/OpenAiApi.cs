using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyBox;
using OpenAI;
using OpenAI.AiModels;
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
        private Configuration config;
        private bool verbose = true;
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

        public OpenAiApi(Configuration config = null, bool verbose = true)
        {
            this.config = config;
            this.verbose = verbose;
        }

        public Configuration ActiveConfig => config ?? OpenAi.Configuration.GlobalConfig;

        public string ApiKey
        {
            get
            {
                if (OpenAi.Configuration.GlobalConfig == null || OpenAi.Configuration.GlobalConfig.ApiKey == "")
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
                if (OpenAi.Configuration.GlobalConfig == null || OpenAi.Configuration.GlobalConfig.Organization == "")
                {
                    OpenAi.Configuration.GlobalConfig = ReadConfigFromUserDirectory();
                }
                return ActiveConfig.Organization;
            }
        }
        
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
        
        public Task<AiText> Send(AiTextRequest request, Callback<AiText> callback=null)
        {
            return Post(request, callback);
        }
        
        public Task<AiChat> Send(AiChatRequest request, Callback<AiChat> callback=null)
        {
            return Post(request, callback);
        }

        public Task<AiImage> Send(AiImageRequest request, Callback<AiImage> callback=null)
        {
            return CreateImage(request, callback);
        }

        #region Completions
        
        public Task<AiText> TextCompletion(string prompt, Callback<AiText> callback)
        {
            return Post(new AiTextRequest{prompt=prompt}, callback);
        }
        
        public Task<AiText> TextCompletion(string prompt, Models.Text model, Callback<AiText> callback)
        {
            return Post(new AiTextRequest{prompt=prompt}, callback);
        }
        
        public Task<AiText> TextCompletion(string prompt, Models.Text model=Models.Text.GPT_3, int n=1, float temperature=.8f, int max_tokens=100, Callback<AiText> callback=null)
        {
            return Post(new AiTextRequest
            {
                prompt = prompt, 
                model = model,
                n = n,
                temperature = temperature,
                max_tokens = max_tokens
            }, callback);
        }
        
        public Task<AiChat> ChatCompletion(Message[] messages, Callback<AiChat> callback=null)
        {
            return Post(new AiChatRequest{messages=messages}, callback);
        }
        
        public Task<AiChat> ChatCompletion(Message[] messages, Models.Chat model, Callback<AiChat> callback=null)
        {
            return Post(new AiChatRequest{messages=messages, model=model}, callback);
        }
        
        public Task<AiChat> ChatCompletion(Message[] messages, Models.Chat model=Models.Chat.GPT_4, int n=1, float temperature=.8f, int max_tokens=100, Callback<AiChat> callback=null)
        {
            return Post(new AiChatRequest
            {
                messages = messages, 
                model = model,
                n = n,
                temperature = temperature,
                max_tokens = max_tokens
            }, callback);
        }
        
        #endregion

        #region Images

        public Task<AiImage> CreateImage(string prompt, Callback<AiImage> callback)
        {
            return CreateImage(new AiImageRequest { prompt=prompt }, callback);
        }

        public Task<AiImage> CreateImage(string prompt, ImageSize size, Callback<AiImage> callback)
        {
            return CreateImage(new AiImageRequest { prompt=prompt, size=size }, callback);
        }

        public Task<AiImage> CreateImage(string prompt, ImageSize size, int n=1, Callback<AiImage> callback=null)
        {
            return CreateImage(new AiImageRequest { prompt=prompt, size=size, n=n}, callback);
        }

        public Task<AiImage> CreateImage(AiImageRequest request, Callback<AiImage> callback=null)
        {
            callback ??= value => {  };
            var taskCompletion = new TaskCompletionSource<AiImage>();
            Callback<AiImage> callbackIntercept = async image =>
            {
                Texture2D[] textures = await GetAllImages(image);
                for (int i = 0; i < textures.Length; i++)
                {
                    ImageData data = image.data[i];

                    Texture2D texture = textures[i];
                    if (OpenAi.Configuration.SaveTempImages)
                    {
                        string num = i > 0 ? (" " + i) : "";
                        texture = AiUtils.Image.SaveImageToFile(request.prompt + num, texture, false, AiUtils.Image.TempImageDirectory);
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
        
        private Task<Texture2D[]> GetAllImages(AiImage aiAiImage)
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
        
        public Task<O> Post<I,O>(I request, Callback<O> completionCallback = null)
            where I : ModelRequest<I>
            where O : ModelResponse<O>, new()
        {
            if (verbose)
            {
                Debug.Log($"Open AI API - Request Sent: \"{request.ToJson()}\"");                
            }
            
            completionCallback ??= value => {  };

            (Task<O> task, Callback<O> taskCallback) = CallbackToTask(completionCallback);
            Runner.StartCoroutine(Post(request.Url, request.ToJson(), taskCallback));

            return task;
        }

        private IEnumerator Post<O>(string url, string body, Callback<O> completionCallback) where O : ModelResponse<O>, new()
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
            
            O response;
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                response = new O().FromJson(webRequest.downloadHandler.text);
                response.Result = webRequest.result;
                
                if (verbose)
                {
                    Debug.Log($"Open AI API - Request Successful: \"{JsonUtility.ToJson(JsonUtility.FromJson<O>(webRequest.downloadHandler.text), true)}\"");
                }
            }
            else
            {
                response = new O
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
                    "body: " + body.Take(1000) + "..." + ": \n\n" +
                    "result: " + request.result + ": \n\n" +
                    "response: " + request.downloadHandler.text);
            }
        }
    }
}
