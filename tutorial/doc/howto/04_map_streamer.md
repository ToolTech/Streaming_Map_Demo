## 4\. Map Streamer

This section describes the complete workflow for setting up and running the **CSWUnity Map Streamer** demo after CDW, Visual Studio, and Unity are installed.

### 4.1 Install Git

1. Open the **Company Portal** application in the CDW.
2. Search for and install **Git**.
3. This installs:

   * Git Bash
   * Git CMD

### 4.2 Clone the CSWUnity Repository

1. Open a browser and go to:

   `https://saab.ghe.com`

2. Authenticate using Saab SSO.
3. Open **Git Bash**.
4. Navigate to the directory where you want to clone the repository.
5. Run:

`git clone https://saab.ghe.com/saab/CSWUnity.git`

6. Navigate into the repository:

`cd CSWUnity`

The repository contains files/folders similar to:

&#x20;   CSWUnity
    │
    ├── Build\_x64.bat
    ├── Build\_x64\_d.bat
    ├── README.md
    ├── projects
    ├── tutorial
    ├── vs17
    ├── com.saab.map-streamer
    └── Saab.Foundation.Map.Manager.Test


### 4.3 Open the Repository in File Explorer

From Git Bash, open the current repository folder in Windows File Explorer:

`explorer .`

Alternatively, navigate manually to the cloned `CSWUnity` folder.

### 4.4 Open the GizmoSDK Solution

In File Explorer, navigate to:

`CSWUnity/vs17/Install\_Gizmo/`

Open:

`Install\_Gizmo.sln`

The solution opens in Visual Studio.

### 4.5 Build the GizmoSDK Components

Building the solution:

* Downloads the required NuGet packages.
* Builds the required libraries.
* Generates plugin DLLs.
* Deploys the plugin binaries required by Unity.

The available build configurations are:

&#x20;   Debug | x64
    Release | x64


Recommended usage:

* **Release | x64** — Recommended for running the demo.
* **Debug | x64** — Recommended for development and debugging.

Use only one configuration at a time.

To build the solution in Visual Studio:

`Build -> Build Solution`

or press:

`Ctrl + Shift + B`

### 4.6 Switching Between Debug and Release

If changing the build configuration from Debug to Release, or from Release to Debug:

1. Close Unity.
2. Run:

`clean.bat`

3. Reopen `Install\_Gizmo.sln`.
4. Select the required configuration.
5. Build the solution again.

> Do not mix Debug and Release plugin DLLs.

### 4.7 Build Using Batch Files

The SDK can also be built using the batch files available in the CSWUnity repository.

For a Release build:

`build\_x64.bat`

For a Debug build:

`build\_x64\_d.bat`

Before switching between build configurations, run:

`clean.bat`

### 4.8 Add the Map Streamer Project to Unity Hub

1. Open **Unity Hub**.
2. Select **Projects**.
3. Click **Add Project From Disk**.
4. Select:

`CSWUnity/projects/com.saab.map-streamer`

> Ensure that the correct Unity project folder is selected.

### 4.9 Open the Project in Unity

1. In Unity Hub, click the `com.saab.map-streamer` project.
2. Wait for Unity Editor to finish:

   * Importing assets
   * Compiling scripts
   * Resolving packages
3. In the Unity Project window, open:

`Assets -> Scenes -> Example Scene`

4. Double-click **Example Scene**.

### 4.10 Run the Demo

Press the **Play (▶)** button at the top of the Unity Editor.

The streaming map should load in the scene.

### 4.11 Navigation Controls

|Key|Action|
|-|-|
|`W`|Move Forward|
|`A`|Move Left|
|`S`|Move Backward|
|`D`|Move Right|
|`Space`|Move Up|
|`Ctrl`|Move Down|
|`Arrow Keys`|Rotate Camera|
|`Shift`|Increase Movement Speed|

### 4.12 Troubleshooting

If the plugin build is changed from **Release to Debug** or from **Debug to Release**:

1. Close Unity.
2. Run `clean.bat`.
3. Rebuild using only the required configuration.
4. Reopen Unity.

This prevents native plugin mismatches and related Unity crashes.

