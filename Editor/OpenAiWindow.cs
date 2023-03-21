using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using TP;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OpenAi
{
    public class OpenAiWindow: EditorWindow
     {
         private static OpenAiWindow window;
         private static Vector2 scroll = new Vector2();
         private static int activeTab = 0;
         private static int spacing = 20;
         private static Scene activeScene;

         private OpenAiCredentialsWindow credsWindow;

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
             { Tabs.help, "Readme" }
         };
         
         private static Dictionary<Tabs, string> shortTabNames = new Dictionary<Tabs, string>()
         {
             { Tabs.raw, "Raw" },
             { Tabs.text, "Text" },
             { Tabs.image, "Image" },
             { Tabs.creds, "Creds" },
             { Tabs.help, "Help" }
         };

         private static Dictionary<Type, Object> Targets = new Dictionary<Type, Object>();
         private static Dictionary<Type, Object> Editors = new Dictionary<Type, Object>();
         private static Dictionary<Type, GameObject> Prefabs = new Dictionary<Type, GameObject>();

         private void RenderInspector<T,P>() 
             where T : EditorWidowOrInspector<T>
             where P : MonoBehaviour
         {
             var editor = GetTargetEditor<T>();
             editor.InternalTarget = GetTarget<P>();

             EditorUtils.ChangeCheck(() =>
             {
                 editor.OnInspectorGUI();
             }, () =>
             {
                 SavePrefab<P>();
             });
         }
         
         private static T GetTargetEditor<T>() where T : Editor
         {
             if (!Editors.ContainsKey(typeof(T)) || Editors[typeof(T)] == null)
             {
                 T editor = CreateInstance<T>();

                 Editors[typeof(T)] = editor;
             }

             return Editors[typeof(T)] as T;
         }

         private static T GetTarget<T>() where T : MonoBehaviour
         {
             Scene previousScene = activeScene;
             activeScene = SceneManager.GetActiveScene();
             if (activeScene != previousScene)
             {
                 // TODO create hidden instance :(
                 // Doing this will allow for "Replace in Scene" to work in EditorWindow.
             }
             
             if (!Targets.ContainsKey(typeof(T)) || Targets[typeof(T)] == null)
             {
                 var findAssetResults = AssetDatabase.FindAssets ( $"t:Script {nameof(OpenAiWindow)}" );
                 var assetPath = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(findAssetResults[0]));

                 string defaultPrefabPath = "/Prefabs/prefab.prefab";
                 string newPrefabPath = $"/Prefabs/{typeof(T)}.prefab";

                 GameObject prefab;
                 if (File.Exists(Application.dataPath + assetPath.Substring("Assets".Length) + newPrefabPath))
                 {
                     GameObject loadedPrefab = PrefabUtility.LoadPrefabContents(assetPath + newPrefabPath);
                     prefab = PrefabUtility.SaveAsPrefabAsset(loadedPrefab, assetPath + newPrefabPath, out bool success);
                     prefab = PrefabUtility.SavePrefabAsset(prefab);
                     PrefabUtility.UnloadPrefabContents(loadedPrefab);
                 }
                 else
                 {
                     GameObject defaultPrefab = PrefabUtility.LoadPrefabContents(assetPath + defaultPrefabPath);
                     prefab = PrefabUtility.SaveAsPrefabAsset(defaultPrefab, assetPath + newPrefabPath, out bool success);
                     PrefabUtility.UnloadPrefabContents(defaultPrefab);
                     prefab.AddComponent<T>();
                     prefab = PrefabUtility.SavePrefabAsset(prefab);
                     
                     AssetDatabase.Refresh();
                 }
                 
                 Prefabs[typeof(T)] = prefab;

                 T newTarget = prefab.GetComponent<T>();

                 Targets[typeof(T)] = newTarget;
             }

             return Targets[typeof(T)] as T;
         }

         private void SavePrefab<T>() where T : MonoBehaviour
         {
             if (Targets.ContainsKey(typeof(T)))
             {
                 PrefabUtility.SavePrefabAsset(Prefabs[typeof(T)]);
             }
         }

         [MenuItem("Window/OpenAI")] 
         private static void Init () 
         {
             if (window != null)
             {
                 window.Destroy();
             }
             
             window  = (OpenAiWindow)GetWindow(typeof(OpenAiWindow), false, "OpenAI");
             
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

             int previousActiveTab = activeTab;
             activeTab = GUILayout.Toolbar(activeTab, names, GUILayout.Width(Screen.width - spacing * 2 - 5));
             bool tabChanged = previousActiveTab == activeTab;

             GUILayout.Space(spacing);

             if (activeTab == (int)Tabs.raw)
             {
                 RenderInspector<OpenAiApiExampleEditor, OpenAiApiExample>();
             }

             if (activeTab == (int)Tabs.text)
             {
                 RenderInspector<OpenAiTextReplaceEditor, OpenAiTextReplace>();
             }

             if (activeTab == (int)Tabs.image)
             {
                 RenderInspector<OpenAiImageReplaceEditor, OpenAiImageReplace>();
             }

             if (activeTab == (int)Tabs.creds)
             {
                 if (credsWindow == null)
                 {
                     credsWindow = OpenAiCredentialsWindow.InitFromEditorWindow();
                 }

                 if (tabChanged)
                 {
                     OpenAiCredentialsWindow.PopulateCurrentCredentials();
                 }
                 
                 credsWindow.DrawUi();
             }

             if (activeTab == (int)Tabs.help)
             {
                 RenderInspector<ReadmeEditor, Readme>();
             }

             GUILayout.Space(spacing);
             EditorGUILayout.EndVertical();
             GUILayout.Space(spacing);
             EditorGUILayout.EndHorizontal();
             EditorGUILayout.EndScrollView();
         }
     }
}