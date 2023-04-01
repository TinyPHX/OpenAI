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
        
        public override void OnInspectorGUI()
        {
            openai = target as OpenAiApiExample;

            AiEditorUtils.DrawDefaultWithEdits(serializedObject, new []
            {
                new AiEditorUtils.DrawEdit(nameof(openai.completionResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawTextCompletionButton();
                }),
                new AiEditorUtils.DrawEdit(nameof(openai.chatCompletionResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawChatCompletionButton();
                }),
                new AiEditorUtils.DrawEdit(nameof(openai.imageResponse), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    DrawImageGenerationButton();
                })
            });
        }

        private void DrawChatCompletionButton()
        {
            AiEditorUtils.Horizontal(() =>
            {
                if (GUILayout.Button("Chat Completion Request"))
                {
                    if (!AiEditorUtils.ApiKeyPromptCheck())
                    {
                        openai.SendChatCompletionRequest();
                    }
                }
                if (GUILayout.Button("?", AiEditorUtils.smallButton))
                {
                    Application.OpenURL("https://platform.openai.com/docs/api-reference/chat/create");
                }
            });
        }

        private void DrawTextCompletionButton()
        {
            AiEditorUtils.Horizontal(() =>
            {
                if (GUILayout.Button("Text Completion Request"))
                {
                    if (!AiEditorUtils.ApiKeyPromptCheck())
                    {
                        openai.SendCompletionRequest();
                    }
                }
                if (GUILayout.Button("?", AiEditorUtils.smallButton))
                {
                    Application.OpenURL("https://platform.openai.com/docs/api-reference/completions");
                }
            });
        }

        private void DrawImageGenerationButton()
        {
            AiEditorUtils.Horizontal(() =>
            {
                if (GUILayout.Button("Image Generation Request"))
                {
                    if (!AiEditorUtils.ApiKeyPromptCheck())
                    {
                        openai.SendImageRequest();
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