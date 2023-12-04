using System;

namespace Install_Gizmo
{
    public class Install
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
