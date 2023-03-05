    using System;
    using System.Linq;
    using MyBox;
    using UnityEditor.PackageManager.UI;
    using UnityEngine;

    namespace OpenAi
    {
        public class OpenAiImageReplace : MonoBehaviour
        {
            public SpriteRenderer spriteRenderer;
            public SpriteRenderer[] spriteRenderers;
            [TextAreaAttribute(1,20)] public string prompt;
            public OpenAiApi.Size size;
            [ReadOnly] public Texture texture;
            
            [Separator("Remove Background")] 
            [OverrideLabel("")] public bool removeBackground;
            [ConditionalField(nameof(removeBackground)), ReadOnly] public Texture2D textureNoBackground;
            [ConditionalField(nameof(removeBackground))] public SamplePointArray samplePoints;
            [ConditionalField(nameof(removeBackground)), Range(0,255)] public int colorSensitivity = 30;
            [ConditionalField(nameof(removeBackground)), Range(0,10)] public int featherSize = 3;
            [ConditionalField(nameof(removeBackground)), Range(0, 100)] public int featherAmount = 50;

            [Separator("Wrap Texture")] 
            [OverrideLabel("")] public bool wrap;
            [ConditionalField(nameof(wrap)), ReadOnly] public Texture2D textureWrapped;
            [ConditionalField(nameof(wrap)), Range(0, 100)] public int wrapSize = 25;
            [ConditionalField(nameof(wrap)), Range(0, 100)] public int wrapAmount = 100;
            [ConditionalField(nameof(wrap)), ReadOnly] public string newTextureSize = "";
            
            [Separator("")] 
            [ReadOnly] public bool requestPending = false;
            public delegate void Callback();
            
            public async void ReplaceImage(Callback callback=null)
            {
                OpenAiApi openai = new OpenAiApi();

                requestPending = true;
                AiImage aiImage = await openai.CreateImage(prompt, size);
                requestPending = false;
                texture = aiImage.data[0].texture;

                SelectSamplePoints();
                RemoveBackground();

                if (wrap)
                {
                    WrapTexture();
                }

                callback?.Invoke();
            }

            private void SelectSamplePoints()
            {
                Texture2D texture2d = (Texture2D)texture;
                float padding = .1f * texture2d.width;

                samplePoints = new SamplePointArray();
                samplePoints.points = new SamplePoint[] {
                    new SamplePoint(texture2d, new Vector2(padding, padding)),
                    new SamplePoint(texture2d, new Vector2(padding, texture2d.height - padding)),
                    new SamplePoint(texture2d, new Vector2(texture2d.width - padding, padding)),
                    new SamplePoint(texture2d, new Vector2(texture2d.width - padding, texture2d.height - padding)),
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
            
            private void UpdateSpriteRenderer()
            {
                Texture2D texture2d = (Texture2D)texture;
                if (removeBackground)
                {
                    texture2d = textureNoBackground;
                }
                Sprite newSprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f));
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = newSprite;
                }

                if (spriteRenderers != null)
                {
                    foreach (SpriteRenderer sprite in spriteRenderers)
                    {
                        sprite.sprite = newSprite;
                    }
                }
                
            }
            
            public void RemoveBackground()
            {
                if (texture)
                {
                    textureNoBackground = Utils.Image.RemoveBackground(
                        (Texture2D)texture,
                        colorSensitivity,
                        featherSize,
                        featherAmount,
                        samplePoints.points
                    );
                    UpdateSpriteRenderer();
                }
            }

            public void WrapTexture()
            {
                Texture textureToWrap = removeBackground ? textureNoBackground : texture;

                if (textureToWrap)
                {
                    textureWrapped = Utils.Image.WrapTexture(
                        (Texture2D)textureToWrap,
                        wrapSize,
                        wrapAmount
                    );

                    newTextureSize = textureWrapped.width + "x" + textureWrapped.height;
                    
                    UpdateSpriteRenderer();
                }
            }
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
            
            public override bool Equals(object obj) => this.Equals(obj as SamplePoint);
            public bool Equals(SamplePoint other) 
            {
                return color == other.color && position == other.position;
            }
        }
    }