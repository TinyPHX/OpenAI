using System.Diagnostics;
using System.Linq;
using NUnit.Framework.Constraints;
using Unity.Collections;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

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
        
        public static void OpenFolder(string folderPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = folderPath.Replace('/', '\\'),
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }

        // private static string apiKey;
        // private static string orgId;
        // private static bool showApiKey;
        // public static Rect DrawCredentialsUi(string curentApiKey, string currentOrgId)
        // {
        //     EditorUtils.apiKey = curentApiKey;
        //     EditorUtils.orgId = currentOrgId;
        //     
        //     return Horizontal(() =>
        //     {
        //         SmallSpace();
        //         Vertical(() =>
        //         {
        //             SmallSpace();
        //             if (helpText != "")
        //             {
        //                 EditorGUILayout.HelpBox(helpText, messageType);
        //             }
        //
        //             SmallSpace();
        //
        //             Horizontal(() =>
        //             {
        //                 string priorValue = EditorUtils.apiKey;
        //                 if (showApiKey)
        //                 {
        //                     EditorUtils.apiKey = EditorGUILayout.TextField("API Key", EditorUtils.apiKey);
        //                 }
        //                 else
        //                 {
        //                     EditorUtils.apiKey = EditorGUILayout.PasswordField("API Key", EditorUtils.apiKey);
        //                 }
        //
        //                 if (apiKey != priorValue)
        //                 {
        //                     canSave = true;
        //                 }
        //
        //                 if (GUILayout.Button("*", smallButton))
        //                 {
        //                     showApiKey = !showApiKey;
        //                 }
        //
        //                 if (GUILayout.Button("?", smallButton))
        //                 {
        //                     Application.OpenURL("https://platform.openai.com/account/api-keys");
        //                 }
        //             });
        //
        //             EditorUtils.Horizontal(() =>
        //             {
        //                 string priorValue = apiKey;
        //                 priorValue = orgId;
        //                 orgId = EditorGUILayout.TextField("Organization ID", orgId);
        //                 if (orgId != priorValue)
        //                 {
        //                     canSave = true;
        //                 }
        //
        //                 if (GUILayout.Button("?", smallButton))
        //                 {
        //                     Application.OpenURL("https://platform.openai.com/account/org-settings");
        //                 }
        //             });
        //             GUILayout.Space(spacingBig);
        //             EditorUtils.Horizontal(() =>
        //             {
        //                 GUILayout.Space(spacingBig);
        //                 if (GUILayout.Button("Cancel"))
        //                 {
        //                     PopulateCurrentCredentials();
        //                     window.Close();
        //                 }
        //
        //                 BigSpace();
        //                 EditorUtils.Disable(!Directory.Exists(OpenAiApi.ConfigFileDir), () =>
        //                 {
        //                     if (GUILayout.Button("Open"))
        //                     {
        //                         OpenFolder(OpenAiApi.ConfigFileDir);
        //                     }
        //                 });
        //                 BigSpace();
        //                 EditorUtils.Disable(!canSave, () =>
        //                 {
        //                     if (GUILayout.Button("Save"))
        //                     {
        //                         Configuration.GlobalConfig = new Configuration(apiKey, orgId);
        //                         OpenAiApi.SaveConfigToUserDirectory(Configuration.GlobalConfig);
        //                         window.Close();
        //                     }
        //                 });
        //                 BigSpace();
        //             });
        //             SmallSpace();
        //         });
        //         SmallSpace();
        //     }));
        // }
    }
}