using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiTextReplace)), CanEditMultipleObjects]
    public class OpenAiTextReplaceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(20);
            
            OpenAiTextReplace openAiTextReplace = target as OpenAiTextReplace;
            
            if (GUILayout.Button("Generate Text"))
            {
                if (Configuration.GlobalConfig.ApiKey == "")
                {
                    OpenAiCredentialsWindow.InitWithHelp("Please setup your API Key before using the Open AI API.", MessageType.Info);
                    return;
                }
                
                openAiTextReplace.ReplaceText();
            }
        }
    }
}