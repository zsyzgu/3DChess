using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class MeshGenerator : MonoBehaviour {
    [Header("Renderer")]
    public Shader shader;

    private List<MeshInfos> meshList;
    private const int verticesMax = 64002;
    private Mesh[] meshArray = null;

    void Start()
    {
        CallPCL.callStart();
    }

    void Update()
    {
        meshList = CallPCL.getMesh(verticesMax);
        Generate();
        if (Input.GetKey(KeyCode.R))
        {
            CallPCL.callRegistration();
        }
        if (Input.GetKey(KeyCode.B))
        {
            CallPCL.callSetBackground();
        }
        if (Input.GetKey(KeyCode.S))
        {
            CallPCL.callSaveScene();
        }
    }

    public void Generate()
    {
        if (meshArray == null || meshArray.Length != meshList.Count)
        {
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
            meshArray = new Mesh[meshList.Count];
        }

        for (int i = 0; i < meshList.Count; i++)
        {
            MeshInfos meshInfo = meshList[i];
            int count = meshInfo.vertexCount;
            int[] indices = new int[count];
            for (int j = 0; j < count; j++) {
                indices[j] = j;
            }
            int[] tris = new int[count];
            for (int j = 0; j < count; j++) {
                int res = j % 3;
                if (res == 0) {
                    tris[j] = j;
                } else if (res == 1) {
                    tris[j] = j + 1;
                } else {
                    tris[j] = j - 1;
                }
            }
            if (meshArray[i] == null)
            {
                meshArray[i] = new Mesh();
            }
            Mesh mesh = meshArray[i];
            if (mesh.vertexCount != meshInfo.vertices.Length)
            {
                mesh.Clear();
            }
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 16f);
            mesh.vertices = meshInfo.vertices;
            mesh.colors = meshInfo.colors;
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
        }

        if (transform.childCount == meshArray.Length)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.GetComponent<MeshFilter>().mesh = meshArray[i];
            }
        }
        else
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }
            for (int i = 0; i < meshArray.Length; i++)
            {
                CreateGameObjectWithMesh(meshArray[i], gameObject.name + "_" + i, transform);
            }
        }
    }

    public GameObject CreateGameObjectWithMesh(Mesh mesh, string name = "GeneratedMesh", Transform parent = null)
    {
        GameObject meshGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        DestroyImmediate(meshGameObject.GetComponent<Collider>());
        meshGameObject.GetComponent<MeshFilter>().mesh = mesh;
        Material material = new Material(shader);
        meshGameObject.GetComponent<Renderer>().sharedMaterial = material;
        meshGameObject.name = name;
        meshGameObject.transform.parent = parent;
        meshGameObject.transform.localPosition = Vector3.zero;
        meshGameObject.transform.localRotation = Quaternion.identity;
        meshGameObject.transform.localScale = Vector3.one;
        return meshGameObject;
    }
}
