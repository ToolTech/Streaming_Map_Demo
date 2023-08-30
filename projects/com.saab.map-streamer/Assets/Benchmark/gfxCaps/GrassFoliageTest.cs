using Saab.Application.Performance;
using Saab.Foundation.Unity.MapStreamer.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saab.Application.Mapstreamer
{
    public class GrassFoliageTest : ITestScenario
    {
        public event Action TestScenarioCompleted;

        public string Title => "Grass Foliage";
        public string Description => "enables Grass Foliage with settings";

        public string InternalResult => throw new NotImplementedException();
        public string Settings => throw new NotImplementedException();

        public bool IsRunning => _running;

        private FoliageModule _foliageModule;
        private List<FeatureSet> _grassFeatures;
        private bool _running;

        public bool Initialize()
        {
            _foliageModule = GameObject.FindObjectOfType<FoliageModule>();
            if( _foliageModule == null ) 
                return false;

            _grassFeatures = _foliageModule.Features.FindAll(f => f.SettingsType == Utility.GfxCaps.SettingsFeatureType.Grass);

            if( _grassFeatures == null || _grassFeatures.Count == 0)
                return false;

            foreach( FeatureSet f in _grassFeatures ) 
            {
                f.Enabled = false;
            }

            return true;
        }

        private void EnableSettings(bool enabled)
        {
            _running = enabled;
            foreach (FeatureSet f in _grassFeatures)
            {
                f.Enabled = enabled;
            }
        }

        public void StartTest()
        {
            EnableSettings(true);
        }

        public void StopTest()
        {
            EnableSettings(false);
            TestScenarioCompleted?.Invoke();
        }

        public void UpdateTest(float dt)
        {

        }
    }
}