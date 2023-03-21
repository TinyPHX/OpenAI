using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiImageReplace)), CanEditMultipleObjects]
    public class OpenAiImageReplaceEditor : EditorWidowOrInspector<OpenAiImageReplaceEditor>
    {
        private bool isPrefab = false;
        private float activeWidth = 0;
        private OpenAiImageReplace openAiImageReplace;
        private static Rect textureDisplayRect;
        private static Vector2 previousMousePosition = Vector2.zero;
        
        // Change detection
        private static Texture2D textureWithSamplePoints;
        private static bool previousRemoveBackground = false;
        private static SamplePointArray previousSamplePoints = new SamplePointArray { };
        private static float previousColorSensitivity = 0;
        private static bool previousContinuous = true;
        private static float previousFeatherSize = 0;
        private static float previousFeatherAmount = 0;
        private static bool previousWrap = false;
        private static int previousWrapSize = 0;
        private static bool previousReplace = false;

        public override void OnInspectorGUI()
        {
            openAiImageReplace = target as OpenAiImageReplace;
            openAiImageReplace.isEditorWindow = isEditorWindow;
            openAiImageReplace.replace &= !isEditorWindow;
            
            if (Screen.width < 500)
            {
                NarrowLayout();
            }
            else
            {
                WideLayout();   
            }
            
            if (Event.current.type != EventType.ExecuteCommand && Event.current.type != EventType.Layout)
            {
                previousMousePosition = Event.current.mousePosition;
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
            activeWidth = Screen.width / 2f - 35;
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
            if (this)
            {
                DrawDefaultInspector();
            }
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
                if (!EditorUtils.ApiKeyPromptCheck())
                {
                    string assetPath = AssetDatabase.GetAssetPath(openAiImageReplace.gameObject);
                    isPrefab = assetPath != "";

                    if (isPrefab)
                    {
                        EditorUtility.SetDirty(openAiImageReplace);
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
            }
            EditorGUI.EndDisabledGroup();
        }

        void SaveButton()
        {
            EditorUtils.Disable(openAiImageReplace.Texture == null, () => {
                if (GUILayout.Button("Save to File"))
                {
                    openAiImageReplace.SaveFinal();
                }
            });
        }

        void UpdateSamplePoints()
        {
            if (openAiImageReplace != null && openAiImageReplace.Texture != null && previousSamplePoints.points != null)
            {
                int textureSize = openAiImageReplace.Texture.height;

                int newPointLength = openAiImageReplace.samplePoints.points.Length;
                int oldPointLength = previousSamplePoints.points.Length;

                bool pointAdded = newPointLength == oldPointLength + 1;
                if (pointAdded && newPointLength > 1)
                {
                    SamplePoint lastPoint = openAiImageReplace.samplePoints.points[newPointLength - 1];
                    SamplePoint nextToLastPoint = openAiImageReplace.samplePoints.points[newPointLength - 2];

                    if (lastPoint.Equals(nextToLastPoint))
                    {
                        SamplePoint point = openAiImageReplace.samplePoints.points[newPointLength - 1];
                        point.color = Color.magenta;
                        openAiImageReplace.samplePoints.points[newPointLength - 1] = point;
                    }
                }

                bool samplePointChanged = 
                    newPointLength == oldPointLength &&
                    !SamplePoint.SequenceEqual(previousSamplePoints.points, openAiImageReplace.samplePoints.points);
                
                if (samplePointChanged)
                {
                    for (int i = 0; i < newPointLength; i++)
                    {
                        SamplePoint newPoint = openAiImageReplace.samplePoints.points[i];;
                        SamplePoint oldPoint = previousSamplePoints.points[i];

                        if (newPoint.color != oldPoint.color)
                        {
                            bool captureMouse = textureDisplayRect.Contains(previousMousePosition);

                            if (captureMouse)
                            {
                                newPoint.position = (previousMousePosition - textureDisplayRect.position);
                                newPoint.position.x = (newPoint.position.x / textureDisplayRect.width) * textureSize;
                                newPoint.position.y = (newPoint.position.y / textureDisplayRect.height) * textureSize;
                                newPoint.position.y = textureSize - newPoint.position.y;

                                newPoint.color = openAiImageReplace.texture.GetPixel(
                                    Mathf.RoundToInt(newPoint.position.x), 
                                    Mathf.RoundToInt(newPoint.position.y)
                                );
                            }
                        }

                        openAiImageReplace.samplePoints.points[i] = newPoint;
                    }
                }
            }
        }

        void UpdateTexture()
        {
            bool removeBackgroundSettingChanged =
                openAiImageReplace != null && (
                    previousRemoveBackground != openAiImageReplace.removeBackground ||
                    !SamplePoint.SequenceEqual(previousSamplePoints.points, openAiImageReplace.samplePoints.points) ||
                    previousColorSensitivity != openAiImageReplace.colorSensitivity ||
                    previousContinuous != openAiImageReplace.continuous ||
                    previousFeatherAmount != openAiImageReplace.featherAmount ||
                    previousFeatherSize != openAiImageReplace.featherSize
                );
            
            UpdateSamplePoints();

            if (removeBackgroundSettingChanged)
            {
                previousRemoveBackground = openAiImageReplace.removeBackground;
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

            if (previousReplace != openAiImageReplace.replace && openAiImageReplace.texture != null)
            {
                previousReplace = openAiImageReplace.replace;
                openAiImageReplace.ReplaceInScene();
            }
        }

        void ImagePreview()
        {
            if (openAiImageReplace.texture != null)
            {
                textureDisplayRect = GUILayoutUtility.GetRect(activeWidth, activeWidth);
                Texture2D textureToDisplay = openAiImageReplace.Texture;

                if (openAiImageReplace.removeBackground)
                {
                    textureToDisplay = GetTextureWithSamplePoints(openAiImageReplace.Texture);
                }

                if (!openAiImageReplace.wrap || openAiImageReplace.previewGrid == 1)
                {
                    EditorGUI.DrawPreviewTexture(textureDisplayRect, textureToDisplay, new Material(Shader.Find("Sprites/Default")), ScaleMode.ScaleToFit);
                }
                else
                {
                    int rectWidth = Mathf.RoundToInt(textureDisplayRect.size.x / openAiImageReplace.previewGrid);
                    int rectHeight = Mathf.RoundToInt(textureDisplayRect.size.y / openAiImageReplace.previewGrid);
                    
                    for (int xi = 0; xi < openAiImageReplace.previewGrid; xi++)
                    {
                        for (int yi = 0; yi < openAiImageReplace.previewGrid; yi++)
                        {
                            Rect gridRect = new Rect(textureDisplayRect.x + rectWidth * xi, textureDisplayRect.y + rectHeight * yi, rectWidth, rectHeight);
                            EditorGUI.DrawPreviewTexture(gridRect, textureToDisplay, new Material(Shader.Find("Sprites/Default")));
                        }
                    }
                }
            }
        }

        private Texture2D GetTextureWithSamplePoints(Texture2D texture)
        {
            textureWithSamplePoints = new Texture2D(texture.width, texture.height);
            textureWithSamplePoints.SetPixels(texture.GetPixels(0, 0, texture.width, texture.height));

            int[] crossHairOffsets = new []{-5, -4, -3, -2, 2, 3, 4, 5};
            
            foreach (SamplePoint samplePoint in openAiImageReplace.samplePoints.points)
            {
                for (int xi = 0; xi < crossHairOffsets.Length; xi++)
                {
                    int x = Mathf.RoundToInt(samplePoint.position.x) + crossHairOffsets[xi];
                    int y = Mathf.RoundToInt(samplePoint.position.y);
                    Color color = textureWithSamplePoints.GetPixel(x, y);
                    Color inverted = Color.white;
                    if (color.a != 0)
                    {
                        inverted = new Color(1 - color.r, 1 - color.g, 1 - color.b, 1);
                    }
                    textureWithSamplePoints.SetPixel(x, y, inverted);
                }
                for (int yi = 0; yi < crossHairOffsets.Length; yi++)
                {
                    int x = Mathf.RoundToInt(samplePoint.position.x);
                    int y = Mathf.RoundToInt(samplePoint.position.y) + crossHairOffsets[yi];
                    Color color = textureWithSamplePoints.GetPixel(x, y);
                    Color inverted = Color.white;
                    if (color.a != 0)
                    {
                        inverted = new Color(1 - color.r, 1 - color.g, 1 - color.b, 1);
                    }
                    textureWithSamplePoints.SetPixel(x, y, inverted);
                }
            }
            textureWithSamplePoints.Apply();
            return textureWithSamplePoints;
        }
    }
}