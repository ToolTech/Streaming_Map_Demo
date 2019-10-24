//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s)
// herein belong to SAAB AB. The program(s) may be used and/or
// copied only with the written permission of Saab AB, or in
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
// File			: DistSession.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistSession class
// Author		: Anders Modén		
// Product		: GizmoDistribution 2.10.4
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
// AMO  181210  Added Concurrent reading of dictionary
//
//******************************************************************************

using System;
using System.Collections.Concurrent;
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
                _instanses = new ConcurrentDictionary<IntPtr, DistSession>();
            }
            public DistSession GetSession(IntPtr nativeReference)
            {
                // We must allow GetSession for null reference

                if (nativeReference == IntPtr.Zero)
                    return null;

                DistSession sess;

                if (!_instanses.TryGetValue(nativeReference, out sess))
                {
                    sess = new DistSession(nativeReference);

                    _instanses.TryAdd(nativeReference, sess);
                }

                if (sess==null || !sess.IsValid())
                {
                    _instanses[nativeReference] = sess = new DistSession(nativeReference);
                }

                return sess;
            }

            public void Clear()
            {
                foreach(var key in _instanses )
                {
                    key.Value.Dispose();
                }

                _instanses.Clear();
            }

            public bool DropSession(IntPtr nativeReference)
            {
                DistSession obj;
                return _instanses.TryRemove(nativeReference,out obj);
            }


            ConcurrentDictionary<IntPtr, DistSession> _instanses;
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
