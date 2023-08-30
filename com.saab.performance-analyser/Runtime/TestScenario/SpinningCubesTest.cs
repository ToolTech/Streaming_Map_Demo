using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Saab.Application.Performance.Example
{
    public class SpinningCubesTest : ITestScenario
    {
        private GameObject _parent;
        private List<Transform> _objects;
        private bool _running;
        private float _rotSpeed = 20;
        private int _count = 100;

        public string Title => "Spinning Cubes";

        public bool IsRunning => _running;

        public string Description => "100 rotating cubes";

        public string Settings => throw new NotImplementedException();

        public string InternalResult => throw new NotImplementedException();

        public event Action TestScenarioCompleted;

        public bool Initialize()
        {
            _parent = new GameObject(Title);
            _objects = new List<Transform>();
            return true;
        }

        public Transform CreateCube()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.SetParent(_parent.transform);

            return go.transform;

        }

        public void StartTest()
        {
            _running = true;
            for (int i = 0; i < _count; i++)
            {
                _objects.Add(CreateCube());
                var pos = _objects[i].transform.position;
                pos.y = UnityEngine.Random.Range(-15, 15);
                pos.x = UnityEngine.Random.Range(-15, 15);
                pos.z = UnityEngine.Random.Range(10, 30);
                _objects[i].transform.position = pos;

                _objects[i].Rotate(new Vector3(UnityEngine.Random.Range(-15, 15) * _rotSpeed, UnityEngine.Random.Range(-15, 15), UnityEngine.Random.Range(-15, 15)));
            }
        }

        public void StopTest()
        {
            _running = false;
            foreach (var obj in _objects) 
            {
                UnityEngine.Object.Destroy(obj.gameObject);
            }
            _objects.Clear();
            TestScenarioCompleted?.Invoke();
        }

        public void UpdateTest(float dt)
        {
            foreach(var obj in _objects)
            {
                obj.Rotate(new Vector3(_rotSpeed * dt, _rotSpeed * dt, _rotSpeed * dt));
            }
        }
    }
}