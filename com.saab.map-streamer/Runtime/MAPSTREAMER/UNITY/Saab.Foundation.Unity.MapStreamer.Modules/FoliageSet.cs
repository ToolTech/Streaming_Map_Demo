using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    [CreateAssetMenu(fileName = "FoliageSet", menuName = "Terrain/FoliageSet")]
    public class FoliageSet : ScriptableObject
    {
        [SerializeField]
        public List<MappedFoliageAsset> Assets;

        private List<Foliage> _foliages = new List<Foliage>();
        public List<Foliage> GetFoliageList
        { 
            get 
            {
                _foliages.Clear();
                foreach (var item in Assets)
                {
                    _foliages.Add(item.Foliage);
                }
                return _foliages; 
            } 
        }

        public float GetMaxHeight
        {
            get
            {
                float max = 0;
                foreach (var item in Assets)
                {
                    max = MathF.Max(item.Foliage.MaxMin.y, max);
                }
                return max;
            }
        }

    }

    [Serializable]
    public struct MappedFoliageAsset
    {
        [SerializeField]
        public Foliage Foliage;
        [SerializeField]
        public float Weight;
    }
}