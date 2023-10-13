namespace Saab.Application.Performance
{
    public interface IProfiler
    {
        public bool IsRunning { get; set; }
        public void StartBenchmark();
        public void StopBenchmark();
        public void UpdateProfiler();
        public string ToString(bool showInMs = true, bool update = false);
        public string GetExcel();
    }
}