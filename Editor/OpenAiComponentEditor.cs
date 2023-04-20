using System.IO;
using System.Linq;
using System;
using Ardenfall.UnityCodeEditor;
using MyBox;
using OpenAI.AiModels;
using TP.ExtensionMethods;
using uCodeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using CodeEditor = Ardenfall.UnityCodeEditor.CodeEditor;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiComponent))]
    public class OpenAiComponentEditor : EditorWidowOrInspector<OpenAiComponentEditor>
    {
        private OpenAiComponent openAiComponent;
        private string codeWithEdits;
        private float activeWidth = 0;
        private CodeEditor codeEditor;
        
        [SerializeField] private bool[] foldoutStates = new bool[] { };
        private static bool scriptsDirty = false; 
        
        public override void OnInspectorGUI()
        {
            openAiComponent = target as OpenAiComponent;

            if (AiEditorUtils.ScaledWidth < 500 || !IsPrefab(openAiComponent))
            {
                NarrowLayout();
            }
            else
            {
                WideLayout();   
            }
        }
        
        void NarrowLayout()
        {
            activeWidth = AiEditorUtils.ScaledWidth - 25;
            DrawGroup1();
        }

        void WideLayout()
        {
            activeWidth = AiEditorUtils.ScaledWidth / 2f - 35;
            EditorGUIUtility.labelWidth = AiEditorUtils.ScaledWidth / 5;
            AiEditorUtils.Horizontal(() => {
                AiEditorUtils.Vertical(() => {
                    DrawGroup1();
                }, GUILayout.Width(activeWidth));
                GUILayout.Space(20);
                AiEditorUtils.Vertical(() => {
                    DrawGroup2();
                }, GUILayout.Width(activeWidth));
            });
        }
        
        void DrawGroup1()
        {
            AddNewScript(openAiComponent);
            
            var property = serializedObject.GetIterator();
            var expanded = true;
            while (property.NextVisible(expanded))
            {
                // Don't draw script field for built-in types
                if ("prompt" == property.propertyPath)
                {
                    EditorGUILayout.LabelField("Prompt");
                    EditorStyles.textField.wordWrap = true;
                    string updatedPrompt = EditorGUILayout.TextArea(openAiComponent.prompt);

                    if (!openAiComponent.CanEdit)
                    {
                        openAiComponent.prompt = updatedPrompt;
                    }
                }
                else if ("m_Script" == property.propertyPath)
                {
                    AiEditorUtils.Disable(true, () =>
                    {
                        EditorGUILayout.PropertyField(property);
                    });
                }
                else
                {
                    EditorGUILayout.PropertyField(property, expanded);
                    serializedObject.ApplyModifiedProperties();
                }

                expanded = false;
            }
            
            if (!openAiComponent.CanEdit)
            {
                if (GUILayout.Button("Create Component"))
                {
                    if (!AiEditorUtils.ApiKeyPromptCheck())
                    {
                        openAiComponent.CreateComponent(() =>
                        {
                            EditorUtility.SetDirty(this);
                        });
                        GUI.FocusControl("");
                    }
                }
            }
            else
            {
                if (openAiComponent.scriptInstance)
                {
                    EditorGUILayout.LabelField("Edits:");
                    OpenAiMonoBehaviour.Edit[] edits = openAiComponent.scriptInstance.editsArray.edits;
                    while (foldoutStates.Length < edits.Length)
                    {
                        foldoutStates = foldoutStates.Append(false).ToArray();
                    }

                    while (foldoutStates.Length > edits.Length)
                    {
                        foldoutStates = foldoutStates.RemoveLast<bool>();
                    }

                    for (var index = 0; index < edits.Length; index++)
                    {
                        var edit = openAiComponent.scriptInstance.editsArray.edits[index];

                        int averageCharacterWidth = 7;
                        int characterLimit = (int)(activeWidth / averageCharacterWidth);
                        string foldoutName = edit.editPrompt;
                        foldoutName = foldoutName.Length <= characterLimit ? 
                            foldoutName : 
                            foldoutName.Substring(0, characterLimit) + "...";
                        foldoutStates[index] = AiEditorUtils.Foldout(foldoutStates[index], foldoutName, () =>
                        {
                            EditorGUILayout.TextField("script", edit.script);
                            EditorGUILayout.TextField("editPrompt", edit.editPrompt);
                            EditorGUILayout.TextField("editedScript", edit.editedScript);
                        });
                    }
                }

                AiEditorUtils.Disable(openAiComponent.editPrompt.IsNullOrEmpty(), () =>
                {
                    if (GUILayout.Button("Create Edit"))
                    {
                        if (!AiEditorUtils.ApiKeyPromptCheck())
                        {
                            CreateEdit(openAiComponent);
                            GUI.FocusControl("");
                        }
                    }
                });
            }
        }

        void DrawGroup2()
        {
            EditorGUILayout.HelpBox(
                "This script generator is for experimental purposes only and should not be used in production " +
                "environments. When using this it's a good idea to backup often.",
                MessageType.Info
            );
            AiEditorUtils.Disable(true, () => {
                EditorGUILayout.LabelField("Code");
                EditorStyles.textField.wordWrap = true;
                if (openAiComponent.script)
                {
                    codeEditor = new CodeEditor("CodeEditor", new DefaultTheme());
                    codeEditor.highlighter = ShaderHighlighter.Highlight;
                    codeEditor.Draw(openAiComponent.script.text, new GUIStyle(EditorStyles.textArea));
                }
                else
                {
                    EditorGUILayout.TextArea("Enter a prompt and click 'Create Component.' This can also be attached to any GameObject!");
                }
            });
        }

        private async void CreateEdit(OpenAiComponent openAiComponent)
        {
            OpenAiApi openAi = new OpenAiApi();

            string scriptName = openAiComponent.script.name.Replace(".cs", "");
            
            // string postPromptWithVars = openAiComponent.postPrompt
            //     .Replace("{unity_version}", Application.unityVersion)
            //     .Replace("{script_name}", scriptName);
            
            string fullPrompt = 
                // openAiComponent.prePrompt + " " + openAiComponent.prompt + " " + postPromptWithVars + "\n" + 
                openAiComponent.editPrePrompt + " '" + openAiComponent.editPrompt + "' " + openAiComponent.editPostPrompt + "\n\n" + 
                openAiComponent.script.text + "\n\n\n Respond with the full the script and not just the edit. ";

            var request = new AiTextRequest{prompt=fullPrompt, model=Models.Text.TEXT_DAVINCI_003, max_tokens=2048};
            var codeCompletion = await openAi.Send(request);

            if (codeCompletion.Result == UnityWebRequest.Result.Success)
            {
                string scriptContents = codeCompletion.Text.Trim();
                string directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(openAiComponent.script));

                AiUtils.Script.CreateScript(scriptName, scriptContents, false, directory, true);
                
                AssetDatabase.Refresh();
                
                string script = openAiComponent.script.text;
                string editPrompt = openAiComponent.editPrompt;
                string editedScript = scriptContents;

                if (openAiComponent.scriptInstance)
                {
                    openAiComponent.scriptInstance.CreateEdit(script, editPrompt, editedScript);
                    openAiComponent.editPrompt = "";
                }
            }
        }

        private static void AddNewScript(OpenAiComponent openAiComponent)
        {
            if (scriptsDirty && (openAiComponent.addOnReload == "" || openAiComponent.script == default))
            {
                scriptsDirty = false;
            }

            if (openAiComponent.addOnReload != "" && scriptsDirty)
            {
                Type type = openAiComponent.script.GetClass();
                openAiComponent.addOnReload = "";
                scriptsDirty = false;
                if (type != null)
                {
                    if (openAiComponent.useOpenAiMonoBehaviour)
                    {
                        if (!IsPrefab(openAiComponent))
                        {
                            openAiComponent.scriptInstance = openAiComponent.gameObject.AddComponent(type) as OpenAiMonoBehaviour;
                        }
                        else
                        {
                            openAiComponent.gameObject.GetComponent<OpenAiMonoBehaviour>().BlowUp();;
                            openAiComponent.scriptInstance = openAiComponent.gameObject.AddComponent<OpenAiMonoBehaviour>();
                        }
                    }
                    else
                    {

                        if (!IsPrefab(openAiComponent))
                        {
                            openAiComponent.gameObject.AddComponent(type);
                        }
                        else
                        {
                            openAiComponent.gameObject.GetComponent<OpenAiMonoBehaviour>().BlowUp();;
                            openAiComponent.gameObject.AddComponent<OpenAiMonoBehaviour>();
                        }
                    }
                }
            }
        }
        
        private static bool IsPrefab(OpenAiComponent openAiComponent)
        {
            bool isPrefab = openAiComponent.gameObject != null && (
                openAiComponent.gameObject.scene.name == null ||
                openAiComponent.gameObject.gameObject != null &&
                openAiComponent.gameObject.gameObject.scene.name == null
            );
            return isPrefab;
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            scriptsDirty = true;
        }
        
    }
}