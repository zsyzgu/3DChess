using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System.Threading.Tasks;

public class SourceManager : MonoBehaviour {
    private const int DEPTH_QUEUE_LENGTH = 8;

    public bool openFilter = true;

    static private SourceManager instance;

    private KinectSensor sensor;
    private ColorFrameReader colorReader;
    private InfraredFrameReader infraredReader;
    private DepthFrameReader depthReader;
    private CoordinateMapper mapper;

    private int colorWidth;
    private int colorHeight;
    private Texture2D colorTexture;
    private byte[] colorData;

    private int infraredWidth;
    private int infraredHeight;
    private Texture2D infraredTexture;
    private ushort[] infraredRawData;
    private byte[] infraredData;

    private int depthWidth;
    private int depthHeight;
    private ushort[] depthData;
    private Queue<ushort[]> depthQueue;
    
    static public CoordinateMapper getCoordinateMapper()
    {
        return instance.mapper;
    }

    static public int getColorWidth()
    {
        return instance.colorWidth;
    }

    static public int getColorHeight()
    {
        return instance.colorHeight;
    }

    static public Texture2D getColorTexture()
    {
        return instance.colorTexture;
    }

    static public int getInfraredWidth()
    {
        return instance.infraredWidth;
    }

    static public int getInfraredHeight()
    {
        return instance.infraredHeight;
    }

    static public Texture2D getInfraredTexture()
    {
        return instance.infraredTexture;
    }

    static public int getDepthWidth()
    {
        return instance.depthWidth;
    }

    static public int getDepthHeight()
    {
        return instance.depthHeight;
    }

    static public ushort[] getDepthData()
    {
        return instance.depthData;
    }

