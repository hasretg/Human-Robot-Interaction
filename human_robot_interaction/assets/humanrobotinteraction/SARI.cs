using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;
namespace HumanRobotInteraction
{
    class SARI
    {
        #region Declarations
        Matrix A;   // State transition matrix
        Matrix B;   // Control Matrix
        Matrix H;   // Observation Matrix
        Matrix Q;   // Estimated process error covariance
        Matrix R;   // Estimated measurement error covariance

        Matrix U;   // Control data
        Matrix Z;   // Observation data

        Matrix XPredict;    // Predicted Coordinates
        Matrix XPredictVisualization;    // Predicted Coordinates for Visualization
        Matrix XUpdate;    // Filtered Coordinates

        EKF[] Filter; // Extended kalman filter

        Vector PlaneNormal; // Normal Vector of the safety plane
        Vector DefaultPlaneNormal;
        Vector DefaultPlanePoint;
        Matrix PlanePoints; // Matrix for the 3 points which define the safety plane
        Vector InitializationPoint; //Potential new point for the plane, reference point to check if the hand stays for e few seconds. 
        private int count = 0;

        bool[] PointsInside;
        int NumOfJoints;    // Number of HandJoints

        #endregion

        #region Constructors

        /* Constructor 1 for Plane */
        public SARI()
        {
            PlanePoints = Matrix.Build.DenseDiagonal(3,3,0);    // Preinstatiate the 3 points which define the plane
            InitializationPoint = Vector.Build.Dense(3, 0);
        }

        /* Constructor 2 for hand tracking*/
        public SARI(int inNumOfJoints)
        {
            NumOfJoints = inNumOfJoints;    // Variable of the available number of joints
            Filter = new EKF[NumOfJoints];  // Preinstatiate a new EKF Filter
        }

        #endregion

        #region Execution of the EKF
        /* Set the functional model */
        public void InitializeFilter()
        {
            double dt = 1.0/60.0; // Time between frames
            double[,] aTemp = { {1, 0, 0, dt, 0, 0},    // Matrix for the functional model: Xnew = Xold + V*dt ; X = [x, y, z, vx, vy, vz]
                                {0, 1, 0, 0, dt, 0},
                                {0, 0, 1, 0, 0, dt},
                                {0, 0, 0, 1, 0, 0},
                                {0, 0, 0, 0, 1, 0},
                                {0, 0, 0, 0, 0, 1 } };

            /* Define the matrices */
            A = Matrix.Build.DenseOfArray(aTemp);
            B = Matrix.Build.DenseIdentity(6);  
            H = Matrix.Build.DenseIdentity(6);
            /* double[,] qTemp = { {2*dt*dt, 0, 0, 3*dt, 0, 0},    // Matrix for the functional model: Xnew = Xold + V*dt ; X = [x, y, z, vx, vy, vz]
                                {0, 2*dt*dt, 0, 0, 3*dt, 0},
                                {0, 0, 2*dt*dt, 0, 0, 3*dt},
                                {3*dt, 0, 0, 6, 0, 0},
                                {0, 3*dt, 0, 0, 6, 0},
                                {0, 0, 3*dt, 0, 0, 6 } };            //Q.SetSubMatrix(3, 3, 3, 3,Matrix.Build.DenseDiagonal(3, 2));
            Q = Matrix.Build.DenseOfArray(qTemp);
            Q = 50 * 50 * dt / 6 * Q; */
            Q = Matrix.Build.DenseDiagonal(6, 0.002);
            double[] rTemp = {0.1, 0.1, 0.1, 0.7, 0.7, 0.7};
            R = Matrix.Build.DenseOfDiagonalArray(rTemp);
            R = R.PointwisePower(2);
            // R = Matrix.Build.DenseDiagonal(6, 2); // Uncorrelated and variance = 0.2
            // R.SetSubMatrix(3, 3, 3, 3, Matrix.Build.DenseDiagonal(3, 20));


            U = Matrix.Build.DenseDiagonal(6, NumOfJoints, 0);  // Control data zero because the system doesn't have any user influence

            Vector X0 = Vector.Build.Dense(6, 0);  // Initial values for X = 0
            Matrix P0 = Matrix.Build.DenseDiagonal(6,1);  // Initial values for P = 0

            XPredict = Matrix.Build.DenseDiagonal(6, NumOfJoints, 0);
            XPredictVisualization = Matrix.Build.DenseDiagonal(6,NumOfJoints, 0);
            XUpdate = Matrix.Build.DenseDiagonal(6, NumOfJoints, 0);

            for (int i = 0; i < NumOfJoints; i++)
            {
                Filter[i] = new EKF(A, B, H, Q, R);    // Instance of extended kalman filter
                Filter[i].Initialization(X0, P0);  // Setting the initial values for the filter
            }            
        }
        /* One calculation step consisting of prediction and correction */
        public double[,] UpdateFilter(double[,] RawCoord)
        {
            Z = Matrix.Build.DenseOfArray(RawCoord);

            /* Execute the filter */
            for (int i = 0; i < NumOfJoints; i++)
            {
                EKF.Result Prediction = Filter[i].Prediction(U.Column(i));
                XPredict.SetColumn(i, Prediction.getX());

                EKF.Result Update = Filter[i].Correct(Z.Column(i));
                XUpdate.SetColumn(i, Update.getX());
            }
            return XUpdate.ToArray();
        }

        public double[,] PredictFilterVisualization()
        {
            for (int i = 0; i < NumOfJoints; i++)
            {
                Vector PredictionVisualization = Filter[i].PredictionVisualization(U.Column(i));
                XPredictVisualization.SetColumn(i, PredictionVisualization);
            }
            return XPredictVisualization.ToArray();
        }
        #endregion

