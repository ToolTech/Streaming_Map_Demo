//******************************************************************************
// File			: IntersectorResult.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzIntersector result class
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
        public struct IntersectorData
        {
            public Vec3 position;
            public Vec3 normal;
        }
        public class IntersectorResult : Reference
        {
            public IntersectorResult(IntPtr nativeReference) : base(nativeReference) { }

            public UInt32 Count
            {
                get
                {
                    return IntersectorResult_getSize(GetNativeReference());
                }
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

            public IntersectorData GetData(UInt32 index)
            {
                IntersectorData data = new IntersectorData();

                if (!IntersectorResult_getData(GetNativeReference(), index, ref data))
                    throw new Exception("Index out of range");

                return data;
            }


            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 IntersectorResult_getSize(IntPtr isectres_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool IntersectorResult_getData(IntPtr isectres_reference,UInt32 index,ref IntersectorData data);
            #endregion
        }
    }
}
