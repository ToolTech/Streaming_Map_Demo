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
// Product		: GizmoBase 2.10.7
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
            AVAILABLE_DATA,                   // 8
            SUBSCRIBE,               // 9
            UNSUBSCRIBE,             // 10
            UNKNOWN = 99
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
            public PurlAvailCommand(Guid id = default, string topic = default, double time= 0,DynamicType parameters = default) : base(PurlAvailCommand_create(id, topic, time, parameters?.GetNativeReference() ?? IntPtr.Zero))
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
            private static extern IntPtr PurlAvailCommand_create(Guid id, string topic, double time, IntPtr parameter_reference);
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

        public class PurlSubscribeCommand : PurlCommandClass
        {
            public PurlSubscribeCommand(Guid subscriber_id = default, string topic = default, DynamicType parameters = default) : base(PurlSubscribeCommand_create(subscriber_id, topic, parameters?.GetNativeReference() ?? IntPtr.Zero))
            {

            }

            public Guid SubscriberId
            {
                get { Guid value = new Guid(); PurlSubscribeCommand_getSubscriberID(GetNativeReference(), ref value); return value; }
                set { PurlSubscribeCommand_setSubscriberID(GetNativeReference(), value); }
            }

            public string Topic
            {
                get { return Marshal.PtrToStringUni(PurlSubscribeCommand_getTopic(GetNativeReference())); }
                set { PurlSubscribeCommand_setTopic(GetNativeReference(), value); }
            }

            
            public DynamicType Parameters
            {
                get { return new DynamicType(PurlSubscribeCommand_getParameters(GetNativeReference())); }
                set { PurlSubscribeCommand_setParameters(GetNativeReference(), value.GetNativeReference()); }
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlSubscribeCommand_create(Guid id, string topic, IntPtr parameter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlSubscribeCommand_getSubscriberID(IntPtr purlcommand_reference, ref Guid subscriber_id);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlSubscribeCommand_setSubscriberID(IntPtr purlcommand_reference, Guid subscriber_id);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlSubscribeCommand_getTopic(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlSubscribeCommand_setTopic(IntPtr purlcommand_reference, string topic);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlSubscribeCommand_getParameters(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlSubscribeCommand_setParameters(IntPtr purlcommand_reference, IntPtr parameter_reference);


            #endregion
        }

        public class PurlUnSubscribeCommand : PurlCommandClass
        {
            public PurlUnSubscribeCommand(Guid subscriber_id = default, string topic = default, DynamicType parameters = default) : base(PurlUnSubscribeCommand_create(subscriber_id, topic, parameters?.GetNativeReference() ?? IntPtr.Zero))
            {

            }

            public Guid SubscriberId
            {
                get { Guid value = new Guid(); PurlUnSubscribeCommand_getSubscriberID(GetNativeReference(), ref value); return value; }
                set { PurlUnSubscribeCommand_setSubscriberID(GetNativeReference(), value); }
            }

            public string Topic
            {
                get { return Marshal.PtrToStringUni(PurlUnSubscribeCommand_getTopic(GetNativeReference())); }
                set { PurlUnSubscribeCommand_setTopic(GetNativeReference(), value); }
            }


            public DynamicType Parameters
            {
                get { return new DynamicType(PurlUnSubscribeCommand_getParameters(GetNativeReference())); }
                set { PurlUnSubscribeCommand_setParameters(GetNativeReference(), value.GetNativeReference()); }
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlUnSubscribeCommand_create(Guid id, string topic, IntPtr parameter_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlUnSubscribeCommand_getSubscriberID(IntPtr purlcommand_reference, ref Guid subscriber_id);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlUnSubscribeCommand_setSubscriberID(IntPtr purlcommand_reference, Guid subscriber_id);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlUnSubscribeCommand_getTopic(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlUnSubscribeCommand_setTopic(IntPtr purlcommand_reference, string topic);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PurlUnSubscribeCommand_getParameters(IntPtr purlcommand_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PurlUnSubscribeCommand_setParameters(IntPtr purlcommand_reference, IntPtr parameter_reference);


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
            public static bool RequestData(SerializeAdapter adapter,string topic,Guid guid,DynamicType parameters=default, byte [] payload=null)
            {
                if (adapter == null || !adapter.IsActive())
                    return false;

                PurlRequestCommand request = new PurlRequestCommand(guid, topic, parameters);

                if(payload!=null)
                {
                    Purl.SendCommand(adapter, PurlCommand.REQUEST_DATA, (uint)payload.Length, request);
                    Purl.SendPayload(adapter, payload);
                }
                else
                {
                    Purl.SendCommand(adapter, PurlCommand.REQUEST_DATA, 0, request);
                }

                return true;
            }

            public static bool AvailableData(SerializeAdapter adapter, string topic, Guid guid, double time=0, DynamicType parameters = default, byte[] payload = null)
            {
                if (adapter == null || !adapter.IsActive())
                    return false;

                PurlAvailCommand request = new PurlAvailCommand(guid, topic, time, parameters);

                if (payload != null)
                {
                    Purl.SendCommand(adapter, PurlCommand.AVAILABLE_DATA, (uint)payload.Length, request);
                    Purl.SendPayload(adapter, payload);
                }
                else
                {
                    Purl.SendCommand(adapter, PurlCommand.AVAILABLE_DATA, 0, request);
                }

                return true;
            }

            public static bool SubscribeTopic(SerializeAdapter adapter, string topic, Guid subscriber_id, DynamicType parameters = default, byte[] payload = null)
            {
                if (adapter == null || !adapter.IsActive())
                    return false;

                PurlSubscribeCommand request = new PurlSubscribeCommand(subscriber_id, topic, parameters);

                if (payload != null)
                {
                    Purl.SendCommand(adapter, PurlCommand.SUBSCRIBE, (uint)payload.Length, request);
                    Purl.SendPayload(adapter, payload);
                }
                else
                {
                    Purl.SendCommand(adapter, PurlCommand.SUBSCRIBE, 0, request);
                }

                return true;
            }

            public static bool UnSubscribeTopic(SerializeAdapter adapter, string topic, Guid subscriber_id, DynamicType parameters = default, byte[] payload = null)
            {
                if (adapter == null || !adapter.IsActive())
                    return false;

                PurlUnSubscribeCommand request = new PurlUnSubscribeCommand(subscriber_id, topic, parameters);

                if (payload != null)
                {
                    Purl.SendCommand(adapter, PurlCommand.UNSUBSCRIBE, (uint)payload.Length, request);
                    Purl.SendPayload(adapter, payload);
                }
                else
                {
                    Purl.SendCommand(adapter, PurlCommand.UNSUBSCRIBE, 0, request);
                }

                return true;
            }

            public static DynamicType ReceiveTopic(SerializeAdapter adapter, Guid guid, ref byte[] payload, double timeout=10)
            {
                if (adapter == null || !adapter.IsActive())
                    return new DynamicTypeError("Adapter is not active");

                Timer timer = new Timer();

                while ((timer.GetTime()<timeout) && !adapter.HasData())
                    System.Threading.Thread.Sleep(1);

                while ((timer.GetTime() < timeout) && adapter.IsActive() && adapter.HasData())
                {
                    PurlCommand command;

                    uint payload_length;

                    if (Purl.ReceiveCommand(adapter, out command, out payload_length))
                    {
                        if (command == PurlCommand.DATA)
                        {
                            PurlDataCommand data_command = new PurlDataCommand();

                            if (Purl.ReceivePayload(adapter, data_command))
                                payload_length -= data_command.GetDataSize();  // Remove size of purl payload

                            if (payload_length != 0)
                                Purl.ReceivePurlPayload(adapter, ref payload, payload_length);

                            if (data_command.Id == guid)
                                return data_command.Result;
                        }
                        else if (command == PurlCommand.AVAILABLE_DATA)
                        {
                            PurlAvailCommand avail_command = new PurlAvailCommand();

                            if (Purl.ReceivePayload(adapter, avail_command))
                                payload_length -= avail_command.GetDataSize();  // Remove size of purl payload

                            if (payload_length != 0)
                                Purl.ReceivePurlPayload(adapter, ref payload, payload_length);

                            if (avail_command.Id == guid)
                                return avail_command.Parameters;

                        }
                        else
                        {
                            Purl.ReceivePurlPayload(adapter, ref payload, payload_length);

                            return new DynamicTypeError("Unknown data");
                        }
                    }
                }

                return new DynamicTypeError("No matching data");
            }

            public static DynamicType QueryTopic(SerializeAdapter adapter, string topic, DynamicType parameters, Guid id , ref byte[] payload , double timeout = 10)
            {
                if (id.Equals(Guid.Empty))
                    id = Guid.NewGuid();

                if (!RequestData(adapter, topic, id, parameters, payload))
                    return new DynamicTypeError("Adapter is not active");

                return ReceiveTopic(adapter, id, ref payload, timeout);
            }

            public static DynamicType QueryTopic(SerializeAdapter adapter, string topic, DynamicType parameters, Guid id = default, double timeout = 10)
            {
                if (id.Equals(Guid.Empty))
                    id = Guid.NewGuid();

	            if (!RequestData(adapter, topic, id, parameters))
		            return new DynamicTypeError("Adapter is not active");

                byte[] payload=null;

	            return ReceiveTopic(adapter, id, ref payload, timeout);
            }
                        
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

