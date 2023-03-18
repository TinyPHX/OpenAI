using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiImageReplace)), CanEditMultipleObjects]
    public class OpenAiImageReplaceEditor : EditorWidowOrInspector<OpenAiImageReplaceEditor>
    {
        private bool isPrefab;
        private float activeWidth = 0;
        private OpenAiImageReplace openAiImageReplace;
        
        // Change detection
        private bool previousRemoveBackgroud = false;
        private SamplePointArray previousSamplePoints = new SamplePointArray { };
        private float previousColorSensitivity = 0;
        private bool previousContinuous = true;
        private float previousFeatherSize = 0;
        private float previousFeatherAmount = 0;
        private bool previousWrap = false;
        private int previousWrapSize = 0;

        public override void OnInspectorGUI()
        {
            openAiImageReplace = target as OpenAiImageReplace;
            
            if (Screen.width < 500)
            {
                NarrowLayout();
            }
            else
            {
                WideLayout();   
            }
        }

        void NarrowLayout()
        {
            activeWidth = Screen.width - 25;
            DrawGroup1();
            GUILayout.Space(20);
            DrawGroup2();
        }

        void WideLayout()
        {
            activeWidth = Screen.width / 2f - 25;
            EditorGUIUtility.labelWidth = Screen.width / 5;
            EditorUtils.Horizontal(() => {
                EditorUtils.Vertical(() => {
                    DrawGroup1();
                }, GUILayout.Width(activeWidth));
                GUILayout.Space(20);
                EditorUtils.Vertical(() => {
                    DrawGroup2();
                }, GUILayout.Width(activeWidth));
            });
        }

        void DrawGroup1()
        { 
            DrawDefaultInspector();
        }

        void DrawGroup2()
        {
            EditorUtils.Horizontal(() => {
                GenerateImageButton();
                GUILayout.Space(20);
                SaveButton();
            });
            
            ImagePreview();

            UpdateTexture();
        }

        void GenerateImageButton()
        {
            EditorGUI.BeginDisabledGroup(openAiImageReplace.requestPending);
            if (GUILayout.Button("Generate Image"))
            {
                if (Configuration.GlobalConfig.ApiKey == "")
                {
                    Configuration.GlobalConfig = OpenAiApi.ReadConfigFromUserDirectory();
                    if (Configuration.GlobalConfig.ApiKey == "")
                    {
                        OpenAiCredentialsWindow.InitWithHelp("Please setup your API Key before using the Open AI API.", MessageType.Info);
                        return;
                    }
                }

                string assetPath = AssetDatabase.GetAssetPath(openAiImageReplace.gameObject);
                isPrefab = assetPath != "";

                if (isPrefab)
                {
                    GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                    OpenAiImageReplace prefabTarget = prefabRoot.GetComponent<OpenAiImageReplace>();
                    prefabTarget.ReplaceImage(() =>
                    {
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath, out bool success);
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                        // OnUpdate?.Invoke(this);
                    });
                }
                else
                {
                    openAiImageReplace.ReplaceImage();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        void SaveButton()
        {
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

        void UpdateTexture()
        {
            bool removeBackgroundSettingChanged =
                openAiImageReplace != null && (
                    previousRemoveBackgroud != openAiImageReplace.removeBackground ||
                    !SamplePoint.SequenceEqual(previousSamplePoints.points, openAiImageReplace.samplePoints.points) ||
                    previousColorSensitivity != openAiImageReplace.colorSensitivity ||
                    previousContinuous != openAiImageReplace.continuous ||
                    previousFeatherAmount != openAiImageReplace.featherAmount ||
                    previousFeatherSize != openAiImageReplace.featherSize
                );
            
            if (removeBackgroundSettingChanged)
            {
                previousRemoveBackgroud = openAiImageReplace.removeBackground;
                previousSamplePoints = new SamplePointArray();
                if (openAiImageReplace.samplePoints.points != null)
                {
                    previousSamplePoints.points = openAiImageReplace.samplePoints.points.Select(samplePoint =>(SamplePoint)samplePoint.Clone()).ToArray();
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
                    previousWrapSize != openAiImageReplace.wrapSize
                );
            
            if (wrapSettingChanged || removeBackgroundSettingChanged)
            {
                previousWrap = openAiImageReplace.wrap;
                previousWrapSize = openAiImageReplace.wrapSize;
                
                openAiImageReplace.WrapTexture();
            }
        }

        void ImagePreview()
        {
            if (openAiImageReplace.texture != null)
            {
                // float currentWidth = EditorUtils.GetWidth();
                // if (currentWidth > 0)
                // {
                //     activeWidth = currentWidth;
                // }

                Rect rect = GUILayoutUtility.GetRect(activeWidth, activeWidth);
                Texture2D textureToDisplay = openAiImageReplace.Texture;

                if (!openAiImageReplace.wrap || openAiImageReplace.previewGrid == 1)
                {
                    EditorGUI.DrawPreviewTexture(rect, textureToDisplay, new Material(Shader.Find("Sprites/Default")), ScaleMode.ScaleToFit);
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
        }

        void SavePrefabChanges()
        {
            if (isPrefab)
            {
                string assetPath = AssetDatabase.GetAssetPath(openAiImageReplace.gameObject);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath, out bool success);
            }
        }
    }
}