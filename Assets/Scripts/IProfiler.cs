using TMPro;

public interface IProfiler
{
    public TMP_Text Text { get; }
    public bool IsRunning { get; set; }
    public void StartBenchmark();
    public void StopBenchmark();
    public bool ToString(out string data);
    public string GetExcel();
}
