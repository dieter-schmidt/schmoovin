using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-mouseandgamepadaimcontroller.html")]
    public class MouseAndGamepadAimController : AimController
    {
        [Header("Mouse Turn")]

        [SerializeField, Tooltip("Number of degrees for 1 unit of mouse movement if sensitivity is set to 0")]
        private float m_MouseTurnAngleMin = 0.25f;

        [SerializeField, Tooltip("Number of degrees for 1 unit of mouse movement if sensitivity is set to 1")]
        private float m_MouseTurnAngleMax = 5f;

        [SerializeField, Tooltip("The transform to calculate the input relative to. Use this to factor tilt into the yaw and pitch input")]
        public Transform m_RelativeTo = null;

        [Header("Mouse Smoothing")]

        [SerializeField, Tooltip("The number of frames to store and use for the mouse smoothing history")]
        private int m_MouseSmoothingBufferSize = 10;

        [SerializeField, Tooltip("The weight multiplier for the previous frame when averaging if the smoothing is set to minimum")]
        private float m_MouseSmoothingMultiplierMin = 0.05f;

        [SerializeField, Tooltip("The weight multiplier for the previous frame when averaging if the smoothing is set to maximum")]
        private float m_MouseSmoothingMultiplierMax = 0.5f;
        
        [Header("Mouse Acceleration")]

        [SerializeField, Tooltip("The base acceleration multiplier when acceleration is set to the minimum.")]
        private float m_MouseAccelSpeedMultiplyMin = 0.001f;

        [SerializeField, Tooltip("The base acceleration multiplier when acceleration is set to the maximum.")]
        private float m_MouseAccelSpeedMultiplyMax = 0.01f;

        [SerializeField, Tooltip("The maximum multiplier acceleration can apply to the mouse input (0 means no maximum)")]
        private float m_MouseAccelerationMax = 0f;
        
        [Header("Gamepad Turn")]

        [SerializeField, Tooltip("Number of degrees per second for the gamepad analog at its limit, if sensitivity is set to 0")]
        private float m_AnalogTurnAngleMin = 45f;

        [SerializeField, Tooltip("Number of degrees per second for the gamepad analog at its limit, if sensitivity is set to 1")]
        private float m_AnalogTurnAngleMax = 90f;

        [SerializeField, Tooltip("The input curve for analog input. This can be used to define a deadzone, and damp smaller movements")]
        private AnimationCurve m_AnalogCurve = new AnimationCurve(new Keyframe[] { new Keyframe (0f, 0.75f, 0f, 0f), new Keyframe (1f, 1f, 0f, 0f) });

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            // Mouse
            m_MouseTurnAngleMin = Mathf.Clamp(m_MouseTurnAngleMin, 0.1f, 10f);
            m_MouseTurnAngleMax = Mathf.Clamp(m_MouseTurnAngleMax, 0.1f, 10f);

            // Mouse Smoothing
            m_MouseSmoothingBufferSize = Mathf.Clamp(m_MouseSmoothingBufferSize, 5, 10);
            m_MouseSmoothingMultiplierMin = Mathf.Clamp(m_MouseSmoothingMultiplierMin, 0.01f, 0.9f);
            m_MouseSmoothingMultiplierMax = Mathf.Clamp(m_MouseSmoothingMultiplierMax, 0.01f, 0.9f);

            // Mouse Acceleration
            m_MouseAccelSpeedMultiplyMin = Mathf.Clamp(m_MouseAccelSpeedMultiplyMin, 0.0001f, 0.1f);
            m_MouseAccelSpeedMultiplyMax = Mathf.Clamp(m_MouseAccelSpeedMultiplyMax, 0.0001f, 0.1f);
            m_MouseAccelerationMax = Mathf.Clamp(m_MouseAccelerationMax, 0f, 20f);

            // Gamepad
            m_AnalogTurnAngleMin = Mathf.Clamp(m_AnalogTurnAngleMin, 15f, 180f);
            m_AnalogTurnAngleMax = Mathf.Clamp(m_AnalogTurnAngleMax, 15f, 180f);
        }
