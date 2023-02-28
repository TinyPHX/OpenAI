using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiImageReplace)), CanEditMultipleObjects]
    public class OpenAiImageReplaceEditor : Editor
    {
        public async override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(20);
            
            OpenAiImageReplace openAiImageReplace = target as OpenAiImageReplace;
            
            var style = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter};
            if (openAiImageReplace.texture != null)
            {
                GUILayout.Label((Texture2D)openAiImageReplace.texture, style);   
            }
            
            if (openAiImageReplace.textureNoBackground != null)
            {
                GUILayout.Label(openAiImageReplace.textureNoBackground, style);   
            }
            
            if (GUILayout.Button("Generate Image"))
            {
                openAiImageReplace.ReplaceImage();
            }
            
            float ColorDiff(Color a, Color b)
            {
                return (Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b)) * 255 / 3;
            }
            
            Texture2D RemoveBackground(Texture2D image)
            {
                Color[] pixels = image.GetPixels(0, 0, image.width, image.height, 0);
                for(int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
                {
                    Color pixel = pixels[pixelIndex];
                    
                    bool removePixel = false;
                    foreach (SamplePoint point in openAiImageReplace.samplePoints)
                    {
                        if (ColorDiff(point.color, pixel) < openAiImageReplace.colorSensitivity)
                        {
                            removePixel = true;
                        }
                    }

                    if (removePixel)
                    {
                        pixels[pixelIndex] = new Color(0,0,0,0); //transparent
                    }
                }
                        
                Texture2D modifiedTexture = new Texture2D(image.width, image.height);
                modifiedTexture.SetPixels(0, 0, image.width, image.height, pixels, 0);
                modifiedTexture.Apply();

                return modifiedTexture;
            }

            if (openAiImageReplace.texture)
            {
                openAiImageReplace.textureNoBackground = RemoveBackground((Texture2D)openAiImageReplace.texture);
                openAiImageReplace.SetImage();
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

                    if (openAiImageReplace.removeBackground)
                    {
                        texture2d = openAiImageReplace.textureNoBackground;
                    }
                    
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