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
// File			: Serialize.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzSerialize.cpp
// Author		: Anders Mod�n		
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
// AMO	180301	Created file 	
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;
using System.IO;

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
            
            public enum SeekOrigin
            {
                Set,
                Current,
                End
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

            public bool IsActive()
            {
                return SerializeAdapter_isActive(GetNativeReference());
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
                try
                {
                    SerializeAdapter_write_buffer(GetNativeReference(), p, 0, (uint)data.Length, (uint)data.Length);
                }
                finally
                {
                    Marshal.FreeHGlobal(p);
                }
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
                try
                {
                    UInt32 count = SerializeAdapter_read_buffer(GetNativeReference(), p, 0, (uint)data.Length, (uint)data.Length);

                    Marshal.Copy(p, data, 0, (int)count); // Transfer to managed memory
                    return count;
                }
                finally
                { 
                    Marshal.FreeHGlobal(p);
                }
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

            public static bool SetAssetManagerHandle(IntPtr JNIEnvHandle,IntPtr assetManagerHandle)
            {
                return SerializeAdapter_SetAssetManagerHandle(JNIEnvHandle, assetManagerHandle);
            }

            public bool CanRead()
            {
                return SerializeAdapter_supportAction(GetNativeReference(), SerializeAction.INPUT);
            }

            public bool CanWrite()
            {
                return SerializeAdapter_supportAction(GetNativeReference(), SerializeAction.OUTPUT);
            }

            public bool CanSeek()
            {
                return SerializeAdapter_canSeek(GetNativeReference());
            }

            public uint Seek(int offset, SeekOrigin origin)
            {
                if (!CanSeek())
                    throw new NotSupportedException();

                var result = SerializeAdapter_seek(GetNativeReference(), offset, origin);
                if (result == uint.MaxValue)
                    throw new InvalidOperationException();

                return result;
            }

            public void Flush()
            {
                SerializeAdapter_flush(GetNativeReference());
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null || (offset + count) > buffer.Length)
                    throw new ArgumentException();

                IntPtr p = Marshal.AllocHGlobal(count);

                try
                {
                    var bytesRead = (int)SerializeAdapter_read_buffer(GetNativeReference(), p, 0, (uint)count, (uint)count);

                    Marshal.Copy(p, buffer, offset, bytesRead);
                    return bytesRead;
                }
                finally
                {
                    Marshal.FreeHGlobal(p);
                }
            }

            public void Write(byte[] buffer, int offset, int count)
            {
                if (buffer == null || (offset + count) > buffer.Length)
                    throw new ArgumentException();

                IntPtr p = Marshal.AllocHGlobal(count);
                Marshal.Copy(buffer, offset, p, count);

                try
                {
                    SerializeAdapter_write_buffer(GetNativeReference(), p, 0, (uint)count, (uint)count );
                }
                finally
                {
                    Marshal.FreeHGlobal(p);
                }

            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr SerializeAdapter_getURLAdapter(string url, SerializeAction action , AdapterFlags flags , string password);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void SerializeAdapter_write_UInt32(IntPtr adapter_reference,UInt32 value,bool bigEndian);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void SerializeAdapter_write(IntPtr adapter_reference, byte value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void SerializeAdapter_read(IntPtr adapter_reference, ref byte value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool SerializeAdapter_hasError(IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr SerializeAdapter_getError(IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool SerializeAdapter_hasData(IntPtr adapter_reference,UInt32 minCount);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 SerializeAdapter_length(IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool SerializeAdapter_isActive(IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool SerializeAdapter_SetAssetManagerHandle(IntPtr handle1, IntPtr handle2);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool SerializeAdapter_supportAction(IntPtr nativeRef, SerializeAction action);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool SerializeAdapter_canSeek(IntPtr nativeRef);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern uint SerializeAdapter_seek(IntPtr nativeRef, int offset, SeekOrigin origin );
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern uint SerializeAdapter_flush(IntPtr nativeRef);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern uint SerializeAdapter_read_buffer(IntPtr nativeRef, IntPtr buffer, uint offset, uint count, uint length);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern uint SerializeAdapter_write_buffer(IntPtr nativeRef, IntPtr buffer, uint offset, uint count, uint length);
            #endregion
        }

        public enum QueueMode
        {
            LIFO, 
            FIFO
        }

        public class SerializeAdapterQueue : SerializeAdapter
        {
            public SerializeAdapterQueue(QueueMode mode = QueueMode.FIFO,UInt32 chunksize=100) : base(SerializeAdapterQueue_create(mode,chunksize))
            {

            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr SerializeAdapterQueue_create(QueueMode mode,UInt32 chunksize);

            #endregion
        }



        public class SerializeAdapterStream : Stream
        {
            private SerializeAdapter _adapter;
            private bool _leaveOpen;

            public enum Mode
            {
                Read = SerializeAdapter.SerializeAction.INPUT,
                Write = SerializeAdapter.SerializeAction.OUTPUT,
                ReadWrite = SerializeAdapter.SerializeAction.DUPLEX,
            }

            public SerializeAdapterStream(string url, Mode mode = Mode.Read) : this(SerializeAdapter.GetURLAdapter(url, (SerializeAdapter.SerializeAction)(mode)), false)
            {
            }
            public SerializeAdapterStream(SerializeAdapter adapter, bool leaveOpen = false)
            {
                _adapter = adapter;
                _leaveOpen = leaveOpen;
            }
            public override bool CanRead => _adapter.CanRead();

            public override bool CanSeek => _adapter.CanSeek();

            public override bool CanWrite => _adapter.CanWrite();

            public override long Length => _adapter.Length();

            public override long Position
            {
                get => _adapter.Seek(0, SerializeAdapter.SeekOrigin.Current);
                set => _adapter.Seek((int)value, SerializeAdapter.SeekOrigin.Set);
            }

            public override void Flush()
            {
                _adapter.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _adapter.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                var adapterOrigin = default(SerializeAdapter.SeekOrigin);
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        adapterOrigin = SerializeAdapter.SeekOrigin.Set;
                        break;
                    case SeekOrigin.Current:
                        adapterOrigin = SerializeAdapter.SeekOrigin.Current;
                        break;
                    case SeekOrigin.End:
                        adapterOrigin = SerializeAdapter.SeekOrigin.End;
                        break;
                }

                return _adapter.Seek((int)offset, adapterOrigin);
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _adapter.Write(buffer, offset, count);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_adapter != null && !_leaveOpen)
                    {
                        _adapter.Dispose();
                        _adapter = null;
                    }
                }

                base.Dispose(disposing);
            }
        }
    }
}
