//******************************************************************************
// File			: Reference.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzReference class
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
using System.Collections.Generic;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public interface IReferenceFactory
        {
            Reference Create(IntPtr nativeReference);
        }

        public interface IReferenceInterface
        {
            IntPtr GetNativeReference();
            IntPtr GetNativeType();
            string GetNativeTypeName();

        }
        public class Reference : IReferenceInterface,IReferenceFactory, IDisposable
        {
            public Reference(IntPtr nativeReference)
            {
                m_reference = new HandleRef(this,nativeReference);

                Reference_ref(nativeReference);
            }

            public Reference(Reference copy)
            {
                m_reference = new HandleRef(this, copy.GetNativeReference());

                if(copy.GetNativeReference()!=IntPtr.Zero)
                    Reference_ref(copy.GetNativeReference());
            }

            ~Reference()
            {
                Release();
            }

            virtual public void Dispose()
            {
                Release(); 

                // Prevent the object from being placed on the
                // finalization queue
                GC.SuppressFinalize(this);
            }

            public string GetNativeTypeName()
            {
                if (m_reference.Handle != IntPtr.Zero)
                    return Marshal.PtrToStringUni(Reference_getReferenceTypeName(m_reference.Handle));
                else return "---";
            }

            public IntPtr GetNativeReference()
            {
                return m_reference.Handle;
            }
            public virtual Reference Clone()
            {
                return CreateObject(Reference_clone(m_reference.Handle));
            }

            public  virtual Reference Create(IntPtr nativeReference)
            {
                return new Reference(nativeReference);
            }

            public bool IsValid()
            {
                return GetNativeReference() != IntPtr.Zero;
            }

            public IntPtr GetNativeType()
            {
                if (m_reference.Handle != IntPtr.Zero)
                    return Reference_getType(m_reference.Handle);
                else
                    return IntPtr.Zero;
            }

            public bool IsOfType(IntPtr native_type)
            {
                return Reference_isOfType(m_reference.Handle, native_type);
            }

            public void UnRefNoDelete()
            {
                if (m_reference.Handle != IntPtr.Zero)
                    Reference_unrefNoDelete(m_reference.Handle);

                // Don't permit the handle to be used again.
                m_reference = new HandleRef(this, IntPtr.Zero);
            }

            #region ----------------- privates --------------------

            private void Release()
            {
                if(m_reference.Handle != IntPtr.Zero)
                    Reference_unref(m_reference.Handle);

                // Don't permit the handle to be used again.
                m_reference = new HandleRef(this, IntPtr.Zero);
            }

            
            // -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Reference_ref(IntPtr ptr);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Reference_unref(IntPtr ptr);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Reference_unrefNoDelete(IntPtr ptr);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Reference_getReferenceTypeName(IntPtr ptr);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Reference_Test();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Reference_clone(IntPtr ptr);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Reference_getType(IntPtr ptr);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Reference_isOfType(IntPtr ptr, IntPtr type);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Reference_getParentType(IntPtr ptr);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Reference_getTypeName(IntPtr ptr);

            static public bool AddFactory(Reference iface)
            {
                if (iface == null)
                    return false;

                if (!iface.IsValid())
                    return false;

                string typeName = iface.GetNativeTypeName();

                lock (s_factory)
                {
                    if (s_factory.ContainsKey(typeName))
                        return false;

                    s_factory.Add(typeName, iface);

                    return true;
                }
            }

            static public bool RemoveFactory(string typeName)
            {
                lock (s_factory)
                {
                    if (!s_factory.ContainsKey(typeName))
                        return false;

                    s_factory.Remove(typeName);

                    return true;
                }
            }

            static public Reference CreateObject(IntPtr nativeReference)
            {
                if (nativeReference==IntPtr.Zero)
                    return null;

                lock (s_factory)
                {
                    IntPtr type = Reference_getType(nativeReference);

                    IReferenceFactory factory;

                    while (type != IntPtr.Zero)
                    {
                        string typeName = Marshal.PtrToStringUni(Reference_getTypeName(type));

                        if (s_factory.TryGetValue(typeName, out factory))
                            return factory.Create(nativeReference);

                        type = Reference_getParentType(type);
                    }
                    return new Reference(nativeReference);
                }
            }

            private HandleRef m_reference;

            private static Dictionary<string, IReferenceFactory> s_factory = new Dictionary<string, IReferenceFactory>();

            #endregion
        }
    }
}

