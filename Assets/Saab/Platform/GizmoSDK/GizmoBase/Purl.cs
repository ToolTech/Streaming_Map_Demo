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
// File			: Purl.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to PURL functions
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
// AMO	200415	Created file 	                                (2.10.15)
//
//******************************************************************************

using System;
using System.Runtime.InteropServices;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public enum PurlCommand
        {
           REQUEST_FILE,            // 0
           FILE_DATA,               // 1
           ERROR,                   // 2
           SAVE_FILE,               // 3	
           OK,                      // 4
           REQUEST_SAVE_FILE,       // 5
           REQUEST_DATA,            // 6
           DATA,                    // 7
           AVAIL,                   // 8
           UNKNOWN=99
        };

        public class PurlCommandClass : Reference , ISerializeData
        {
            public PurlCommandClass(IntPtr nativeReference) :  base(nativeReference)
            {

            }
            public uint GetDataSize(SerializeAdapter adapter = null)
            {
                return PurlCommandClass_getDataSize(GetNativeReference(), adapter?.GetNativeReference() ?? IntPtr.Zero);
            }

            public void PushBack(SerializeAdapter adapter)
            {
                PurlCommandClass_pushBack(GetNativeReference(), adapter.GetNativeReference());
            }

            public void Read(SerializeAdapter adapter)
            {
                PurlCommandClass_read(GetNativeReference(), adapter.GetNativeReference());
            }

            public void Write(SerializeAdapter adapter)
            {
                PurlCommandClass_write(GetNativeReference(), adapter.GetNativeReference());
            }
                       

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 PurlCommandClass_getDataSize(IntPtr purlcommand_reference,IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlCommandClass_pushBack(IntPtr purlcommand_reference, IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlCommandClass_read(IntPtr purlcommand_reference, IntPtr adapter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlCommandClass_write(IntPtr purlcommand_reference, IntPtr adapter_reference);

            #endregion
        }

        public class PurlRequestCommand : PurlCommandClass
        {
            public PurlRequestCommand(Guid id = default, string topic = default, DynamicType parameters=default) : base(PurlRequestCommand_create(id,topic, parameters?.GetNativeReference() ?? IntPtr.Zero))
            {

            }

            public Guid Id
            {
                get { Guid value=new Guid(); PurlRequestCommand_getID(GetNativeReference(), ref value); return value; }
                set { PurlRequestCommand_setID(GetNativeReference(), value); }
            }

            public string Topic
            {
                get { return Marshal.PtrToStringUni(PurlRequestCommand_getTopic(GetNativeReference())); }
                set { PurlRequestCommand_setTopic(GetNativeReference(), value); }
            }

            public DynamicType Parameters
            {
                get { return new DynamicType(PurlRequestCommand_getParameters(GetNativeReference())); }
                set { PurlRequestCommand_setParameters(GetNativeReference(), value.GetNativeReference()); }
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlRequestCommand_create(Guid id, string topic, IntPtr parameter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlRequestCommand_getID(IntPtr purlcommand_reference,ref Guid id);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlRequestCommand_setID(IntPtr purlcommand_reference,Guid id);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlRequestCommand_getTopic(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlRequestCommand_setTopic(IntPtr purlcommand_reference, string topic);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlRequestCommand_getParameters(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlRequestCommand_setParameters(IntPtr purlcommand_reference, IntPtr parameter_reference);


            #endregion
        }

        public class PurlAvailCommand : PurlCommandClass
        {
            public PurlAvailCommand(Guid id = default, string topic = default, double time=0) : base(PurlAvailCommand_create(id, topic, time))
            {

            }

            public Guid Id
            {
                get { Guid value = new Guid(); PurlAvailCommand_getID(GetNativeReference(), ref value); return value; }
                set { PurlAvailCommand_setID(GetNativeReference(), value); }
            }

            public string Topic
            {
                get { return Marshal.PtrToStringUni(PurlAvailCommand_getTopic(GetNativeReference())); }
                set { PurlAvailCommand_setTopic(GetNativeReference(), value); }
            }

            public double Time
            {
                get { return PurlAvailCommand_getTime(GetNativeReference()); }
                set { PurlAvailCommand_setTime(GetNativeReference(), value); }
            }

            public DynamicType Parameters
            {
                get { return new DynamicType(PurlAvailCommand_getParameters(GetNativeReference())); }
                set { PurlAvailCommand_setParameters(GetNativeReference(), value.GetNativeReference()); }
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlAvailCommand_create(Guid id, string topic, double time);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlAvailCommand_getID(IntPtr purlcommand_reference, ref Guid id);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlAvailCommand_setID(IntPtr purlcommand_reference, Guid id);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlAvailCommand_getTopic(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlAvailCommand_setTopic(IntPtr purlcommand_reference, string topic);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double PurlAvailCommand_getTime(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlAvailCommand_setTime(IntPtr purlcommand_reference, double time);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlAvailCommand_getParameters(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlAvailCommand_setParameters(IntPtr purlcommand_reference, IntPtr parameter_reference);


            #endregion
        }

        public class PurlDataCommand : PurlCommandClass
        {
            public PurlDataCommand(Guid id = default, string topic = default, DynamicType result = default) : base(PurlDataCommand_create(id, topic, result?.GetNativeReference() ?? IntPtr.Zero))
            {

            }

            public Guid Id
            {
                get { Guid value = new Guid(); PurlDataCommand_getID(GetNativeReference(), ref value); return value; }
                set { PurlDataCommand_setID(GetNativeReference(), value); }
            }

            public string Topic
            {
                get { return Marshal.PtrToStringUni(PurlDataCommand_getTopic(GetNativeReference())); }
                set { PurlDataCommand_setTopic(GetNativeReference(), value); }
            }

            public DynamicType Result
            {
                get { return new DynamicType(PurlDataCommand_getResult(GetNativeReference())); }
                set { PurlDataCommand_setResult(GetNativeReference(), value.GetNativeReference()); }
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlDataCommand_create(Guid id, string topic, IntPtr result_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlDataCommand_getID(IntPtr purlcommand_reference, ref Guid id);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlDataCommand_setID(IntPtr purlcommand_reference, Guid id);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlDataCommand_getTopic(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlDataCommand_setTopic(IntPtr purlcommand_reference, string topic);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlDataCommand_getResult(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlDataCommand_setResult(IntPtr purlcommand_reference, IntPtr parameter_reference);

            #endregion
        }

        public class Purl
        {

            public static void SendCommand(SerializeAdapter adapter, PurlCommand command, UInt32 payload_size=0, PurlCommandClass command_payload =null)
            {
                Purl_sendCommand(adapter.GetNativeReference(), command,payload_size, command_payload?.GetNativeReference() ?? IntPtr.Zero);
            }

            public static void SendPayload(SerializeAdapter adapter, byte[] payload)
            {
                IntPtr p = Marshal.AllocHGlobal(payload.Length);

                Marshal.Copy(payload, 0, p, payload.Length); // Transfer to unmanaged memory

                Purl_sendPayload(adapter.GetNativeReference(), p, (uint)payload.Length);

                Marshal.FreeHGlobal(p);
            }

            public static void SendPayload(SerializeAdapter adapter, SerializeAdapter payload)
            {
                Purl_sendPayload2(adapter.GetNativeReference(), payload.GetNativeReference());
            }

            public static bool ReceiveCommand(SerializeAdapter adapter, out PurlCommand command, out UInt32 payload_length, UInt32 retry = 50 )
            {
                PurlCommand result_Command = PurlCommand.UNKNOWN;
                UInt32 result_Length = 0;

                bool result = Purl_receiveCommand(adapter.GetNativeReference(), ref result_Command, ref result_Length, retry);

                command = result_Command;
                payload_length = result_Length;

                return result;
            }

            public static bool ReceivePayload(SerializeAdapter adapter, PurlCommandClass command_payload, UInt32 retry = 50)
            {
                return Purl_receivePayload(adapter.GetNativeReference(), command_payload.GetNativeReference(), retry);
            }

            public static bool ReceivePurlPayload(SerializeAdapter adapter, ref byte [] payload, UInt32 data_payload_length, UInt32 retry=50)
            {
                if (payload == null || payload.Length != data_payload_length)
                    payload = new byte[data_payload_length];

                IntPtr p = Marshal.AllocHGlobal((int)data_payload_length);

                bool result = Purl_receivePayload2(adapter.GetNativeReference(),p, data_payload_length,retry);

                Marshal.Copy(p, payload, 0, (int)data_payload_length); // Transfer to managed memory

                Marshal.FreeHGlobal(p);

                return result;
            }


            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Purl_sendCommand(IntPtr adapter_reference, PurlCommand command, UInt32 payload_size , IntPtr command_payload_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Purl_sendPayload(IntPtr adapter_reference, IntPtr data, UInt32 payload_size);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Purl_sendPayload2(IntPtr adapter_reference, IntPtr payload_reference);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Purl_receiveCommand(IntPtr adapter_reference, ref PurlCommand command, ref UInt32 payload_size, UInt32 retry);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Purl_receivePayload(IntPtr adapter_reference, IntPtr command_class_reference,UInt32 retry);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Purl_receivePayload2(IntPtr adapter_reference, IntPtr destination, UInt32 length,UInt32 retry);

            #endregion
        }
    }
}

