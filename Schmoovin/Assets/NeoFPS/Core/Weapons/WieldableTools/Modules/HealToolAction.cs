using UnityEngine;

namespace NeoFPS.WieldableTools
{
    public class HealToolAction : BaseWieldableToolModule, IDamageSource
    {
        [SerializeField, FlagsEnum, Tooltip("When should the tool apply the heal.")]
        private WieldableToolActionTiming m_Timing = WieldableToolActionTiming.Start;
        [SerializeField, Tooltip("How many points to heal the subject for.")]
        private int m_HealAmount = 1;
        [SerializeField, Tooltip("Who/what to heal. Wielder means the character using the tool will heal themselves. Target means the tool will be used on the health manager in front of the user.")]
        private Subject m_Subject = Subject.Wielder;
        [SerializeField, Tooltip("The heal will be applied every nth fixed update tick for continuous heals.")]
        private int m_HealInterval = 1;
        [SerializeField, Tooltip("Should the heal be applied on the first frame of the continuous action, or should it wait for the first interval to elapse.")]
        private bool m_Instant = true;
        [SerializeField, Tooltip("The physics layers that the tool should check against to get a valid heal subject.")]
        private LayerMask m_TargetLayers = PhysicsFilter.LayerFilter.CharacterControllers;
        [SerializeField, Tooltip("The maximum distance in front of the character that the tool should cast to check for valid heal subjects.")]
        private float m_MaxRange = 2f;

        private IHealthManager m_HealthManager = null;
        private int m_CountDown = 0;

        private enum Subject
        {
            Wielder,
            Target
        }

        public override bool isValid
        {
            get { return m_Timing != 0; }
        }

        public override WieldableToolActionTiming timing
        {
            get { return m_Timing; }
        }

        private DamageFilter m_OutDamageFilter = DamageFilter.AllDamageAllTeams;
        public DamageFilter outDamageFilter
        {
            get { return m_OutDamageFilter; }
            set { m_OutDamageFilter = value; }
        }

        public IController controller
        {
            get { return tool.wielder.controller; }
        }

        public Transform damageSourceTransform
        {
            get { return tool.wielder.transform; }
        }

        public string description
        {
            get { return "Heal"; }
        }

        void OnValidate()
        {
            m_HealAmount = Mathf.Clamp(m_HealAmount, 1, 1000);
            m_HealInterval = Mathf.Clamp(m_HealInterval, 1, 500);
            m_MaxRange = Mathf.Clamp(m_MaxRange, 0.5f, 100f);
        }

        public override void Initialise(IWieldableTool t)
        {
            base.Initialise(t);

            if (m_Subject == Subject.Wielder)
            {
                m_HealthManager = t.wielder.GetComponent<IHealthManager>();
                if (m_HealthManager == null)
                    enabled = false;
            }
        }

        void OnDisable()
        {
            m_CountDown = 0;
        }

        public override void FireStart()
        {
            var h = GetSubject();
            if (h != null)
                h.AddHealth(m_HealAmount, this);

            // Set interval for continuous
            if (m_Instant)
                m_CountDown = 1;
            else
                m_CountDown = m_HealInterval;
        }

        public override void FireEnd(bool success)
        {
            if (success)
            {
                var h = GetSubject();
                if (h != null)
                    h.AddHealth(m_HealAmount, this);
            }

            m_CountDown = 0;
        }

        public override bool TickContinuous()
        {
            if (--m_CountDown == 0)
            {
                var h = GetSubject();
                if (h == null)
                {
                    tool.Interrupt();
                    return false;
                }
                else
                {
                    m_CountDown = m_HealInterval;
                    h.AddHealth(m_HealAmount, this);

                    if (h.health == h.healthMax)
                    {
                        tool.Interrupt();
                        return false;
                    }
                }
            }
            return true;
        }

        IHealthManager GetSubject()
        {
            if (m_Subject == Subject.Wielder)
                return m_HealthManager;
            else
            {
                // Check for hit
                RaycastHit hit;
                if (PhysicsExtensions.RaycastNonAllocSingle(tool.wielder.fpCamera.GetAimRay(), out hit, m_MaxRange, m_TargetLayers, tool.wielder.transform, QueryTriggerInteraction.Ignore))
                    return hit.collider.GetComponent<IHealthManager>();
                else
                    return null;
            }
        }
    }
}