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

    public class Main : MonoBehaviour
    {
    #region Declaration
    /* Declaration of class variables */

    bool DepthOn = true;
    bool WithDepth = true;
    bool WithReplay = false;

    private const int MaxHands = 3; // Maximum number of tracked hands 
    private const int MaxJoints = PXCMHandData.NUMBER_OF_JOINTS; // Maximum number of tracked joints
    private HandCoord[] RawCoord = new HandCoord[MaxHands];
    private HandCoord[] FilteredCoord= new HandCoord[MaxHands];
    private HandCoord[] PredictedCoord = new HandCoord[MaxHands];
    private DepthData DepthData;
    private HandsData HandsData;
    private SARI[] Sari = new SARI[MaxHands];
    private SARI SariPlane;
    private Visualization[] VisFiltered = new Visualization[MaxHands];
    private Visualization[] VisRaw = new Visualization[MaxHands];
    private Visualization[] VisPredicted = new Visualization[MaxHands];
    private Visualization VisPlane;
    double[] PlaneNormal;
    double[] PlanePoint;

    HandCoord[] CalibrationData = new HandCoord[200];
    HandCoord[,] RecordedData = new HandCoord[3,10000];

    double[,] Depth;
    bool[] DepthSafety;

    private int count = 0;
    private int counter = 0;
    private int wait = 0;

    bool ShowPrediction = false;
    bool ShowRaw = false;
    bool ShowFiltered = true;
    string Mode = "Default";

    int count2 = 0;
    bool Live = true;
    int Max = 10000;
    double[] R = { 0.02, 0.02, 0.02, 0.08, 0.08, 0.08 };
    double[] Q = { 0.0002, 0.0002, 0.0002, 0.0002, 0.0002, 0.0002};
    #endregion

    // Use this for initialization
    void Start()
    {

        // AssetDatabase.ImportPackage("/packages/UnityToolkit",false);

        #region Instantiate HandData and DepthData
        if (WithDepth)
        {
            DepthData = new HumanRobotInteraction.DepthData(); // Configure and setting of the getting of the depth data
        }

        HandsData = new HandsData(MaxHands, MaxJoints); // Configure and setting of the getting of the hand data
        #endregion

        #region Instantiate the SARI Algorithm
        /* Instantiation of the SARI and initializing the filters */
        for (int i = 0; i < MaxHands; i++)
        {
            Sari[i] = new SARI(MaxJoints);  
            Sari[i].InitializeFilter();

        }
        SariPlane = new SARI();
        #endregion

        #region Instatiate the visualization
        for (int i = 0; i < MaxHands; i++)
        {
            VisPredicted[i] = new Visualization("Hand Predicted " + (i + 1).ToString(), MaxJoints, new Color(0, 0, 1), new Color(0, 0, 1), new Color(0, 0, 1), new Color(0.47f,0.108f,0.221f));
            VisFiltered[i] = new Visualization("Hand Filtered " + (i + 1).ToString(), MaxJoints, new Color(0.65f, 0.22f, 0f), new Color(0.9f, 0.62f, 0.43f), new Color(0.65f, 0.22f, 0f), new Color(1, 0, 0));
            VisRaw[i] = new Visualization("Hand Raw " + (i + 1).ToString(), MaxJoints, new Color(0.65f, 0.22f, 0f), new Color(0.9f, 0.62f, 0.43f), new Color(0.65f, 0.22f, 0f), new Color(1, 0, 0));
        }

        VisPlane = new Visualization("Plane");

        #endregion

        #region Prepare arrays for coordinates
        /* Declaration of the array for the hand data*/
        for (int i = 0; i < MaxHands; i++)
        {
            RawCoord[i] = new HandCoord(6, MaxJoints);
            FilteredCoord[i] = new HandCoord(6, MaxJoints);
            PredictedCoord[i] = new HandCoord(6, MaxJoints);
        }
        for (int i = 0; i < 10000; i++)
        {
            for (int j = 0; j < 3; j++)
                RecordedData[j, i] = new HandCoord(6, MaxJoints);
            if (i < 200)
                CalibrationData[i] = new HandCoord(6, MaxJoints);
        }
        #endregion


    }
    // Update is called once per frame
    void Update()
    {
      
        #region Get depth and hand coordinates
        if (WithDepth)
        {

            counter++;
            if (counter == 2)
            {
                Depth = DepthData.GetDepthData();
                
            }
        }
        if (Live)
        {
            RawCoord = HandsData.GetJointCoordinates();
        }
        else
        {
            double[,] temp = new double[6, MaxJoints];
            Array.Copy(RecordedData[0, count].Coordinates, temp, 6 * MaxJoints);
            RawCoord[0].Coordinates = temp;
        }
                

        #endregion

        #region Filter and Predict joint coordinates
        /* Calling the update function of SARI for all coordinates */
        for (int i = 0; i < MaxHands; i++)
        {
            if (RawCoord[i].HandDetected)
            {
                if(Live)
                FilteredCoord[i].Coordinates = Sari[i].UpdateFilter(RawCoord[i].Coordinates);
                else
                    FilteredCoord[i].Coordinates = Sari[i].UpdateFilter(RecordedData[0,count].Coordinates);
                PredictedCoord[i].Coordinates = Sari[i].PredictFilterVisualization();
            }
        }
        #endregion

        #region Visualize hands

        for (int i = 0; i < MaxHands; i++)
        {
            for (int j = 0; j < MaxJoints; j++)
                RawCoord[i].Coordinates[0, j] += 20;
            if (RawCoord[i].HandDetected)
            {

                if (ShowFiltered)
                    VisFiltered[i].UpdateHand(FilteredCoord[i].Coordinates, true);
                else
                    VisFiltered[i].UpdateHand(false);

                if (ShowPrediction)
                    VisPredicted[i].UpdateHand(PredictedCoord[i].Coordinates, true);
                else
                    VisPredicted[i].UpdateHand(false);

                if (ShowRaw && Live)
                    VisRaw[i].UpdateHand(RawCoord[i].Coordinates, true);
                else if (ShowRaw)
                     VisRaw[i].UpdateHand(RecordedData[0,count].Coordinates, true);
                else
                    VisRaw[i].UpdateHand(false);



            }

            else
            {
                VisPredicted[i].UpdateHand(false);
                VisFiltered[i].UpdateHand(false);
                VisRaw[i].UpdateHand(false);
            }
            for (int j = 0; j < MaxJoints; j++)
                RawCoord[i].Coordinates[0, j] -= 20;
        }

       
        //Debug.LogWarning(RawCoord[0].Coordinates[3, 21]);
        //Debug.LogWarning(RawCoord[0].Coordinates[0, 1] - FilteredCoord[0].Coordinates[0, 1]);
        #endregion

        #region Safetyzone Setup
        switch (Mode)
        {
            /* Default Mode: the plane is set to 60 cm away of the camera in the xy plane */
            case "Default":
                double[] Default = new double[3];
                Default[2] = 60;
                SariPlane.SetDefaultPlaneNormal(Default);   // Set the Default Safety Plane 40 cm in front of the Camera
                SariPlane.SetDefaultPlanePoint(Default);  // Set the Plane vertical, so that it lies in the xy plane
                VisPlane.ShowPlane(Default, Default);
                Mode = "ShowSafety";
                break;

            /* Setup Mode: The previous plane gets deleted and a new plane can be defined by the User */
            case "Setup":
                {
                    WithDepth = false;
                    GameObject.Find("ButtonSetup").GetComponent<Image>().color = new Color(1, 0.4f, 0.4f);
                    for (int i = 0; i < MaxHands; i++)
                    {
                        VisFiltered[i].ShowPointsInside(new bool[MaxJoints]);

                        if (RawCoord[i].HandDetected)
                        {
                            double[] PosIndexTip = { FilteredCoord[i].Coordinates[0, 9], FilteredCoord[i].Coordinates[1, 9], FilteredCoord[i].Coordinates[2, 9] };
                            PlanePoint = SariPlane.GetPlanePoint(PosIndexTip);
                            if (PlanePoint != null)
                            {
                                VisPlane.ShowPlanePoint(PlanePoint, count);
                                Mode = "Confirmation";
                            }
                        }
                    }
                    break;
                }

            case "Confirmation":
                {

                    if (HandsData.IsGestureFired("v_sign"))
                    {
                        VisPlane.ConfirmPlanePoint(count);
                        SariPlane.ConfirmPoint(count);
                        Mode = "Wait";
                        count++;
                    }

                    if (HandsData.IsGestureFired("thumb_down"))
                    {
                        VisPlane.DeletePlanePoint(count);
                        Mode = "Setup";
                        Mode = "Wait";
                    }

                    if (count == 3)
                    {
                        PlaneNormal = SariPlane.CalculatePlaneNormal();
                        VisPlane.ShowPlane(PlaneNormal, PlanePoint);
                        Mode = "ShowSafety";
                        count = 0;
                    }

                    break;

                }

            case "Wait":
                {
                    wait++;
                    if (wait == 60)
                    {
                        Mode = "Setup";
                        wait = 0;
                    }
                        break;
                }
            case "NoPlane":
                {
                    WithDepth = false;
                    var image = GameObject.Find("ButtonSetup").GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f); ;

                    for (int i = 0; i < MaxHands; i++)
                    {
                        VisFiltered[i].ShowPointsInside(new bool[MaxJoints]);
                    }

                    break;
                }

            #endregion

        #region Safetyzone Visualization

            case "ShowSafety":
                {

                    if (DepthOn)
                        WithDepth = true;
                    else
                        WithDepth = false;
                    GameObject.Find("ButtonSetup").GetComponent<Image>().color = new Color(1, 1, 1);
                    
                    if ((WithDepth) && (counter == 2))
                    {
                        
                        if (Depth != null)
                        {
                            //Debug.Log(Depth[2, 100000]);
                            DepthSafety = SariPlane.UpdateSafetyStatus(Depth);
                            int n = DepthSafety.Length;
                         }
                        counter = 0;
                    }

                    for (int i = 0; i < MaxHands; i++)
                    {

                        FilteredCoord[i].PointsInside = SariPlane.UpdateSafetyStatus(FilteredCoord[i].Coordinates);
                        VisFiltered[i].ShowPointsInside(FilteredCoord[i].PointsInside);
                        
                        PredictedCoord[i].PointsInside = SariPlane.UpdateSafetyStatus(PredictedCoord[i].Coordinates);
                        VisPredicted[i].ShowPointsInside(PredictedCoord[i].PointsInside);
                        
                    }

                    break;
                
                }
            #endregion

        #region Calibration and Recording
            case "Calibration":
                {
                    WithDepth = false;
                    double[,] temp = new double[6, MaxJoints];
                    Array.Copy(RawCoord[0].Coordinates, temp, 6 * MaxJoints);
                    CalibrationData[count].Coordinates = temp;
                    VisPlane.ShowCalibration((float)count / 200);
                    count++;

                    if (count == 200)
                    {
                        double[] StdDev = Sari[0].Calibration(CalibrationData);
                        for (int i = 0; i < MaxHands; i++)
                        {
                            Sari[i].SetR(StdDev);
                        }
                        Debug.Log(StdDev[0]);
                        Debug.Log(StdDev[1]);
                        Debug.Log(StdDev[2]);
                        Debug.Log(StdDev[3]);
                        Debug.Log(StdDev[4]);
                        Debug.Log(StdDev[5]);
                        count = 0;
                        Mode = "Default";
                    }
                    break;
                }

            case "Recording":
                {
                    WithDepth = false;
                    GameObject.Find("ButtonRecording").GetComponent<Image>().color = new Color(1, 0.4f, 0.4f);
                    double[,] temp;
                    if (Live)
                    {
                        temp = new double[6, MaxJoints];
                        Array.Copy(RawCoord[0].Coordinates, temp, 6 * MaxJoints);
                        RecordedData[0, count].Coordinates = temp;
                    }
                    temp = new double[6, MaxJoints];
                    Array.Copy(FilteredCoord[0].Coordinates, temp, 6 * MaxJoints);
                    RecordedData[1, count].Coordinates = temp;
                    temp = new double[6, MaxJoints];
                    Array.Copy(PredictedCoord[0].Coordinates, temp, 6 * MaxJoints);
                    RecordedData[2, count].Coordinates = temp;

                    VisPlane.ShowRecording(count);
                    count++;
                    if (count == Max)
                        Mode = "WritingFile";
                    break;
                }

            case "WritingFile":
                {
                    
                    WithDepth = false;
                    GameObject.Find("ButtonRecording").GetComponent<Image>().color = new Color(1, 1, 1);
                    VisPlane.ShowRecording(60); // 60 sets the RecordingPoint inactive

                    using(StreamWriter Streamwriter = new StreamWriter(@"C:\Users\Daniel\Desktop\HumanRobotInteractionU\Raw"+count2.ToString()+".txt"))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                for (int k = 0; k < MaxJoints-1; k++)
                                {
                                    Streamwriter.Write(RecordedData[0, i].Coordinates[j, k]);
                                    Streamwriter.Write(";");
                                }
                                Streamwriter.Write(RecordedData[0, i].Coordinates[j, MaxJoints-1]);
                                Streamwriter.WriteLine();
                            }
                            Streamwriter.WriteLine();
                        }
                    }
                    using(StreamWriter Streamwriter = new StreamWriter(@"C:\Users\Daniel\Desktop\HumanRobotInteractionU\Filtered" + count2.ToString() + ".txt"))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                for (int k = 0; k < MaxJoints-1; k++)
                                {
                                    Streamwriter.Write(RecordedData[1, i].Coordinates[j, k]);
                                    Streamwriter.Write(";");
                                }
                                Streamwriter.Write(RecordedData[1, i].Coordinates[j, MaxJoints - 1]);
                                Streamwriter.WriteLine();
                            }
                            Streamwriter.WriteLine();
                        }
                    }
                    using (StreamWriter Streamwriter = new StreamWriter(@"C:\Users\Daniel\Desktop\HumanRobotInteractionU\Predicted" + count2.ToString() + ".txt"))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                for (int k = 0; k < MaxJoints-1; k++)
                                {
                                    Streamwriter.Write(RecordedData[2, i].Coordinates[j, k]);
                                    Streamwriter.Write(";");
                                }
                                Streamwriter.Write(RecordedData[2, i].Coordinates[j, MaxJoints - 1]);
                                Streamwriter.WriteLine();
                            }
                            Streamwriter.WriteLine();
                        }
                    }
                    
                    if (WithReplay)
                    {
                        Live = false;
                        Debug.Log(count2);

                        for (int i = 0; i < 6; i++)
                        {
                            //R[i] *= 3;
                            
                        }
                        //Sari[0].SetR(R); 

                        double[] QOrig = { 0.2, 0.2, 0.2, 0.4, 0.4, 0.4 };
                        for (int i = 0; i < 6; i++)
                        {
                            Q[i] *= 5;
                            //R[i] *= 3;
                        }
                        //Sari[0].SetR(R);
                        Sari[0].SetQ(Q);

                        Max = count - 1;
                        count2++;
                        if (count2 < 5)
                        {
                            Mode = "Recording";
                        }
                        else
                        {
                            Mode = "ShowSafety";
                            Live = true;
                            count = 0;
                            count2 = 0;
                            Max = 10000;
                        }
                    }
                    else Mode = "ShowSafety";
                    count = 0;
                    break;

                }

        }
        #endregion

        #region Clean-up for the next frame
        //depth.ReleaseAccess(ImageData);
        //depth.Dispose();
        if (Live)
        {
            for (int i = 0; i < MaxHands; i++)
                RawCoord[i].HandDetected = false;
        }
        #endregion

    }

    #region Setters and getters
    void OnGUI()
    {
        if (WithDepth)
        {
            if (DepthSafety != null)
                VisPlane.ShowDepth(DepthSafety);
        }
    }
    
    public void SetShowPrediction(bool set)
    {
        ShowPrediction = set;
    }

    public void SetShowRaw(bool set)
    {
        ShowRaw = set;
    }

    public void SetShowFiltered(bool set)
    {
        ShowFiltered = set;
    }

    public void SetMode(string set)
    {
        Mode = set;
        if (set=="Setup")
            VisPlane.DeletePlane();

    }

    public void DeletePlane()
    {
        VisPlane.DeletePlane();
    }

    public void SetPredictionTime(float set)
    {
        for (int i = 0; i < MaxHands; i++)
        {
            Sari[i].SetPredictionTime(set);
        }
    }

    public void SetDepthOn(bool set)
    {
        DepthOn = set;
    }
    #endregion

    #region Helper classes
    /* Class to bundle all data for one hand */
    public class HandCoord
    {
        public string name;
        public bool HandDetected = false;
        public double[,] Coordinates;
        public string SafetyStatus;
        public bool[] PointsInside;

        public HandCoord(int row, int column)
        {
           Coordinates = new double[row,column];
        }
    }
    #endregion

}

