using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;

namespace OpenAI.AiModels
{
    public static class Endpoints
    {
        public const string Completion = "https://api.openai.com/v1/completions";
        public const string ChatCompletion = "https://api.openai.com/v1/chat/completions";
        public const string Edits = "https://api.openai.com/v1/edits";
        public const string ImageGenerations = "https://api.openai.com/v1/images/generations";
        public const string ImageEdits = "https://api.openai.com/v1/images/edits";
        public const string ImageVariations = "https://api.openai.com/v1/images/variations";
    }

    public static class ModelTypes
    {
        public enum ImageGenerations
        {
            DALL_E
        }
        
        public enum ImageEdits
        {
            DALL_E_EDIT
        }
        
        public enum ImageVariations
        {
            DALL_E_VARIATION
        }

        public enum ChatCompletion
        {
            GPT_3_5_TURBO,
            GPT_3_5_TURBO_0301,
            GPT_4,
            GPT_4_0314,
        }

        public static readonly Dictionary<Enum, string> ChatCompletionToModelString = new Dictionary<Enum, string>()
        {
            { ChatCompletion.GPT_3_5_TURBO, "gpt-3.5-turbo" },
            { ChatCompletion.GPT_3_5_TURBO_0301, "gpt-3.5-turbo-0301" },
            { ChatCompletion.GPT_4, "gpt-4" },
            { ChatCompletion.GPT_4_0314, "gpt-4-0314" }
        };

        public enum TextCompletion
        {
            GPT_3,
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
        
        public static readonly Dictionary<Enum, string> TextCompletionToModelString = new Dictionary<Enum, string>()
        {            
            { TextCompletion.GPT_3, "text-davinci-003" },
            { TextCompletion.ADA, "ada" },
            { TextCompletion.ADA_CODE_SEARCH_CODE, "ada-code-search-code" },
            { TextCompletion.ADA_CODE_SEARCH_TEXT, "ada-code-search-text" },
            { TextCompletion.ADA_SEARCH_DOCUMENT, "ada-search-document" },
            { TextCompletion.ADA_SEARCH_QUERY, "ada-search-query" },
            { TextCompletion.ADA_SIMILARITY, "ada-similarity" },
            { TextCompletion.ADA_2020_05_03, "ada:2020-05-03" },
            { TextCompletion.BABBAGE, "babbage" },
            { TextCompletion.BABBAGE_CODE_SEARCH_CODE, "babbage-code-search-code" },
            { TextCompletion.BABBAGE_CODE_SEARCH_TEXT, "babbage-code-search-text" },
            { TextCompletion.BABBAGE_SEARCH_DOCUMENT, "babbage-search-document" },
            { TextCompletion.BABBAGE_SEARCH_QUERY, "babbage-search-query" },
            { TextCompletion.BABBAGE_SIMILARITY, "babbage-similarity" },
            { TextCompletion.BABBAGE_2020_05_03, "babbage:2020-05-03" },
            { TextCompletion.CODE_SEARCH_ADA_CODE_001, "code-search-ada-code-001" },
            { TextCompletion.CODE_SEARCH_ADA_TEXT_001, "code-search-ada-text-001" },
            { TextCompletion.CODE_SEARCH_BABBAGE_CODE_001, "code-search-babbage-code-001" },
            { TextCompletion.CODE_SEARCH_BABBAGE_TEXT_001, "code-search-babbage-text-001" },
            { TextCompletion.CURIE, "curie" },
            { TextCompletion.CURIE_INSTRUCT_BETA, "curie-instruct-beta" },
            { TextCompletion.CURIE_SEARCH_DOCUMENT, "curie-search-document" },
            { TextCompletion.CURIE_SEARCH_QUERY, "curie-search-query" },
            { TextCompletion.CURIE_SIMILARITY, "curie-similarity" },
            { TextCompletion.CURIE_2020_05_03, "curie:2020-05-03" },
            { TextCompletion.CUSHMAN_2020_05_03, "cushman:2020-05-03" },
            { TextCompletion.DAVINCI, "davinci" },
            { TextCompletion.DAVINCI_IF_3_0_0, "davinci-if:3.0.0" },
            { TextCompletion.DAVINCI_INSTRUCT_BETA, "davinci-instruct-beta" },
            { TextCompletion.DAVINCI_INSTRUCT_BETA_2_0_0, "davinci-instruct-beta:2.0.0" },
            { TextCompletion.DAVINCI_SEARCH_DOCUMENT, "davinci-search-document" },
            { TextCompletion.DAVINCI_SEARCH_QUERY, "davinci-search-query" },
            { TextCompletion.DAVINCI_SIMILARITY, "davinci-similarity" },
            { TextCompletion.DAVINCI_2020_05_03, "davinci:2020-05-03" },
            { TextCompletion.TEXT_ADA_001, "text-ada-001" },
            { TextCompletion.TEXT_ADA__001, "text-ada:001" },
            { TextCompletion.TEXT_BABBAGE_001, "text-babbage-001" },
            { TextCompletion.TEXT_BABBAGE__001, "text-babbage:001" },
            { TextCompletion.TEXT_CURIE_001, "text-curie-001" },
            { TextCompletion.TEXT_CURIE__001, "text-curie:001" },
            { TextCompletion.TEXT_DAVINCI_001, "text-davinci-001" },
            { TextCompletion.TEXT_DAVINCI_002, "text-davinci-002" },
            { TextCompletion.TEXT_DAVINCI_003, "text-davinci-003" },
            { TextCompletion.TEXT_DAVINCI_EDIT_001, "text-davinci-edit-001" },
            { TextCompletion.TEXT_DAVINCI_INSERT_001, "text-davinci-insert-001" },
            { TextCompletion.TEXT_DAVINCI_INSERT_002, "text-davinci-insert-002" },
            { TextCompletion.TEXT_DAVINCI__001, "text-davinci:001" },
            { TextCompletion.TEXT_EMBEDDING_ADA_002, "text-embedding-ada-002" },
            { TextCompletion.TEXT_SEARCH_ADA_DOC_001, "text-search-ada-doc-001" },
            { TextCompletion.TEXT_SEARCH_ADA_QUERY_001, "text-search-ada-query-001" },
            { TextCompletion.TEXT_SEARCH_BABBAGE_DOC_001, "text-search-babbage-doc-001" },
            { TextCompletion.TEXT_SEARCH_BABBAGE_QUERY_001, "text-search-babbage-query-001" },
            { TextCompletion.TEXT_SEARCH_CURIE_DOC_001, "text-search-curie-doc-001" },
            { TextCompletion.TEXT_SEARCH_CURIE_QUERY_001, "text-search-curie-query-001" },
            { TextCompletion.TEXT_SEARCH_DAVINCI_DOC_001, "text-search-davinci-doc-001" },
            { TextCompletion.TEXT_SEARCH_DAVINCI_QUERY_001, "text-search-davinci-query-001" },
            { TextCompletion.TEXT_SIMILARITY_ADA_001, "text-similarity-ada-001" },
            { TextCompletion.TEXT_SIMILARITY_BABBAGE_001, "text-similarity-babbage-001" },
            { TextCompletion.TEXT_SIMILARITY_CURIE_001, "text-similarity-curie-001" },
            { TextCompletion.TEXT_SIMILARITY_DAVINCI_001, "text-similarity-davinci-001" },
        };

