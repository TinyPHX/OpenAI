    using System;
    using System.Collections;
    using System.Linq;
    using MyBox;
    using OpenAI.AiModels;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.UI;

    namespace OpenAi
    {
        public class OpenAiReplaceImage : MonoBehaviour
        {
            [TextArea(1,20)] public string prompt;
            public ImageSize size = ImageSize.SMALL;
            [ReadOnly] public Texture2D texture;
            [ReadOnly] public Texture2D textureVariant;
            
            [Separator("Remove Background")] 
            [OverrideLabel("")] public bool removeBackground;
            [ConditionalField(nameof(removeBackground)), ReadOnly] public Texture2D textureNoBackground;
            [ConditionalField(nameof(removeBackground))] public SamplePointArray samplePoints = new SamplePointArray();
            [ConditionalField(nameof(removeBackground)), Range(0,100)] public int colorSensitivity = 25;
            [ConditionalField(nameof(removeBackground))] public bool continuous = true;
            [ConditionalField(nameof(removeBackground)), Range(0,10)] public int featherSize = 3;
            [ConditionalField(nameof(removeBackground)), Range(0, 100)] public int featherAmount = 80;

            [Separator("Wrap Texture")] 
            [OverrideLabel("")] public bool wrap;
            [ConditionalField(nameof(wrap)), ReadOnly] public Texture2D textureWrapped;
            [ConditionalField(nameof(wrap)), Range(0, 100)] public int wrapSize = 25;
            [ConditionalField(nameof(wrap)), Range(1, 10)] public int previewGrid = 2;
            [ConditionalField(nameof(wrap)), ReadOnly] public string newTextureSize = "";

            [Separator("Extend Bounds")] 
            [OverrideLabel("")] public bool extendBounds;
            [ConditionalField(nameof(extendBounds)), ReadOnly] public Texture2D textureExtended;
            [ConditionalField(nameof(extendBounds)), Range(0, 100)] public int extended = 25;
            [ConditionalField(nameof(extendBounds)), ReadOnly] public ImageSize extendedSize = ImageSize.MEDIUM;
            
            // TODO: add image painting/editing.
            // [Separator("Edit")] 
            // [OverrideLabel("")] public bool edit;

            [Separator("Replace In Scene")] 
            [ConditionalField(nameof(isEditorWindow)), ReadOnly]
            public string WARNING = "ONLY WORKS AS COMPONENT";
            [OverrideLabel("")] public bool replace;
            [ConditionalField(nameof(replace))] public SpriteRenderers sprite = new SpriteRenderers();
            [ConditionalField(nameof(replace))] public MeshRenderers mesh = new MeshRenderers();
            [ConditionalField(nameof(replace))] public UiImage uiImage = new UiImage();
            
            [Separator("")] 
            [ReadOnly] public bool requestPending = false;

            [HideInInspector] public bool isEditorWindow;
            [SerializeField, HideInInspector] private bool componentsInitialized = false;
            
            private bool IsPrefab()
            {
                bool isPrefab = gameObject != null && (gameObject.scene.name == null ||
                                                       gameObject.gameObject != null &&
                                                       gameObject.gameObject.scene.name == null);
                return isPrefab;
            }
            
            public delegate void Callback();
            
            public void Update()
            {
                GetComponents();
            }

            public void Reset()
            {
                GetComponents();
            }

            public void GetComponents()
            {
                if (!componentsInitialized)
                {
                    componentsInitialized = true;
                    sprite.single = GetComponent<SpriteRenderer>();
                    mesh.single = GetComponent<MeshRenderer>();
                    uiImage.single = GetComponent<Image>();
                }
            }
            
            private IEnumerator RequestPendingTimout() {
                yield return new WaitForSeconds(20);
                requestPending = false;
            }
            
            public enum ReplaceType
            {
                CREATE,
                EDIT,
                VARIANT
            }
            
            public async void ReplaceImage(Callback callback=null, ReplaceType type = ReplaceType.CREATE)
            {
                OpenAiApi openai = new OpenAiApi();

                requestPending = true;
                Coroutine requestPendingTimeoutRoutine = OpenAiApi.Runner.StartCoroutine(RequestPendingTimout());
                
                AiImage aiAiImage = null;
                if (type == ReplaceType.CREATE)
                {
                    aiAiImage = await openai.CreateImage(prompt, size);
                    texture = aiAiImage.Texture;
                    textureVariant = null;
                }
                else if (type == ReplaceType.EDIT)
                {
                    size = extendedSize;
                    aiAiImage = await openai.CreateImageEdit(textureExtended, textureExtended, prompt + " extend the bounds", size);
                    texture = aiAiImage.Texture;
                    textureVariant = null;
                }
                else if (type == ReplaceType.VARIANT)
                {
                    aiAiImage = await openai.CreateImageVariant(texture, size);
                    textureVariant = aiAiImage.Texture;
                }
                
                OpenAiApi.Runner.StopCoroutine(requestPendingTimeoutRoutine);
                requestPending = false;

                if (aiAiImage != null && aiAiImage.Result == UnityWebRequest.Result.Success)
                {
                    SelectSamplePoints();
                    RemoveBackground();

                    if (wrap)
                    {
                        WrapTexture();
                    }

                    extendBounds = false;

                    callback?.Invoke();
                }
            }

            private void SelectSamplePoints()
            {
                float padding = .1f * texture.width;

                samplePoints = new SamplePointArray();
                samplePoints.points = new SamplePoint[] {
                    new SamplePoint(texture, new Vector2(padding, padding)),
                    new SamplePoint(texture, new Vector2(padding, texture.height - padding)),
                    new SamplePoint(texture, new Vector2(texture.width - padding, padding)),
                    new SamplePoint(texture, new Vector2(texture.width - padding, texture.height - padding)),
                };

                float ColorDiff(Color a, Color b)
                {
                    return (Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b)) * 255 / 3;
                }

                foreach (SamplePoint a in samplePoints.points)
                {
                    foreach (SamplePoint b in samplePoints.points)
                    {
                        if (a != b)
                        {
                            float diff = ColorDiff(a.color, b.color);
                            a.similarity += diff / samplePoints.points.Length / 2;
                            b.similarity += diff / samplePoints.points.Length / 2;
                        }
                    }
                }

                samplePoints.points = samplePoints.points.OrderBy(a => a.similarity).Take(samplePoints.points.Length - 1).ToArray(); //Drop one outlier.
            }
            
            public Texture2D Texture 
            {
                get
                {
                    Texture2D resultTexture = new []
                    {
                        (true, texture),
                        (textureVariant, textureVariant),
                        (removeBackground && textureNoBackground, textureNoBackground),
                        (extendBounds && textureExtended, textureExtended),
                        (wrap && textureWrapped, textureWrapped),
                    }.Last(tuple => tuple.Item1).Item2;
                    
                    return resultTexture;
                }
            }

            public string TextureName
            {
                get
                {
                    string textureName = prompt;

                    if (textureNoBackground && removeBackground)
                    {
                        textureName += "_alpha";
                    }
                
                    if (textureWrapped && wrap)
                    {
                        textureName += "_wrapped";
                    }

                    if (textureVariant)
                    {
                        textureName += "_variant";
                    }

                    return textureName;
                }
            }
            
            
            public void ReplaceInScene()
            {
                if (replace && Texture)
                {
                    Texture2D newTexture = Texture;

                    ReplaceMeshTexture(mesh.single, newTexture);
                    foreach (MeshRenderer meshRenderer in mesh.multiple)
                    {
                        ReplaceMeshTexture(meshRenderer, newTexture);
                    }
                    
                    ReplaceSpriteTexture(sprite.single, newTexture);
                    foreach (SpriteRenderer spriteRenderer in sprite.multiple)
                    {
                        ReplaceSpriteTexture(spriteRenderer, newTexture);
                    }
                    
                    ReplaceImageTexture(uiImage.single, newTexture);
                    foreach (Image image in uiImage.multiple)
                    {
                        ReplaceImageTexture(image, newTexture);
                    }
                }
            }

            private void ReplaceMeshTexture(MeshRenderer mesh, Texture2D texture)
            {
                if (mesh)
                {
                    if (Application.isPlaying)
                    {
                        mesh.material.mainTexture = texture;
                    }
                    else
                    {
                        Material newMaterial = new Material(mesh.sharedMaterial);
                        newMaterial.mainTexture = texture;
                        mesh.sharedMaterial = newMaterial;
                    }
                }
            }
            
            private void ReplaceSpriteTexture(SpriteRenderer sprite, Texture2D texutre)
            {
                if (sprite)
                {
                    Sprite newSprite = Sprite.Create(texutre, new Rect(0, 0, texutre.width, texutre.height), new Vector2(0.5f, 0.5f));
                    sprite.sprite = newSprite;
                }
            }
            
            private void ReplaceImageTexture(Image image, Texture2D texutre)
            {
                if (image)
                {
                    Sprite newSprite = Sprite.Create(texutre, new Rect(0, 0, texutre.width, texutre.height), new Vector2(0.5f, 0.5f));
                    image.sprite = newSprite;
                }
            }
            
            public void RemoveBackground()
            {
                Texture2D textureToModify = new []
                {
                    (true, texture),
                    (textureVariant, textureVariant),
                }.Last(tuple => tuple.Item1).Item2;
                
                if (textureToModify)
                {
                    textureNoBackground = AiUtils.Image.RemoveBackground(
                        textureToModify,
                        (int)(colorSensitivity / 100f * 255f),
                        featherSize,
                        featherAmount,
                        samplePoints.points,
                        continuous
                    );

                    if (IsPrefab())
                    {
                        textureNoBackground = SaveTemp("alpha", textureNoBackground);
                    }
                }
            }
            
            public void ExtendBounds()
            {
                if (extendBounds)
                {
                    Texture2D textureToModify = new []
                    {
                        (true, texture),
                        (textureVariant, textureVariant),
                        (removeBackground, textureNoBackground)
                    }.Last(tuple => tuple.Item1).Item2;
                    
                    Color backgroundColor = Color.white;

                    if (removeBackground && samplePoints.points.Length > 0)
                    {
                        backgroundColor = samplePoints.points[0].color;
                    }
                    
                    textureExtended = AiUtils.Image.ExtendTexture(
                        textureToModify,
                        (int)(extended / 100f * textureToModify.width),
                        backgroundColor
                    );

                    if (IsPrefab())
                    {
                        textureExtended = SaveTemp("extended", textureExtended);
                    }
                }
            }

            public void WrapTexture()
            {
                Texture2D textureToModify = new []
                {
                    (true, texture),
                    (textureVariant, textureVariant),
                    (removeBackground, textureNoBackground),
                    (extendBounds, textureExtended),
                }.Last(tuple => tuple.Item1).Item2;

                if (textureToModify)
                {
                    textureWrapped = AiUtils.Image.WrapTexture(
                        textureToModify,
                        wrapSize
                    );

                    if (IsPrefab())
                    {
                        textureWrapped = SaveTemp("wrapped", textureWrapped);
                    }

                    newTextureSize = textureWrapped.width + "x" + textureWrapped.height;
                }
                else
                {
                    textureWrapped = null;
                }
            }

            Texture2D SaveTemp(string name, Texture2D textureToSave)
            {
                if (Configuration.SaveTempImages)
                {
                    string fullName = prompt + "_temp_" + name;
                    Texture2D tempImage = AiUtils.Image.SaveTempImageToFile(fullName, textureToSave, false, true);
                    return tempImage;
                }
                else
                {
                    return textureToSave;
                }
            }

            public void SaveFinal()
            {
                if (Texture != null)
                {   
                    AiUtils.Image.SaveImageToFile(TextureName, Texture);   
                }
            }
        }

        [Serializable]
        public struct SpriteRenderers
        {
            public SpriteRenderer single;
            public SpriteRenderer[] multiple;
        }

        [Serializable]
        public struct MeshRenderers
        {
            public MeshRenderer single;
            public MeshRenderer[] multiple;
        }

        [Serializable]
        public struct UiImage
        {
            public Image single;
            public Image[] multiple;
        }
        
        
        [Serializable]
        public struct SamplePointArray { public SamplePoint[] points; } //Workaround for ConditionalField https://github.com/Deadcows/MyBox/issues/10#issuecomment-629495790
        
        [Serializable]
        public class SamplePoint : ICloneable
        {
            public Color color;
            public Vector2 position;
            [HideInInspector] public float similarity;

            public SamplePoint(Color color, Vector2 position)
            {
                this.color = color;
                this.position = position;
            }

            public SamplePoint(Texture2D texture2D, Vector2 position)
            {
                color = texture2D.GetPixel((int)position.x, (int)position.y);
                this.position = position;
            }

            public object Clone()
            {
                return new SamplePoint(color, position);
            }

            public static bool SequenceEqual(SamplePoint[] points1, SamplePoint[] points2)
            {
                if (points1 == null && points2 == null)
                {
                    return true;
                }
                else if (points1 == null || points2 == null)
                {
                    return false;
                }
                else
                {
                    return points1.SequenceEqual(points2);
                }
            }

            // https://stackoverflow.com/a/1646913
            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 31 + color.GetHashCode();
                hash = hash * 31 + position.GetHashCode();
                return hash;
            }

            public override bool Equals(object obj) => this.Equals(obj as SamplePoint);
            public bool Equals(SamplePoint other) 
            {
                return color == other.color && position == other.position;
            }
        }
    }