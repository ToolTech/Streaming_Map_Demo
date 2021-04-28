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
using System.Linq;
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
        private int _maxVertices;
        private int _maxIndices;

        private ComputeBuffer _vertices;
        private ComputeBuffer _indices;
        private ComputeBuffer _texcoords;

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
        public int GetBufferSize
        {
            get
            {
                return (_vertices.count * sizeof(float) * 3) + (_indices.count * sizeof(int)) + (_texcoords.count * sizeof(float) * 2);
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

        public bool PlacementMapEnabled
        {
            set { _shader.SetBool(ComputeShaderID.PlacementMapEnabled, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        float _density;
        public float Density
        {
            set
            {
                _density = value;
                _shader.SetFloat(ComputeShaderID.surfaceGridStep, value);
            }
        }

        public ComputeBuffer OutputBuffer
        {
            set
            {
                value.SetCounterValue(0);
                _generatorKernel.SetBuffer(ComputeShaderID.terrainBuffer, value);
            }
        }

        public enum Feature
        {
            Tree,
            Grass,
            PointCloud,
        }

        private struct ShaderFunctions
        {
            public const string GeneratorGrass = "MeshGrassGenerator";
            public const string GeneratorTree = "MeshTreeGenerator";
            public const string PointCloudPlacement = "PointCloudPlacement";
        }

        // TODO: fixed dynamic maxVertices
        public InstanceGenerator(ComputeShader shader, Feature feature, int maxVertecis = 10000)
        {
            _shader = shader;

            switch (feature)
            {
                case Feature.Grass:
                    _generatorKernel = new ComputeKernel(ShaderFunctions.GeneratorGrass, shader);
                    break;
                case Feature.Tree:
                    _generatorKernel = new ComputeKernel(ShaderFunctions.GeneratorTree, shader);
                    break;
                case Feature.PointCloud:
                    _generatorKernel = new ComputeKernel(ShaderFunctions.PointCloudPlacement, shader);
                    break;
            }
            _maxIndices = maxVertecis * 3;
            GenerateBuffers(maxVertecis, _maxIndices);
        }

        private void GenerateBuffers(int maxVertices, int maxIndices)
        {
            _maxVertices = maxVertices;
            _maxIndices = maxIndices;

            if (_vertices != null)
            {
                Debug.LogWarning("--------------------- cleared buffers ---------------------");
                _vertices.SafeRelease();
                _indices.SafeRelease();
                _texcoords.SafeRelease();
            }

            _vertices = new ComputeBuffer(maxVertices, sizeof(float) * 3, ComputeBufferType.Default);
            _indices = new ComputeBuffer(maxIndices, sizeof(int), ComputeBufferType.Default);
            _texcoords = new ComputeBuffer(maxVertices, sizeof(float) * 2, ComputeBufferType.Default);

            _vertices.SetCounterValue(0);
            _indices.SetCounterValue(0);
            _texcoords.SetCounterValue(0);

            _generatorKernel.SetBuffer(ComputeShaderID.surfaceVertices, _vertices);
            _generatorKernel.SetBuffer(ComputeShaderID.surfaceIndices, _indices);
            _generatorKernel.SetBuffer(ComputeShaderID.surfaceUVs, _texcoords);
        }

        public void SetMesh(Mesh mesh, bool pointCloud = false)
        {
            var surfaceVertices = mesh.vertices;
            var surfaceIndices = mesh.GetIndices(0);
            var surfaceUVs = mesh.uv;
            var bounds = mesh.bounds;

            var vert = _maxVertices;
            var indi = _maxIndices;

            if (surfaceIndices.Length > _maxIndices || surfaceVertices.Length > _maxVertices)
            {
                Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, "\nupdated vertex buffer size from: {0} -- {1}\nupdated vertex buffer size from: {2} -- {3}", _maxVertices, surfaceIndices.Length, _maxIndices, surfaceIndices.Length);
                _maxVertices = surfaceVertices.Length;
                _maxIndices = surfaceIndices.Length;
                GenerateBuffers(_maxVertices, _maxIndices);
            }

            // maybe remove this*?
            _vertices.SetCounterValue(0);
            _indices.SetCounterValue(0);
            _texcoords.SetCounterValue(0);

            // fill surface vertices
            _vertices.SetData(surfaceVertices);
            _indices.SetData(surfaceIndices);
            _texcoords.SetData(surfaceUVs);

            if(pointCloud)
            {
                _shader.SetInt(ComputeShaderID.indexCount, surfaceVertices.Length);
            }
            else
            {
                _shader.SetInt(ComputeShaderID.indexCount, surfaceIndices.Length);
            }
            

            var extents = bounds.extents;
            var maxExtent = Mathf.Max(extents.x, extents.y, extents.z);
            _shader.SetFloat(ComputeShaderID.size, maxExtent);
        }

        public void Dispatch(int threadGroups)
        {
            //Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, "dispatch with :: {0} / 65535", threadGroups);
            _generatorKernel.Dispatch(threadGroups, 16, 1);
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
