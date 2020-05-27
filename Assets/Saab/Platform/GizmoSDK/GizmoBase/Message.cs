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
// File			: Message.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzMessage class
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
// AMO	180301	Created file 	
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;
using System.Xml.Serialization;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        [Flags]
        public enum MessageLevel
        {
            MEM_DEBUG = 0x1000,
            PERF_DEBUG = 0x1001,
            DETAIL_DEBUG = 0x1002,
            DEBUG = 0x2000,
            TRACE_DEBUG = 0x2001,
            NOTICE = 0x3000,
            WARNING = 0x4000,
            FATAL = 0x5000,
            ASSERT = 0x6000,
            ALWAYS = 0x7000,

            LEVEL_MASK = 0xf07f,
            LEVEL_MASK_STD = 0xf000,

            INTERNAL =(1<<11),
        }

        [XmlRoot]
        public class ExceptionMessage
        {
            [XmlElement]
            public string Message { get; set; }
            [XmlElement]
            public string Source { get; set; }
            [XmlElement]
            public string Stacktrace { get; set; }
            [XmlElement]
            public string Type { get; set; }
        }

                      

        public class Message
        {
            public const string GIZMOSDK = "GizmoSDK";

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public delegate void EventHandler_OnMessage(string sender ,MessageLevel level, string message);

            static public event EventHandler_OnMessage OnMessage;

            static public void Send(string sender, MessageLevel level, string message)
            {
                Message_message(sender,level, message);
            }

            static public void SendException(string sender, Exception ex)
            {
                var exceptionMessage = new ExceptionMessage()
                {
                    Message = ex.Message,
                    Source = ex.Source,
                    Stacktrace = ex.StackTrace,
                    Type = ex.GetType().FullName,
                };

                var xmlSerializer = new XmlSerializer(typeof(ExceptionMessage));

                var xml = new System.Text.StringBuilder();
                using (var xmlWriter = System.Xml.XmlWriter.Create(xml))
                    xmlSerializer.Serialize(xmlWriter, exceptionMessage);

                Message_message(sender, MessageLevel.ASSERT, xml.ToString());
            }

            static public void SetMessageLevel(MessageLevel level)
            {
                Message_setMessageLevel(level);
            }

            static public void Initialize()
            {
                if (s_class_init == null)
                    s_class_init = new Initializer();
            }

            static public void Uninitialize()
            {
                if (s_class_init != null)
                    s_class_init = null;
            }

            #region ---------------- Private functions ------------------------


            private sealed class Initializer
            {
                public Initializer()
                {
                    if (s_dispatcher == null)
                    {
                        s_dispatcher = new EventHandler_OnMessage(MessageHandler);
                        Message_SetCallback(s_dispatcher);
                    }
                }

                ~Initializer()
                {
                    if (s_dispatcher != null)
                    {
                        Message_SetCallback(null);
                        s_dispatcher = null;
                    }
                }
            }
            
            static private Initializer s_class_init =new Initializer();

            static private EventHandler_OnMessage s_dispatcher;

            [MonoPInvokeCallback(typeof(EventHandler_OnMessage))]
            private static void MessageHandler(string sender, MessageLevel level, string message)
            {
                OnMessage?.Invoke(sender, level, message);
            }

            #endregion

            #region // --------------------- Native calls -----------------------
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode,CallingConvention =CallingConvention.Cdecl)]
            private static extern void Message_SetCallback(EventHandler_OnMessage fn);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode,CallingConvention = CallingConvention.Cdecl)]
            private static extern void Message_message(string sender, MessageLevel level, string message);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Message_setMessageLevel(MessageLevel level);
            #endregion
        }






    }
}

