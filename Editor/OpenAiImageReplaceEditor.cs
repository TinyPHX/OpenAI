using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiImageReplace)), CanEditMultipleObjects]
    public class OpenAiImageReplaceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(20);
            
            OpenAiImageReplace openAiImageReplace = target as OpenAiImageReplace;
            
            var style = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};
            if (openAiImageReplace.texture != null)
            {
                GUILayout.Label((Texture2D)openAiImageReplace.texture, style);   
            }
            
            if (GUILayout.Button("Generate Image"))
            {
                openAiImageReplace.ReplaceImage();
            }
            
            if (GUILayout.Button("Save to File"))
            {
                string guid = AssetDatabase.FindAssets ($"t:Script {nameof(OpenAiImageReplaceEditor)}")[0];
                string rootPath = AssetDatabase.GUIDToAssetPath (guid);
                
                string title = "Save Generated Image";
                string fileName = openAiImageReplace.prompt;
                char[] invalids = Path.GetInvalidFileNameChars();
                fileName = String.Join("_", fileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                fileName = fileName.Replace(" ", "_");
                string extension = "png";
                string directory = Application.dataPath + "/Packages/TP/OpenAI/Images";
                
                string newFullPath = directory + "/" + fileName + "." + extension;
                string adjustedFileName = fileName;
                int fileCount = 1;
                while(File.Exists(newFullPath)) 
                {
                    adjustedFileName = string.Format("{0}_{1}", fileName, fileCount++);
                    newFullPath = directory + "/" + adjustedFileName + "." + extension;
                }
                
                string path = EditorUtility.SaveFilePanel(title, directory, adjustedFileName, extension);
                
                if (path.Length > 0)
                {
                    Texture texture = openAiImageReplace.texture;
                    Texture2D texture2d = (Texture2D)texture;
                    byte[] bytes = texture2d.EncodeToPNG();
                    if (bytes != null)
                    {
                        File.WriteAllBytes(path, bytes);
                    }
                }
            }
        }
    }
}