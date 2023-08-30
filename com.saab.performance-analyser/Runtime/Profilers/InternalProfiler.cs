using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Profiling;
using UnityEngine;

namespace Saab.Application.Performance
{
    public class InternalProfiler : IProfiler
    {
        private ProfilerRecorder _mainThreadTimeRecorder;
        private ProfilerRecorder _gpuTimeRecorder;
        private ProfilerRecorder _behaviourUpdateRecorder;
        private ProfilerRecorder _gcCollectRecorder;

        private bool _running = false;

        public bool IsRunning { get => _running; set => _running = value; }
        private string _update, _max, _min, _render, _behaviour, _gc;

        double GetRecorderFrameAverage(ProfilerRecorder recorder, out double max, out double min)
        {
            max = 0;
            min = double.MaxValue;

            var samplesCount = recorder.Capacity;
            if (samplesCount == 0)
                return 0;

            double r = 0;
            unsafe
            {
                var samples = stackalloc ProfilerRecorderSample[samplesCount];
                recorder.CopyTo(samples, samplesCount);
                for (var i = 0; i < samplesCount; ++i)
                {
                    var sample = samples[i].Value;
                    max = Math.Max(max, sample);
                    min = Math.Min(min, sample);
                    r += sample;
                }
                r /= samplesCount;
            }

            return r;
        }
        public bool ToString(out string data)
        {
            var fps = GetRecorderFrameAverage(_mainThreadTimeRecorder, out var max, out var min);
            var render = GetRecorderFrameAverage(_gpuTimeRecorder, out var renderMax, out var renderMin);
            var behaviour = GetRecorderFrameAverage(_behaviourUpdateRecorder, out var behaviourMax, out var behaviourMin);
            var gc = GetRecorderFrameAverage(_gcCollectRecorder, out var gcMax, out var gcMin);

            var fpsText = $"Update: {fps * 1e-6f:F2} ms\nWorst: {max * 1e-6f:F2} ms\nBest: {min * 1e-6f:F2} ms\n";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var renderText = $"render: {render * 1e-6f:F2} ms\n";
            var behaviourText = $"Behaviour: {behaviour * 1e-6f:F2} ms\n";
#else
        var renderText = "";
        var behaviourText = "";
#endif

            _update += $"{fps * 1e-6f:F2}\t";
            _max += $"{max * 1e-6f:F2}\t";
            _min += $"{min * 1e-6f:F2}\t";
            _render += $"{renderMax * 1e-6f:F2}\t";
            _behaviour += $"{behaviourMax * 1e-6f:F2}\t";
            _gc += $"{gcMax * 1e-6f:F2}\t";

            data = $"<b>Update:</b>\n{fpsText}{renderText}{behaviourText}";
            return true;
        }
        public void StartBenchmark()
        {
            if (IsRunning)
            {
                Debug.LogWarning("FrameStats is already running");
                return;
            }
            IsRunning = true;

            _update = _max = _min = _render = _behaviour = _gc = "";

            _mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 50);
            _gpuTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Gfx.WaitForPresentOnGfxThread", 50);
            _behaviourUpdateRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "BehaviourUpdate", 50);
            _gcCollectRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GC.Collect", 50);
        }
        public void StopBenchmark()
        {
            if (!IsRunning)
            {
                Debug.LogWarning("FrameStats is not running");
                return;
            }
            IsRunning = false;

            _mainThreadTimeRecorder.Dispose();
            _gpuTimeRecorder.Dispose();
            _behaviourUpdateRecorder.Dispose();
            _gcCollectRecorder.Dispose();
        }
        public string GetExcel()
        {
            string excel = $"Update avg:\t{_update}\nUpdate Worst:\t{_max}\nGFX_wait Worst:\t{_render}\nBehaviour Worst:\t{_behaviour}\nGC Collect Worst:\t{_gc}\n";
            return excel;
        }
    }
}
