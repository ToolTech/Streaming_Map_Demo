//******************************************************************************
// File			: Platforms.cs
// Module		: GizmoBase C#
// Description	: C# Bridge Platform
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.1
//		
// Copyright © 2003- Saab Training Systems AB, Sweden
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
            }

            static public void UnInitializeFactories()
            {
                Node.UnInitializeFactory();
                Group.UnInitializeFactory();
                Transform.UnInitializeFactory();
                Lod.UnInitializeFactory();
                State.UnInitializeFactory();
                Geometry.UnInitializeFactory();
                Scene.UnInitializeFactory();
                PerspCamera.UnInitializeFactory();
                DynamicLoader.UnInitializeFactory();
                CullTraverseAction.UnInitializeFactory();
                NodeAction.UnInitializeFactory();
                Context.UnInitializeFactory();
                Texture.UnInitializeFactory();
                Roi.UnInitializeFactory();
                RoiNode.UnInitializeFactory();
                ExtRef.UnInitializeFactory();
            }

            public static bool Initialize()
            {
                bool result = GizmoBase.Platform.Initialize();

                if(result)
                    result=Platform_initialize();

                if (result)
                    InitializeFactories();

                DynamicLoader.Initialize();

                return result;
            }

            public static bool UnInitialize(bool forceShutdown=false, bool shutdownBase=false)
            {
                DynamicLoader.UnInitialize();

                UnInitializeFactories();
                return Platform_uninitialize(forceShutdown,shutdownBase);
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
