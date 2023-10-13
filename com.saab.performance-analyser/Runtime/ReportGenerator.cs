using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Saab.Application.Performance
{
    public class Report
    {
        public IBenchmark Benchmark { get; }
        public string Suffix { get; }
        private List<IProfiler> _profilers;

        public string Result { get; private set; }

        private string VsyncInfo()
        {
            string result = string.Empty;

            switch (QualitySettings.vSyncCount)
            {
                case 0:
                    result = $"Vsync: Off";
                    // Default no vsync
                    break;
                default:
                    result = $"Vsync: On ";
                    if (UnityEngine.Application.targetFrameRate == -1)
                        result += $"(Target: {(Screen.currentResolution.refreshRate / QualitySettings.vSyncCount)})";
                    else
                        result += $"(Target: {UnityEngine.Application.targetFrameRate})";
                    break;
            }

            return result;
        }

        public Report(IBenchmark benchmark, List<IProfiler> profilers, string suffix = null)
        {
            this.Benchmark = benchmark;
            this._profilers = profilers;
            this.Suffix = suffix;

            var name = SystemInfo.deviceName;
            var cpu = SystemInfo.processorType;
            var hz = SystemInfo.processorFrequency;
            var ram = SystemInfo.systemMemorySize;
            var gpu = SystemInfo.graphicsDeviceName;
            var vram = SystemInfo.graphicsMemorySize;
            var res = Screen.currentResolution;

            Result = $"******************** {benchmark.Title} ********************\n";
            Result += $"Device: {name}\nCPU: {cpu}\nFrequency: {hz:F2} hz\nRAM: {ram / 1024f:F2} GB\nGPU: {gpu}\nVRAM: {vram / 1024f:F2} GB\nResolution {Screen.width}x{Screen.height} {res.refreshRate} hz\n{VsyncInfo()}\n\n";
        }

        public void AppendToReport(string header)
        {
            var report = header;
            foreach (var profiler in _profilers)
            {
                report += profiler.GetExcel() + "\n";
            }

            Result += report;
        }

        public void AppendToReport(ITestScenario testScenario)
        {
            var header = $"\n******************** {testScenario.Title} ********************\n";
            AppendToReport(header);
        }
    }

    public class ReportGenerator
    {
        public static Report CreateReport(IBenchmark benchmark, List<IProfiler> profilers)
        {
           return new Report(benchmark, profilers);
        }

        public static void SaveReport(Report report, string path = null)
        {
            if(report == null) 
            {
                return;
            }

#if UNITY_ANDROID
        Debug.Log(result);
#else
            var savePath = UnityEngine.Application.dataPath + $"/../Results/{report.Benchmark.Title}/";

            if (path != null)
                savePath = path;

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            DateTime dt = DateTime.Now;

            var suffix = report.Suffix == null ? "" : $"_{report.Suffix}";
            StreamWriter writer = new StreamWriter(savePath + $"{report.Benchmark.Title}{suffix}_{dt.ToString("yy-MM-dd-hh-mm")}.txt", true);
            writer.WriteLine(report.Result);
            writer.Close();
#endif
        }

        public static void SaveReport(string FileName, string result, string path)
        {
#if UNITY_ANDROID
        Debug.Log(result);
#else
            var savePath = UnityEngine.Application.dataPath + $"/../Results/";

            if (path != null)
                savePath = path;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            DateTime dt = DateTime.Now;
            StreamWriter writer = new StreamWriter(path + $"{FileName}_{dt.ToString("yy-MM-dd-hh-mm")}.txt", true);
            writer.WriteLine(result);
            writer.Close();
#endif
        }
    }
}