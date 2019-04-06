using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


namespace HumanRobotInteraction
{
    class Visualization
    {
        /*Declaration of the Game Objects*/
        GameObject Hand;    // Empty Unity Game Object as parent for the joints and bones (of type GameObject)
        Joint[] Joints; // Vector for all Joints (of type GameObjects/Joint)
        Bone[] Bones;   // Vector for all Bones (of type GameObjects/Joint)
        HandCenter HandCenter;  // GameObject for the center of the hand

        GameObject Plane;
        GameObjects[] PlanePoints;
        GameObject PlaneVis;

        GameObject Slider;
        GameObject RecordingPoint;

        Texture2D Texture;
        Color InsideColor;

        private int MaxJoints;  // Number of maximal detectable joints
        private double[,] ActualCoordinates;

        public Visualization(string Name)
        {
            #region Plane Setup

            Plane = new GameObject();
            Plane.name = Name;

            PlanePoints = new GameObjects[3];

            for (int i = 0; i < 3; i++)
            {
                PlanePoints[i] = new GameObjects("Plane Point " + i.ToString(), Plane, PrimitiveType.Sphere, new Color(1, 0.5f, 0.5f));
                PlanePoints[i].SetActive(false);
                PlanePoints[i].SetScale(2);
            }

           
            PlaneVis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            PlaneVis.name = "Plane Visualization";
            PlaneVis.transform.parent = Plane.transform;
            PlaneVis.transform.localScale = new Vector3(0.01f, 500, 500);
            //PlaneVis.GetComponent<Renderer>().material.shader = 
            var material = PlaneVis.GetComponent<Renderer>().material;
            material.SetFloat("_Mode", 3.0f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000; 
            PlaneVis.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.5412f,0.1686f, 0.8863f, 0.4f));

            //PlaneVis.GetComponent<Renderer>().material.color = new Color(1.0f,0.0f,0.0f,0.3f);
            PlaneVis.SetActive(false);

            Slider = GameObject.Find("SliderCalibration");
            Slider.SetActive(false);

            RecordingPoint = GameObject.Find("RecordingPoint");
            RecordingPoint.SetActive(false);

            Texture = new Texture2D(64,48);
            #endregion
        }
        /* Constructor - Setup for Camera and Hand-GameOjects */
        public Visualization(string Name, int NumOfJoints, Color JointColor, Color BoneColor, Color CenterColor, Color InsideColorIn)
        {
            #region Hand Setup
            /* Instantiation of all GameObjects and define the parents */
            MaxJoints = NumOfJoints;

            /* Instantiation of all GameObjects */
            Hand = new GameObject();
            Hand.name = Name;

            Joints = new Joint[MaxJoints];
            Bones = new Bone[MaxJoints];
            HandCenter = new HandCenter("Hand Center", Hand, PrimitiveType.Sphere,CenterColor);
            /* Instantiation and set the name and parent of each GameObject */
            for (int i = 0; i < MaxJoints; i++)
            {
                Joints[i] = new Joint("Joint " + (i+1).ToString(),Hand, PrimitiveType.Sphere, JointColor);
                Bones[i] = new Bone("Bone " + (i+1).ToString(),Hand, PrimitiveType.Capsule, BoneColor);
            }

            InsideColor = InsideColorIn;

            Hand.SetActive(false);  // Hide all GameObjects
            #endregion
        }

        public void UpdateHand(bool HandDetected)
        {
            if (HandDetected)
                Debug.LogError("Error, no data for hand update!");
            else
            {
                Hand.SetActive(false); // If the hand is not detected, set it to unvisible
            }
               
        }

        /* Function to change the position of all GameOjects to the current position */
        public void UpdateHand(double[,] FilteredCoord, bool HandDetected)
        {
            ActualCoordinates = FilteredCoord;
            if (HandDetected) // change position only if only if hand is detected
            {
                Hand.SetActive(true);
                Joints[1].SetActive(false);
                Bones[0].SetActive(false);
                for (int i = 0; i < MaxJoints; i++)
                {
                    if (i == 1) // i = 1 is the index for the hand center
                        HandCenter.SetPosition(GetVector(i));
                    else // The other indexes are normal joints
                        Joints[i].SetPosition(GetVector(i));

                    if (i != 0 && i != 1 && i != 2 && i != 6 && i != 10 && i != 14 && i != 18) // Set bones between two adjacent joints, but not on the fingertips
                        Bones[i].SetPosition(GetVector(i-1), GetVector(i));

                    /* Set bones between two joints */
                    Bones[2].SetPosition(GetVector(0), GetVector(2));
                    Bones[6].SetPosition(GetVector(2), GetVector(6));
                    Bones[10].SetPosition(GetVector(6), GetVector(10));
                    Bones[14].SetPosition(GetVector(10), GetVector(14));
                    Bones[18].SetPosition(GetVector(14), GetVector(18));
                    Bones[1].SetPosition(GetVector(0), GetVector(18));
                }
            }
            else
            {
                Hand.SetActive(false); // If the hand is not detected, set it to unvisible
            }
        }
        public Vector3 GetVector(int i)
        {
            return new Vector3((float)ActualCoordinates[0, i], (float)ActualCoordinates[1, i], (float)ActualCoordinates[2, i]);
        }

        public void SetColorOfHand(Color ColorHand)
        {
            HandCenter.SetColor(ColorHand);
            for (int i = 0; i < MaxJoints; i++)
            {
                Joints[i].SetColor(ColorHand);
                Bones[i].SetColor(ColorHand);
            }
        }

