using System;
using System.IO;
using MyBox;
using OpenAI.AiModels;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace OpenAi
{
    [ExecuteInEditMode, Serializable]
    public class OpenAiComponent : MonoBehaviour
    {
        #if UNITY_EDITOR
        public enum FileType
        {
            SHADER,
            CS,
            SCENE,
            PREFAB,
            SCRIPTABLE_OBJECT,
            MATERIAL,
            OBJ
        }
        
        [TextArea(1,20)] public string prompt;
        
        [Separator("Advanced")]
        [OverrideLabel("")] public bool advanced;
        [ConditionalField(nameof(advanced))] public string scriptName;
        [ConditionalField(nameof(advanced))] public bool useOpenAiMonoBehaviour = true; 
        [ConditionalField(nameof(advanced)), TextArea(1,20)] public string prePrompt = "Create unity component using the following user prompt: ";
        [ConditionalField(nameof(advanced)), TextArea(1,20)] public string postPrompt = "" +
            "Make sure to use UnityEngine PropertyAttributes to make the UI user friendly. Also this needs to be compatible with " +
            "Unity version {unity_version}. Lastly, the class name should be {script_name}. Don't ever try to load anything from the " +
            "resources folder unless the user prompt specifically calls for it. Add public fields for anything needed by the components. " +
            "Make sure to include tooltip attributes when appropriate. Only include code that compiles in your response.";

        [ConditionalField(nameof(advanced)), TextArea(1, 20)] public string editPrePrompt = "Edit this script \n\nEdit: ";
        [ConditionalField(nameof(advanced)), TextArea(1,20)] public string editPostPrompt = "\n\nScript:\n";

        private readonly string namePrePrompt = "What's a good name for a C# Unity component that was generated using this prompt: ";
        private readonly string namePostPrompt = "Don't include any punctuation or file extensions";

        [Separator("Edits")] 
        public MonoScript script;
        public OpenAiMonoBehaviour scriptInstance;
        [SerializeField, HideInInspector] private bool canEdit = false;
        [ConditionalField(nameof(canEdit)), TextArea(1,20)] public string editPrompt = "";

        private MonoScript previousScript;
        private OpenAiMonoBehaviour previousScriptInstance;
        [HideInInspector] public string addOnReload = "";

        public bool CanEdit
        {
            get
            {
                canEdit = script != null;
                return canEdit;
            }
        }

        public void Update()
        {
            if (previousScript != script && script != default && script.GetClass() != null)
            {
                previousScript = script;
                if (script.GetClass().IsSubclassOf(typeof(OpenAiMonoBehaviour)) || script.GetClass() == typeof(OpenAiMonoBehaviour))
                {
                    scriptInstance = GetComponent(script.GetClass()) as OpenAiMonoBehaviour;
                }
                else
                {
                    script = null;
                    previousScript = script;
                }
            }
            else if (previousScriptInstance != scriptInstance)
            {
                previousScriptInstance = scriptInstance;
                script = MonoScript.FromMonoBehaviour(scriptInstance);
            }

            canEdit = script != null;
        }

        public delegate void Callback();
        public async void CreateComponent(Callback callback)
        {
            OpenAiApi openAi = new OpenAiApi();
            
            if (scriptName.IsNullOrEmpty())
            {
                var nameCompletion = await openAi.TextCompletion(namePrePrompt + " `" + prompt + "` " + namePostPrompt);
                scriptName = nameCompletion.Text;
            }
            scriptName = string.Concat(
                scriptName
                    .Replace(".cs", "")
                    .Replace(".", "")
                    .Split(Path.GetInvalidFileNameChars()));

            string postPromptWithVars = postPrompt
                .Replace("{unity_version}", Application.unityVersion)
                .Replace("{script_name}", scriptName);
            string fullPrompt = prePrompt + " " + prompt + " " + postPromptWithVars;

            var request = new AiTextRequest
            {
                prompt = fullPrompt, 
                model = Models.Text.TEXT_DAVINCI_003, 
                max_tokens = 2048
            };
            var codeCompletion = await openAi.Send(request);

            if (codeCompletion.Result == UnityWebRequest.Result.Success)
            {
                string scriptContents = codeCompletion.Text;
                
                //Weird hack for openAi returing code that starts with "'". Don't ask my why. 
                scriptContents = scriptContents.StartsWith(".") ? scriptContents.Substring(1) : scriptContents;
                scriptContents = scriptContents.Trim();

                string monoBehaviourSearch = " : " + nameof(MonoBehaviour);
                string openAiMonoBehaviourReplace = " : " + nameof(OpenAiMonoBehaviour);
                if (useOpenAiMonoBehaviour && scriptContents.Contains(monoBehaviourSearch))
                {
                    scriptContents = "using OpenAiMonoBehaviour = OpenAi.OpenAiMonoBehaviour;\n" +
                                     scriptContents.Replace(monoBehaviourSearch, openAiMonoBehaviourReplace);
                }

                string unityEngineUsing = "using UnityEngine;";
                if (!scriptContents.Contains(unityEngineUsing))
                {
                    scriptContents = unityEngineUsing + "\n" + scriptContents;
                }
                
                addOnReload = scriptName;
                script = AiUtils.Script.CreateScript(scriptName, scriptContents);
                callback();
            }
        }
        
        //TODO
        // public int GetErrorCount()
        // {
        //     if (Application.isBatchMode)
        //     {
        //         CompilationPipeline.assemblyCompilationFinished += ProcessBatchModeCompileFinish;
        //     }
        //
        //     void ProcessBatchModeCompileFinish(string s, CompilerMessage[] compilerMessages)
        //     {
        //         if (compilerMessages.Count(m => m.type == CompilerMessageType.Error) > 0) 
        //             EditorApplication.Exit(-1);
        //     }
        // }
        
        #endif
    }
}