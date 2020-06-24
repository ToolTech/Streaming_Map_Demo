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
// File			: SceneManager.cs
// Module		:
// Description	: Management of dynamic asset loader from GizmoSDK
// Author		: Anders Modén
// Product		: Gizmo3D 2.10.6
//
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows, Android, iOS and HoloLens for  
//			usage in Game or VisSim development.
//
//
// Revision History...
//
// Who	Date	Description
//
// AMO	180607	Created file        (2.9.1)
// AMO  200304  Updated SceneManager with events for external users     (2.10.1)
// AMO  200624  Updated transforms with rotation and generic transform  (2.10.6)
//
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************

//#define DEBUG_CAMERA

// Unity Managed classes
using UnityEngine;

// Gizmo Managed classes
using GizmoSDK.GizmoBase;
using GizmoSDK.Gizmo3D;
using GizmoSDK.Coordinate;

// Fix some conflicts between unity and Gizmo namespaces
using gzCamera = GizmoSDK.Gizmo3D.Camera;
using gzTransform = GizmoSDK.Gizmo3D.Transform;
using gzTexture = GizmoSDK.Gizmo3D.Texture;


// Map utility
using Saab.Foundation.Map;
using Saab.Unity.Extensions;
using Saab.Utility.Unity.NodeUtils;
using Saab.Utility.GfxCaps;

// Fix unity conflicts
using unTransform = UnityEngine.Transform;

// System
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.Networking;


namespace Saab.Foundation.Unity.MapStreamer
{
    // The SceneManager behaviour takes a unity camera and follows that to populate the current scene with GameObjects in a scenegraph hierarchy

    public interface ISceneManagerCamera
    {
        UnityEngine.Camera Camera { get; }
        Vec3D GlobalPosition { get; set; }           // Position in Global coordinate system

        Vector3 Up { get; }                         // Get up vector in global coordinate system for current position                  

        void PreTraverse();                         // Executed before scene is traversed and updated with new transform

        void PostTraverse();                        // Executed after nodes are repositioned with new transforms

        void MapChanged();                          // Executed when map is changed
    }

    [Serializable]
    public struct SceneManagerSettings
    {
        public UnityEngine.Shader DefaultShader;
        public UnityEngine.Shader CrossboardShader;
        public UnityEngine.ComputeShader ComputeShader;
        public int FrameCleanupInterval;
        public double MaxBuildTime;
        public byte DynamicLoaders;

        public static readonly SceneManagerSettings Default = new SceneManagerSettings 
        { 
            FrameCleanupInterval = 1000, 
            MaxBuildTime = 0.01, /*100hz*/ 
            DynamicLoaders = 4,
        };
    }


    public class SceneManager : MonoBehaviour
    {
        public SceneManagerSettings Settings = SceneManagerSettings.Default;
        public ISceneManagerCamera  SceneManagerCamera;
        public string               MapUrl;

        // Events ----------------------------------------------------------

        public delegate void EventHandler_OnGameObject(GameObject o);
        public delegate void EventHandler_OnNode(Node node);
        public delegate void EventHandler_OnMapLoadError(ref string url,string errorString,SerializeAdapter.AdapterError errorType,ref bool retry);

        // Notifications for external users that wants to add components to created game objects. Be swift as we are in edit lock

        public event EventHandler_OnGameObject  OnNewGeometry;   // GameObject with mesh
        public event EventHandler_OnGameObject  OnNewLod;        // GameObject that toggles on off dep on distance
        public event EventHandler_OnGameObject  OnNewTransform;  // GameObject that has a specific parent transform
        public event EventHandler_OnGameObject  OnNewLoader;     // GameObject that works like a dynamic loader
        

        public delegate void EventHandler_Traverse();

        public event EventHandler_Traverse      OnPreTraverse;
        public event EventHandler_Traverse      OnPostTraverse;
        public event EventHandler_OnNode        OnMapChanged;
        public event EventHandler_OnMapLoadError OnMapLoadError;

        #region ------------- Privates ----------------

        private Scene _native_scene;
        private gzCamera _native_camera;
        private Context _native_context;
        private CullTraverseAction _native_traverse_action;
        private GameObject _root;

        private NodeAction _actionReceiver;
  
        private int _unusedCounter = 0;
  
