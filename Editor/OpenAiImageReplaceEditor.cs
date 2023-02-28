using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiImageReplace)), CanEditMultipleObjects]
    public class OpenAiImageReplaceEditor : Editor
    {
        private bool previousRemoveBackgroud = false;
        private SamplePoint[] previousSamplePoints = new SamplePoint[] { };
        float previousColorSensitivity = 0;
        float previousFeatherAmount = 0;
        float previousFeatherSize = 0;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(20);
            
            OpenAiImageReplace openAiImageReplace = target as OpenAiImageReplace;

            if (GUILayout.Button("Generate Image"))
            {
                openAiImageReplace.ReplaceImage();
            }
            
            if (openAiImageReplace.texture != null)
            {
                Rect rect = GUILayoutUtility.GetRect(Screen.width, Screen.width);
                bool useNoBackground = openAiImageReplace.textureNoBackground != null && openAiImageReplace.removeBackground;
                Texture previewTexture = useNoBackground ? openAiImageReplace.textureNoBackground : openAiImageReplace.texture;
                EditorGUI.DrawPreviewTexture(rect, previewTexture, new Material(Shader.Find("Sprites/Default")));
            }

            bool settingChanged = 
                previousRemoveBackgroud != openAiImageReplace.removeBackground || 
                !previousSamplePoints.SequenceEqual(openAiImageReplace.samplePoints) || 
                previousColorSensitivity != openAiImageReplace.colorSensitivity ||
                previousFeatherAmount != openAiImageReplace.featherAmount ||
                previousFeatherSize != openAiImageReplace.featherSize;
            if (settingChanged)
            {
                previousRemoveBackgroud = openAiImageReplace.removeBackground;
                previousSamplePoints = openAiImageReplace.samplePoints.Select(samplePoint =>(SamplePoint)samplePoint.Clone()).ToArray();
                previousColorSensitivity = openAiImageReplace.colorSensitivity;
                previousFeatherAmount = openAiImageReplace.featherAmount;
                previousFeatherSize = openAiImageReplace.featherSize;

                openAiImageReplace.RemoveBackground();
            }

            // if (GUILayout.Button("Remove Background"))
            // {
            //     openAiImageReplace.RemoveBackground();
            // }

            if (GUILayout.Button("Save to File"))
            {
                Texture2D textureToSave = openAiImageReplace.removeBackground ? 
                    openAiImageReplace.textureNoBackground : 
                    (Texture2D)openAiImageReplace.texture;
                
                Utils.Image.SaveToFile("Save Generated Image", openAiImageReplace.prompt, textureToSave);
            }
        }
    }
}