using UnityEngine;

namespace Saab.Unity.Sandbox
{
    public class TerrainGen : MonoBehaviour
    {
        public GenerateGrass GenerateGrass;

        public GameObject[] GameObjects;
        void Start()
        {
            foreach (GameObject go in GameObjects)
            {
                GenerateGrass.AddGrass(go);
            }
        }
    }
}

