using Saab.Application.Performance;
using Saab.Foundation.Unity.MapStreamer.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saab.Application.Mapstreamer
{
    public class DefaultTest : ITestScenario
    {
        public string Title => "Clean Test";

        public string Description => "default test to compare results the to";

        public string Settings => throw new NotImplementedException();

        public string InternalResult => throw new NotImplementedException();

        public event Action TestScenarioCompleted;

        public bool IsRunning => _running;

        private List<FeatureSet> _featuresSets;
        private bool _running;


        public bool Initialize()
        {
            var _foliageModule = GameObject.FindObjectOfType<FoliageModule>();
            if (_foliageModule == null)
                return false;

            _featuresSets = _foliageModule.Features;
            foreach (FeatureSet f in _featuresSets)
            {
                f.Enabled = false;
            }
            return true;
        }

        public void StartTest()
        {
            _running = true;
            foreach (FeatureSet f in _featuresSets)
            {
                f.Enabled = false;
            }
        }

        public void StopTest()
        {
            _running = false;
            foreach (FeatureSet f in _featuresSets)
            {
                f.Enabled = false;
            }
            TestScenarioCompleted?.Invoke();
        }

        public void UpdateTest(float dt)
        {
            //throw new NotImplementedException();
        }
    }
}