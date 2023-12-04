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
// Product		: Gizmo3D 2.12.120
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
// AMO  221130  Updated SM with new locking and camera sync             (2.12.35)
//
//******************************************************************************

//#define DEBUG_CAMERA

// Unity Managed classes
using UnityEngine;

// Gizmo Managed classes
using GizmoSDK.GizmoBase;
using GizmoSDK.Gizmo3D;

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
        Vector3 North { get; }                      // Get north vector in global coordinate system for current position

        void PreTraverse(bool locked);              // Executed before scene is traversed and updated with new transform and new geometry

        void PostTraverse(bool locked);             // Executed after nodes are repositioned with new transforms and correct activations

        double UpdateCamera(double renderTime);     // Executed just before camera transform is used. Update you cam animation in this

        void MapChanged();                          // Executed when map is changed

        float LodFactor { get; }                    // Current lod factor
    }

    [Serializable]
    public struct SceneManagerSettings
    {
        public UnityEngine.Shader DefaultShader;
        // ************* [Deprecated] *************
        //public UnityEngine.Shader CrossboardShader;
        public UnityEngine.ComputeShader ComputeShader;
        public int      SceneCleanupTime;                   // Time interval to cleanup scene and do gc
        public double   MaxBuildTime;                       // Max time to spend in frame to build objects
        public byte     DynamicLoaders;
        public bool RenderInUpdate;                         // true to render during component update method

        public static readonly SceneManagerSettings Default = new SceneManagerSettings 
        { 
            SceneCleanupTime = 10, 
            MaxBuildTime = 0.016,
            DynamicLoaders = 4,
            RenderInUpdate = true,
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
        public delegate void EventHandler_OnUpdateCamera(double renderTime);
        public delegate void EventHandler_OnMapLoadError(ref string url,string errorString,SerializeAdapter.AdapterError errorType,ref bool retry);

        // Notifications for external users that wants to add components to created game objects. Be swift as we are in edit lock

        public event EventHandler_OnGameObject  OnNewGeometry;      // GameObject with mesh
        public event EventHandler_OnGameObject  OnNewCrossboard;    // CrossBoard/tree placement 
        public event EventHandler_OnGameObject  OnNewLod;           // GameObject that toggles on off dep on distance
        public event EventHandler_OnGameObject  OnNewTransform;     // GameObject that has a specific parent transform
        public event EventHandler_OnGameObject  OnNewLoader;        // GameObject that works like a dynamic loader
        public event EventHandler_OnGameObject  OnEnterPool;

        public delegate void EventHandler_Traverse(bool locked);    // Pre and Post traversal in locked or unlocked mode (edit)

        public event EventHandler_Traverse          OnPreTraverse;  // Called after SceneManagerCamera is updated
        public event EventHandler_Traverse          OnPostTraverse; // Called after SceneManagerCamera is updated
        public event EventHandler_OnNode            OnMapChanged;
        public event EventHandler_OnMapLoadError    OnMapLoadError;
        public event EventHandler_OnUpdateCamera    OnUpdateCamera; // Called after SceneManagerCamera is updated

        #region ------------- Privates ----------------

        private Scene _native_scene;
        private gzCamera _native_camera;
        private Context _native_context;
        private CullTraverseAction _native_traverse_action;
        private GameObject _root;

        private NodeAction _actionReceiver;

        private Timer _cleanupTimer = new Timer();
  
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

        // stores information for a pending build job
        struct BuildInfo
        {
            public INodeBuilder Builder;
            public NodeHandle NodeHandle;
            public Node Node;
            public GameObject GameObject;
            public NodeHandle ActiveStateNode;
        }

        // A queue for new pending loads/unloads
        List<NodeLoadInfo> pendingLoaders = new List<NodeLoadInfo>(100);
        Dictionary<IntPtr,NodeLoadInfo> activePendingLoaders = new Dictionary<IntPtr, NodeLoadInfo>();

        // A queue for pending activations/deactivations
        List<ActivationInfo> pendingActivations = new List<ActivationInfo>(100);
  
        // A queue for post build work
        Queue<BuildInfo> pendingBuilds = new Queue<BuildInfo>(200);

        // A queue for AssetLoading
        Stack<AssetLoadInfo> pendingAssetLoads = new Stack<AssetLoadInfo>(100);

        // The current active asset bundles
        Dictionary<string, AssetBundle> currentAssetBundles = new Dictionary<string, AssetBundle>();

        // Linked List for nodes that needs updates on update
        LinkedList<GameObject> updateNodeObjects = new LinkedList<GameObject>();
        
        private bool _initialized;

        // Builders. Right now only one builder per node type hierarchy

        private readonly List<INodeBuilder> _builders = new List<INodeBuilder>();

        public void AddBuilder(INodeBuilder builder)
        {
            _builders.Add(builder);
        }

        public void RemoveBuilder(INodeBuilder builder)
        {
            _builders.Remove(builder);
        }

        private void CleanUpBuilders()
        {
            foreach (var builder in _builders)
            {
                builder.CleanUp();
            }
        }

        private INodeBuilder GetBuilderForNode(Node node)
        {
            foreach (var builder in _builders)
            {
                if (!builder.CanBuild(node))
                    continue;

                return builder;
            }

            return null;
        }

        private void BuildNode(INodeBuilder builder, NodeHandle nodeHandle, NodeHandle activeStateNode)
        {
            switch (builder.Priority)
            {
                case BuildPriority.Immediate:
                    if (builder.Build(nodeHandle, nodeHandle.gameObject, activeStateNode))
                        nodeHandle.builder = builder;
                    break;
                default:
                    pendingBuilds.Enqueue(new BuildInfo() 
                    { 
                        Builder = builder, 
                        NodeHandle = nodeHandle, 
                        Node = nodeHandle.node,
                        GameObject = nodeHandle.gameObject, 
                        ActiveStateNode = activeStateNode,
                    });
                    break;
            }
        }

        private void ProcessTransformNode(gzTransform node, unTransform transform)
        {
            // Check if transform is Active and not unit (1)
            if (!node.IsActive())
                return;

            if (node.GetTranslation(out Vec3 translation))
            {
                transform.localPosition = new Vector3(translation.x, translation.y, translation.z);
                return;
            }

            node.GetTransform(out Matrix4 mat4);

            transform.localPosition = mat4.Translation().ToVector3();
            transform.localScale = mat4.Scale().ToVector3();
            transform.localRotation = mat4.Quaternion().ToQuaternion();
        }

        private GameObject ProcessDynamicLoaderNode(DynamicLoader node, NodeHandle nodeHandle)
        {
            // Possibly we can add action interfaces for dyn load childs as they will get traversable if they have node actons
            if (NodeUtils.FindGameObjectsUnsafe(node.GetNativeReference(), out List<GameObject> list))
                return list[0]; // We are already in list, lets return first object wich is our main registered node
            
            // We are not registered
            NodeUtils.AddGameObjectReferenceUnsafe(node.GetNativeReference(), nodeHandle.gameObject);

            nodeHandle.inNodeUtilsRegistry = true;  // Added to registry

            return null;
        }

        private void ProcessLodNode(Lod node, NodeHandle nodeHandle, NodeHandle activeStateNode)
        {
            ProcessGroup(node, nodeHandle, activeStateNode, true);
        }

        private void ProcessRoiNode(Roi node, NodeHandle nodeHandle, NodeHandle activeStateNode)
        {
            RegisterNodeForUpdate(nodeHandle);
            ProcessGroup(node, nodeHandle, activeStateNode, true);
        }

        private void RegisterNodeForUpdate(NodeHandle nodeHandle)
        {
            nodeHandle.updateTransform = true;
            nodeHandle.inNodeUpdateList = true;
            updateNodeObjects.AddLast(nodeHandle.gameObject);
        }

        private void ProcessGroup(Group node, NodeHandle nodeHandle, NodeHandle activeStateNode, bool addActionInterfaces = false)
        {
            var parent = nodeHandle.transform;

            if (addActionInterfaces)        // use addActionInterface if we shall be able to enable/disable part of the tree using action callbacks
            {
                foreach (var child in node)
                {
                    var gameObject = TraverseInternal(child, activeStateNode);

                    var childNodeHandle = gameObject.GetComponent<NodeHandle>();

                    if (!NodeUtils.HasGameObjectsUnsafe(child.GetNativeReference()))
                    {
                        NodeUtils.AddGameObjectReferenceUnsafe(child.GetNativeReference(), gameObject);

                        childNodeHandle.inNodeUtilsRegistry = true;
                        child.AddActionInterface(_actionReceiver, NodeActionEvent.IS_TRAVERSABLE);
                        child.AddActionInterface(_actionReceiver, NodeActionEvent.IS_NOT_TRAVERSABLE);
                    }

                    gameObject.transform.SetParent(parent, false);
                }
                
                return;
            }

            foreach (var child in node)
            {
                var gameObject = TraverseInternal(child, activeStateNode);

                gameObject.transform.SetParent(parent, false);
            }
        }

        private NodeHandle CreateNodeHandle(Node node, PoolObjectFeature feature)
        {
            var nodeHandle = Allocate(feature, node);
            // we only use the name inside editor to avoid allocations in runtime
#if UNITY_EDITOR
            nodeHandle.name = node.GetName();
            
            if (string.IsNullOrEmpty(nodeHandle.name))
                nodeHandle.name = node.GetType().Name;
#endif
            return nodeHandle;
        }

        private GameObject BeginTraverse(Node node,bool dynloaded=false)
        {
            // We must be called in edit lock

            // if dynloaded we should add actions for traverse here as we can be toggled on/off by loader (fancier look)

            return TraverseInternal(node, null);
        }

        private GameObject TraverseInternal(Node node, NodeHandle activeStateNode)
        {
            // We must be called in edit lock
            
            System.Diagnostics.Debug.Assert(node != null && node.IsValid());

            // --------------------------- Add game object ---------------------------------------

            NodeHandle nodeHandle;

            var builder = GetBuilderForNode(node);

            if (builder == null)
            {
                nodeHandle = CreateNodeHandle(node, PoolObjectFeature.None);
                
                // check for new active state
                if (node.HasState())
                    activeStateNode = nodeHandle;
            }
            else
            {
                nodeHandle = CreateNodeHandle(node, builder.Feature);

                // check for new active state
                if (node.HasState())
                    activeStateNode = nodeHandle;

                // build gameobjects for this node
                BuildNode(builder, nodeHandle, activeStateNode);
            }

            var gameObject = nodeHandle.gameObject;

            // ------- GROUP based ITEMS first --------------------------------------------------

            // ---------------------------- Transform check -------------------------------------

            if (node is gzTransform tr)
            {
                try
                {
                    Performance.Enter("SM.Traverse.Transform");

                    ProcessTransformNode(tr, gameObject.transform);

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

            // Add dynamic loader as game object in dictionary
            // so other dynamic loaded data can parent them as child to loader
            else if (node is DynamicLoader dl)
            {
                try
                {
                    Performance.Enter("SM.Traverse.Loader");

                    var res = ProcessDynamicLoaderNode(dl, nodeHandle);
                    if (res != null)
                        return res;
                    
                    // We shall continue to iterate as a group to see if we already have loaded children


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

            else if (node is Lod ld)
            {
                ProcessLodNode(ld, nodeHandle, activeStateNode);

                Performance.Enter("SM.Traverse.OnNewLod");
                
                // Notify subscribers of new Lod
                OnNewLod?.Invoke(gameObject);
                
                Performance.Leave();

                // Dont process group as group is already processed
                return gameObject;
            }

            // ---------------------------- Roi check -------------------------------------

            if (node is Roi roi)
            {
                ProcessRoiNode(roi, nodeHandle, activeStateNode);

                // Dont process group as group is already processed
                return gameObject;
            }

            // ---------------------------- RoiNode check -------------------------------------

            else if (node is RoiNode)
            {
                RegisterNodeForUpdate(nodeHandle);

                // Fall through to group 
            }

            // ---------------------------- Group check -------------------------------------
                
            if (node is Group g)
            {
                ProcessGroup(g, nodeHandle, activeStateNode);
                return gameObject;
            }

            // -------------- ITEMS not built by builders --------------------------------------


            // ---------------------------- Geometry check -------------------------------------

            if (node is Geometry geom)
            {
                OnNewGeometry?.Invoke(gameObject);
            }

            // ---------------------------- Crossboard check -------------------------------------

            else if (node is Crossboard cross)
            {
                OnNewCrossboard?.Invoke(gameObject);
            }

            // ---------------------------ExtRef check -----------------------------------------

            else if (node is ExtRef extRef)
            {
                var info = new AssetLoadInfo(gameObject, extRef.ResourceURL, extRef.ObjectID);

                pendingAssetLoads.Push(info);
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
                    GameObject scene = BeginTraverse(MapControl.SystemMap.CurrentMap);

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
            //MapUrl = null;

            NodeLock.WaitLockEdit();

            try // We are now locked in edit
            {

                foreach (var p in pendingLoaders)
                {
                    p.loader?.Dispose();
                    p.node?.Dispose();
                }

                pendingLoaders.Clear();
                activePendingLoaders.Clear();

                foreach (var p in pendingActivations)
                {
                    p.node?.Dispose();
                }

                pendingActivations.Clear();

                if (_root)
                {
                    Free(_root.transform);
                    _root = null;
                }

                _native_scene?.RemoveAllNodes();

                MapControl.SystemMap.Reset();
            }
            finally
            {
                NodeLock.UnLock();
            }

            return true;
        }

        private void AddDefaultBuilders()
        {
            // initialize node builders
            AddBuilder(new DefaultGeometryNodeBuilder(Settings.DefaultShader));

            // ************* [Deprecated] *************
            //if (GfxCaps.CurrentCaps.HasFlag(Capability.UseTreeCrossboards))
            //AddBuilder(new CrossboardNodeBuilder(Settings.CrossboardShader, Settings.ComputeShader));
        }


        public bool InitializeInternal()        
        {
            // Initialize streamer APIs
            if (!GizmoSDK.Gizmo3D.Platform.Initialize())
                return false;

            // Initialize formats
            DbManager.Initialize();

            GizmoSDK.GizmoBase.Message.Send("SceneManager", MessageLevel.DEBUG, "Initialize Graph Streaming");

            // Add builder for registered types
            AddDefaultBuilders();

            // Setup internal subscription events
            _actionReceiver = new NodeAction("DynamicLoadManager");
            _actionReceiver.OnAction += ActionReceiver_OnAction;

            DynamicLoader.OnDynamicLoad += DynamicLoader_OnDynamicLoad;

            
            NodeLock.WaitLockEdit();

            try // We are now locked in edit
            {
                // Camera setup
                _native_camera = new PerspCamera("Test");
                _native_camera.RoiPosition = true;
                MapControl.SystemMap.Camera = _native_camera;

                // Top scene
                _native_scene = new Scene("Scene");
                _native_camera.Scene = _native_scene;

                // Top context
                _native_context = new Context();

#if DEBUG_CAMERA

                // If we want to visualize debug 3D
                _native_camera.Debug(_native_context);      // Enable to debug view

#endif // DEBUG_CAMERA

                // Default travrser
                _native_traverse_action = new CullTraverseAction();

                // _native_traverse_action.SetOmniTraverser(true);  // To skip camera cull and use LOD in omni directions


            }
            finally
            {

                NodeLock.UnLock();
            }

            // Set up dynamic loading
            DynamicLoader.UsePreCache(true);                    // Enable use of mipmap creation on dynamic loading
            DynamicLoaderManager.SetNumberOfActiveLoaders(Settings.DynamicLoaders);   // Lets start with 4 parallell threads
            DynamicLoaderManager.StartManager();

            // Start coroutines for asset loading
            StartCoroutine(AssetLoader());

            return true;
        }

        public bool Uninitialize()
        {
            if (!_initialized)
                   return false;

            // Stop manager
            DynamicLoaderManager.StopManager();

            ResetMap();

            // Remove actions
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

            // Dont do this as Unity wants to keep modules loaded
            //// Drop platform streamer
            //GizmoSDK.Gizmo3D.Platform.Uninitialize();

            _initialized = false;

            return true;
        }

        public bool Init(bool loadMap = true)
        {
            if (_initialized)
                return true;

            _initialized = true;
           
            // Initialize this manager
            if (!InitializeInternal())
                return false;

            // Load the map
            if (loadMap && !LoadMap(MapUrl))
                return false;

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
            // important that we do not run Init() from OnEable() since that will run during AddComponent(),
            // and in BTA we need to control when the map is loaded.
            if (!_initialized)
                return;

            Init();
        }

        private void OnApplicationQuit()
        {
            Uninitialize();
        }

        private void OnDisable()
        {
            // We add Unitialize and shut down threads here as this routine gets called by an edit in code
            Uninitialize();
        }
               
        private void DynamicLoader_OnDynamicLoad(DynamicLoadingState state, DynamicLoader loader, Node node)
        {
            // Locked in edit or render (render) by caller

            if (state == DynamicLoadingState.LOADED || state == DynamicLoadingState.UNLOADED)
            {
                if (!activePendingLoaders.ContainsKey(loader.GetNativeReference()))
                {
                    NodeLoadInfo info = new NodeLoadInfo(state, loader, node);

                    pendingLoaders.Add(info);       // Sorted order
                    activePendingLoaders.Add(loader.GetNativeReference(), info);    // Lookup
                }
                else
                {
                    // Balanced add/remove that will cancel traversal or delete

                    for (int i = pendingLoaders.Count - 1; i >= 0; i--)
                    {
                        if (pendingLoaders[i].loader.GetNativeReference() == loader.GetNativeReference())
                        {
                            pendingLoaders.RemoveAt(i);
                            break;
                        }
                    }

                    // remove reference
                    activePendingLoaders.Remove(loader.GetNativeReference());
                }

            }
            //else if (state == DynamicLoadingState.REQUEST_LOAD || state == DynamicLoadingState.REQUEST_UNLOAD || state == DynamicLoadingState.REQUEST_LOAD_CANCEL || state == DynamicLoadingState.REQUEST_LOAD_CANCEL)
            //{
            //    loader?.ReleaseNoDelete();      // Same here. We are getting refs to objects in scene graph that we shouldnt release in GC
            //    node?.ReleaseNoDelete();
            //}
            //else if (state == DynamicLoadingState.IN_LOADING)
            //{
            //    loader?.ReleaseNoDelete();      // Same here. We are getting refs to objects in scene graph that we shouldnt release in GC
            //    node?.ReleaseNoDelete();
            //}
            else
            {
                loader?.ReleaseNoDelete();      // Same here. We are getting refs to objects in scene graph that we shouldnt release in GC
                node?.ReleaseNoDelete();
            }
        }

        Timer _timeInPendingUpdates = new Timer();

        private void ProcessPendingUpdatesPreTraversal()
        {
            // We must be called in edit lock

            #region Dynamic Loading/Add/Remove native handles ---------------------------------------------------------------

            Performance.Enter("SM.ProcessPendingUpdates.BuildGO");

            // Process changes of the scenegraph
            ProcessPendingLoaders();

            // Potentiallly cleanup after pending unloads
            CleanUpBuilders();

            Performance.Leave();

            #endregion
        }

        private void ProcessPendingUpdatesPostTraversal()
        {
            // We must be called in edit lock

            #region Activate/Deactivate GameObjects based on scenegraph -----------------------------------------------------

            Performance.Enter("SM.ProcessPendingUpdates.ActivateGO");

            ProcessPendingActivations();

            Performance.Leave();

            #endregion

            #region Update slow loading assets ------------------------------------------------------------------------------

            Performance.Enter("SM.ProcessPendingUpdates.DequeBuildGO");

            var _remainingTimeToBuild = Settings.MaxBuildTime - _timeInPendingUpdates.GetTime();

            if(_remainingTimeToBuild>0)
                ProcessPendingBuilders(_remainingTimeToBuild);

            Performance.Leave();

            #endregion

            // Right now we use this as a dirty fix to handle unused shared materials            

            if(_cleanupTimer.GetTime() >= Settings.SceneCleanupTime)
            {             
                Performance.Enter("SM.ProcessPendingUpdates.Cleanup");

               _cleanupTimer.Reset();

                Resources.UnloadUnusedAssets();
                Performance.Leave();
            }
        }

        private void ProcessPendingLoaders()
        {
            foreach (NodeLoadInfo nodeLoadInfo in pendingLoaders)
            {
                if (nodeLoadInfo.state == DynamicLoadingState.LOADED)   // We got a callback from dyn loader that we were loaded or unloaded
                {
                    unTransform transform = NodeUtils.FindFirstGameObjectTransformUnsafe(nodeLoadInfo.loader.GetNativeReference());

                    if (transform == null)              // We have been unloaded or not registered
                        continue;

                    if (transform.childCount != 0)       // We have already a child and our sub graph was loaded
                        continue;

                    // TODO: Active state node can be further up the tree and should be located and passed here

                    GameObject go = BeginTraverse(nodeLoadInfo.node,true);       // Build sub graph as result of dynamic loader

                    if (go != null)
                        go.transform.SetParent(transform, false);               // Connect to our parent

                }
                else if (nodeLoadInfo.state == DynamicLoadingState.UNLOADED)
                {
                    List<GameObject> list;

                    if (NodeUtils.FindGameObjectsUnsafe(nodeLoadInfo.loader.GetNativeReference(), out list))
                    {
                        foreach (var go in list)
                        {
                            //We need to unload all limked go in hierarchy
                            var tr = go.transform;
                            for (var i = tr.childCount - 1; i >= 0; i--)
                                Free(tr.GetChild(i));
                        }
                    }
                }
            }

            pendingLoaders.Clear();         // Clear List
            activePendingLoaders.Clear();   // Clear index
        }

        private void ProcessPendingActivations()
        {
            foreach (ActivationInfo activationInfo in pendingActivations)
            {
                List<GameObject> list;  // We need to activate the correct nodes

                if (NodeUtils.FindGameObjectsUnsafe(activationInfo.node.GetNativeReference(), out list))
                {
                    foreach (GameObject obj in list)
                    {
                        if (activationInfo.state == NodeActionEvent.IS_TRAVERSABLE)
                            obj.SetActive(true);
                        else if (activationInfo.state == NodeActionEvent.IS_NOT_TRAVERSABLE)
                            obj.SetActive(false);
                    }
                }
                else 
                {
                    Message.Send(ID, MessageLevel.DEBUG, $"Got Activation {activationInfo.state} for missing node");
                }
            }

            pendingActivations.Clear();
        }

        private void ProcessPendingBuilders(double maxBuildTime)
        {
            var _timeInPendingBuilders = new Timer();

            while ( (pendingBuilds.Count > 0) && (_timeInPendingBuilders.GetTime() < maxBuildTime))
            {
                var buildInfo = pendingBuilds.Dequeue();

                // make sure the pending build is still valid
                if (buildInfo.NodeHandle.node != buildInfo.Node)
                    continue;

                var res = buildInfo.Builder.Build(buildInfo.NodeHandle, buildInfo.GameObject, buildInfo.ActiveStateNode);
                System.Diagnostics.Debug.Assert(res);
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
            if (Settings.RenderInUpdate)
                Render();
        }

        
        public void Render()
        {
            // Check if global world camera is present -----------------------

            if (SceneManagerCamera == null)
                return;

            // Check if local unity camera is present ------------------------

            var UnityCamera = SceneManagerCamera.Camera;

            if (UnityCamera == null)
                return;

            // Check if local native camera is present ------------------------

            if (_native_camera == null)
                return;

            try
            {
                Performance.Enter("SM.Update");

                // Lets try to build a scenegraph from pending changes from previous pass

                if (!NodeLock.TryLockEdit(30))      // 30 msek allow latency of other pending editor
                {
                    Message.Send(ID,MessageLevel.DEBUG, "Lock contention detected! NodeLock::TryLockEdit() FRAME LOST");

                    // We failed to refresh scene in reasonable time but we still need to issue updates;

                    Performance.Enter("SM.Update.PreTraverse");
                    SceneManagerCamera.PreTraverse(false);
                    OnPreTraverse?.Invoke(false);
                    Performance.Leave();

                    return;
                }

                // Notify about we are starting to traverse -----------------------

                Performance.Enter("SM.Update.PreTraverse");

                // Signal the world camera we are in pre traverse locked
                SceneManagerCamera.PreTraverse(true);

                // Signal the SM we are in pre traverse locked
                OnPreTraverse?.Invoke(true);

                Performance.Leave();

                try // We are now locked in edit
                {
                    Performance.Enter("SM.ProcessPendingUpdatesPreTraversal");

                    // Builds a scenegraph from changes from previous frame
                    ProcessPendingUpdatesPreTraversal();
                }
                finally
                {
                    Performance.Leave();

                    if (!NodeLock.ChangeToRenderLock())
                    {
                        NodeLock.UnLock();

                        Message.Send(ID, MessageLevel.DEBUG, "Failed to change into RenderLock");
                    }
                }

                if (!NodeLock.IsLockedRender())
                    return;


                if (activePendingLoaders.Count > 0) // Check if we got a mismatch in updates
                {
                    NodeLock.UnLock(); // Unlock render

                    Message.Send(ID, MessageLevel.FATAL, "Mismatch in virtual context (loaded/unloaded data)");

                    return;
                }


                try // We are now locked in read
                {
                    // Setup LOD

                    // lod bias
                    var lodFactor = SceneManagerCamera.LodFactor;
                    Lod.SetLODFactor(_native_context, lodFactor);
                    MapControl.SystemMap.LodFactor = lodFactor;

                    // Transfer camera parameters

                    PerspCamera perspCamera = _native_camera as PerspCamera;

                    // Right now we use system time as render time but this can be controlled externally in the future
                    var renderTime = GizmoSDK.GizmoBase.Time.SystemSeconds;

                    // Syncronized update. You should use the rendertime
                    renderTime = SceneManagerCamera.UpdateCamera(renderTime);
                    OnUpdateCamera?.Invoke(renderTime);

                    // Use this time in render
                    _native_context.SetCurrentRenderTime(renderTime);

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
                    if (!NodeLock.ChangeToEditLock())
                    {
                        NodeLock.UnLock();

                        Message.Send(ID, MessageLevel.DEBUG, "Failed to change into EditLock");
                    }
                }

                if (!NodeLock.IsLockedByMe())
                    return;

                try // We are now locked in edit
                {
                    Performance.Enter("SM.ProcessPendingUpdatesPostTraversal");

                    // Builds a scenegraph from changes from previous frame
                    ProcessPendingUpdatesPostTraversal();
                }
                finally
                {
                    Performance.Leave();

                    NodeLock.UnLock();
                }

                // Unlocked updates
                UpdateNodeInternals();

                // -------------------------------------------------------------
            }
            finally
            {
                // Notify about we are ready in traverse -----------------------

                Performance.Enter("SM.Update.PostTraverse");
                SceneManagerCamera.PostTraverse(false);
                OnPostTraverse?.Invoke(false);
                Performance.Leave();

                // Leave Scm update -------------------------------------------
                Performance.Leave();
            }
        }

        private readonly Dictionary<PoolObjectFeature, Stack<NodeHandle>> _free = new Dictionary<PoolObjectFeature, Stack<NodeHandle>>();

        private NodeHandle Allocate(PoolObjectFeature featureKey, Node node)
        {
            if (_free.TryGetValue(featureKey, out Stack<NodeHandle> pool) && pool.Count > 0)
            {
                var res = pool.Pop();
                res.node = node;

                // init
                res.gameObject.SetActive(true);
                res.gameObject.hideFlags = HideFlags.None;

                return res;
            }

            var go = new GameObject();
   
            var nh = go.AddComponent<NodeHandle>();
            nh.node = node;
            nh.featureKey = featureKey;
            


            return nh;
        }

        private void Free(unTransform transform)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
                Free(transform.GetChild(i));

            var nodeHandle = transform.GetComponent<NodeHandle>();

            if (nodeHandle)
                FreeHandle(nodeHandle);
            else
                Destroy(transform.gameObject);
        }

        private void FreeHandle(NodeHandle nodeHandle)
        {
            var featureKey = nodeHandle.featureKey;

            if (!_free.TryGetValue(featureKey, out Stack<NodeHandle> pool))
            {
                pool = new Stack<NodeHandle>();
                _free.Add(featureKey, pool);
            }

            pool.Push(nodeHandle);

            var go = nodeHandle.gameObject;
            go.SetActive(false);
            go.hideFlags = HideFlags.HideInHierarchy;

            var tr = go.transform;
            tr.parent = null;
            tr.localPosition = Vector3.zero;
            tr.localRotation = UnityEngine.Quaternion.identity;
            tr.localScale = Vector3.one;

            
            var node = nodeHandle.node;

            if (nodeHandle.inNodeUtilsRegistry)
                NodeUtils.RemoveGameObjectReferenceUnsafe(node.GetNativeReference(), go);

            if (nodeHandle.inNodeUpdateList)
                updateNodeObjects.Remove(go);

            if (node != null)
            {
                node.ReleaseAlreadyLocked();
                nodeHandle.node = null;
            }
            
            if (nodeHandle.builder != null)
            {
                nodeHandle.builder.BuiltObjectReturnedToPool(go);
                nodeHandle.builder = null;
            }

            nodeHandle.stateLoadInfo = StateLoadInfo.None;
            nodeHandle.stateFlags = NodeStateFlags.None;
            nodeHandle.texture = null;

            OnEnterPool?.Invoke(go);
        }
    }

    [Flags]
    public enum PoolObjectFeature : byte
    {
        //
        None = 0,

        //
        StaticMesh = 1 << 0,

        //
        Crossboard = 1 << 1,
    }


}



