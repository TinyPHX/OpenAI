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
    }
}