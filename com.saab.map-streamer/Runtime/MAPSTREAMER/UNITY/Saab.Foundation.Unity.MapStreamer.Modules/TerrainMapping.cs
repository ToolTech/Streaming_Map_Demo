using System;
using System.Collections.Generic;
using System.Linq;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    [Flags]
    public enum MapFeature
    {
        None        = 0,
        // ****** ****** Ground types  ****** ******
        // both a Ground type and a foliage
        Grass       = 1 << 0,
        // for mountainous terrain or other types of rocky terrain
        Rock        = 1 << 1,
        // needed for year around snow covered terrain (mountain peaks)
        Snow        = 1 << 2,      
        Sand        = 1 << 3,
        Gravel      = 1 << 4,
        Dirt        = 1 << 5,
        Barren      = 1 << 6,
        Water       = 1 << 7,
        Concrete    = 1 << 8,
        // ****** Transport ******
        PavedRoad   = 1 << 9,
        DirtRoad    = 1 << 10,
        // ****** Foliage ******
        // unknown foliage (bushes + trees)
        Vegetation  = 1 << 11,
        GrassLawn   = 1 << 12,
        GrassWildFlowers = 1 << 13,
        GrassTall   = 1 << 14,
        CropField   = 1 << 15,
        Bushes      = 1 << 16,
        // unknown tree (Evergreen + Deciduous)
        Trees       = 1 << 17,      
        Evergreen   = 1 << 18,
        Deciduous   = 1 << 19,
    }

    public static class TerrainMapping
    {
        static private MapFeature MapMaxarData(int label)
        {
            switch(label)
            {
                case 21:
                    return MapFeature.Grass;
                case 22:
                    return MapFeature.Barren;
                case 40:
                    return MapFeature.Vegetation;
                case 60:
                case 65:
                    return MapFeature.Water;
                case 6:
                case 80:
                    return MapFeature.Concrete;
                case 81:
                    return MapFeature.PavedRoad;
                case 83:
                case 47:
                    return MapFeature.DirtRoad;
                default:
                    return MapFeature.None;
            }
        }

        public static int BinaryToIndex(int binaryValue)
        {
            if (binaryValue == 0)
                return 0;

            int index = 1;
            while ((binaryValue & 1) != 1)
            {
                binaryValue >>= 1;
                index++;
            }
            return index;
        }

        public static int[] MapMaxarData()
        {
            return MapFeatureData(MapMaxarData);
        }

        public static int[] MapFeatureData(Func<int, MapFeature> Mapping)
        {
            int[] labels = new int[256];

            for (int i = 0; i < 256; i++)
            {
                labels[i] = BinaryToIndex((int)Mapping(i));
            }
            return labels;
        }

        public static int[] FeatureTruthTable(int[] labels, MapFeature features)
        {
            var truthTable = new int[labels.Length];

            for (int i = 0; i < labels.Length; i++)
            {      
                MapFeature feature = (MapFeature)(1 << (labels[i] - 1));
                truthTable[i] = features.HasFlag(feature) ? 1 : 0;
            }

            return truthTable;
        }

        public static MapFeature[] ExtractFlags(MapFeature flags, bool includeZero = false)
        {
            var values = Enum.GetValues(typeof(MapFeature));
            List<MapFeature> foundFlags = new List<MapFeature>();
            foreach (var value in values)
            {
                var flag = (MapFeature)value;

                if (flags.HasFlag(flag) && (includeZero || flag != 0))
                    foundFlags.Add(flag);
            }

            return foundFlags.ToArray();
        }

        public static int[] ExtractFlagsAsIndices(MapFeature flags)
        {
            return ExtractFlags(flags).Select(a => BinaryToIndex((int)a)).ToArray();
        }
    }
}