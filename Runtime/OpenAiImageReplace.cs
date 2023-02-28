    using System;
    using System.Linq;
    using System.Net.Mime;
    using UnityEngine;
    using OpenAi;
    using UnityEditor.PackageManager.UI;

    namespace OpenAi
    {
        [Serializable]
        public class SamplePoint
        {
            public Color color;
            public Vector2 point;
            public float similarity;

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
        }
        
        public class OpenAiImageReplace : MonoBehaviour
        {
            public SpriteRenderer spriteRenderer;
            public string prompt;
            public OpenAiApi.Size size;
            public Texture texture;
            public bool removeBackground;
            public SamplePoint[] samplePoints;
            [Range(0,255)] public int colorSensitivity = 30;
            public Texture2D textureNoBackground;

            
            public async void ReplaceImage()
            {
                OpenAiApi openai = new OpenAiApi(this);
                
                Image image = await openai.CreateImage(prompt, size);
                texture = image.data[0].texture;
                Texture2D texture2d = (Texture2D)texture;
                Sprite newSprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f));
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = newSprite;
                }
                
                // openai.CreateImage(prompt, size, image =>
                // {
                //     texture = image.data[0].texture;
                //     Debug.Log(texture);
                //     Texture2D texture2d = (Texture2D)texture;
                //     // Texture2D blankTexture = new Texture2D(1024, 1024, TextureFormat.RGBA32, true);
                //     Sprite newSprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f));
                //     if (spriteRenderer != null)
                //     {
                //         spriteRenderer.sprite = newSprite;
                //     }
                // });
                
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
            
            public void SetImage()
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
        }
    }