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
using Saab.Utility.GfxCaps;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    public class MapShadingModule : MonoBehaviour
    {
        public SceneManager SceneManager;

        [Header("Main Settings")]
        public bool EnableDetailedTextures = true;

        [Range(0, 1)]
        public float SmoothnessModifier;

        [Range(0, 1)]
        public float RoughnessFallback;

        [Range(0, 1)]
        public float HueShiftInclusion = 1;

        [Range(0, 1)]
        public float SecondaryNormalIntensity = 0.4f;

        [Range(0, 1)]
        public float TertiaryNormalIntensity = 0.4f;

        [Header("Module Settings")]
        public TerrainDetailTextureAssetSet DetailTextureSet;

        private ComputeBuffer _mappingBuffer;

        private Texture2DArray _textureArray;
        private Texture2DArray _normalMapArray;
        private Texture2DArray _heightMapArray;
        private Texture2DArray _roughnessMapArray;


        List<Material> _nodeMaterials = new List<Material>();

        private void Start()
        {
            if (SceneManager == null)
                return;

            EnableDetailedTextures = GfxCaps.CurrentCaps.HasFlag(Capability.UseTerrainDetailTextures);

            InitializeModule();
        }

        private void OnDestroy()
        {
            if (_mappingBuffer != null)
                _mappingBuffer.Release();
        }

        public void InitializeModule()
        {
            if (SceneManager)
            {
                InitMapModules();
                InitDetailTexturing();
            }
        }

        private void InitMapModules()
        {
            SceneManager.OnNewGeometry += SceneManager_OnNewGeometry;
            SceneManager.OnEnterPool += SceneManager_OnEnterPool;
            SceneManager.OnPostTraverse += SceneManager_OnPostTraverse;
        }

        private void InitDetailTexturing()
        {
            if (SceneManager &&
                DetailTextureSet &&
                EnableDetailedTextures)
            {
                var mapping = TerrainMapping.MapFeatureData();

                if (!_textureArray)
                {
                    if (DetailTextureSet.Textures == null || DetailTextureSet.Textures.Count < 1)
                        return;

                    int width = DetailTextureSet.Textures[0].Asset.Albedo.width;
                    int height = DetailTextureSet.Textures[0].Asset.Albedo.height;

                    List<int> resolved = new List<int>();
                    for (int i = 0; i < DetailTextureSet.Textures.Count; i++)
                    {
                        var textureAsset = DetailTextureSet.Textures[i];
                        var flagIndices = TerrainMapping.ExtractFlagsAsIndices(textureAsset.Mapping).ToList();

                        //Remove all indices which deal with unclassified data since we will not have textures for these.
                        flagIndices.RemoveAll(fi => fi == 0);

                        for (int mapIndex = 0; mapIndex < mapping.Length; mapIndex++)
                        {
                            if (resolved.Contains(mapIndex))
                                continue;

                            if (flagIndices.Contains(mapping[mapIndex]))
                            {
                                mapping[mapIndex] = i + 1;
                                resolved.Add(mapIndex);
                            }
                        }
                    }

                    //Exclude features which have no texture.
                    for (int i = 0; i < mapping.Length; i++)
                    {
                        if (!resolved.Contains(i))
                            mapping[i] = 0;
                    }

                    int depth = DetailTextureSet.Textures.Count;

                    _textureArray = new Texture2DArray(width, height, depth, TextureFormat.DXT1, true);
                    _normalMapArray = new Texture2DArray(width, height, depth, TextureFormat.DXT5, true);
                    _heightMapArray = new Texture2DArray(width, height, depth, TextureFormat.DXT1, true);
                    _roughnessMapArray = new Texture2DArray(width, height, depth, TextureFormat.DXT1, true);

                    for (int i = 0; i < DetailTextureSet.Textures.Count; i++)
                    {
                        var textureAsset = DetailTextureSet.Textures[i];
                        Graphics.CopyTexture(textureAsset.Asset.Albedo, 0, _textureArray, i);
                        Graphics.CopyTexture(textureAsset.Asset.Normal, 0, _normalMapArray, i);
                        Graphics.CopyTexture(textureAsset.Asset.Displacement, 0, _heightMapArray, i);
                        Graphics.CopyTexture(textureAsset.Asset.Roughness, 0, _roughnessMapArray, i);
                    }
                }

                if (_mappingBuffer == null)
                {
                    _mappingBuffer = new ComputeBuffer(mapping.Length, sizeof(int));
                    _mappingBuffer.SetData(mapping);
                }
            }
        }

        private void SceneManager_OnNewGeometry(GameObject go)
        {
            var nodehandle = go.GetComponent<NodeHandle>();
            var meshRenderer = go.GetComponent<MeshRenderer>();

            var mask = nodehandle.node.GetIntersectMask();
            if (mask.HasFlag(GizmoSDK.Gizmo3D.IntersectMaskValue.BUILDING))
                return;

            if (meshRenderer != null &&
                nodehandle != null &&
                nodehandle.feature != null &&
                nodehandle.texture != null)
            {
                var splatDimensions = new Vector2(nodehandle.texture.width, nodehandle.texture.height);

                meshRenderer.material.SetTexture(Shader.PropertyToID("_FeatureMap"), nodehandle.feature);
                meshRenderer.material.SetVector(Shader.PropertyToID("_FeatureMap_ST"), new Vector4(2, 2, 0, 0));
                meshRenderer.material.SetVector(Shader.PropertyToID("_SplatMapDimensions"), splatDimensions);
                meshRenderer.material.SetFloat(Shader.PropertyToID("_Smoothness"), 1);
                meshRenderer.material.SetFloat(Shader.PropertyToID("_SplatVisualization"), 1);
                meshRenderer.material.SetFloat(Shader.PropertyToID("_DetailTextureFadeStart"), 50);
                meshRenderer.material.SetFloat(Shader.PropertyToID("_DetailTextureFadeZoneLength"), 100);

                meshRenderer.material.SetTexture(Shader.PropertyToID("_Textures"), _textureArray);
                meshRenderer.material.SetTexture(Shader.PropertyToID("_NormalMaps"), _normalMapArray);
                meshRenderer.material.SetTexture(Shader.PropertyToID("_HeightMaps"), _heightMapArray);
                meshRenderer.material.SetTexture(Shader.PropertyToID("_RoughnessMaps"), _roughnessMapArray);

                meshRenderer.material.SetBuffer("_MappingBuffer", _mappingBuffer);

                _nodeMaterials.Add(meshRenderer.material);
            }
        }

        private void SceneManager_OnEnterPool(GameObject go)
        {
            var meshRenderer = go.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
                return;

            for (var i = 0; i < _nodeMaterials.Count; ++i)
            {
                if (_nodeMaterials[i] != meshRenderer.material)
                    continue;

                if ((i + 1) < _nodeMaterials.Count)
                    _nodeMaterials[i] = _nodeMaterials[_nodeMaterials.Count - 1];

                _nodeMaterials.RemoveAt(_nodeMaterials.Count - 1);

                return;
            }
        }

        private void SceneManager_OnPostTraverse(bool locked)
        {
            _nodeMaterials.ForEach(m =>
            {
                m.SetFloat("_Smoothness", SmoothnessModifier);
                m.SetFloat("_RoughnessFallback", RoughnessFallback);
                m.SetFloat("_HueShiftInclusion", HueShiftInclusion);
                m.SetFloat("_SecondaryNormalIntensity", SecondaryNormalIntensity);
                m.SetFloat("_TertiaryNormalIntensity", TertiaryNormalIntensity);
                m.SetKeyword(new UnityEngine.Rendering.LocalKeyword(m.shader, "DETAIL_TEXTURES_ON"), EnableDetailedTextures);
            });
        }
    }
}