using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ViewPointCloud : MonoBehaviour {
    const string pcdFilePath = "model1.pcd";

	void Start () {
        StreamReader sr = new StreamReader(pcdFilePath);

        ArrayList xyzList = new ArrayList();
        ArrayList rgbList = new ArrayList();

        bool begined = false;
        string line = "";
        while ((line = sr.ReadLine()) != null)
        {
            string[] tags = line.Split(' ');
            if (begined == false)
            {
                if (tags[0] == "DATA")
                {
                    begined = true;
                }
            } else
            {
                Vector3 xyz = new Vector3(float.Parse(tags[0]), float.Parse(tags[1]), float.Parse(tags[2]));
                uint color = uint.Parse(tags[3]);
                Color rgb = new Color((float)((color >> 16) & 255) / 255, (float)((color >> 8) & 255) / 255, (float)(color & 255) / 255);
                xyzList.Add(xyz);
                rgbList.Add(rgb);
            }
        }

        renderPointCloud(ref xyzList, ref rgbList);
	}

    void renderPointCloud(ref ArrayList xyzList, ref ArrayList rgbList)
    {
        GameObject obj = new GameObject();
        obj.name = "Mesh";
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        createMesh(ref mesh, ref xyzList, ref rgbList);
        obj.GetComponent<MeshFilter>().mesh = mesh;
        obj.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Custom/VertexColor"));
    }

    void createMesh(ref Mesh mesh, ref ArrayList xyzList, ref ArrayList rgbList)
    {
        int length = xyzList.Count;
        Vector3[] points = new Vector3[length];
        Color[] colors = new Color[length];
        int[] indecies = new int[length];

        for (int i = 0; i < length; i++)
        {
            points[i] = (Vector3)xyzList[i];
            colors[i] = (Color)rgbList[ i];
            indecies[i] = i;
        }

        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
    }
	
	void Update () {
		
	}
}
