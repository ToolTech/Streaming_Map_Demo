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
// File			: SceneManager.cs
// Module		:
// Description	: Management of dynamic asset loader from GizmoSDK
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
        public static Transform FindFirstGameObjectTransform(IntPtr nativeReference)
        {
            ConcurrentBag<GameObject> gameObjectList;

            if (!FindGameObjects(nativeReference, out gameObjectList))
                return null;

            GameObject go;

            if (!gameObjectList.TryPeek(out go))
                return null;

            return go.transform;
        }

        public static bool FindGameObjects(IntPtr nativeReference, out ConcurrentBag<GameObject> gameObjectList)
        {
            gameObjectList = null;

            if (nativeReference == IntPtr.Zero)
                return false;

            return currentObjects.TryGetValue(nativeReference, out gameObjectList);
        }

        public static bool AddGameObjectReference(IntPtr nativeReference, GameObject gameObject)
        {
            ConcurrentBag<GameObject> gameObjectList;

            if (!currentObjects.TryGetValue(nativeReference, out gameObjectList))
            {
                gameObjectList = new ConcurrentBag<GameObject>();
                if (!currentObjects.TryAdd(nativeReference, gameObjectList))
                    return false;

            }

            gameObjectList.Add(gameObject);

            return true;
        }

        static public bool RemoveGameObjectReference(IntPtr nativeReference, GameObject gameObject)
        {
            ConcurrentBag<GameObject> gameObjectList;

            if (currentObjects.TryGetValue(nativeReference, out gameObjectList))
            {
                if (gameObjectList.Count == 0)
                {
                    return false;   // No game objects in list
                }
                else if (gameObjectList.Count == 1)      // Only one
                {
                    if (!gameObjectList.TryTake(out gameObject))
                        return false;
                }
                else    // Multiple GO in one list
                {
                    ConcurrentBag<GameObject> newBag = new ConcurrentBag<GameObject>();

                    GameObject o;                   // Multiple items in bag, remove only gameobject

                    while(gameObjectList.Count!=0)
                    {
                        if (!gameObjectList.TryTake(out o))
                            return false;

                        if (o != gameObject)
                            newBag.Add(o);
                    }

                    gameObjectList = newBag;
                }

                if (gameObjectList.Count == 0)
                    currentObjects.TryRemove(nativeReference, out gameObjectList);

                return true;
            }
            else
                return false;
        }


        // The lookup dictinary to find a game object with s specfic native handle
        static private ConcurrentDictionary<IntPtr, ConcurrentBag<GameObject>> currentObjects = new ConcurrentDictionary<IntPtr, ConcurrentBag<GameObject>>();
    }
}