    void Awake()
    {
        instance = this;

        sensor = KinectSensor.GetDefault();

        if (sensor != null)
        {
            colorReader = sensor.ColorFrameSource.OpenReader();
            infraredReader = sensor.InfraredFrameSource.OpenReader();
            depthReader = sensor.DepthFrameSource.OpenReader();
            mapper = sensor.CoordinateMapper;

            var colorFrameDesc = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            colorWidth = colorFrameDesc.Width;
            colorHeight = colorFrameDesc.Height;
            colorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            colorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];

            var infraredFrameDesc = sensor.InfraredFrameSource.FrameDescription;
            infraredWidth = infraredFrameDesc.Width;
            infraredHeight = infraredFrameDesc.Height;
            infraredTexture = new Texture2D(infraredFrameDesc.Width, infraredFrameDesc.Height, TextureFormat.RGBA32, false);
            infraredRawData = new ushort[infraredFrameDesc.LengthInPixels];
            infraredData = new byte[infraredFrameDesc.LengthInPixels * 4];

            var depthFrameDesc = sensor.DepthFrameSource.FrameDescription;
            depthWidth = depthFrameDesc.Width;
            depthHeight = depthFrameDesc.Height;
            depthData = new ushort[depthFrameDesc.LengthInPixels];
            depthQueue = new Queue<ushort[]>();

            if (!sensor.IsOpen)
            {
                sensor.Open();
            }
        }
	}
    
    void updateColor(ColorFrame colorFrame)
    {
        if (colorFrame != null)
        {
            colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Rgba);
            colorTexture.LoadRawTextureData(colorData);
            colorTexture.Apply();

            colorFrame.Dispose();
            colorFrame = null;
        }
    }
    
    void updateInfrared(InfraredFrame infraredFrame)
    {
        if (infraredFrame != null)
        {
            infraredFrame.CopyFrameDataToArray(infraredRawData);
            
            Parallel.For(0, infraredHeight, y =>
            {
                for (int x = 0; x < infraredWidth; x++)
                {
                    int index = y * infraredWidth + x;
                    byte intensity = (byte)(infraredRawData[index] >> 8);
                    infraredData[(index << 2) | 0] = intensity;
                    infraredData[(index << 2) | 1] = intensity;
                    infraredData[(index << 2) | 2] = intensity;
                    infraredData[(index << 2) | 3] = 255;
                }
            });

            infraredTexture.LoadRawTextureData(infraredData);
            infraredTexture.Apply();

            infraredFrame.Dispose();
            infraredFrame = null;
        }
    }

    ushort[] depthDataPixelFiltering(ushort[] rawDepthData)
    {
        const int INNER_THRESHOLD = 2;
        const int OUTER_THRESHOLD = 4;

        ushort[] depthData = new ushort[rawDepthData.Length];

        Parallel.For(0, depthHeight, y =>
        {
            for (int x = 0; x < depthWidth; x++)
            {
                int index = y * depthWidth + x;
                if (rawDepthData[index] == 0)
                {
                    int innerBand = 0;
                    int outerBand = 0;
                    int sum = 0;
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            if (dx != 0 || dy != 0)
                            {
                                int xSearch = x + dx;
                                int ySearch = y + dy;

                                if (0 <= xSearch && xSearch < depthWidth && 0 <= ySearch && ySearch < depthHeight)
                                {
                                    int searchIndex = ySearch * depthWidth + xSearch;
                                    if (rawDepthData[searchIndex] != 0)
                                    {
                                        sum += rawDepthData[searchIndex];
                                        if (-1 <= dx && dx <= 1 && -1 <= dy && dy <= 1)
                                        {
                                            innerBand++;
                                        } else
                                        {
                                            outerBand++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (innerBand >= INNER_THRESHOLD || outerBand >= OUTER_THRESHOLD)
                    {
                        depthData[index] = (ushort)(sum / (innerBand + outerBand));
                    } else
                    {
                        depthData[index] = 0;
                    }
                } else
                {
                    depthData[index] = rawDepthData[index];
                }
            }
        });

        return depthData;
    }

    ushort[] depthDataMovingAverage(ushort[] rawDepthData)
    {
        const int DEPTH_CHANGE_THRESHOLD = 10;

        ushort[] depthData = new ushort[rawDepthData.Length];

        depthQueue.Enqueue(rawDepthData);
        if (depthQueue.Count > DEPTH_QUEUE_LENGTH)
        {
            depthQueue.Dequeue();
        }

        Parallel.For(0, depthHeight, y =>
        {
            for (int x = 0; x < depthWidth; x++)
            {
                int index = y * depthWidth + x;
                int sum = 0;
                int cnt = 0;
                foreach (ushort[] item in depthQueue)
                {
                    if (item[index] != 0)
                    {
                        sum += item[index];
                        cnt++;
                    }
                }
                if (cnt == 0)
                {
                    depthData[index] = 0;
                } else
                {
                    depthData[index] = (ushort)(sum / cnt);
                    if (Mathf.Abs(rawDepthData[index] - depthData[index]) > DEPTH_CHANGE_THRESHOLD)
                    {
                        depthData[index] = rawDepthData[index];
                        foreach (ushort[] item in depthQueue)
                        {
                            item[index] = 0;
                        }
                    }
                }
            }
        });

        return depthData;
    }

    void updateDepth(DepthFrame depthFrame)
    {
        if (depthFrame != null)
        {
            depthFrame.CopyFrameDataToArray(depthData);

            if (openFilter)
            {
                depthData = depthDataPixelFiltering(depthData);
                depthData = depthDataMovingAverage(depthData);
            } else
            {
                depthQueue.Clear();
            }

            depthFrame.Dispose();
            depthFrame = null;
        }
    }

	void Update () {
        if (colorReader != null)
        {
            updateColor(colorReader.AcquireLatestFrame());
        }
        if (infraredReader != null)
        {
            updateInfrared(infraredReader.AcquireLatestFrame());
        }
        if (depthReader != null)
        {
            updateDepth(depthReader.AcquireLatestFrame());
        }
	}

    void OnApplicationQuit()
    {
        if (colorReader != null)
        {
            colorReader.Dispose();
            colorReader = null;
        }
        if (infraredReader != null)
        {
            infraredReader.Dispose();
            infraredReader = null;
        }
        if (depthReader != null)
        {
            depthReader.Dispose();
            depthReader = null;
        }

        if (sensor != null)
        {
            if (sensor.IsOpen)
            {
                sensor.Close();
            }
            sensor = null;
        }
    }
}
