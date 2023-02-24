    using System;
    using System.Net.Mime;
    using UnityEngine;
    using OpenAi;

    namespace OpenAi
    {
        public class OpenAiImageReplace : MonoBehaviour
        {
            public SpriteRenderer spriteRenderer;
            public string prompt;
            public OpenAiApi.Size size;
            public Texture texture;
            
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
            }
        }
    }