        private readonly string ID = "Saab.Foundation.Unity.MapStreamer.SceneManager";

 
        //#pragma warning disable 414
        //private UnityPluginInitializer _plugin_initializer;
        //#pragma warning restore 414

        #endregion

        struct NodeLoadInfo
        {
            public NodeLoadInfo(DynamicLoadingState _state, DynamicLoader _loader, Node _node)
            {
                state = _state;
                loader = _loader;
                node = _node;
            }

            public DynamicLoadingState state;
            public DynamicLoader loader;
            public Node node;

        };

        // The activation struct carries information about the state of a native node (loaded/unloaded)
        struct ActivationInfo
        {
            public ActivationInfo(NodeActionEvent _state, Node _node)
            {
                state = _state;
                node = _node;
            }

            public NodeActionEvent state;
            public Node node;
        };

        
        // AssetLoadInfo carries info about loading an asset
        struct AssetLoadInfo
        {
            public AssetLoadInfo(GameObject _parent, string _res_url, string _obj_id)
            {
                parent = _parent;
                resource_url = _res_url;
                object_id = _obj_id;
            }

            public GameObject parent;
            public string resource_url;
            public string object_id;
        };

        // A queue for new pending loads/unloads
        List<NodeLoadInfo> pendingLoaders = new List<NodeLoadInfo>(100);
        // A queue for pending activations/deactivations
        List<ActivationInfo> pendingActivations = new List<ActivationInfo>(100);
  

        // A queue for post build work
        Queue<NodeHandle> pendingBuilds = new Queue<NodeHandle>(500);

        // A queue for AssetLoading
        Stack<AssetLoadInfo> pendingAssetLoads = new Stack<AssetLoadInfo>(100);

        // The current active asset bundles
        Dictionary<string, AssetBundle> currentAssetBundles = new Dictionary<string, AssetBundle>();

        // Lookup for used materials
        Dictionary<IntPtr, Material> textureMaterialStorage = new Dictionary<IntPtr, Material>();

        // Linked List for nodes that needs updates on update
        LinkedList<GameObject> updateNodeObjects = new LinkedList<GameObject>();
        private bool _initialized;

        //Add GameObjects to dictionary

