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
        static float ColorDiff(Color a, Color b) => (Math.Abs(a.r - b.r) + Math.Abs(a.g - b.g) + Math.Abs(a.b - b.b)) * 255 / 3;
        
        public static Texture2D RemoveBackground(Texture2D image, int colorSensitivity, int feather, int featherAmount, SamplePoint[] samples)
        {
            Color[] pixels = image.GetPixels(0, 0, image.width, image.height, 0);
            List<int> opaquePixelIndexes = new List<int>();
            for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                Color pixel = pixels[pixelIndex];
                (int x, int y) = IndexToCoords(pixelIndex, image.width);

                bool removePixel = false;
                foreach (SamplePoint point in samples)
                {
                    if (ColorDiff(point.color, pixel) < colorSensitivity)
                    {
                        removePixel = true;
                    }
                }

                if (removePixel)
                {
                    pixels[pixelIndex] = new Color(0, 0, 0, 0); //transparent
                }
                else
                {
                    opaquePixelIndexes.Add(pixelIndex);
                }
            }
            
            Texture2D modifiedTexture = new Texture2D(image.width, image.height);
            modifiedTexture.SetPixels(0, 0, image.width, image.height, pixels, 0);
            modifiedTexture.Apply();

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

            modifiedTexture.SetPixels(0, 0, image.width, image.height, pixels, 0);
            modifiedTexture.Apply();

            return modifiedTexture;
        }

        public static void SaveToFile(string title, string name, Texture2D texture2d)
        {
            #if UNITY_EDITOR
                string fileName = name;
                char[] invalids = Path.GetInvalidFileNameChars();
                fileName = String.Join("_", fileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries))
                    .TrimEnd('.');
                fileName = fileName.Replace(" ", "_");
                string extension = "png";
                string directory = Application.dataPath + "/Packages/TP/OpenAI/Images";

                string newFullPath = directory + "/" + fileName + "." + extension;
                string adjustedFileName = fileName;
                int fileCount = 1;
                while (File.Exists(newFullPath))
                {
                    adjustedFileName = string.Format("{0}_{1}", fileName, fileCount++);
                    newFullPath = directory + "/" + adjustedFileName + "." + extension;
                }

                string path = EditorUtility.SaveFilePanel(title, directory, adjustedFileName, extension);

                if (path.Length > 0)
                {
                    byte[] bytes = texture2d.EncodeToPNG();
                    if (bytes != null)
                    {
                        File.WriteAllBytes(path, bytes);
                    }
                }
            #endif
        }
    }
}