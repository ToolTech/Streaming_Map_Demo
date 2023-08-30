using System;
using TMPro;
using UnityEngine;

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

        // Start is called before the first frame update
        void Start()
        {
            var name = SystemInfo.deviceName;
            var cpu = SystemInfo.processorType;
            var hz = SystemInfo.processorFrequency;
            var ram = SystemInfo.systemMemorySize;
            var gpu = SystemInfo.graphicsDeviceName;
            var vram = SystemInfo.graphicsMemorySize;
            var res = Screen.currentResolution;
            SystemStats.text = $"Device: {name}\nCPU: {cpu}\nFrequency: {hz:F2} hz\nRAM: {ram / 1024f:F2} GB\nGPU: {gpu}\nVRAM: {vram / 1024f:F2} GB\nResolution {Screen.width}x{Screen.height} {res.refreshRate} hz";

            _memoryProfiler = new MemoryProfiler();
            _internalProfiler = new InternalProfiler();
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
                    _internalProfiler.ToString(out result);
                var displayText = result;

                if (ShowMemoryStats)
                {
                    _memoryProfiler.ToString(out result);
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
            DisplayFrameStats(ShowFrameStats || ShowMemoryStats);
            DisplaySystemStats(ShowSystemStats);
        }
    }
}