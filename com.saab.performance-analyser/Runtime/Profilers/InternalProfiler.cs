using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Saab.Application.Performance
{
    public struct FrameStats
    {
        public double fps;
        public double max;
        public double min;
    }

    public class InternalProfiler : IProfiler
    {
        private ProfilerRecorder _mainThreadTimeRecorder;
        private ProfilerRecorder _gpuTimeRecorder;
        private ProfilerRecorder _behaviourUpdateRecorder;
        private ProfilerRecorder _gcCollectRecorder;

        private bool _running = false;
        private FrameStats _stats = new FrameStats();

        public bool IsRunning { get => _running; set => _running = value; }
        private string _update, _max, _min, _render, _behaviour, _gc;

        public InternalProfiler(int sampleFrames = 50)
        {
            SampleFrames = sampleFrames;
        }

        public int SampleFrames
        {
            get; private set;
        }

        public FrameStats Frame
        {
            get; private set;
        }
        public FrameStats Render
        {
            get; private set;
        }
        public FrameStats Behaviour
        {
            get; private set;
        }
        public FrameStats Garbage
        {
            get; private set;
        }

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

                    if (r > double.MaxValue - sample)
                        Debug.LogError($"the sample is larger then a double... (at sample count {i})");

                    r += sample;
                }
                r /= samplesCount;
            }

            return r;
        }

        private string PrintFps(double value, bool showInMs = true, bool showUnit = true)
        {
            string result = string.Empty;
            if (showInMs)
            {
                result = $"{value * 1e-6f:F2}";
                if (showUnit)
                    result += " ms";
            }
            else
            {
                result = $"{1000 / (value * 1e-6f):F0}";
                if (showUnit)
                    result += " fps";
            }

            return result;
        }

        public string ToString(bool showInMs = true, bool update = true)
        {
            if (update)
                UpdateProfiler();

            var fpsText = $"Update: {PrintFps(Frame.fps, showInMs)}\nWorst: {PrintFps(Frame.max, showInMs)}\nBest: {PrintFps(Frame.min, showInMs)}\n";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var renderText = $"render: {PrintFps(Render.fps)}\n";
            var behaviourText = $"Behaviour: {PrintFps(Behaviour.fps)}\n";
#else
        var renderText = "";
        var behaviourText = "";
#endif

            return $"<b>Update:</b>\n{fpsText}{renderText}{behaviourText}";
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

            _mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", SampleFrames);
            _gpuTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Gfx.WaitForPresentOnGfxThread", SampleFrames);
            _behaviourUpdateRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "BehaviourUpdate", SampleFrames);
            _gcCollectRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GC.Collect", SampleFrames);
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

        public void UpdateProfiler()
        {
            var fps = GetRecorderFrameAverage(_mainThreadTimeRecorder, out var max, out var min);
            _stats.fps = fps;
            _stats.max = max;
            _stats.min = min;
            Frame = _stats;
            var render = GetRecorderFrameAverage(_gpuTimeRecorder, out var renderMax, out var renderMin);
            _stats.fps = render;
            _stats.max = renderMax;
            _stats.min = renderMin;
            Render = _stats;
            var behaviour = GetRecorderFrameAverage(_behaviourUpdateRecorder, out var behaviourMax, out var behaviourMin);
            _stats.fps = behaviour;
            _stats.max = behaviourMax;
            _stats.min = behaviourMin;
            Behaviour = _stats;
            var gc = GetRecorderFrameAverage(_gcCollectRecorder, out var gcMax, out var gcMin);
            _stats.fps = gc;
            _stats.max = gcMax;
            _stats.min = gcMin;
            Garbage = _stats;

            _update += $"{PrintFps(Frame.fps, showUnit: false)}\t";
            _max += $"{PrintFps(Frame.max, showUnit: false)}\t";
            _min += $"{PrintFps(Frame.min, showUnit: false)}\t";
            _render += $"{PrintFps(Render.max, showUnit: false)}\t";
            _behaviour += $"{PrintFps(Behaviour.max, showUnit: false)}\t";
            _gc += $"{PrintFps(Garbage.max, showUnit: false)}\t";
        }
    }
}
