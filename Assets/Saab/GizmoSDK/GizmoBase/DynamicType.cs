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
            public static Func<System.Reflection.AssemblyName, System.Reflection.Assembly> AssemblyResolver;
            public static Func<System.Reflection.Assembly, string, bool, System.Type> TypeResolver;

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

            const string TYPE_REFLECT = "_type_";

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

            public static DynamicType CreateDynamicType(object obj,bool allProperties=false,bool addReflectedType=false)
            {
                if (obj == null)
                    return new DynamicType();

                System.Type t = obj.GetType();

                if (obj is DynamicType)
                    return obj as DynamicType;

                if(obj is DynamicTypeContainer)
                    return (DynamicType)(obj as DynamicTypeContainer);

                if (obj is DynamicTypeArray)
                    return (DynamicType)(obj as DynamicTypeArray);

                if (t == typeof(string))
                    return new DynamicType((string)obj);

                if (t == typeof(Int64))
                    return new DynamicTypeInt64((Int64)obj);

                if (t == typeof(UInt64))
                    return new DynamicTypeInt64((UInt64)obj);

                if (t == typeof(Vec2))
                    return new DynamicType((Vec2)obj);

                if (t == typeof(Vec3))
                    return new DynamicType((Vec3)obj);

                if (t == typeof(Vec4))
                    return new DynamicType((Vec4)obj);

                if (t == typeof(Guid))
                    return new DynamicType((Guid)obj);

                if (t == typeof(Reference))
                    return new DynamicType((Reference)obj);

                if (t == typeof(System.Type))
                    return new DynamicType(t.AssemblyQualifiedName);

                if (t.IsEnum)
                {
                    if (Marshal.SizeOf(Enum.GetUnderlyingType(obj.GetType())) <= sizeof(UInt32))
                        return new DynamicType((UInt32)Convert.ChangeType(obj, typeof(UInt32)));
                    else
                        return new DynamicType((UInt64)Convert.ChangeType(obj, typeof(UInt64)));
                }
                               

                if(t.IsValueType && !t.IsPrimitive)     // Struct
                {
                    DynamicTypeContainer cont = new DynamicTypeContainer();
                    DynamicTypeContainer.StorePropertiesAndFields(cont, obj, allProperties);

                    if (addReflectedType)
                        cont.SetAttribute(TYPE_REFLECT, t.AssemblyQualifiedName);

                    return cont;
                }

                if (obj is System.Collections.IEnumerable) 
                {
                    System.Type elemType = obj.GetType().GetElementType();

                    if (elemType==null && obj.GetType().IsGenericType)
                        elemType = obj.GetType().GenericTypeArguments[0];

                    DynamicTypeArray array = new DynamicTypeArray();
                    DynamicTypeArray.StoreEnumerable(array, (System.Collections.IEnumerable)obj, allProperties,elemType);

                    return array;
                }

                if (t.IsClass)
                {
                    DynamicTypeContainer cont = new DynamicTypeContainer();
                    DynamicTypeContainer.StorePropertiesAndFields(cont, obj, allProperties);

                    if(addReflectedType)
                        cont.SetAttribute(TYPE_REFLECT, t.AssemblyQualifiedName);

                    return cont;
                }

                return new DynamicType((double)Convert.ChangeType(obj,typeof(double)));    // default to double
            }

            public T GetObject<T>(bool allProperties = false)
            {
                return (T)GetObject(typeof(T), allProperties);
            }

            public object GetObject(System.Type t,bool allProperties=false)
            {
                if (Is(DynamicType.Type.VOID))
                    return null;

                if (t==typeof(DynamicType))
                    return this;

                if (t.IsSubclassOf(typeof(DynamicType)))
                    return this;

                if (t == typeof(DynamicTypeArray) && Is(DynamicType.Type.ARRAY))
                    return (DynamicTypeArray)this;

                if (t.IsSubclassOf(typeof(DynamicTypeArray)) && Is(DynamicType.Type.ARRAY))
                {
                    object o = Activator.CreateInstance(t);

                    ((DynamicTypeArray)o).Set(this);

                    return o;
                }

                if (t == typeof(DynamicTypeContainer) && Is(DynamicType.Type.CONTAINER))
                    return (DynamicTypeContainer)this;

                if (t.IsSubclassOf(typeof(DynamicTypeContainer)) && Is(DynamicType.Type.CONTAINER))
                {
                    object o=Activator.CreateInstance(t);

                    ((DynamicTypeContainer)o).Set(this);
                    
                    return o;
                }

                // ------- Converts to builtins --------------------

                if (t==typeof(string))
                    return (string)this;

                if (t == typeof(UInt64))
                    return (UInt64)(DynamicTypeInt64)this;

                if (t == typeof(Int64))
                    return (Int64)(DynamicTypeInt64)this;

                if (t == typeof(Vec2))
                    return (Vec2)this;

                if (t == typeof(Vec3))
                    return (Vec3)this;

                if (t == typeof(Vec4))
                    return (Vec4)this;

                if (t == typeof(Guid))
                    return (Guid)this;

                if(t==typeof(System.Type))
                {
                    if (TypeResolver != null)
                        return System.Type.GetType((string)this, AssemblyResolver, TypeResolver);
                    else
                        return System.Type.GetType((string)this);
                }

                if (t == typeof(Reference) && Is(DynamicType.Type.REFERENCE))
                    return (Reference)this;
                
                if (t.IsSubclassOf(typeof(Reference)) && Is(DynamicType.Type.REFERENCE))
                    return (Reference)this;

                if (t == typeof(object) && !Is(Type.CONTAINER))
                {
                    // ------- Converts to builtins --------------------

                    if (Is(Type.STRING))
                        return (string)this;

                    if (Is(Type.INT64))
                        return (Int64)this;

                    if (Is(Type.VEC2))
                        return (Vec2)this;

                    if (Is(Type.VEC3))
                        return (Vec3)this;

                    if (Is(Type.VEC4))
                        return (Vec4)this;

                    if (Is(Type.GUID))
                        return (Guid)this;

                    if(Is(Type.REFERENCE))
                        return (Reference)this;
                }

                if (t.IsEnum)
                {
                    if(Marshal.SizeOf(Enum.GetUnderlyingType(t))<=sizeof(UInt32))
                        return Enum.ToObject(t, (UInt32)this);
                    else
                        return Enum.ToObject(t, (UInt64)this);
                }

                if (Is(DynamicType.Type.ARRAY))
                {
                    if (typeof(Array).IsAssignableFrom(t))        // Array
                    {
                        DynamicTypeArray array = (DynamicTypeArray)this;

                        Array obj = Array.CreateInstance(t.GetElementType(), array.Count);

                        DynamicTypeArray.RestoreArray(array, obj, allProperties);

                        return obj;
                    }

                    if (typeof(System.Collections.IList).IsAssignableFrom(t))
                    {
                        object obj = Activator.CreateInstance(t);

                        System.Collections.IList list = (System.Collections.IList)obj;

                        DynamicTypeArray array = (DynamicTypeArray)this;

                        if(t.IsGenericType)
                            DynamicTypeArray.RestoreList(array, list,t.GenericTypeArguments[0], allProperties);

                        return obj;
                    }


                    // We can create it but not populate it

                    return Activator.CreateInstance(t);
                                        
                }

                if(t.IsValueType && !t.IsPrimitive)                 // struct
                {
                    object obj = Activator.CreateInstance(t);

                    DynamicTypeContainer.RestorePropertiesAndFields(this, obj, allProperties);

                    return obj;
                }

                if (t.IsClass && Is(DynamicType.Type.CONTAINER))
                {
                    DynamicTypeContainer container = this;

                    object obj;

                    string _type_ = container.GetAttribute(TYPE_REFLECT);

                    if (_type_!=null)
                    {
                        if(TypeResolver!=null)
                            obj = Activator.CreateInstance(System.Type.GetType(_type_, AssemblyResolver, TypeResolver));
                        else
                            obj = Activator.CreateInstance(System.Type.GetType(_type_));

                        if (obj==null)
                            obj = Activator.CreateInstance(t);
                    }
                    else
                        obj = Activator.CreateInstance(t);

                    DynamicTypeContainer.RestorePropertiesAndFields(container, obj, allProperties);

                    return obj;
                }
    
                // Default to integer number 

                return Convert.ChangeType(GetNumber(), t);
            }

            public bool IsVoid()
            {
                return Is("void");
            }


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

                if (IsVoid())
                    return null;

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
                    return null;

                if (IsVoid())
                    return null;

                return Marshal.PtrToStringUni(DynamicType_getString(GetNativeReference()));
            }

            public string AsString(bool stripXML = true, bool skipDynTag = true, string tagName="data")
            {
                if (!IsValid())
                    return null;

                return Marshal.PtrToStringUni(DynamicType_asString(GetNativeReference(),stripXML,skipDynTag,tagName));
            }


            public override string ToString()
            {
                //return ToJSON();
                return AsString();
            }

            public static DynamicType FromString(string type, string value,bool skipDynTag = true)
            {
                return new DynamicType(DynamicType_fromString(type,value, skipDynTag));
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
            private static extern IntPtr DynamicType_fromString(string type,string value, bool skipDynTag);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_fromXML(string xml,bool skipDynTag);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicType_fromJSON(string json);

        }
    }
}

