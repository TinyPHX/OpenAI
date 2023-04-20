using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            return TextCompletion(request, callback);
        }
        
        public Task<AiChat> Send(AiChatRequest request, Callback<AiChat> callback=null)
        {
            return Post(request, callback);
        }

        public Task<AiImage> Send(AiImageRequest request, Callback<AiImage> callback=null)
        {
            return CreateImage(request, callback);
        }

        public Task<AiImage> Send(AiImageEditRequest request, Callback<AiImage> callback=null)
        {
            return CreateImageEdit(request, callback);
        }

        public Task<AiImage> Send(AiImageVariationRequest aiImageVariationRequest, Callback<AiImage> callback=null)
        {
            return CreateImageVariant(aiImageVariationRequest, callback);
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
        
        public Task<AiText> TextCompletion(string prompt, Models.Text model=Models.Text.GPT_3, int n=1, float temperature=.8f, int max_tokens=100, bool stream=false, Callback<AiText> callback=null)
        {
            return TextCompletion(new AiTextRequest
            {
                prompt = prompt, 
                model = model,
                n = n,
                temperature = temperature,
                max_tokens = max_tokens,
                stream = stream
            }, callback);
        }

        public Task<AiText> TextCompletion(AiTextRequest request, Callback<AiText> callback=null)
        {
            return ResponseWrapper(request, callback);
        }
        
        public Task<AiChat> ChatCompletion(Message[] messages, Callback<AiChat> callback=null)
        {
            return Post(new AiChatRequest{messages=messages}, callback);
        }
        
        public Task<AiChat> ChatCompletion(Message[] messages, Models.Chat model, Callback<AiChat> callback=null)
        {
            return Post(new AiChatRequest{messages=messages, model=model}, callback);
        }
        
        public Task<AiChat> ChatCompletion(Message[] messages, Models.Chat model=Models.Chat.GPT_4, int n=1, float temperature=.8f, int max_tokens=100, bool stream=false, Callback<AiChat> callback=null)
        {
            return ChatCompletion(new AiChatRequest
            {
                messages = messages, 
                model = model,
                n = n,
                temperature = temperature,
                max_tokens = max_tokens,
                stream = stream
            }, callback);
        }

        public Task<AiChat> ChatCompletion(AiChatRequest request, Callback<AiChat> callback=null)
        {
            return ResponseWrapper(request, callback);
        }
        
        #endregion

        #region Images

        // CreateImage
        
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
            return ResponseWrapper(request, callback);
        }

        // CreateImageEdit
        
        public Task<AiImage> CreateImageEdit(Texture2D image, Texture2D mask, string prompt, Callback<AiImage> callback)
        {
            return CreateImageEdit(new AiImageEditRequest { image=image, mask=mask, prompt=prompt }, callback);
        }

        public Task<AiImage> CreateImageEdit(Texture2D image, Texture2D mask, string prompt, ImageSize size, Callback<AiImage> callback)
        {
            return CreateImageEdit(new AiImageEditRequest { image=image, mask=mask, prompt=prompt, size=size }, callback);
        }

        public Task<AiImage> CreateImageEdit(Texture2D image, Texture2D mask, string prompt, ImageSize size, int n=1, Callback<AiImage> callback=null)
        {
            return CreateImageEdit(new AiImageEditRequest { image=image, mask=mask, prompt=prompt, size=size, n=n}, callback);
        }

        public Task<AiImage> CreateImageEdit(AiImageEditRequest request, Callback<AiImage> callback=null)
        {
            return ResponseWrapper(request, callback);
        }

        // CreateImageVariant
        
        public Task<AiImage> CreateImageVariant(Texture2D image, Callback<AiImage> callback)
        {
            return CreateImageVariant(new AiImageVariationRequest { image=image }, callback);
        }

        public Task<AiImage> CreateImageVariant(Texture2D image, ImageSize size, Callback<AiImage> callback)
        {
            return CreateImageVariant(new AiImageVariationRequest { image=image, size=size }, callback);
        }

        public Task<AiImage> CreateImageVariant(Texture2D image, ImageSize size, int n=1, Callback<AiImage> callback=null)
        {
            return CreateImageVariant(new AiImageVariationRequest { image=image, size=size, n=n}, callback);
        }

        public Task<AiImage> CreateImageVariant(AiImageVariationRequest request, Callback<AiImage> callback=null)
        {
            return ResponseWrapper(request, callback);
        }

        // Calls ResponseCallback on the modelRequest object so we can customize what hallpends post response for different types.
        public Task<O> ResponseWrapper<I,O>(I request, Callback<O> callback=null)
            where I : ModelRequest<I>
            where O : ModelResponse<O>, new()
        {
            callback ??= (value) => {  };
            var taskCompletion = new TaskCompletionSource<O>();
            Callback<O> callbackIntercept = async response =>
            {
                await response.ResponseCallback(request);
                
                callback(response);
                
                if (response.Result == UnityWebRequest.Result.Success)
                {
                    taskCompletion.SetResult(response);
                }
            };
            Post(request, callbackIntercept);
            return taskCompletion.Task;
        }
        
        #endregion
        
        public Task<O> Post<I,O>(I request, Callback<O> completionCallback = null)
            where I : ModelRequest<I>
            where O : ModelResponse<O>, new()
        {
            if (verbose)
            {
                Debug.Log($"Open AI API - Request Sent: \"{request.ToJson()}\"");                
            }
            
            completionCallback ??= (value) => {  };
            (Task<O> task, Callback<O> taskCallback) = ModelResponseCallbackToTask(completionCallback);
            if (request.UseForm)
            {
                Runner.StartCoroutine(PostForm(request.Url, request.ToForm(), request.Stream, taskCallback));
            }
            else
            {
                Runner.StartCoroutine(Post(request.Url, request.ToJson(), request.Stream, taskCallback));
            }

            return task;
        }

        private static Tuple<Task<T>, Callback<T>> ModelResponseCallbackToTask<T>(Callback<T> callback=null)
            where T : ModelResponse<T>
        {
            var taskCompletion = new TaskCompletionSource<T>();
            callback ??= (value) => {  };
            Callback<T> wrappedCallback = value =>
            {
                if (value.Result == UnityWebRequest.Result.Success)
                {
                    taskCompletion.SetResult(value);
                }

                callback(value);
            };

            return new Tuple<Task<T>, Callback<T>>(taskCompletion.Task, wrappedCallback);
        }
        
        private IEnumerator Post<O>(string url, string body, bool stream, Callback<O> completionCallback) where O : ModelResponse<O>, new()
        {
            O response = null;
            
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
            if (stream)
            {
                webRequest.downloadHandler = new OpenAiDownloadHandler<O>(streamResponse =>
                {
                    response = streamResponse;
                    response.Result = UnityWebRequest.Result.InProgress;
                
                    if (verbose)
                    {
                        Debug.Log($"Open AI API - Stream Request Successful: \"{JsonUtility.ToJson(response, true)}\"");
                    }
                    
                    completionCallback(response);
                }, errorCallback =>
                {
                    Debug.LogWarning(errorCallback);
                });
            }
            else
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();
            }
            webRequest.disposeUploadHandlerOnDispose = true;
            webRequest.disposeDownloadHandlerOnDispose = true;
            webRequest.method = UnityWebRequest.kHttpVerbPOST;
            
            yield return webRequest.SendWebRequest();
            
            LogRequestResult(body, webRequest);

            if (stream)
            {
                completionCallback(response);
            }
            else
            {
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    response = JsonUtility.FromJson<O>(webRequest.downloadHandler.text);
                    response.Result = webRequest.result;

                    if (verbose)
                    {
                        Debug.Log($"Open AI API - Request Successful: \"{JsonUtility.ToJson(response, true)}\"");
                    }
                }
                else
                {
                    response = new O
                    {
                        Result = webRequest.result
                    };
                
                    if (verbose)
                    {
                        Debug.LogWarning("Web request not successful: " +webRequest.result + ": " + webRequest.error);
                    }
                }
            
                completionCallback(response);
            }
                
            webRequest.Dispose();
        }

        private IEnumerator PostForm<O>(string url, WWWForm form, bool stream, Callback<O> completionCallback) where O : ModelResponse<O>, new()
        {
            O response = null;
            
            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);
                
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "Authorization", "Bearer " + ApiKey },
                { "OpenAI-Organization", Organization },
            };
            
            foreach (var entry in headers)
            {
                webRequest.SetRequestHeader(entry.Key, entry.Value);
            }
            
            foreach (var entry in form.headers)
            {
                webRequest.SetRequestHeader(entry.Key, entry.Value);
            }   
            
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.disposeUploadHandlerOnDispose = true;
            webRequest.disposeDownloadHandlerOnDispose = true;
            webRequest.method = UnityWebRequest.kHttpVerbPOST;
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                response = JsonUtility.FromJson<O>(webRequest.downloadHandler.text);
                response.Result = webRequest.result;
            
                if (verbose)
                {
                    Debug.Log($"Open AI API - Request Successful: \"{JsonUtility.ToJson(response, true)}\"");
                }
            }
            else
            {
                response = new O
                {
                    Result = webRequest.result
                };
                
                if (verbose)
                {
                    Debug.LogWarning("Web request not successful: " +webRequest.result + ": " + webRequest.error);
                }
            }
            
            completionCallback(response);
                
            webRequest.Dispose();
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
                    "body: " + body.Substring(0, Mathf.Min(body.Length, 1000)) + "..." + ": \n\n" +
                    "result: " + request.result + ": \n\n" +
                    "response: " + request.downloadHandler.text);
            }
        }
    }

    public class OpenAiDownloadHandler<T> : DownloadHandlerScript where T : ModelResponse<T> {
        public delegate void Callback(T response);
        private Callback streamCallback;
        public delegate void ErrorCallback(string response);
        private ErrorCallback errorCallback;
        private T combinedResult = null;

        // Standard scripted download handler - allocates memory on each ReceiveData callback
        public OpenAiDownloadHandler(Callback streamCallback, ErrorCallback errorCallback = null): base()
        {
            this.streamCallback = streamCallback;
            this.errorCallback = errorCallback;
        }

        // Pre-allocated scripted download handler
        // reuses the supplied byte array to deliver data.
        // Eliminates memory allocation.
        public OpenAiDownloadHandler(byte[] buffer): base(buffer) {
        }

        protected override byte[] GetData() { return null; }

        // Called once per frame when data has been received from the network.
        protected override bool ReceiveData(byte[] data, int dataLength) {
            if(data == null || data.Length < 1) {
                return false;
            }

            string text = System.Text.Encoding.UTF8.GetString(data);
            string[] textArray = text.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            string dataLabel = "data: ";
            string dataCompleteTag = "[DONE]";

            bool error = false;
            string errorMessage = "";
            
            foreach (string textEntry in textArray)
            {
                if (textEntry.StartsWith(dataLabel) && !error)
                {
                    string streamText = textEntry.Substring(dataLabel.Length);

                    if (streamText != dataCompleteTag)
                    {
                        T streamResponse = JsonUtility.FromJson<T>(streamText);
                        if (combinedResult == null)
                        {
                            combinedResult = streamResponse;
                        }
                        else
                        {
                            combinedResult = combinedResult.AppendStreamResult(streamResponse);
                        }
                    }
                }
                else
                {
                    error = true;
                    errorMessage += textEntry + "\n";
                }
            }

            if (!error)
            {
                streamCallback(combinedResult);
            }
            else
            {
                errorCallback(errorMessage);
            }

            return true;
        }
    }
}
