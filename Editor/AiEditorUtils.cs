using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OpenAi
{
    public static class AiEditorUtils
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

        public static float ScaledWidth => 1 / (Screen.dpi / 96.0f) * Screen.width;

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

        public static bool Foldout(bool shown, string name, Callback callback)
        {
            bool newShown = EditorGUILayout.Foldout(shown, name);
            if (newShown)
            {
                EditorGUI.indentLevel++;
                callback();
                EditorGUI.indentLevel--;
            }

            return newShown;
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
        
        public class DrawEdit
        {
            public enum DrawType { BEFORE, REPLACE, AFTER }
            
            public Callback drawCallback = () => { };
            public DrawType drawType = DrawType.BEFORE;
            public string fieldName = "";
            public int fieldIndex = -1;

            public DrawEdit(string fieldName, DrawType drawType=DrawType.REPLACE, Callback drawCallback=null)
            {
                this.drawCallback = drawCallback;
                this.drawType = drawType;
                this.fieldName = fieldName;
            }

            public DrawEdit(int fieldIndex, DrawType drawType=DrawType.REPLACE, Callback drawCallback=null)
            {
                this.drawCallback = drawCallback;
                this.drawType = drawType;
                this.fieldIndex = fieldIndex;
            }
        };

        public static void DrawDefaultWithEdits(SerializedObject serializedObject, DrawEdit edit, bool expanded=true)
        {
            DrawDefaultWithEdits(serializedObject, new DrawEdit[] { edit });
        }

        public static void DrawDefaultWithEdits(SerializedObject serializedObject, DrawEdit[] edits)
        {
            var property = serializedObject.GetIterator();
            int propertyIndex = 0;

            void DrawDefaultProperty(SerializedProperty property)
            {
                if ("m_Script" == property.propertyPath)
                {
                    Disable(true, () => { EditorGUILayout.PropertyField(property); });
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);                    
                }
            }

            // bool drawAfterChildren = false;
            // int drawDepth = -1;
            // DrawEdit toDrawAfterChildren = null;
            bool expandedFirstLevel = true;
            while (property.NextVisible(expandedFirstLevel))
            {
                DrawEdit edit = edits.FirstOrDefault(edit => edit.fieldName == property.propertyPath || edit.fieldIndex == propertyIndex);
                
                if (edit is { drawType: DrawEdit.DrawType.BEFORE })
                {
                    edit.drawCallback();
                }
                        
                if (!(edit is { drawType: DrawEdit.DrawType.REPLACE }))
                {
                    DrawDefaultProperty(property);
                }
                        
                if (edit is { drawType: DrawEdit.DrawType.AFTER })
                {
                    edit.drawCallback();
                }

                expandedFirstLevel = false;
                propertyIndex++;
            }
            
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        public static bool ApiKeyPromptCheck()
        {
            bool promptShown = false;
            
            if (Configuration.GlobalConfig.ApiKey == "")
            {
                Configuration.GlobalConfig = OpenAiApi.ReadConfigFromUserDirectory();
                if (Configuration.GlobalConfig.ApiKey == "")
                {
                    OpenAiWindow.InitTab(OpenAiWindow.Tabs.help);
                    OpenAiCredentialsWindow.InitWithHelp("Please setup your API Key before using the Open AI API.", MessageType.Info);
                    promptShown = true;
                }
            }

            return promptShown;
        }
    }
}