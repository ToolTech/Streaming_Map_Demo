//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s) 
// herein belong to Saab AB. The program(s) may be used and/or
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
// File			: NodeUtils.cs
// Module		:
// Description	: Utility to control access between native handles and game objects
// Author		: Anders Modén
// Product		: Gizmo3D 2.10.6
//
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows (Win32) and IRIX® for  
//			usage in Game or VisSim development.
//
//
// Revision History...
//
// Who	Date	Description
//
// AMO	180607	Created file        (2.9.1)
//
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************


// Unity Managed classes
using UnityEngine;


// System
using System.Collections.Generic;
using System;

namespace Saab.Utility.Unity.NodeUtils
{
   
    public class NodeUtils
    {
       
        public static bool HasGameObjects(IntPtr nativeReference)
        {
            GizmoSDK.Gizmo3D.NodeLock.WaitLockEdit();

            try
            {
                return HasGameObjectsUnsafe(nativeReference);
            }
            finally
            {
                GizmoSDK.Gizmo3D.NodeLock.UnLock();
            }
        }

        public static bool HasGameObjectsUnsafe(IntPtr nativeReference)
        {
            if (!FindGameObjectsUnsafe(nativeReference, out List<GameObject> gameObjectList))
                return false;

            return gameObjectList.Count > 0;
        }


        public static Transform FindFirstGameObjectTransform(IntPtr nativeReference)
        {
            GizmoSDK.Gizmo3D.NodeLock.WaitLockEdit();

            try
            {
                return FindFirstGameObjectTransformUnsafe(nativeReference);
            }
            finally
            {
                GizmoSDK.Gizmo3D.NodeLock.UnLock();
            }
        }

        public static Transform FindFirstGameObjectTransformUnsafe(IntPtr nativeReference)
        {
            if (!FindGameObjectsUnsafe(nativeReference, out List<GameObject> gameObjectList))
                return null;

            if (gameObjectList.Count == 0)
                return null;

            return gameObjectList[0].transform;
        }

        public static bool FindGameObjects(IntPtr nativeReference, out List<GameObject> gameObjectList)
        {
            GizmoSDK.Gizmo3D.NodeLock.WaitLockEdit();

            try
            {
                return FindGameObjectsUnsafe(nativeReference, out gameObjectList);
            }
            finally
            {
                GizmoSDK.Gizmo3D.NodeLock.UnLock();
            }
        }

        public static bool FindGameObjectsUnsafe(IntPtr nativeReference, out List<GameObject> gameObjectList)
        {
            return currentObjects.TryGetValue(nativeReference.ToInt64(), out gameObjectList);
        }

        public static bool AddGameObjectReference(IntPtr nativeReference, GameObject gameObject)
        {
            GizmoSDK.Gizmo3D.NodeLock.WaitLockEdit();

            try
            {
                return AddGameObjectReferenceUnsafe(nativeReference, gameObject);
            }
            finally
            {
                GizmoSDK.Gizmo3D.NodeLock.UnLock();
            }
        }

        public static bool AddGameObjectReferenceUnsafe(IntPtr nativeReference, GameObject gameObject)
        {
            if (!currentObjects.TryGetValue(nativeReference.ToInt64(), out List<GameObject> gameObjectList))
            {
                gameObjectList = _gameObjectListPool.Count > 0 ? _gameObjectListPool.Pop() : new List<GameObject>(1);

                currentObjects.Add(nativeReference.ToInt64(), gameObjectList);
            }

            gameObjectList.Add(gameObject);

            return true;
        }

        static public bool RemoveGameObjectReference(IntPtr nativeReference, GameObject gameObject)
        {
            GizmoSDK.Gizmo3D.NodeLock.WaitLockEdit();

            try
            {
                return RemoveGameObjectReferenceUnsafe(nativeReference, gameObject);
            }
            finally
            {
                GizmoSDK.Gizmo3D.NodeLock.UnLock();
            }

        }

        public static bool RemoveGameObjectReferenceUnsafe(IntPtr nativeReference, GameObject gameObject)
        {
            if (currentObjects.TryGetValue(nativeReference.ToInt64(), out List<GameObject> gameObjectList))
            {
                var removed = gameObjectList.Remove(gameObject);

                if (gameObjectList.Count == 0) // We should remove list as no objects are registered
                {
                    _gameObjectListPool.Push(gameObjectList); // but we recycle it instead to avoid allocating new lists all the time
                    currentObjects.Remove(nativeReference.ToInt64());
                }

                return removed;
            }

            return false;
        }


        // The lookup dictinary to find a game object with s specfic native handle (we use long as key since IntPtr will cause boxing)
        static private Dictionary<long, List<GameObject>> currentObjects = new Dictionary<long, List<GameObject>>();

        // used to pool lists to avoid runtime allocations
        private static readonly Stack<List<GameObject>> _gameObjectListPool = new Stack<List<GameObject>>(1024);
    }
}
