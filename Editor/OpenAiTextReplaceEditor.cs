using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiTextReplace)), CanEditMultipleObjects]
    public class OpenAiTextReplaceEditor : EditorWidowOrInspector<OpenAiTextReplaceEditor>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(20);
            
            OpenAiTextReplace openAiTextReplace = target as OpenAiTextReplace;
            
            if (GUILayout.Button("Generate Text"))
            {
                if (!EditorUtils.ApiKeyPromptCheck())
                {
                    openAiTextReplace.ReplaceText();
                }
            }
        }
    }
}