using UnityEngine;

namespace Assets.Crossboard
{
    public class CrossboardRenderer : MonoBehaviour
    {
        public bool OpaqueCrossboard = false;
        public bool OpaqueCrossboardCompute = false;
        public Material DefaultMaterial;
        public Material Material { get; set; }

        private void Awake()
        {
            if(Material == null)
            {
                Material = DefaultMaterial;
            }
        }

        public virtual void SetCrossboardDataset(CrossboardDataset dataset)
        {
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Material;

            var mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = dataset.POSITION;

            if (OpaqueCrossboard)
            {
                mesh.SetUVs(0, dataset.UV0List);
                mesh.SetUVs(1, dataset.UV1List);
                mesh.SetUVs(2, dataset.UV2List);
                mesh.SetUVs(3, dataset.UV3List);
            }
            else if (OpaqueCrossboardCompute)
            {
                mesh.SetUVs(0, dataset.UV0ListComp);
                mesh.SetUVs(1, dataset.UV1ListComp);
            }
            else
            {
                mesh.uv = dataset.UV0;
                mesh.uv2 = dataset.UV1;
            }

            mesh.colors = dataset.COLOR;

            var n = dataset.POSITION.Length;
            var indices = new int[n];
            for (var i = 0; i < n; ++i)
            {
                indices[i] = i;
            }

            mesh.SetIndices(indices, MeshTopology.Points, 0);


            //mesh.bounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f));

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
        }
    }
}