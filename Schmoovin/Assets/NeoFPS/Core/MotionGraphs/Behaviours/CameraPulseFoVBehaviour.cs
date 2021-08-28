using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS
{
    [MotionGraphElement("Camera/CameraPulseFoVBehaviour", "CameraPulseFoVBehaviour")]
    public class CameraPulseFoVBehaviour : MotionGraphBehaviour
    {
        [SerializeField, Tooltip("When should the camera FoV pulse be triggered.")]
        private When m_When = When.OnEnter;
        [SerializeField, Tooltip("The FoV multiplier to apply to the camera when the animation curve Y-axis is at 1.")]
        private float m_FovMultiplier = 1.2f;
        [SerializeField, Tooltip("The duration in seconds for the pulse to last.")]
        private float m_PulseDuration = 1f;
        [SerializeField, Tooltip("A curve for the strength of the pulse. X is normalised time. Y = 0 means the FoV is 1x (no effect), Y = 1 means the FoV is the target FoV multiplier")]
        private AnimationCurve m_PulseCurve = new AnimationCurve( new Keyframe[] {
            new Keyframe(0f, 0f), new Keyframe (0.1f, 1f), new Keyframe(1f, 0f)
        });

        private FirstPersonCamera m_FirstPersonCamera = null;

        enum When
        {
            OnEnter,
            OnExit
        }

        public override void OnValidate()
        {
            m_FovMultiplier = Mathf.Clamp(m_FovMultiplier, 0.1f, 2f);
            m_PulseDuration = Mathf.Clamp(m_PulseDuration, 0.1f, 60f);

            var keys = m_PulseCurve.keys;
            if (keys.Length < 2)
            {
                var newKeys = new Keyframe[2];
                if (keys.Length == 1)
                    newKeys[0] = keys[0];
                else
                    newKeys[0] = new Keyframe(0f, 1f);
                newKeys[1] = new Keyframe(1f, 0f);
                m_PulseCurve.keys = newKeys;
            }
            else
            {
                keys[0].time = 0f;
                keys[keys.Length - 1].time = 1f;
            }
        }

        public override void Initialise(MotionGraphConnectable o)
        {
            base.Initialise(o);
            m_FirstPersonCamera = controller.GetComponentInChildren<FirstPersonCamera>();
            m_PulseCurve.preWrapMode = WrapMode.ClampForever;
            m_PulseCurve.postWrapMode = WrapMode.ClampForever;
        }

        void Pulse()
        {
            if (m_FirstPersonCamera != null)
                m_FirstPersonCamera.PulseFoV(m_PulseCurve, m_FovMultiplier, m_PulseDuration);
        }

        public override void OnEnter()
        {
            if (m_When != When.OnExit)
                Pulse();
        }

        public override void OnExit()
        {
            if (m_When != When.OnEnter)
                Pulse();
        }
    }
}