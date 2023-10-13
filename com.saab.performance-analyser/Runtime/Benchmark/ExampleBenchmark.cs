using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Saab.Application.Performance.Example
{
    public class ExampleBenchmark : MonoBehaviour, IBenchmark
    {
        public TMP_Text TestInfo;
        private List<ITestScenario> _tests = new List<ITestScenario>();
        private List<IProfiler> _profilers = new List<IProfiler>();
        private MemoryProfiler _memoryProfiler;
        private InternalProfiler _internalProfiler;
        private int _currentIndex;
        private bool _running;
        private float _currentTestTime;

        public string Title => "Example Benchmark";

        public string Description => "";

        public float TimeLeft => throw new System.NotImplementedException();

        public float BenchmarkTotalTime => _tests.Count * TestDuration;

        public List<ITestScenario> Tests => _tests;

        public bool Running => _running;

        public float TestDuration = 10;
        private Report _report;

        public void Initialize()
        {
            _memoryProfiler = new Performance.MemoryProfiler();
            _internalProfiler = new InternalProfiler();

            _profilers.Add(_memoryProfiler);
            _profilers.Add(_internalProfiler);

            _report = ReportGenerator.CreateReport(this, _profilers);

            _tests.Add(new SpinningCubesTest());
            _tests.Add(new ParticleTest());

            foreach (var test in _tests)
            {
                if (!test.Initialize())
                    Debug.LogError($"failed to initilize {test.Title} test scenario");

                test.TestScenarioCompleted += Test_TestScenarioCompleted;
            }
        }

        private void EnableProfilers(bool enable)
        {
            if (enable)
            {
                _memoryProfiler.StartBenchmark();
                _internalProfiler.StartBenchmark();
            }
            else
            {
                _memoryProfiler.StopBenchmark();
                _internalProfiler.StopBenchmark();
            }
        }

        private void Test_TestScenarioCompleted()
        {
            EnableProfilers(false);

            _report.AppendToReport(_tests[_currentIndex]);

            if (++_currentIndex > _tests.Count - 1 || !_running)
            {
                Debug.Log("stop benchmark");
                StopBenchmark();
                return;
            }
            EnableProfilers(true);
            _currentTestTime = TestDuration;
            _tests[_currentIndex].StartTest();
        }

        public void StartBenchmark()
        {
            _running = true;
            _currentIndex = 0;
            _currentTestTime = TestDuration;

            EnableProfilers(true);
            _tests[_currentIndex].StartTest();
        }

        public void StopBenchmark()
        {
            ReportGenerator.SaveReport(_report);

            _running = false;
            EnableProfilers(false);

            if (_currentIndex < _tests.Count && _tests[_currentIndex].IsRunning)
                _tests[_currentIndex].StopTest();

            _currentIndex = 0;
        }

        // Start is called before the first frame update
        void Start()
        {
            Initialize();
        }

        // Update is called once per frame
        void Update()
        {
            TestInfo.transform.parent.gameObject.SetActive(_running);

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (!_running)
                    StartBenchmark();
                else
                    StopBenchmark();
            }


            if (_tests[_currentIndex].IsRunning && _running)
            {
                TestInfo.text = $"{_tests[_currentIndex].Title}: {_currentTestTime:f1}\nTest: {_currentIndex+1} / {_tests.Count}";

                var mem = _memoryProfiler.ToString();
                var fps = _internalProfiler.ToString();
                _currentTestTime -= Time.unscaledDeltaTime;

                _tests[_currentIndex].UpdateTest(Time.deltaTime);

                if (_currentTestTime < 0)
                {
                    _tests[_currentIndex].StopTest();
                }
            }
        }
    }
}