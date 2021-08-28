using NeoFPS.Constants;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    public abstract class BaseWieldableItem : MonoBehaviour, IWieldable
    {
        [Header ("Select / Deselect")]

        [SerializeField, Tooltip("The animator component of the weapon.")]
        private Animator m_Animator = null;

        [SerializeField, AnimatorParameterKey("m_Animator", AnimatorControllerParameterType.Trigger), Tooltip("The key for the AnimatorController trigger property that triggers the draw animation.")]
        private string m_AnimKeyDraw = "Draw";

        [SerializeField, Tooltip("The audio clip when raising the weapon.")]
        private AudioClip m_AudioSelect = null;

        [SerializeField, Tooltip("The audio clip when lowering the weapon.")]
        private AudioClip m_AudioDeselect = null;

        [SerializeField, AnimatorParameterKey("m_Animator", AnimatorControllerParameterType.Trigger), Tooltip("The trigger for the weapon lower animation (blank = no animation).")]
        private string m_AnimKeyLower = string.Empty;

        [SerializeField, Tooltip("The time it takes to raise the weapon.")]
        private float m_DrawDuration = 0.5f;

        [SerializeField, Tooltip("The time taken to lower the item on deselection.")]
        private float m_LowerDuration = 0f;

        private DeselectionWaitable m_DeselectionWaitable = null;
        private int m_AnimHashDraw = -1;
        private int m_AnimHashLower = -1;
        private Coroutine m_BlockingCoroutine = null;
        private float m_DrawTimer = 0f;

        public event UnityAction<ICharacter> onWielderChanged;

        private ICharacter m_Wielder = null;
        public ICharacter wielder
        {
            get { return m_Wielder; }
            private set
            {
                if (m_Wielder != value)
                {
                    m_Wielder = value;
                    if (onWielderChanged != null)
                        onWielderChanged(m_Wielder);
                }
            }
        }

        protected Animator animator
        {
            get { return m_Animator; }
        }

        protected virtual bool CheckIsBlocked()
        {
            return m_BlockingCoroutine != null || !m_DeselectionWaitable.isComplete;
        }

        public class DeselectionWaitable : Waitable
        {
            private float m_Duration = 0f;
            private float m_StartTime = 0f;

            public DeselectionWaitable(float duration)
            {
                m_Duration = duration;
            }

            public void ResetTimer()
            {
                m_StartTime = Time.time;
            }

            protected override bool CheckComplete()
            {
                return (Time.time - m_StartTime) > m_Duration;
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_Animator == null)
                m_Animator = GetComponentInChildren<Animator>();
            m_DrawDuration = Mathf.Clamp(m_DrawDuration, 0f, 5f);
            m_LowerDuration = Mathf.Clamp(m_LowerDuration, 0f, 5f);
        }
#endif

        protected virtual void Awake()
        {
            if (!string.IsNullOrWhiteSpace(m_AnimKeyDraw))
                m_AnimHashDraw = Animator.StringToHash(m_AnimKeyDraw);
            else
                m_DrawDuration = 0f;

            if (!string.IsNullOrWhiteSpace(m_AnimKeyLower))
            {
                m_AnimHashLower = Animator.StringToHash(m_AnimKeyLower);
                if (m_LowerDuration > 0f)
                    m_DeselectionWaitable = new DeselectionWaitable(m_LowerDuration);
            }
            else
                m_LowerDuration = 0f;

            // Set up pose handler
            m_PoseHandler = new PoseHandler(transform, Vector3.zero, Quaternion.identity);
        }

        protected virtual void OnEnable()
        {
            wielder = GetComponentInParent<ICharacter>();

            if (m_AnimHashDraw != -1 && m_Animator != null)
                m_Animator.SetTrigger(m_AnimHashDraw);

            if (m_DrawDuration > 0f)
                m_BlockingCoroutine = StartCoroutine(DrawCoroutine(m_DrawDuration));

            if (m_AudioSelect != null)
                wielder.audioHandler.PlayClip(m_AudioSelect, FpsCharacterAudioSource.Body);
        }

        protected virtual void OnDisable()
        {
            m_BlockingCoroutine = null;
            // Reset pose
            m_PoseHandler.OnDisable();
        }

        public void Select()
        {
            // Play lower animation
            if (m_AnimHashDraw != -1 && m_Animator != null)
                m_Animator.SetTrigger(m_AnimHashDraw);
        }

        public void DeselectInstant()
        { }

        public Waitable Deselect()
        {
            // Play lower animation
            if (m_AnimHashLower != 0 && m_Animator != null)
                m_Animator.SetTrigger(m_AnimHashLower);

            // Play the lower audio
            if (m_AudioDeselect != null)
                wielder.audioHandler.PlayClip(m_AudioDeselect, FpsCharacterAudioSource.Body);

            // Wait for deselection
            if (m_DeselectionWaitable != null)
                m_DeselectionWaitable.ResetTimer();

            return m_DeselectionWaitable;
        }

        IEnumerator DrawCoroutine(float timer)
        {
            m_DrawTimer = timer;
            while (m_DrawTimer > 0f)
            {
                yield return null;
                m_DrawTimer -= Time.deltaTime;
            }
            m_BlockingCoroutine = null;
        }

        #region POSE

        private PoseHandler m_PoseHandler = null;

        public void SetPose(Vector3 position, Quaternion rotation, float duration)
        {
            m_PoseHandler.SetPose(position, rotation, duration);
        }

        public void SetPose(Vector3 position, CustomPositionInterpolation posInterp, Quaternion rotation, CustomRotationInterpolation rotInterp, float duration)
        {
            m_PoseHandler.SetPose(position, posInterp, rotation, rotInterp, duration);
        }

        public void ResetPose(float duration)
        {
            m_PoseHandler.ResetPose(duration);
        }

        public void ResetPose(CustomPositionInterpolation posInterp, CustomRotationInterpolation rotInterp, float duration)
        {
            m_PoseHandler.ResetPose(posInterp, rotInterp, duration);
        }

        void Update()
        {
            m_PoseHandler.UpdatePose();
        }

        #endregion

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_DrawTimerKey = new NeoSerializationKey("drawTimer");

        public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (saveMode == SaveMode.Default)
            {
                // Write coroutine if relevant
                if (m_BlockingCoroutine != null)
                {
                    if (m_DrawTimer > 0f)
                        writer.WriteValue(k_DrawTimerKey, m_DrawTimer);
                }
            }
        }

        public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            // Read and start coroutines if relevant
            float floatResult = 0f;
            if (reader.TryReadValue(k_DrawTimerKey, out floatResult, 0f))
                m_BlockingCoroutine = StartCoroutine(DrawCoroutine(floatResult));
        }

        #endregion
    }
}