using Saab.Application.Performance;
using Saab.Foundation.Unity.MapStreamer.Modules;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saab.Application.Mapstreamer
{
    public class TreeFoliageTest : ITestScenario
    {
        public event Action TestScenarioCompleted;

        public string Title => "Tree Foliage";
        public string Description => "enables Tree Foliage with settings";

        public string InternalResult => throw new NotImplementedException();
        public string Settings => throw new NotImplementedException();

        public bool IsRunning => _running;

        private FoliageModule _foliageModule;
        private List<FeatureSet> _treeFeatures;
        private bool _running = false;

        public bool Initialize()
        {
            _foliageModule = GameObject.FindObjectOfType<FoliageModule>();
            if( _foliageModule == null ) 
                return false;

            _treeFeatures = _foliageModule.Features.FindAll(f => f.SettingsType == Utility.GfxCaps.SettingsFeatureType.Trees);

            if( _treeFeatures == null || _treeFeatures.Count == 0)
                return false;

            foreach( FeatureSet f in _treeFeatures ) 
            {
                f.Enabled = false;
            }

            return true;
        }
        private void EnableSettings(bool enabled)
        {
            _running = enabled;
            foreach (FeatureSet f in _treeFeatures)
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