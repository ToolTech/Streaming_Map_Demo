using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    [CreateAssetMenu(fileName = "TerrainDetailTextureAssetSet", menuName = "Terrain/DetailTextureAssetSet")]
    [Serializable]
    public class TerrainDetailTextureAssetSet : ScriptableObject
    {
        [SerializeField]
        public List<MappedDetailTextureAsset> Textures;
    }

    [Serializable]
    public struct MappedDetailTextureAsset
    {
        [SerializeField]
        public TerrainDetailTextureAsset Asset;
        [SerializeField]
        public MapFeature Mapping;
    }
}