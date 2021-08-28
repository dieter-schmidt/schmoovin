using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class GamepadUnityInputs
    {
        private string m_AnalogLeftHorizontal = string.Empty;
        private string m_AnalogLeftVertical = string.Empty;
        private string m_AnalogRightHorizontal = string.Empty;
        private string m_AnalogRightVertical = string.Empty;
        private string m_GyroHorizontal = string.Empty;
        private string m_GyroVertical = string.Empty;
        private string[] m_Buttons = { };

        public string name
        {
            get;
            private set;
        }

        public GamepadUnityInputs(
            string gamepad_name,
            string analogLeftHorizontal, string analogLeftVertical,
            string analogRightHorizontal, string analogRightVertical,
            string gyroHorizontal, string gyroVertical,
            string[] buttons)
        {
            name = gamepad_name;
            m_AnalogLeftHorizontal = analogLeftHorizontal;
            m_AnalogLeftVertical = analogLeftVertical;
            m_AnalogRightHorizontal = analogRightHorizontal;
            m_AnalogRightVertical = analogRightVertical;
            m_GyroHorizontal = gyroHorizontal;
            m_GyroVertical = gyroVertical;
            m_Buttons = buttons;
        }

        public float GetLeftAnalogH()
        {
            return Input.GetAxis(m_AnalogLeftHorizontal);
        }

        public float GetLeftAnalogV()
        {
            return Input.GetAxis(m_AnalogLeftVertical);
        }

        public float GetRightAnalogH()
        {
            return Input.GetAxis(m_AnalogRightHorizontal);
        }

        public float GetRightAnalogV()
        {
            return Input.GetAxis(m_AnalogRightVertical);
        }

        public float GetLeftAnalogRawH()
        {
            return Input.GetAxisRaw(m_AnalogLeftHorizontal);
        }

        public float GetLeftAnalogRawV()
        {
            return Input.GetAxisRaw(m_AnalogLeftVertical);
        }

        public float GetRightAnalogRawH()
        {
            return Input.GetAxisRaw(m_AnalogRightHorizontal);
        }

        public float GetRightAnalogRawV()
        {
            return Input.GetAxisRaw(m_AnalogRightVertical);
        }

        public Vector2 GetLeftAnalog()
        {
            return new Vector2(Input.GetAxis(m_AnalogLeftHorizontal), Input.GetAxis(m_AnalogLeftVertical));
        }

        public Vector2 GetRightAnalog()
        {
            return new Vector2(Input.GetAxis(m_AnalogRightHorizontal), Input.GetAxis(m_AnalogRightVertical));
        }

        public Vector2 GetLeftAnalogRaw()
        {
            return new Vector2(Input.GetAxisRaw(m_AnalogLeftHorizontal), Input.GetAxisRaw(m_AnalogLeftVertical));
        }

        public Vector2 GetRightAnalogRaw()
        {
            return new Vector2(Input.GetAxisRaw(m_AnalogRightHorizontal), Input.GetAxisRaw(m_AnalogRightVertical));
        }

        public Vector2 GetGyro()
        {
            return new Vector2(Input.GetAxis(m_GyroHorizontal), Input.GetAxis(m_GyroVertical));
        }

        public bool GetButton(GamepadButton button)
        {
            return Input.GetAxis(m_Buttons[(int)button]) > 0.75;
        }
    }
}