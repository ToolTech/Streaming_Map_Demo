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

        private readonly Dictionary<GameObject, Material> _materials = new Dictionary<GameObject, Material>();

        private void Awake()
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
            if (SceneManager && EnableDetailedTextures)
            {
                InitMapModules();
                InitDetailTexturing();
            }
        }

        private void InitMapModules()
        {
            SceneManager.OnNewTerrain += SceneManager_OnNewTerrain;
            SceneManager.OnRemoveTerrain += SceneManager_OnRemoveTerrain;
        }

        private void InitDetailTexturing()
        {
            if (!EnableDetailedTextures || !DetailTextureSet)
                return;
            
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

        private void SceneManager_OnNewTerrain(GameObject go, bool isAsset)
        {
            if (!go.TryGetComponent<NodeHandle>(out var nodehandle))
                return;

            if (!nodehandle.feature || !nodehandle.texture)
                return;

            var mask = nodehandle.node.IntersectMask;
            if (!mask.HasFlag(GizmoSDK.Gizmo3D.IntersectMaskValue.GROUND))
                return;

            if (!go.TryGetComponent<MeshRenderer>(out var meshRenderer))
                return;

            var material = meshRenderer.sharedMaterial;

            // todo: validate that this material uses correct shader
            if (!material)
                return;
                
            var splatDimensions = new Vector2(nodehandle.texture.width, nodehandle.texture.height);

            material.SetTexture(Shader.PropertyToID("_FeatureMap"), nodehandle.feature);
            material.SetVector(Shader.PropertyToID("_FeatureMap_ST"), new Vector4(2, 2, 0, 0));
            material.SetVector(Shader.PropertyToID("_SplatMapDimensions"), splatDimensions);
            material.SetFloat(Shader.PropertyToID("_DetailTextureFadeStart"), 50);
            material.SetFloat(Shader.PropertyToID("_DetailTextureFadeZoneLength"), 100);


            material.SetTexture(Shader.PropertyToID("_Textures"), _textureArray);
            material.SetTexture(Shader.PropertyToID("_NormalMaps"), _normalMapArray);
            material.SetTexture(Shader.PropertyToID("_HeightMaps"), _heightMapArray);
            material.SetTexture(Shader.PropertyToID("_RoughnessMaps"), _roughnessMapArray);
            
            material.SetBuffer("_MappingBuffer", _mappingBuffer);

            _materials.Add(go, material);
            
            RefreshSettings(material);
        }

        private void SceneManager_OnRemoveTerrain(GameObject go)
        {
            _materials.Remove(go);
        }

        private void OnValidate()
        {
            RefreshSettings();
        }

        public void RefreshSettings()
        {
            foreach (var kvp in _materials)
                RefreshSettings(kvp.Value);
        }

        private void RefreshSettings(Material material)
        {
            material.SetFloat("_Smoothness", SmoothnessModifier);
            material.SetFloat("_RoughnessFallback", RoughnessFallback);
            material.SetFloat("_HueShiftInclusion", HueShiftInclusion);
            material.SetFloat("_SecondaryNormalIntensity", SecondaryNormalIntensity);
            material.SetFloat("_TertiaryNormalIntensity", TertiaryNormalIntensity);
            material.SetKeyword(new UnityEngine.Rendering.LocalKeyword(material.shader, "DETAIL_TEXTURES_ON"), EnableDetailedTextures);
        }
    }
}