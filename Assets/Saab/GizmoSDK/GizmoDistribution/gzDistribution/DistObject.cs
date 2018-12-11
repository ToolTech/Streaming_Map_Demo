//******************************************************************************
// File			: DistObject.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistObject class
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
// AMO  181210  Added Concurrent reading of dictionary
//
//******************************************************************************

using System;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;
using System.Collections.Concurrent;


namespace GizmoSDK
{
    namespace GizmoDistribution
    {
       
        public class DistObjectInstanceManager
        {
            public DistObjectInstanceManager()
            {
                _instanses = new ConcurrentDictionary<IntPtr, DistObject>();
            }

            public DistObject GetObject(IntPtr nativeReference)
            {
                // We must allow GetObject for null reference

                if (nativeReference == IntPtr.Zero)
                    return null;

                DistObject obj;

                if (!_instanses.TryGetValue(nativeReference, out obj))
                {
                    obj = Reference.CreateObject(nativeReference) as DistObject;

                    // At least we will always get a DistObject

                    _instanses.TryAdd(nativeReference, obj);
                }

                if( obj==null || !obj.IsValid() )
                {
                    // In case we lost the factory native ref
                    _instanses[nativeReference]=obj=Reference.CreateObject(nativeReference) as DistObject;
                }

                return obj;
            }

            public void Clear()
            {
                foreach (var key in _instanses)
                {
                    key.Value.Dispose();
                }

                _instanses.Clear();
            }

            public bool DropObject(IntPtr nativeReference)
            {
                DistObject obj;
                return _instanses.TryRemove(nativeReference,out obj);
            }


            ConcurrentDictionary<IntPtr, DistObject>         _instanses;
        }

        public class DistObject : Reference
        {
            public DistObject(IntPtr nativeReference) : base(nativeReference){}

            public DistObject(string name):base(DistObject_createDefaultObject(name)){}

            public string GetName()
            {
                return Marshal.PtrToStringUni(DistObject_getName(GetNativeReference()));
            }

            public bool SetAttributeValue(string name, DynamicType value)
            {
                return DistObject_setAttributeValue(GetNativeReference(), name, value.GetNativeReference());
            }

            public bool RemoveAttribute(string name)
            {
                return DistObject_removeAttribute(GetNativeReference(), name);
            }

            public DynamicType GetAttributeValue(string name)
            {
                var value = DistObject_getAttributeValue(GetNativeReference(), name);
                return value == IntPtr.Zero ? null : new DynamicType(value);
            }

            public bool HasAttribute(string name)
            {
                return DistObject_hasAttribute(GetNativeReference(), name);
            }

            public static void InitializeFactory()
            {
                AddFactory(new DistObject("factory"));
            }

            public static void UninitializeFactory()
            {
                RemoveFactory("gzDistObject");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new DistObject(nativeReference) as Reference;
            }
                        
            public override string ToString()
            {
                return Marshal.PtrToStringUni(DistObject_asString(GetNativeReference()));
            }

            public string ToJSON()
            {
                return Marshal.PtrToStringUni(DistObject_asJSON(GetNativeReference()));
            }

            public bool RestoreFromXML(string xml)
            {
                return DistObject_fromXML(GetNativeReference(), xml);
            }

            public bool RestoreFromJSON(string json)
            {
                return DistObject_fromJSON(GetNativeReference(), json);
            }

            #region --------------------------- private ----------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistObject_fromXML(IntPtr event_reference, string xml);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistObject_fromJSON(IntPtr event_reference, string json);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistObject_createDefaultObject(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistObject_setAttributeValue(IntPtr event_reference,string name,IntPtr dynamic_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistObject_removeAttribute(IntPtr event_reference, string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistObject_getAttributeValue(IntPtr event_reference, string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistObject_hasAttribute(IntPtr event_reference, string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistObject_getName(IntPtr object_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistObject_asString(IntPtr event_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistObject_asJSON(IntPtr event_reference);

            #endregion
        }
    }
}
