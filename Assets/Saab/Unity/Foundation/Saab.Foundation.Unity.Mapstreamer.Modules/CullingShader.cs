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


        private struct ShaderFunctions
        {
            public const string Cull = "TreeCull";
        }

        public ComputeBuffer InputBuffer
        {
            set
            {
                System.Diagnostics.Debug.Assert(_inputBuffer == null);

                _cullKernel.SetBuffer(ComputeShaderID.cullInBuffer, value);
                ComputeBuffer.CopyCount(value, _indirectInputBuffer, 0);

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

        public CullingShader(ComputeShader shader)
        {
            _shader = shader;

            _cullKernel = new ComputeKernel(ShaderFunctions.Cull, _shader);

            _cullKernel.SetBuffer(ComputeShaderID.indirectBuffer, _indirectInputBuffer);
        }

        public void Dispatch(int threadGroups)
        {
            _cullKernel.Dispatch(threadGroups, 1, 1);
        }

        public void Dispose()
        {
            _inputBuffer.SafeRelease();
            _indirectInputBuffer.SafeRelease();

            GameObject.Destroy(_shader);
        }
    }
}
