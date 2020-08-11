using UnityEngine;
using System;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    [Serializable]
    public struct Settings
    {
        public TerrainTextures[] GrassTextures;
        public TerrainTextures[] TreeTextures;

        public Texture2D PerlinNoise;
        public Texture2D DefaultSplatMap;
        public Texture2D PlacementMap;

        public float wind;

        public float GrassDensity;      // 0.0413
        public float TreeDensity;       // 22.127

        public bool GrassShadows;
        public bool TreeShadows;

        public int GrassDrawDistance;
        public int TreeDrawDistance;

        public Mesh TreeTestMesh;
        public Material TreeTestMaterial;

        public ComputeShader ComputeTerrainShader;
        public Shader GrassShader;
        public Shader TreeShader;
    }

    public class TerrainModule : MonoBehaviour
    {
        public SceneManager SceneManager;
        private GrassModule _grassModule;
        private TreeModule _treeModule;

        private GameObject _modulesParent;

        public bool EnableTrees = false;
        public bool EnableGrass = false;
        public bool UseETC2 = false;

        public Settings TerrainSettings;

        private void Start()
        {
            _modulesParent = new GameObject("Terrain Modules");

            if (SceneManager == null) { return; }

            InitializeModule(SceneManager);
        }
        public void InitializeModule(SceneManager sceneManager)
        {
            SceneManager = sceneManager;

            if (SceneManager)
            {
                InitMapModules();

                if (_treeModule != null)
                {
                    SceneManager.OnPostTraverse += _treeModule.Camera_OnPostTraverse;
                    SceneManager.OnEnterPool += _treeModule.RemoveTree;
                }

                if (_grassModule != null)
                {
                    SceneManager.OnPostTraverse += _grassModule.Camera_OnPostTraverse;
                    SceneManager.OnEnterPool += _grassModule.RemoveGrass;
                }

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
                //_grassModule.UseETC2 = UseETC2;

                _grassModule.GrassTextures = TerrainSettings.GrassTextures;
                _grassModule.PerlinNoise = TerrainSettings.PerlinNoise;
                _grassModule.DefaultSplatMap = TerrainSettings.DefaultSplatMap;
                _grassModule.ComputeShader = TerrainSettings.ComputeTerrainShader;
                _grassModule.Shader = TerrainSettings.GrassShader;

                _grassModule.DrawDistance = TerrainSettings.GrassDrawDistance;
                _grassModule.DrawShadows = TerrainSettings.GrassShadows;

                _grassModule.Wind = TerrainSettings.wind;

                _grassModule.Density = TerrainSettings.GrassDensity;
                //_grassModule.PlacementMap = TerrainSettings.PlacementMap;

                //_grassModule.UpdateSceneCamera(SceneManager.SceneManagerCamera as SceneManagerCamera);
            }

            if (EnableTrees)
            {
                // ******* setup GameObject *******
                var go = new GameObject("TreeModule");
                go.transform.parent = _modulesParent.transform;
                _treeModule = go.AddComponent<TreeModule>();

                // ********************************
                //_treeModule.UseETC2 = UseETC2;

                _treeModule.TreeTextures = TerrainSettings.TreeTextures;
                _treeModule.PerlinNoise = TerrainSettings.PerlinNoise;
                _treeModule.DefaultSplatMap = TerrainSettings.DefaultSplatMap;
                _treeModule.ComputeShader = TerrainSettings.ComputeTerrainShader;
                _treeModule.Shader = TerrainSettings.TreeShader;

                _treeModule.TestMesh = TerrainSettings.TreeTestMesh;
                _treeModule.TestMat = TerrainSettings.TreeTestMaterial;

                _treeModule.DrawDistance = TerrainSettings.TreeDrawDistance;
                _treeModule.DrawShadows = TerrainSettings.TreeShadows;

                _treeModule.Wind = TerrainSettings.wind / 10;

                _treeModule.Density = TerrainSettings.TreeDensity;
                //_treeModule.PlacementMap = TerrainSettings.PlacementMap;

                //_treeModule.UpdateSceneCamera(SceneManager.SceneManagerCamera as SceneManagerCamera);
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
