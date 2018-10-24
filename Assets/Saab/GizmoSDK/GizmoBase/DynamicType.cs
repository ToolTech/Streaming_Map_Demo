//******************************************************************************
// File			: DynamicType.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzDynamicType class
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.1
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

using System.Runtime.InteropServices;
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public class DynamicType : Reference , ISerializeData
        {
            public static class Type
            {
                public static string STRING = "str";
                public static string VOID = "void";
                public static string NUMBER = "num";
                public static string POINTER = "ptr";
                public static string REFERENCE = "ref";
                public static string ERROR = "error";
                public static string INT64 = "llnum";
                public static string ARRAY = "array";
                public static string VEC2 = "vec2";
                public static string VEC3 = "vec3";
                public static string VEC4 = "vec4";
                public static string CONTAINER = "cont";
                public static string CHAIN = "chain";
                public static string GUID = "guid";
                public static string TIME_TAGGED = "ttag";
            }

            #region ---------------------- implicits --------------------

            public static implicit operator DynamicType(double value)
            {
                return new DynamicType(value);
            }
                        
            public static implicit operator double(DynamicType type)
            {
                return type.GetNumber();
            }
            
            public static implicit operator DynamicType(string value)
            {
                return new DynamicType(value);
            }

            public static implicit operator string(DynamicType type)
            {
                return type.GetString();
            }

            public static implicit operator DynamicType(Vec2 value)
            {
                return new DynamicType(value);
            }

            public static implicit operator Vec2(DynamicType type)
            {
                return type.GetVec2();
            }

            public static implicit operator DynamicType(Vec3 value)
            {
                return new DynamicType(value);
            }

            public static implicit operator Vec3(DynamicType type)
            {
                return type.GetVec3();
            }

            public static implicit operator DynamicType(Vec4 value)
            {
                return new DynamicType(value);
            }

            public static implicit operator Vec4(DynamicType type)
            {
                return type.GetVec4();
            }

            public static implicit operator DynamicType(Guid value)
            {
                return new DynamicType(value);
            }

            public static implicit operator Guid(DynamicType type)
            {
                return type.GetGuid();
            }

            
            #endregion

            public DynamicType() : base(DynamicType_create_void()) { }
            public DynamicType(double value) : base(DynamicType_create_num(value)) { }
            public DynamicType(string value) : base(DynamicType_create_str(value)) { }
            public DynamicType(Vec2 value) : base(DynamicType_create_vec2(value)) { }
            public DynamicType(Vec3 value) : base(DynamicType_create_vec3(value)) { }
            public DynamicType(Vec4 value) : base(DynamicType_create_vec4(value)) { }

            public DynamicType(Guid value) : base(DynamicType_create_guid(value.ToString())) { }

            public DynamicType(IntPtr nativeReference) : base(nativeReference) { }

            public DynamicType(Reference reference) : base(DynamicType_create_reference(reference.GetNativeReference())) { }

            public string GetDynamicType()
            {
                if (!IsValid())
                    return "";

                return Marshal.PtrToStringUni(DynamicType_getDynamicType(GetNativeReference()));
            }

            public bool Is(string type)
            {
                if (!IsValid())
                    return false;

                return DynamicType_is(GetNativeReference(), type);
            }

            public double GetNumber()
            {
                if(!IsValid())
                    throw (new Exception("DynamicType is not VALID"));

                return DynamicType_getNumber(GetNativeReference());
            }

            public Vec2 GetVec2()
            {
                if (!IsValid())
                    throw (new Exception("DynamicType is not VALID"));

                return DynamicType_getVec2(GetNativeReference());
            }

            public Vec3 GetVec3()
            {
                if (!IsValid())
                    throw (new Exception("DynamicType is not VALID"));

                return DynamicType_getVec3(GetNativeReference());
            }

            public Reference GetReference()
            {
                if (!IsValid())
                    throw (new Exception("DynamicType is not VALID"));

                return new Reference(DynamicType_getReference(GetNativeReference()));
            }

            public Vec4 GetVec4()
            {
                if (!IsValid())
                    throw (new Exception("DynamicType is not VALID"));

                return DynamicType_getVec4(GetNativeReference());
            }

            public Guid GetGuid()
            {
                if (!IsValid())
                    throw (new Exception("DynamicType is not VALID"));

                return Guid.Parse(Marshal.PtrToStringUni(DynamicType_getGuid(GetNativeReference())));
            }

            public string GetString()
            {
                if (!IsValid())
                    return "Invalid";

                return Marshal.PtrToStringUni(DynamicType_getString(GetNativeReference()));
            }

            public string AsString(bool stripXML = true, bool skipDynTag = true, string tagName="data")
            {
                if (!IsValid())
                    return "Invalid";

                return Marshal.PtrToStringUni(DynamicType_asString(GetNativeReference(),stripXML,skipDynTag,tagName));
            }


            public override string ToString()
            {
                //return ToJSON();
                return AsString();
            }

            public string ToXML(bool skipDynTag=true)
            {
                return AsString(false, skipDynTag);
            }

            public static DynamicType FromXML(string xml,bool skipDynTag=true)
            {
                return new DynamicType(DynamicType_fromXML(xml,skipDynTag));
            }

            public string ToJSON()
            {
                if (!IsValid())
                    return "Invalid";

                return Marshal.PtrToStringUni(DynamicType_asJSON(GetNativeReference()));
            }

            public static DynamicType FromJSON(string json)
            {
                return new DynamicType(DynamicType_fromJSON(json));
            }

            public void Write(SerializeAdapter adapter)
            {
                DynamicType_write(GetNativeReference(), adapter.GetNativeReference());
            }

            public void Read(SerializeAdapter adapter)
            {
                DynamicType_read(GetNativeReference(), adapter.GetNativeReference());
            }

            public void PushBack(SerializeAdapter adapter)
            {
                DynamicType_pushBack(GetNativeReference(), adapter.GetNativeReference());
            }

            public uint GetDataSize(SerializeAdapter adapter = null)
            {
                return DynamicType_getDataSize(GetNativeReference(),adapter?.GetNativeReference()??IntPtr.Zero);
            }

            // -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_create_void();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_create_num(double value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_create_guid(string value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_create_vec2(Vec2 value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_create_vec3(Vec3 value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_create_vec4(Vec4 value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_create_reference(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_create_str(string value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_getDynamicType(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DynamicType_is(IntPtr reference,string type);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern Vec2 DynamicType_getVec2(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern Vec3 DynamicType_getVec3(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern Vec4 DynamicType_getVec4(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_getReference(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double DynamicType_getNumber(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_getString(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_getGuid(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicType_read(IntPtr dynamic_reference,IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicType_write(IntPtr dynamic_reference, IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicType_pushBack(IntPtr dynamic_reference, IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 DynamicType_getDataSize(IntPtr dynamic_reference, IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_asString(IntPtr dynamic_reference, bool stripXML, bool skipDynTag, string tagName);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_asJSON(IntPtr dynamic_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_fromXML(string xml,bool skipDynTag);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_fromJSON(string json);

        }
    }
}

