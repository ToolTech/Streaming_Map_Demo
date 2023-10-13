using System;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;

namespace Saab.Application.Performance
{
    public class GuiStats : MonoBehaviour
    {
        public bool ShowSystemStats;
        public bool ShowFrameStats;
        public bool ShowMemoryStats;

        public TMP_Text SystemStats;
        public TMP_Text FrameStats;

        private MemoryProfiler _memoryProfiler;
        private InternalProfiler _internalProfiler;
        private Resolution _res;

        [Range(1f, 30f)]
        public float UpdatesPerSecond = 2f;
        private float nextUpdateTime;

        public bool ShowInMs;

        // Start is called before the first frame update
        void Start()
        {
            UpdateStats();

            var target = UnityEngine.Application.targetFrameRate <= 0 ? 120 : UnityEngine.Application.targetFrameRate;

            _memoryProfiler = new MemoryProfiler();
            _internalProfiler = new InternalProfiler(Mathf.CeilToInt(target / UpdatesPerSecond));
            nextUpdateTime = Time.time;

        }

        private void UpdateStats()
        {
            var name = SystemInfo.deviceName;
            var cpu = SystemInfo.processorType;
            var hz = SystemInfo.processorFrequency;
            var ram = SystemInfo.systemMemorySize;
            var gpu = SystemInfo.graphicsDeviceName;
            var vram = SystemInfo.graphicsMemorySize;
            _res = Screen.currentResolution;

            SystemStats.text = $"Device: {name}\nCPU: {cpu}\nFrequency: {hz:F2} hz\nRAM: {ram / 1024f:F2} GB\nGPU: {gpu}\nVRAM: {vram / 1024f:F2} GB\nResolution {Screen.width}x{Screen.height} {_res.refreshRate} hz\n{VsyncInfo()}";
        }
        private string VsyncInfo()
        {
            string result = string.Empty;

            switch (QualitySettings.vSyncCount)
            {
                case 0:
                    result = $"Vsync: Off ";
                    if (UnityEngine.Application.targetFrameRate == -1)
                        result += $"(Target: no limit)";
                    else
                        result += $"(Target: {UnityEngine.Application.targetFrameRate})"; 
                    break;
                default:
                    result = $"Vsync: On ";
                    result += $"(Target: {(Screen.currentResolution.refreshRate / QualitySettings.vSyncCount)})";
                    break;
            }

            return result;
        }

        private void DisplaySystemStats(bool show)
        {

            SystemStats.transform.parent.gameObject.SetActive(show);
        }

        private void DisplayFrameStats(bool show)
        {
            FrameStats.transform.parent.gameObject.SetActive(show);

            if (show)
            {
                if (!_memoryProfiler.IsRunning && ShowMemoryStats)
                    _memoryProfiler.StartBenchmark();

                if (!_internalProfiler.IsRunning && ShowFrameStats)
                    _internalProfiler.StartBenchmark();

                var result = "";
                if (ShowFrameStats)
                    result = _internalProfiler.ToString(ShowInMs);
                var displayText = result;

                if (ShowMemoryStats)
                {
                    result = _memoryProfiler.ToString();
                    displayText += result;
                }
                FrameStats.text = displayText;
            }
            else
            {
                if (_memoryProfiler.IsRunning)
                    _memoryProfiler.StopBenchmark();
                if (_internalProfiler.IsRunning)
                    _internalProfiler.StopBenchmark();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time >= nextUpdateTime)
            {
                DisplayFrameStats(ShowFrameStats || ShowMemoryStats);
                DisplaySystemStats(ShowSystemStats);

                nextUpdateTime = Time.time + (1f / UpdatesPerSecond);

                if(_res.width != Screen.width || _res.height!= Screen.height)
                {
                    UpdateStats();
                }
            }
        }
    }
}