using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class View3D : MonoBehaviour
{
    private const int DOWNSAMPLE = 2;
    private const float M_PER_MM = 0.001f;
    private const float MAX_DISTANCE = 8f;
    private const float TAN_HALF_H_FOV = 0.7133f;
    private const float TAN_HALF_V_FOV = 0.4774f;

    private CoordinateMapper mapper;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;

	void Start () {
        mapper = SourceManager.getCoordinateMapper();
        createMesh();
	}
	
	void Update () {
        updateMesh();
	}

    void createMesh()
    {
        int width = SourceManager.getDepthWidth() / DOWNSAMPLE;
        int height = SourceManager.getDepthHeight() / DOWNSAMPLE;

        GetComponent<MeshFilter>().mesh = mesh = new Mesh();

        vertices = new Vector3[width * height];
        uv = new Vector2[width * height];
        triangles = new int[6 * (width - 1) * (height - 1)];

        int triangleIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width) + x;

                vertices[index] = new Vector3(x - width / 2, -(y - height / 2), 0) * M_PER_MM;
                uv[index] = new Vector2((float)x / width, (float)y / height);

                if (x != (width - 1) && y != (height - 1))
                {
                    int topLeft = index;
                    int topRight = index + 1;
                    int bottomLeft = index + width;
                    int bottomRight = index + width + 1;

                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void updateMesh()
    {
        GetComponent<Renderer>().material.mainTexture = SourceManager.getColorTexture();

        ushort[] depthData = SourceManager.getDepthData();
        int depthWidth = SourceManager.getDepthWidth();
        int depthHeight = SourceManager.getDepthHeight();
        int colorWidth = SourceManager.getColorWidth();
        int colorHeight = SourceManager.getColorHeight();

        ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
        mapper.MapDepthFrameToColorSpace(depthData, colorSpace);

        for (int y = 0; y < depthHeight; y += DOWNSAMPLE)
        {
            for (int x = 0; x < depthWidth; x += DOWNSAMPLE)
            {
                int index = (y / DOWNSAMPLE) * (depthWidth / DOWNSAMPLE) + (x / DOWNSAMPLE);

                vertices[index].z = getAvgDepth(depthData, x, y, depthWidth, depthHeight) * M_PER_MM;
                vertices[index].x = vertices[index].z * TAN_HALF_H_FOV * (2.0f * x / depthWidth - 1.0f);
                vertices[index].y = vertices[index].z * TAN_HALF_V_FOV * -(2.0f * y / depthHeight - 1.0f);
                vertices[index].z -= MAX_DISTANCE;

                ColorSpacePoint colorSpacePoint = colorSpace[y * depthWidth + x];
                uv[index] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
            }
        }
        transform.position = new Vector3(0f, 0f, MAX_DISTANCE); //To evade the near clipping planes 

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    float getAvgDepth(ushort[] depthData, int xBase, int yBase, int width, int height)
    {
        float sum = 0;

        for (int y = yBase; y < yBase + DOWNSAMPLE; y++)
        {
            for (int x = xBase; x < xBase + DOWNSAMPLE; x++)
            {
                int index = y * width + x;

                if (depthData[index] == 0)
                {
                    sum += MAX_DISTANCE / M_PER_MM;
                } else
                {
                    sum += depthData[index];
                }
            }
        }

        return sum / (DOWNSAMPLE * DOWNSAMPLE);
    }
}
