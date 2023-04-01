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
         private static string helpText = "";
         private static bool showApiKey;
         private static MessageType messageType;
         private bool isShowing = false;
         
         GUILayoutOption[] smallButton = new[]
         {
             GUILayout.Width(EditorGUIUtility.singleLineHeight),
             GUILayout.Height(EditorGUIUtility.singleLineHeight)
         };
         
         // [MenuItem("Window/OpenAI/Credentials")]
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

         public static OpenAiCredentialsWindow InitFromEditorWindow()
         {
             return Init(false);
         }
         
         private static OpenAiCredentialsWindow Init(bool show=true) 
         {
             PopulateCurrentCredentials();
             if (window != null)
             {
                 window.Destroy();
             }
             
             window = CreateInstance( typeof(OpenAiCredentialsWindow) ) as OpenAiCredentialsWindow;
             if (show)
             {
                window.ShowUtility();
                window.isShowing = true;
             }

             return window;
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
             CloseWindow();

             if (Application.isPlaying)
             {
                 Destroy(this);
             }
             else
             {
                 DestroyImmediate(this);
             }
         }
         
         public static void PopulateCurrentCredentials()
         {
             Configuration config = OpenAiApi.ReadConfigFromUserDirectory();
             apiKey = config.ApiKey;
             orgId = config.Organization;
         }

         void CloseWindow()
         {
             if (window != null && window.isShowing)
             {
                 window.Close();
             }
         }

         void Cancel()
         {
             PopulateCurrentCredentials();
             GUI.FocusControl("");
             CloseWindow();
         }

         void OnGUI()
         {
             UpdateInstance();
             
             UpdateWindowSize(DrawUi());
         }

         public Rect DrawUi()
         {
             return AiEditorUtils.Horizontal(() =>
             {
                 AiEditorUtils.SmallSpace();
                 AiEditorUtils.Vertical(() =>
                 {
                     AiEditorUtils.SmallSpace();
                     if (helpText != "")
                     {
                         EditorGUILayout.HelpBox(helpText, messageType);
                     }

                     AiEditorUtils.SmallSpace();

                     AiEditorUtils.Horizontal(() =>
                     {
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

                         if (GUILayout.Button("*", AiEditorUtils.smallButton))
                         {
                             showApiKey = !showApiKey;
                         }

                         if (GUILayout.Button("?", AiEditorUtils.smallButton))
                         {
                             Application.OpenURL("https://platform.openai.com/account/api-keys");
                         }
                     });

                     AiEditorUtils.Horizontal(() =>
                     {
                         string priorValue = apiKey;
                         priorValue = orgId;
                         orgId = EditorGUILayout.TextField("Organization ID", orgId);
                         if (orgId != priorValue)
                         {
                             canSave = true;
                         }

                         if (GUILayout.Button("?", AiEditorUtils.smallButton))
                         {
                             Application.OpenURL("https://platform.openai.com/account/org-settings");
                         }
                     });
                     AiEditorUtils.BigSpace();
                     AiEditorUtils.Horizontal(() =>
                     {
                         AiEditorUtils.BigSpace();
                         if (GUILayout.Button("Cancel"))
                         {
                             Cancel();
                         }

                         AiEditorUtils.BigSpace();
                         AiEditorUtils.Disable(!Directory.Exists(OpenAiApi.ConfigFileDir), () =>
                         {
                             if (GUILayout.Button("Open"))
                             {
                                 AiEditorUtils.OpenFolder(OpenAiApi.ConfigFileDir);
                             }
                         });
                         AiEditorUtils.BigSpace();
                         AiEditorUtils.Disable(!canSave, () =>
                         {
                             if (GUILayout.Button("Save"))
                             {
                                 canSave = false;
                                 Configuration.GlobalConfig = new Configuration(apiKey, orgId);
                                 OpenAiApi.SaveConfigToUserDirectory(Configuration.GlobalConfig);
                                 CloseWindow();
                             }
                         });
                         AiEditorUtils.BigSpace();
                     });
                     AiEditorUtils.SmallSpace();
                 });
                 AiEditorUtils.SmallSpace();
             });
         }
     }
}