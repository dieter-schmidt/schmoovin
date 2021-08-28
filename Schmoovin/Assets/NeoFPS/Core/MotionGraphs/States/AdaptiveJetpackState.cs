#if !NEOFPS_FORCE_QUALITY && (UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || (UNITY_WSA && NETFX_CORE) || NEOFPS_FORCE_LIGHTWEIGHT)
#define NEOFPS_LIGHTWEIGHT
#endif

using UnityEngine;
using NeoFPS.CharacterMotion.MotionData;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion.States
{
    [MotionGraphElement("Airborne/Jetpack (Adaptive)", "Adaptive Jetpack")]
    [HelpURL("https://docs.neofps.com/manual/motiongraphref-mgs-adaptivejetpackstate.html")]
    public class AdaptiveJetpackState : FallingState
    {
        [SerializeField, Tooltip("An acceleration force (ignores mass) upwards for the jetpack.")]
        private FloatDataReference m_JetpackForce = new FloatDataReference(15f);
        [SerializeField, Tooltip("An optional parameter for the jetpack fuel. Will be consumed based on output.")]
        private FloatParameter m_JetpackFuel = null;

        [SerializeField, Tooltip("How the jetpack should work. Smooth reduces power as approaching the max vertical speed. Burst sets a minimum jetpack burst duration and gap between bursts to create a bouncier hover.")]
        private JetpackMode m_Mode = JetpackMode.Smooth;

        [SerializeField, Tooltip("The maximum vertical speed, at which the jetpack stops pushing upwards. A speed of zero is hovering.")]
        private float m_MaxVerticalSpeed = 0f;
        [SerializeField, Tooltip("A speed differential where the jetpack will switch on/off. Max + half this = off. Max - half this = on.")]
        private float m_Hysteresis = 0.25f;
        [SerializeField, Tooltip("The speed below the max at which jetpack power (and fuel consumption) starts to fall off. The power will fade out exponentially the closer you get to the max speed.")]
        private float m_SpeedFalloff = 10f;

        [SerializeField, Tooltip("The amount of fuel burned per second at full burn.")]
        private float m_FuelBurnRate = 0.5f;
        [SerializeField, Range(0f, 1f), Tooltip("A damping amount for the fuel consumption to smooth it out. Set to zero for direct feedback.")]
        private float m_FuelDamping = 0.25f;
        [SerializeField, Tooltip("The fuel burn rate when at the target speed, as opposed to accelerating towards it.")]
        private float m_MinFuelBurn = 0.25f;

        private Vector3 m_JetpackVelocity = Vector3.zero;
        private bool m_Firing = false;
        private float m_FuelConsumption = 0f;

        enum JetpackMode
        {
            Smooth,
            Burst
            // Could add more modes later
        }

        public override bool completed
        {
            get { return false; }
        }
        public override Vector3 moveVector
        {
            get { return base.moveVector + (m_JetpackVelocity * Time.deltaTime); }
        }

        public override void OnValidate()
        {
            base.OnValidate();
            m_MaxVerticalSpeed = Mathf.Clamp(m_MaxVerticalSpeed, -50f, 100f);
            m_Hysteresis = Mathf.Clamp(m_Hysteresis, 0.01f, 5f);
            m_MinFuelBurn = Mathf.Clamp(m_MinFuelBurn, 0f, m_FuelBurnRate);
        }

        public override void OnExit()
        {
            base.OnExit();
            m_JetpackVelocity = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();

            float force = m_JetpackForce.value;
            //if (m_JetpackPower != null)
            //    force *= m_JetpackPower.value;


            switch (m_Mode)
            {
                case JetpackMode.Smooth:
                    {
                        // Get upSpeed
                        float upSpeed = Vector3.Dot(fallVelocity, characterController.up);

                        // Get the falloff based on speed
                        float falloff = Mathf.Clamp01((m_MaxVerticalSpeed - upSpeed) / m_SpeedFalloff);
                        falloff = EasingFunctions.EaseOutQuadratic(falloff);

                        // Calculate velocity
                        m_JetpackVelocity = characterController.up * force * falloff * Time.deltaTime;
                        if (falloff > 0f)
                            m_JetpackVelocity += characterController.gravity * -Time.deltaTime;

                        // Burn fuel
                        if (m_JetpackFuel != null)
                        {
                            float fuelBurn = Mathf.Lerp(m_MinFuelBurn, m_FuelBurnRate, falloff);

                            float fuel = m_JetpackFuel.value;
                            fuel -= fuelBurn * Time.deltaTime;
                            if (fuel < 0f)
                                fuel = 0f;
                            m_JetpackFuel.value = fuel;
                        }
                    }
                    break;
                case JetpackMode.Burst:
                    {
                        // Get upSpeed
                        //Vector3 fallWithGravity = fallVelocity + characterController.gravity * Time.deltaTime;
                        float upSpeed = Vector3.Dot(fallVelocity, characterController.up);

                        // Check hysteresis
                        float halfHysteresis = m_Hysteresis * 0.5f;
                        if (upSpeed > m_MaxVerticalSpeed + halfHysteresis)
                            m_Firing = false;
                        if (upSpeed < m_MaxVerticalSpeed - halfHysteresis)
                            m_Firing = true;

                        float targetFuelConsumption = 0f;

                        // Calculate the movement
                        if (m_Firing)
                        {
                            m_JetpackVelocity = (characterController.up * force * Time.deltaTime);
                            targetFuelConsumption = 1f;
                        }
                        else
                            m_JetpackVelocity = Vector3.zero;


                        // Burn fuel
                        if (m_JetpackFuel != null)
                        {
                            float fuelDamp = 1f - m_FuelDamping;
                            fuelDamp = fuelDamp * fuelDamp * fuelDamp;
                            fuelDamp = 0.1f + fuelDamp * 0.9f;
                            m_FuelConsumption = Mathf.Lerp(m_FuelConsumption, targetFuelConsumption, fuelDamp);

                            float fuel = m_JetpackFuel.value;
                            fuel -= m_FuelConsumption * m_FuelBurnRate * Time.deltaTime;
                            if (fuel < 0f)
                                fuel = 0f;
                            m_JetpackFuel.value = fuel;
                        }
                    }
                    break;
            }

            //|| Vector3.Dot(fallVelocity, characterController.up) < m_MaxVerticalSpeed)
            // Calculate jetpack velocity
            //m_JetpackVelocity = (characterController.up * force * Time.deltaTime);
        }

        public override void CheckReferences(IMotionGraphMap map)
        {
            m_JetpackForce.CheckReference(map);
            m_JetpackFuel = map.Swap(m_JetpackFuel);
            base.CheckReferences(map);
        }

        #region SAVE / LOAD

        private static readonly NeoSerializationKey k_JetpackVelocityKey = new NeoSerializationKey("jetpackV");

        public override void WriteProperties(INeoSerializer writer)
        {
            base.WriteProperties(writer);

            writer.WriteValue(k_JetpackVelocityKey, m_JetpackVelocity);
        }

        public override void ReadProperties(INeoDeserializer reader)
        {
            base.ReadProperties(reader);

            reader.TryReadValue(k_JetpackVelocityKey, out m_JetpackVelocity, m_JetpackVelocity);
        }

        #endregion
    }
}