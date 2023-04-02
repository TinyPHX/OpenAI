using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiApiExample)), CanEditMultipleObjects]
    public class OpenAiApiExampleEditor : EditorWidowOrInspector<OpenAiApiExampleEditor>
    {
        private OpenAiApiExample openai;
        private float activeWidth = 0;
        
        public override void OnInspectorGUI()
        {
            openai = target as OpenAiApiExample;

            EditorStyles.textField.wordWrap = true;
            
            if (Screen.width < 500)
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
            AiEditorUtils.DrawDefaultWithEdits(serializedObject, new []
            {
                new AiEditorUtils.DrawEdit(nameof(openai.configuration), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    EditorGUILayout.LabelField("Configuration Setup Code");
                    EditorGUILayout.TextArea(openai.GetConfigurationCode());
                }),
                new AiEditorUtils.DrawEdit(nameof(openai.aiText), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawAiTextButton();
                    EditorGUILayout.LabelField("Ai Text Code");
                    EditorGUILayout.TextArea(openai.GetAiTextRequestCode());
                }),
                new AiEditorUtils.DrawEdit(nameof(openai.aiChat), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawAiChatButton();
                    EditorGUILayout.LabelField("Ai Chat Code");
                    EditorGUILayout.TextArea(openai.GetAiChatRequestCode());
                }),
                new AiEditorUtils.DrawEdit(nameof(openai.aiImageResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawAiImageButton();
                    EditorGUILayout.LabelField("Ai Image Code");
                    EditorGUILayout.TextArea(openai.GetAiImageRequestCode());
                })
            });
        }

        void WideLayout()
        {
            activeWidth = Screen.width / 2f - 35;
            AiEditorUtils.Horizontal(() =>
            {
                AiEditorUtils.Vertical(() =>
                {
                    AiEditorUtils.DrawDefaultWithEdits(serializedObject, new[]
                    {
                        new AiEditorUtils.DrawEdit(nameof(openai.aiText), AiEditorUtils.DrawEdit.DrawType.AFTER,
                            () => { DrawAiTextButton(); }),
                        new AiEditorUtils.DrawEdit(nameof(openai.aiChat), AiEditorUtils.DrawEdit.DrawType.AFTER,
                            () => { DrawAiChatButton(); }),
                        new AiEditorUtils.DrawEdit(nameof(openai.aiImageResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, 
                            () => { DrawAiImageButton(); })
                    });
                });
                
                AiEditorUtils.SmallSpace();
                
                AiEditorUtils.Vertical(() =>
                {
                    EditorGUILayout.LabelField("Sample Code");
                    EditorGUILayout.TextArea(openai.GetFullCode(), GUILayout.MaxWidth(activeWidth));
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
                        openai.SendAiChatRequest();
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
                        openai.SendAiTextRequest();
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
                        openai.SendAiImageRequest();
                    }
                }
                if (GUILayout.Button("?", AiEditorUtils.smallButton))
                {
                    Application.OpenURL("https://platform.openai.com/docs/api-reference/images");
                }
            });
        }
    }
}