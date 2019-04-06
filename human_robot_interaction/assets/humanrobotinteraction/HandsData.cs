using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using HumanRobotInteraction;

public class HandsData
{
    PXCMSenseManager SenseManager = null;   // SenseManager
    pxcmStatus Status;  // Status report
    PXCMHandModule HandModule = null;   // // HandModule Instance
    PXCMHandData HandData;   // Hand data
    PXCMHandData.JointData[][] JointData;
    Main.HandCoord[] Coordinates;
    int NumOfHands;
    int NumOfJoints;

    /* Constructor */
    public HandsData(int MaxHands, int MaxJoints)
    {
        NumOfHands = MaxHands;
        NumOfJoints = MaxJoints;
        JointData = new PXCMHandData.JointData[NumOfHands][];   // Joint coordinates
        Coordinates = new Main.HandCoord[NumOfHands];

        /* Declaration of the array for the hand data*/
        for (int i = 0; i < NumOfHands; i++)
        {
            JointData[i] = new PXCMHandData.JointData[NumOfJoints];
            Coordinates[i] = new Main.HandCoord(6, NumOfJoints);
            for (int j = 0; j < NumOfJoints; j++)
                JointData[i][j] = new PXCMHandData.JointData();
        }

        /* Initialization of SenseManager */
        SenseManager = PXCMSenseManager.CreateInstance();
        if (SenseManager == null)
            Debug.LogError("Initialization of the SenseManager has failed");

        /* Enable hand tracking and get an instance of an hand module */
        Status = SenseManager.EnableHand();
        HandModule = SenseManager.QueryHand();
        if (Status != pxcmStatus.PXCM_STATUS_NO_ERROR)
            Debug.LogError("SenseManager --> EnableHand " + Status);

        /* Create the connection to the Intel RealSense camera */
        Status = SenseManager.Init();
        if (Status != pxcmStatus.PXCM_STATUS_NO_ERROR)
            Debug.LogError("Initialization of SenseManager: " + Status);



        /* Settings for the hand module */
        PXCMHandConfiguration HandConfig = HandModule.CreateActiveConfiguration();
        if (HandConfig != null)
        {
            HandConfig.EnableAllGestures();
            HandConfig.EnableAlert(PXCMHandData.AlertType.ALERT_HAND_NOT_DETECTED);

            PXCMHandData.JointType st = PXCMHandData.JointType.JOINT_WRIST;
            for (int i = 0; i < NumOfJoints; i++)
            {

                HandConfig.EnableJointSpeed(st, PXCMHandData.JointSpeedType.JOINT_SPEED_AVERAGE, 1000 / 10);
                st++;
            }

            //HandConfig.EnableJointSpeed(PXCMHandData.JointType.JOINT_WRIST, PXCMHandData.JointSpeedType.JOINT_SPEED_ABSOLUTE,1000/20);
            HandConfig.ApplyChanges();
            HandConfig.Dispose();
        }
    }

    public Main.HandCoord[] GetJointCoordinates()
    {
        #region Catch exception errors
        /* Wait until frame is available and SenseManager is ready */

        /* Make sure SenseManager has an instance */
        if (SenseManager == null)
            return null;

        /* Wait until any frame data is available */
        if (SenseManager.AcquireFrame(true) != pxcmStatus.PXCM_STATUS_NO_ERROR)
            return null;

        #endregion

        #region Get joint coordinates
        HandModule = SenseManager.QueryHand();  // Get hand tracking Module instance
        HandData = HandModule.CreateOutput();   // Get hand tracking data

        if (HandData != null)
        {
            HandData.Update();  // Update hand data to the most current output
            /*Reset the detected hands to zero*/
            for (int i = 0; i < NumOfHands; i++)
            {
                Coordinates[i].HandDetected = false;
            }

            /* Retrieve all joint data */
            for (int i = 0; i < HandData.QueryNumberOfHands(); i++)
            {
                Coordinates[i].HandDetected = true;
                PXCMHandData.IHand iHand;
                Status = HandData.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_FIXED, i, out iHand);
                if (Status != pxcmStatus.PXCM_STATUS_NO_ERROR)
                    Debug.LogError(Status);
                else
                {
                    for (int j = 0; j < NumOfJoints; j++)
                    {
                        Status = iHand.QueryTrackedJoint((PXCMHandData.JointType)j, out JointData[i][j]);
                        if (Status == pxcmStatus.PXCM_STATUS_NO_ERROR)
                        {
                            Coordinates[i].Coordinates[0, j] = JointData[i][j].positionWorld.x * -100;
                            Coordinates[i].Coordinates[1, j] = JointData[i][j].positionWorld.y * 100;
                            Coordinates[i].Coordinates[2, j] = JointData[i][j].positionWorld.z * 100;
                            Coordinates[i].Coordinates[3, j] = JointData[i][j].speed.x * -100;
                            Coordinates[i].Coordinates[4, j] = JointData[i][j].speed.y * -100;
                            Coordinates[i].Coordinates[5, j] = JointData[i][j].speed.z * 100;
                        }

                    }
                }
            }
        }

        SenseManager.ReleaseFrame();    // Prepare for a new frame 

        return Coordinates;
        #endregion

    }

    public bool IsGestureFired(string Gesture)
    {
        PXCMHandData.GestureData data;
        return HandData.IsGestureFired(Gesture, out data);
    }
}