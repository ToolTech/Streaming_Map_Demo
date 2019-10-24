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


