namespace Saab.Application.Performance
{
    public interface IProfiler
    {
        public bool IsRunning { get; set; }
        public void StartBenchmark();
        public void StopBenchmark();
        public bool ToString(out string data);
        public string GetExcel();
    }
}