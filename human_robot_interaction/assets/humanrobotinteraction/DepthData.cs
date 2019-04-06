using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using HumanRobotInteraction;
using MathNet.Numerics.LinearAlgebra;

namespace HumanRobotInteraction
{
    class DepthData
    {
        PXCMSenseManager sm;
        PXCMCaptureManager CaptureManager;
        PXCMCapture.Device Device;     // Device Instance
        pxcmStatus Status;  // Status report
        PXCMSession Session;
        PXCMProjection Projection;  // Projection Instance
        double[,] Depth;

        public DepthData()
        {
            sm = PXCMSenseManager.CreateInstance();
            Status = sm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH, 640, 480);
            /*PXCMVideoModule.DataDesc ddesc = new PXCMVideoModule.DataDesc();
            ddesc.deviceInfo.streams = PXCMCapture.StreamType.STREAM_TYPE_DEPTH;
            sm.EnableStreams(ddesc);*/
            sm.Init();
        }

        public double[,] GetDepthData()
        {
            Depth = null;
            PXCMCapture.Sample Image = sm.QuerySample();

            PXCMImage depth = Image.depth;

            if (depth != null)
            {
                var DepthWidth = depth.info.width;
                var DepthHeight = depth.info.height;

                Device = sm.captureManager.QueryDevice();
                Device.SetMirrorMode(PXCMCapture.Device.MirrorMode.MIRROR_MODE_HORIZONTAL);
                Projection = Device.CreateProjection();
                
                PXCMPoint3DF32[] Vertices = new PXCMPoint3DF32[DepthHeight * DepthWidth];
                Status = Projection.QueryVertices(depth, Vertices);
                
                Depth = new double[6, (DepthWidth * DepthHeight)/100];
                int j = 0;
                for (int i = 0; i < DepthWidth * DepthHeight; i+=10)
                {
                    Depth[0, j] = -Vertices[i].x/10;
                    Depth[1, j] = Vertices[i].y/10;
                    Depth[2, j] = Vertices[i].z/10;
                    j++;
                    if (i % 640 == 0)
                        i += 6400;
                }
                
    
                
                Projection.Dispose();

            }

            sm.ReleaseFrame();
            return Depth;
        }

    }
}

