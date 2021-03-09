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
// File			: IntersectorResult.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzIntersector result class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.7
//		
//
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
            public Vec3 coordinate;

            public Vec3 a, b, c;

            public Vec3 normal;

            public Vec2 uv;

            public Matrix4 transform;

            public Node node;

            public IntersectQuery resultMask;
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
                    try
                    {
                        NodeLock.WaitLockEdit();

                        base.Release();
                    }
                    finally
                    {

                        NodeLock.UnLock();
                    }
                }
            }

            virtual public void ReleaseInRender()
            {
                if (IsValid())
                {
                    try
                    {
                        NodeLock.WaitLockRender();

                        base.Release();
                    }
                    finally
                    {
                        NodeLock.UnLock();
                    }
                }
            }

            public static bool HasResult(IntersectorData data,IntersectQuery flags)
            {
                return data.resultMask.HasFlag(flags);
            }

            public IntersectorData GetData(UInt32 index)
            {
                IntersectorData data = new IntersectorData();

                if (!IntersectorResult_getData(GetNativeReference(), index, ref data.resultMask, ref data.coordinate,ref data.a, ref data.b, ref data.c, ref data.normal, ref data.uv, ref data.transform))
                    throw new Exception("Index out of range");

                return data;
            }


            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 IntersectorResult_getSize(IntPtr isectres_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool IntersectorResult_getData(IntPtr isectres_reference, UInt32 index, ref IntersectQuery result_mask,ref Vec3 coordinate, ref Vec3 a, ref Vec3 b, ref Vec3 c, ref Vec3 normal, ref Vec2 uv, ref Matrix4 transform);
            #endregion
        }
    }
}

