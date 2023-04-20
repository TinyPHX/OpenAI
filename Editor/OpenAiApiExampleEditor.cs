using MyBox;
using OpenAI.AiModels;
using UnityEditor;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiApiExample)), CanEditMultipleObjects]
    public class OpenAiApiExampleEditor : EditorWidowOrInspector<OpenAiApiExampleEditor>
    {
        private OpenAiApiExample example;
        private float activeWidth = 0;
        private float lastRefresh = 0;
        private float refreshInterval = 10;
        private string fullCode = "";

        private bool needsRefresh => EditorApplication.timeSinceStartup - lastRefresh > refreshInterval;
        
        public override void OnInspectorGUI()
        {
            example = target as OpenAiApiExample;

            RequiresConstantRepaint();

            EditorStyles.textField.wordWrap = true;
            
            if (AiEditorUtils.ScaledWidth < 500)
            {
                NarrowLayout();
            }
            else
            {
                WideLayout();   
            }

            if (needsRefresh)
            {
                lastRefresh = (float)EditorApplication.timeSinceStartup;
            }
        }
        
        void NarrowLayout()
        {
            activeWidth = AiEditorUtils.ScaledWidth - 25;
            AiEditorUtils.DrawDefaultWithEdits(serializedObject, new []
            {
                new AiEditorUtils.DrawEdit(nameof(example.configuration), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    EditorGUILayout.LabelField("Configuration Setup Code");
                    EditorGUILayout.TextArea(GetConfigurationCode());
                }),
                new AiEditorUtils.DrawEdit(nameof(example.aiText), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawAiTextButton();
                    EditorGUILayout.LabelField("Ai Text Code");
                    EditorGUILayout.TextArea(GetAiTextRequestCode());
                }),
                new AiEditorUtils.DrawEdit(nameof(example.aiChat), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawAiChatButton();
                    EditorGUILayout.LabelField("Ai Chat Code");
                    EditorGUILayout.TextArea(GetAiChatRequestCode());
                }),
                new AiEditorUtils.DrawEdit(nameof(example.aiImageResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawAiImageButton();
                    EditorGUILayout.LabelField("Ai Image Code");
                    EditorGUILayout.TextArea(GetAiImageRequestCode());
                }),
                new AiEditorUtils.DrawEdit(nameof(example.aiImageEditResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawAiImageEditButton();
                    EditorGUILayout.LabelField("Ai Image Edit Code");
                    EditorGUILayout.TextArea(GetAiImageEditRequestCode());
                }),
                new AiEditorUtils.DrawEdit(nameof(example.aiImageVariationResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawAiImageVariationButton();
                    EditorGUILayout.LabelField("Ai Image Variant Code");
                    EditorGUILayout.TextArea(GetAiImageVariantRequestCode());
                })
            });
        }

        void WideLayout()
        {
            activeWidth = AiEditorUtils.ScaledWidth / 2f - 35;
            AiEditorUtils.Horizontal(() =>
            {
                AiEditorUtils.Vertical(() =>
                {
                    AiEditorUtils.DrawDefaultWithEdits(serializedObject, new[]
                    {
                        new AiEditorUtils.DrawEdit(nameof(example.aiText), AiEditorUtils.DrawEdit.DrawType.AFTER,
                            () => { DrawAiTextButton(); }),
                        new AiEditorUtils.DrawEdit(nameof(example.aiChat), AiEditorUtils.DrawEdit.DrawType.AFTER,
                            () => { DrawAiChatButton(); }),
                        new AiEditorUtils.DrawEdit(nameof(example.aiImageResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, 
                            () => { DrawAiImageButton(); }),
                        new AiEditorUtils.DrawEdit(nameof(example.aiImageEditResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, 
                            () => { DrawAiImageEditButton(); }),
                        new AiEditorUtils.DrawEdit(nameof(example.aiImageVariationResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, 
                            () => { DrawAiImageVariationButton(); })
                    });
                });
                
                AiEditorUtils.SmallSpace();
                
                AiEditorUtils.Vertical(() =>
                {
                    EditorGUILayout.LabelField("Sample Code");
                    EditorGUILayout.TextArea(GetFullCode(), GUILayout.MaxWidth(activeWidth));
                });
            });
        }

        private void DrawAiChatButton()
        {
            AiEditorUtils.Horizontal(() =>
            {
                if (GUILayout.Button("Chat Completion Request"))
                {
                    if (!AiEditorUtils.ApiKeyPromptCheck())
                    {
                        example.SendAiChatRequest();
                    }
                }
                if (GUILayout.Button("?", AiEditorUtils.smallButton))
                {
                    Application.OpenURL("https://platform.openai.com/docs/api-reference/chat/create");
                }
            });
        }

        private void DrawAiTextButton()
        {
            AiEditorUtils.Horizontal(() =>
            {
                if (GUILayout.Button("Text Completion Request"))
                {
                    if (!AiEditorUtils.ApiKeyPromptCheck())
                    {
                        example.SendAiTextRequest();
                    }
                }
                if (GUILayout.Button("?", AiEditorUtils.smallButton))
                {
                    Application.OpenURL("https://platform.openai.com/docs/api-reference/completions");
                }
            });
        }

        private void DrawAiImageButton()
        {
            AiEditorUtils.Horizontal(() =>
            {
                if (GUILayout.Button("Image Generation Request"))
                {
                    if (!AiEditorUtils.ApiKeyPromptCheck())
                    {
                        example.SendAiImageRequest();
                    }
                }
                if (GUILayout.Button("?", AiEditorUtils.smallButton))
                {
                    Application.OpenURL("https://platform.openai.com/docs/api-reference/images");
                }
            });
        }

        private void DrawAiImageEditButton()
        {
            AiEditorUtils.Horizontal(() =>
            {
                if (GUILayout.Button("Image Edit Request"))
                {
                    if (!AiEditorUtils.ApiKeyPromptCheck())
                    {
                        example.SendAiImageEditRequest();
                    }
                }
                if (GUILayout.Button("?", AiEditorUtils.smallButton))
                {
                    Application.OpenURL("https://platform.openai.com/docs/api-reference/images/create-edit");
                }
            });
        }
        
        private void DrawAiImageVariationButton()
        {
            AiEditorUtils.Horizontal(() =>
            {
                if (GUILayout.Button("Image Variation Request"))
                {
                    if (!AiEditorUtils.ApiKeyPromptCheck())
                    {
                        example.SendAiImageVariationRequest();
                    }
                }
                if (GUILayout.Button("?", AiEditorUtils.smallButton))
                {
                    Application.OpenURL("https://platform.openai.com/docs/api-reference/images/create-variation");
                }
            });
        }
        
        public string GetFullCode()
        {
            return "";
            
            if (needsRefresh)
            {
                Debug.Log("Refreshing");
                
                fullCode = GetConfigurationCode() + "\n\n" +
                           GetAiTextRequestCode() + "\n\n" + 
                           GetAiChatRequestCode() + "\n\n" + 
                           GetAiImageRequestCode() + "\n\n" + 
                           GetAiImageEditRequestCode() + "\n\n" + 
                           GetAiImageVariantRequestCode();
            }

            return fullCode;
        }

        public string GetConfigurationCode()
        {
            string code = "";
            if (!example.configuration.ApiKey.IsNullOrEmpty() || !example.configuration.Organization.IsNullOrEmpty())
            {
                code = $@"

// ------------ Required imports -----------

/* The code below requires these imports:
using OpenAi;
using OpenAI.AiModels;
using UnityEditor;
*/

// ------------ Configuration -----------

Configuration configuration = new Configuration(""{example.configuration.ApiKey}"", ""{example.configuration.Organization}"");
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
            string Prompt() => $"\"{example.aiTextRequest.prompt.Replace("\n", "\\n")}\"";
            string Model() => $", {returnTab}Models.Text.{example.aiTextRequest.model.ToString()}";
            string N() => example.aiTextRequest.n == 1 ? "" : $", {returnTab}n:{example.aiTextRequest.n}";
            string Temperature() => example.aiTextRequest.temperature == .8f ? "" : $", {returnTab}temperature:{example.aiTextRequest.temperature}f";
            string MaxTokens() => example.aiTextRequest.max_tokens == 100 ? "" : $", {returnTab}max_tokens:{example.aiTextRequest.max_tokens}";
            string Stream() => !example.aiTextRequest.stream ? "" : $", {returnTab}stream:true";
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
                for (var index = 0; index < example.aiChatRequest.messages.Length; index++)
                {
                    if (index > 0)
                    {
                        messages += ", ";
                    }
                    if (example.aiChatRequest.messages.Length > 1)
                    {
                        messages += returnTabTab;
                    }
                    
                    var message = example.aiChatRequest.messages[index];
                    
                    string Content() => $"\"{message.content.Replace("\n", "\\n")}\"";
                    string Role() => message.role == Message.Role.USER ? "" : $", Message.Role.{message.role.ToString()}";

                    messages += $"new Message({Content()}{Role()})";
                }
                if (example.aiChatRequest.messages.Length > 1)
                {
                    messages += returnTab;
                }
                messages += " }";

                return messages;
            }
            string Model() => $", {returnTab}Models.Chat.{example.aiChatRequest.model.ToString()}";
            string N() => example.aiChatRequest.n == 1 ? "" : $", {returnTab}n:{example.aiChatRequest.n}";
            string Temperature() => example.aiChatRequest.temperature == .8f ? "" : $", {returnTab}temperature:{example.aiChatRequest.temperature}f";
            string MaxTokens() => example.aiChatRequest.max_tokens == 100 ? "" : $", {returnTab}max_tokens:{example.aiChatRequest.max_tokens}";
            string Stream() => !example.aiChatRequest.stream ? "" : $", {returnTab}stream:true";
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
            string Prompt() => $"\"{example.aiImageRequest.prompt.Replace("\n", "\\n")}\"";
            string Size() => $", {returnTab}ImageSize.{example.aiImageRequest.size.ToString()}";
            string N() => example.aiImageRequest.n == 1 ? "" : $", {returnTab}n:{example.aiImageRequest.n}";
            string Callback() => ", aiImage =>";
            
            return $@"
// ------------ AI Image -----------

openai.CreateImage({Prompt()}{Size()}{N()}{Callback()}
{{
    Debug.Log(aiImage.data[0].texture); // Do something with result!
}});
".Trim();
        }
        
        public string GetAiImageEditRequestCode()
        {
            string returnTab = "\n    ";
            string VarSetup()
            {
                string imagePath = AssetDatabase.GetAssetPath(example.aiImageEditRequest.image);
                string imageSource = imagePath == "" ? 
                    "Texture2D.blackTexture; //TODO: add reference to your texture." :
                    $"AssetDatabase.LoadAssetAtPath<Texture2D>(\"{imagePath}\");";
                string maskPath = AssetDatabase.GetAssetPath(example.aiImageEditRequest.mask);
                string maskSource = maskPath == "" ? 
                    "Texture2D.blackTexture; //TODO: add reference to your texture." :
                    $"AssetDatabase.LoadAssetAtPath<Texture2D>(\"{maskPath}\");";

                return $"Texture2D imageToEdit = {imageSource}\nTexture2D imageMask = {maskSource}";
            }
            string Image()
            {
                return "imageToEdit";
            }
            string Mask()
            {
                return ", imageMask";
            }
            string Prompt() => $", \"{example.aiImageEditRequest.prompt.Replace("\n", "\\n")}\"";
            string Size() => $", {returnTab}ImageSize.{example.aiImageEditRequest.size.ToString()}";
            string N() => example.aiImageEditRequest.n == 1 ? "" : $", {returnTab}n:{example.aiImageEditRequest.n}";
            string Callback() => ", aiImage =>";
            
            return $@"
// ------------ AI Image Edit -----------

{VarSetup()}

openai.CreateImageEdit({Image()}{Mask()}{Prompt()}{Size()}{N()}{Callback()}
{{
    Debug.Log(aiImage.data[0].texture); // Do something with result!
}});
".Trim();
        }
        
        public string GetAiImageVariantRequestCode()
        {
            string returnTab = "\n    ";
            string VarSetup()
            {
                string imagePath = AssetDatabase.GetAssetPath(example.aiImageVariationRequest.image);
                string imageSource = imagePath == "" ? 
                    "Texture2D.blackTexture; //TODO: add reference to your texture." :
                    $"AssetDatabase.LoadAssetAtPath<Texture2D>(\"{imagePath}\");";

                return $"Texture2D imageToVary = {imageSource}";
            }
            string Image()
            {
                return "imageToVary";
            }
            string Size() => $", {returnTab}ImageSize.{example.aiImageVariationRequest.size.ToString()}";
            string N() => example.aiImageVariationRequest.n == 1 ? "" : $", {returnTab}n:{example.aiImageVariationRequest.n}";
            string Callback() => ", aiImage =>";
            
            return $@"
// ------------ AI Image Variant -----------

{VarSetup()}

openai.CreateImageVariant({Image()}{Size()}{N()}{Callback()}
{{
    Debug.Log(aiImage.data[0].texture); // Do something with result!
}});
".Trim();
        }
    }
}