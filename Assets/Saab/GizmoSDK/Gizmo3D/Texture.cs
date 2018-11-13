//******************************************************************************
// File			: Texture.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzTexture class
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace Gizmo3D
    {
        public class Texture : Reference
        {
            public Texture(IntPtr nativeReference) : base(nativeReference) { }

            public Texture() : base(Texture_create()) { }

            static public void InitializeFactory()
            {
                AddFactory(new Texture());
            }

            public static void UnInitializeFactory()
            {
                RemoveFactory("gzTexture");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Texture(nativeReference) as Reference;
            }

            override public void Release()
            {
                if (IsValid())
                {
                    NodeLock.WaitLockEdit();

                    base.Release();

                    NodeLock.UnLock();
                }
            }

            virtual public void ReleaseInRender()
            {
                if (IsValid())
                {
                    NodeLock.WaitLockRender();

                    base.Release();

                    NodeLock.UnLock();
                }
            }

            public bool HasImage()
            {
                return Texture_hasImage(GetNativeReference());
            }

            public Image GetImage()
            {
                return CreateObject(Texture_getImage(GetNativeReference())) as Image;
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Texture_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Texture_hasImage(IntPtr texture_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Texture_getImage(IntPtr texture_reference);

            #endregion
        }
    }
}
