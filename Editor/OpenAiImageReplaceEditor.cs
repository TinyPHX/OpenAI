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
        private bool previousContinuous = true;
        private float previousFeatherSize = 0;
        private float previousFeatherAmount = 0;

        private bool previousWrap = false;
        private int previousWrapSize = 0;
        private int previousPreviewGrid = 0;
        
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
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath, out bool success);
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
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
                Texture2D textureToDisplay = openAiImageReplace.Texture;

                if (!openAiImageReplace.wrap || openAiImageReplace.previewGrid == 1)
                {
                    EditorGUI.DrawPreviewTexture(rect, textureToDisplay, new Material(Shader.Find("Sprites/Default")));
                }
                else
                {
                    int rectWidth = Mathf.RoundToInt(rect.size.x / openAiImageReplace.previewGrid);
                    int rectHeight = Mathf.RoundToInt(rect.size.y / openAiImageReplace.previewGrid);
                    
                    for (int xi = 0; xi < openAiImageReplace.previewGrid; xi++)
                    {
                        for (int yi = 0; yi < openAiImageReplace.previewGrid; yi++)
                        {
                            Rect gridRect = new Rect(rect.x + rectWidth * xi, rect.y + rectHeight * yi, rectWidth, rectHeight);
                            EditorGUI.DrawPreviewTexture(gridRect, textureToDisplay, new Material(Shader.Find("Sprites/Default")));
                        }
                    }
                }
            }

            bool removeBackgroundSettingChanged =
                openAiImageReplace != null && (
                    previousRemoveBackgroud != openAiImageReplace.removeBackground ||
                    !SamplePoint.SequenceEqual(previousSamplePoints, openAiImageReplace.samplePoints.points) ||
                    previousColorSensitivity != openAiImageReplace.colorSensitivity ||
                    previousContinuous != openAiImageReplace.continuous ||
                    previousFeatherAmount != openAiImageReplace.featherAmount ||
                    previousFeatherSize != openAiImageReplace.featherSize
                );
            
            if (removeBackgroundSettingChanged)
            {
                previousRemoveBackgroud = openAiImageReplace.removeBackground;
                if (openAiImageReplace.samplePoints.points == null)
                {
                    previousSamplePoints = null;
                }
                else
                {
                    previousSamplePoints = openAiImageReplace.samplePoints.points.Select(samplePoint =>(SamplePoint)samplePoint.Clone()).ToArray();
                }
                previousColorSensitivity = openAiImageReplace.colorSensitivity;
                previousContinuous = openAiImageReplace.continuous;
                previousFeatherAmount = openAiImageReplace.featherAmount;
                previousFeatherSize = openAiImageReplace.featherSize;

                openAiImageReplace.RemoveBackground();
            }
            
            bool wrapSettingChanged =
                openAiImageReplace != null && (
                    previousWrap != openAiImageReplace.wrap ||
                    previousWrapSize != openAiImageReplace.wrapSize ||
                    previousPreviewGrid != openAiImageReplace.previewGrid
                );

            if (wrapSettingChanged || removeBackgroundSettingChanged)
            {
                previousWrap = openAiImageReplace.wrap;
                previousWrapSize = openAiImageReplace.wrapSize;
                previousPreviewGrid = openAiImageReplace.previewGrid;

                openAiImageReplace.WrapTexture();
            }

            // Save to file
            Texture2D textureToSave = openAiImageReplace.texture;
            string name = openAiImageReplace.prompt;
            if (openAiImageReplace.textureNoBackground != null && openAiImageReplace.removeBackground)
            {
                textureToSave = openAiImageReplace.textureNoBackground;
                name += "_alpha";
            }
            if (openAiImageReplace.textureWrapped != null && openAiImageReplace.wrap)
            {
                textureToSave = openAiImageReplace.textureWrapped;
                name += "_wrapped";
            }
            EditorGUI.BeginDisabledGroup(textureToSave == null);
            if (GUILayout.Button("Save to File"))
            {
                Utils.Image.SaveToFile(name, textureToSave, true);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}