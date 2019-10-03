//******************************************************************************
// File			: Serialize.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzSerialize.cpp
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.4
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
        public interface ISerializeData
        {
            void Write(SerializeAdapter adapter);
            void Read(SerializeAdapter adapter);
            void PushBack(SerializeAdapter adapter);
            UInt32 GetDataSize(SerializeAdapter adapter=null);
        }
        public class SerializeAdapter : Reference
        {
            [Flags]
            public enum SerializeAction
            {
                OUTPUT = 1,
                INPUT = 2,
                DUPLEX=3,
            }

            [Flags]
            public enum AdapterFlags
            {
               DEFAULT=0,
            }
                        
            public SerializeAdapter(IntPtr nativeReference) : base(nativeReference) { }

            public static SerializeAdapter GetURLAdapter(string url, SerializeAction action=SerializeAction.INPUT, AdapterFlags flags=AdapterFlags.DEFAULT,string password="")
            {
                SerializeAdapter adapter= new SerializeAdapter(SerializeAdapter_getURLAdapter(url,action,flags,password));
                return adapter;
            }

            public bool HasData(UInt32 minCount=1)
            {
                return SerializeAdapter_hasData(GetNativeReference(), minCount);
            }

            public UInt32 Length()
            {
                return SerializeAdapter_length(GetNativeReference());
            }

            public void Write(ISerializeData data)
            {
                data.Write(this);
            }

            public void Write(UInt32 value,bool bigEndian=true)
            {
                SerializeAdapter_write_UInt32(GetNativeReference(), value, bigEndian);
            }

            public void Write(byte value)
            {
                SerializeAdapter_write(GetNativeReference(), value);
            }

            public void Write(byte [] data)
            {
                IntPtr p = Marshal.AllocHGlobal(data.Length);

                Marshal.Copy(data,0,p,data.Length); // Transfer to unmanaged memory

                SerializeAdapter_write_count(GetNativeReference(), p,data.Length);

                Marshal.FreeHGlobal(p);
            }

            public void Read(ISerializeData data)
            {
                data.Read(this);
            }

            public void Read(out byte data)
            {
                data = new byte();
                SerializeAdapter_read(GetNativeReference(), ref data);
            }

            public uint Read(ref byte[] data)
            {
                IntPtr p = Marshal.AllocHGlobal(data.Length);

                UInt32 count=SerializeAdapter_read_count(GetNativeReference(), p,data.Length);

                Marshal.Copy(p, data, 0, (int)count); // Transfer to managed memory

                Marshal.FreeHGlobal(p);

                return count;
            }

            public void PushBack(ISerializeData data)
            {
                data.PushBack(this);
            }

            public bool HasError()
            {
                return SerializeAdapter_hasError(GetNativeReference());
            }

            public string GetError()
            {
                return Marshal.PtrToStringUni(SerializeAdapter_getError(GetNativeReference()));
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr SerializeAdapter_getURLAdapter(string url, SerializeAction action , AdapterFlags flags , string password);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void SerializeAdapter_write_UInt32(IntPtr adapter_reference,UInt32 value,bool bigEndian);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void SerializeAdapter_write(IntPtr adapter_reference, byte value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void SerializeAdapter_write_count(IntPtr adapter_reference, IntPtr byte_mem, int count);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void SerializeAdapter_read(IntPtr adapter_reference, ref byte value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 SerializeAdapter_read_count(IntPtr adapter_reference, IntPtr byte_mem,int count);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool SerializeAdapter_hasError(IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr SerializeAdapter_getError(IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool SerializeAdapter_hasData(IntPtr adapter_reference,UInt32 minCount);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 SerializeAdapter_length(IntPtr adapter_reference);

            #endregion
        }
    }
}
