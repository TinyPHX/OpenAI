using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using UnityEngine.UIElements;

namespace OpenAi
{
    public class OpenAiCredentialsWindow: EditorWindow
     {
         private static OpenAiCredentialsWindow window;
         private static bool canSave = false;
         private static string apiKey;
         private static string orgId;
         private static int spacingBig = 25;
         private static int spacingSmall = 10;
         private static string helpText = "";
         private static bool showApiKey;
         private static MessageType messageType;
         GUILayoutOption[] smallButton = new[]
         {
             GUILayout.Width(EditorGUIUtility.singleLineHeight),
             GUILayout.Height(EditorGUIUtility.singleLineHeight)
         };
         
         [MenuItem("Window/OpenAI/Credentials")]
         public static void InitFromMenu()
         {
             HideHelp();
             Init();
         }

         public static void InitWithHelp(string helpText, MessageType messageType)
         {
             ShowHelp(helpText, messageType);
             Init();
         }
         
         private static void Init () 
         {
             PopulateCurrentCredentials();
             if (window != null)
             {
                 window.Destroy();
             }
             
             window = CreateInstance( typeof(OpenAiCredentialsWindow) ) as OpenAiCredentialsWindow;
             window.ShowUtility();
         }

         private static void ShowHelp(string helpText, MessageType messageType)
         {
             OpenAiCredentialsWindow.helpText = helpText;
             OpenAiCredentialsWindow.messageType = messageType;
         }

         public static void HideHelp()
         {
             helpText = "";
         }

         private void UpdateWindowSize(Rect content)
         {
             if (Event.current.type == EventType.Repaint)
             {
                 float height = content.size.y;
                 float minWidth = 400;
                 float maxWidth = 600;
                 minSize = new Vector2(minWidth, height);
                 maxSize = new Vector2(maxWidth, height);
             }
         }

         private void UpdateInstance()
         {
             if (this != window)
             {
                 Destroy();
             }
         }

         public void Destroy()
         {
             Close();

             if (Application.isPlaying)
             {
                 Destroy(this);
             }
             else
             {
                 DestroyImmediate(this);
             }
         }
         
         private static void PopulateCurrentCredentials()
         {
             Configuration config = OpenAiApi.ReadConfigFromUserDirectory();
             apiKey = config.ApiKey;
             orgId = config.Organization;
         }
         
         private static void OpenFolder(string folderPath)
         {
             ProcessStartInfo startInfo = new ProcessStartInfo
             {
                 Arguments = folderPath.Replace('/', '\\'),
                 FileName = "explorer.exe"
             };

             Process.Start(startInfo);
         }
         
         void OnGUI()
         {
             UpdateInstance();
             
             UpdateWindowSize(EditorUtils.Horizontal(() => {
                 GUILayout.Space(spacingSmall);
                 EditorUtils.Vertical(() => {
                     GUILayout.Space(spacingSmall);
                     if (helpText != "")
                     {
                         EditorGUILayout.HelpBox(helpText, messageType);
                     }
                     GUILayout.Space(spacingSmall);

                     EditorUtils.Horizontal(() => {
                         string priorValue = apiKey;
                         if (showApiKey)
                         {
                             apiKey = EditorGUILayout.TextField("API Key", apiKey);                             
                         }
                         else
                         {
                             apiKey = EditorGUILayout.PasswordField("API Key", apiKey);                             
                         }
                         if (apiKey != priorValue)
                         {
                             canSave = true;
                         }
                         
                         if (GUILayout.Button("*", smallButton))
                         {
                             showApiKey = !showApiKey;
                         }

                         if (GUILayout.Button("?", smallButton))
                         {
                             Application.OpenURL("https://platform.openai.com/account/api-keys");
                         }
                     });

                     EditorUtils.Horizontal(() => {
                         string priorValue = apiKey;
                         priorValue = orgId;
                         orgId = EditorGUILayout.TextField("Organization ID", orgId);
                         if (orgId != priorValue)
                         {
                             canSave = true;
                         }

                         if (GUILayout.Button("?", smallButton))
                         {
                             Application.OpenURL("https://platform.openai.com/account/org-settings");
                         }
                     });
                     GUILayout.Space(spacingBig);
                     EditorUtils.Horizontal(() => {
                         GUILayout.Space(spacingBig);
                         if (GUILayout.Button("Cancel"))
                         {
                             PopulateCurrentCredentials();
                             window.Close();
                         }

                         GUILayout.Space(spacingBig);
                         EditorUtils.Disable(!Directory.Exists(OpenAiApi.ConfigFileDir), () =>
                         {
                             if (GUILayout.Button("Open"))
                             {
                                 OpenFolder(OpenAiApi.ConfigFileDir);
                             }
                         });
                         GUILayout.Space(spacingBig);
                         EditorUtils.Disable(!canSave, () =>
                         {
                             if (GUILayout.Button("Save"))
                             {
                                 Configuration.GlobalConfig = new Configuration(apiKey, orgId);
                                 OpenAiApi.SaveConfigToUserDirectory(Configuration.GlobalConfig);
                                 window.Close();
                             }
                         });
                         GUILayout.Space(spacingBig); 
                     });
                     GUILayout.Space(spacingSmall);
                 });
                 GUILayout.Space(spacingSmall);
             }));
         }
     }
}