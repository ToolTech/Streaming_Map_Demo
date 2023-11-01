using System;
using System.Collections.Generic;
using System.Linq;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    [Flags]
    public enum MapFeature
    {
        None = 0,
        // ****** ****** Ground types  ****** ******
        // both a Ground type and a foliage
        Grass = 1 << 0,
        // for mountainous terrain or other types of rocky terrain
        Rock = 1 << 1,
        // needed for year around snow covered terrain (mountain peaks)
        Snow = 1 << 2,
        Sand = 1 << 3,
        Gravel = 1 << 4,
        Dirt = 1 << 5,
        Barren = 1 << 6,
        Water = 1 << 7,
        Concrete = 1 << 8,
        // ****** Transport ******
        PavedRoad = 1 << 9,
        DirtRoad = 1 << 10,
        // ****** Foliage ******
        // unknown foliage (bushes + trees)
        Vegetation = 1 << 11,
        GrassLawn = 1 << 12,
        GrassWildFlowers = 1 << 13,
        GrassTall = 1 << 14,
        CropField = 1 << 15,
        Bushes = 1 << 16,
        // unknown tree (Evergreen + Deciduous)
        Trees = 1 << 17,
        Evergreen = 1 << 18,
        Deciduous = 1 << 19,
        Swamp = 1 << 20,
        Asphalt = 1 << 21,
    }

    public static class TerrainMapping
    {
        private static Dictionary<int, MapFeature> _XLSToFeature = new Dictionary<int, MapFeature>
        {
            {0, MapFeature.None},                   // shadow_void
            {10, MapFeature.Sand},                  // sand_void
            {11, MapFeature.Sand},                  // sand_low_grass_sparse
            {12, MapFeature.Sand},                  // sand_low_grass_dense
            {13, MapFeature.Sand},                  // sand_high_grass_sparse
            {14, MapFeature.Sand},                  // sand_high_grass_dense
            {15, MapFeature.Sand},                  // sand_rocks_sparse
            {16, MapFeature.Sand},                  // sand_rocks_dense
            {20, MapFeature.Barren},                // barren_void
            {21, MapFeature.Barren},                // barren_low_grass_sparse
            {22, MapFeature.Barren},                // barren_low_grass_dense
            {23, MapFeature.Barren},                // barren_high_grass_sparse
            {24, MapFeature.Barren},                // barren_high_grass_dense
            {25, MapFeature.Barren},                // barren_rocks_sparse
            {26, MapFeature.Barren},                // barren_rocks_dense
            {30, MapFeature.Rock},                  // rock_void
            {31, MapFeature.Rock},                  // stone_void
            {32, MapFeature.Rock},                  // browncoal_void
            {40, MapFeature.GrassLawn},             // grass_lawn
            {41, MapFeature.GrassWildFlowers},      // grass_grazing
            {42, MapFeature.Grass},                 // grass_low
            {43, MapFeature.GrassTall},             // grass_high
            {50, MapFeature.Grass},                 // heath_sparse
            {51, MapFeature.Grass},                 // heath_dense
            {60, MapFeature.Bushes},                // bush_low_sparse
            {61, MapFeature.Bushes},                // bush_low_dense
            {62, MapFeature.Bushes},                // bush_medium_sparse
            {63, MapFeature.Bushes},                // bush_medium_dense
            {64, MapFeature.Bushes},                // bush_high_sparse
            {65, MapFeature.Bushes},                // bush_high_dense
            {70, MapFeature.Evergreen},             // forest_coniferous_moss_sparse
            {71, MapFeature.Evergreen},             // forest_coniferous_moss_dense
            {72, MapFeature.Evergreen},             // forest_coniferous_fern_sparse
            {73, MapFeature.Evergreen},             // forest_coniferous_fern_dense
            {74, MapFeature.Evergreen},             // forest_coniferous_brush_sparse
            {75, MapFeature.Evergreen},             // forest_coniferous_brush_dense
            {80, MapFeature.Deciduous},             // forest_deciduous_moss_sparse
            {81, MapFeature.Deciduous},             // forest_deciduous_moss_dense
            {82, MapFeature.Deciduous},             // forest_deciduous_fern_sparse
            {83, MapFeature.Deciduous},             // forest_deciduous_fern_dense
            {84, MapFeature.Deciduous},             // forest_deciduous_brush_sparse
            {85, MapFeature.Deciduous},             // forest_deciduous_brush_dense
            {90, MapFeature.Trees},                 // forest_mixed_moss_sparse
            {91, MapFeature.Trees},                 // forest_mixed_moss_dense
            {92, MapFeature.Trees},                 // forest_mixed_fern_spars
            {93, MapFeature.Trees},                 // forest_mixed_fern_dense
            {94, MapFeature.Trees},                 // forest_mixed_brush_sparse
            {95, MapFeature.Trees},                 // forest_mixed_brush_dense
            {100, MapFeature.CropField},            // crop_ploughed
            {101, MapFeature.CropField},            // crop_corn    
            {102, MapFeature.CropField},            // crop_coleseed
            {103, MapFeature.CropField},            // crop_maize
            {104, MapFeature.CropField},            // crop_cowl
            {105, MapFeature.CropField},            // crop_potato
            {106, MapFeature.CropField},            // crop_hop
            {107, MapFeature.CropField},            // crop_fruit_plant
            {108, MapFeature.CropField},            // crop_fruit_tree
            {109, MapFeature.CropField},            // crop_wine
            {110, MapFeature.CropField},            // crop_unknown
            {120, MapFeature.Dirt},                 // hydrosoil_sand
            {121, MapFeature.Dirt},                 // hydrosoil_rock
            {122, MapFeature.Dirt},                 // hydrosoil_dirt
            {123, MapFeature.Dirt},                 // hydrosoil_sparse_veg
            {124, MapFeature.Dirt},                 // hydrosoil_dense_veg
            {130, MapFeature.Concrete},             // concrete_void
            {131, MapFeature.Concrete},             // concrete_dirt
            {140, MapFeature.Asphalt},              // asphalt_void
            {141, MapFeature.Asphalt},              // asphalt_dirt
            {150, MapFeature.PavedRoad},            // paved_roadway
            {151, MapFeature.PavedRoad},            // paved_pedestrian
            {160, MapFeature.Gravel},               // gravel_void
            {161, MapFeature.Gravel},               // gravel_dirt
            {162, MapFeature.Gravel},               // gravel_rocks
            {170, MapFeature.Concrete},             // built-up_void
            {171, MapFeature.Sand},                 // built-up_sand
            {172, MapFeature.Barren},               // built-up_barren
            {173, MapFeature.Concrete},             // built-up_concrete
            {180, MapFeature.Swamp},                // swamp_void
            {181, MapFeature.Swamp},                // swamp_low_grass_sparse
            {182, MapFeature.Swamp},                // swamp_low_grass_dense
            {183, MapFeature.Swamp},                // swamp_high_grass_sparse
            {184, MapFeature.Swamp},                // swamp_high_grass_dense
            {241, MapFeature.Barren},               // barren_void
            {242, MapFeature.Barren},               // barren_void
            {243, MapFeature.Barren},               // barren_void
            {244, MapFeature.Barren},               // barren_void
            {245, MapFeature.Barren},               // barren_void
            {246, MapFeature.Barren},               // barren_void
            {254, MapFeature.Barren},               // barren_void
            {255, MapFeature.Barren}                // barren_void
        };

        private static Dictionary<int, MapFeature> _maxarToFeature = new Dictionary<int, MapFeature>
        {
            {0, MapFeature.None },          // Nodata
            {1, MapFeature.None },          // Undefined
            {6, MapFeature.Concrete},       // Building
            {8, MapFeature.Concrete },      // Manmade object
            {21, MapFeature.Grass},         // Ground grass
            {22, MapFeature.Barren},        // Ground barren
            {40, MapFeature.Vegetation},    // Vegetation
            {47, MapFeature.DirtRoad},      // Vegetation over road
            {48, MapFeature.None },         // Vegetation over building
            {49, MapFeature.None },         // Vegetation over bridge
            {60, MapFeature.Water},         // Water unspecified
            {65, MapFeature.Water},         // Water swimmingpool
            {80, MapFeature.Concrete},      // Man-made surface
            {81, MapFeature.PavedRoad},     // Man-made surface paved road
            {83, MapFeature.DirtRoad},      // Man-made surface dirt road
            {86, MapFeature.None},          // Man-made surface bridge
            {89, MapFeature.None},          // Man-made surface railbridge
            {90, MapFeature.Concrete},      // Man-made surface rail
            {82, MapFeature.Asphalt},       // Man-made surface runway
        };

        private static Dictionary<int, MapFeature> _gzFeature = new Dictionary<int, MapFeature>
        {
            {0, MapFeature.None },              // Nodata
            {1, MapFeature.None },              // Undefined
            {6, MapFeature.Concrete},           // Building
            {8, MapFeature.Concrete },          // Manmade object
            {21, MapFeature.Grass},             // Ground grass
            {22, MapFeature.Barren},            // Ground barren
            {23, MapFeature.Sand},              // Ground sand
            {24, MapFeature.Rock},              // Ground hydro soil
            {25, MapFeature.Dirt},              // Ground rock
            {26, MapFeature.Gravel},            // Ground Gravel
            {27, MapFeature.CropField},         // Ground Crops
            {28, MapFeature.Swamp},             // Ground Swamp
            {29, MapFeature.GrassWildFlowers},  // Ground heath

            {40, MapFeature.Vegetation},        // Vegetation
            {47, MapFeature.DirtRoad},          // Vegetation over road
            {48, MapFeature.None },             // Vegetation over building
            {49, MapFeature.None },             // Vegetation over bridge

            {60, MapFeature.Water},             // Water unspecified
            {65, MapFeature.Water},             // Water swimmingpool

            {80, MapFeature.Concrete},          // Man-made surface
            {81, MapFeature.PavedRoad},         // Man-made surface paved road
            {83, MapFeature.DirtRoad},          // Man-made surface dirt road
            {86, MapFeature.None},              // Man-made surface bridge
            {89, MapFeature.None},              // Man-made surface railbridge
            {90, MapFeature.Concrete},          // Man-made surface rail
            {92, MapFeature.Asphalt},           // Man-made surface runway
        };


        static private MapFeature MapXLSData(int label)
        {
            if (_XLSToFeature.TryGetValue(label, out var feature))
            {
                return feature;
            }
            else
            {
                return MapFeature.None;
            }
        }

        static private MapFeature MapMaxarData(int label)
        {
            if (_maxarToFeature.TryGetValue(label, out var feature))
            {
                return feature;
            }
            else
            {
                return MapFeature.None;
            }
        }

        static private MapFeature MapgzData(int label)
        {
            if (_gzFeature.TryGetValue(label, out var feature))
            {
                return feature;
            }
            else
            {
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

        public static int[] MapFeatureData()
        {
            return MapFeatureData(MapgzData);
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