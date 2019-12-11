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
// File			: Initializer.cs
// Module		:
// Description	: Bindings stub for Gizmo Messages
// Author		: Anders Modén
// Product		: Gizmo3D 2.9.1
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
using GizmoSDK.GizmoBase;
using GizmoSDK.GizmoDistribution;
using UnityEngine;
using System.Threading;

namespace Saab.Unity.Initializer
{
    public class Initializer : MonoBehaviour
    {
        DebugCommandStation station;

        //void test()
        //{

        //    if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.INTERNET"))
        //        UnityEngine.Android.Permission.RequestUserPermission("android.permission.INTERNET");

        //    if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.CHANGE_NETWORK_STATE"))
        //        UnityEngine.Android.Permission.RequestUserPermission("android.permission.CHANGE_NETWORK_STATE");

        //    if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.CHANGE_WIFI_MULTICAST_STATE"))
        //        UnityEngine.Android.Permission.RequestUserPermission("android.permission.CHANGE_WIFI_MULTICAST_STATE");



        //    //GizmoSDK.GizmoDistribution.Platform.Initialize();

        //    //DistManager manager = DistManager.GetManager(true);

        //    //manager.Start(DistRemoteChannel.CreateDefaultSessionChannel(), DistRemoteChannel.CreateDefaultServerChannel());

        //    //DistClient client = new DistClient("putte", manager);



        //}
        // Start is called before the first frame update
        void SetupJavaBindings()
        {
#if UNITY_ANDROID

            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

            if (playerClass == null)
                return;

            AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");

            if (activity == null)
                return;

            AndroidJavaObject assetManager = activity.Call<AndroidJavaObject>("getAssets");

            if (assetManager == null)
                return;

            Message.Send("GizmoSDK", MessageLevel.DEBUG, $"assetManager {assetManager}");

            SerializeAdapter.SetAssetManagerHandle(System.IntPtr.Zero, assetManager.GetRawObject());
#endif
        }

        void EnableMulticastState()
        {
#if UNITY_ANDROID

            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");

            var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi");
                           
            var multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "lock");

            multicastLock.Call("setReferenceCounted", true);

            multicastLock.Call("acquire");

            Message.Send(Message.GIZMOSDK, MessageLevel.DEBUG, "MultiCast Lock acquired");
#endif
        }

        private void Awake()
        {
            GizmoSDK.GizmoBase.Platform.Initialize();
              
            Message.OnMessage += Message_OnMessage;           // Right now strings do not work with IL2CPP

            Message.SetMessageLevel(MessageLevel.DEBUG);

            // Initialize application ragistry
            KeyDatabase.SetDefaultRegistry($"/data/data/{Application.identifier}/files/gizmosdk.reg");


            // Set local xml config
            KeyDatabase.SetLocalRegistry("asset:config.xml");

            EnableMulticastState();

            station = new DebugCommandStation("udp::45456?blocking=no&nic=${wlan0}");

            station.OnExec += Station_OnExec;

            Thread thread = new Thread(new ThreadStart(WorkThreadFunction));
            thread.Start();

            #region -------- Test Related stuff in init --------------------


            //SetupJavaBindings();

            //test();

            #endregion

        }

        public void WorkThreadFunction()
        {
            while (station != null && station.Exec())
                Thread.Sleep(100);
        }


        private bool Station_OnExec(string exec_message)
        {
            Message.Send(Message.GIZMOSDK, MessageLevel.DEBUG, $"Exec from station {exec_message}");

            return true;
        }

        private void Message_OnMessage(string sender, MessageLevel level, string message)
        {

            switch (level & MessageLevel.LEVEL_MASK)
            {
                case MessageLevel.MEM_DEBUG:
                case MessageLevel.PERF_DEBUG:
                case MessageLevel.DEBUG:
                case MessageLevel.TRACE_DEBUG:
                    Debug.Log(message);
                    break;

                case MessageLevel.NOTICE:
                    Debug.Log(message);
                    break;

                case MessageLevel.WARNING:
                    Debug.LogWarning(message);
                    break;

                case MessageLevel.FATAL:
                    Debug.LogError(message);
                    break;

                case MessageLevel.ASSERT:
                    Debug.LogAssertion(message);
                    break;

                case MessageLevel.ALWAYS:
                    Debug.Log(message);
                    break;
            }
        }

     
    }

}
