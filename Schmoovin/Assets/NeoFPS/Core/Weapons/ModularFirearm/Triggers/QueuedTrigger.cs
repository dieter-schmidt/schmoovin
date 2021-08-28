using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Events;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-queuedtrigger.html")]
    public class QueuedTrigger : BaseTriggerBehaviour
    {
        [Header("Trigger Settings")]

        [SerializeField, Tooltip("The time between adding new shots to the queue.")]
        private int m_QueueSpacing = 25;

        [SerializeField, Tooltip("Should the enqueued shot be instant, or delayed by the queue spacing.")]
        private bool m_DelayFirst = false;

        [SerializeField, Tooltip("The maximum number of shots that can be enqueued.")]
        private int m_MaxQueueSize = 6;

        [SerializeField, Tooltip("Cooldown between shots fired (number of fixed update frames).")]
        private int m_BurstSpacing = 5;

        [SerializeField, Tooltip("The minimum amount of time (fixed update frames) after firing before you can queue more shots again.")]
        private int m_RepeatDelay = 25;

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Bool, true, true), Tooltip("The bool animator property key to set while the trigger is pressed.")]
        private string m_TriggerHoldAnimKey = string.Empty;

        [SerializeField, AnimatorParameterKey(AnimatorControllerParameterType.Int, true, true), Tooltip("")]
        private string m_QueueCountAnimKey = string.Empty;

        public event UnityAction<int> onQueueCountChanged;

        private bool m_Triggered = false;
        private int m_TriggerHoldHash = -1;
        private int m_QueueCountHash = -1;
        private bool m_Shooting = false;
        private int m_Cooldown = 0;
        private int m_QueueTicker = 0;

        private int m_CurrentQueue = 0;
        public int currentQueueCount
        {
            get { return m_CurrentQueue; }
            private set
            {
                if (m_CurrentQueue != value)
                {
                    m_CurrentQueue = value;

                    if (onQueueCountChanged != null)
                        onQueueCountChanged(m_CurrentQueue);

                    if (firearm.animator != null && m_QueueCountHash != -1)
                        firearm.animator.SetInteger(m_QueueCountHash, m_CurrentQueue);
                }
            }
        }


#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_MaxQueueSize < 2)
                m_MaxQueueSize = 2;
            if (m_QueueSpacing < 1)
                m_QueueSpacing = 1;
            if (m_BurstSpacing < 1)
                m_BurstSpacing = 1;
            if (m_RepeatDelay < 1)
                m_RepeatDelay = 1;
        }
#endif
        protected override void Awake()
        {
            if (m_TriggerHoldAnimKey != string.Empty)
                m_TriggerHoldHash = Animator.StringToHash(m_TriggerHoldAnimKey);
            if (m_QueueCountAnimKey != string.Empty)
                m_QueueCountHash = Animator.StringToHash(m_QueueCountAnimKey);
            base.Awake();
        }

        public override bool pressed
        {
            get { return m_Triggered; }
        }

        public override void Press()
        {
            base.Press();

            m_Triggered = true;

            // Should this use events instead?
            if (firearm.animator != null && m_TriggerHoldHash != -1)
                firearm.animator.SetBool(m_TriggerHoldHash, true);

            // Set queue ticker
            if (m_DelayFirst)
                m_QueueTicker = m_QueueSpacing;
            else
                m_QueueTicker = 0;
        }

        public override void Release()
        {
            base.Release();

            m_Triggered = false;

            // Should this use events instead?
            if (firearm.animator != null && m_TriggerHoldHash != -1)
                firearm.animator.SetBool(m_TriggerHoldHash, false);

            if (!m_Shooting)
            {
                // Shoot if shots are queued
                if (currentQueueCount > 0)
                {
                    m_Shooting = true;
                    m_Cooldown = 0;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Triggered = false;
        }

        protected override void OnSetBlocked(bool to)
        {
            base.OnSetBlocked(to);
            if (to && !m_Shooting)
            {
                m_Triggered = false;
                m_QueueTicker = 0;
                currentQueueCount = 0;
            }
        }

        protected override void FixedTriggerUpdate()
        {
            if (m_Cooldown > 0)
                --m_Cooldown;
            else
            {
                if (m_Shooting)
                {
                    Shoot();
                    if (--currentQueueCount <= 0)
                    {
                        m_Shooting = false;
                        m_Cooldown = m_RepeatDelay;
                    }
                    else
                    {
                        m_Cooldown = m_BurstSpacing;
                    }
                }
                else
                {
                    var reloader = firearm.reloader;
                    if (m_Triggered && reloader != null && reloader.empty)
                    {
                        Shoot(); // Effectively reload
                        m_Triggered = false;
                    }

                    int maxQueue = m_MaxQueueSize;

                    if (reloader != null && reloader.currentMagazine < maxQueue)
                        maxQueue = firearm.reloader.currentMagazine;

                    if (m_Triggered && !blocked && currentQueueCount < maxQueue)
                    {
                        if (m_QueueTicker <= 0)
                        {
                            ++currentQueueCount;
                            m_QueueTicker = m_QueueSpacing;
                        }
                        else
                            --m_QueueTicker;
                    }
                }
            }
        }

        private static readonly NeoSerializationKey k_TriggeredKey = new NeoSerializationKey("triggered");
        private static readonly NeoSerializationKey k_WaitKey = new NeoSerializationKey("wait");
        private static readonly NeoSerializationKey k_RepeatKey = new NeoSerializationKey("repeat");
        private static readonly NeoSerializationKey k_ShotsKey = new NeoSerializationKey("shots");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            if (saveMode == SaveMode.Default)
            {
                writer.WriteValue(k_TriggeredKey, m_Triggered);
                //writer.WriteValue(k_WaitKey, m_Wait);
                //writer.WriteValue(k_RepeatKey, m_Repeat);
                //writer.WriteValue(k_ShotsKey, m_Shots);
            }
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            //reader.TryReadValue(k_WaitKey, out m_Wait, m_Wait);
            //reader.TryReadValue(k_RepeatKey, out m_Repeat, m_Repeat);
            //reader.TryReadValue(k_ShotsKey, out m_Shots, m_Shots);

            reader.TryReadValue(k_TriggeredKey, out m_Triggered, m_Triggered);
            if (m_Triggered)
                Release();
        }
    }
}