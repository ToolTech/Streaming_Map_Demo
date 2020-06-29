//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s) 
// herein belong to Saab AB. The program(s) may be used and/or
// copied only with the written permission of Saab AB, or in
// accordance with the terms and conditions stipulated in the
// agreement/contract under which the program(s) have been
// supplied. 
//
//
// Information Class:	COMPANY UNCLASSIFIED
// Defence Secrecy:		NOT CLASSIFIED
// Export Control:		NOT EXPORT CONTROLLED
//
//
// File			: CrossboardRenderer_ComputeShader.cs
// Module		:
// Description	: Shader Code
// Author		: ALBNI
// Product		: BTA
//
//
// Revision History...
//
// Who	Date	Description
//
//
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************

using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Crossboard
{
    public struct CrossboardDataset
    {
        public Vector3[] POSITION;
        public Vector2[] UV0;
        public Vector2[] UV1;

        // Opaque shader
        public List<Vector2> UV0List;
        public List<Vector3> UV1List;
        public List<Vector3> UV2List;
        public List<Vector3> UV3List;

        // ************* Opaque shader compute *************
        public List<Vector4> UV0ListComp;
        public List<Vector4> UV1ListComp;

        public Color[] COLOR;
    }

    public class CrossboardRenderer_ComputeShader : CrossboardRenderer
    {
        public ComputeShader _computeShader;
        public ComputeBuffer _inputBuffer;
        public ComputeBuffer _outputBuffer;
        public ComputeBuffer _argBuffer;

        private Plane[] _planes = new Plane[6];
        private float[] _normalsFloat = new float[12];  // 4x3
        private bool _rendering = false;
        private CommandBuffer Cb = null;
        private Camera _cam;

        [SerializeField]
        private Material _material;

        private int _cullingKernel = -1;

        [StructLayout(LayoutKind.Sequential)]
        struct instance_data
        {
            public Vector3 Position;
            public float Size;
            public Vector3 Rotation;
            public Vector3 Color;
            public float Offset;
            public Vector3 PlaneOffset;
        }

        private void OnDestroy()
        {
            _inputBuffer.Release();
            _outputBuffer.Release();
            _argBuffer.Release();
        }

        public override void SetCrossboardDataset(CrossboardDataset dataset, Material material)
        {
            var n = dataset.POSITION.Length;

            // instance data is 6 floats (position x,y,z, extents x,y,z)
            _inputBuffer = new ComputeBuffer(n, Marshal.SizeOf(typeof(instance_data)), ComputeBufferType.Default);

            // render data is 3 floats (position x,y,z)
            _outputBuffer = new ComputeBuffer(n, Marshal.SizeOf(typeof(instance_data)), ComputeBufferType.Append);

            // n instances
            var instanceDataArray = new instance_data[n];

            // copy data
            for (var i = 0; i < n; ++i)
            {
                instanceDataArray[i].Position = dataset.POSITION[i];
                instanceDataArray[i].Size = dataset.UV0ListComp[i].x;
                instanceDataArray[i].Rotation = new Vector3(dataset.UV0ListComp[i].y, dataset.UV0ListComp[i].z, dataset.UV0ListComp[i].w);
                instanceDataArray[i].Color = new Vector3(dataset.COLOR[i].r, dataset.COLOR[i].g, dataset.COLOR[i].b);

                instanceDataArray[i].Offset = dataset.UV1ListComp[i].x;
                instanceDataArray[i].PlaneOffset = new Vector3(dataset.UV1ListComp[i].y, dataset.UV1ListComp[i].z, dataset.UV1ListComp[i].w);
            }

            // copy data to GPU buffer
            _inputBuffer.SetData(instanceDataArray);

            // create indirect argument buffer
            _argBuffer = new ComputeBuffer(4, Marshal.SizeOf(typeof(int)), ComputeBufferType.IndirectArguments);

            // clone the material
            // _material = Instantiate(material);// Material;// Instantiate(Material);
            if (_material)
                Destroy(_material);
            
            _material = material ?? DefaultMaterial;

            // get cull kernel
            _cullingKernel = _computeShader.FindKernel("CS_Cull");

            _computeShader.SetBuffer(_cullingKernel, Shader.PropertyToID("Input"), _inputBuffer);
            _computeShader.SetBuffer(_cullingKernel, Shader.PropertyToID("Output"), _outputBuffer);
            _computeShader.SetInt(Shader.PropertyToID("_count"), n);

            int[] args = new int[] { 0, 1, 0, 0 };
            _argBuffer.SetData(args);

            _material.SetBuffer("_buffer", _outputBuffer);
        }

        private void OnDisable()
        {
            _rendering = false;
            if (Cb != null && _cam != null)
            {
                _cam.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, Cb);
            }
        }

        private void OnEnable()
        {
            _rendering = true;
        }

        private void StartRender(Camera cam)
        {
            if (Cb == null)
            {
                Cb = new CommandBuffer();
                Cb.name = "ComputeShader Trees";
            }
            else
            {
                cam.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, Cb);
            }

            if ((cam.cullingMask & 1) == 0) return;

            _material.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
            var r = _material.SetPass(0);
            Debug.Assert(r);

            // set buffer

            Cb.Clear();
            Cb.DrawProceduralIndirect(Matrix4x4.identity, _material, -1, MeshTopology.Points, _argBuffer, 0);
            cam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, Cb);
        }

        private void Update()
        {
            if (_cullingKernel == -1) return;

            var cameras = FindObjectsOfType<Camera>();
            foreach (var camera in cameras)
            {
                if (!camera.enabled) continue;

                //var camera = Camera.main;
                if (camera == null) { return; }

                _cam = camera;

                GeometryUtility.CalculateFrustumPlanes(camera, _planes);
                //CalculateFrustumPlanes(camera.projectionMatrix * camera.worldToCameraMatrix, ref _planes);

                for (int i = 0; i < 4; i++)
                {
                    //Debug.DrawRay(camera.transform.position, _planes[i].normal * 10f, Color.yellow);
                    _normalsFloat[i + 0] = _planes[i].normal.x;
                    _normalsFloat[i + 4] = _planes[i].normal.y;
                    _normalsFloat[i + 8] = _planes[i].normal.z;
                }

                var camPos = camera.transform.position;

                // reset counter
                _outputBuffer.SetCounterValue(0);

                // assign shader buffers

                _computeShader.SetFloats("_CameraPos", camPos.x, camPos.y, camPos.z);
                _computeShader.SetFloats("_CameraFrustumNormals", _normalsFloat);
                _computeShader.SetMatrix("_ToWorld", transform.localToWorldMatrix);

                // execute the compute shader
                _computeShader.Dispatch(_cullingKernel, Mathf.CeilToInt(_inputBuffer.count / 64f), 1, 1);
                //_computeShader.Dispatch(_cullingKernel, _inputBuffer.count, 1, 1);

                // get append buffer counter value
                ComputeBuffer.CopyCount(_outputBuffer, _argBuffer, 0);

                if (_rendering)
                {
                    StartRender(camera);
                }
            }
        }
    }
}
