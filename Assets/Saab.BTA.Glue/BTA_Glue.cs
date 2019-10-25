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
// File			: BTA_Glue.cs
// Module		: 
// Description	: Glue for BTA architecture
// Author		: Anders Modén		
//		
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	180301	Created file 	
//
//******************************************************************************

using GizmoSDK.GizmoBase;
using System;
using System.Collections;
using UnityEngine;

namespace Saab.Core
{
    public interface IWorldCoord
    {
        Vec3D Coordinate { get; }
    }

    public class BtaApplication
    {
        public static string GetConfigValue(string key, string defaultValue)
        {
            return defaultValue;
        }

        public static int GetConfigValue(string key, int defaultValue)
        {
            return defaultValue;
        }
    }
}



namespace Saab.Unity.Core
{
    
    public abstract class BtaComponent : MonoBehaviour
    {
        protected bool Initialized = false;

        protected virtual bool CheckDependencies()
        {
            return true;
        }

        private void Success_callback(bool ok)
        {
            if(ok)
            {
                Initialized = true;
            }
        }

        void Start()
        {
            if(!Initialized)
                StartCoroutine(InitComponent(Success_callback));
        }

              
        
        // ----------------- Mono Bahaviours ---------------------

        private void Awake()
        {
            OnAwake();
        }

        private void Update()
        {
            OnUpdate();
        }

        protected abstract void OnAwake();
        protected abstract void OnUpdate();
        protected abstract IEnumerator InitComponent(Action<bool> success);
    }
}


