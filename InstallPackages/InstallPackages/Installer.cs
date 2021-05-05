using System;

namespace InstallPackages
{
    public class Installer
    {
        public void InstallLibraries()
        {
            GizmoSDK.GizmoBase.Platform.Initialize();
            GizmoSDK.Gizmo3D.Platform.Initialize();
            GizmoSDK.GizmoDistribution.Platform.Initialize();
            GizmoSDK.Coordinate.Platform.Initialize();
        }
    }
}
