using Saab.Unity.Core.ComputeExtension;
using System;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    /// <summary>
    /// Generates instance data for a batch of items from a mesh
    /// </summary>
    public class InstanceGenerator : IDisposable
    {
        private readonly ComputeShader _shader;

        private readonly ComputeKernel _generatorKernel;

        private readonly ComputeBuffer _vertices;
        private readonly ComputeBuffer _indices;
        private readonly ComputeBuffer _texcoords;

        /// <summary>
        /// 
        /// </summary>
        public Texture2D SplatMap
        {
            set
            {
                _generatorKernel.SetTexture(ComputeShaderID.splatMap, value);
                _shader.SetInt(ComputeShaderID.terrainResolution, value.height);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Texture2D PlacementMap
        {
            set { _generatorKernel.SetTexture(ComputeShaderID.placementMap, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Texture2D ColorMap
        {
            set { _generatorKernel.SetTexture(ComputeShaderID.nodeTexture, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public float Density
        {
            set { _shader.SetFloat(ComputeShaderID.surfaceGridStep, value); }
        }

        public ComputeBuffer OutputBuffer
        {
            set { _generatorKernel.SetBuffer(ComputeShaderID.terrainBuffer, value); }
        }

        private struct ShaderFunctions
        {
            public const string Generator = "MeshTreeGenerator";
        }

        public InstanceGenerator(ComputeShader shader, int maxVertices = 20000)
        {
            _shader = shader;
            _generatorKernel = new ComputeKernel(ShaderFunctions.Generator, shader);

            _vertices = new ComputeBuffer(maxVertices, sizeof(float) * 3, ComputeBufferType.Default);
            _indices = new ComputeBuffer(maxVertices * 3, sizeof(int), ComputeBufferType.Default);
            _texcoords = new ComputeBuffer(maxVertices, sizeof(float) * 2, ComputeBufferType.Default);

            _generatorKernel.SetBuffer(ComputeShaderID.surfaceVertices, _vertices);
            _generatorKernel.SetBuffer(ComputeShaderID.surfaceIndices, _indices);
            _generatorKernel.SetBuffer(ComputeShaderID.surfaceUVs, _texcoords);
        }

        public void SetMesh(Mesh mesh)
        {
            var surfaceVertices = mesh.vertices;
            var surfaceIndices = mesh.GetIndices(0);
            var surfaceUVs = mesh.uv;

            // maybe remove this*?
            _vertices.SetCounterValue(0);
            _indices.SetCounterValue(0);
            _texcoords.SetCounterValue(0);

            // fill surface vertices
            _vertices.SetData(surfaceVertices);
            _indices.SetData(surfaceIndices);
            _texcoords.SetData(surfaceUVs);

            _shader.SetInt(ComputeShaderID.indexCount, surfaceIndices.Length);
        }

        public void Dispatch(int threadGroups)
        {
            _generatorKernel.Dispatch(threadGroups, 1, 1);
        }

        public void Dispose()
        {
            _vertices.SafeRelease();
            _indices.SafeRelease();
            _texcoords.SafeRelease();

            GameObject.Destroy(_shader);
        }
    }
}
