using System;
using UnityEditor;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiApiExample)), CanEditMultipleObjects]
    public class OOpenAiApiExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20);

            OpenAiApiExample openai = target as OpenAiApiExample;

            if (GUILayout.Button("Text Completion Request"))
            {
                openai.SendCompletionRequest();
            }

            if (GUILayout.Button("Image Generation Request"))
            {
                openai.SendImageRequest();
            }
        }
    }
}