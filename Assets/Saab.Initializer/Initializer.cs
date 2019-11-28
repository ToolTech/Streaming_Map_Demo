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

namespace Saab.Unity.Initializer
{
    public class Initializer : MonoBehaviour
    {
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

        private void Awake()
        {
            GizmoSDK.GizmoBase.Platform.Initialize();
              
            // Initialize application ragistry
            KeyDatabase.SetDefaultRegistry($"/data/data/{Application.identifier}/files/gizmosdk.reg");

            Message.SetMessageLevel(MessageLevel.DEBUG);

            // Set local xml config
            KeyDatabase.SetLocalRegistry("asset:config.xml");

            #region -------- Test Related stuff in init --------------------

            // Message.OnMessage += Message_OnMessage;           // Right now strings do not work with IL2CPP

            //SetupJavaBindings();

            //test();

            #endregion

        }


        private void Message_OnMessage(string sender, MessageLevel level, string message)
        {
            if ((level & (MessageLevel.DEBUG | MessageLevel.MEM_DEBUG)) > 0)
            {
                Debug.Log(message);
            }
            else if ((level & (MessageLevel.NOTICE | MessageLevel.ALWAYS)) > 0)
            {
                Debug.Log(message);
            }
            else if ((level & MessageLevel.WARNING) > 0)
            {
                Debug.LogWarning(message);
            }
            else if ((level & MessageLevel.ASSERT) > 0)
            {
                Debug.LogAssertion(message);
            }
            else if ((level & MessageLevel.FATAL) > 0)
            {
                Debug.LogError(message);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
