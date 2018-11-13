//******************************************************************************
// File			: State.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzState class
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
        public enum StateMode
        {
            LINE_STIPPLE = (1 << 0), //!<	Enable line stipple parameters
            POLYGON_MODE = (1 << 1),    //!<	Enable polygon mode parameters.
            BLENDING = (1 << 2), //!<	Enable blending parameters
            TEXTURE = (1 << 3), //!<	Enable textures
            MATERIAL = (1 << 4), //!<	Enable materials
            TEXENV = (1 << 5), //!<	Enable texture environment
            TEXGEN = (1 << 6), //!<	Enable texture coord generation
            POLYGON_OFFSET = (1 << 7), //!<	Enable polygon offset
            ACTION_STAGE = (1 << 8), //!<	Set active rendering stage
            GENERATE_DEBUG_INFO = (1 << 9), //!<	Generate debug info
            ALPHA_FUNC = (1 << 10),//!<	Enable alpha func
            GFX_PROGRAM = (1 << 11),//!<	Enable GFX Programs
        }

        public enum StateModeActivation
        {
            ON,
            OFF,
            GLOBAL
        }
        
        public class State : Reference
        {
            public State(IntPtr nativeReference) : base(nativeReference) { }

            public State() : base(State_create()) { }

            static public void InitializeFactory()
            {
                AddFactory(new State());
            }

            public static void UnInitializeFactory()
            {
                RemoveFactory("gzState");
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

            public bool HasTexture(UInt32 unit=0)
            {
                return State_hasTexture(GetNativeReference(),unit);
            }

            public Texture GetTexture(UInt32 unit=0)
            {
                return new Texture(State_getTexture(GetNativeReference(), unit));
            }

            public void SetTexture(Texture texture,UInt32 unit = 0)
            {
                State_setTexture(GetNativeReference(), texture.GetNativeReference(),unit);
            }

            public StateModeActivation GetMode(StateMode mode)
            {
                return State_getMode(GetNativeReference(), mode);
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new State(nativeReference) as Reference;
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr State_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool State_hasTexture(IntPtr state_reference, UInt32 unit);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr State_getTexture(IntPtr state_reference, UInt32 unit);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void State_setTexture(IntPtr state_reference, IntPtr texture_reference,UInt32 unit);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern StateModeActivation State_getMode(IntPtr state_reference, StateMode mode);

            #endregion
        }
    }
}
