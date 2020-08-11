using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    [Serializable]
    public struct TerrainTextures
    {
        public Texture2D FeatureTexture;

        public Vector2 Height;
        public Vector2 Width;
        public float Yoffset;

        public Vector4 GetMinMaxWidthHeight
        {
            get
            {
                return new Vector4(Width.x, Width.y, Height.x, Height.y);
            }
        }
    }
    public struct ComputeShaderID
    {
        // Buffers
        static public int surfaceVertices = Shader.PropertyToID("surfaceVertices");
        static public int surfaceIndices = Shader.PropertyToID("surfaceIndices");
        static public int surfaceUVs = Shader.PropertyToID("surfaceUVs");

        // Calculated points
        static public int terrainBuffer = Shader.PropertyToID("terrainPoints");
        //cull
        static public int cullInBuffer = Shader.PropertyToID("Input");
        static public int cullOutBuffer = Shader.PropertyToID("Output");

        static public int closeBuffer = Shader.PropertyToID("closeBuffer");

        // Indirect buffer
        static public int indirectBuffer = Shader.PropertyToID("indirectBuffer");

        // Textures           
        static public int splatMap = Shader.PropertyToID("splatMap");
        static public int nodeTexture = Shader.PropertyToID("NodeTexture");
        static public int placementMap = Shader.PropertyToID("PlacementMap");

        // Scalars & vectors
        static public int objToWorld = Shader.PropertyToID("ObjToWorld");

        static public int surfaceGridStep = Shader.PropertyToID("surfaceGridStep");
        static public int cullCount = Shader.PropertyToID("cullCount");
        static public int indexCount = Shader.PropertyToID("indexCount");

        // Size
        static public int size = Shader.PropertyToID("surfaceSize");

        static public int frustumPlanes = Shader.PropertyToID("frustumPlanes");
        static public int terrainResolution = Shader.PropertyToID("terrainResolution");
    }

    public class TerrainFeature : MonoBehaviour
    {
        // camera frustum planes, updated in render
        public Vector4[] _frustum = new Vector4[6];
        // Rendering settings
        [Header("****** RENDER SETTINGS ******")]
        public Shader Shader;
        public int DrawDistance = 500;
        public float NearFadeStart = 5;
        public float NearFadeEnd = 5;
        public bool DrawShadows = true;
        public float Wind = 0.0f;
        public float Density = 22.127f;

        private enum Sides
        {
            Full = 0,
            Front = 1 << 0,
            Side = 1 << 1,
            Top = 1 << 2,
        };

        // Active rendered item
        public struct Item : IDisposable
        {
            // source object
            public GameObject GameObject;

            // culls instances and output to rendering buffer
            public CullingShader CullShader;

            // local bounds of the object
            public Bounds Bounds;

            public void Dispose()
            {
                CullShader.Dispose();
            }
        }

        // GPU work waiting to be processed by the CPU and placed into the active items list
        public struct JobOutput
        {
            public GameObject GameObject;
            public ComputeBuffer PointBuffer;
            public Bounds Bounds;
            public InstanceGenerator Generator;
        }

        // Objects not yet processed, will be processed by the GPU in batches during update
        public struct PendingJob
        {
            public GameObject GameObject;
            public Mesh Mesh;
            public Texture2D Diffuse;
        }

        public int GetModuleBufferMemory(List<Item> currentJobs)
        {
            var size = 0;

            foreach (var b in currentJobs)
            {
                size += (b.CullShader.GetBufferSize * sizeof(float) * 4);
            }
            return size;
        }

        public void GenerateFrustumPlane(Camera camera)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);


            for (int i = 0; i < 6; i++)
            {
                _frustum[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
            }

            _frustum[5].w = DrawDistance;
        }

        public bool IsInFrustum(Vector3 positionAfterProjection, float treshold = -1)
        {
            float cullValue = treshold;

            return (Vector3.Dot(_frustum[0], positionAfterProjection) >= cullValue &&
                Vector3.Dot(_frustum[1], positionAfterProjection) >= cullValue &&
                Vector3.Dot(_frustum[2], positionAfterProjection) >= cullValue &&
                Vector3.Dot(_frustum[3], positionAfterProjection) >= cullValue) &&
            (_frustum[5].w >= Mathf.Abs(Vector3.Distance(Vector3.zero, positionAfterProjection)));
        }

        public static Texture2DArray Create2DArray(TerrainTextures[] texture, TextureFormat targetFormat)
        {
            var textureCount = texture.Length;

            var textureResolution = Math.Max(texture.Max(item => item.FeatureTexture.width), texture.Max(item => item.FeatureTexture.height));

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
                Graphics.Blit(texture[i].FeatureTexture, temporaryRenderTexture);
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

        // TODO: Shaders should use the TRANSFORM of the node to 
        public Transform FindFirstNodeParent(Transform child)
        {
            var parent = child.parent;
            if (parent == null)
            {
                return child;
            }

            var node = parent.GetComponent<NodeHandle>();

            if (node == null)
            {
                return child;
            }

            return FindFirstNodeParent(parent);
        }

        public Vector4[] GetQuads(TerrainTextures[] texture, bool fullImage = false)
        {
            // NOTE: GetPixels() is slowing this down, and we cant use multithreading to help with this,
            // so instead to improve the performance of this code, try using the GPU


            var quads = new Vector4[texture.Length * 3];
            var i = 0;

            if (fullImage)
            {
                foreach (TerrainTextures terrain in texture)
                {
                    quads[i++] = GetQuad(terrain.FeatureTexture, Sides.Full);
                    quads[i++] = GetQuad(terrain.FeatureTexture, Sides.Full);
                    quads[i++] = GetQuad(terrain.FeatureTexture, Sides.Full);
                }
            }
            else
            {
                foreach (TerrainTextures terrain in texture)
                {
                    quads[i++] = GetQuad(terrain.FeatureTexture, Sides.Front);
                    quads[i++] = GetQuad(terrain.FeatureTexture, Sides.Side);
                    quads[i++] = GetQuad(terrain.FeatureTexture, Sides.Top);
                }
            }

            return quads;
        }

        private static Vector4 GetQuad(Texture2D texture, Sides side)
        {
            // get pixel data for a 1/4 'side' sub image
            var pixels = GrabRect(texture, side);

            // get minimum fit
            var rc = FitMinimumUV(pixels, texture.width / 2);

            var w = texture.width / 2f;
            var h = texture.height / 2f;

            return new Vector4(rc.xMin / w, rc.xMax / w, rc.yMin / h, rc.yMax / h);
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

        private static Color[] GrabRect(Texture2D source, Sides side)
        {
            int sx = 0;
            int sy = 0;

            switch (side)
            {
                case Sides.Front:
                    sx = 0;
                    sy = source.height / 2;
                    break;
                case Sides.Side:
                    sx = 0;
                    sy = 0;
                    break;
                case Sides.Top:
                    sx = source.width / 2;
                    sy = 0;
                    break;
                case Sides.Full:
                    return source.GetPixels(0, 0, source.width, source.height, 0);
            }

            return source.GetPixels(sx, sy, source.width / 2, source.height / 2, 0);
        }

        private static RectInt FitMinimumUV(Color[] data, int width)
        {
            var height = data.Length / width;

            const float cutoff = 0.9f;

            int maxX = 0;
            int maxY = 0;
            int minX = width - 1;
            int minY = height - 1;

            for (int y = 0; y < height; y++)
            {
                int lx = 0;

                var p = y * width;

                for (; lx < width; lx++)
                {
                    var color = data[p++];
                    if (color.a > cutoff)
                    {
                        minX = lx < minX ? lx : minX;
                        minY = y < minY ? y : minY;
                        maxY = y > maxY ? y : maxY;
                        break;
                    }
                }

                p = (y + 1) * width - 1;

                for (var rx = width - 1; rx >= lx; rx--)
                {
                    var color = data[p--];
                    if (color.a > cutoff)
                    {
                        maxX = rx > maxX ? rx : maxX;
                        break;
                    }
                }
            }

            return new RectInt(minX, minY, maxX - minX, maxY - minY);
        }
    }
}