        public void ShowPlanePoint(double[] PlanePoint, int i)
        {
            PlanePoints[i].SetColor(new Color(1, 0.5f, 0.5f));
            PlanePoints[i].SetPosition(new Vector3((float)PlanePoint[0], (float)PlanePoint[1], (float)PlanePoint[2]));
            PlanePoints[i].SetActive(true);
        }

        public void ConfirmPlanePoint(int i)
        {
            PlanePoints[i].SetColor(new Color(1,0,0));
        }


        public void DeletePlanePoint(int i)
        {
            PlanePoints[i].SetActive(false);
        }

        public void ShowPlane(double[] NormalVector, double[] PointVector)
        {
            Vector3 Normal = new Vector3((float)NormalVector[0], (float)NormalVector[1], (float)NormalVector[2]);
            Vector3 Point = new Vector3((float)PointVector[0], (float)PointVector[1], (float)PointVector[2]);

            PlaneVis.transform.position = Point;
            PlaneVis.transform.localRotation = Quaternion.FromToRotation(new Vector3(1,0,0), Normal);
            PlaneVis.SetActive(true);
            for (int i = 0; i < 3; i++)
            {
                PlanePoints[i].SetActive(false);
            }
        }

        public void DeletePlane()
        {
            PlaneVis.SetActive(false);
            for (int i = 0; i < 3; i++)
            {
                PlanePoints[i].SetActive(false);
            }
        }

        public void ShowPointsInside(bool[] PointsInside)
        {
            for (int i = 0; i < MaxJoints; i++)
            {
                if (PointsInside[i])
                {
                    Joints[i].SetColor(InsideColor);
                    if (i != 1)
                        Bones[i].SetColor(InsideColor);
                    else
                        HandCenter.SetColor(InsideColor);
                    if (i == 18)
                        Bones[1].SetColor(InsideColor);
                }
                else
                {
                    Joints[i].SetDefaultColor();
                    if (i != 1)
                        Bones[i].SetDefaultColor();
                    else
                        HandCenter.SetDefaultColor();
                    if (i == 18)
                        Bones[1].SetDefaultColor();
                }
                    
            }
        }

        public void ShowCalibration(float progress)
        {
            Slider.SetActive(true);
            Slider.GetComponent<Slider>().value = progress;
            if (progress > 0.99)
            {
                Slider.SetActive(false);
            }
        }
        public void ShowRecording(int count)
        {
            if (count % 60 == 0)
                RecordingPoint.SetActive(false);
            else if (count % 30 == 0)
                RecordingPoint.SetActive(true);

        }

        public void ShowDepth(bool[] SafetyDepth)
        {
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0;j < 48; j++)
                {
                    if (SafetyDepth[(48-j-1)*64 + i])
                        Texture.SetPixel(i, j, new Color(1, 0, 0));
                    else
                        Texture.SetPixel(i, j, new Color(1, 1, 1));
                }

            }
            Texture.Apply();
            UnityEngine.GUI.DrawTexture(new Rect(0, 0, 240 , 180), Texture);
        }

    }

    class GameObjects
    {
        protected GameObject GameObj;
        protected Color DefaultColor;

        public GameObjects(string GameObjectName, GameObject Hand, PrimitiveType Type, Color Color)
        {
            GameObj = GameObject.CreatePrimitive(Type);
            GameObj.name = GameObjectName;  // Set the name of the GameObject
            GameObj.transform.parent = Hand.transform;
            DefaultColor = Color;
        }

        public void SetPosition(Vector3 Pos)
        {
            GameObj.transform.position = Pos;
        }

        public void SetActive(bool show)
        {
            GameObj.SetActive(show);
        }

        public void SetColor(Color Color)
        {
            GameObj.GetComponent<Renderer>().material.color = Color;
        }
        public void SetDefaultColor()
        {
            GameObj.GetComponent<Renderer>().material.color = DefaultColor;
        }
        public void SetScale(int Scale)
        {
            GameObj.transform.localScale = new Vector3(Scale, Scale, Scale);
        }
    }

    /* Extension of class GameObject, specific for the type Bone */
    class Bone: GameObjects
    {
        /* Constructor - Set the type and color of GameObject */
        public Bone(string BoneName, GameObject Hand, PrimitiveType Type, Color Color) : base(BoneName, Hand,Type, Color)
        {
               // Set the type to Capsule
            GameObj.GetComponent<Renderer>().material.color = Color;    // Set the color
        }

        /* Function to change the position, scale and the orientation of the bone, overload for base function */
        public void SetPosition(Vector3 PointPrev, Vector3 PointNext)
        {

            GameObj.transform.position = ((PointNext - PointPrev) / 2f) + PointPrev;   // Update Position 
            GameObj.transform.localScale = new Vector3(1.2f, (PointNext - PointPrev).magnitude - (PointPrev - PointNext).magnitude / 2f, 1.2f);    // Update Scale
            GameObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, PointNext - PointPrev); // Update rotation
        }
    }
    /* Extension of class GameObject, specific for the type Joint */
    class Joint : GameObjects
    {
        /* Constructor - Set the type and color of GameObject */
        public Joint(string JointName, GameObject Hand, PrimitiveType Type, Color Color) : base(JointName, Hand, Type, Color)
        {
            GameObj.GetComponent<Renderer>().material.color = Color;
        }
    }

    /* Extension of class GameObject, specific for the type Hand Center */
    class HandCenter : GameObjects
    {
        /* Constructor - Set the type and color of GameObject */
        public HandCenter(string CenterName, GameObject Hand, PrimitiveType Type, Color Color) : base(CenterName, Hand, Type, Color)
        {
            GameObj.GetComponent<Renderer>().material.color = Color;
        }
    }
}
