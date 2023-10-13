using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Profiling;
using UnityEngine;

namespace Saab.Application.Performance
{
    public class MemoryProfiler : IProfiler
    {
        private ProfilerRecorder _systemMemoryRecorder;
        private ProfilerRecorder _totalMemoryRecorder;
        private ProfilerRecorder _totalMemoryRecorderReserved;
        private ProfilerRecorder _GpuMemoryRecorder;
        private ProfilerRecorder _GpuMemoryRecorderReserved;
        private ProfilerRecorder _gcMemoryRecorder;
        private ProfilerRecorder _gcMemoryRecorderReserved;

        private string _system, _mem, _memReserved, _memgpu, _memgpuReserved, _gc, _gcReserved;
        private bool _running = false;

        public bool IsRunning { get => _running; set => _running = value; }

        public MemoryProfiler(int sampleFrames = 50)
        {
            SampleFrames = sampleFrames;
        }

        public int SampleFrames
        {
            get; private set;
        }

        public void StartBenchmark()
        {
            if (IsRunning)
            {
                Debug.LogWarning("MemoryBenchmark is already running");
                return;
            }

            IsRunning = true;

            _system = _mem = _memReserved = _memgpu = _memgpuReserved = _gc = _gcReserved = "";

            _systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");

            _totalMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            _totalMemoryRecorderReserved = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");

            _GpuMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory");
            _GpuMemoryRecorderReserved = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Reserved Memory");

            _gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
            _gcMemoryRecorderReserved = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        }
        public void StopBenchmark()
        {
            if (!IsRunning)
            {
                Debug.LogWarning("MemoryBenchmark is not running");
                return;
            }
            IsRunning = false;

            _systemMemoryRecorder.Dispose();

            _totalMemoryRecorder.Dispose();
            _totalMemoryRecorderReserved.Dispose();

            _GpuMemoryRecorder.Dispose();
            _GpuMemoryRecorderReserved.Dispose();

            _gcMemoryRecorder.Dispose();
            _gcMemoryRecorderReserved.Dispose();
        }

        private double ByteToMB(double bytes)
        {
            return bytes / (1024.0 * 1024.0);
        }
        public string ToString(bool showInMs = true, bool update = false)
        {
            if (update)
                UpdateProfiler();

            var system = $"System: {ByteToMB(_systemMemoryRecorder.LastValue):F0} MB\n";
            var totalmem = $"Ram: {ByteToMB(_totalMemoryRecorder.LastValue):F0} / {ByteToMB(_totalMemoryRecorderReserved.LastValue):F0} MB\n";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var vram = $"Vram: {ByteToMB(_GpuMemoryRecorder.LastValue):F0} / {ByteToMB(_GpuMemoryRecorderReserved.LastValue):F0} MB\n";
#else
        var vram = "";
#endif
            var gc = $"GC: {ByteToMB(_gcMemoryRecorder.LastValue):F1} / {ByteToMB(_gcMemoryRecorderReserved.LastValue):F1} MB\n";

            var data = $"<b>Memory:</b>\n{system}{totalmem}{vram}{gc}";


            _system += $"{ByteToMB(_systemMemoryRecorder.LastValue):F0}\t";

            _mem += $"{ByteToMB(_totalMemoryRecorder.LastValue):F0}\t";
            _memReserved += $"{ByteToMB(_totalMemoryRecorderReserved.LastValue):F0}\t";

            _memgpu += $"{ByteToMB(_GpuMemoryRecorder.LastValue):F0}\t";
            _memgpuReserved += $"{ByteToMB(_GpuMemoryRecorderReserved.LastValue):F0}\t";

            _gc += $"{ByteToMB(_gcMemoryRecorder.LastValue):F0}\t";
            _gcReserved += $"{ByteToMB(_gcMemoryRecorderReserved.LastValue):F0}\t";

            return data;
        }

        public string GetExcel()
        {
            string excel = $"System Memory:\t{_system}\nMemory:\t{_mem}\nMemory Reserved:\t{_memReserved}\nMemory GPU:\t{_memgpu}\nMemory GPU Reserved:\t{_memgpuReserved}\nGC:\t{_gc}\nGC Reserved:\t{_gcReserved}";
            return excel;
        }

        public void UpdateProfiler()
        {
            //throw new System.NotImplementedException();
        }
    }
}