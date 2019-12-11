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
// Product		: Gizmo3D 2.10.1
//
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows (Win32) and IRIX® for  
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
using System;
using GizmoSDK.GizmoBase;

namespace Saab.Utility.GfxCaps
{
    [Flags]
    public enum Capability
    {
        UseGeneralShaders   = 1 << 0,
        UseCrossboards      = 1 << 1,

        DefaultCaps         = 0xffff,
    }

    public class GfxCaps
    {
        public static Capability CurrentCaps = KeyDatabase.GetDefaultUserKey("GfxCaps/CurrentCaps", Capability.DefaultCaps);
       
        public static bool HasCapability(Capability caps)
        {
            return (caps & CurrentCaps) != 0;
        }
    }
}
