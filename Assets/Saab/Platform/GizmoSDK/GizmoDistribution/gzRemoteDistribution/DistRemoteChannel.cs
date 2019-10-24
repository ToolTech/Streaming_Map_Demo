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
// File			: RemoteChannel.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistRemoteChannel class
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
//
//******************************************************************************

using System;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace GizmoDistribution
    {
        public enum DistTransportType
        {
            MULTICAST,
            BROADCAST,
            TCP
        }

        public interface IDistRemoteChannelInterface : IReferenceInterface
        {

        }

        public class DistRemoteChannel : Reference , IDistRemoteChannelInterface
        {
            public const string DEFAULT_IP_ADDRESS = "234.5.6.7";
            public const UInt16 DEFAULT_SERVER_PORT = 1122;
            public const UInt16 DEFAULT_SESSION_PORT = 2211;

            static public DistRemoteChannel CreateDefaultSessionChannel(bool reliable=true, DistTransportType transportType=DistTransportType.MULTICAST, string interfaceAddress=null)
            {
                return new DistRemoteChannel(DistCreateDefaultSessionChannel(reliable, transportType, interfaceAddress));
            }
            static public DistRemoteChannel CreateDefaultServerChannel(bool reliable = true, DistTransportType transportType = DistTransportType.MULTICAST, string interfaceAddress = null)
            {
                return new DistRemoteChannel(DistCreateDefaultServerChannel(reliable, transportType, interfaceAddress));
            }
            static public DistRemoteChannel CreateChannel(UInt32 reliableBufferSize = 5000, DistTransportType transportType = DistTransportType.MULTICAST, string address= DEFAULT_IP_ADDRESS, UInt16 port= DEFAULT_SESSION_PORT, string interfaceAddress = null)
            {
                return new DistRemoteChannel(DistCreateChannel(reliableBufferSize, transportType, address, port, interfaceAddress));
            }
            public DistRemoteChannel(IntPtr nativeReference) : base(nativeReference)
            {

            }


            [DllImport(Platform.GZ_DYLIB_REMOTE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistCreateDefaultSessionChannel(bool reliable, DistTransportType transportType, string interfaceAddress);
            [DllImport(Platform.GZ_DYLIB_REMOTE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistCreateDefaultServerChannel(bool reliable, DistTransportType transportType, string interfaceAddress);
            [DllImport(Platform.GZ_DYLIB_REMOTE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistCreateChannel(UInt32 reliableBufferSize, DistTransportType transportType, string address, UInt16 port, string interfaceAddress);
        }
    }
}
