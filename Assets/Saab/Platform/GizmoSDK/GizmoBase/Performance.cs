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
// File			: Performance.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to performance utilities
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.6
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
// AMO	180301	Created file 	
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {

        public class Performance
        {
            [Flags]
            public enum DumpFlags
            {
                RUNNING = (1 << 0),
                STOPPED = (1 << 1),
                ACCUMULATED_SECTIONS = (1 << 2),
                HIERARCHICAL_SECTIONS = (1 << 3),
                ALL = -1,
            };

            static public void Enter(string section)
            {
                Performance_enterPerformanceSection(section);
            }

            static public void Leave()
            {
                Performance_leavePerformanceSection();
            }

            static public void DumpPerformanceInfo(DumpFlags flags = DumpFlags.RUNNING | DumpFlags.ACCUMULATED_SECTIONS | DumpFlags.HIERARCHICAL_SECTIONS)
            {
                Performance_dumpPerformanceInfo(flags);
            }

            static public void Clear(string section="",UInt32 threadID=0)
            {
                Performance_clearPerformanceSection(section,threadID);
            }

            static public PerformanceResult GetResult(string section, UInt32 threadID = 0)
            {
                return new PerformanceResult(Performance_getPerformanceResult(section, threadID));
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Performance_dumpPerformanceInfo(DumpFlags flags);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Performance_enterPerformanceSection(string section);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Performance_leavePerformanceSection();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Performance_clearPerformanceSection(string section,UInt32 threadID);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Performance_getPerformanceResult(string section, UInt32 threadID);


            #endregion
        }

        public class Monitor
        {
            static public void Enter(string monitor)
            {
                Monitor_enter(monitor);
            }

            static public void Leave(string monitor)
            {
                Monitor_leave(monitor);
            }

            static public void InstallMonitor(string url="udp::45454?nic=127.0.0.1")
            {
                Monitor_install(url);
            }

            static public void AddValue(string monitor,DynamicType value,double time=-1,UInt32 instanceID=0)
            {
                Monitor_addValue(monitor,value.GetNativeReference(),time,instanceID);
            }

            static public void AddValueOpt(bool addValue,string monitor,DynamicType value, double time = -1, UInt32 instanceID = 0)
            {
                if (addValue)
                    AddValue(monitor, value, time, instanceID);
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Monitor_enter(string monitor);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Monitor_leave(string monitor);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Monitor_install(string url);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Monitor_addValue(string monitor,IntPtr dynamic_reference,double time,UInt32 instanceID);

            #endregion
        }

        public class PerformanceResult : Reference
        {
            public PerformanceResult(IntPtr nativeReference) : base(nativeReference) { }

            public UInt32 Iterations    { get {return PerformanceResult_getIterations(GetNativeReference()); }}

            public UInt32 Recursive { get { return PerformanceResult_getRecursive(GetNativeReference()); } }

            public double ExecTotTime { get { return PerformanceResult_getExecTotTime(GetNativeReference()); } }

            public double ExecSelfTime { get { return PerformanceResult_getExecSelfTime(GetNativeReference()); } }

            public double ExeParentTime { get { return PerformanceResult_getExecParentTime(GetNativeReference()); } }

            public float ExecTotPercentage { get { return PerformanceResult_getExecTotPercentage(GetNativeReference()); } }

            public float ExecSelfPercentage { get { return PerformanceResult_getExecSelfPercentage(GetNativeReference()); } }

            public float ExecParentPercentage { get { return PerformanceResult_getExecParentPercentage(GetNativeReference()); } }

            public UInt32 Callers { get { return PerformanceResult_getCallers(GetNativeReference()); } }

            public UInt32 ThreadID { get { return PerformanceResult_getThreadID(GetNativeReference()); } }

            public double SystemTime { get { return PerformanceResult_getSystemTime(GetNativeReference()); } }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 PerformanceResult_getIterations(IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 PerformanceResult_getRecursive(IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double PerformanceResult_getExecTotTime(IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double PerformanceResult_getExecSelfTime(IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double PerformanceResult_getExecParentTime(IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float PerformanceResult_getExecTotPercentage(IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float PerformanceResult_getExecSelfPercentage(IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float PerformanceResult_getExecParentPercentage(IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 PerformanceResult_getCallers(IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 PerformanceResult_getThreadID(IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double PerformanceResult_getSystemTime(IntPtr result_reference);


            #endregion
        }

        public class PerformanceTracer : Reference , IThread
        {
            public PerformanceTracer(IntPtr nativeReference) : base(nativeReference) { }

            public PerformanceTracer() : base(PerformanceTracer_create()) { }

            public bool IsRunning(bool tick = false)
            {
                return PerformanceTracer_isRunning(GetNativeReference(),tick);
            }
            public bool IsStopping(bool tick = false)
            {
                return PerformanceTracer_isStopping(GetNativeReference(),tick);
            }

            public bool Run(bool waitForRunning = false)
            {
                return PerformanceTracer_run(GetNativeReference(),waitForRunning);
            }

            public void Stop(bool waitForStopping = false)
            {
                PerformanceTracer_stop(GetNativeReference(),waitForStopping);
            }

            public bool AddSection(string sectionName, UInt32 threadID = 0)
            {
                return PerformanceTracer_addSection(GetNativeReference(), sectionName, threadID);
            }
            public bool RemoveSection(string sectionName, UInt32 threadID = 0)
            {
                return PerformanceTracer_removeSection(GetNativeReference(), sectionName, threadID);
            }

            public bool AddAll(UInt32 threadID = 0)
            {
                return PerformanceTracer_addAll(GetNativeReference(), threadID);
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PerformanceTracer_create();

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool PerformanceTracer_isRunning(IntPtr tracer_reference,bool tick);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool PerformanceTracer_isStopping(IntPtr tracer_reference,bool tick);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool PerformanceTracer_run(IntPtr tracer_reference,bool waitForRunning);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PerformanceTracer_stop(IntPtr tracer_reference, bool waitForStopping);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool PerformanceTracer_addSection(IntPtr tracer_reference, string sectionName, UInt32 threadID);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool PerformanceTracer_removeSection(IntPtr tracer_reference, string sectionName, UInt32 threadID);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool PerformanceTracer_addAll(IntPtr tracer_reference, UInt32 threadID);

            #endregion
        }
    }
}

