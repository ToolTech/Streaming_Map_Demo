using System;

namespace InstallPackages
{
    public class Installer
    {
        public void InstallLibraries()
        {
            GizmoSDK.GizmoBase.Platform.Initialize();
        }
    }
}
