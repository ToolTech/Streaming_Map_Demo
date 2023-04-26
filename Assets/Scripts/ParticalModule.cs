using Saab.Foundation.Unity.MapStreamer;
using Saab.Foundation.Map;
using GizmoSDK.GizmoBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

public class ParticalModule : MonoBehaviour
{
    public float MinDistance = 5.0f;
    public float MaxDistance = 10.0f;
    public float MaxViewAngle = 30.0f;

    public bool EnabledFlak = false;
    public int maxParticalCount = 200;
    public int SpawnCount = 2;
    public float SpawnInterval = 0.1f;
    private float _interval = 0;

    public GameObject ParticalPrefab;
    private Camera _camera;

    private Queue<Transform> ParticalPool = new Queue<Transform>();

    private void Start()
    {
        _camera = Camera.main;
        for (int i = 0; i < maxParticalCount; i++)
        {
            var obj = Instantiate(ParticalPrefab);
            obj.SetActive(false);

            var scale = obj.transform.localScale;
            scale.z *= -1;
            obj.transform.localScale = scale;

            ParticalPool.Enqueue(obj.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            var partical = Instantiate(ParticalPrefab);
            GenerateRandomPoint(partical.transform);
        }

        if(EnabledFlak)
        {
            if(_interval > SpawnInterval)
            {
                _interval = 0;
                for (int i = 0; i < SpawnCount; i++)
                {
                    var partical = ParticalPool.Dequeue();
                    partical.gameObject.SetActive(true);
                    GenerateRandomPoint(partical);
                    ParticalPool.Enqueue(partical);
                }              
            }
            else
            {
                _interval += UnityEngine.Time.deltaTime;
            }
        }
    }

    private bool GenerateRandomPoint(Transform transform)
    {
        Vector3 randomDirection = Quaternion.Euler(Random.Range(-MaxViewAngle, MaxViewAngle), Random.Range(-MaxViewAngle, MaxViewAngle), 0) * _camera.transform.forward;
        float randomDistance = Random.Range(MinDistance, MaxDistance);
        Vector3 randomPosition = randomDirection * randomDistance;
        var campos = _camera.GetComponent<ISceneManagerCamera>().GlobalPosition;

        var mapPos = MapControl.SystemMap.GlobalToLocal(new Vec3D(campos.x + randomPosition.x, campos.y + randomPosition.y, campos.z - randomPosition.z));
        if (!MapUtil.MapToUnity(transform, mapPos))
            return false;
        return true;
    }
}