        public enum TextEdit
        {
            TEXT_DAVINCI_EDIT_001,
            CODE_DAVINCI_EDIT_001
        }

        public static readonly Dictionary<Enum, string> TextEditToModelString = new Dictionary<Enum, string>
        {
            { TextEdit.TEXT_DAVINCI_EDIT_001, "text-davinci-edit-001" },
            { TextEdit.CODE_DAVINCI_EDIT_001, "code-davinci-edit-001" }
        };

        public enum Audio
        {
            WHISPER_1
        }

        public static readonly Dictionary<Enum, string> AudioToModelString = new Dictionary<Enum, string>
        {
            { Audio.WHISPER_1, "whisper-1" },
        };

        public enum Other
        {
            IF_CURIE_V2,
            IF_DAVINCI_V2,
            IF_DAVINCI_3_0_0
        }
    }
    
    public static class AiModelJson
    {
        public static string ReplaceEnum<TEnum>(string json, string name, Dictionary<Enum, string> mapping) where TEnum : struct, Enum
        {
            Regex regex = new Regex($"\"{name}\": (\\d)+");
            MatchCollection matches = regex.Matches(json);
            List<Match> matchList = matches.Cast<Match>().OrderByDescending(m => m.Index).ToList();
            
            foreach (Match match in matchList)
            {
                Group group = match.Groups[1];
                string before = json.Substring(0, group.Index);
                Enum.TryParse(group.Value, out TEnum valueEnum);
                string newValue = $"\"{mapping[valueEnum]}\"";
                string after = json.Substring(group.Index + group.Length, json.Length - group.Index - group.Length);
            
                json = before + newValue + after;
            }
            
            return json;
        }
    }
    
