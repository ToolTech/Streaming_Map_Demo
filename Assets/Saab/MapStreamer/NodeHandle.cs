//*****************************************************************************
// File			: NodeHandle.cs
// Module		:
// Description	: Handle to native Gizmo3D nodes
// Author		: Anders Modén
// Product		: Gizmo3D 2.9.1
//
// Copyright © 2003- Saab Training Systems AB, Sweden
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
// AMO	180607	Created file                                                        (2.9.1)
//
//******************************************************************************

// Unity Managed classes
using UnityEngine;

// Gizmo Managed classes
using GizmoSDK.GizmoBase;
using GizmoSDK.Gizmo3D;

// Fix some conflicts between unity and Gizmo namespaces
using gzTransform = GizmoSDK.Gizmo3D.Transform;

namespace Saab.Unity.MapStreamer
{
    // The NodeHandle component of a game object stores a Node reference to the corresponding Gizmo item on the native side
    public class NodeHandle : MonoBehaviour
    {

        // Handle to native gizmo node
        internal Node node;

        // True if we have added this object as a lookup table object
        internal bool inObjectDict = false;

        internal bool updateTransform = false;

        // We need to release all existing objects in a locked mode
        void OnDestroy()
        {

            // Basically all nodes in the GameObject scene should already be release by callbacks but there might be some nodes left that needs this behaviour
            if (node != null)
            {
                if (node.IsValid())
                {
                    NodeLock.WaitLockEdit();
                    node.Dispose();
                    NodeLock.UnLock();
                }
            }
        }

        void Update()
        {
            if (updateTransform)
            {
                gzTransform tr = node as gzTransform;

                if (tr != null)
                {
                    Vec3 translation;

                    if (tr.GetTranslation(out translation))
                    {
                        Vector3 trans = new Vector3(translation.x, translation.y, translation.z);
                        transform.localPosition = trans;
                    }
                }
            }
        }
    }

}