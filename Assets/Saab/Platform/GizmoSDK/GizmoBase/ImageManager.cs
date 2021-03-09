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
// File			: ImageManager.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzImageManager class
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
// AMO	201210	Created file 	                                (2.10.6)
//
//******************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace GizmoBase
    {
        public class ImageManager
        {

            [Flags]
            public enum AdapterFlags : UInt64
            {
                DEFAULT = 0,

                FLIP_FLIPPED_IMAGES      = 1 << (0 + (int)SerializeAdapter.AdapterFlags.FLAG_MAX_SIZE),
                NO_CACHED_IMAGE             = 1 << (1 + (int)SerializeAdapter.AdapterFlags.FLAG_MAX_SIZE),
                NO_ALTERNATE_IMAGE_EXT      = 1 << (2 + (int)SerializeAdapter.AdapterFlags.FLAG_MAX_SIZE),
                IGNORE_IMAGE_MIPMAPS        = 1 << (3 + (int)SerializeAdapter.AdapterFlags.FLAG_MAX_SIZE),
                NO_DXT1_ALPHA               = 1 << (4 + (int)SerializeAdapter.AdapterFlags.FLAG_MAX_SIZE),
                NO_MISSING_REF_IMAGE_WARN   = 1 << (5 + (int)SerializeAdapter.AdapterFlags.FLAG_MAX_SIZE),

                FLAG_MAX_SIZE = 6,
            }


            static public Image LoadImage(string url, string extension="", AdapterFlags flags=AdapterFlags.DEFAULT, UInt32 version=0, string password="", Reference associatedData=null)
            {
                SerializeAdapter.AdapterError error = SerializeAdapter.AdapterError.NO_ERROR;
                IntPtr nativeErrorString = IntPtr.Zero;

                return Reference.CreateObject(ImageManager_loadImage(url,extension,ref flags,version, password, associatedData?.GetNativeReference() ?? IntPtr.Zero,ref nativeErrorString,ref error)) as Image;
            }

            static public Image LoadImage(string url, ref string errorString, ref SerializeAdapter.AdapterError error,string extension = "", AdapterFlags flags = AdapterFlags.DEFAULT, UInt32 version = 0, string password = "", Reference associatedData = null)
            {
                IntPtr nativeErrorString = IntPtr.Zero;

                IntPtr node=ImageManager_loadImage(url, extension, ref flags, version, password, associatedData?.GetNativeReference() ?? IntPtr.Zero, ref nativeErrorString, ref error);

                if (nativeErrorString != IntPtr.Zero)
                    errorString = Marshal.PtrToStringUni(nativeErrorString);

                return Reference.CreateObject(node) as Image;
            }

            static public bool Initialize()
            {
                return ImageManager_initialize();
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr ImageManager_loadImage(string url, string extension , ref AdapterFlags flags, UInt32 version ,  string password , IntPtr associatedData, ref IntPtr nativeErrorString, ref SerializeAdapter.AdapterError error);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool ImageManager_initialize();
        }
    }
}
