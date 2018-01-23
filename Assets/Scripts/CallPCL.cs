using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

namespace PointCloudExporter
{
    public class CallPCL : MonoBehaviour
    {
        const int BUFFER_SIZE = 12000000;

        static byte[] buffer = new byte[BUFFER_SIZE];

        [DllImport("pc-recog", EntryPoint = "callStart")]
        public static extern void callStart();

        [DllImport("pc-recog", EntryPoint = "callUpdate")]
        public static extern IntPtr callUpdate();

        public static MeshInfos getMesh()
        {
            MeshInfos mesh = new MeshInfos();

            Marshal.Copy(callUpdate(), buffer, 0, BUFFER_SIZE);

            int size = System.BitConverter.ToInt32(buffer, 0);

            mesh.vertexCount = size;
            mesh.vertices = new Vector3[size];
            mesh.normals = new Vector3[size];
            mesh.colors = new Color[size];

            for (int i = 0; i < size; i++)
            {
                int id = i * 27 + 4;

                mesh.vertices[i].x = System.BitConverter.ToSingle(buffer, id + 0);
                mesh.vertices[i].y = System.BitConverter.ToSingle(buffer, id + 4);
                mesh.vertices[i].z = System.BitConverter.ToSingle(buffer, id + 8);
                mesh.colors[i].r = (float)buffer[id + 12] / 255;
                mesh.colors[i].g = (float)buffer[id + 13] / 255;
                mesh.colors[i].b = (float)buffer[id + 14] / 255;
                mesh.normals[i].x = System.BitConverter.ToSingle(buffer, id + 15);
                mesh.normals[i].y = System.BitConverter.ToSingle(buffer, id + 19);
                mesh.normals[i].z = System.BitConverter.ToSingle(buffer, id + 23);
            }

            return mesh;
        }
    }
}