        // Traverse function iterates the scene graph to build local branches on Unity
        GameObject Traverse(Node n, Material currentMaterial)
        {
            // We must be called in edit lock

            if (n == null || !n.IsValid())
                return null;

            // --------------------------- Add game object ---------------------------------------

            string name = n.GetName();

            if (String.IsNullOrEmpty(name))
                name = n.GetNativeTypeName();

            GameObject gameObject = new GameObject(name);

            var nodeHandle = gameObject.AddComponent<NodeHandle>();

            //nodeHandle.Renderer = Renderer;
            nodeHandle.node = n;
            nodeHandle.currentMaterial = currentMaterial;
            nodeHandle.ComputeShader = Settings.ComputeShader;


            // ---------------------------- Check material state ----------------------------------

            if (n.HasState())
            {
                try
                {
                    Performance.Enter("SM.Traverse.State");

                    State state = n.State;

                    if (state.HasTexture(0) && state.GetMode(StateMode.TEXTURE) == StateModeActivation.ON)
                    {
                        gzTexture texture = state.GetTexture(0);

                        if (!textureMaterialStorage.TryGetValue(texture.GetNativeReference(), out currentMaterial))
                        {
                            if (texture.HasImage())
                            {

                                ImageFormat image_format;
                                ComponentType comp_type;
                                uint components;

                                uint depth;
                                uint width;
                                uint height;

                                uint size;

                                bool uncompress = false;

                                Image image = texture.GetImage();

                                image_format = image.GetFormat();

                                image.Dispose();

                                switch (image_format)      // Not yet
                                {
                                    case ImageFormat.COMPRESSED_RGBA8_ETC2:
                                        if (!SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGBA8))
                                            uncompress = true;
                                        break;

                                    case ImageFormat.COMPRESSED_RGB8_ETC2:
                                        if (!SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGB))
                                            uncompress = true;
                                        break;

                                    case ImageFormat.COMPRESSED_RGBA_S3TC_DXT1:
                                    case ImageFormat.COMPRESSED_RGB_S3TC_DXT1:
                                        if (!SystemInfo.SupportsTextureFormat(TextureFormat.DXT1))
                                            uncompress = true;
                                        break;

                                    case ImageFormat.COMPRESSED_RGBA_S3TC_DXT5:
                                        if (!SystemInfo.SupportsTextureFormat(TextureFormat.DXT5))
                                            uncompress = true;
                                        break;
                                }

                                IntPtr native_memory = IntPtr.Zero;

                                if (texture.GetMipMapImageArray(ref native_memory, out size, out image_format, out comp_type, out components, out width, out height, out depth, true, uncompress))
                                {
                                    if (depth == 1)
                                    {
                                        if (n is Crossboard)
                                            currentMaterial = new Material(Settings.CrossboardShader);
                                        else
                                            currentMaterial = new Material(Settings.DefaultShader);

                                        TextureFormat format = TextureFormat.ARGB32;

                                        switch (comp_type)
                                        {
                                            case ComponentType.UNSIGNED_BYTE:
                                                {
                                                    switch (image_format)
                                                    {
                                                        case ImageFormat.RGBA:
                                                            format = TextureFormat.RGBA32;
                                                            break;

                                                        case ImageFormat.RGB:
                                                            format = TextureFormat.RGB24;
                                                            break;

                                                        case ImageFormat.COMPRESSED_RGBA_S3TC_DXT1:
                                                        case ImageFormat.COMPRESSED_RGB_S3TC_DXT1:
                                                            format = TextureFormat.DXT1;
                                                            break;

                                                        case ImageFormat.COMPRESSED_RGBA_S3TC_DXT5:
                                                            format = TextureFormat.DXT5;
                                                            break;

                                                        case ImageFormat.COMPRESSED_RGB8_ETC2:
                                                            format = TextureFormat.ETC2_RGB;
                                                            break;

                                                        case ImageFormat.COMPRESSED_RGBA8_ETC2:
                                                            format = TextureFormat.ETC2_RGBA8;
                                                            break;


                                                        default:
                                                            // Issue your own error here because we can not use this texture yet
                                                            return null;
                                                    }
                                                }
                                                break;

                                            default:
                                                // Issue your own error here because we can not use this texture yet
                                                return null;

                                        }


                                        Texture2D tex = new Texture2D((int)width, (int)height, format, true);

                                        tex.LoadRawTextureData(native_memory,(int)size);


                                        switch (texture.MinFilter)
                                        {
                                            default:
                                                tex.filterMode = FilterMode.Point;
                                                break;

                                            case gzTexture.TextureMinFilter.LINEAR:
                                            case gzTexture.TextureMinFilter.LINEAR_MIPMAP_NEAREST:
                                                tex.filterMode = FilterMode.Bilinear;
                                                break;

                                            case gzTexture.TextureMinFilter.LINEAR_MIPMAP_LINEAR:
                                                tex.filterMode = FilterMode.Trilinear;
                                                break;
                                        }

                                        tex.Apply(texture.UseMipMaps, true);

                                        currentMaterial.mainTexture = tex;

                                    }
                                }
                            }

                            // Add some kind of check for textures shared by many
                            // Right now only for crossboards

                            if (n is Crossboard)
                                textureMaterialStorage.Add(texture.GetNativeReference(), currentMaterial);

                        }

                        nodeHandle.currentMaterial = currentMaterial;
                        texture.Dispose();
                    }

                    state.Dispose();

                }
                finally
                {
                    Performance.Leave();
                }
            }

            // ---------------------------- Transform check -------------------------------------

            gzTransform tr = n as gzTransform;

            if (tr != null)
            {
                try
                {
                    Performance.Enter("SM.Traverse.Transform");

                    if (tr.IsActive())
                    {
                        Vec3 translation;

                        bool handled=false;

                        if (tr.GetTranslation(out translation))
                        {
                            Vector3 trans = new Vector3(translation.x, translation.y, translation.z);
                            gameObject.transform.localPosition = trans;

                            handled = true;
                        }

                        // Rotation Euler   // TBD. Right now not handled 

                        // Rotation Quat

                        // Scale 

                        // Or generic if not handled

                        if(!handled)
                        {
                            Matrix4 transform;
                            tr.GetTransform(out transform);
                                                      

                            gameObject.transform.localPosition = transform.Translation().ToVector3();
                            gameObject.transform.localScale = transform.Scale().ToVector3();
                            gameObject.transform.localRotation = transform.Quaternion().ToQuaternion();
                        }
                    }

                    Performance.Enter("SM.Traverse.OnNewTransform");
                    // Notify subscribers of new Transform
                    OnNewTransform?.Invoke(gameObject);
                    Performance.Leave();
                }
                finally
                {
                    Performance.Leave();
                }
            }

