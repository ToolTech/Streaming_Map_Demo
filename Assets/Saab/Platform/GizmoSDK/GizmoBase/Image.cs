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
// File			: Image.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzImage class
// Author		: Anders Modén		
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

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public enum ImageType
        {
            /*!An image with RGB format. It has three components: Red, Green and Blue.
            */
            RGB_8,
            /*!An image with RGBA format. It has four components: Red, Green, Blue and Alpha.
            */
            RGBA_8,
            BW_8,
            BW_HALF,
            BWA_8,
            BWA_HALF,
            DEPTHMAP,
            BITMAP,
            BGRA_5_5_5_1,
            BGR_5_6_5,
            RGBA_FLOAT,   // 32 bits
            RGB_FLOAT,
            RGBA_HALF,    // 16 bits
            RGB_HALF,
            RGBA_FPX, // Special FPX Gizmo Extension packed bits r7g7b7 r6g6b6 etc..

            RGB_8_DXT1,
            RGBA_8_DXT1,
            RGBA_8_DXT3,
            RGBA_8_DXT5,

            BW_16,
            BW_FLOAT,
            RGB_16,
            RGBA_16,
            ABGR_16,

            BGR_8,
            ABGR_8,

            CUSTOM
        };
        public enum ImageFormat
        {
            RGB                         = Enums.GZ_RGB,
            BGR                         = Enums.GZ_BGR,
            ABGR                        = Enums.GZ_BGRA,
            RGBA                        = Enums.GZ_RGBA,
            LUMINANCE                   = Enums.GZ_LUMINANCE,
            LUMINANCE_ALPHA             = Enums.GZ_LUMINANCE_ALPHA,
            COLOR_INDEX                 = Enums.GZ_COLOR_INDEX,

            // Predefined Compressed variants
            COMPRESSED_RGB_S3TC_DXT1    = Enums.GZ_COMPRESSED_RGB_S3TC_DXT1,
            COMPRESSED_RGBA_S3TC_DXT1   = Enums.GZ_COMPRESSED_RGBA_S3TC_DXT1,
            COMPRESSED_RGBA_S3TC_DXT3   = Enums.GZ_COMPRESSED_RGBA_S3TC_DXT3,
            COMPRESSED_RGBA_S3TC_DXT5   = Enums.GZ_COMPRESSED_RGBA_S3TC_DXT5,

            // Special depth image format
            DEPTH_COMPONENT             = Enums.GZ_DEPTH_COMPONENT,

            // Predefined ETC2/EAC Compressed variants
            COMPRESSED_RGB8_ETC2        = Enums.GZ_COMPRESSED_RGB8_ETC2,
            COMPRESSED_RGB8_A1_ETC2     = Enums.GZ_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2,
            COMPRESSED_RGBA8_ETC2       = Enums.GZ_COMPRESSED_RGBA8_ETC2_EAC,
            COMPRESSED_R11_EAC          = Enums.GZ_COMPRESSED_R11_EAC,
            COMPRESSED_RG11_EAC         = Enums.GZ_COMPRESSED_RG11_EAC,
        };

        public enum ComponentType
        {
            BYTE                        = Enums.GZ_BYTE,
            UNSIGNED_BYTE               = Enums.GZ_UNSIGNED_BYTE,
            SHORT                       = Enums.GZ_SHORT,
            UNSIGNED_SHORT              = Enums.GZ_UNSIGNED_SHORT,
            UNSIGNED_SHORT_1_5_5_5_REV  = Enums.GZ_UNSIGNED_SHORT_1_5_5_5_REV,
            UNSIGNED_SHORT_4_4_4_4_REV  = Enums.GZ_UNSIGNED_SHORT_4_4_4_4_REV,
            UNSIGNED_SHORT_5_6_5_REV    = Enums.GZ_UNSIGNED_SHORT_5_6_5_REV,
            INT                         = Enums.GZ_INT,
            UNSIGNED_INT                = Enums.GZ_UNSIGNED_INT,
            FLOAT                       = Enums.GZ_FLOAT,
            HALF_FLOAT                  = Enums.GZ_HALF_FLOAT,
            DOUBLE                      = Enums.GZ_DOUBLE,
            BITMAP                      = Enums.GZ_BITMAP,
        };

        public class Image : Object , INameInterface
        {
            protected Image(IntPtr nativeReference) : base(nativeReference) { }

            public Image(string name) : base(Image_create(name)) { }

            static new public void InitializeFactory()
            {
                AddFactory(new Image("Factory"));
            }

            public static void UninitializeFactory()
            {
                RemoveFactory("gzImage");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Image(nativeReference) as Reference;
            }

            public string GetName()
            {
                return Marshal.PtrToStringUni(Image_getName(GetNativeReference()));
            }

            public void SetName(string name)
            {
                Image_setName(GetNativeReference(), name);
            }

            public ImageFormat GetFormat()
            {
                return Image_getFormat(GetNativeReference());
            }

            public ImageType GetImageType()
            {
                return Image_getImageType(GetNativeReference());
            }

            public UInt32 GetWidth()
            {
                return Image_getWidth(GetNativeReference());
            }

            public UInt32 GetHeight()
            {
                return Image_getHeight(GetNativeReference());
            }

            public UInt32 GetDepth()
            {
                return Image_getDepth(GetNativeReference());
            }

            public bool GetImageArray(ref byte[] image_data)
            {
                UInt32 size = 0;

                IntPtr native_image_data = IntPtr.Zero;

                if (Image_getImageArray(GetNativeReference(), ref size, ref native_image_data))
                {
                    // Check alloc memory return
                    if (image_data == null || image_data.Length != size)
                        image_data = new byte[size];
                    
                    Marshal.Copy(native_image_data, image_data, (int)0, (int)size);

                    return true;
                }

                return false;
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Image_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Image_setName(IntPtr image_reference, string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Image_getName(IntPtr image_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern ImageFormat Image_getFormat(IntPtr image_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern ImageType Image_getImageType(IntPtr image_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 Image_getWidth(IntPtr image_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 Image_getHeight(IntPtr image_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 Image_getDepth(IntPtr image_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Image_getImageArray(IntPtr image_reference, ref UInt32 size, ref IntPtr native_image_data);


            #endregion
        }
    }
}

