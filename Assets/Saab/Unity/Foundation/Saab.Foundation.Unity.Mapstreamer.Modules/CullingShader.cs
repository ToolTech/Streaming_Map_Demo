/* 
 * Copyright (C) SAAB AB
 *
 * All rights, including the copyright, to the computer program(s) 
 * herein belong to Saab AB. The program(s) may be used and/or
 * copied only with the written permission of Saab AB, or in
 * accordance with the terms and conditions stipulated in the
 * agreement/contract under which the program(s) have been
 * supplied. 
 * 
 * Information Class:          COMPANY RESTRICTED
 * Defence Secrecy:            UNCLASSIFIED
 * Export Control:             NOT EXPORT CONTROLLED
 */

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
                if (_inputBuffer == null)
                    return 0;
                return _inputBuffer.count;
            }
        }

        public ComputeBuffer InputBuffer
        {
            set
            {
                System.Diagnostics.Debug.Assert(_inputBuffer == null);
                ComputeBuffer.CopyCount(value, _indirectInputBuffer, 0);

                // ***************************************** Small buffer ***************************************** //

                //int[] size = new int[4];
                //_indirectInputBuffer.GetData(size);
                //_bufferSize = size[0];

                //if (_bufferSize == 0)
                //{
                //    _inputBuffer = null;
                //    return;
                //}

                //// ************ Create new smaller buffer ************

                //var _copyBuffer = new ComputeKernel("Resize", _shader);
                //var _temp = new ComputeBuffer(_bufferSize, sizeof(float) * 4, ComputeBufferType.Append);

                //_temp.SetCounterValue(0);

                //_copyBuffer.SetBuffer(ComputeShaderID.BigBuffer, value);
                //_copyBuffer.SetBuffer(ComputeShaderID.SmallBuffer, _temp);
                //_copyBuffer.Dispatch(Mathf.CeilToInt(_bufferSize / 64f), 1, 1);

                //_cullKernel.SetBuffer(ComputeShaderID.cullInBuffer, _temp);
                ////value.SafeRelease();

                ////_inputBuffer = _temp;
                //_temp.SetCounterValue(0);

                //_shader.SetInt(ComputeShaderID.CopyCount, _bufferSize);

                //_inputBuffer = _temp;
                //_inputBuffer.SetCounterValue(0);
                //_cullKernel.SetBuffer(ComputeShaderID.cullInBuffer, _inputBuffer);
                //_inputBuffer.SetCounterValue(0);

                // ***************************************** ************** ***************************************** //

                _bufferSize = value.count;
                _shader.SetInt(ComputeShaderID.CopyCount, _bufferSize);

                _inputBuffer = value;
                _cullKernel.SetBuffer(ComputeShaderID.cullInBuffer, _inputBuffer);
                _inputBuffer.SetCounterValue(0);
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

        public Vector3 CameraPosition
        {
            set { _shader.SetVector(ComputeShaderID.CameraPosition, value); }
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
            var threadgroup = Mathf.CeilToInt(_bufferSize / 128f);

            if (_bufferSize != 0)
                _cullKernel.Dispatch(threadgroup > 0 ? threadgroup : 1, 1, 1);
        }

        public void Dispose()
        {
            _inputBuffer.SafeRelease();
            _indirectInputBuffer.SafeRelease();

            GameObject.Destroy(_shader);
        }
    }
}
