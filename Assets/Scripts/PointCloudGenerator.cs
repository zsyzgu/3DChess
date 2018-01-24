using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace PointCloudExporter
{
	public class PointCloudGenerator : MonoBehaviour
	{
		[Header("Point Cloud")]
		public string fileName = "scene.pcd";

		[Header("Renderer")]
		public float size = 0.1f;
		public Texture sprite;
		public Shader shader;

		private MeshInfos points;
		private const int verticesMax = 64000;
		private Mesh[] meshArray;
		private Transform[] transformArray;
		private Texture2D colorMapTexture;

		void Start ()
		{
            CallPCL.callStart();
            //points = SimpleImporter.Instance.Load(fileName);
        }
		
		void Update ()
		{
            points = CallPCL.getMesh();
            Generate(points, MeshTopology.Points);
            if (Input.GetKey(KeyCode.R))
            {
                CallPCL.callRegistration();
            }
        }

		public void Generate (MeshInfos meshInfos, MeshTopology topology)
		{
			for (int c = transform.childCount - 1; c >= 0; --c) {
				Transform child = transform.GetChild(c);
				DestroyImmediate(child.gameObject);
			}

            if (meshArray != null)
            {
                foreach (Mesh mesh in meshArray)
                {
                    if (mesh != null)
                    {
                        Destroy(mesh);
                    }
                }
            }

            int vertexCount = meshInfos.vertexCount;
			int meshCount = (int)Mathf.Ceil(vertexCount / (float)verticesMax);

			meshArray = new Mesh[meshCount];
			transformArray = new Transform[meshCount];

			int index = 0;
			int meshIndex = 0;

			int resolution = GetNearestPowerOfTwo(Mathf.Sqrt(vertexCount));

            while (meshIndex < meshCount) {
                int count = verticesMax;
                if (vertexCount <= verticesMax) {
                    count = vertexCount;
                } else if (vertexCount > verticesMax && meshCount == meshIndex + 1) {
                    count = vertexCount % verticesMax;
                }
                
                int[] subIndices = new int[count];
                Parallel.For(0, count, i => {
                    subIndices[i] = i;
                });

				Mesh mesh = new Mesh(); 
				mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1f);
                mesh.vertices = meshInfos.vertices.Skip(meshIndex * verticesMax).Take(count).ToArray();
                mesh.normals = meshInfos.normals.Skip(meshIndex * verticesMax).Take(count).ToArray();
                mesh.colors = meshInfos.colors.Skip(meshIndex * verticesMax).Take(count).ToArray();
				mesh.SetIndices(subIndices, topology, 0);

                GameObject go = CreateGameObjectWithMesh(mesh, gameObject.name + "_" + meshIndex, transform);
				
				meshArray[meshIndex] = mesh;
				transformArray[meshIndex] = go.transform;

				index += count;
				++meshIndex;
			}
        }
        
		public int GetNearestPowerOfTwo (float x)
		{
			return (int)Mathf.Pow(2f, Mathf.Ceil(Mathf.Log(x) / Mathf.Log(2f)));
		}

        public GameObject CreateGameObjectWithMesh(Mesh mesh, string name = "GeneratedMesh", Transform parent = null)
        {
            GameObject meshGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DestroyImmediate(meshGameObject.GetComponent<Collider>());
            meshGameObject.GetComponent<MeshFilter>().mesh = mesh;
            Material material = new Material(shader);
            material.SetFloat("_Size", size);
            material.SetTexture("_MainTex", sprite);
            meshGameObject.GetComponent<Renderer>().sharedMaterial = material;
            meshGameObject.name = name;
            meshGameObject.transform.parent = parent;
            meshGameObject.transform.localPosition = Vector3.zero;
            meshGameObject.transform.localRotation = Quaternion.identity;
            meshGameObject.transform.localScale = Vector3.one;
            return meshGameObject;
        }
    }
}

/*for (int i = 0; i < uv2.Length; i++) {
    float x = vertexIndex % resolution;
    float y = Mathf.Floor(vertexIndex / (float)resolution);
    uvs2[i] = new Vector2(x, y) / (float)resolution;
    ++vertexIndex;
});
mesh.uv2 = uvs2;*/
