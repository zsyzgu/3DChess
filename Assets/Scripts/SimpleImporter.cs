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
		// Singleton
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

		public MeshInfos Load (string filePath, int maximumVertex = 65000)
		{
            MeshInfos data = new MeshInfos();

            StreamReader sr = new StreamReader(filePath);

            ArrayList xyzList = new ArrayList();
            ArrayList rgbList = new ArrayList();
            ArrayList norList = new ArrayList();

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

            int N = data.vertexCount = xyzList.Count;
            data.vertices = new Vector3[N];
            data.normals = new Vector3[N];
            data.colors = new Color[N];

            for (int i = 0; i < N; i++)
            {
                data.vertices[i] = (Vector3)xyzList[i];
                data.normals[i] = (Vector3)norList[i];
                data.colors[i] = (Color)rgbList[i];
            }

            return data;
		}
    }
}
