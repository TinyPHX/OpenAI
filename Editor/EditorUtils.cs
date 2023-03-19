using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OpenAi
{
    public static class EditorUtils
    {
        public static void SmallSpace() => GUILayout.Space(10); 
        public static void BigSpace() => GUILayout.Space(25); 
        
        public static GUILayoutOption[] smallButton = new[]
        {
            GUILayout.Width(EditorGUIUtility.singleLineHeight),
            GUILayout.Height(EditorGUIUtility.singleLineHeight)
        };
        
        public static float GetWidth()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            if (Event.current.type == EventType.Repaint)
            {
                return GUILayoutUtility.GetLastRect().width;
            }
            else
            {
                return -1;
            }
        }
        
        public delegate void Callback();

        public static Rect Horizontal(Callback callback)
        {
            return Horizontal(callback, new GUILayoutOption[] {});
        }
        public static Rect Horizontal(Callback callback, GUILayoutOption option)
        {
            return Horizontal(callback, new GUILayoutOption[] { option });
        }
        public static Rect Horizontal(Callback callback, GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
            callback();
            GUILayout.EndHorizontal();
            Rect content = new Rect(GUILayoutUtility.GetLastRect());
            return content;
        }
        
        public static Rect Vertical(Callback callback)
        {
            return Vertical(callback, new GUILayoutOption[] {});
        }
        public static Rect Vertical(Callback callback, GUILayoutOption option)
        {
            return Vertical(callback, new GUILayoutOption[] { option });
        }
        
        public static Rect Vertical(Callback callback, GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(options);
            callback();
            GUILayout.EndVertical();
            Rect content = new Rect(GUILayoutUtility.GetLastRect());
            return content;
        }
        
        public static void Disable(bool disable, Callback callback)
        {
            EditorGUI.BeginDisabledGroup(disable);
            callback();
            EditorGUI.EndDisabledGroup();
        }

        public static void ChangeCheck(Callback callback, Callback changeCallback)
        {
            EditorGUI.BeginChangeCheck();
            callback();
            if (EditorGUI.EndChangeCheck())
            {
                changeCallback();
            }
        }
        
        public static void OpenFolder(string folderPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = folderPath.Replace('/', '\\'),
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }

        public static bool ApiKeyPromptCheck()
        {
            bool promptShown = false;
            
            if (Configuration.GlobalConfig.ApiKey == "")
            {
                Configuration.GlobalConfig = OpenAiApi.ReadConfigFromUserDirectory();
                if (Configuration.GlobalConfig.ApiKey == "")
                {
                    OpenAiCredentialsWindow.InitWithHelp("Please setup your API Key before using the Open AI API.", MessageType.Info);
                    promptShown = true;
                }
            }

            return promptShown;
        }
    }
}