using Saab.Unity.Core.ComputeExtension;
using System;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    /// <summary>
    /// Culls a batch of generated instances and outputs to mega render buffer
    /// </summary>
    public class CullingShader : IDisposable
    {
        private readonly ComputeShader _shader;
        private readonly ComputeKernel _cullKernel;

        private readonly ComputeBuffer _indirectInputBuffer = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
        private ComputeBuffer _inputBuffer;
        private int _bufferSize;

        public enum CullingType
        {
            Fade,
            Remove,
        }

        private struct ShaderFunctions
        {
            public const string Fade = "TreeCull";
            public const string Remove = "Cull";
        }

        public int GetBufferSize
        {
            get
            {
                return _inputBuffer.count;
            }
        }

        public ComputeBuffer InputBuffer
        {
            set
            {
                System.Diagnostics.Debug.Assert(_inputBuffer == null);

                _cullKernel.SetBuffer(ComputeShaderID.cullInBuffer, value);
                ComputeBuffer.CopyCount(value, _indirectInputBuffer, 0);

                //var sw = System.Diagnostics.Stopwatch.StartNew();

                int[] size = new int[4];
                _indirectInputBuffer.GetData(size);
                _bufferSize = size[0];

                //sw.Stop();
                //Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, "{0:0.0000} ms", sw.Elapsed.TotalMilliseconds);

                _inputBuffer = value;
            }
        }

        public ComputeBuffer RenderBufferNear
        {
            set { _cullKernel.SetBuffer(ComputeShaderID.closeBuffer, value); }
        }

        public ComputeBuffer RenderBufferFar
        {
            set { _cullKernel.SetBuffer(ComputeShaderID.cullOutBuffer, value); }
        }

        public Matrix4x4 LocalToWorld
        {
            set { _shader.SetMatrix(ComputeShaderID.objToWorld, value); }
        }

        public Vector4[] Frustum
        {
            set { _shader.SetVectorArray(ComputeShaderID.frustumPlanes, value); }
        }

        public CullingShader(ComputeShader shader, CullingType type = CullingType.Remove)
        {
            _shader = shader;
            switch (type)
            {
                case CullingType.Remove:
                    _cullKernel = new ComputeKernel(ShaderFunctions.Fade, _shader);
                    break;
                case CullingType.Fade:
                    _cullKernel = new ComputeKernel(ShaderFunctions.Remove, _shader);
                    break;
            }


            _cullKernel.SetBuffer(ComputeShaderID.indirectBuffer, _indirectInputBuffer);
        }

        public void Dispatch()
        {
            //Debug.LogError("buffer size calc failed! :: " + _bufferSize);
            if (_bufferSize != 0)
                _cullKernel.Dispatch(Mathf.CeilToInt(_bufferSize / 128f), 1, 1);
        }

        public void Dispose()
        {
            _inputBuffer.SafeRelease();
            _indirectInputBuffer.SafeRelease();

            GameObject.Destroy(_shader);
        }
    }
}
