using Saab.Foundation.Unity.MapStreamer;
using Saab.Foundation.Unity.MapStreamer.Modules;
using System.Linq;
using TMPro;
using UnityEngine;
using System;
using System.IO;
using Time = UnityEngine.Time;
using System.Text.RegularExpressions;
using GizmoSDK.GizmoBase;
using UnityEngine.Rendering.PostProcessing;

public class Benchmark : MonoBehaviour
{
    public TMP_Text StatsText;
    public TMP_Text TimeText;
    public CameraControl CameraControl;
    public float UpdateRate = 1f;
    public float TestTime = 10f;

    public MemoryProfiler MemoryProfiler;
    public InternalProfiler InternalProfiler;

    public SceneManager SceneManager;
    public FoliageModule FoliageModule;
    public TerrainModule TerrainModule;
    public ParticalModule ParticalModule;
    public PostProcessVolume Volume;

    private float _currentTime = 0f;
    private float _currentRate = 0f;
    private int _runId = 0;
    private Transform _parent;

    private bool _running;
    private int _autoSpeedMultipler;
    private string _SystemStats;
    private string _startDate;
    private string _title = "None";
    private string _gfxCaps;
    private RainEffect _rainEffect;
    private FogEffect _fogEffect;
    private ColorGrading _colorGrading;

    private void Start()
    {
        var name = SystemInfo.deviceName;
        var cpu = SystemInfo.processorType;
        var hz = SystemInfo.processorFrequency;
        var ram = SystemInfo.systemMemorySize;
        var gpu = SystemInfo.graphicsDeviceName;
        var vram = SystemInfo.graphicsMemorySize;
        var res = Screen.currentResolution;
        _SystemStats = $"Device: {name}\nCPU: {cpu}\nFrequency: {hz:F2} hz\nRAM: {ram / 1024f:F2} GB\nGPU: {gpu}\nVRAM: {vram / 1024f:F2} GB\nResolution {Screen.width}x{Screen.height} {res.refreshRate} hz";

#if UNITY_ANDROID
        // get defult settings (not reading config) 
        _gfxCaps = "";
#else
        string config = File.ReadAllText(Application.dataPath + "/../config.xml");
        GetSettings(config, out var result);
        _gfxCaps = result;
#endif

        _parent = StatsText.GetComponentsInParent<Canvas>().FirstOrDefault().transform;
        _parent.gameObject.SetActive(false);

        Volume.profile.TryGetSettings<FogEffect>(out _fogEffect);
        Volume.profile.TryGetSettings<RainEffect>(out _rainEffect);
        Volume.profile.TryGetSettings<ColorGrading>(out _colorGrading);
    }

