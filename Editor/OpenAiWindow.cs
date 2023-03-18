using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace OpenAi
{
    public class OpenAiWindow: EditorWindow
     {
         private static OpenAiWindow window;
         private static OpenAiImageReplace target;
         private static OpenAiImageReplaceEditor openAiImageReplaceEditor;
         private static Vector2 scroll = new Vector2();
         private static int activeTab = 0;
         private static int spacing = 20;
         private static Scene activeScene;

         private enum Tabs
         {
             raw = 0,
             text = 1,
             image = 2,
             creds = 3,
             help = 4,
         }
         
         private static Dictionary<Tabs, string> tabNames = new Dictionary<Tabs, string>()
         {
             { Tabs.raw, "Raw Requests" },
             { Tabs.text, "Text Completion" },
             { Tabs.image, "Image Generation" },
             { Tabs.creds, "Credentials" },
             { Tabs.help, "Help" }
         };
         
         private static Dictionary<Tabs, string> shortTabNames = new Dictionary<Tabs, string>()
         {
             { Tabs.raw, "Raw" },
             { Tabs.text, "Text" },
             { Tabs.image, "Image" },
             { Tabs.creds, "Creds" },
             { Tabs.help, "Help" }
         };

         private static OpenAiImageReplace Target {
             get
             {
                 Scene previousScene = activeScene;
                 activeScene = SceneManager.GetActiveScene();
                 if (activeScene != previousScene)
                 {
                     //TODO create hidden instance :(
                 }
                 
                 if (target == null)
                 {
                     var findAssetResults = AssetDatabase.FindAssets ( $"t:Script {nameof(OpenAiWindow)}" );
                     var assetPath = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(findAssetResults[0]));

                     string defaultPrefabPath = "/Prefabs/prefab.prefab";
                     string newPrefabPath = "/Prefabs/OpenAiImageReplace.prefab";

                     GameObject prefab;
                     if (Directory.Exists(Application.dataPath + assetPath))
                     {
                         prefab = PrefabUtility.LoadPrefabContents(assetPath + newPrefabPath);
                     }
                     else
                     {
                         GameObject defaultPrefab = PrefabUtility.LoadPrefabContents(assetPath + defaultPrefabPath);
                         prefab = PrefabUtility.SaveAsPrefabAsset(defaultPrefab, assetPath + newPrefabPath, out bool success);
                         // PrefabUtility.SaveAsPrefabAsset(defaultPrefab, assetPath + newPrefabPath, out bool success);
                         PrefabUtility.UnloadPrefabContents(defaultPrefab);
                         // prefab = PrefabUtility.LoadPrefabContents(assetPath + newPrefabPath);
                         target = prefab.AddComponent<OpenAiImageReplace>();
                         prefab = PrefabUtility.SavePrefabAsset(target.gameObject);
                         
                         // Save();
                     }

                     target = prefab.GetComponent<OpenAiImageReplace>();
                 }

                 return target;
             }
         }

         // private static void OnUpdate<T>(T updated)
         private static void OnUpdate<T>(T updated)
         {
             if (typeof(T) == typeof(OpenAiImageReplace))
             {
                 //TIS GARBAGE!
                 
                 // var findAssetResults = AssetDatabase.FindAssets ( $"t:Script {nameof(OpenAiWindow)}" );
                 // var assetPath = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(findAssetResults[0]));
                 // string newPrefabPath = "/Prefabs/OpenAiImageReplace.prefab";
                 // GameObject prefab = PrefabUtility.LoadPrefabContents(assetPath + newPrefabPath);
                 // target = updated as OpenAiImageReplace;
                 
                 // Save();
                 // DrawUi();
             }
         }

         private static void Save()
         {
             // string newPrefabPath = "/Prefabs/OpenAiImageReplace.prefab";
             // PrefabUtility.SaveAsPrefabAsset(target.gameObject, newPrefabPath, out bool success);
             // GameObject prefab = PrefabUtility.SavePrefabAsset(target.gameObject);
             // target = prefab.GetComponent<OpenAiImageReplace>();
             // AssetDatabase.SaveAssets();
             // AssetDatabase.Refresh();
         }

         [MenuItem("Window/OpenAI/Show Window")] 
         private static void Init () 
         {
             if (window != null)
             {
                 window.Destroy();
             }
             
             window  = (OpenAiWindow)GetWindow(typeof(OpenAiWindow), false, "OpenAI");
             // window = CreateInstance( typeof(OpenAiWindow) ) as OpenAiWindow;
             
             window.ShowUtility();
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
         
         void OnGUI()
         {
             DrawUi();
         }

         void DrawUi()
         {
             scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(Screen.width));
             EditorGUILayout.BeginHorizontal();
             GUILayout.Space(spacing);
             EditorGUILayout.BeginVertical();
             GUILayout.Space(spacing);

             string[] names = (Screen.width < 550 ? shortTabNames.Values : tabNames.Values).ToArray();
             
             activeTab = GUILayout.Toolbar(activeTab, names, GUILayout.Width(Screen.width - spacing * 2 - 5));
             
             GUILayout.Space(spacing);

             if (activeTab == (int)Tabs.image)
             {
                 if (openAiImageReplaceEditor == null)
                 {
                     openAiImageReplaceEditor = CreateInstance<OpenAiImageReplaceEditor>();
                 }

                 if (openAiImageReplaceEditor.OnUpdate == null)
                 {
                     openAiImageReplaceEditor.OnUpdate = OnUpdate;
                 }

                 if (openAiImageReplaceEditor.InternalTarget == null)
                 {
                     openAiImageReplaceEditor.InternalTarget = Target;
                 }

                 if (openAiImageReplaceEditor.InternalTarget != Target)
                 { 
                     Debug.Log("WTF");
                 }

                 openAiImageReplaceEditor.OnInspectorGUI();
                 
                 
                 // if (Event.current.type == EventType.Used)
                 // {
                 //     // Save();
                 // }
                 //
                 // Debug.Log("Event.current.type: " + Event.current.type);
             }

             GUILayout.Space(spacing);
             EditorGUILayout.EndVertical();
             GUILayout.Space(spacing);
             EditorGUILayout.EndHorizontal();
             EditorGUILayout.EndScrollView();
         }
     }
}