    public static class AiModelDefaults
    {
        public static readonly string prompt = "";
        public static readonly Message[] messages = new [] { new Message("") };
        public static readonly int n = 1;
        public static readonly int max_tokens = 1000;
        public static readonly float temperature = .8f;
        public static readonly string size = "256x256";
    }
    
    #region Request Types
    
    [Serializable]
    public class ModelRequest<T> where T : class
    {
        public virtual string Url { get; }
        
        public virtual string ToJson()
        {
            return JsonUtility.ToJson(this as T, true);
        }
    }
    
    // https://platform.openai.com/docs/api-reference/completions/create
    [Serializable]
    public class CompletionRequest : ModelRequest<CompletionRequest>
    {
        public override string Url => Endpoints.Completion;
        
        public ModelTypes.TextCompletion model = ModelTypes.TextCompletion.GPT_3;
        public string prompt = AiModelDefaults.prompt;
        public int n = AiModelDefaults.n;
        public int max_tokens = AiModelDefaults.max_tokens;
        public float temperature = AiModelDefaults.temperature;
        
        public override string ToJson()
        {
            string json = JsonUtility.ToJson(this, true);
            json = AiModelJson.ReplaceEnum<ModelTypes.TextCompletion>(json, nameof(model), ModelTypes.TextCompletionToModelString);
            return json;
        }
    }

    [Serializable]
    public class FullCompletionRequest : ModelRequest<FullCompletionRequest>
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
    
    [Serializable]
    public class ChatCompletionRequest : ModelRequest<ChatCompletionRequest> 
    {
        public override string Url => Endpoints.ChatCompletion;

        public ModelTypes.ChatCompletion model = ModelTypes.ChatCompletion.GPT_4;
        public Message[] messages = AiModelDefaults.messages;
        public int n = AiModelDefaults.n;
        public float temperature = AiModelDefaults.temperature;
        public int max_tokens = AiModelDefaults.max_tokens;
        
        public override string ToJson()
        {
            string json = JsonUtility.ToJson(this, true);
            json = AiModelJson.ReplaceEnum<ModelTypes.ChatCompletion>(json, nameof(model), ModelTypes.ChatCompletionToModelString);
            json = AiModelJson.ReplaceEnum<Message.Role>(json, nameof(Message.role), Message.RoleToString);
            return json;
        }
    }
    
    [Serializable]
    public class ImageGenerationRequest : ModelRequest<ImageGenerationRequest>
    {
        public override string Url => Endpoints.ImageGenerations;
        
        public string prompt = AiModelDefaults.prompt;
        public ImageSize size;
        public int n = AiModelDefaults.n;
        
        public override string ToJson()
        {
            string json = JsonUtility.ToJson(this, true);
            json = AiModelJson.ReplaceEnum<ImageSize>(json, nameof(size), ImageSizeToString);
            return json;
        }
        
        private static readonly Dictionary<Enum, string> ImageSizeToString = new Dictionary<Enum, string>()
        {
            { ImageSize.SMALL, "256x256" }, 
            { ImageSize.MEDIUM, "512x512" }, 
            { ImageSize.LARGE, "1024x1024" }
        };
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
        
        public UnityWebRequest.Result Result { get; set; }
    }

    [Serializable]
    public class CompletionResponse : ModelResponse<CompletionResponse>
    {
        public string Text => choices.Length > 0 ? choices[0].text : "";
        
        public string id;
        public string obj;
        public int created;
        public ModelTypes.TextCompletion model;
        public Choice[] choices = new Choice[] {};
        public Usage usage;
    }

    [Serializable]
    public class ChatCompletionResponse : ModelResponse<ChatCompletionResponse>
    {
        public string Text => choices.Length > 0 ? choices[0].message.content : "";
        
        public string id;
        public string obj;
        public int created;
        public ModelTypes.ChatCompletion model;
        public MessageChoice[] choices = new MessageChoice[] {};
        public Usage usage;
    }

    [Serializable]
    public class ImageGenerationResponse : ModelResponse<ImageGenerationResponse>
    {
        public Texture2D Texture => data.Length > 0 ? data[0].texture : default;
        
        public int created;
        public ImageData[] data = new ImageData[]{};
    }

    #endregion Response Types

    #region Request/Response Dependency Types
    
    public enum ImageSize { SMALL, MEDIUM, LARGE };

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
            SYSTEM,
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
        public int index = 0;
        public string logprobs;
        public string finish_reason;
    }
    
    
    [Serializable]
    public class ImageData
    {
        public string url;
        public Texture2D texture;
    }

    #endregion
}