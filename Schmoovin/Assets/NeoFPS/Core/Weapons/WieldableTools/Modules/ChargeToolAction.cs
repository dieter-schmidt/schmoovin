using UnityEngine;

namespace NeoFPS.WieldableTools
{
    public class ChargeToolAction : BaseWieldableToolModule
    {
        [SerializeField, Tooltip("The time it takes to reach full charge.")]
        private float m_ChargeDuration = 3f;

        [Header("Audio")]

        [SerializeField, Tooltip("The audio to play while charging.")]
        private AudioClip m_ChargeLoop = null;
        [SerializeField, Range(0f, 1f), Tooltip("The volume of the charging audio loop.")]
        private float m_Volume = 1f;
        [SerializeField, Tooltip("The pitch of the audio loop at the start of the charge.")]
        private float m_StartPitch = 0.75f;
        [SerializeField, Tooltip("The pitch of the audio loop when charge hits 100%.")]
        private float m_EndPitch = 1.25f;

        [Header("Animation")]

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Bool, true, false), Tooltip("A bool parameter in the tool's animator controller that should be set while charging.")]
        private string m_IsChargingBool = string.Empty;
        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Float, true, false), Tooltip("A float parameter in the tool's animator controller used to show charge progress (0-1).")]
        private string m_ChargeFloat = string.Empty;

        private AudioSource m_AudioSource = null;
        private Animator m_Animator = null;
        private int m_IsChargingHash = -1;
        private int m_ChargeHash = -1;
        private float m_ChargeValue = 0f;
        private bool m_Completed = false;

        public override bool isValid
        {
            get { return true; }
        }

        public override WieldableToolActionTiming timing
        {
            get { return k_TimingsAll; }
        }

        private void OnValidate()
        {
            m_ChargeDuration = Mathf.Clamp(m_ChargeDuration, 0.1f, 100f);
            m_StartPitch = Mathf.Clamp(m_StartPitch, 0.1f, 2f);
            m_EndPitch = Mathf.Clamp(m_EndPitch, 0.1f, 2f);
        }

        public override void Initialise(IWieldableTool t)
        {
            base.Initialise(t);

            // Set up animation
            m_Animator = GetComponentInChildren<Animator>();
            if (m_Animator != null)
            {
                if (!string.IsNullOrWhiteSpace(m_IsChargingBool))
                    m_IsChargingHash = Animator.StringToHash(m_IsChargingBool);
                if (!string.IsNullOrWhiteSpace(m_ChargeFloat))
                    m_ChargeHash = Animator.StringToHash(m_ChargeFloat);
            }

            // Set up audio
            if (m_ChargeLoop != null)
            {
                m_AudioSource = GetComponent<AudioSource>();
                if (m_AudioSource != null)
                {
                    m_AudioSource.loop = true;
                    m_AudioSource.pitch = m_StartPitch;
                    m_AudioSource.volume = m_Volume;
                    m_AudioSource.clip = m_ChargeLoop;
                }
            }
        }

        public override void FireStart()
        {
            // Start animation
            if (m_IsChargingHash != -1)
                m_Animator.SetBool(m_IsChargingHash, true);

            // Start audio loop
            if (m_AudioSource != null)
            {
                m_AudioSource.pitch = m_StartPitch;
                m_AudioSource.Play();
            }

            m_ChargeValue = 0f;
            m_Completed = false;
        }

        public override void FireEnd(bool success)
        {
            // Stop animation
            if (m_IsChargingHash != -1)
                m_Animator.SetBool(m_IsChargingHash, false);

            // Stop audio
            if (m_AudioSource != null)
                m_AudioSource.Stop();

            m_Completed = false;
        }

        public override bool TickContinuous()
        {
            if (m_Completed)
            {
                Debug.Log("Interrupt");
                tool.Interrupt();
                return true;
            }
            else
            {
                // Increment charge
                m_ChargeValue += Time.deltaTime / m_ChargeDuration;
                if (m_ChargeValue > 1f)
                {
                    m_ChargeValue = 1f;
                    m_Completed = true;
                }

                // Set animator charge
                if (m_ChargeHash != -1)
                    m_Animator.SetFloat(m_ChargeHash, m_ChargeValue);

                // Adjust pitch
                if (m_AudioSource != null)
                    m_AudioSource.pitch = Mathf.Lerp(m_StartPitch, m_EndPitch, m_ChargeValue);

                return m_Completed;
            }
        }
    }
}