            // ---------------------------- DynamicLoader check -------------------------------------

            DynamicLoader dl = n as DynamicLoader;      // Add dynamic loader as game object in dictionary
                                                        // so other dynamic loaded data can parent them as child to loader
            if (dl != null)
            {
                try
                {
                    Performance.Enter("SM.Traverse.Loader");

                    List<GameObject> list;

                    if (!NodeUtils.FindGameObjects(dl.GetNativeReference(), out list))     // We are not registered
                    {
                        NodeUtils.AddGameObjectReference(dl.GetNativeReference(), gameObject);

                        nodeHandle.inNodeUtilsRegistry = true;  // Added to registry

                        // We shall continue to iterate as a group to see if we already have loaded children
                    }
                    else  // We are already in list
                    {
                        return list[0];     // Lets return first object wich is our main registered node
                    }

                    Performance.Enter("SM.Traverse.OnNewLoader");
                    // Notify subscribers of new Loader
                    OnNewLoader?.Invoke(gameObject);
                    Performance.Leave();
                }
                finally
                {
                    Performance.Leave();
                }
            }

            // ---------------------------- Lod check -------------------------------------

            Lod ld = n as Lod;

            if (ld != null)
            {
                foreach (Node child in ld)
                {
                    GameObject go_child = Traverse(child, currentMaterial);

                    if (go_child == null)
                        return null;

                    NodeHandle h = go_child.GetComponent<NodeHandle>();

                    if (h != null)
                    {
                        if (!NodeUtils.HasGameObjects(h.node.GetNativeReference()))
                        {
                            NodeUtils.AddGameObjectReference(h.node.GetNativeReference(), go_child);

                            h.inNodeUtilsRegistry = true;
                            h.node.AddActionInterface(_actionReceiver, NodeActionEvent.IS_TRAVERSABLE);
                            h.node.AddActionInterface(_actionReceiver, NodeActionEvent.IS_NOT_TRAVERSABLE);
                        }

                    }

                    go_child.transform.SetParent(gameObject.transform, false);
                }

                Performance.Enter("SM.Traverse.OnNewLod");
                // Notify subscribers of new Lod
                OnNewLod?.Invoke(gameObject);
                Performance.Leave();

                // Dont process group as group is already processed
                return gameObject;
            }

            // ---------------------------- Roi check -------------------------------------

            Roi roi = n as Roi;

            if (roi != null)
            {

                nodeHandle.updateTransform = true;
                nodeHandle.inNodeUpdateList = true;
                updateNodeObjects.AddLast(gameObject);

                foreach (Node child in roi)
                {
                    GameObject go_child = Traverse(child, currentMaterial);

                    if (go_child == null)
                        return null;

                    NodeHandle h = go_child.GetComponent<NodeHandle>();

                    if (h != null)
                    {
                        if (!NodeUtils.HasGameObjects(h.node.GetNativeReference()))
                        {
                            NodeUtils.AddGameObjectReference(h.node.GetNativeReference(), go_child);

                            h.inNodeUtilsRegistry = true;
                            h.node.AddActionInterface(_actionReceiver, NodeActionEvent.IS_TRAVERSABLE);
                            h.node.AddActionInterface(_actionReceiver, NodeActionEvent.IS_NOT_TRAVERSABLE);
                        }

                    }

                    go_child.transform.SetParent(gameObject.transform, false);
                }

                // Dont process group
                return gameObject;

            }

            // ---------------------------- RoiNode check -------------------------------------

            RoiNode roinode = n as RoiNode;

            if (roinode != null)
            {
                nodeHandle.updateTransform = true;
                nodeHandle.inNodeUpdateList = true;
                updateNodeObjects.AddLast(gameObject);
            }

            // ---------------------------- Group check -------------------------------------

            Group g = n as Group;

            if (g != null)
            {

                foreach (Node child in g)
                {
                    GameObject go_child = Traverse(child, currentMaterial);

                    if (go_child == null)
                        return null;

                    go_child.transform.SetParent(gameObject.transform, false);
                }

                return gameObject;
            }

