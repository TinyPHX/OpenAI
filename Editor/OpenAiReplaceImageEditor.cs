using System.IO;
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
        private OpenAiReplaceImage openAiReplaceImage;
        private static Rect textureDisplayRect;
        private static Vector2 previousMousePosition = Vector2.zero;
        private Texture2D backgroundDisplay;
        
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
        private static bool previousExtendBounds = false;
        private static int previousExtended = 0;

        public override void OnInspectorGUI()
        {
            openAiReplaceImage = target as OpenAiReplaceImage;
            openAiReplaceImage.isEditorWindow = isEditorWindow;
            openAiReplaceImage.replace &= !isEditorWindow;
            
            if (AiEditorUtils.ScaledWidth < 600)
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

        new void DrawDefaultInspector()
        {
            AiEditorUtils.DrawDefaultWithEdits(serializedObject, new []
            {
                new AiEditorUtils.DrawEdit(nameof(openAiReplaceImage.extendedSize), AiEditorUtils.DrawEdit.DrawType.AFTER, () =>
                {
                    ApplyExtendedBoundsButton();
                }),
            });
        }

        void NarrowLayout()
        {
            activeWidth = AiEditorUtils.ScaledWidth - 25;
            DrawGroup1();
            GUILayout.Space(20);
            DrawGroup2();
        }

        void WideLayout()
        {
            activeWidth = AiEditorUtils.ScaledWidth / 2f - 35;
            EditorGUIUtility.labelWidth = AiEditorUtils.ScaledWidth / 5;
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
                GUILayout.Space(10);
                GenerateImageVariantButton();
                GUILayout.Space(10);
                SaveButton();
            });
            
            GUILayout.Space(5);
            
            ImagePreview();

            UpdateTexture();
        }

        void ApplyExtendedBoundsButton()
        {
            if (openAiReplaceImage.extendBounds)
            {
                if (GUILayout.Button("Apply"))
                {
                    if (!AiEditorUtils.ApiKeyPromptCheck())
                    {
                        ReplaceImage(OpenAiReplaceImage.ReplaceType.EDIT);
                    }
                }
            }
        }

        void GenerateImageButton()
        {
            EditorGUI.BeginDisabledGroup(openAiReplaceImage.requestPending);
            if (GUILayout.Button("Generate Image"))
            {
                if (!AiEditorUtils.ApiKeyPromptCheck())
                {
                    ReplaceImage(OpenAiReplaceImage.ReplaceType.CREATE);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        void GenerateImageVariantButton()
        {
            EditorGUI.BeginDisabledGroup(openAiReplaceImage.requestPending || openAiReplaceImage.texture == null);
            if (GUILayout.Button("Create Variant"))
            {
                if (!AiEditorUtils.ApiKeyPromptCheck())
                {
                    ReplaceImage(OpenAiReplaceImage.ReplaceType.VARIANT);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void ReplaceImage(OpenAiReplaceImage.ReplaceType replaceType)
        {
            string assetPath = AssetDatabase.GetAssetPath(openAiReplaceImage.gameObject);
            isPrefab = assetPath != "";

            if (isPrefab)
            {
                EditorUtility.SetDirty(openAiReplaceImage);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
                OpenAiReplaceImage prefabTarget = prefabRoot.GetComponent<OpenAiReplaceImage>();
                prefabTarget.ReplaceImage(() =>
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath, out bool success);
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }, replaceType);
            }
            else
            {
                openAiReplaceImage.ReplaceImage(type: replaceType);
            }
        }

        void SaveButton()
        {
            AiEditorUtils.Disable(openAiReplaceImage.Texture == null, () => {
                if (GUILayout.Button("Save"))
                {
                    openAiReplaceImage.SaveFinal();
                }
            });
        }

        void UpdateSamplePoints()
        {
            if (openAiReplaceImage != null && openAiReplaceImage.Texture != null && previousSamplePoints.points != null)
            {
                int textureSize = openAiReplaceImage.Texture.height;

                int newPointLength = openAiReplaceImage.samplePoints.points.Length;
                int oldPointLength = previousSamplePoints.points.Length;

                bool pointAdded = newPointLength == oldPointLength + 1;
                if (pointAdded && newPointLength > 1)
                {
                    SamplePoint lastPoint = openAiReplaceImage.samplePoints.points[newPointLength - 1];
                    SamplePoint nextToLastPoint = openAiReplaceImage.samplePoints.points[newPointLength - 2];

                    if (lastPoint.Equals(nextToLastPoint))
                    {
                        SamplePoint point = openAiReplaceImage.samplePoints.points[newPointLength - 1];
                        point.color = Color.magenta;
                        openAiReplaceImage.samplePoints.points[newPointLength - 1] = point;
                    }
                }

                bool samplePointChanged = 
                    newPointLength == oldPointLength &&
                    !SamplePoint.SequenceEqual(previousSamplePoints.points, openAiReplaceImage.samplePoints.points);
                
                if (samplePointChanged)
                {
                    for (int i = 0; i < newPointLength; i++)
                    {
                        SamplePoint newPoint = openAiReplaceImage.samplePoints.points[i];;
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

                                newPoint.color = openAiReplaceImage.texture.GetPixel(
                                    Mathf.RoundToInt(newPoint.position.x), 
                                    Mathf.RoundToInt(newPoint.position.y)
                                );
                            }
                        }

                        openAiReplaceImage.samplePoints.points[i] = newPoint;
                    }
                }
            }
        }

        void UpdateTexture()
        {
            bool removeBackgroundSettingChanged =
                openAiReplaceImage != null && (
                    previousRemoveBackground != openAiReplaceImage.removeBackground ||
                    !SamplePoint.SequenceEqual(previousSamplePoints.points, openAiReplaceImage.samplePoints.points) ||
                    previousColorSensitivity != openAiReplaceImage.colorSensitivity ||
                    previousContinuous != openAiReplaceImage.continuous ||
                    previousFeatherAmount != openAiReplaceImage.featherAmount ||
                    previousFeatherSize != openAiReplaceImage.featherSize
                );
            
            UpdateSamplePoints();

            bool toUpdate = removeBackgroundSettingChanged;

            if (toUpdate)
            {
                previousRemoveBackground = openAiReplaceImage.removeBackground;
                previousSamplePoints = new SamplePointArray();
                if (openAiReplaceImage.samplePoints.points != null)
                {
                    previousSamplePoints.points = openAiReplaceImage.samplePoints.points.Select(samplePoint =>(SamplePoint)samplePoint.Clone()).ToArray();
                }
                previousColorSensitivity = openAiReplaceImage.colorSensitivity;
                previousContinuous = openAiReplaceImage.continuous;
                previousFeatherAmount = openAiReplaceImage.featherAmount;
                previousFeatherSize = openAiReplaceImage.featherSize;
                
                openAiReplaceImage.RemoveBackground();
            }

            bool extendBoundsSettingChanged =
                openAiReplaceImage != null && (
                    previousExtendBounds != openAiReplaceImage.extendBounds ||
                    previousExtended != openAiReplaceImage.extended
                );
            
            toUpdate |= extendBoundsSettingChanged;
            
            if (toUpdate)
            {
                openAiReplaceImage.ExtendBounds();

                previousExtendBounds = openAiReplaceImage.extendBounds;
                previousExtended = openAiReplaceImage.extended;
            }
            
            bool wrapSettingChanged =
                openAiReplaceImage != null && (
                    previousWrap != openAiReplaceImage.wrap ||
                    previousWrapSize != openAiReplaceImage.wrapSize
                );

            toUpdate |= wrapSettingChanged;
            
            if (toUpdate)
            {
                previousWrap = openAiReplaceImage.wrap;
                previousWrapSize = openAiReplaceImage.wrapSize;
                
                openAiReplaceImage.WrapTexture();
            }

            bool replaceSettingChanged = previousReplace != openAiReplaceImage.replace;
            
            toUpdate |= replaceSettingChanged;
            
            if (toUpdate)
            {
                previousReplace = openAiReplaceImage.replace;
                openAiReplaceImage.ReplaceInScene();
            }
        }

        void ImagePreview()
        {
            textureDisplayRect = GUILayoutUtility.GetRect(activeWidth, activeWidth);

            if (backgroundDisplay == null)
            {
                MonoScript monoScript = MonoScript.FromScriptableObject(this);
                string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(monoScript));
                backgroundDisplay = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "/_grid.png");
            }

            if (backgroundDisplay != null)
            {
                EditorGUI.DrawPreviewTexture(textureDisplayRect, backgroundDisplay, new Material(Shader.Find("Sprites/Default")), ScaleMode.ScaleToFit);
            }
            
            if (openAiReplaceImage.texture != null)
            {
                Texture2D textureToDisplay = openAiReplaceImage.Texture;

                if (openAiReplaceImage.removeBackground)
                {
                    textureToDisplay = GetTextureWithSamplePoints(openAiReplaceImage.Texture);
                }

                if (!openAiReplaceImage.wrap || openAiReplaceImage.previewGrid == 1)
                {
                    EditorGUI.DrawPreviewTexture(textureDisplayRect, textureToDisplay, new Material(Shader.Find("Sprites/Default")), ScaleMode.ScaleToFit);
                }
                else
                {
                    int rectWidth = Mathf.RoundToInt(textureDisplayRect.size.x / openAiReplaceImage.previewGrid);
                    int rectHeight = Mathf.RoundToInt(textureDisplayRect.size.y / openAiReplaceImage.previewGrid);
                    
                    for (int xi = 0; xi < openAiReplaceImage.previewGrid; xi++)
                    {
                        for (int yi = 0; yi < openAiReplaceImage.previewGrid; yi++)
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
            
            foreach (SamplePoint samplePoint in openAiReplaceImage.samplePoints.points)
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