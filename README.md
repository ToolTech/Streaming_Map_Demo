# Create new Unity project with MapStreamer

## Create a new project with MapStreamer

```
git init NewProject
```

_Note: "NewProject" is an example name in this guide_

## Add component_8866000201_MapStreamer as submodule

```
git submodule add https://devops.saab.se/TS/BTA/_git/component_8866000201_MapStreamer submodules/map-streamer
```

## Add gz-unity as submodule

```
git submodule add https://devops.saab.se/TS/BTA/_git/component_8866026201_gzUnity submodules/gz-unity
```

_Tip: If fetching of submodule hangs, make sure you have set up authentication for Git LFS. See [Wiki in DevOps Common](https://devops.saab.se/TS/Common/_wiki/wikis/Common.wiki/631/Git?anchor=git-lfs) for more information._

## Build gz-unity solution

```
submodules\gz-unity\build.bat
```

## Add packages in Unity Editor

Add packages in Unity Editor's Package Manager (under "Window" menu). Add following package from disk:  
* **com.saab.gz-unity**  
`C:\src\NewProject\submodules\gz-unity\com.saab.gz-unity\package.json`
* **com.saab.map-streamer**  
`C:\src\NewProject\submodules\gz-unity\com.saab.map-streamer\package.json`

Unity Editor will add absolute path's in the `manifest.json` file. Change them to relative path's

```
    "com.saab.gz-unity": "file:../../submodules/gz-unity/com.saab.gz-unity",
    "com.saab.map-streamer": "file:../../submodules/map-streamer/com.saab.map-streamer",
```
