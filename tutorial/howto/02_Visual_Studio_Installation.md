## 2.Visual Studio Installation

Visual Studio is required to build the GizmoSDK components and generate the plugin DLLs used by the Unity Map Streamer demo.

### 2.1 Install Visual Studio

1. Open the **Company Portal** application in the CDW.
2. Search for and install one of the following:

   * **Visual Studio 2022 Professional**
   * **Visual Studio 2026 Enterprise**
3. After installation is complete, restart the CDW.

### 2.2 Install Required Workloads

1. Open **Visual Studio Installer** from the Windows Start menu.
2. Find the installed Visual Studio version.
3. Click **Modify**.
4. Keep the default selected workloads.
5. Additionally select:

   * `.NET desktop development`
   * `Game development with Unity`
6. Under **Installation Details**, select **Unity Hub** if available.
7. Click **Modify**.

This installs the dependencies required to build the GizmoSDK solution and work with Unity.

### 2.3 First-Time Visual Studio Login

1. Open Visual Studio.
2. Sign in using a Microsoft or GitHub account if required.
3. Complete any authentication steps.

### 2.4 Solution Used for the Map Streamer Build

After cloning the CSWUnity repository, the solution used for the GizmoSDK build is:

`CSWUnity/vs17/Install\_Gizmo/Install\_Gizmo.sln`

The complete clone and build process is described in `04\_map\_streamer.md`.

