//*****************************************************************************
// File			: SceneManager.cs
// Module		:
// Description	: Management of dynamic asset loader from GizmoSDK
// Author		: Anders Modén
// Product		: Gizmo3D 2.10.1
//
// Copyright © 2003- Saab Training Systems AB, Sweden
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
using gzImage = GizmoSDK.GizmoBase.Image;

// Map utility
using Saab.Map.CoordUtil;
using Saab.Unity.Extensions;
using Saab.Unity.PluginLoader;

// Fix unity conflicts
using unTransform = UnityEngine.Transform;

// System
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.Networking;
using Saab.Core;

namespace Saab.Unity.MapStreamer
{
    public static class UnityGizmoExtensions
    {
        public static Vec3D ToVec3D(this Vector3 vec)
        {
            return new Vec3D(vec.x, vec.y, vec.z);
        }
        public static Vector3 ToVector3(this Vec3D vec)
        {
            return new Vector3((float)vec.x, (float)vec.y, (float)vec.z);
        }
    }

    // The SceneManager behaviour takes a unity camera and follows that to populate the current scene with GameObjects in a scenegraph hierarchy
    public class SceneManager : MonoBehaviour
    {

        public UnityEngine.Camera   UnityCamera;
        public UnityEngine.Shader   Shader;

        public string               MapUrl;
        public int                  FrameCleanupInterval = 1000;


        #region ------------- Privates ----------------

        private Scene _native_scene;
        private gzCamera _native_camera;
        private Context _native_context;
        private CullTraverseAction _native_traverse_action;
        private GameObject _root;

        private NodeAction _actionReceiver;
        private Matrix4x4 _zflipMatrix;

        private int _unusedCounter = 0;
        private readonly Controller _controller = new Controller();

        #pragma warning disable 414
        private UnityPluginInitializer _plugin_initializer;   // If we need our own plugin initializer
        #pragma warning restore 414

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

        // GameObjectInfo carries info about added objects to scene
        struct GameObjectInfo
        {
            public GameObjectInfo(NodeLoadInfo _info, GameObject _go)
            {
                nodeInfo = _info;
                go = _go;
            }

            public NodeLoadInfo nodeInfo;
            public GameObject go;

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
        // A queue for pending new objects
        List<GameObjectInfo> pendingObjects = new List<GameObjectInfo>(100);

        // A queue for AssetLoading
        Stack<AssetLoadInfo> pendingAssetLoads = new Stack<AssetLoadInfo>(100);

        // The lookup dictinary to find a game object with s specfic native handle
        Dictionary<IntPtr, List<GameObject>> currentObjects = new Dictionary<IntPtr, List<GameObject>>();

        Dictionary<string, AssetBundle> currentAssetBundles = new Dictionary<string, AssetBundle>();

        //Add GameObjects to dictionary

        void AddGameObjectReference(IntPtr nativeReference, GameObject gameObject)
        {
            List<GameObject> gameObjectList;

            if (!currentObjects.TryGetValue(nativeReference, out gameObjectList))
            {
                gameObjectList = new List<GameObject>();
                currentObjects.Add(nativeReference, gameObjectList);
            }

            gameObjectList.Add(gameObject);
        }

        void RemoveGameObjectReference(IntPtr nativeReference, GameObject gameObject)
        {
            List<GameObject> gameObjectList;

            if (currentObjects.TryGetValue(nativeReference, out gameObjectList))
            {
                gameObjectList.Remove(gameObject);

                if (gameObjectList.Count == 0)
                    currentObjects.Remove(nativeReference);
            }
        }

