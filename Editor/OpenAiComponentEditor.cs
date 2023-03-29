using System.IO;
using System.Linq;
using System;
using MyBox;
using TP.ExtensionMethods;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiComponent))]
    public class OpenAiComponentEditor : EditorWidowOrInspector<OpenAiComponentEditor>
    {
        private OpenAiComponent openAiComponent;
        private float activeWidth = 0;
        
        [SerializeField] private bool[] foldoutStates = new bool[] { };
        private static bool scriptsDirty = false; 
        
        public override void OnInspectorGUI()
        {
            openAiComponent = target as OpenAiComponent;

            if (Screen.width < 500 || !IsPrefab(openAiComponent))
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
            activeWidth = Screen.width - 25;
            DrawGroup1();
        }

        void WideLayout()
        {
            activeWidth = Screen.width / 2f - 35;
            EditorGUIUtility.labelWidth = Screen.width / 5;
            EditorUtils.Horizontal(() => {
                EditorUtils.Vertical(() => {
                    DrawGroup1();
                }, GUILayout.Width(activeWidth));
                GUILayout.Space(20);
                EditorUtils.Vertical(() => {
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
                    EditorUtils.Disable(true, () =>
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
                    if (!EditorUtils.ApiKeyPromptCheck())
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
                        foldoutStates[index] = EditorUtils.Foldout(foldoutStates[index], edit.editPrompt, () =>
                        {
                            EditorGUILayout.TextField("script", edit.script);
                            EditorGUILayout.TextField("editPrompt", edit.editPrompt);
                            EditorGUILayout.TextField("editedScript", edit.editedScript);
                        });
                    }
                }

                EditorUtils.Disable(openAiComponent.editPrompt.IsNullOrEmpty(), () =>
                {
                    if (GUILayout.Button("Create Edit"))
                    {
                        if (!EditorUtils.ApiKeyPromptCheck())
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
            EditorUtils.Disable(true, () => {
                EditorGUILayout.LabelField("Code");
                EditorStyles.textField.wordWrap = true;
                if (openAiComponent.script)
                {
                    EditorGUILayout.TextArea(openAiComponent.script.text);
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
            
            string fullPrompt = 
                openAiComponent.editPrePrompt + " '" + openAiComponent.editPrompt + "' " + openAiComponent.editPostPrompt + "\n\n" + 
                openAiComponent.script.text;

            var request = new AiText.Request(fullPrompt, OpenAiApi.Model.TEXT_DAVINCI_003, 1, .8f, max_tokens: 2048);
            var codeCompletion = await openAi.CreateCompletion(request);

            if (codeCompletion.Result == UnityWebRequest.Result.Success)
            {
                string scriptContents = codeCompletion.Text.Trim();
                string directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(openAiComponent.script));

                Utils.Script.CreateScript(scriptName, scriptContents, false, directory, true);
                
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
            if (openAiComponent.addOnReload == "" && scriptsDirty)
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