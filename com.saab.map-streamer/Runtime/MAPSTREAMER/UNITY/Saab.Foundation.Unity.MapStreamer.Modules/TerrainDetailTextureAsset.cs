using System;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    [CreateAssetMenu(fileName = "TerrainDetailTextureAsset", menuName = "Terrain/DetailTextureAsset")]
    [Serializable]
    public class TerrainDetailTextureAsset : ScriptableObject
    {
        public MapFeature MatchingFeatures;
        public Texture2D Albedo;
        public Texture2D Normal;
        public Texture2D Displacement;
        public Texture2D Roughness;
    }
}