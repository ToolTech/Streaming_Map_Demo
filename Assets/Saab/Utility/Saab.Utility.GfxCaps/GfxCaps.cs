//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s) 
// herein belong to Saab AB. The program(s) may be used and/or
// copied only with the written permission of Saab AB, or in
// accordance with the terms and conditions stipulated in the
// agreement/contract under which the program(s) have been
// supplied. 
//
//
// Information Class:	COMPANY UNCLASSIFIED
// Defence Secrecy:		NOT CLASSIFIED
// Export Control:		NOT EXPORT CONTROLLED
//
//
// File			: GfxCaps.cs
// Module		:
// Description	: Manage selected capabilities of Graphics Performance
// Author		: Anders Modén	
// Product		: GizmoBase 2.12.40
//
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows, Android, iOS and HoloLens for  
//			usage in Game or VisSim development.
//
//
// Revision History...
//
// Who	Date	Description
//
// AMO	191205	Created file                                        (2.10.1)
//
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************

using System;
using GizmoSDK.GizmoBase;

namespace Saab.Utility.GfxCaps
{
    [Flags]
    public enum Capability
    {
        None = 0,
        UseGeneralShaders           = 1 << 0,
        UseTreeCrossboards          = 1 << 1,
        UseDynamicTreeCrossboards   = 1 << 2,
        UseDynamicGrassCrossboards  = 1 << 3,

        DefaultCaps                 = 0x0,
    }

    public struct RenderSettings
    {
        public int DrawDistance;
        public float Density;

        public RenderSettings(int drawDistance, float density)
        {
            DrawDistance = drawDistance;
            Density = density;
        }
    }

    public class GfxCaps
    {
        public static Capability CurrentCaps = KeyDatabase.GetDefaultUserKey("GfxCaps/CurrentCaps", Capability.DefaultCaps);

        public static RenderSettings GetGrassSettings
        {
            get
            {
                var renderSettings = new RenderSettings(300, 0.1f);
                renderSettings.DrawDistance = KeyDatabase.GetDefaultUserKey("GfxCaps/Grass/DrawDistance", renderSettings.DrawDistance);
                renderSettings.Density = KeyDatabase.GetDefaultUserKey("GfxCaps/Grass/Density", renderSettings.Density);
                return renderSettings;
            }
        }

        public static RenderSettings GetTreeSettings
        {
            get
            {
                var renderSettings = new RenderSettings(5000, 20);
                renderSettings.DrawDistance = KeyDatabase.GetDefaultUserKey("GfxCaps/Tree/DrawDistance", renderSettings.DrawDistance);
                renderSettings.Density = KeyDatabase.GetDefaultUserKey("GfxCaps/Tree/Density", renderSettings.Density);
                return renderSettings;
            }
        }

        public static bool HasCapability(Capability caps)
        {
            return (caps & CurrentCaps) != 0;
        }
    }
}