    private bool GetSettings(string input, out string output)
    {
        string pattern = "(<GfxCaps>[\\s\\S]*?<\\/GfxCaps>)";
        output = "";
        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            output = match.Groups[1].Value;
            return true;
        }
        return false;
    }

    private void Update()
    {
        if (_currentTime > TestTime && _running)
        {
            StopBenchmark();
            if (RunThroughGFXCaps(_runId++))
            {
                StartBenchmark();
            }
            else
                _runId = 0;
        }           
        else if(_running)
        {
            _currentTime += Time.deltaTime;
            TimeText.text = $"{_currentTime:F2} / {TestTime} sec";
        }
            

        if (Input.GetKeyDown(KeyCode.F1))
            _autoSpeedMultipler = 1;
        if (Input.GetKeyDown(KeyCode.F2))
            _autoSpeedMultipler = 2;
        if (Input.GetKeyDown(KeyCode.F3))
            _autoSpeedMultipler = 4;
        if (Input.GetKeyDown(KeyCode.F4))
            _autoSpeedMultipler = 8;

        if ((Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.R)) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
        {
            if (_running)
                StopBenchmark();
            else
            {
                TerrainModule.EnableTrees = false;
                TerrainModule.EnableGrass = false;
                FoliageModule.Disabled = true;

                DateTime dt = DateTime.Now;

                _startDate = dt.ToString("yy-MM-dd-hh-mm");
                SaveFile("Result", "******************** Computer hardware ********************\n" + _SystemStats 
                    + "\n\n******************** GFX Settings ********************\n" + _gfxCaps + "\n\n******************** Map ********************\nMap Url:" + SceneManager.MapUrl);

                StartBenchmark();
            }             
        }


        if (_running)
        {
            RunBenchmark();
            UpdateFps();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

    }

    private void OnDestroy()
    {
        StopBenchmark();
    }
    private void StopBenchmark()
    {
        if (_running)
        {
            MemoryProfiler.StopBenchmark();
            InternalProfiler.StopBenchmark();

            SaveFile("Result", $"\n\n ******************** {_title} ********************\n" 
                + InternalProfiler.GetExcel() + "\n" + MemoryProfiler.GetExcel() + "\n");
        }

        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;

        _running = false;
        _parent.gameObject.SetActive(false);
        CameraControl.UpdateMoveCamera(0, 0, 0, 0, 0, false);
    }
    private void StartBenchmark()
    {
        MemoryProfiler.StartBenchmark();
        InternalProfiler.StartBenchmark();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 2000;

        // clean up
        _currentTime = 0f;

        _running = true;
        _autoSpeedMultipler = 1;
        _parent.gameObject.SetActive(true);

        var north = CameraControl.North;
        var rot = CameraControl.Camera.transform.rotation;
        rot.SetLookRotation(north);
        CameraControl.Camera.transform.rotation = rot;

        CameraControl.X = 0;
        CameraControl.Y = 70;
        CameraControl.Z = 0;
        _currentRate = 0;
    }
    private void SaveFile(string name, string result)
    {
#if UNITY_ANDROID
        Debug.Log(result);
#else
        var path = Application.dataPath + "/../Results/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
   
        StreamWriter writer = new StreamWriter(path+$"{name}_{_startDate}.txt", true);
        writer.WriteLine(result);
        writer.Close();
#endif
    }
    private void RunBenchmark()
    {
        CameraControl.UpdateMoveCamera(200 * _autoSpeedMultipler, 0, 0, 25, 0);
    }
    private void UpdateFps()
    {
        if(_currentRate > UpdateRate)
        {
            UpdateStats();
            _currentRate = 0;
        }
        _currentRate += Time.deltaTime;
    }
    private void UpdateStats()
    {
       StatsText.text = _SystemStats;
        string result = "";
        if (MemoryProfiler.ToString(out result))
            MemoryProfiler.Text.text = result;
        if (InternalProfiler.ToString(out result))
            InternalProfiler.Text.text = result;
    }
    private bool RunThroughGFXCaps(int RunId)
    {
        switch(RunId)
        {
            case 0:
                _title = "TerrainModule Tree";
                TerrainModule.EnableTrees = true;
                break;
            case 1:
                _title = "TerrainModule Grass";
                TerrainModule.EnableTrees = false;
                TerrainModule.EnableGrass = true;
                break;
            case 2:
                _title = "FoliageModule Tree";
                TerrainModule.EnableGrass = false;
                FoliageModule.Disabled = false;
                break;
            case 3:
                _title = "ParticalModule";
                TerrainModule.EnableTrees = false;
                TerrainModule.EnableGrass = false;
                FoliageModule.Disabled = true;
                ParticalModule.EnabledFlak = true;
                break;
            case 4:
                _title = "PostProcessing";
                _fogEffect.active = true;
                _rainEffect.active = true;
                _colorGrading.active = true;
                ParticalModule.EnabledFlak = false;
                TerrainModule.EnableTrees = false;
                TerrainModule.EnableGrass = false;
                FoliageModule.Disabled = true;   
                break;
            case 5:
                _title = "None";
                _fogEffect.active = false;
                _rainEffect.active = false;
                _colorGrading.active = false;
                return false;
        }
        return true;
    }
}
