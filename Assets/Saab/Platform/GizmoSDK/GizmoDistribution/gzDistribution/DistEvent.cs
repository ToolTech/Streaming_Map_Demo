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
// File			: DistEvent.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistEvent class
// Author		: Anders Modén		
// Product		: GizmoDistribution 2.10.7
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace GizmoDistribution
    {
        public class DistEventAttributeIterator : Reference,IEnumerator<DistAttribute>
        {
            public DistEventAttributeIterator(DistEvent e):base(DistEventAttributeIterator_create(e.GetNativeReference())){}

            public DistAttribute Current => m_current;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if(DistEventAttributeIterator_iterate(GetNativeReference()))
                {
                    m_current = new DistAttribute(DistEventAttributeIterator_current(GetNativeReference()));

                    return true;
                }

                return false;
            }

            public void Reset()
            {
                DistEventAttributeIterator_reset(GetNativeReference());
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistEventAttributeIterator_create(IntPtr event_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistEventAttributeIterator_iterate(IntPtr iterator_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistEventAttributeIterator_current(IntPtr iterator_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistEventAttributeIterator_reset(IntPtr iterator_ref);

            private DistAttribute m_current;
        }
                     
        public class DistEvent : Reference, IEnumerable<DistAttribute>
        {
            protected DistEvent(IntPtr nativeReference) : base(nativeReference){}

            public DistEvent():base(DistEvent_createDefaultEvent()){}

            public bool SetAttributeValues(object source,bool allProperties=false)
            {
                return StorePropertiesAndFields(this, source, allProperties);
            }

            public object GetAttributeValues(Type objectType,bool allProperties=false)
            {
                object obj = Activator.CreateInstance(objectType);

                RestorePropertiesAndFields(this, obj, allProperties);

                return obj;
            }

            public T GetAttributeValues<T>(bool allProperties) where T : class
            {
                return GetAttributeValues(typeof(T), allProperties) as T;
            }

            public bool SetAttributeValue(NativeString name, DynamicType value)
            {
                return DistEvent_setAttributeValue(GetNativeReference(), name.GetNativeReference(), value.GetNativeReference());
            }

            public DynamicType GetAttributeValue(NativeString name)
            {
                return new DynamicType(DistEvent_getAttributeValue(GetNativeReference(), name.GetNativeReference()));
            }

            public bool HasAttribute(NativeString name)
            {
                return DistEvent_hasAttribute(GetNativeReference(), name.GetNativeReference());
            }

            public DistInstanceID GetSource()
            {
                return new DistInstanceID(DistEvent_getSource(GetNativeReference()));
            }

            public DistInstanceID GetDestination()
            {
                return new DistInstanceID(DistEvent_getDestination(GetNativeReference()));
            }


            public static void InitializeFactory()
            {
                AddFactory(new DistEvent());
            }

            public static void UninitializeFactory()
            {
                RemoveFactory("gzDistEvent");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new DistEvent(nativeReference) as Reference;
            }

            
            public override string ToString()
            {
                return Marshal.PtrToStringUni(DistEvent_asString(GetNativeReference()));
            }

            public string ToJSON()
            {
                return Marshal.PtrToStringUni(DistEvent_asJSON(GetNativeReference()));
            }

            public bool RestoreFromXML(string xml)
            {
                return DistEvent_fromXML(GetNativeReference(),xml);
            }

            public bool RestoreFromJSON(string json)
            {
                return DistEvent_fromJSON(GetNativeReference(), json);
            }

            // --- Reflection mechanisms --------------------------------

            public bool StorePropertiesAndFields(bool allProperties=false)
            {
                return StorePropertiesAndFields(this, this, allProperties);
            }

            public void RestorePropertiesAndFields(bool allProperties = false)
            {
                RestorePropertiesAndFields(this, this, allProperties);
            }

            // --- Reflection mechanisms --------------------------------

            static public bool StorePropertiesAndFields(DistEvent e,object obj,bool allProperties = false)
            {
                foreach (System.Reflection.PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (allProperties || Attribute.IsDefined(prop, typeof(DistProperty)))
                        if (!e.SetAttributeValue(prop.Name, DynamicType.CreateDynamicType(prop.GetValue(obj), allProperties)))
                            return false;
                }

                foreach (System.Reflection.FieldInfo field in obj.GetType().GetFields())
                {
                    if (allProperties || Attribute.IsDefined(field, typeof(DistProperty)))
                        if (!e.SetAttributeValue(field.Name, DynamicType.CreateDynamicType(field.GetValue(obj), allProperties)))
                            return false;
                }

                return true;
            }

            static public void RestorePropertiesAndFields(DistEvent e, object obj, bool allProperties = false)
            {
                foreach (System.Reflection.PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (allProperties || Attribute.IsDefined(prop, typeof(DistProperty)))
                        prop.SetValue(obj, e.GetAttributeValue(prop.Name).GetObject(prop.PropertyType, allProperties));
                }

                foreach (System.Reflection.FieldInfo field in obj.GetType().GetFields())
                {
                    if (allProperties || Attribute.IsDefined(field, typeof(DistProperty)))
                        field.SetValue(obj, e.GetAttributeValue(field.Name).GetObject(field.FieldType, allProperties));
                }
            }

            #region --------------------------- private ----------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistEvent_fromXML(IntPtr event_reference,string xml);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistEvent_fromJSON(IntPtr event_reference, string json);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistEvent_createDefaultEvent();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistEvent_setAttributeValue(IntPtr event_reference, IntPtr name,IntPtr dynamic_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistEvent_getAttributeValue(IntPtr event_reference, IntPtr name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistEvent_hasAttribute(IntPtr event_reference, IntPtr name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistEvent_asString(IntPtr event_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistEvent_asJSON(IntPtr event_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistEvent_getSource(IntPtr event_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistEvent_getDestination(IntPtr event_reference);


            public IEnumerator<DistAttribute> GetEnumerator()
            {
                return new DistEventAttributeIterator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }


            
            #endregion
        }
    }
}
