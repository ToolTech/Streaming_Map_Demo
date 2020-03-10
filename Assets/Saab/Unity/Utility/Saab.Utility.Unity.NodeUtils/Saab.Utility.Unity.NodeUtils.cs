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
// Product		: Gizmo3D 2.10.1
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

// Unity Managed classes
using UnityEngine;


// System
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System;
using System.Collections;

namespace Saab.Utility.Unity.NodeUtils
{
   
    public class NodeUtils
    {
       
        public static bool HasGameObjects(IntPtr nativeReference)
        {
            GizmoSDK.Gizmo3D.NodeLock.WaitLockEdit();

            try
            {
                List<GameObject> gameObjectList;

                if (!FindGameObjects(nativeReference, out gameObjectList))
                    return false;

                if (gameObjectList.Count == 0)
                    return false;

                return true;
            }
            finally
            {
                GizmoSDK.Gizmo3D.NodeLock.UnLock();
            }
        }
        public static Transform FindFirstGameObjectTransform(IntPtr nativeReference)
        {
            GizmoSDK.Gizmo3D.NodeLock.WaitLockEdit();

            try
            {
                List<GameObject> gameObjectList;

                if (!FindGameObjects(nativeReference, out gameObjectList))
                    return null;

                if (gameObjectList.Count == 0)
                    return null;

                GameObject go = gameObjectList[0];

                return go.transform;
            }
            finally
            {
                GizmoSDK.Gizmo3D.NodeLock.UnLock();
            }
        }

        public static bool FindGameObjects(IntPtr nativeReference, out List<GameObject> gameObjectList)
        {
            GizmoSDK.Gizmo3D.NodeLock.WaitLockEdit();

            try
            {
                gameObjectList = null;

                if (nativeReference == IntPtr.Zero)
                    return false;

                return currentObjects.TryGetValue(nativeReference, out gameObjectList);
            }
            finally
            {
                GizmoSDK.Gizmo3D.NodeLock.UnLock();
            }
        }

        public static bool AddGameObjectReference(IntPtr nativeReference, GameObject gameObject)
        {
            GizmoSDK.Gizmo3D.NodeLock.WaitLockEdit();

            try
            {

                List<GameObject> gameObjectList;

                if (!currentObjects.TryGetValue(nativeReference, out gameObjectList))
                {
                    gameObjectList = new List<GameObject>();

                    currentObjects.Add(nativeReference, gameObjectList);
                }

                gameObjectList.Add(gameObject);

                return true;
            }
            finally
            {
                GizmoSDK.Gizmo3D.NodeLock.UnLock();
            }
        }

        static public bool RemoveGameObjectReference(IntPtr nativeReference, GameObject gameObject)
        {
            GizmoSDK.Gizmo3D.NodeLock.WaitLockEdit();

            try
            {

                List<GameObject> gameObjectList;

                if (currentObjects.TryGetValue(nativeReference, out gameObjectList))
                {
                    bool removed=gameObjectList.Remove(gameObject);

                    if(removed)
                    {
                        if(gameObjectList.Count==0) // We should remove list as no objects are registered
                        {
                            return currentObjects.Remove(nativeReference);
                        }
                    }

                    return removed;
                   
                }
                else
                    return false;
            }
            finally
            {
                GizmoSDK.Gizmo3D.NodeLock.UnLock();
            }

        }


        // The lookup dictinary to find a game object with s specfic native handle
        static private Dictionary<IntPtr, List<GameObject>> currentObjects = new Dictionary<IntPtr, List<GameObject>>();
    }
}