using UnityEngine;

namespace Saab.Unity.Sandbox
{
    public class TerrainGen : MonoBehaviour
    {
        public GenerateGrass GenerateGrass;
        public GenerateTree GenerateTree;

        public float FadeFarBillboard = 200;
        public float FadeNearBillboard = 150;

        public float FadeDistance = 50;

        public GameObject[] GameObjects;
        void Start()
        {
            foreach (GameObject go in GameObjects)
            {
                if(GenerateGrass != null)
                {
                    GenerateGrass.AddGrass(go);
                }
                if (GenerateTree != null)
                {
                    GenerateTree.AddTree(go);
                }
            }
        }

        private void Update()
        {
            if (GenerateTree != null)
            {
                GenerateTree.FadeFarValue = FadeFarBillboard;
                GenerateTree.FadeNearValue = FadeNearBillboard;
            }
        }
    }
}