#endif

        private Vector2[] m_SmoothingBuffer = null;
        private int m_SmoothingStartIndex = 0;
        private int m_SmoothingCount = 0;

		public float mouseTurnAngleH
		{
			get { return Mathf.Lerp (m_MouseTurnAngleMin, m_MouseTurnAngleMax, FpsSettings.input.horizontalMouseSensitivity); }
        }
		public float mouseTurnAngleV
		{
			get { return Mathf.Lerp (m_MouseTurnAngleMin, m_MouseTurnAngleMax, FpsSettings.input.verticalMouseSensitivity); }
        }
        
		public float analogTurnAngleH
		{
			get { return Mathf.Lerp (m_AnalogTurnAngleMin, m_AnalogTurnAngleMax, FpsSettings.gamepad.horizontalAnalogSensitivity); }
        }
		public float analogTurnAngleV
		{
			get { return Mathf.Lerp (m_AnalogTurnAngleMin, m_AnalogTurnAngleMax, FpsSettings.gamepad.verticalAnalogSensitivity); }
        }

        protected override void Awake()
        {
            base.Awake ();
            m_SmoothingBuffer = new Vector2[m_MouseSmoothingBufferSize];
        }

        void OnEnable()
        {
            FpsSettings.input.onMouseSettingsChanged += OnMouseSettingsChanged;
            OnMouseSettingsChanged();
        }

        void OnDisable()
        {
            FpsSettings.input.onMouseSettingsChanged -= OnMouseSettingsChanged;
        }

        void OnMouseSettingsChanged()
        {
            ResetSmoothing();
        }

        public void HandleMouseInput (Vector2 input)
		{
			if (!NeoFpsInputManager.captureMouseCursor)
				return;

			// Invert mouse vertical
			if (!FpsSettings.input.invertMouse)
				input.y *= -1f;

            // Acceleration
            if (FpsSettings.input.enableMouseAcceleration)
            {
                float acceleration = FpsSettings.input.mouseAcceleration;
                input = GetAcceleratedMouseInput(input, acceleration);
            }

            // Smoothing
            if (FpsSettings.input.enableMouseSmoothing)
			{
				float smoothing = FpsSettings.input.mouseSmoothing;
                input = GetSmoothedMouseInput(input, smoothing);
            }

            if (m_RelativeTo != null)
            {
                input.x *= mouseTurnAngleH;
                input.y *= mouseTurnAngleV;
                AddRotationInput(input, m_RelativeTo);
            }
            else
                AddRotation (input.x * mouseTurnAngleH, input.y * mouseTurnAngleV);
        }
        
        void ResetSmoothing()
        {
            m_SmoothingStartIndex = -1;
            m_SmoothingCount = 0;
        }

        Vector2 GetSmoothedMouseInput(Vector2 input, float strength)
        {
            // Increment the index
            ++m_SmoothingStartIndex;
            if (m_SmoothingStartIndex == m_SmoothingBuffer.Length)
                m_SmoothingStartIndex = 0;

            // Add input to front of buffer (with frame time to make framerate independent)
            m_SmoothingBuffer[m_SmoothingStartIndex] = input / Time.deltaTime;
            ++m_SmoothingCount;
            
            // Check if first entry
            if (m_SmoothingCount == 1)
                return input;

            // Get the weight multiplier
            float multiplier = Mathf.Lerp(m_MouseSmoothingMultiplierMin, m_MouseSmoothingMultiplierMax, strength);

            // Get smoothed value
            Vector2 accumulated = Vector2.zero;
            float currentMultiplier = 1f;
            float weight = 0f;
            int index = m_SmoothingStartIndex;
            for (int i = 0; i < m_SmoothingCount; ++i)
            {
                accumulated += m_SmoothingBuffer[index] * currentMultiplier;
                weight += currentMultiplier;

                currentMultiplier *= multiplier;

                --index;
                if (index < 0)
                    index += m_SmoothingBuffer.Length;
            }
            return accumulated *= Time.deltaTime / weight;
        }

        Vector2 GetAcceleratedMouseInput(Vector2 input, float strength)
        {
            float speed = input.magnitude / Time.deltaTime;

            float speedMultiplier = Mathf.Lerp(m_MouseAccelSpeedMultiplyMin, m_MouseAccelSpeedMultiplyMax, strength);

            float multiplier = 1f + (speed * speedMultiplier);

            // Clamp the multiplier?
            if (m_MouseAccelerationMax > 1f && multiplier > m_MouseAccelerationMax)
                multiplier = m_MouseAccelerationMax;

            return input * multiplier;
        }

        public void HandleAnalogInput (Vector2 input)
		{
            // Use something other than this
			if (!NeoFpsInputManager.captureMouseCursor)
				return;
            
            // Invert mouse vertical
            float magnitude = Mathf.Clamp01(input.magnitude);
            float multiplier = m_AnalogCurve.Evaluate(magnitude);

            input.x = multiplier * input.x;
			input.y = multiplier * input.y;
            if (!FpsSettings.gamepad.invertLook)
                input.y *= -1f;

            AddRotation (input.x * analogTurnAngleH * Time.deltaTime, input.y * analogTurnAngleV * Time.deltaTime);
		}
	}
}