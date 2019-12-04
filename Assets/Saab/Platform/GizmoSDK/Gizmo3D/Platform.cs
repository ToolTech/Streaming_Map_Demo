//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s)
// herein belong to SAAB AB. The program(s) may be used and/or
// copied only with the written permission of SAAB AB, or in
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
// File			: Platforms.cs
// Module		: GizmoBase C#
// Description	: C# Bridge Platform
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.5
//		
//
//			
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows (Win32) and Android for  
//			usage in Game or VisSim development.
//
//
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	180301	Created file 	
//
//******************************************************************************

using System;
using System.Runtime.InteropServices;

namespace GizmoSDK
{
    namespace Gizmo3D
    {
        public class Platform
        {
            static public void InitializeFactories()
            {
                Node.InitializeFactory();
                Group.InitializeFactory();
                Transform.InitializeFactory();
                Lod.InitializeFactory();
                State.InitializeFactory();
                Geometry.InitializeFactory();
                Scene.InitializeFactory();
                PerspCamera.InitializeFactory();
                DynamicLoader.InitializeFactory();
                CullTraverseAction.InitializeFactory();
                NodeAction.InitializeFactory();
                Context.InitializeFactory();
                Texture.InitializeFactory();
                Roi.InitializeFactory();
                RoiNode.InitializeFactory();
                ExtRef.InitializeFactory();
                Crossboard.InitializeFactory();
            }

            static public void UninitializeFactories()
            {
                Node.UninitializeFactory();
                Group.UninitializeFactory();
                Transform.UninitializeFactory();
                Lod.UninitializeFactory();
                State.UninitializeFactory();
                Geometry.UninitializeFactory();
                Scene.UninitializeFactory();
                PerspCamera.UninitializeFactory();
                DynamicLoader.UninitializeFactory();
                CullTraverseAction.UninitializeFactory();
                NodeAction.UninitializeFactory();
                Context.UninitializeFactory();
                Texture.UninitializeFactory();
                Roi.UninitializeFactory();
                RoiNode.UninitializeFactory();
                ExtRef.UninitializeFactory();
                Crossboard.UninitializeFactory();
            }

            public static bool Initialize()
            {
                bool result = GizmoBase.Platform.Initialize();

                if(result)
                    result=Platform_initialize();

                if (result)
                {
                    InitializeFactories();

                    DynamicLoader.Initialize();
                    NodeAction.Initialize();
                }

                return result;
            }

            public static bool Uninitialize(bool forceShutdown=false, bool shutdownBase=false)
            {
                NodeLock.WaitLockEdit();

                NodeAction.Uninitialize();
                DynamicLoader.Uninitialize();

                UninitializeFactories();

                NodeLock.UnLock();

                bool result= Platform_uninitialize(forceShutdown,shutdownBase);
                                
                return result;
            }

#if INTERNAL_LIB
            public const string BRIDGE = "__Internal";
#else
            public const string BRIDGE = "gzGraphBridge" + GizmoSDK.Platform.GZ_LIB_EXT;
#endif

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Platform_initialize();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Platform_uninitialize(bool forceShutdown,bool shutdownBase);

            #endregion
        }
    }
}
