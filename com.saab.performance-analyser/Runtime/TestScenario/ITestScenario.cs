using System;

namespace Saab.Application.Performance
{
    public interface ITestScenario
    {
        /// <summary>
        /// event that should be throw on Test Scenario completed
        /// </summary>
        public event Action TestScenarioCompleted;
        /// <summary>
        /// the title of the Test Scenario (should be unique)
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// returns whether or not the test is currently running
        /// </summary>
        public bool IsRunning { get; }
        /// <summary>
        /// Initialization of the TestScenario, read deault settings get required components
        /// </summary>       
        public bool Initialize();
        /// <summary>
        /// an optional description of what the test scenario does 
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// start Test Scenario, should enable/setup requiered modules
        /// </summary>
        public void StartTest();
        /// <summary>
        /// stop Test Scenario, should clenup/revert every thing done one start
        /// </summary>
        public void StopTest();
        /// <summary>
        /// an optional update for the test scenario
        /// </summary>
        /// <param name="dt">delta time</param>
        public void UpdateTest(float dt);
        /// <summary>
        /// get the settings of the correlated settings/modules in the test
        /// e.g. Two FoliageSet are active with Density: X and Y drawdistance: Z and W etc... 
        /// </summary>
        public string Settings { get;  }
        /// <summary>
        /// an optional way to export messuremnt done in the Test Scenario e.g. load times
        /// </summary>
        public string InternalResult { get; }
    }
}