        // Traverse function iterates the scene graph to build local branches on Unity
        GameObject Traverse(Node n, Material currentMaterial)
        {
            if (n == null || !n.IsValid())
                return null;

            string name = n.GetName();

            if (String.IsNullOrEmpty(name))
                name = n.GetNativeTypeName();

            GameObject gameObject = new GameObject(name);

            var nodeHandle = gameObject.AddComponent<NodeHandle>();

            nodeHandle.node = n;
            nodeHandle.inObjectDict = false;

            // ---------------------------- Check material state ----------------------------------

            if (n.HasState())
            {
                State state = n.State;

                if (state.HasTexture(0) && state.GetMode(StateMode.TEXTURE) == StateModeActivation.ON)
                {
                    gzTexture texture = state.GetTexture(0);

                    if (texture.HasImage())
                    {
                        gzImage image = texture.GetImage();

                        int depth = (int)image.GetDepth();
                        int width = (int)image.GetWidth();
                        int height = (int)image.GetHeight();

                        if (depth == 1)
                        {
                            if (currentMaterial == null)
                                currentMaterial = new Material(Shader);


                            TextureFormat format = TextureFormat.ARGB32;

                            ImageType image_type = image.GetImageType();

                            switch (image_type)
                            {
                                case ImageType.RGB_8_DXT1:
                                    format = TextureFormat.DXT1;
                                    break;
                            }

                            Texture2D tex = new Texture2D(width, height, format, false);

                            byte[] image_data;

                            image.GetImageArray(out image_data);

                            tex.LoadRawTextureData(image_data);

                            tex.filterMode = FilterMode.Trilinear;

                            tex.Apply();

                            currentMaterial.mainTexture = tex;

                        }



                        image.Dispose();
                    }

                    texture.Dispose();
                }


                state.Dispose();
            }

            // ---------------------------- Transform check -------------------------------------

            gzTransform tr = n as gzTransform;

            if (tr != null)
            {
                Vec3 translation;

                if (tr.GetTranslation(out translation))
                {
                    Vector3 trans = new Vector3(translation.x, translation.y, translation.z);
                    gameObject.transform.localPosition = trans;
                }

            }

            // ---------------------------- DynamicLoader check -------------------------------------

            DynamicLoader dl = n as DynamicLoader;

            if (dl != null)
            {
                if (!nodeHandle.inObjectDict)
                {
                    AddGameObjectReference(dl.GetNativeReference(), gameObject);
                    nodeHandle.inObjectDict = true;
                }
            }

            // ---------------------------- Lod check -------------------------------------

            Lod ld = n as Lod;

            if (ld != null)
            {
                foreach (Node child in ld)
                {
                    GameObject go_child = Traverse(child, currentMaterial);

                    NodeHandle h = go_child.GetComponent<NodeHandle>();

                    if (h != null)
                    {
                        if (!h.inObjectDict)
                        {
                            AddGameObjectReference(h.node.GetNativeReference(), go_child);
                            h.node.AddActionInterface(_actionReceiver, NodeActionEvent.IS_TRAVERSABLE);
                            h.node.AddActionInterface(_actionReceiver, NodeActionEvent.IS_NOT_TRAVERSABLE);
                            h.inObjectDict = true;
                        }

                    }

                    go_child.transform.SetParent(gameObject.transform, false);
                }

                // Dont process group
                return gameObject;
            }

            // ---------------------------- Roi check -------------------------------------

            Roi roi = n as Roi;

            if (roi != null)
            {
                nodeHandle.updateTransform = true;

                foreach (Node child in roi)
                {
                    GameObject go_child = Traverse(child, currentMaterial);

                    NodeHandle h = go_child.GetComponent<NodeHandle>();

                    if (h != null)
                    {
                        if (!h.inObjectDict)
                        {
                            AddGameObjectReference(h.node.GetNativeReference(), go_child);
                            h.node.AddActionInterface(_actionReceiver, NodeActionEvent.IS_TRAVERSABLE);
                            h.node.AddActionInterface(_actionReceiver, NodeActionEvent.IS_NOT_TRAVERSABLE);
                            h.inObjectDict = true;
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
            }

            // ---------------------------- Group check -------------------------------------

            Group g = n as Group;

            if (g != null)
            {

                foreach (Node child in g)
                {
                    GameObject go_child = Traverse(child, currentMaterial);

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

            // ---------------------------- Geometry check -------------------------------------

            Geometry geom = n as Geometry;

            if (geom != null)
            {
                float[] float_data;
                int[] indices;

                if (geom.GetVertexData(out float_data, out indices))
                {
                    MeshFilter filter = gameObject.AddComponent<MeshFilter>();
                    MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

                    Mesh mesh = new Mesh();

                    Vector3[] vertices = new Vector3[float_data.Length / 3];

                    int float_index = 0;

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(float_data[float_index], float_data[float_index + 1], float_data[float_index + 2]);

                        float_index += 3;
                    }

                    mesh.vertices = vertices;
                    mesh.triangles = indices;


                    if (geom.GetColorData(out float_data))
                    {
                        if (float_data.Length / 4 == vertices.Length)
                        {
                            float_index = 0;

                            Color[] cols = new Color[vertices.Length];

                            for (int i = 0; i < vertices.Length; i++)
                            {
                                cols[i] = new Color(float_data[float_index], float_data[float_index + 1], float_data[float_index + 2], float_data[float_index + 3]);
                                float_index += 4;
                            }

                            mesh.colors = cols;
                        }
                    }

                    if (geom.GetNormalData(out float_data))
                    {
                        if (float_data.Length / 3 == vertices.Length)
                        {
                            float_index = 0;

                            Vector3[] normals = new Vector3[vertices.Length];

                            for (int i = 0; i < vertices.Length; i++)
                            {
                                normals[i] = new Vector3(float_data[float_index], float_data[float_index + 1], float_data[float_index + 2]);
                                float_index += 3;
                            }

                            mesh.normals = normals;
                        }
                    }
                    //else
                    //    mesh.RecalculateNormals();

                    uint texture_units = geom.GetTextureUnits();

                    if (texture_units > 0)
                    {
                        if (geom.GetTexCoordData(out float_data, 0))
                        {
                            if (float_data.Length / 2 == vertices.Length)
                            {
                                float_index = 0;

                                Vector2[] tex_coords = new Vector2[vertices.Length];

                                for (int i = 0; i < vertices.Length; i++)
                                {
                                    tex_coords[i] = new Vector2(float_data[float_index], float_data[float_index + 1]);
                                    float_index += 2;
                                }

                                mesh.uv = tex_coords;
                            }
                        }

                        if ((texture_units > 1) && geom.GetTexCoordData(out float_data, 1))
                        {
                            if (float_data.Length / 2 == vertices.Length)
                            {
                                float_index = 0;

                                Vector2[] tex_coords = new Vector2[vertices.Length];

                                for (int i = 0; i < vertices.Length; i++)
                                {
                                    tex_coords[i] = new Vector2(float_data[float_index], float_data[float_index + 1]);
                                    float_index += 2;
                                }

                                mesh.uv2 = tex_coords;
                            }
                        }

                        if ((texture_units > 2) && geom.GetTexCoordData(out float_data, 2))
                        {
                            if (float_data.Length / 2 == vertices.Length)
                            {
                                float_index = 0;

                                Vector2[] tex_coords = new Vector2[vertices.Length];

                                for (int i = 0; i < vertices.Length; i++)
                                {
                                    tex_coords[i] = new Vector2(float_data[float_index], float_data[float_index + 1]);
                                    float_index += 2;
                                }

                                mesh.uv3 = tex_coords;
                            }
                        }

                        if ((texture_units > 3) && geom.GetTexCoordData(out float_data, 3))
                        {
                            if (float_data.Length / 2 == vertices.Length)
                            {
                                float_index = 0;

                                Vector2[] tex_coords = new Vector2[vertices.Length];

                                for (int i = 0; i < vertices.Length; i++)
                                {
                                    tex_coords[i] = new Vector2(float_data[float_index], float_data[float_index + 1]);
                                    float_index += 2;
                                }

                                mesh.uv4 = tex_coords;
                            }
                        }
                    }

                    filter.sharedMesh = mesh;

                    renderer.sharedMaterial = currentMaterial;

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

            if (!ResetMap())
            {
                NodeLock.UnLock();
                return false;
            }

            var node = DbManager.LoadDB(mapURL);

            if (node == null || !node.IsValid())
            {
                NodeLock.UnLock();
                return false;
            }

            MapUrl = mapURL;

            _native_scene.AddNode(node);

            _controller.CurrentMap = node;

            _native_scene.Debug();

            _root = new GameObject("root");

            GameObject scene = Traverse(node, null);

            scene.transform.SetParent(_root.transform, false);

            // As GizmoSDK has a flipped Z axis going out of the screen we need a top transform to flip Z
            _root.transform.localScale = new Vector3(1, 1, -1);

            //// Add example object under ROI --------------------------------------------------------------

            //MapPos mapPos;

            //GetMapPosition(new LatPos(1.0084718541, 0.24984267815,300),out mapPos, GroundClampType.GROUND,true);

            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            //sphere.transform.parent = FindFirstGameObjectTransform(mapPos.roiNode);
            //sphere.transform.localPosition = mapPos.position.ToVector3();
            //sphere.transform.localScale = new Vector3(10,10,10);

            //// ------------------------------------------------------------------------------------------

            NodeLock.UnLock();

            return true;
        }

        public bool ResetMap()
        {
            NodeLock.WaitLockEdit();

            foreach (var p in pendingLoaders)
            {
                p.loader.Dispose();
                p.node.Dispose();
            }

            pendingLoaders.Clear();

            foreach (var p in pendingActivations)
            {
                p.node.Dispose();
            }

            pendingActivations.Clear();

            RemoveGameObjectHandles(_root);

            _controller.Reset();

            NodeLock.UnLock();

            return true;
        }

        private void Awake()
        {
            _plugin_initializer = new UnityPluginInitializer();  // in case we need it our own
        }

        public bool Initialize()
        {
            _actionReceiver = new NodeAction("DynamicLoadManager");
            _actionReceiver.OnAction += ActionReceiver_OnAction;

            _zflipMatrix = new Matrix4x4(new Vector4(1, 0, 0), new Vector4(0, 1, 0), new Vector4(0, 0, -1), new Vector4(0, 0, 0, 1));

            GizmoSDK.GizmoBase.Message.Send("SceneManager", MessageLevel.DEBUG,"Loading Graph");

            GizmoSDK.Gizmo3D.Platform.Initialize();

            NodeLock.WaitLockEdit();

            _native_camera = new PerspCamera("Test");
            _native_camera.RoiPosition = true;
            _controller.Camera = _native_camera;

            _native_scene = new Scene("TestScene");

            _native_context = new Context();

            //native_camera.Debug(native_context);

            _native_traverse_action = new CullTraverseAction();

            DynamicLoader.OnDynamicLoad += DynamicLoader_OnDynamicLoad;

            _native_camera.Scene = _native_scene;

            NodeLock.UnLock();

            DbManager.Initialize();

            return true;
        }

        public bool Uninitialize()
        {

            ResetMap();

            DynamicLoader.OnDynamicLoad -= DynamicLoader_OnDynamicLoad;
            _actionReceiver.OnAction -= ActionReceiver_OnAction;

            NodeLock.WaitLockEdit();

            _native_camera.Debug(_native_context, false);
            _native_camera.Dispose();
            _native_camera = null;

            _native_context.Dispose();
            _native_context = null;

            _native_scene.Dispose();
            _native_scene = null;


            _actionReceiver.Dispose();
            _actionReceiver = null;

            NodeLock.UnLock();

            GizmoSDK.Gizmo3D.Platform.Uninitialize();

            _plugin_initializer = null;

            return true;
        }
        

        void Start()
        {
            StartCoroutine(AssetLoader());

            Initialize();
            LoadMap(MapUrl);
        }

        private void ActionReceiver_OnAction(NodeAction sender, NodeActionEvent action, Context context, NodeActionProvider trigger, TraverseAction traverser, IntPtr userdata)
        {
            // Locked in edit or render (render) by caller

            if ((action == NodeActionEvent.IS_TRAVERSABLE) || (action == NodeActionEvent.IS_NOT_TRAVERSABLE))
            {
                pendingActivations.Add(new ActivationInfo(action, trigger as Node));
            }
            else
                trigger?.UnRefNoDelete();      // We are getting ref counts on objects in scene graph and these need to be released immediately

            traverser?.UnRefNoDelete();
            context?.UnRefNoDelete();
        }

        private void OnDestroy()
        {
            Uninitialize();
        }

        private void DynamicLoader_OnDynamicLoad(DynamicLoadingState state, DynamicLoader loader, Node node)
        {
            // Locked in edit or render (render) by caller

            if (state == DynamicLoadingState.LOADED || state == DynamicLoadingState.UNLOADED)
            {
                //if(node != null && node.IsValid())
                //    Message.Send("Unity", MessageLevel.DEBUG, state + " " + node.GetName());
                //else if (loader != null && loader.IsValid())
                //    Message.Send("Unity", MessageLevel.DEBUG, state + " " + loader.GetName());

                pendingLoaders.Add(new NodeLoadInfo(state, loader, node));
            }
            else
            {
                loader?.UnRefNoDelete();      // Same here. We are getting refs to objects in scene graph that we shouldnt release in GC
                node?.UnRefNoDelete();
            }
        }

        private void RemoveGameObjectHandles(GameObject obj)
        {
            if (obj == null)
                return;

            foreach (unTransform child in obj.transform)
            {
                RemoveGameObjectHandles(child.gameObject);
            }

            NodeHandle h = obj.GetComponent<NodeHandle>();

            if (h != null)
            {
                if (h.inObjectDict)
                {
                    RemoveGameObjectReference(h.node.GetNativeReference(), obj);
                    h.inObjectDict = false;
                }

                h.node.Dispose();
                h.node = null;
            }

        }

        #region ---- Map and object position update utilities ---------------------------------------------------------------

        public double GetAltitude(LatPos pos, ClampFlags flags = ClampFlags.DEFAULT)
        {
            return _controller.GetAltitude(pos, flags);
        }

        public bool GetScreenGroundPosition(int x, int y, uint size_x, uint size_y, out MapPos result, ClampFlags flags = ClampFlags.DEFAULT)
        {
            return _controller.GetScreenGroundPosition(x, y, size_x, size_y, out result, flags);
        }

        public bool GetLatPos(MapPos pos, out LatPos latpos)
        {
            return _controller.GetPosition(pos, out latpos);
        }

        public bool GetMapPosition(LatPos latpos, out MapPos pos, GroundClampType groundClamp, ClampFlags flags = ClampFlags.DEFAULT)
        {
            return _controller.GetPosition(latpos, out pos, groundClamp, flags);
        }

        public bool UpdateMapPosition(ref MapPos pos, GroundClampType groundClamp, ClampFlags flags = ClampFlags.DEFAULT)
        {
            // Right now this is trivial as we assume same coordinate system between unity and gizmo but we need a double precision conversion

            return _controller.UpdatePosition(ref pos, groundClamp, flags);
        }

        public Vec3D LocalToWorld(MapPos mappos)
        {
            return _controller.LocalToWorld(mappos);
        }

        public MapPos WorldToLocal(Vec3D position)
        {
            return _controller.WorldToLocal(position);
        }

        #endregion -----------------------------------------------------------------------------------------------

        #region --- Object lookup - Translation between GameObjects and Node ---------------------------

        public unTransform FindFirstGameObjectTransform(Node node)
        {
            List<GameObject> gameObjectList;

            if (!FindGameObjects(node, out gameObjectList))
                return null;

            return gameObjectList[0].transform;
        }

        public bool FindGameObjects(Node node, out List<GameObject> gameObjectList)
        {
            gameObjectList = null;

            if (node == null)
                return false;

            if (!node.IsValid())
                return false;

            NodeLock.WaitLockEdit();

            bool result = currentObjects.TryGetValue(node.GetNativeReference(), out gameObjectList);

            NodeLock.UnLock();

            return result;
        }

        #endregion ------------------------------------------------------------------------------------


        private void ProcessPendingUpdates()
        {
            foreach (NodeLoadInfo nodeLoadInfo in pendingLoaders)
            {
                if (nodeLoadInfo.state == DynamicLoadingState.LOADED)
                {
                    GameObject go = Traverse(nodeLoadInfo.node, null);
                    pendingObjects.Add(new GameObjectInfo(nodeLoadInfo, go));
                }
                else if (nodeLoadInfo.state == DynamicLoadingState.UNLOADED)
                {
                    List<GameObject> gameObjectList;

                    if (currentObjects.TryGetValue(nodeLoadInfo.loader.GetNativeReference(), out gameObjectList))
                    {
                        foreach (GameObject go in gameObjectList)
                        {
                            foreach (unTransform child in go.transform)
                            {
                                RemoveGameObjectHandles(child.gameObject);
                                GameObject.Destroy(child.gameObject);
                            }
                        }
                    }
                    else
                    {
                        Message.Send("Unity", MessageLevel.WARNING, "LOADER Break!!");
                    }
                }

            }

            pendingLoaders.Clear();

            foreach (ActivationInfo activationInfo in pendingActivations)
            {
                List<GameObject> gameObjectList;

                if (currentObjects.TryGetValue(activationInfo.node.GetNativeReference(), out gameObjectList))
                {
                    foreach (GameObject obj in gameObjectList)
                    {
                        if (activationInfo.state == NodeActionEvent.IS_TRAVERSABLE)
                            obj.SetActive(true);
                        else
                            obj.SetActive(false);
                    }
                }
                else
                {
                    Message.Send("Unity", MessageLevel.WARNING, "Activation break!!");
                }
            }

            pendingActivations.Clear();


            foreach (GameObjectInfo go_info in pendingObjects)
            {
                List<GameObject> gameObjectList;

                if (currentObjects.TryGetValue(go_info.nodeInfo.loader.GetNativeReference(), out gameObjectList))
                {
                    bool shared = false;

                    foreach (GameObject go in gameObjectList)
                    {
                        if (!shared)
                            go_info.go.transform.SetParent(go.transform, false);
                        else
                        {
                            GameObject shared_go = Traverse(go_info.nodeInfo.node, null);
                            shared_go.transform.SetParent(go.transform, false);
                        }
                        shared = true;
                    }
                }
                else
                {
                    RemoveGameObjectHandles(go_info.go);
                    GameObject.Destroy(go_info.go);
                }

            }

            pendingObjects.Clear();

            //Message.Send("SceneManager", MessageLevel.ALWAYS, $"currentObjects Size {currentObjects.Count}");

            //Message.Send("SceneManager", MessageLevel.ALWAYS, String.Format("currentObjects Size {0}", currentObjects.Count));


            // Right now we use this as a dirty fix to handle unused shared materials

            _unusedCounter = (_unusedCounter + 1) % FrameCleanupInterval;
            if(_unusedCounter==0)
                Resources.UnloadUnusedAssets();
        }

      
        // Update is called once per frame
        private void Update()
        {
            NodeLock.WaitLockEdit();

            ProcessPendingUpdates();

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

            Matrix4x4 gz_transform = _zflipMatrix * unity_camera_transform * _zflipMatrix;

            _native_camera.Transform = gz_transform.ToMatrix4();

            IWorldCoord ctrl = UnityCamera.GetComponent<IWorldCoord>();

            if (ctrl != null)
                _native_camera.Position = ctrl.Position;

            NodeLock.UnLock();

            NodeLock.WaitLockRender();
            _native_camera.Render(_native_context, 1000, 1000, 1000, _native_traverse_action);
            //native_camera.DebugRefresh();
            NodeLock.UnLock();
        }

    }

}