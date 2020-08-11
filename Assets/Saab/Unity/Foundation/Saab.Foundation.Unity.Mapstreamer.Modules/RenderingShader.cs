using Saab.Unity.Core.ComputeExtension;
using System;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    /// <summary>
    /// Draws everything
    /// </summary>
    public class RenderingShader : IDisposable
    {
        private readonly ComputeShader _shader;
        private readonly Material _material;

        private readonly ComputeBuffer _renderBufferNear = new ComputeBuffer(1000000, sizeof(float) * 4, ComputeBufferType.Append);
        private readonly ComputeBuffer _indirectBufferNear = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

        private readonly ComputeBuffer _renderBufferFar = new ComputeBuffer(4000000, sizeof(float) * 4, ComputeBufferType.Append);
        private readonly ComputeBuffer _indirectBufferFar = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);

        public ComputeBuffer RenderBufferNear
        {
            get { return _renderBufferNear; }
        }

        public ComputeBuffer RenderBufferFar
        {
            get { return _renderBufferFar; }
        }

        public Texture2D Noise
        {
            set { _material.SetTexture(ShaderID.perlinNoise, value); }
        }

        public Texture2D ColorVariance
        {
            set { _material.SetTexture(ShaderID.colorVariance, value); }
        }

        public Vector4[] Frustum
        {
            set { _shader.SetVectorArray(ComputeShaderID.frustumPlanes, value); }
        }

        public float Wind
        {
            set { _material.SetFloat(ShaderID.Wind, value); }
        }

        public Matrix4x4 WorldToLocal
        {
            set { _material.SetMatrix(ShaderID.worldToObj, value); }
        }

        public Vector3 ViewDirection
        {
            set { _material.SetVector(ShaderID.viewDir, value); }
        }

        public UnityEngine.Rendering.ShadowCastingMode ShadowCastingMode
        {
            get;
            set;
        } = UnityEngine.Rendering.ShadowCastingMode.On;


        private struct ShaderID
        {
            // Bufers
            public static readonly int pointBuffer = Shader.PropertyToID("_PointBuffer");

            // Textures   const 
            public static readonly int nodeTexture = Shader.PropertyToID("_NodeTexture");
            public static readonly int mainTexture = Shader.PropertyToID("_MainTex");
            public static readonly int perlinNoise = Shader.PropertyToID("_PerlinNoise");
            public static readonly int colorVariance = Shader.PropertyToID("_ColorVariance");

            // Matrix     const 
            public static readonly int worldToObj = Shader.PropertyToID("_worldToObj");

            // wind       const 
            public static readonly int Wind = Shader.PropertyToID("_TextureWaving");
            public static readonly int Yoffset = Shader.PropertyToID("_Yoffset");

            public static readonly int frustumPlanes = Shader.PropertyToID("_FrustumPlanes");

            public static readonly int minMaxWidthHeight = Shader.PropertyToID("_MinMaxWidthHeight");
            public static readonly int quads = Shader.PropertyToID("_Quads");

            public static readonly int viewDir = Shader.PropertyToID("_ViewDir");
            public static readonly int FadeFar = Shader.PropertyToID("_FadeFar");
            public static readonly int FadeNear = Shader.PropertyToID("_FadeNear");
            public static readonly int FadeNearAmount = Shader.PropertyToID("_FadeNearAmount");
            public static readonly int FadeFarAmount = Shader.PropertyToID("_FadeFarAmount");
        }


        public RenderingShader(ComputeShader shader, Shader materialShader)
        {
            _shader = shader;

            _material = new Material(materialShader);

            _material.SetBuffer(ShaderID.pointBuffer, _renderBufferFar);

            _indirectBufferFar.SetData(new uint[] { 0, 1, 0, 0 });
        }

        public void SetNearFade(float nearFadeStart, float nearFadeEnd)
        {
            // TODO: Rename shader parameters
            // also, we could use Vector4 or 2x Vector2 ?
            _material.SetFloat(ShaderID.FadeNear, nearFadeStart);
            _material.SetFloat(ShaderID.FadeNearAmount, nearFadeEnd);
        }

        public void SetFarFade(float value, float ammount)
        {
            // TODO: Rename shader parameters
            // also, we could use Vector4 or 2x Vector2 ?
            _material.SetFloat(ShaderID.FadeFar, value);
            _material.SetFloat(ShaderID.FadeFarAmount, ammount);
        }

        public void SetBillboardData(Texture2DArray textureArray, Vector4[] sizeDesc, float[] offsets)
        {
            System.Diagnostics.Debug.Assert(textureArray.depth == sizeDesc.Length);
            System.Diagnostics.Debug.Assert(textureArray.depth == offsets.Length);

            _material.SetTexture(ShaderID.mainTexture, textureArray);
            _material.SetVectorArray(ShaderID.minMaxWidthHeight, sizeDesc);
            _material.SetFloatArray(ShaderID.Yoffset, offsets);
        }

        // TODO: Look over this impl
        public void SetQuads(Vector4[] quads)
        {
            _material.SetVectorArray(ShaderID.quads, quads);
        }

        public void RenderBegin()
        {
            _renderBufferNear.SetCounterValue(0);
            _renderBufferFar.SetCounterValue(0);
        }

        public void RenderEnd(Bounds renderBounds)
        {
            ComputeBuffer.CopyCount(_renderBufferFar, _indirectBufferFar, 0);
            ComputeBuffer.CopyCount(_renderBufferNear, _indirectBufferNear, 4 * 1);

            //int[] array = new int[4];
            //_indirectBufferFar.GetData(array);
            //Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, this, "Current buffer size :: {0}", array[0].ToString());
            //Debug.LogWarning("Buffer size :: " + array[0].ToString());


            Graphics.DrawProceduralIndirect(_material, renderBounds, MeshTopology.Points, _indirectBufferFar, 0, null, null, ShadowCastingMode);
        }

        public void Dispose()
        {
            _renderBufferNear.SafeRelease();
            _indirectBufferNear.SafeRelease();

            _renderBufferFar.SafeRelease();
            _indirectBufferFar.SafeRelease();

            GameObject.Destroy(_material);
        }
    }
}
