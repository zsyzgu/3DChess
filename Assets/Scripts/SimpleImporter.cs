using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine;

namespace PointCloudExporter
{
	public class SimpleImporter
	{
		private static SimpleImporter instance;
		private SimpleImporter () {}
		public static SimpleImporter Instance {
			get {
				if (instance == null) {
					instance = new SimpleImporter();
				}
				return instance;
			}
		}

        private ArrayList xyzList;
        private ArrayList rgbList;
        private ArrayList norList;

        public List<MeshInfos> Load (string filePath, int verticesMax)
		{
            if (xyzList == null)
            {
                StreamReader sr = new StreamReader(filePath);

                xyzList = new ArrayList();
                rgbList = new ArrayList();
                norList = new ArrayList();

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
                    }
                    else
                    {
                        Vector3 xyz = new Vector3(float.Parse(tags[0]), float.Parse(tags[1]), float.Parse(tags[2]));
                        uint color = uint.Parse(tags[3]);
                        Color rgb = new Color((float)((color >> 16) & 255) / 255, (float)((color >> 8) & 255) / 255, (float)(color & 255) / 255);
                        Vector3 nor = new Vector3(float.Parse(tags[4]), float.Parse(tags[5]), float.Parse(tags[6]));
                        xyzList.Add(xyz);
                        rgbList.Add(rgb);
                        norList.Add(nor);
                    }
                }
            }

            return Update(verticesMax);
		}

        public List<MeshInfos> Update(int verticesMax)
        {
            int N = xyzList.Count;
            List<MeshInfos> meshList = new List<MeshInfos>();
            for (int st = 0; st < N; st += verticesMax)
            {
                int len = verticesMax;
                if (len > N - st)
                {
                    len = N - st;
                }
                MeshInfos mesh = new MeshInfos();
                mesh.vertexCount = len;
                mesh.vertices = new Vector3[len];
                mesh.normals = new Vector3[len];
                mesh.colors = new Color[len];
                for (int i = 0; i < len; i++)
                {
                    mesh.vertices[i] = (Vector3)xyzList[st + i];
                    mesh.normals[i] = (Vector3)norList[st + i];
                    mesh.colors[i] = (Color)rgbList[st + i];
                }
                meshList.Add(mesh);
            }

            return meshList;
        }
    }
}