        #region Setup of the Safety Plane
        /* Chech if the tip of the index finger stays at a position for a few seconds and then give back the inital position */
        public double[] GetPlanePoint(double[] Pos)
        {
            Vector ActualPos = Vector.Build.DenseOfArray(Pos);
            double[] Res = null;    // Instatiation of the result, is null when the hand did not yet stay for long enough

            Vector Diff = ActualPos - InitializationPoint;  // Difference vector to check how fat the hand moved
            if (Diff.L2Norm() > 2)  // If the hand moved more then 1 cm
            {
                InitializationPoint = ActualPos;    // Set the new position to the potential new plane point
                count = 0;  // Start countinh anew
            }
            else
            {
                count++;    //Increase the counter to check for how long it stays there
            }

            if (count == 20)    // If the hand stayed there for 50 frames, a new point is found
            {
                Res = InitializationPoint.ToArray();    
                count = 0;
            }
            return Res;
        }

        /* If the point is really a new point of the plane, confirm it by writing it into the matrix "PlanePoints" */
        public void ConfirmPoint(int i)
        {
            PlanePoints.SetColumn(i, InitializationPoint);
            InitializationPoint = Vector.Build.Dense(3,0);
        }

        public double[] CalculatePlaneNormal()
        {

            Vector Vector1 = PlanePoints.Column(1) - PlanePoints.Column(0);
            Vector Vector2 = PlanePoints.Column(2) - PlanePoints.Column(0);

            PlaneNormal = CrossProduct(Vector1, Vector2);
            Vector InfPoint = Vector.Build.Dense(3,0);
            InfPoint[2] = 10000;
            if ((InfPoint * PlaneNormal) < 0)
                PlaneNormal = PlaneNormal * (-1);

            return PlaneNormal.ToArray();
        }

        #endregion

        #region Get Safety Status

        public bool[] UpdateSafetyStatus(double[,] Pos)
        {
            int n = Pos.GetLength(1);
            PointsInside = new bool[n];
            Matrix ActualPos = Matrix.Build.DenseOfArray(Pos);
            ActualPos = ActualPos.RemoveRow(5);
            ActualPos =  ActualPos.RemoveRow(4);
            ActualPos = ActualPos.RemoveRow(3);
            for (int i = 0; i < n; i++)
            {   
                double DotProduct = PlaneNormal * (ActualPos.Column(i) - PlanePoints.Column(2));
                if (DotProduct >= 0)
                    PointsInside[i] = false;
                else if (ActualPos[0, i] == 0 && ActualPos[1, i] ==0  && ActualPos[2, i]==0)
                    PointsInside[i] = false;
                else
                    PointsInside[i] = true;
            }
            return PointsInside;
        }

        public string GetSafetyStatus()
        {
            return null;
        }

        public double[] GetVisualizationsStatus()
        {

            return null;
        }

        public void SetDefaultPlaneNormal(double[] DefaultPlaneNormal)
        {
            PlaneNormal = Vector.Build.DenseOfArray(DefaultPlaneNormal);
        }

        public void SetDefaultPlanePoint(double[] DefaultPlanePoint)
        {
            PlanePoints.SetColumn(2, DefaultPlanePoint);
        }

        #endregion

        #region Calibration

        public double[] Calibration(Main.HandCoord[] HandDataIn)
        {
            int n = HandDataIn.Length;
            Matrix[] HandData = new Matrix[n];
            Matrix Summe = Matrix.Build.Dense(6, NumOfJoints, 0);
            for (int i = 0; i < n; i++)
            {
                HandData[i] = Matrix.Build.DenseOfArray(HandDataIn[i].Coordinates);

                Summe = Summe + HandData[i];
            }

            Summe = Summe.Divide(n);

            Matrix StdDev = Matrix.Build.Dense(6, NumOfJoints, 0);

            for (int i = 0; i < n; i++)
            {
                Matrix Diff = Summe - HandData[i];
                StdDev = StdDev + Diff.PointwisePower(2);   // Sum of squared deviation from the mean
            }
            StdDev = StdDev.Divide(n-1);  // Divide by the number of samples
            StdDev = StdDev.PointwisePower(0.5);    // Variance to Standard Deviation
            Vector Res = StdDev.RowSums() / NumOfJoints;  // Mean over all landmarks

            return Res.ToArray();
        }

        public void SetR(double[] Calib)
        {
            R = Matrix.Build.DenseOfDiagonalArray(Calib);
            R = R.PointwisePower(2);
                for(int i = 0; i < NumOfJoints; i++)
            {
                Filter[i].SetR(R);
            }
        }

        public void SetQ(double[] QIn)
        {
            Q = Matrix.Build.DenseOfDiagonalArray(QIn);
            Q = Q.PointwisePower(2);
            for (int i = 0; i < NumOfJoints; i++)
            {
                Filter[i].SetQ(Q);
            }
        }
        #endregion

        #region Helper Functions

        public void SetPredictionTime(float set)
        {
            for (int i = 0; i < NumOfJoints; i++)
            {
                Filter[i].SetPredictionTime(set);

            }
        }

        public Vector CrossProduct(Vector a, Vector b)
        {
            Vector Res = Vector.Build.Dense(3, 0);

            Res[0] = a[1] * b[2] - a[2] * b[1];
            Res[1] = a[2] * b[0] - a[0] * b[2];
            Res[2] = a[0] * b[1] - a[1] * b[0];
            return Res;
        }
        #endregion
    }
}
