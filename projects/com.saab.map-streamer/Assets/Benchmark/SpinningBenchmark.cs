using Saab.Foundation.Unity.MapStreamer;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Saab.Application.Performance;
using UnityEngine.Profiling;

namespace Saab.Application.Mapstreamer
{
    public class SpinningBenchmark : MonoBehaviour, IBenchmark
    {
        public TMP_Text TestInfo;
        public CameraControl CameraControl;
        public float TestDuration = 10.0f;
        public string Title => "SpinningBenchmark";
        public string Description => "camera is spinning around it's axle";

        private float _countDown = 0;
        private float _currentTestTime = 0;
        public float TimeLeft => _countDown;
        public float BenchmarkTotalTime => TestDuration * _tests.Count;

        public List<ITestScenario> Tests => _tests;

        public bool Running => throw new System.NotImplementedException();

        private List<ITestScenario> _tests = new List<ITestScenario>();
        private List<IProfiler> _profilers = new List<IProfiler>();

        private Performance.MemoryProfiler _memoryProfiler;
        private InternalProfiler _internalProfiler;

        private int _currentIndex = 0;
        private bool _running = false;
        private Report _report;

        public void Initialize()
        {
            _report = ReportGenerator.CreateReport(this, _profilers);

            foreach (var test in _tests)
            {
                if (!test.Initialize())
                    Debug.LogWarning($"failed to initilize {test.Title} test scenario");

                test.TestScenarioCompleted += Test_TestScenarioCompleted;
            }
        }

        private void Test_TestScenarioCompleted()
        {
            EnableProfilers(false);
            _report.AppendToReport(_tests[_currentIndex]);

            if (++_currentIndex > _tests.Count - 1 || !_running)
            {
                StopBenchmark();
                return;
            }

            EnableProfilers(true);
            _currentTestTime = TestDuration;
            _tests[_currentIndex].StartTest();
        }

        private void EnableProfilers(bool enable)
        {
            if(enable)
            {
                _memoryProfiler.StartBenchmark();
                _internalProfiler.StartBenchmark();

                CameraControl.transform.eulerAngles = Vector3.zero;

                CameraControl.X = 0;
                CameraControl.Y = 20;
                CameraControl.Z = 0;
            }
            else 
            {
                CameraControl.X = 0;
                CameraControl.Y = 200;
                CameraControl.Z = 0;

                CameraControl.UpdateMoveCamera(0, 0, 0, 0, 0, false);

                _memoryProfiler.StopBenchmark();
                _internalProfiler.StopBenchmark();
            }    
        }

        public void StartBenchmark()
        {

            TestInfo.transform.parent.gameObject.SetActive(true);

            _running = true;
            _currentIndex = 0;
            _currentTestTime = TestDuration;
            _countDown = BenchmarkTotalTime;

            EnableProfilers(true);
            _tests[_currentIndex].StartTest();
        }

        public void StopBenchmark()
        {
            ReportGenerator.SaveReport(_report);
            TestInfo.transform.parent.gameObject.SetActive(false);
            _running = false;

            if (_currentIndex < _tests.Count && _tests[_currentIndex].IsRunning)
                _tests[_currentIndex].StopTest();

            _currentIndex = 0;
            EnableProfilers(false);
        }

        public void OnDestroy()
        {
            StopBenchmark();
        }

        public void Start()
        {
            TestInfo.transform.parent.gameObject.SetActive(false);

            _memoryProfiler = new Performance.MemoryProfiler();
            _internalProfiler = new InternalProfiler();

            _profilers.Add(_memoryProfiler);
            _profilers.Add(_internalProfiler);

            var defaultTest = new DefaultTest();
            var treeFoliage = new TreeFoliageTest();
            var grassFoliage = new GrassFoliageTest();

            _tests.Add(defaultTest);
            _tests.Add(treeFoliage);
            _tests.Add(grassFoliage);

            Initialize();
        }

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                if(!_running)
                    StartBenchmark();
                else
                    StopBenchmark();
            }
                

            if (_tests[_currentIndex].IsRunning && _running)
            {
                TestInfo.text = $"{_tests[_currentIndex].Title}: {_currentTestTime:f1}\n{_countDown:f0} / {BenchmarkTotalTime:f0}";

                CameraControl.UpdateMoveCamera(150, 0, 0, 10, 0);

                var mem = _memoryProfiler.ToString();
                var fps = _internalProfiler.ToString();

                _countDown -= Time.unscaledDeltaTime;
                _currentTestTime -= Time.unscaledDeltaTime;

                if(_currentTestTime < 0)
                {
                    _tests[_currentIndex].StopTest();
                }
            }     
        }
    }
}