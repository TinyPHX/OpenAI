using System;
using UnityEditor;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiApiExample)), CanEditMultipleObjects]
    public class OpenAiApiExampleEditor : EditorWidowOrInspector<OpenAiApiExampleEditor>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20);

            OpenAiApiExample openai = target as OpenAiApiExample;

            EditorUtils.Horizontal(() =>
            {
                if (GUILayout.Button("Text Completion Request"))
                {
                    if (!EditorUtils.ApiKeyPromptCheck())
                    {
                        openai.SendCompletionRequest();
                    }
                }
                if (GUILayout.Button("?", EditorUtils.smallButton))
                {
                    Application.OpenURL("https://platform.openai.com/docs/api-reference/completions");
                }
            });
            
            EditorUtils.Horizontal(() =>
            {
                if (GUILayout.Button("Image Generation Request"))
                {
                    if (!EditorUtils.ApiKeyPromptCheck())
                    {
                        openai.SendImageRequest();
                    }
                }
                if (GUILayout.Button("?", EditorUtils.smallButton))
                {
                    Application.OpenURL("https://platform.openai.com/docs/api-reference/images");
                }
            });
        }
    }
}