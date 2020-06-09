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
// File			: Reference.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzReference class
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
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public interface IReferenceFactory
        {
            Reference Create(IntPtr nativeReference);
        }

        public interface IReference
        {
            IntPtr GetNativeReference();
            IntPtr GetNativeType();
            string GetNativeTypeName();
            void Release();
            UInt32 GetReferenceCount();
        }

        public class Reference : IReference,IReferenceFactory, IDisposable
        {
            public Reference(IntPtr nativeReference)
            {
                Reference_ref(nativeReference);

                m_reference = new HandleRef(this,nativeReference);
            }

            public Reference(Reference copy)
            {
                if (copy.GetNativeReference() != IntPtr.Zero)
                    Reference_ref(copy.GetNativeReference());

                m_reference = new HandleRef(this, copy.GetNativeReference());
            }

            public void Reset(IntPtr nativeReference)
            {
                if(nativeReference!= IntPtr.Zero)
                    Reference_ref(nativeReference);

                IntPtr oldRef = m_reference.Handle;

                m_reference = new HandleRef(this, nativeReference);

                if (oldRef != IntPtr.Zero)
                    Reference_unref(oldRef);
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
                if (GetNativeReference() == IntPtr.Zero)
                    throw new InvalidOperationException();
                
                return Marshal.PtrToStringUni(Reference_getReferenceTypeName(m_reference.Handle));
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

            virtual public void ReleaseNoDelete()
            {
                IntPtr oldRef = m_reference.Handle;

                // Don't permit the handle to be used again.
                m_reference = new HandleRef(this, IntPtr.Zero);

                if (oldRef != IntPtr.Zero)
                    Reference_unrefNoDelete(oldRef);
            }

            
            virtual public void Release()
            {
                IntPtr oldRef = m_reference.Handle;

                // Don't permit the handle to be used again.
                m_reference = new HandleRef(this, IntPtr.Zero);

                if (oldRef != IntPtr.Zero)
                    Reference_unref(oldRef);
            }

            public uint GetReferenceCount()
            {
                return Reference_getRef(m_reference.Handle);
            }


            static public bool AddFactory(Reference nativeRef)
            {
                if (nativeRef == null)
                    throw new ArgumentNullException(nameof(nativeRef));

                if (!nativeRef.IsValid())
                    throw new ArgumentException("native reference was not valid", nameof(nativeRef));

                return s_factory.TryAdd(nativeRef.GetNativeTypeName(), nativeRef);
            }

            static public bool AddFactory(Reference nativeRef, string nativeTypename)
            {
                if (nativeRef == null)
                    throw new ArgumentNullException(nameof(nativeRef));

                if (!nativeRef.IsValid())
                    throw new ArgumentException("native reference was not valid", nameof(nativeRef));

                return s_factory.TryAdd(nativeTypename, nativeRef);
            }

            static public bool RemoveFactory<T>() where T : Reference
            {
                return RemoveFactory(typeof(T).Name);
            }
            static public bool RemoveFactory(string typeName)
            {
                IReferenceFactory factory;

                return s_factory.TryRemove(typeName, out factory);

            }

            static public Reference CreateObject(IntPtr nativeReference)
            {
                if (nativeReference == IntPtr.Zero)
                    return null;

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

            #region ----------------- privates --------------------

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
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 Reference_getRef(IntPtr ptr);

            // ------------- reference -------------

            private HandleRef m_reference;

            private static ConcurrentDictionary<string, IReferenceFactory> s_factory = new ConcurrentDictionary<string, IReferenceFactory>();

            #endregion
        }

        public static class ReferenceDictionary<T> where T : class,IReference
        {
            static public bool AddObject(IReference nativeRef)
            {
                if (nativeRef == null)
                    return false;

                if (nativeRef.GetNativeReference() == IntPtr.Zero)
                    return false;

                return _instances.TryAdd(nativeRef.GetNativeReference(), nativeRef);
            }

            static public bool RemoveObject(IReference nativeRef)
            {
                if (nativeRef == null)
                    return false;

                return RemoveObject(nativeRef.GetNativeReference());
            }

            static public bool RemoveObject(IntPtr nativeRef)
            {
                if (nativeRef == IntPtr.Zero)
                    return false;

                IReference obj;

                return _instances.TryRemove(nativeRef, out obj);
            }

            static public T GetObject(IntPtr nativeReference)
            {
                // We must allow GetObject for null reference
                if (nativeReference == IntPtr.Zero)
                    return null;

                IReference obj;

                if (!_instances.TryGetValue(nativeReference, out obj))
                {
                    obj = Reference.CreateObject(nativeReference) as IReference;

                    if (obj is T)    // if we have a valid factory created we will register in here
                    {
                        //If we have an auto register in constructor we will hit the false condition
                        if (!_instances.TryAdd(nativeReference, obj))
                        {
                            bool ok = _instances.TryGetValue(nativeReference, out obj);
                            System.Diagnostics.Debug.Assert(ok);
                        }
                    }
                }

                return obj as T;
            }

            static public void Clear()
            {
                foreach (var key in _instances)
                {
                    key.Value.Release();
                }

                _instances.Clear();
            }

            private static ConcurrentDictionary<IntPtr, IReference> _instances = new ConcurrentDictionary<IntPtr, IReference>();
        }
    }
}

