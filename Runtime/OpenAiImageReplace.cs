    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Mime;
    using UnityEngine;
    using OpenAi;
    using UnityEditor.PackageManager.UI;

    namespace OpenAi
    {
        public class OpenAiImageReplace : MonoBehaviour
        {
            public SpriteRenderer spriteRenderer;
            [TextAreaAttribute(1,20)] public string prompt;
            public OpenAiApi.Size size;
            public Texture texture;
            public bool removeBackground;
            public SamplePoint[] samplePoints;
            [Range(0,255)] public int colorSensitivity = 30;
            [Range(0,10)] public int featherSize = 3;
            [Range(0, 100)] public int featherAmount = 50;
            public Texture2D textureNoBackground;

            public async void ReplaceImage()
            {
                OpenAiApi openai = new OpenAiApi(this);
                
                Image image = await openai.CreateImage(prompt, size);
                texture = image.data[0].texture;

                SelectSamplePoints();
                RemoveBackground();
            }

            private void SelectSamplePoints()
            {
                Texture2D texture2d = (Texture2D)texture;
                float padding = .1f * texture2d.width;
                samplePoints = new SamplePoint[] {
                    new SamplePoint(texture2d, new Vector2(padding, padding)),
                    new SamplePoint(texture2d, new Vector2(padding, texture2d.height - padding)),
                    new SamplePoint(texture2d, new Vector2(texture2d.width - padding, padding)),
                    new SamplePoint(texture2d, new Vector2(texture2d.width - padding, texture2d.height - padding)),
                };

                float ColorDiff(Color a, Color b)
                {
                    return (Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b)) * 255 / 3;
                }

                foreach (SamplePoint a in samplePoints)
                {
                    foreach (SamplePoint b in samplePoints)
                    {
                        if (a != b)
                        {
                            float diff = ColorDiff(a.color, b.color);
                            a.similarity += diff / samplePoints.Length / 2;
                            b.similarity += diff / samplePoints.Length / 2;
                        }
                    }
                }

                samplePoints = samplePoints.OrderBy(a => a.similarity).Take(samplePoints.Length - 1).ToArray(); //Drop one outlier.
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
                        samplePoints
                    );
                    UpdateSpriteRenderer();
                }
            }
            
        }
        
        [Serializable]
        public class SamplePoint : ICloneable
        {
            public Color color;
            public Vector2 point;
            [HideInInspector] public float similarity;

            public SamplePoint(Color color, Vector2 point)
            {
                this.color = color;
                this.point = point;
            }

            public SamplePoint(Texture2D texture2D, Vector2 point)
            {
                color = texture2D.GetPixel((int)point.x, (int)point.y);
                this.point = point;
            }

            public object Clone()
            {
                return new SamplePoint(color, point);
            }
            
            public override bool Equals(object obj) => this.Equals(obj as SamplePoint);
            public bool Equals(SamplePoint other) 
            {
                return color == other.color && point == other.point;
            }
        }
    }