using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Saab.Unity.MapAnalyzer
{
    [Serializable]
    public struct TerrainTextures
    {
        public Texture2D Diffuse;
        public Texture2D Normal;
        public float Bump;
    }

    public class TerrainGen : MonoBehaviour
    {
        public Material TerrainMaterial;
        public TerrainTextures[] DetailTextures;
        // Start is called before the first frame update

        void Awake()
        {

        #if UNITY_ANDROID
            var format = TextureFormat.ARGB32;
            Debug.Log("Tree Use ETC2");
        #else
            var format = TextureFormat.DXT5;
        #endif

            var textures = Create2DArray(DetailTextures, format);
            var normals = Create2DArray(DetailTextures, format, true);
            TerrainMaterial.SetTexture("_DetailTexs", textures);
            TerrainMaterial.SetTexture("_NormalTexs", normals);

            float[] BumpDetail = new float[10];

            for (int i = 0; i < 10; i++)
            {
                var value = 0f;

                if(DetailTextures.Count() > i)
                {
                    value = DetailTextures[i].Bump;
                }

                BumpDetail[i] = value;
            }

            TerrainMaterial.SetFloatArray("BumpDetail", BumpDetail);
        }

        private static uint NextPowerOfTwo(uint v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;

            return v + 1;
        }

        public static Texture2DArray Create2DArray(TerrainTextures[] texture, TextureFormat targetFormat, bool normal = false)
        {
            var textureCount = texture.Length;
            var textureResolution = 0;

            if (normal)
                textureResolution = Math.Max(texture.Max(item => item.Normal.width), texture.Max(item => item.Normal.height));
            else
                textureResolution = Math.Max(texture.Max(item => item.Diffuse.width), texture.Max(item => item.Diffuse.height));


            textureResolution = (int)NextPowerOfTwo((uint)textureResolution);

            Texture2DArray textureArray;

            textureArray = new Texture2DArray(textureResolution, textureResolution, textureCount, targetFormat, true)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            RenderTexture temporaryRenderTexture = new RenderTexture(textureResolution, textureResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
            {
                useMipMap = true,
                antiAliasing = 1
            };

            for (int i = 0; i < textureCount; i++)
            {
                if (normal)
                    Graphics.Blit(texture[i].Normal, temporaryRenderTexture);
                else
                    Graphics.Blit(texture[i].Diffuse, temporaryRenderTexture);

                RenderTexture.active = temporaryRenderTexture;

                //Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, "creating 2d texture: {0} x {0}", textureResolution);
                Texture2D temporaryTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, true);

                temporaryTexture.ReadPixels(new Rect(0, 0, temporaryTexture.width, temporaryTexture.height), 0, 0);
                RenderTexture.active = null;
                temporaryTexture.Apply(true);
                temporaryTexture.Compress(true);

                //TexToFile(temporaryGrassTexture, Application.dataPath + "/../grassTextureArraySaved_" + i + ".png");

                Graphics.CopyTexture(temporaryTexture, 0, textureArray, i);
                Destroy(temporaryTexture);
            }
            textureArray.Apply(false, true);

            Destroy(temporaryRenderTexture);

            return textureArray;
        }
    }
}
