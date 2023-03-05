using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;
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

        public static Texture2D WrapTexture(Texture2D image, int wrap, int wrapAmount)
        {
            int oldSize = image.width;
            Color[] oldPixels = image.GetPixels(0, 0, oldSize, oldSize, 0);

            int inset = (int)(wrap / 200f * image.width / 2);
            int overlapSize = inset * 2;
            int newSize = image.width - overlapSize;
            Color[] newPixels = image.GetPixels(inset, inset, newSize, newSize, 0);

            float alphaMultiplier = wrapAmount / 100f;

            // for points inside
            for (int x = 0; x < oldSize; x++)
            {
                for (int y = 0; y < oldSize; y++)
                {
                    if (x <= inset|| x > oldSize - inset || y <= inset || y > oldSize - inset)
                    {
                        int mirrorX;
                        int mirrorY;

                        float distance = 0;
                        float maxDistance = inset;
                        
                        if (x <= inset)
                        {
                            mirrorX = inset - x;
                            distance += mirrorX;
                            mirrorX = newSize - mirrorX - 1;
                        }
                        else if (x > oldSize- inset)
                        {
                            mirrorX = newSize - (x - (oldSize - inset));
                            distance += newSize - mirrorX;
                            mirrorX = newSize - mirrorX - 1;
                        }
                        else
                        {
                            mirrorX = x - inset;
                        }
                        
                        if (y <= inset)
                        {
                            mirrorY = inset - y;
                            distance += mirrorY;
                            mirrorY = newSize - mirrorY - 1;
                        }
                        else if (y > oldSize - inset)
                        {
                            mirrorY = newSize - (y - (oldSize - inset));
                            distance += newSize - mirrorY;
                            mirrorY = newSize - mirrorY - 1;
                        }
                        else
                        {
                            mirrorY = y - inset;
                        }

                        float alphaFade = 1 - (.5f + Mathf.Min(distance / maxDistance, 1) / 2);

                        int oldi = CoordsToIndex(x, y, oldSize);
                        int newi = CoordsToIndex(mirrorX, mirrorY, newSize);

                        try
                        {
                            Color oldColor = oldPixels[oldi];
                            Color newColor = newPixels[newi];

                            float oldAlpha = alphaFade;
                            float newAlpha = 1 - alphaFade;

                            newPixels[newi] = new Color(
                                oldColor.r * oldAlpha + newColor.r * newAlpha,
                                oldColor.g * oldAlpha + newColor.g * newAlpha,
                                oldColor.b * oldAlpha + newColor.b * newAlpha,
                                1
                            );
                        }
                        catch (Exception exception)
                        {
                            Debug.LogWarning(exception);
                            Debug.Log("x: " + x);
                            Debug.Log("y: " + y);
                            Debug.Log("oldSize: " + oldSize);
                            Debug.Log("mirrorX: " + mirrorX);
                            Debug.Log("mirrorY: " + mirrorY);
                            Debug.Log("newSize: " + newSize);
                            Debug.Log("oldi: " + oldi);
                            Debug.Log("newi: " + newi);
                        }
                    }
                }   
            }
            
            // for points inside
            // for (int x = 0; x < newSize; x++)
            // {
            //     for (int y = 0; y < newSize; y++)
            //     {
            //         if (x < inset || x > newSize - inset || y < inset || y > newSize - inset)
            //         {
            //             Vector2 pointTo
            //             
            //             int i = CoordsToIndex(x, y, newSize);
            //         }
            //     }   
            // }
            
            Texture2D modifiedTexture = new Texture2D(newSize, newSize);
            modifiedTexture.SetPixels(0, 0, newSize, newSize, newPixels, 0);
            modifiedTexture.Apply();

            return modifiedTexture;
        }

        private static string lastSaveFileLocation = "";
        private static string DefaultDirectory => Application.dataPath + "/Packages/TP/OpenAI/Images";
        public static string TempDirectory => Application.dataPath + "/Packages/TP/OpenAI/Images/Temp";

        public static Texture SaveToFile(string name, Texture2D texture2d, bool showDialogue=true, string directory="")
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
                while (File.Exists(newFullPath))
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
                        return default; //Canceled
                    }

                    lastSaveFileLocation = Path.GetDirectoryName(path);
                }
                
                
                //Create new texture to avoid error, Texture2D::EncodeTo functions do not support compressed texture formats.
                Color[] pixels = texture2d.GetPixels(0, 0, texture2d.width, texture2d.height, 0);
                Texture2D uncompressedTexture = new Texture2D(texture2d.width, texture2d.height);
                uncompressedTexture.SetPixels(0, 0, texture2d.width, texture2d.height, pixels, 0);
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

                Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                MakeTextureReadable(texture);

                return texture;
            #else
                return default;
            #endif
            
        }
        
        public static void MakeTextureReadable(Texture texture)
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