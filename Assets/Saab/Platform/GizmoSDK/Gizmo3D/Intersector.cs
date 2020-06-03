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
// File			: Intersector.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzIntersector class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.6
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
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace Gizmo3D
    {
        [Flags]
        public enum IntersectQuery
        {
            NULL =                  0,
            NEAREST_POINT =         (1 << 0),       // Return only closest HIT
            NORMAL =                (1 << 1),       // Add normal of HIT triangle
            NODE =                  (1 << 2),       // Add node that was hit
            HISTORY =               (1 << 3),       // Add traversal history
            ABC_TRI =               (1 << 4),       // Returns triangle points
            ALL_HITS =              (1 << 5),       // Return all HITS
            ONE_HIT =               (1 << 6),       // Return first HIT
            ACCELLERATE =           (1 << 7),       // Build and use nonatree
            WAIT_FOR_DYNAMIC_DATA = (1 << 8),       // Wait for dynamic loaded data
            UPDATE_DYNAMIC_DATA =   (1 << 9),       // Update dynamic loaded data
            TRANSFORM =             (1 << 10),      // Add transform to result
            UV =                    (1 << 11),      // Add UV Coordinate
        };

        public class Intersector : GizmoBase.Object , INameInterface
        {
            public Intersector(IntPtr nativeReference) : base(nativeReference) { }

            public Intersector(string name="") : base(Intersector_create(name)) { }

            public new static void InitializeFactory()
            {
                AddFactory(new Intersector());
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Intersector(nativeReference) as Reference;
            }

            // We added NodeLock to Release to allow GC to be locked by edit or render
            // The intersector can run in edit or render mode but we must not release any scene graph data during rendering mode
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


            public string GetName()
            {
                return Marshal.PtrToStringUni(Intersector_getName(GetNativeReference()));
            }

            public void SetName(string name)
            {
                Intersector_setName(GetNativeReference(), name);
            }

            public void SetStartPosition(Vec3 startPosition)
            {
                Intersector_setStartPosition(GetNativeReference(), ref startPosition);
            }

            public void SetDirection(Vec3 direction)
            {
                Intersector_setDirection(GetNativeReference(), ref direction);
            }

            public bool Intersect(Node node, IntersectQuery flags = IntersectQuery.NEAREST_POINT, float lodFactor = 1.0f, bool useRoiPosition = false,Vec3D roiEyePos=default(Vec3D))
            {
                return Intersector_intersect(GetNativeReference(), node.GetNativeReference(), flags, lodFactor, useRoiPosition, roiEyePos);
            }

            public IntersectorResult GetResult()
            {
                return new IntersectorResult(Intersector_getResult(GetNativeReference()));
            }

            public void SetCamera(Camera camera)
            {
                Intersector_setCamera(GetNativeReference(), camera.GetNativeReference());
            }

            public Camera GetCamera()
            {
                return Reference.CreateObject(Intersector_getCamera(GetNativeReference())) as Camera;
            }


            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Intersector_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Intersector_setName(IntPtr intersector_reference, string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Intersector_getName(IntPtr intersector_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Intersector_setStartPosition(IntPtr intersector_reference,ref Vec3 position);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Intersector_setDirection(IntPtr intersector_reference, ref Vec3 direction);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Intersector_intersect(IntPtr intersector_reference, IntPtr node_reference, IntersectQuery flags, float lodFactor, bool useRoiPosition,Vec3D roiPos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Intersector_getResult(IntPtr intersector_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Intersector_setCamera(IntPtr intersector_reference,IntPtr camera_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Intersector_getCamera(IntPtr intersector_reference);
            #endregion
        }
    }
}
