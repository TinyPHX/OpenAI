using System;
using System.Collections;
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
         public static int ActiveTab { get; private set; } = -1;
         private int previousActiveTab = -1;
         private static int spacing = 20;
         private static Scene activeScene;
         private static float saveFrequency = 1;
         private static HashSet<Type> toSave = new HashSet<Type>();
         private static Coroutine saveCoroutine = null;

         private OpenAiCredentialsWindow credsWindow;

         public enum Tabs
         {
             raw = 0,
             text = 1,
             image = 2,
             script = 3,
             creds = 4,
             help = 5,
         }
         
         private static Dictionary<Tabs, string> tabNames = new Dictionary<Tabs, string>()
         {
             { Tabs.raw, "☷ Raw Requests" },
             { Tabs.text, "❡ Text Completion" },
             { Tabs.image, "✎ Image Generation" },
             { Tabs.script, "{} Script (Beta)" },
             { Tabs.creds, "✱ Credentials" },
             { Tabs.help, "❤ Readme" }
         };
         
         private static Dictionary<Tabs, string> shortTabNames = new Dictionary<Tabs, string>()
         {
             { Tabs.raw, "Raw" },
             { Tabs.text, "Text" },
             { Tabs.image, "Image" },
             { Tabs.script, "Script" },
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

             AiEditorUtils.ChangeCheck(() =>
             {
                 editor.OnInspectorGUI();
             }, () =>
             {
                 if (toSave.Add(typeof(P)) && saveCoroutine == null)
                 {
                     saveCoroutine = OpenAiApi.Runner.StartCoroutine(Save(saveFrequency));
                 }
             });
         }

         static IEnumerator Save(float delay)
         {
             yield return new WaitForSeconds(delay);
             
             foreach (Type type in toSave)
             {
                 PrefabUtility.SavePrefabAsset(Prefabs[type]);
             }

             toSave.Clear();
             saveCoroutine = null;
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
         public static void Init ()
         {
             InitTab(Tabs.help);
         }

         public static void InitTab(Tabs tab)
         {
             Init((int)tab);
         }

         private static void Init(int startTab)
         {
             if (window != null)
             {
                 window.Destroy();
             }
             
             window  = (OpenAiWindow)GetWindow(typeof(OpenAiWindow), false, "OpenAI");

             if (startTab != -1)
             {
                 ActiveTab = startTab;
             }

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
             scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(AiEditorUtils.ScaledWidth));
             EditorGUILayout.BeginHorizontal();
             GUILayout.Space(spacing);
             EditorGUILayout.BeginVertical();
             GUILayout.Space(spacing);

             string[] names = (AiEditorUtils.ScaledWidth < 550 ? shortTabNames.Values : tabNames.Values).ToArray();

             ActiveTab = ActiveTab != -1 ? ActiveTab : previousActiveTab;
             previousActiveTab = ActiveTab;
             ActiveTab = GUILayout.Toolbar(ActiveTab, names, GUILayout.Width(AiEditorUtils.ScaledWidth - spacing * 2 - 5));
             bool tabChanged = previousActiveTab != ActiveTab;

             GUILayout.Space(spacing);

             if (ActiveTab == (int)Tabs.raw)
             {
                 RenderInspector<OpenAiApiExampleEditor, OpenAiApiExample>();
             }

             if (ActiveTab == (int)Tabs.text)
             {
                 RenderInspector<OpenAiTextReplaceEditor, OpenAiReplaceText>();
             }

             if (ActiveTab == (int)Tabs.image)
             {
                 RenderInspector<OpenAiReplaceImageEditor, OpenAiReplaceImage>();
             }

             if (ActiveTab == (int)Tabs.script)
             {
                 RenderInspector<OpenAiComponentEditor, OpenAiComponent>();
             }

             if (ActiveTab == (int)Tabs.creds)
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

             if (ActiveTab == (int)Tabs.help)
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