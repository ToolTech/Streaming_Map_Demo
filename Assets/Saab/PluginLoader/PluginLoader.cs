using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GizmoSDK.GizmoBase;

namespace Saab.Unity.PluginLoader
{
    public class Platform
    {
        public const string BRIDGE = "UnityPluginInterface";
    }

    class UnityPluginInitializer
    {
        public UnityPluginInitializer()
        {
            UnityPlugin_Initialize();

            Message.OnMessage += On_Gizmo_Message;

            if (!KeyDatabase.SetLocalRegistry("config.xml"))
            {
                Message.Send("ConfigManager", MessageLevel.FATAL, "Couldn't load 'config.xml'");
            }
        }

        private static void On_Gizmo_Message(string sender, MessageLevel level, string message)
        {
            if ((level & (MessageLevel.DEBUG | MessageLevel.MEM_DEBUG)) > 0)
            {
                // Add your own
            }
            else if ((level & (MessageLevel.NOTICE | MessageLevel.ALWAYS)) > 0)
            {
               
            }
            else if ((level & MessageLevel.WARNING) > 0)
            {
               
            }
            else if ((level & (MessageLevel.FATAL | MessageLevel.ASSERT)) > 0)
            {
               
            }
        }

        ~UnityPluginInitializer()
        {
            UnityPlugin_UnInitialize();
        }

        [DllImport("UnityPluginInterface", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void UnityPlugin_Initialize();
        [DllImport("UnityPluginInterface", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void UnityPlugin_UnInitialize();
    }

    public class UnityPlugin : Reference
    {
        public UnityPlugin(string name) : base(UnityPlugin_GetPlugin(name))
        {

        }

        public DynamicType InvokeMethod(string method,DynamicType arg0=null)
        {
            return new DynamicType(UnityPlugin_InvokeMethod(GetNativeReference(), method,arg0?.GetNativeReference() ?? IntPtr.Zero));
        }

        static public string GetVersionInfo()
        {
            return Marshal.PtrToStringUni(UnityPlugin_GetVersionInfo());
        }

        

        [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr UnityPlugin_GetPlugin(string module);
        [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr UnityPlugin_GetVersionInfo();
        [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr UnityPlugin_InvokeMethod(IntPtr plugin, string method,IntPtr arg0_reference);
 
    }
}
