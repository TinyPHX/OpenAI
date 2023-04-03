using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OpenAi
{
    [CustomEditor(typeof(OpenAiReplaceImage)), CanEditMultipleObjects]
    public class OpenAiReplaceImageEditor : EditorWidowOrInspector<OpenAiReplaceImageEditor>
    {
        private bool isPrefab = false;
        private float activeWidth = 0;
        private OpenAiReplaceImage _openAiReplaceImage;
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
            _openAiReplaceImage = target as OpenAiReplaceImage;
            _openAiReplaceImage.isEditorWindow = isEditorWindow;
            _openAiReplaceImage.replace &= !isEditorWindow;
            
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
            AiEditorUtils.Horizontal(() => {
                AiEditorUtils.Vertical(() => {
                    DrawGroup1();
                }, GUILayout.Width(activeWidth));
                GUILayout.Space(20);
                AiEditorUtils.Vertical(() => {
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
            AiEditorUtils.Horizontal(() => {
                GenerateImageButton();
                GUILayout.Space(20);
                SaveButton();
            });
            
            ImagePreview();

            UpdateTexture();
        }

        void GenerateImageButton()
        {
            EditorGUI.BeginDisabledGroup(_openAiReplaceImage.requestPending);
            if (GUILayout.Button("Generate Image"))
            {
                if (!AiEditorUtils.ApiKeyPromptCheck())
                {
                    string assetPath = AssetDatabase.GetAssetPath(_openAiReplaceImage.gameObject);
                    isPrefab = assetPath != "";

                    if (isPrefab)
                    {
                        EditorUtility.SetDirty(_openAiReplaceImage);
                        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                        OpenAiReplaceImage prefabTarget = prefabRoot.GetComponent<OpenAiReplaceImage>();
                        prefabTarget.ReplaceImage(() =>
                        {
                            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath, out bool success);
                            PrefabUtility.UnloadPrefabContents(prefabRoot);
                        });
                    }
                    else
                    {
                        _openAiReplaceImage.ReplaceImage();
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        void SaveButton()
        {
            AiEditorUtils.Disable(_openAiReplaceImage.Texture == null, () => {
                if (GUILayout.Button("Save to File"))
                {
                    _openAiReplaceImage.SaveFinal();
                }
            });
        }

        void UpdateSamplePoints()
        {
            if (_openAiReplaceImage != null && _openAiReplaceImage.Texture != null && previousSamplePoints.points != null)
            {
                int textureSize = _openAiReplaceImage.Texture.height;

                int newPointLength = _openAiReplaceImage.samplePoints.points.Length;
                int oldPointLength = previousSamplePoints.points.Length;

                bool pointAdded = newPointLength == oldPointLength + 1;
                if (pointAdded && newPointLength > 1)
                {
                    SamplePoint lastPoint = _openAiReplaceImage.samplePoints.points[newPointLength - 1];
                    SamplePoint nextToLastPoint = _openAiReplaceImage.samplePoints.points[newPointLength - 2];

                    if (lastPoint.Equals(nextToLastPoint))
                    {
                        SamplePoint point = _openAiReplaceImage.samplePoints.points[newPointLength - 1];
                        point.color = Color.magenta;
                        _openAiReplaceImage.samplePoints.points[newPointLength - 1] = point;
                    }
                }

                bool samplePointChanged = 
                    newPointLength == oldPointLength &&
                    !SamplePoint.SequenceEqual(previousSamplePoints.points, _openAiReplaceImage.samplePoints.points);
                
                if (samplePointChanged)
                {
                    for (int i = 0; i < newPointLength; i++)
                    {
                        SamplePoint newPoint = _openAiReplaceImage.samplePoints.points[i];;
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

                                newPoint.color = _openAiReplaceImage.texture.GetPixel(
                                    Mathf.RoundToInt(newPoint.position.x), 
                                    Mathf.RoundToInt(newPoint.position.y)
                                );
                            }
                        }

                        _openAiReplaceImage.samplePoints.points[i] = newPoint;
                    }
                }
            }
        }

        void UpdateTexture()
        {
            bool removeBackgroundSettingChanged =
                _openAiReplaceImage != null && (
                    previousRemoveBackground != _openAiReplaceImage.removeBackground ||
                    !SamplePoint.SequenceEqual(previousSamplePoints.points, _openAiReplaceImage.samplePoints.points) ||
                    previousColorSensitivity != _openAiReplaceImage.colorSensitivity ||
                    previousContinuous != _openAiReplaceImage.continuous ||
                    previousFeatherAmount != _openAiReplaceImage.featherAmount ||
                    previousFeatherSize != _openAiReplaceImage.featherSize
                );
            
            UpdateSamplePoints();

            if (removeBackgroundSettingChanged)
            {
                previousRemoveBackground = _openAiReplaceImage.removeBackground;
                previousSamplePoints = new SamplePointArray();
                if (_openAiReplaceImage.samplePoints.points != null)
                {
                    previousSamplePoints.points = _openAiReplaceImage.samplePoints.points.Select(samplePoint =>(SamplePoint)samplePoint.Clone()).ToArray();
                }
                previousColorSensitivity = _openAiReplaceImage.colorSensitivity;
                previousContinuous = _openAiReplaceImage.continuous;
                previousFeatherAmount = _openAiReplaceImage.featherAmount;
                previousFeatherSize = _openAiReplaceImage.featherSize;
                
                _openAiReplaceImage.RemoveBackground();
            }
            
            bool wrapSettingChanged =
                _openAiReplaceImage != null && (
                    previousWrap != _openAiReplaceImage.wrap ||
                    previousWrapSize != _openAiReplaceImage.wrapSize
                );
            
            if (wrapSettingChanged || removeBackgroundSettingChanged)
            {
                previousWrap = _openAiReplaceImage.wrap;
                previousWrapSize = _openAiReplaceImage.wrapSize;
                
                _openAiReplaceImage.WrapTexture();
            }

            if (previousReplace != _openAiReplaceImage.replace && _openAiReplaceImage.texture != null)
            {
                previousReplace = _openAiReplaceImage.replace;
                _openAiReplaceImage.ReplaceInScene();
            }
        }

        void ImagePreview()
        {
            if (_openAiReplaceImage.texture != null)
            {
                textureDisplayRect = GUILayoutUtility.GetRect(activeWidth, activeWidth);
                Texture2D textureToDisplay = _openAiReplaceImage.Texture;

                if (_openAiReplaceImage.removeBackground)
                {
                    textureToDisplay = GetTextureWithSamplePoints(_openAiReplaceImage.Texture);
                }

                if (!_openAiReplaceImage.wrap || _openAiReplaceImage.previewGrid == 1)
                {
                    EditorGUI.DrawPreviewTexture(textureDisplayRect, textureToDisplay, new Material(Shader.Find("Sprites/Default")), ScaleMode.ScaleToFit);
                }
                else
                {
                    int rectWidth = Mathf.RoundToInt(textureDisplayRect.size.x / _openAiReplaceImage.previewGrid);
                    int rectHeight = Mathf.RoundToInt(textureDisplayRect.size.y / _openAiReplaceImage.previewGrid);
                    
                    for (int xi = 0; xi < _openAiReplaceImage.previewGrid; xi++)
                    {
                        for (int yi = 0; yi < _openAiReplaceImage.previewGrid; yi++)
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
            
            foreach (SamplePoint samplePoint in _openAiReplaceImage.samplePoints.points)
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