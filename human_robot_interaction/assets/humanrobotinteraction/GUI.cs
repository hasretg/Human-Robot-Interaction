using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;



namespace HumanRobotInteraction
{
    class GUI : MonoBehaviour
    {
        bool IsRecording = false;

        public void ToggleShowPrediction()
        {
            GameObject Toggle;
            Toggle = GameObject.Find("TogglePrediction");
            var Main = GameObject.Find("HumanRobotInteraction").GetComponent<Main>();
            if (Toggle.GetComponent<Toggle>().isOn)
                Main.SetShowPrediction(true);
            else
                Main.SetShowPrediction(false);
        }

        public void ToggleShowFiltered()
        {
            GameObject Toggle;
            Toggle = GameObject.Find("ToggleFiltered");
            var Main = GameObject.Find("HumanRobotInteraction").GetComponent<Main>();
            if (Toggle.GetComponent<Toggle>().isOn)
                Main.SetShowFiltered(true);
            else
                Main.SetShowFiltered(false);
        }

        public void ToggleShowRaw()
        {
            GameObject Toggle;
            Toggle = GameObject.Find("ToggleRaw");
            var Main = GameObject.Find("HumanRobotInteraction").GetComponent<Main>();
            if (Toggle.GetComponent<Toggle>().isOn)
                Main.SetShowRaw(true);
            else
                Main.SetShowRaw(false);
        }


        public void ButtonSafetyZoneSetup()
        {
            var Main = GameObject.Find("HumanRobotInteraction").GetComponent<Main>();
            Main.SetMode("Setup");
        }

        public void ButtonDeletePlane()
        {
            var Main = GameObject.Find("HumanRobotInteraction").GetComponent<Main>();
            Main.DeletePlane();
            Main.SetMode("NoPlane");
        }

        public void ButtonSetDefaultPlane()
        {
            var Main = GameObject.Find("HumanRobotInteraction").GetComponent<Main>();
            Main.SetMode("Default");
        }

        public void ButtonCalibration()
        {
            var Main = GameObject.Find("HumanRobotInteraction").GetComponent<Main>();
            Main.SetMode("Calibration");
        }


        public void SliderSetPredictionTime()
        {
            var Main = GameObject.Find("HumanRobotInteraction").GetComponent<Main>();
            GameObject Slider = GameObject.Find("SliderPredictionTime");
            float value = Slider.GetComponent<Slider>().value;
            Main.SetPredictionTime(value);
        }

        public void ButtonRecording()
        {
            var Main = GameObject.Find("HumanRobotInteraction").GetComponent<Main>();

            if (IsRecording)
            {
                Main.SetMode("WritingFile");
                IsRecording = false;
            }
            else
            {
                Main.SetMode("Recording");
                IsRecording = true;
            }

        }
        public void ToggleDepth()
        {
            GameObject Toggle;
            Toggle = GameObject.Find("ToggleDepth");
            var Main = GameObject.Find("HumanRobotInteraction").GetComponent<Main>();
            if (Toggle.GetComponent<Toggle>().isOn)
                Main.SetDepthOn(true);
            else
                Main.SetDepthOn(false);
        }

    }
}
