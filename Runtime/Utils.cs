using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OpenAi.Utils
{
    public static class Image
    {
        static (int, int) IndexToCoords(int index, int size) => (index % size, index / size);
        static int CoordsToIndex(int x, int y, int size) => size * y + x;
        static float ColorDiff(Color a, Color b) => 
            (
                Math.Abs(a.r - b.r) + 
                Math.Abs(a.g - b.g) + 
                Math.Abs(a.b - b.b) + 
                Math.Abs(a.a - b.a)
            ) * 255 / 4;

        private static Tuple<List<int>, List<int>> Fill(Color[] pixels, int size, int colorSensitivity, SamplePoint[] samples)
        {
            List<int> matched = new List<int>();
            List<int> unmatched = new List<int>();
            
            for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                Color pixel = pixels[pixelIndex];

                bool matches = false;
                foreach (SamplePoint point in samples)
                {
                    if (ColorDiff(point.color, pixel) < colorSensitivity)
                    {
                        matches = true;
                    }
                }

                if (matches)
                {
                    matched.Add(pixelIndex);
                }
                else
                {
                    unmatched.Add(pixelIndex);
                }
            }

            return new Tuple<List<int>, List<int>>(matched, unmatched);
        }

        private static Color[] FloodFill(Color[] pixels, int size, int colorSensitivity, Color[] target, Vector2 origin, Color fillColor)
        {
            Stack<int> stack = new Stack<int>();
            
            Color Get(int x, int y)
            {
                return pixels[CoordsToIndex(x, y, size)];
            }

            void Set(int x, int y)
            {
                pixels[CoordsToIndex(x, y, size)] = fillColor;
            }

            (int, int) Position(int i)
            {
                return IndexToCoords(i, size);
            }

            bool IsMatch(Color color)
            {
                return target.Any(targetColor => ColorDiff(color, targetColor) < colorSensitivity);
            }

            bool IsTargetColor(Color color)
            {
                return target.Any(targetColor => ColorDiff(color, Color.clear) < colorSensitivity);
            }

            void Push(int x, int y)
            {
                stack.Push(CoordsToIndex(x, y, size));
            }

            int i = CoordsToIndex((int)origin.x, (int)origin.y, size);

            var (x1, y1) = Position(i);
            if (IsTargetColor(Get(x1, y1)))
            {
                return pixels;
            }

            stack.Push(i);
 
            while (stack.Count > 0)
            {
                (x1, y1) = Position(stack.Pop());
                if (x1 < size && x1 >= 0 && y1 < size && y1 >= 0)
                {
                    if (IsMatch(Get(x1, y1)))
                    {
                        Set(x1, y1);
                        Push(x1 - 1, y1);
                        Push(x1 + 1, y1);
                        Push(x1, y1 - 1);
                        Push(x1, y1 + 1);
                    }
                }
                
                //Try to detect infinite loops
                if (stack.Count > size * size * 10)
                {
                    Debug.LogWarning("Flood fill recursion detected. Exiting to prevent infinite loop.");
                    break;
                }
            }

            return pixels;
        }


        public static Texture2D RemoveBackground(Texture2D image, int colorSensitivity, int feather, int featherAmount, SamplePoint[] samples, bool continuous = true)
        {
            Color[] sourcePixels = image.GetPixels(0, 0, image.width, image.height, 0);
            Color[] pixels = new Color[sourcePixels.Length];
            Array.Copy(sourcePixels, pixels, sourcePixels.Length);

            Texture2D modifiedTexture = new Texture2D(image.width, image.height);

            List<int> opaquePixelIndexes;
            if (continuous)
            {
                Color[] colorSamples = samples.Select(sample => sample.color).ToArray();
                foreach (var sample in samples)
                {
                    pixels = FloodFill(pixels, image.width, colorSensitivity, colorSamples, sample.position, Color.clear);
                }
                opaquePixelIndexes = pixels.Select((value, index) => index).Where(i => pixels[i].a > 0).ToList();
            }
            else
            {
                List<int> clearPixels;
                (clearPixels, opaquePixelIndexes) = Fill(pixels, image.width, colorSensitivity, samples);
                foreach (int clearIndex in clearPixels)
                {
                    pixels[clearIndex] = Color.clear;
                }
            }

            modifiedTexture.SetPixels(0, 0, image.width, image.height, pixels, 0);
            modifiedTexture.Apply();

            if (feather > 0)
            {
                int featherSize = 1 + feather * 2;
                float maxAlphaAmount = featherSize * featherSize * (1 - featherAmount / 100f);
                foreach (int opaqueIndex in opaquePixelIndexes)
                {
                    Color pixel = pixels[opaqueIndex];
                    (int x, int y) = IndexToCoords(opaqueIndex, image.width);

                    Color[] featherArea = new Color[] { };
                    if (x > feather && x < image.width - feather && y > feather && y < image.height - feather)
                    {
                        featherArea = modifiedTexture.GetPixels(x - feather, y - feather, featherSize, featherSize);
                    }

                    int alphaCount = featherArea.Count(color => color.a == 0);

                    if (alphaCount > 0)
                    {
                        float featherRatio = 1 - Mathf.Min(1, alphaCount / (float)maxAlphaAmount);

                        pixels[opaqueIndex] = new Color(pixel.r, pixel.g, pixel.b, featherRatio); //transparent
                    }
                }
            }

            modifiedTexture.SetPixels(0, 0, image.width, image.height, pixels, 0);
            modifiedTexture.Apply();

            return modifiedTexture;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Texture2D WrapTexture(Texture2D image, int wrap)
        {
            bool test = false;
            
            int oldSize = image.width;
            Color[] oldPixels = image.GetPixels(0, 0, oldSize, oldSize, 0);

            int inset = Mathf.RoundToInt(wrap / 100f * oldSize / 4f); //Max inset is 1 quarter of each side of the image.
            int newSize = oldSize - inset * 2;
            Color[] newPixels = image.GetPixels(inset, inset, newSize, newSize, 0);

            if (test)
            {
                oldPixels = Enumerable.Repeat(new Color(1, 1, 1, 1), oldSize * oldSize).ToArray();
                newPixels = Enumerable.Repeat(new Color(1, 1, 1, 1), newSize * newSize).ToArray(); 
            }

            Dictionary<int, List<Color>> overlay = new Dictionary<int, List<Color>>();
            
            // for points inside
            for (int x = 0; x < oldSize; x++)
            {
                for (int y = 0; y < oldSize; y++)
                {
                    bool xLow = x < inset;
                    bool xHigh = x >= newSize + inset;
                    bool yLow = y < inset;
                    bool yHigh = y >= newSize + inset;
                    
                    if ((xLow || xHigh) ^ (yLow || yHigh))
                    {
                        int Flip(int position)
                        {
                            return newSize - position - 1;
                        }
                        
                        // If you dont write this with ternary hell you have to write it will if statement hell. I'm sorry.  
                        int xOutset = xLow ? inset - x : xHigh ? x - (newSize + inset) + 1 : 0;
                        int yOutset = yLow ? inset - y : yHigh ? y - (newSize + inset) + 1 : 0;
                        int mirrorX = xLow ? Flip(xOutset - 1) : xHigh ? Flip(newSize - xOutset) : x - inset;
                        int mirrorY = yLow ? Flip(yOutset - 1) : yHigh ? Flip(newSize - yOutset) : y - inset;
                        int xInset = mirrorX < inset ? mirrorX + 1 : mirrorX > newSize - inset ? newSize - mirrorX : inset;
                        int yInset = mirrorY < inset ? mirrorY + 1 : mirrorY > newSize - inset ? newSize - mirrorY : inset;

                        float maxDistance = inset;
                        float xDistance = 0;
                        float yDistance = 0;
                        
                        if (xOutset > 0 && yOutset == 0)
                        {
                            float cornerRatio = (xOutset - 1) / (float)yInset;
                            xDistance = inset * cornerRatio;
                        }
                        
                        if (yOutset > 0 && xOutset == 0)
                        {
                            float cornerRatio = (yOutset - 1) / (float)xInset;
                            yDistance = inset * cornerRatio;
                        }
                        
                        float distance = xDistance + yDistance;
                        float distanceRatio = 1 - Mathf.Min(distance / maxDistance, 1);
                        
                        bool outOfImageBounds = mirrorX >= newSize || mirrorX < 0 || mirrorY >= newSize || mirrorY < 0;
                        if (outOfImageBounds)
                        {
                            Debug.LogWarning($"(x: {mirrorX},y: {mirrorY}) out of bounds with size (x: {newSize},y: {newSize})");   
                        }
                        else
                        {
                            if (test)
                            {
                                Set(mirrorX, mirrorY, new Color(
                                    .5f,
                                    .5f,
                                    .5f,
                                    distanceRatio
                                ));
                            }
                            else
                            {
                                Color oldColor = GetOld(x, y);
                                oldColor.a *= distanceRatio;
                                Set(mirrorX, mirrorY, oldColor);
                            }
                        }
                        
                        Color GetOld(int x, int y)
                        {
                            int i = CoordsToIndex(x, y, oldSize);
                            return oldPixels[i];
                        }

                        void Set(int x, int y, Color color)
                        {
                            int i = CoordsToIndex(x, y, newSize);
                            if (!overlay.TryGetValue(i, out var colors))
                            {
                                overlay[i] = new List<Color>{ color };
                            }
                            else
                            {
                                colors.Add(color);
                                overlay[i] = colors;
                            }
                        }
                    }
                }   
            }

            foreach (int i in overlay.Keys)
            {
                if (i >= 0 && i < newPixels.Length)
                { 
                    newPixels[i] = Average(newPixels[i], overlay[i]);
                }
                else
                {
                    (int x, int y) = IndexToCoords(i, newSize);
                    Debug.LogWarning(
                        $"{i} (x: {x},y: {y}) out of bounds of array with length {newPixels.Length} and size (x: {newSize}, y: {newSize})");
                }
            }
            
            Color Average(Color baseColor, List<Color> colors)
            {
                colors.Sort((color1, color2) => (int)((color1.a - color2.a) * 100));
                Color averageOfColors = colors[0];
                
                for (int i = 1; i < colors.Count; i++)
                {
                    Color color = colors[i];
                    float alphaSum = averageOfColors.a + color.a;
                    if (alphaSum != 0)
                    {
                        float lerp = ((color.a - averageOfColors.a) / 2 + .5f) / alphaSum;
                        lerp /= i;

                        lerp = Mathf.Clamp01(lerp);
                        averageOfColors = new Color(
                            averageOfColors.r + (color.r - averageOfColors.r) * lerp, 
                            averageOfColors.g + (color.g - averageOfColors.g) * lerp, 
                            averageOfColors.b + (color.b - averageOfColors.b) * lerp, 
                            averageOfColors.a + (color.a - averageOfColors.a) * lerp //Not used
                        );
                    }
                }
                float maxAlpha = colors.Max(color => color.a);
                averageOfColors.a = maxAlpha;
                float alpha = averageOfColors.a * (averageOfColors.a / (averageOfColors.a + baseColor.a));
                
                Color averageColor = new Color(
                    baseColor.r * (1 - alpha) + averageOfColors.r * alpha,
                    baseColor.g * (1 - alpha) + averageOfColors.g * alpha,
                    baseColor.b * (1 - alpha) + averageOfColors.b * alpha,
                    Mathf.Max(baseColor.a, averageOfColors.a) 
                );

                return averageColor;
            }

            Texture2D modifiedTexture = new Texture2D(newSize, newSize);
            modifiedTexture.SetPixels(0, 0, newSize, newSize, newPixels, 0);
            modifiedTexture.Apply();

            return modifiedTexture;
        }

        private static string lastSaveFileLocation = "";
        private static string DefaultDirectory => Application.dataPath + "/Packages/TP/OpenAI/Images";
        public static string TempDirectory => Application.dataPath + "/Packages/TP/OpenAI/Images/Temp";

        public static Texture2D SaveToFile(string name, Texture2D texture, bool showDialogue=true, string directory="", bool overwrite=false)
        {
            #if UNITY_EDITOR
                if (!Directory.Exists(DefaultDirectory)) { Directory.CreateDirectory(DefaultDirectory); }
                if (!Directory.Exists(TempDirectory)) { Directory.CreateDirectory(TempDirectory); }
            
                string fileName = name;
                char[] invalids = Path.GetInvalidFileNameChars();
                fileName = String.Join("_", fileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)) .TrimEnd('.');
                fileName = fileName.Replace(" ", "_");
                string extension = "png";
                if (directory == "")
                {
                    if (lastSaveFileLocation != "")
                    {
                        directory = lastSaveFileLocation;
                    }
                    else
                    {
                        directory = DefaultDirectory;
                    }
                }

                string newFullPath = directory + "/" + fileName + "." + extension;
                string adjustedFileName = fileName;
                int fileCount = 1;
                while (File.Exists(newFullPath) && !overwrite)
                {
                    adjustedFileName = string.Format("{0}_{1}", fileName, fileCount++);
                    newFullPath = directory + "/" + adjustedFileName + "." + extension;
                }

                string path = newFullPath;

                if (showDialogue)
                {
                    path = EditorUtility.SaveFilePanel("Save Image", directory, adjustedFileName, extension);
                    if (path.Length == 0)
                    {
                        GUIUtility.ExitGUI();
                        return default; //Canceled
                    }

                    lastSaveFileLocation = Path.GetDirectoryName(path);
                }
                
                //Create new texture to avoid error, Texture2D::EncodeTo functions do not support compressed texture formats.
                Color[] pixels = texture.GetPixels(0, 0, texture.width, texture.height, 0);
                Texture2D uncompressedTexture = new Texture2D(texture.width, texture.height);
                uncompressedTexture.SetPixels(0, 0, texture.width, texture.height, pixels, 0);
                uncompressedTexture.Apply();

                byte[] bytes = uncompressedTexture.EncodeToPNG();
                if (bytes != null)
                {
                    File.WriteAllBytes(path, bytes);
                }
                
                AssetDatabase.Refresh();
                
                string assetPath = "";
                if (path.StartsWith(Application.dataPath)) {
                    assetPath = "Assets" + path.Substring(Application.dataPath.Length);
                }

                Texture2D newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                MakeTextureReadable(newTexture);

                if (showDialogue)
                {
                    System.Threading.Thread.Sleep(1000); // Stupid hack: https://forum.unity.com/threads/endlayoutgroup-beginlayoutgroup-must-be-called-first.523209/#post-3652876
                    GUIUtility.ExitGUI();
                }

                return newTexture;
            #else
                return texture;
            #endif
            
        }
        
        public static void MakeTextureReadable(Texture2D texture)
        {
            if (texture != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(texture);
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (textureImporter != null)
                {
                    textureImporter.textureType = TextureImporterType.Default;
                    textureImporter.isReadable = true;
                    AssetDatabase.ImportAsset(assetPath);
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}