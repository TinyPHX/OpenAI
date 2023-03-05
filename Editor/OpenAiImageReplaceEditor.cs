using System.Linq;
using NUnit.Framework.Constraints;
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
        private float previousColorSensitivity = 0;
        private float previousFeatherSize = 0;
        private float previousFeatherAmount = 0;

        private bool previousWrap = false;
        private int previousWrapSize = 0;
        private int previousWrapAmount = 0;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(20);
            
            OpenAiImageReplace openAiImageReplace = target as OpenAiImageReplace;

            EditorGUI.BeginDisabledGroup(openAiImageReplace.requestPending);
            if (GUILayout.Button("Generate Image"))
            {
                string assetPath = AssetDatabase.GetAssetPath(openAiImageReplace.gameObject);
                bool isPrefab = assetPath != "";

                if (isPrefab)
                {
                    GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                    OpenAiImageReplace prefabTarget = prefabRoot.GetComponent<OpenAiImageReplace>();
                    prefabTarget.ReplaceImage(() =>
                    {
                        // PrefabUtility.SaveAsPrefabAsset(openAiImageReplace.gameObject, assetPath);

                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath, out bool success);
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                        
                        Debug.Log("Prefab update: " + (success ? "successful" : "failed"));
                    });
                }
                else
                {
                    openAiImageReplace.ReplaceImage();
                }
            }
            EditorGUI.EndDisabledGroup();
            
            if (openAiImageReplace.texture != null)
            {
                Rect rect = GUILayoutUtility.GetRect(Screen.width, Screen.width);
                Texture textureToDisplay = openAiImageReplace.texture;

                if (openAiImageReplace.textureNoBackground != null && openAiImageReplace.removeBackground)
                {
                    textureToDisplay = openAiImageReplace.textureNoBackground;
                }
                
                if (openAiImageReplace.textureWrapped != null && openAiImageReplace.wrap)
                {
                    textureToDisplay = openAiImageReplace.textureWrapped;
                }
                
                EditorGUI.DrawPreviewTexture(rect, textureToDisplay, new Material(Shader.Find("Sprites/Default")));
            }

            bool removeBackgroundSettingChanged =
                openAiImageReplace != null && (
                    previousRemoveBackgroud != openAiImageReplace.removeBackground ||
                    !previousSamplePoints.SequenceEqual(openAiImageReplace.samplePoints.points) ||
                    previousColorSensitivity != openAiImageReplace.colorSensitivity ||
                    previousFeatherAmount != openAiImageReplace.featherAmount ||
                    previousFeatherSize != openAiImageReplace.featherSize
                );
            
            if (removeBackgroundSettingChanged)
            {
                previousRemoveBackgroud = openAiImageReplace.removeBackground;
                previousSamplePoints = openAiImageReplace.samplePoints.points.Select(samplePoint =>(SamplePoint)samplePoint.Clone()).ToArray();
                previousColorSensitivity = openAiImageReplace.colorSensitivity;
                previousFeatherAmount = openAiImageReplace.featherAmount;
                previousFeatherSize = openAiImageReplace.featherSize;

                openAiImageReplace.RemoveBackground();
            }
            
            bool wrapSettingChanged =
                openAiImageReplace != null && (
                    previousWrap != openAiImageReplace.wrap ||
                    previousWrapSize != openAiImageReplace.wrapSize ||
                    previousWrapAmount != openAiImageReplace.wrapAmount
                );

            if (wrapSettingChanged)
            {
                previousWrap = openAiImageReplace.wrap;
                previousWrapSize = openAiImageReplace.wrapSize;
                previousWrapAmount = openAiImageReplace.wrapAmount;

                openAiImageReplace.WrapTexture();
            }

            if (GUILayout.Button("Save to File"))
            {
                Texture textureToSave = openAiImageReplace.texture;

                if (openAiImageReplace.textureNoBackground != null && openAiImageReplace.removeBackground)
                {
                    textureToSave = openAiImageReplace.textureNoBackground;
                }
                
                if (openAiImageReplace.textureWrapped != null && openAiImageReplace.wrap)
                {
                    textureToSave = openAiImageReplace.textureWrapped;
                }
                
                Utils.Image.SaveToFile(openAiImageReplace.prompt, (Texture2D)textureToSave, true);
            }
        }
    }
}