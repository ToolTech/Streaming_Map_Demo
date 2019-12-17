using System;
using UnityEngine;

namespace Saab.Unity.Core
{
    public class NoiseTexture
    {
        public enum TextureFormat
        {
            RFloat = UnityEngine.TextureFormat.RFloat,
            RGFloat = UnityEngine.TextureFormat.RGFloat,
            RGBAFloat = UnityEngine.TextureFormat.RGBAFloat,
        }

        // Default texture for saving GPU memory
        private static Texture2D uniformR128x1;
        public static Texture2D UniformR128x1
        {
            get
            {
                if (uniformR128x1 == null)
                {
                    uniformR128x1 = GenerateUniform(128, 1, TextureFormat.RFloat, false);
                }
                return uniformR128x1;
            }
        }

        private static Texture2D uniformR128x128;
        public static Texture2D UniformR128x128
        {
            get
            {
                if (uniformR128x128 == null)
                {
                    uniformR128x128 = GenerateUniform(128, 128, TextureFormat.RFloat, false);
                }
                return uniformR128x128;
            }
        }

        private static Texture2D uniformRGBA128x128;
        public static Texture2D UniformRGBA128x128
        {
            get
            {
                if (uniformRGBA128x128 == null)
                {
                    uniformRGBA128x128 = GenerateUniform(128, 128, TextureFormat.RGBAFloat, false);
                }
                return uniformRGBA128x128;
            }
        }

        private static Texture2D uniformRGBA256x256;
        public static Texture2D UniformRGBA256x256
        {
            get
            {
                if (uniformRGBA256x256 == null)
                {
                    uniformRGBA256x256 = GenerateUniform(256, 256, TextureFormat.RGBAFloat, false);
                }
                return uniformRGBA256x256;
            }
        }

        private static Texture2D uniformRGBA512x512;
        public static Texture2D UniformRGBA512x512
        {
            get
            {
                if (uniformRGBA512x512 == null)
                {
                    uniformRGBA512x512 = GenerateUniform(512, 512, TextureFormat.RGBAFloat, false);
                }
                return uniformRGBA512x512;
            }
        }

        public static void ReleaseAll()
        {
            UnityEngine.Object.Destroy(uniformR128x1);
            UnityEngine.Object.Destroy(uniformRGBA128x128);
            UnityEngine.Object.Destroy(uniformRGBA256x256);
            UnityEngine.Object.Destroy(uniformRGBA512x512);

            uniformR128x1 = null;
            uniformRGBA128x128 = null;
            uniformRGBA256x256 = null;
            uniformRGBA512x512 = null;
        }

        public static Texture2D GenerateUniform(int width, int height, TextureFormat textureFormat, bool linear)
        {
            Texture2D texture = new Texture2D(width, height, (UnityEngine.TextureFormat)textureFormat, false, linear);

            float[] floatBuffer = new float[width * height * GetChannelsCount(textureFormat)];
            for (int i = 0; i < floatBuffer.Length; i++)
            {
                floatBuffer[i] = UnityEngine.Random.value;
            }

            byte[] byteArray = new byte[floatBuffer.Length * sizeof(float)];

            Buffer.BlockCopy(floatBuffer, 0, byteArray, 0, byteArray.Length);

            texture.LoadRawTextureData(byteArray);
            texture.Apply();

            return texture;
        }

        private static int GetChannelsCount(TextureFormat textureFormat)
        {
            switch (textureFormat)
            {
                case TextureFormat.RFloat:
                    {
                        return 1;
                    }
                case TextureFormat.RGFloat:
                    {
                        return 2;
                    }
                default:
                    {
                        return 4;
                    }
            }
        }
    }
}
