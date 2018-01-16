using UnityEngine;
using System;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.Cuda;
using OpenCvSharp.Detail;
using OpenCvSharp.Face;
using OpenCvSharp.ML;
using OpenCvSharp.Flann;
using OpenCvSharp.OptFlow;
using OpenCvSharp.Tracking;
using OpenCvSharp.Util;
using OpenCvSharp.XFeatures2D;
using OpenCvSharp.XImgProc;
using OpenCvSharp.XPhoto;
using System.Runtime.InteropServices;

class Recognition : MonoBehaviour
{
    void Start()
    {
        Mat rawImg = Cv2.ImRead("img1.png", ImreadModes.Color);

        Mat img = new Mat();
        Cv2.CvtColor(rawImg, img, ColorConversionCodes.RGB2GRAY);

        //Cv2.Threshold(img, img, 100, 255, ThresholdTypes.Binary);
        Cv2.Canny(img, img, 50, 100);

        Cv2.ImShow("image", img);

        /*Mat img1 = new Mat();
        Mat img2 = new Mat();
        Cv2.CvtColor(rawImg1, img1, ColorConversionCodes.RGB2GRAY);
        Cv2.CvtColor(rawImg2, img2, ColorConversionCodes.RGB2GRAY);
        
        SIFT sift = SIFT.Create();

        KeyPoint[] keyPoints1 = sift.Detect(img1);
        KeyPoint[] keyPoints2 = sift.Detect(img2);
        Mat desc1 = new Mat();
        Mat desc2 = new Mat();
        sift.Compute(img1, ref keyPoints1, desc1);
        sift.Compute(img2, ref keyPoints2, desc2);

        BFMatcher matcher = new BFMatcher();
        DMatch[] matches = matcher.Match(desc1, desc2);
        List<DMatch> goodMatches = new List<DMatch>();

        float maxDist = 0f;
        for (int i = 0; i < matches.Length; i++)
        {
            float dist = matches[i].Distance;
            if (dist > maxDist)
            {
                maxDist = dist;
            }
        }
        for (int i = 0; i < matches.Length; i++)
        {
            float dist = matches[i].Distance;
            if (dist < maxDist * 0.6f)
            {
                goodMatches.Add(matches[i]);
            }
        }

        Mat imgMatches = new Mat();
        Cv2.DrawMatches(img1, keyPoints1, img2, keyPoints2, goodMatches, imgMatches);
        Cv2.ImShow("Matches", imgMatches);*/
    }
}
