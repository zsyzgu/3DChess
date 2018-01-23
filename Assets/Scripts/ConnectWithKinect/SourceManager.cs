using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using System.Threading.Tasks;

public class SourceManager : MonoBehaviour {
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

            if (!sensor.IsOpen)
            {
                sensor.Open();
            }
        }
	}

    void Update()
    {
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

    void updateDepth(DepthFrame depthFrame)
    {
        if (depthFrame != null)
        {
            depthFrame.CopyFrameDataToArray(depthData);

            depthFrame.Dispose();
            depthFrame = null;
        }
    }
}
