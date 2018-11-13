//******************************************************************************
// File			: DistSession.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistSession class
// Author		: Anders Modén		
// Product		: GizmoDistribution 2.10.1
//		
// Copyright © 2003- Saab Training Systems AB, Sweden
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace GizmoDistribution
    {
        public class DistSessionInstanceManager
        {
            public DistSessionInstanceManager()
            {
                instanses = new Dictionary<IntPtr, DistSession>();
            }
            public DistSession GetSession(IntPtr nativeReference)
            {
                // We must allow GetSession for null reference

                if (nativeReference == IntPtr.Zero)
                    return null;

                lock (instanses)
                {
                    DistSession sess;

                    if (!instanses.TryGetValue(nativeReference, out sess))
                    {
                        sess = new DistSession(nativeReference);

                        instanses.Add(nativeReference, sess);
                    }

                    if (sess==null || !sess.IsValid())
                    {
                        instanses[nativeReference] = sess = new DistSession(nativeReference);
                    }

                    return sess;
                }
            }

            public void Clear()
            {
                lock (instanses)
                {
                    foreach(var key in instanses )
                    {
                        key.Value.Dispose();
                    }

                    instanses.Clear();
                }
            }

            public bool DropSession(IntPtr nativeReference)
            {
                lock (instanses)
                {
                    return instanses.Remove(nativeReference);
                }
            }


            Dictionary<IntPtr, DistSession> instanses;
        }

        public class DistSession : Reference 
        {
            public DistSession(IntPtr nativeReference) : base(nativeReference)
            {
            }

            public string GetName()
            {
                return Marshal.PtrToStringUni(DistSession_getName(GetNativeReference()));
            }


            #region --------------------------------- private --------------------------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistSession_getName(IntPtr session_reference);
           
            #endregion
        }
    }
}
