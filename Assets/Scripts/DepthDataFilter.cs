using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class DepthDataFilter : MonoBehaviour
{
    private const int DEPTH_QUEUE_LENGTH = 8;
    private const int DEPTH_VARY_THRESHOLD = 10;
    private const int PIXEL_QUEUE_LENGTH = 512 * 424 * 9;

    static private Queue<ushort[]> depthQueue = new Queue<ushort[]>();
    static private int[] pixelQueue = new int[PIXEL_QUEUE_LENGTH];

    static public ushort[] process(ushort[] depthData, int width, int height)
    {
        depthData = depthDataPixelFiltering(depthData, width, height);
        depthData = depthDataMovingAverage(depthData, width, height);
        return depthData;
    }

    static ushort[] depthDataPixelFiltering(ushort[] depthData, int width, int height)
    {
        int n = depthData.Length;
        int[] tot = new int[9];
        int[] cnt = new int[9];
        byte[] uncertainty = new byte[n];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (depthData[index] == 0)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx != 0 || dy != 0)
                            {
                                int xSearch = x + dx;
                                int ySearch = y + dy;

                                if (0 <= xSearch && xSearch < width && 0 <= ySearch && ySearch < height)
                                {
                                    int searchIndex = ySearch * width + xSearch;
                                    if (depthData[searchIndex] == 0)
                                    {
                                        uncertainty[index]++;
                                    }
                                } else
                                {
                                    uncertainty[index]++;
                                }
                            }
                        }
                    }
                    int lv = uncertainty[index];
                    pixelQueue[lv * n + tot[lv]++] = index;
                }
            }
        }
        
        for (int lv = 0; lv < 9; )
        {
            if (cnt[lv] >= tot[lv])
            {
                lv++;
                continue;
            }

            int index = pixelQueue[lv * n + cnt[lv]++];

            if (uncertainty[index] != lv)
            {
                continue;
            }

            int y = index / width;
            int x = index % width;

            int sum = 0;
            int num = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx != 0 || dy != 0)
                    {
                        int xSearch = x + dx;
                        int ySearch = y + dy;

                        if (0 <= xSearch && xSearch < width && 0 <= ySearch && ySearch < height)
                        {
                            int searchIndex = ySearch * width + xSearch;
                            if (depthData[searchIndex] == 0)
                            {
                                int searchLv = --uncertainty[searchIndex];
                                pixelQueue[searchLv * n + tot[searchLv]++] = searchIndex;
                            } else
                            {
                                sum += depthData[searchIndex];
                                num++;
                            }
                        }
                    }
                }
            }

            if (num == 0)
            {
                return depthData; //All pixels of depthData equal to zero
            }
            else
            {
                depthData[index] = (ushort)(sum / num);
            }
            
            if (lv - 1 >= 0 && cnt[lv - 1] < tot[lv - 1])
            {
                lv--;
            }
        }

        return depthData;
    }

    static ushort[] depthDataMovingAverage(ushort[] rawDepthData, int width, int height)
    {
        ushort[] depthData = new ushort[rawDepthData.Length];

        depthQueue.Enqueue(rawDepthData);
        if (depthQueue.Count > DEPTH_QUEUE_LENGTH)
        {
            depthQueue.Dequeue();
        }

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
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
                }
                else
                {
                    depthData[index] = (ushort)(sum / cnt);
                    if (Mathf.Abs(rawDepthData[index] - depthData[index]) > DEPTH_VARY_THRESHOLD)
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

    //Simple pixel filtering
    /*static ushort[] depthDataPixelFiltering(ushort[] rawDepthData, int width, int height)
{
    ushort[] depthData = new ushort[rawDepthData.Length];

    Parallel.For(0, height, y =>
    {
        for (int x = 0; x < width; x++)
        {
            int index = y * width + x;
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

                            if (0 <= xSearch && xSearch < width && 0 <= ySearch && ySearch < height)
                            {
                                int searchIndex = ySearch * width + xSearch;
                                if (rawDepthData[searchIndex] != 0)
                                {
                                    sum += rawDepthData[searchIndex];
                                    if (-1 <= dx && dx <= 1 && -1 <= dy && dy <= 1)
                                    {
                                        innerBand++;
                                    }
                                    else
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
                }
                else
                {
                    depthData[index] = 0;
                }
            }
            else
            {
                depthData[index] = rawDepthData[index];
            }
        }
    });

    return depthData;
}*/
}