            // ---------------------------ExtRef check -----------------------------------------

            ExtRef ext = n as ExtRef;

            if (ext != null)
            {
                AssetLoadInfo info = new AssetLoadInfo(gameObject, ext.ResourceURL, ext.ObjectID);

                pendingAssetLoads.Push(info);
            }

            // ---------------------------- Crossboard check -----------------------------------

            Crossboard cb = n as Crossboard;

            if (cb != null && GfxCaps.HasCapability(Capability.UseTreeCrossboards))
            {
                // Scheduled for later build
                pendingBuilds.Enqueue(nodeHandle);
            }

            // ---------------------------- Geometry check -------------------------------------

            Geometry geom = n as Geometry;

            if (geom != null)
            {
                try
                {
                    Performance.Enter("SM.Traverse.Geometry");

                    nodeHandle.BuildGameObject();

                    Performance.Enter("SM.Traverse.OnNewGeometry");
                    // Notify subscribers of new Geometry
                    OnNewGeometry?.Invoke(gameObject);
                    Performance.Leave();

                    // Later on we will identify types of geoemtry that will be scheduled later if they are extensive and not ground that covers other geometry
                    // and build them in a later pass distributed over time
                    // pendingBuilds.Enqueue(nodeHandle);
                }
                finally
                {
                    Performance.Leave();
                }
            }

