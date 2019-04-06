using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using Vector = MathNet.Numerics.LinearAlgebra.Vector<double>;


namespace HumanRobotInteraction
{
    class EKF
    {
        /*Declaration of the class variables */
        private Matrix A;   // Process or State transition matrix
        private Matrix B;   // Control matrix
        private Matrix H;   // Measurement or observation matrix
        private Matrix Q;   // Process error covariance 
        private Matrix R;   // Measurement error covariance

        private Vector X;   // State matrix
        private Vector XPredict;   // State matrix prediction
        private Vector XPredictVis;   // State matrix prediction
        private Matrix P;   // State covariance
        private Matrix PPredict;   // State covariance prediction

        private int PredictionTimeMax = 100;
        private int PredictionTime = 20;

        /* Constructor */
        public EKF(Matrix inA, Matrix inB, Matrix inH, Matrix inQ, Matrix inR)
        {
            /* Set the class variables to the input */
            A = inA;
            B = inA;
            H = inH;
            Q = inQ;
            R = inR;
    }

    /* Set the start values for the EKF for state matrix x and covariance matrix P */
    public void Initialization(Vector x0, Matrix P0)
        {
            /* Set the initial or approximate start values */
            X = x0;
            P = P0;
        }

        /* Predict the state and covariance with previous x and P */
        public Result Prediction(Vector U)  //Control matrix - indicates the magnitudes of any control system's or user's control on the situation
        {
            XPredict = A * X + B * U;    //  State prediction
            PPredict = A * P * A.Transpose() + Q;    // Covariance prediction
            return new Result(XPredict, PPredict);
        }

        /* Update the prediction with measurements */ 
        public Result Correct(Vector Z)  // Z - Measurement or observation matrix - Contains the real world measurements in this timestep
        {
            Vector Y = Z - H * XPredict;   // Innovation or residual - Compare reality against prediction
            Matrix S = H * PPredict * H.Transpose() + R;   // Innocation Covariance - Compare real error against prediction
            Matrix K = PPredict * H.Transpose() * S.Inverse(); // Kalman Gain - Moderate the prediction
            int n = K.RowCount;   // Number of rows, for building the identity matrix

            Vector XNew = XPredict + K * Y; // State Update - Current estimate of the state
            Matrix PNew = (Matrix.Build.DenseIdentity(n) - K * H) * PPredict;   // Covariance Update - Current estimate of the error of the sate

            X = XNew;   // Set the old x to the new one - Recursion
            P = PNew;   // Set the old x to the new one - Recursion

            return new Result(XNew, PNew);
        }

        /* Predict the state and covariance with previous x and P */
        public Vector PredictionVisualization(Vector U)  //Control matrix - indicates the magnitudes of any control system's or user's control on the situation
        {
            XPredictVis = X;
            for (int i = 0; i < PredictionTime; i++)
            {
                XPredictVis = A * XPredictVis + B * U;    //  State prediction
            }
            return XPredictVis;
        }

        public void SetPredictionTime(float set)
        {
            PredictionTime = (int)(PredictionTimeMax*set);
        }

        public void SetR(Matrix RIn)
        {
            R = RIn;
        }

        public void SetQ(Matrix QIn)
        {
            Q = QIn;
        }
        /* Class for the result */
        public class Result
        {
            private Vector outX;
            private Matrix outP;

            /* Constructor */
            public Result(Vector X, Matrix P)
            {
                this.outX = X;
                this.outP = P;
            }

            /* Getter for x */
            public Vector getX()
            {
                return outX;
            }

            /* Getter for P */
            public Matrix getP()
            {
                return outP;
            }
        }
    }
}
