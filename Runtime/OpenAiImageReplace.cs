    using System;
    using System.Linq;
    using MyBox;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.UI;

    namespace OpenAi
    {
        public class OpenAiImageReplace : MonoBehaviour
        {
            [TextAreaAttribute(1,20)] public string prompt;
            public OpenAiApi.Size size;
            [ReadOnly] public Texture2D texture;
            
            [Separator("Remove Background")] 
            [OverrideLabel("")] public bool removeBackground;
            [ConditionalField(nameof(removeBackground)), ReadOnly] public Texture2D textureNoBackground;
            [ConditionalField(nameof(removeBackground))] public SamplePointArray samplePoints = new SamplePointArray();
            [ConditionalField(nameof(removeBackground)), Range(0,255)] public int colorSensitivity = 25;
            [ConditionalField(nameof(removeBackground))] public bool continuous = true;
            [ConditionalField(nameof(removeBackground)), Range(0,20)] public int featherSize = 3;
            [ConditionalField(nameof(removeBackground)), Range(0, 100)] public int featherAmount = 80;

            [Separator("Wrap Texture")] 
            [OverrideLabel("")] public bool wrap;
            [ConditionalField(nameof(wrap)), ReadOnly] public Texture2D textureWrapped;
            [ConditionalField(nameof(wrap)), Range(0, 100)] public int wrapSize = 25;
            [ConditionalField(nameof(wrap)), Range(1, 10)] public int previewGrid = 2;
            [ConditionalField(nameof(wrap)), ReadOnly] public string newTextureSize = "";

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
            
            public OpenAiImageReplace ShallowCopy()
            {
                return (OpenAiImageReplace)this.MemberwiseClone();
            }
            
            public delegate void Callback();
            
            public async void ReplaceImage(Callback callback=null)
            {
                OpenAiApi openai = new OpenAiApi();

                requestPending = true;
                AiImage aiImage = await openai.CreateImage(prompt, size);
                requestPending = false;

                if (aiImage.Result == UnityWebRequest.Result.Success)
                {
                    texture = aiImage.data[0].texture;

                    SelectSamplePoints();
                    RemoveBackground();

                    if (wrap)
                    {
                        WrapTexture();
                    }

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
                    Texture2D resultTexture = texture;

                    if (textureNoBackground != null && removeBackground)
                    {
                        resultTexture = textureNoBackground;
                    }
                
                    if (textureWrapped != null && wrap)
                    {
                        resultTexture = textureWrapped;
                    }

                    return resultTexture;
                }
            }
            
            private void ReplaceInScene()
            {
                if (replace)
                {
                    Texture2D newTexture = Texture;

                    ReplaceMeshTexture(mesh.singleMesh, newTexture);
                    foreach (MeshRenderer meshRenderer in mesh.multipleMeshes)
                    {
                        ReplaceMeshTexture(meshRenderer, newTexture);
                    }
                    
                    ReplaceSpriteTexture(sprite.singleSprite, newTexture);
                    foreach (SpriteRenderer spriteRenderer in sprite.multipleSprites)
                    {
                        ReplaceSpriteTexture(spriteRenderer, newTexture);
                    }
                    
                    ReplaceImageTexture(uiImage.singleImage, newTexture);
                    foreach (Image image in uiImage.multipleImages)
                    {
                        ReplaceImageTexture(image, newTexture);
                    }
                }
            }

            private void ReplaceMeshTexture(MeshRenderer mesh, Texture2D texutre)
            {
                if (mesh)
                {
                    if (Application.isPlaying)
                    {
                        mesh.material.mainTexture = texutre;
                    }
                    else
                    {
                        Material newMaterial = new Material(mesh.sharedMaterial);
                        newMaterial.mainTexture = texutre;
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
                if (texture)
                {
                    textureNoBackground = Utils.Image.RemoveBackground(
                        texture,
                        colorSensitivity,
                        featherSize,
                        featherAmount,
                        samplePoints.points,
                        continuous
                    );
                    ReplaceInScene();
                }
            }

            public void WrapTexture()
            {
                Texture2D textureToWrap = removeBackground ? textureNoBackground : texture;

                if (textureToWrap)
                {
                    textureWrapped = Utils.Image.WrapTexture(
                        textureToWrap,
                        wrapSize
                    );

                    newTextureSize = textureWrapped.width + "x" + textureWrapped.height;
                    
                    ReplaceInScene();
                }
            }
        }

        [Serializable]
        public struct SpriteRenderers
        {
            public SpriteRenderer singleSprite;
            public SpriteRenderer[] multipleSprites;
        }

        [Serializable]
        public struct MeshRenderers
        {
            public MeshRenderer singleMesh;
            public MeshRenderer[] multipleMeshes;
        }

        [Serializable]
        public struct UiImage
        {
            public Image singleImage;
            public Image[] multipleImages;
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