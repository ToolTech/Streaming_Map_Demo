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
// File			: Memory.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzMemory class
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.5
//		
//
//			
// NOTE:	GizmoBase is a platform abstraction utility layer for C++. It contains 
//			design patterns and C++ solutions for the advanced programmer.
//
//
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	200507	Created file 	    (2.10.5)
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public class MemoryControl 
        {
            public static void TraceAlloc(bool on)
            {
                MemoryControl_traceAlloc(on);
            }

            public static void DebugMem(bool on)
            {
                MemoryControl_debugMem(on);
            }

            public static void ResetDebugMem(UInt32 state = 0, UInt32 pid = 0, bool resetInternalGizmoMem = false)
            {
                MemoryControl_resetDebugMem(state, pid, resetInternalGizmoMem);
            }

            public static void DumpAllocMem(bool deltaAlloc=true,UInt32 state = 0, UInt32 pid = 0, bool dumpInternalGizmoMem = false)
            {
                MemoryControl_dumpAllocMem(deltaAlloc,state, pid, dumpInternalGizmoMem);
            }

            public static UInt32 UpdateState(UInt32 state = 0, UInt32 pid = 0)
            {
                return MemoryControl_updateState(state, pid);
            }

            public static UInt32 GetState(UInt32 pid = 0)
            {
                return MemoryControl_getState(pid);
            }

            public static UInt64 GetAllocMem(UInt32 state = 0, UInt32 pid = 0,bool user_memory=true,bool internal_memory=false)
            {
                return MemoryControl_getAllocMem(state, pid,user_memory,internal_memory);
            }

            public static void CleanAllocMem()
            {
                MemoryControl_cleanAllocMem();
            }

            public static void UseFormatOutput(bool on)
            {
                MemoryControl_useFormatOutput(on);
            }


            #region // --------------------- Native calls -----------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void MemoryControl_traceAlloc(bool on);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void MemoryControl_debugMem(bool on);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void MemoryControl_resetDebugMem(UInt32 state = 0, UInt32 pid = 0, bool resetInternalGizmoMem = false);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void MemoryControl_dumpAllocMem(bool deltaAlloc,UInt32 state = 0, UInt32 pid = 0, bool dumpInternalGizmoMem = false);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 MemoryControl_updateState(UInt32 state = 0, UInt32 pid = 0);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 MemoryControl_getState(UInt32 pid = 0);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt64 MemoryControl_getAllocMem(UInt32 state = 0, UInt32 pid = 0,bool user_memory=true,bool internal_memory=false);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void MemoryControl_cleanAllocMem();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void MemoryControl_useFormatOutput(bool on);

            #endregion
        }
    }
}

