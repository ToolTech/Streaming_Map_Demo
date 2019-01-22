using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Assets.Crossboard
{
    public class CrossboardRenderer_ComputeShader : CrossboardRenderer
    {
        public ComputeShader _computeShader;
        public ComputeBuffer _inputBuffer;
        public ComputeBuffer _outputBuffer;
        public ComputeBuffer _argBuffer;

        [SerializeField]
        private Material _material;

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

        public override void SetCrossboardDataset(CrossboardDataset dataset)
        {
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Material;

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

                //instanceDataArray[i].Size = dataset.UV0[i].x;
                //instanceDataArray[i].Rotation = new Vector3(dataset.UV0[i].y, dataset.UV1[i].x, dataset.UV1[i].y);
                //instanceDataArray[i].Color = new Vector3(dataset.COLOR[i].r, dataset.COLOR[i].g, dataset.COLOR[i].b);
            }

            // copy data to GPU buffer
            _inputBuffer.SetData(instanceDataArray);

            // create indirect argument buffer
            _argBuffer = new ComputeBuffer(4, Marshal.SizeOf(typeof(int)), ComputeBufferType.IndirectArguments);

            // clone the material
            _material = Material;// Instantiate(Material);

            // get cull kernel
            _cullingKernel = _computeShader.FindKernel("CS_Cull");

            _computeShader.SetBuffer(_cullingKernel, Shader.PropertyToID("Input"), _inputBuffer);
            _computeShader.SetBuffer(_cullingKernel, Shader.PropertyToID("Output"), _outputBuffer);
            _computeShader.SetInt(Shader.PropertyToID("_count"), n);

            int[] args = new int[] { 0, 1, 0, 0 };
            _argBuffer.SetData(args);

            _material.SetBuffer("_buffer", _outputBuffer);
        }

        private int _cullingKernel = -1;

        private void Update()
        {
            if (_cullingKernel == -1) return;

            var camera = Camera.main;
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

            // get 

            // get append buffer counter value
            ComputeBuffer.CopyCount(_outputBuffer, _argBuffer, 0);

            //_argBuffer.GetData(args);

            //Debug.Log("vertex count " + args[0]);
            ////
            //Debug.Log("instance count " + args[1]);
            //
            //Debug.Log("start vertex " + args[2]);
            //
            //Debug.Log("start instance " + args[3]);

            //int n = _outputBuffer.count;
            //var res = new Vector3[n];

            //_outputBuffer.GetData(res);
        }

        private void OnRenderObject()
        {
            // set first pass
            if(_material == null)
            {
                return;
            }
            
            _material.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
            //_material.SetMatrix("_World2Object", transform.wo);

            var r = _material.SetPass(0);
            Debug.Assert(r);


            //_material.SetMatrix("unity_MatrixVP", Camera.current.projectionMatrix * Camera.current.worldToCameraMatrix);
            //_material.SetMatrix("unity_ObjectToWorld", Matrix4x4.identity);

            //var m = _material.GetMatrix("unity_ObjectToWorld");
            //Debug.Log(m);

            // set buffer

            



            // ... this crash ...

            Graphics.DrawProceduralIndirect(MeshTopology.Points, _argBuffer, 0);
        }


        private Plane[] _planes = new Plane[6];
        private float[] _normalsFloat = new float[12];  // 4x3
       
    }

    
}
