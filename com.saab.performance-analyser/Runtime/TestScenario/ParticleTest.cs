using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Saab.Application.Performance.Example
{
    public class ParticleTest : ITestScenario
    {
        private bool _running;
        private GameObject _parent;
        private int _count =  100;
        private List<Transform> _particals = new List<Transform>();

        public string Title => "Particles";

        public bool IsRunning => _running;

        public string Description => "spawns 100 random particle effects";

        public string Settings => throw new NotImplementedException();

        public string InternalResult => throw new NotImplementedException();

        public event Action TestScenarioCompleted;

        public bool Initialize()
        {
            _parent = new GameObject("ParticleSystem");

            return true;
        }

        public Transform CreateParticle()
        {
            var gameObject = new GameObject();
            gameObject.transform.SetParent(_parent.transform);

            // Create a new Particle System
            var particleSystem = gameObject.AddComponent<ParticleSystem>();

            // Set the default particle material
            var particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            particleRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

            // Configure the Particle System with randomized parameters
            var main = particleSystem.main;
            main.startColor = new Color(Random.value, Random.value, Random.value, 1.0f); // Random color
            main.startSize = Random.Range(0.2f, 1.0f); // Random start size between 0.2 and 1
            main.startLifetime = Random.Range(1.0f, 5.0f); // Random lifetime between 1 and 5 seconds
            main.duration = Random.Range(1.0f, 5.0f); // Random duration between 1 and 5 seconds
            main.loop = true;

            var emission = particleSystem.emission;
            emission.rateOverTime = Random.Range(5, 20); // Random emission rate between 5 and 20

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = Random.Range(10, 45); // Random angle between 10 and 45 degrees
            shape.radius = Random.Range(0.5f, 2.0f); // Random radius between 0.5 and 2

            // Start the Particle System
            particleSystem.Play();

            return gameObject.transform;
        }

        public void StartTest()
        {
            _running = true;
            for (int i = 0; i < _count; i++)
            {
                _particals.Add(CreateParticle());
                var pos = _particals[i].transform.position;
                pos.y = UnityEngine.Random.Range(-15, 15);
                pos.x = UnityEngine.Random.Range(-15, 15);
                pos.z = UnityEngine.Random.Range(10, 30);
                _particals[i].transform.position = pos;

                _particals[i].Rotate(new Vector3(UnityEngine.Random.Range(-15, 15) * 100, UnityEngine.Random.Range(-15, 15) * 100, UnityEngine.Random.Range(-15, 15) * 100));
            }
        }

        public void StopTest()
        {
            _running = false;
            foreach (var obj in _particals)
            {
                UnityEngine.Object.Destroy(obj.gameObject);
            }
            _particals.Clear();
            TestScenarioCompleted?.Invoke();
        }

        public void UpdateTest(float dt)
        {

        }
    }
}