            return gameObject;

        }

        private IEnumerator AssetLoader()
        {
            while (true)
            {
                if (pendingAssetLoads.Count > 0)
                {
                    AssetLoadInfo info = pendingAssetLoads.Pop();

                    AssetBundle assetBundle;

                    if (!currentAssetBundles.TryGetValue(info.resource_url, out assetBundle))
                    {
                        UnityEngine.Networking.UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(info.resource_url, 0);

                        yield return request.SendWebRequest();

                        assetBundle = DownloadHandlerAssetBundle.GetContent(request);

                        if (assetBundle)
                            currentAssetBundles.Add(info.resource_url, assetBundle);
                    }

                    if (assetBundle != null)
                    {

                        GameObject extRefObject = assetBundle.LoadAsset<GameObject>(info.object_id);

                        if (extRefObject != null)
                        {
                            GameObject instance = Instantiate(extRefObject);

                            if (instance != null)
                            {
                                instance.name = info.object_id;
                                instance.transform.SetParent(info.parent.transform, false);
                            }
                        }
                    }
                }

                yield return null;
            }
        }

        // The LoadMap function takes an URL and loads the map into GizmoSDK native db
        public bool LoadMap(string mapURL)
        {
            NodeLock.WaitLockEdit();      // We assume we do all editing from main thread and to allow render we assume we edit in edit mode

            try // We are now locked in edit
            {

                if (!ResetMap())
                    return false;

                Node node = null;

                while (true)
                {
                    if (string.IsNullOrEmpty(mapURL))
                        break;

                    string errorString = "";
                    SerializeAdapter.AdapterError errorType = SerializeAdapter.AdapterError.NO_ERROR;
                    bool retry = false;

                    node = DbManager.LoadDB(mapURL, ref errorString, ref errorType);

                    if (node == null || !node.IsValid())
                    {
                        Message.Send(ID, MessageLevel.WARNING, $"Failed to load map {mapURL}");

                        OnMapLoadError?.Invoke(ref mapURL, errorString, errorType, ref retry);

                        if (retry)
                            continue;

                        return false;
                    }

                    break;
                }

                MapUrl = mapURL;

                MapControl.SystemMap.NodeURL = mapURL;
                MapControl.SystemMap.CurrentMap = node;

                

                if (node != null)
                {
                    _native_scene.AddNode(MapControl.SystemMap.CurrentMap);
                    _native_scene.Debug();

                    _root = new GameObject("root");
                    GameObject scene = Traverse(MapControl.SystemMap.CurrentMap, null);

                    if (scene != null)
                        scene.transform.SetParent(_root.transform, false);

                    // As GizmoSDK has a flipped Z axis going out of the screen we need a top transform to flip Z
                    _root.transform.localScale = new Vector3(1, 1, -1);
                }
                
                

                


                //// Add example object under ROI --------------------------------------------------------------

                //MapPos mapPos;

                //GetMapPosition(new LatPos(1.0084718541, 0.24984267815, 300), out mapPos, GroundClampType.GROUND, true);

                //_test = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                //_test.transform.parent = FindFirstGameObjectTransform(mapPos.roiNode);
                //_test.transform.localPosition = mapPos.position.ToVector3();
                //_test.transform.localScale = new Vector3(10, 10, 10);

                if (SceneManagerCamera != null)
                    SceneManagerCamera.MapChanged();

                OnMapChanged?.Invoke(node);
            }
            finally
            {
                NodeLock.UnLock();
            }

            return true;
        }

        public bool ResetMap()
        {
            MapUrl = null;

            NodeLock.WaitLockEdit();

            try // We are now locked in edit
            {

                foreach (var p in pendingLoaders)
                {
                    p.loader?.Dispose();
                    p.node?.Dispose();
                }

                pendingLoaders.Clear();

                foreach (var p in pendingActivations)
                {
                    p.node?.Dispose();
                }

                pendingActivations.Clear();

                RemoveGameObjectHandles(_root);

                GameObject.Destroy(_root);

                _root = null;

                MapControl.SystemMap.Reset();
            }
            finally
            {
                NodeLock.UnLock();
            }

            return true;
        }


        public bool InitializeInternal()        
        {
            _actionReceiver = new NodeAction("DynamicLoadManager");

            _actionReceiver.OnAction += ActionReceiver_OnAction;

            GizmoSDK.GizmoBase.Message.Send("SceneManager", MessageLevel.DEBUG, "Loading Graph");

            NodeLock.WaitLockEdit();

            try // We are now locked in edit
            {

                _native_camera = new PerspCamera("Test");
                _native_camera.RoiPosition = true;
                MapControl.SystemMap.Camera = _native_camera;

                _native_scene = new Scene("TestScene");

                _native_context = new Context();

#if DEBUG_CAMERA
                _native_camera.Debug(_native_context);      // Enable to debug view
#endif // DEBUG_CAMERA

                _native_traverse_action = new CullTraverseAction();

                // _native_traverse_action.SetOmniTraverser(true);  // To skip camera cull and use LOD in omni directions

                DynamicLoader.OnDynamicLoad += DynamicLoader_OnDynamicLoad;

                _native_camera.Scene = _native_scene;
            }
            finally
            {

                NodeLock.UnLock();
            }


            DynamicLoader.UsePreCache(true);                    // Enable use of mipmap creation on dynamic loading
            DynamicLoaderManager.SetNumberOfActiveLoaders(Settings.DynamicLoaders);   // Lets start with 4 parallell threads
            DynamicLoaderManager.StartManager();

            return true;
        }

        public bool Uninitialize()
        {

            DynamicLoaderManager.StopManager();

            ResetMap();

            DynamicLoader.OnDynamicLoad -= DynamicLoader_OnDynamicLoad;
            _actionReceiver.OnAction -= ActionReceiver_OnAction;

            NodeLock.WaitLockEdit();

            try // We are now locked in edit
            {

                _native_camera.Debug(_native_context, false);
                _native_camera.Dispose();
                _native_camera = null;

                _native_context.Dispose();
                _native_context = null;

                _native_scene.Dispose();
                _native_scene = null;


                _actionReceiver.Dispose();
                _actionReceiver = null;

            }
            finally
            {
                NodeLock.UnLock();
            }


            return true;
        }

        internal bool Init(bool loadMap = true)
        {
            if (_initialized)
                return true;


            _initialized = true;

            StartCoroutine(AssetLoader());

            if (!InitMap(loadMap))
            {
                _initialized = false;
                return false;
            }

            return true;
        }


        private void ActionReceiver_OnAction(NodeAction sender, NodeActionEvent action, Context context, NodeActionProvider trigger, TraverseAction traverser, IntPtr userdata)
        {
            // Locked in edit or render (render) by caller

            if ((action == NodeActionEvent.IS_TRAVERSABLE) || (action == NodeActionEvent.IS_NOT_TRAVERSABLE))
            {
                pendingActivations.Add(new ActivationInfo(action, trigger as Node));
            }
            else
                trigger?.ReleaseNoDelete();      // We are getting ref counts on objects in scene graph and these need to be released immediately

            traverser?.ReleaseNoDelete();
            context?.ReleaseNoDelete();
        }

        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            if (!_initialized)
                return;

            InitMap(true);
        }


        private void OnDisable()
        {
            // We add Unitialize and shut down threads here as this routine gets called by an edit in code
            Uninitialize();
        }
        private bool InitMap(bool loadMap)
        {
            //_plugin_initializer = new UnityPluginInitializer();  // in case we need it our own
            if (!GizmoSDK.Gizmo3D.Platform.Initialize())
                return false;

            // Initialize formats
            DbManager.Initialize();

            // Initialize this manager
            if (!InitializeInternal())
                return false;

            // Load the map
            if (loadMap && !LoadMap(MapUrl))
                return false;

            return true;
        }

       
        private void DynamicLoader_OnDynamicLoad(DynamicLoadingState state, DynamicLoader loader, Node node)
        {
            // Locked in edit or render (render) by caller

            if (state == DynamicLoadingState.LOADED || state == DynamicLoadingState.UNLOADED)
            {
                pendingLoaders.Add(new NodeLoadInfo(state, loader, node));
            }
            else
            {
                loader?.ReleaseNoDelete();      // Same here. We are getting refs to objects in scene graph that we shouldnt release in GC
                node?.ReleaseNoDelete();
            }
        }

        private void RemoveGameObjectHandles(GameObject obj)
        {
            if (obj == null)
                return;

            foreach (unTransform child in obj.transform)    // Recursive remove
                RemoveGameObjectHandles(child.gameObject);

            NodeHandle h = obj.GetComponent<NodeHandle>();

            if (h != null)
            {
                if (h.inNodeUtilsRegistry)
                {
                    NodeUtils.RemoveGameObjectReference(h.node.GetNativeReference(), obj);
                    h.inNodeUtilsRegistry = false;
                }

                if (h.inNodeUpdateList)
                {
                    updateNodeObjects.Remove(obj);
                    h.inNodeUpdateList = false;
                }

                h.node?.Dispose();
                h.node = null;
            }

        }


        
        private void ProcessPendingUpdates()
        {
            // We must be called in edit lock

            var timer = System.Diagnostics.Stopwatch.StartNew();      // Measure time precise in update

            #region Dynamic Loading/Add/Remove native handles ---------------------------------------------------------------

            Performance.Enter("SM.ProcessPendingUpdates.BuildGO");

            foreach (NodeLoadInfo nodeLoadInfo in pendingLoaders)
            {
                if (nodeLoadInfo.state == DynamicLoadingState.LOADED)   // We got a callback from dyn loader that we were loaded or unloaded
                {
                    unTransform transform = NodeUtils.FindFirstGameObjectTransform(nodeLoadInfo.loader.GetNativeReference());

                    if (transform == null)              // We have been unloaded or not registered
                        continue;

                    if (transform.childCount != 0)       // We have already a child and our sub graph was loaded
                        continue;

                    GameObject go = Traverse(nodeLoadInfo.node, null);          // Build sub graph

                    if(go!=null)
                        go.transform.SetParent(transform, false);               // Connect to our parent

                }
                else if (nodeLoadInfo.state == DynamicLoadingState.UNLOADED)
                {
                    List<GameObject> list;

                    if(NodeUtils.FindGameObjects(nodeLoadInfo.loader.GetNativeReference(),out list))
                    {
                        foreach (GameObject go in list)
                        {
                            foreach (unTransform child in go.transform)         //We need to unload all limked go in hierarchy
                            {
                                RemoveGameObjectHandles(child.gameObject);
                                GameObject.Destroy(child.gameObject);
                            }
                        }
                    }
                }
            }

            pendingLoaders.Clear();

            Performance.Leave();

            #endregion

            #region Activate/Deactivate GameObjects based on scenegraph -----------------------------------------------------

            Performance.Enter("SM.ProcessPendingUpdates.ActivateGO");

            foreach (ActivationInfo activationInfo in pendingActivations)
            {
                List<GameObject> list;  // We need to activate the correct nodes

                if (NodeUtils.FindGameObjects(activationInfo.node.GetNativeReference(), out list))
                {
                    foreach (GameObject obj in list)
                    {
                        if (activationInfo.state == NodeActionEvent.IS_TRAVERSABLE)
                            obj.SetActive(true);
                        else
                            obj.SetActive(false);
                    }
                }
            }

            pendingActivations.Clear();

            Performance.Leave();

            #endregion

            #region Update slow loading assets ------------------------------------------------------------------------------

            Performance.Enter("SM.ProcessPendingUpdates.DequeBuildGO");

            while (pendingBuilds.Count > 0 && timer.Elapsed.TotalSeconds < Settings.MaxBuildTime)
            {
                NodeHandle handle = pendingBuilds.Dequeue();
                handle.BuildGameObject();
            }

            Performance.Leave();

            #endregion

            // Right now we use this as a dirty fix to handle unused shared materials

            _unusedCounter = (_unusedCounter + 1) % Settings.FrameCleanupInterval;
            if (_unusedCounter == 0)
            {
                Performance.Enter("SM.ProcessPendingUpdates.Cleanup");
                Resources.UnloadUnusedAssets();
                Performance.Leave();
            }
        }
        
        private void UpdateNodeInternals()
        {
            // Only called if SceneManagerCamera is not null

            Performance.Enter("SM.UpdateNodeInternals");

            foreach (GameObject go in updateNodeObjects)
            {
                NodeHandle h = go.GetComponent<NodeHandle>();

                h.UpdateNodeInternals();
            }

            Performance.Leave();
        }

        // Update is called once per frame
        private void Update()
        {
            try
            {
                Performance.Enter("SM.Update");

                if (!NodeLock.TryLockEdit(30))      // 30 msek allow latency of other pending editor
                {
                    // We failed to refresh scene in reasonable time but we still need to issue updates;
                    
                    Performance.Enter("SM.Update.PreTraverse");
                    if(SceneManagerCamera!=null)
                        SceneManagerCamera.PreTraverse();
                    OnPreTraverse?.Invoke();
                    Performance.Leave();

                    return;
                }

                try // We are now locked in edit
                {
                    ProcessPendingUpdates();
                }
                finally
                {
                    NodeLock.UnLock();
                }

                // Notify about we are starting to traverse -----------------------

                Performance.Enter("SM.Update.PreTraverse");
                if (SceneManagerCamera != null)
                    SceneManagerCamera.PreTraverse();
                OnPreTraverse?.Invoke();
                Performance.Leave();

                // Check if camera present ---------------------------------------

                if (SceneManagerCamera == null)
                    return;
                              

                // ---------------------------------------------------------------

                var UnityCamera = SceneManagerCamera.Camera;

                if (UnityCamera == null)
                    return;

                if (!NodeLock.TryLockRender(30))    // 30 millisek latency allowed
                    return;

                try // We are now locked in read
                {
                    // Transfer camera parameters

                    PerspCamera perspCamera = _native_camera as PerspCamera;

                    if (perspCamera != null)
                    {
                        perspCamera.VerticalFOV = UnityCamera.fieldOfView;
                        perspCamera.HorizontalFOV = 2 * Mathf.Atan(Mathf.Tan(UnityCamera.fieldOfView * Mathf.Deg2Rad / 2) * UnityCamera.aspect) * Mathf.Rad2Deg; ;
                        perspCamera.NearClipPlane = UnityCamera.nearClipPlane;
                        perspCamera.FarClipPlane = UnityCamera.farClipPlane;
                    }

                    Matrix4x4 unity_camera_transform = UnityCamera.transform.worldToLocalMatrix;

                    _native_camera.Transform = unity_camera_transform.ToZFlippedMatrix4();

                    _native_camera.Position = SceneManagerCamera.GlobalPosition;

                    _native_camera.Render(_native_context, 1000, 1000, 1000, _native_traverse_action);

#if DEBUG_CAMERA
                _native_camera.DebugRefresh();
#endif
                }
                finally
                {
                    NodeLock.UnLock();
                }

                UpdateNodeInternals();

                // -------------------------------------------------------------
            }
            finally
            {
                // Notify about we are ready in traverse -----------------------

                Performance.Enter("SM.Update.PostTraverse");
                if(SceneManagerCamera!=null)
                    SceneManagerCamera.PostTraverse();
                OnPostTraverse?.Invoke();
                Performance.Leave();

                // Leave Scm update -------------------------------------------
                Performance.Leave();
            }
        }
        
    }

}
