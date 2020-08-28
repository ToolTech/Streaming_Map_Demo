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
// File			: Texture.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzTexture class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.6
//		
//
//			
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows, Android, iOS and HoloLens for  
//			usage in Game or VisSim development.
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace Gizmo3D
    {
        public class Texture : Reference
        {
            public enum TextureMagFilter
            {
                ///	The texel with coordinates nearest the center of the pixel is used for magnification.
                NEAREST = Enums.GZ_NEAREST,

                ///	A weighted average of the texels nearest to the center of the pixel is used for magnification.
                LINEAR = Enums.GZ_LINEAR,
            };

            public enum TextureMinFilter
            {
                ///	The texel with coordinates nearest the center of the pixel is used for minification.
                NEAREST = Enums.GZ_NEAREST,

                ///	A weighted average of the texels nearest to the center of the pixel is used for minification.
                LINEAR = Enums.GZ_LINEAR,

                ///  The nearest pixel in an individual mipmap is used.
                NEAREST_MIPMAP_NEAREST = Enums.GZ_NEAREST_MIPMAP_NEAREST,

                ///	The values of nearest texel in each of the two mipmaps are selected and linear interpolated.
                NEAREST_MIPMAP_LINEAR = Enums.GZ_NEAREST_MIPMAP_LINEAR,

                ///	Linear interpolation is used within an individual mipmap.
                LINEAR_MIPMAP_NEAREST = Enums.GZ_LINEAR_MIPMAP_NEAREST,

                ///	Linear interpolates the value in each of two maps and then linear interpolates these two resulting values.
                LINEAR_MIPMAP_LINEAR = Enums.GZ_LINEAR_MIPMAP_LINEAR,
            };



            public Texture(IntPtr nativeReference) : base(nativeReference) { }

            public Texture() : base(Texture_create()) { }

            static public void InitializeFactory()
            {
                AddFactory(new Texture());
            }

            public static void UninitializeFactory()
            {
                RemoveFactory("gzTexture");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Texture(nativeReference) as Reference;
            }

            override public void Release()
            {
                if (IsValid())
                {
                    try
                    {
                        NodeLock.WaitLockEdit();

                        base.Release();
                    }
                    finally
                    {

                        NodeLock.UnLock();
                    }
                }
            }

            virtual public void ReleaseInRender()
            {
                if (IsValid())
                {
                    try
                    {
                        NodeLock.WaitLockRender();

                        base.Release();
                    }
                    finally
                    {
                        NodeLock.UnLock();
                    }
                }
            }

            public bool HasImage()
            {
                return Texture_hasImage(GetNativeReference());
            }

            public Image GetImage()
            {
                return CreateObject(Texture_getImage(GetNativeReference())) as Image;
            }

            public TextureMagFilter MagFilter
            {
                get
                {
                    return Texture_getMagFilter(GetNativeReference());
                }
                set
                {
                    Texture_setMagFilter(GetNativeReference(), value);
                }
            }

            public TextureMinFilter MinFilter
            {
                get
                {
                    return Texture_getMinFilter(GetNativeReference());
                }
                set
                {
                    Texture_setMinFilter(GetNativeReference(), value);
                }
            }

            public bool UseMipMaps
            {
                get
                {
                    return Texture_getUseMipMaps(GetNativeReference());
                }
                set
                {
                    Texture_setUseMipMaps(GetNativeReference(), value);
                }
            }

            public bool GetMipMapImageArray(ref byte[] image_data,out UInt32 size,out ImageFormat format,out ComponentType componentType, out UInt32 components,out UInt32 width,out UInt32 height,out UInt32 depth, bool useMipMaps, bool uncompress)
            {
                size = 0;

                width = 0;
                height = 0;
                depth = 0;

                IntPtr native_image_data = IntPtr.Zero;

                format = ImageFormat.RGBA;
                componentType = ComponentType.BYTE;
                components = 4;

                if (Texture_getMipMapImageArray(GetNativeReference(), useMipMaps, uncompress,ref size, ref native_image_data,ref format,ref componentType, ref components,ref width, ref height, ref depth))
                {
                    // Check alloc memory return
                    if(image_data == null || image_data.Length < size)
                        image_data = new byte[size];

                    Marshal.Copy(native_image_data, image_data, (int)0, (int)size);

                    return true;
                }

                return false;
            }

            public bool GetMipMapImageArray(ref IntPtr native_image_data, out UInt32 size, out ImageFormat format, out ComponentType componentType, out UInt32 components, out UInt32 width, out UInt32 height, out UInt32 depth, bool useMipMaps, bool uncompress)
            {
                size = 0;

                width = 0;
                height = 0;
                depth = 0;

                format = ImageFormat.RGBA;
                componentType = ComponentType.BYTE;
                components = 4;

                return Texture_getMipMapImageArray(GetNativeReference(), useMipMaps, uncompress, ref size, ref native_image_data, ref format, ref componentType, ref components, ref width, ref height, ref depth);
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Texture_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Texture_hasImage(IntPtr texture_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Texture_getImage(IntPtr texture_reference);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern TextureMagFilter Texture_getMagFilter(IntPtr texture_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Texture_setMagFilter(IntPtr texture_reference,TextureMagFilter filter);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern TextureMinFilter Texture_getMinFilter(IntPtr texture_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Texture_setMinFilter(IntPtr texture_reference, TextureMinFilter filter);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Texture_getUseMipMaps(IntPtr texture_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Texture_setUseMipMaps(IntPtr texture_reference, bool on);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Texture_getMipMapImageArray(IntPtr texture_reference, bool createMipMaps, bool uncompress, ref UInt32 size, ref IntPtr native_image_data,ref ImageFormat format,ref ComponentType compType, ref UInt32 components, ref UInt32 width,ref UInt32 height,ref UInt32 depth);

            #endregion
        }
    }
}
