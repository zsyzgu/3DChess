using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PointCloudExporter
{
    public class CallPCL : MonoBehaviour
    {
        const int BUFFER_SIZE = 12000000;
        const int POINT_BYTES = 27;

        static byte[] buffer = new byte[BUFFER_SIZE];

        [DllImport("pc-recog", EntryPoint = "callStart")]
        public static extern void callStart();

        [DllImport("pc-recog", EntryPoint = "callUpdate")]
        public static extern IntPtr callUpdate();

        [DllImport("pc-recog", EntryPoint = "callRegistration")]
        public static extern void callRegistration();

        [DllImport("pc-recog", EntryPoint = "callStop")]
        public static extern void callStop();

        public static List<MeshInfos> getMesh(int vMax)
        {
            List<MeshInfos> meshList = new List<MeshInfos>();

            IntPtr ptr = callUpdate();
            Marshal.Copy(ptr, buffer, 0, 4);
            int size = System.BitConverter.ToInt32(buffer, 0);

            Marshal.Copy(ptr + 4, buffer, 0, size * POINT_BYTES);
            for (int st = 0; st < size; st += vMax)
            {
                MeshInfos mesh = new MeshInfos();

                int len = vMax;
                if (size - st < len)
                {
                    len = size - st;
                }

                mesh.vertexCount = len;
                mesh.vertices = new Vector3[len];
                mesh.normals = new Vector3[len];
                mesh.colors = new Color[len];

                Parallel.For(0, len, i => {
                    int id = (st + i) * POINT_BYTES;

                    mesh.vertices[i].x = System.BitConverter.ToSingle(buffer, id + 0);
                    mesh.vertices[i].y = System.BitConverter.ToSingle(buffer, id + 4);
                    mesh.vertices[i].z = System.BitConverter.ToSingle(buffer, id + 8);
                    mesh.colors[i].r = (float)buffer[id + 12] / 255;
                    mesh.colors[i].g = (float)buffer[id + 13] / 255;
                    mesh.colors[i].b = (float)buffer[id + 14] / 255;
                    mesh.normals[i].x = System.BitConverter.ToSingle(buffer, id + 15);
                    mesh.normals[i].y = System.BitConverter.ToSingle(buffer, id + 19);
                    mesh.normals[i].z = System.BitConverter.ToSingle(buffer, id + 23);
                });

                meshList.Add(mesh);
            }

            return meshList;
        }
    }
}
