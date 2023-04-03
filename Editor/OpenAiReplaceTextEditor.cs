using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiReplaceText)), CanEditMultipleObjects]
    public class OpenAiTextReplaceEditor : EditorWidowOrInspector<OpenAiTextReplaceEditor>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(20);
            
            OpenAiReplaceText openAiReplaceText = target as OpenAiReplaceText;
            
            if (GUILayout.Button("Generate Text"))
            {
                if (!AiEditorUtils.ApiKeyPromptCheck())
                {
                    openAiReplaceText.ReplaceText();
                }
            }
        }
    }
}