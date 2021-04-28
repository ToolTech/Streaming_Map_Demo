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

using UnityEngine;
using System.Collections;
using System;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    [Serializable]
    public struct TerrainSetting
    {
        public TerrainTextures[] GrassTextures;
        public TerrainTextures[] TreeTextures;

        public Texture2D PerlinNoise;
        public Texture2D DefaultSplatMap;
        public Texture2D PlacementMap;

        public float wind;
        public bool SortByDistance;

        public float GrassDensity;      // 0.0413
        public float TreeDensity;       // 22.127

        public Mesh TreeTestMesh;
        public Material TreeTestMaterial;

        public int GrassDrawDistance;
        public int TreeDrawDistance;

        public bool GrassShadows;
        public bool TreeShadows;

        public ComputeShader ComputeTerrainShader;
        public Shader GrassShader;
        public Shader TreeShader;
    }

    public class TerrainModule : MonoBehaviour
    {
        public SceneManager SceneManager;

        [Header("Compute Shader Settings")]
        public ComputeShader DepthShader;
        public bool OcclusionCulling;

        [Header("Main Settings")]
        public bool EnableTrees = false;
        public bool EnableGrass = false;

        [Header("Module Settings")]
        public TerrainSetting TerrainSettings;

        [Header("Debug/Test Settings")]
        public bool PointCloud;
        public bool TreeMesh;

        private GrassModule _grassModule;
        private TreeModule _treeModule;
        private GameObject _modulesParent;
        private Camera _camera;
        
        [SerializeField]
        private RenderTexture _outputTex;
        [SerializeField]
        private RenderTexture _whiteTex;

        private Vector4[] _frustum = new Vector4[6];
        private int _kernelHandle;

        private void Awake()
        {
            _whiteTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _whiteTex.enableRandomWrite = true;
            _whiteTex.Create();

            RenderTexture.active = _whiteTex;
            GL.Clear(true, true, Color.white);
            RenderTexture.active = null;

            if (SceneManager == null) { return; }

            InitializeModule(SceneManager);
        }
        public void InitializeModule(SceneManager sceneManager)
        {
            SceneManager = sceneManager;
            _modulesParent = this.gameObject;

            

            if (SceneManager)
            {

                InitMapModules();
                if (_treeModule != null || _grassModule != null)
                    SceneManager.OnPostTraverse += SceneManager_OnPostTraverse;


                if (_treeModule != null)
                {
                    SceneManager.OnEnterPool += _treeModule.RemoveTree;
                }

                if (_grassModule != null)
                {
                    SceneManager.OnEnterPool += _grassModule.RemoveGrass;
                }

            }
        }

        public void GenerateFrustumPlane(Camera camera)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);

            for (int i = 0; i < 6; i++)
            {
                _frustum[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
            }
        }

        public void GetDepthTexture(Camera camera)
        {
            if (_outputTex == null || (_outputTex.width != camera.pixelWidth || _outputTex.height != camera.pixelHeight))
            {
                _outputTex = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                _outputTex.enableRandomWrite = true;
                _outputTex.Create();

                _whiteTex = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                _whiteTex.enableRandomWrite = true;
                _whiteTex.Create();

                RenderTexture.active = _whiteTex;
                GL.Clear(true, true, Color.white);
                RenderTexture.active = null;
            }

            _kernelHandle = DepthShader.FindKernel("Depth");
            DepthShader.SetTextureFromGlobal(_kernelHandle, "_DepthTexture", "_CameraDepthTexture");
            DepthShader.SetTexture(_kernelHandle, "_OutputTexture", _outputTex);
            DepthShader.Dispatch(_kernelHandle, Mathf.CeilToInt(camera.pixelWidth / 32), Mathf.CeilToInt(camera.pixelHeight / 32), 1);
        }

        private void SceneManager_OnPostTraverse()
        {
            _camera = Camera.main;

            if (_camera == null)
                return;

            if (_camera.depthTextureMode != DepthTextureMode.Depth)
                _camera.depthTextureMode = _camera.depthTextureMode | DepthTextureMode.Depth;

            GenerateFrustumPlane(_camera);

            if(OcclusionCulling)
                GetDepthTexture(_camera);

            if (_treeModule != null && EnableTrees)
            {
                if (OcclusionCulling)
                    _treeModule.DepthTexture = _outputTex;
                else
                    _treeModule.DepthTexture = _whiteTex;

                _treeModule.FrustumPlane = _frustum;
                _treeModule.CurrentCamera = _camera;
                _treeModule.Camera_OnPostTraverse();
            }

            if (_grassModule != null && EnableGrass)
            {
                if (OcclusionCulling)
                    _grassModule.DepthTexture = _outputTex;
                else
                    _grassModule.DepthTexture = _whiteTex;

                _grassModule.FrustumPlane = _frustum;
                _grassModule.CurrentCamera = _camera;
                _grassModule.Camera_OnPostTraverse();
            }
        }

        private IEnumerator GetTerrainMemory()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f); // wait two seconds
                int size = 0;
                if (_grassModule != null)
                {
                    size += _grassModule.GetMemoryFootprint;
                }

                if (_treeModule != null)
                    size += _treeModule.GetMemoryFootprint;

                Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, "Memory Footprint {0} MB", (size / 1000000f).ToString("F2"));
            }
        }

        private void InitMapModules()
        {
            if (EnableGrass)
            {
                // ******* setup GameObject *******
                var go = new GameObject("GrassModule");
                go.transform.parent = _modulesParent.transform;
                _grassModule = go.AddComponent<GrassModule>();

                // ********************************
                _grassModule.GrassTextures = TerrainSettings.GrassTextures;
                _grassModule.PerlinNoise = TerrainSettings.PerlinNoise;
                _grassModule.DefaultSplatMap = TerrainSettings.DefaultSplatMap;
                _grassModule.ComputeShader = TerrainSettings.ComputeTerrainShader;
                _grassModule.Shader = TerrainSettings.GrassShader;

                _grassModule.DrawDistance = TerrainSettings.GrassDrawDistance;
                _grassModule.DrawShadows = TerrainSettings.GrassShadows;
                _grassModule.SortByDistance = TerrainSettings.SortByDistance;

                _grassModule.Wind = TerrainSettings.wind;

                _grassModule.Density = TerrainSettings.GrassDensity;
                //_grassModule.PlacementMap = TerrainSettings.PlacementMap;
            }

            if (EnableTrees)
            {
                // ******* setup GameObject *******
                var go = new GameObject("TreeModule");
                go.transform.parent = _modulesParent.transform;
                _treeModule = go.AddComponent<TreeModule>();

                // ********************************
                _treeModule.PointCloud = PointCloud;
                _treeModule.MeshTree = TreeMesh;

                _treeModule.TreeTextures = TerrainSettings.TreeTextures;
                _treeModule.PerlinNoise = TerrainSettings.PerlinNoise;
                _treeModule.DefaultSplatMap = TerrainSettings.DefaultSplatMap;
                _treeModule.ComputeShader = TerrainSettings.ComputeTerrainShader;
                _treeModule.Shader = TerrainSettings.TreeShader;

                _treeModule.TestMesh = TerrainSettings.TreeTestMesh;
                _treeModule.TestMat = TerrainSettings.TreeTestMaterial;

                _treeModule.DrawDistance = TerrainSettings.TreeDrawDistance;
                _treeModule.DrawShadows = TerrainSettings.TreeShadows;
                _treeModule.SortByDistance = TerrainSettings.SortByDistance;

                _treeModule.Wind = TerrainSettings.wind / 100;

                _treeModule.Density = TerrainSettings.TreeDensity;
                //_treeModule.PlacementMap = TerrainSettings.PlacementMap;
            }

            if (EnableGrass || EnableTrees)
            {
                SceneManager.OnNewGeometry += SceneManager_OnNewGeometry;
            }
        }
        private void SceneManager_OnNewGeometry(GameObject o)
        {
            var nodehandler = o.GetComponent<NodeHandle>();
            if (nodehandler != null)
            {
                if (EnableGrass)
                {
                    if (nodehandler.node.BoundaryRadius < 190 && nodehandler.node.BoundaryRadius > 0)
                    {
                        _grassModule.AddGrass(o);
                    }
                }
                if (EnableTrees)
                {
                    if (nodehandler.node.BoundaryRadius < 890 && nodehandler.node.BoundaryRadius > 0)
                    {
                        _treeModule.AddTree(o);
                    }
                }
            }
        }
    }
}
