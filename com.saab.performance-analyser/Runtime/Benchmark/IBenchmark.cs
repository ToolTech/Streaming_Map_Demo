using System.Collections.Generic;

namespace Saab.Application.Performance
{
    public interface IBenchmark
    {
        /// <summary>
        /// the title of the Test Scenario (should be unique)
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// an optional description of what the test scenario does 
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// the (maximum) amount of seconds left of the benchmark
        /// </summary>
        public float TimeLeft { get; }
        /// <summary>
        /// Initialization of the Benchmark, read deault settings get required components
        /// </summary>
        public void Initialize();
        /// <summary>
        /// the (maximum) amount of seconds for runing the entire benchmark (all the test scenario)
        /// </summary>
        public float BenchmarkTotalTime { get; }
        /// <summary>
        /// a list of all the test that is run by the benchmark
        /// </summary>
        public List<ITestScenario> Tests { get; }
        /// <summary>
        /// Start the benchmark
        /// </summary>
        public void StartBenchmark();
        /// <summary>
        /// Stop the benchmark
        /// </summary>
        public void StopBenchmark();
    }
}