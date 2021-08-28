using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-weaponbob.html")]
	public class WeaponBob : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
	{
		[SerializeField, Tooltip("The maximum position offset along the x-axis in either direction.")]
        private float m_HorizontalBobRange = 0.005f;

        [SerializeField, Tooltip("The maximum position offset along the y-axis in either direction.")]
        private float m_VerticalBobRange = 0.005f;

        [SerializeField, Tooltip("The maximum rotation offset around the x-axis in either direction.")]
        private float m_HorizontalAngleRange = 0.5f;

        [SerializeField, Tooltip("The maximum rotation offset around the y-axis in either direction.")]
        private float m_VerticalAngleRange = 0.5f;

        [SerializeField, Tooltip("The curve over one step cycle for the weapon bob.")]
        private AnimationCurve m_BobCurve = new AnimationCurve(
			new Keyframe(0f, -1f), new Keyframe(0.5f, 0f),
			new Keyframe(1f, 1f), new Keyframe(1.5f, 0f),
			new Keyframe(2f, -1f)); // sin curve for head bob

        [SerializeField, Tooltip("The name of a float parameter on the character motion graph that sets the bob interval distance.")]
        private string m_BobIntervalParamKey = "bobInterval";

        [SerializeField, Range(0.5f, 10f), Tooltip("The distance travelled for one full bob cycle.")]
        private float m_BobInterval = 3f;

        [SerializeField, Range(0f, 5f), Tooltip("At or below this speed the bob will be scaled to zero.")]
        private float m_MinLerpSpeed = 0.5f;

        [SerializeField, Range(0.25f, 10f), Tooltip("At or above this speed the bob will have its full effect.")]
        private float m_MaxLerpSpeed = 2f;

        private static readonly NeoSerializationKey k_CycleXKey = new NeoSerializationKey("cycleX");
        private static readonly NeoSerializationKey k_CycleYKey = new NeoSerializationKey("cycleY");
        private static readonly NeoSerializationKey k_TimeKey = new NeoSerializationKey("time");
        private static readonly NeoSerializationKey k_FadeKey = new NeoSerializationKey("fade");

        private FloatParameter m_BobIntervalParameter = null;
        private float m_CyclePositionX = 0f;
		private float m_CyclePositionY = 0f;
		private float m_Time = 0f;
		private float m_FadeTime = 0f;
        private float m_PreviousSpeed = 0f;

        private IAdditiveTransformHandler m_Handler;
		private MotionController m_Controller;

		private Quaternion m_Rotation = Quaternion.identity;
		public Quaternion rotation
		{
			get { return m_Rotation; }
		}

		private Vector3 m_Position = Vector3.zero;
		public Vector3 position
		{
			get { return m_Position; }
        }

        public bool bypassPositionMultiplier
        {
            get { return false; }
        }

        public bool bypassRotationMultiplier
        {
            get { return false; }
        }

        public float bobInterval
        {
            get
            {
                if (m_BobIntervalParameter == null)
                {
                    if (m_Controller.characterController.isGrounded)
                        return m_BobInterval;
                    else
                        return 0f;
                }
                else
                    return m_BobIntervalParameter.value;
            }
        }

#if UNITY_EDITOR
        void OnValidate ()
        {
            m_HorizontalBobRange = Mathf.Clamp(m_HorizontalBobRange, 0f, 0.5f);
            m_VerticalBobRange = Mathf.Clamp(m_VerticalBobRange, 0f, 0.5f);
            m_HorizontalAngleRange = Mathf.Clamp(m_HorizontalAngleRange, 0f, 30f);
            m_VerticalAngleRange = Mathf.Clamp(m_VerticalAngleRange, 0f, 30f);
        }
#endif

        void Start ()
		{
			// get the length of the curve in time
			m_Time = m_BobCurve[m_BobCurve.length - 1].time;
        }

		void Awake ()
		{
			m_Handler = GetComponent<IAdditiveTransformHandler>();
		}

		void OnEnable ()
		{
			m_Controller = GetComponentInParent <MotionController> ();
            if (m_Controller != null)
            {
                if (!string.IsNullOrEmpty(m_BobIntervalParamKey))
                    m_BobIntervalParameter = m_Controller.motionGraph.GetFloatProperty(m_BobIntervalParamKey);
                m_Handler.ApplyAdditiveEffect(this);
            }
		}

		void OnDisable ()
		{
			if (m_Controller != null)
			{
				m_Controller = null;
				m_Handler.RemoveAdditiveEffect (this);
			}
            m_BobIntervalParameter = null;
        }

		public void UpdateTransform ()
		{
            float interval = bobInterval;
			if (interval != 0f)
			{
				float xCurve = m_BobCurve.Evaluate (m_CyclePositionX);
				float yCurve = m_BobCurve.Evaluate (m_CyclePositionY);

                // Smooth the speed value to prevent the bob going crazy when rapidly changing directions
                float speed = Mathf.Lerp(m_PreviousSpeed, m_Controller.characterController.velocity.magnitude, Time.deltaTime * 5f);
                m_PreviousSpeed = speed;

                if (speed < m_MinLerpSpeed)
					FadeOut ();
				else
                {
                    float timeMultiplier = speed / interval;
                    if (timeMultiplier > 5f)
                        timeMultiplier = 5f;

                    m_CyclePositionX += Time.deltaTime * timeMultiplier;
                    m_CyclePositionY += Time.deltaTime * timeMultiplier * 2f;

					if (m_CyclePositionX > m_Time)
						m_CyclePositionX = m_CyclePositionX - m_Time;
					if (m_CyclePositionY > m_Time)
						m_CyclePositionY = m_CyclePositionY - m_Time;

                    float lerp = (speed - m_MinLerpSpeed) / (m_MaxLerpSpeed - m_MinLerpSpeed);

                    // Calculate position bob
                    float hRange = Mathf.Lerp(0f, m_HorizontalBobRange, lerp);
                    float vRange = Mathf.Lerp(0f, m_VerticalBobRange, lerp);
                    m_Position = new Vector3 (xCurve * hRange, yCurve * vRange, 0f);

                    // Calculate rotation bob
                    if (m_HorizontalAngleRange > 0f || m_VerticalAngleRange > 0f)
                    {
                        hRange = Mathf.Lerp(0f, m_HorizontalAngleRange, lerp);
                        vRange = Mathf.Lerp(0f, m_VerticalAngleRange, lerp);
                        m_Rotation = Quaternion.Euler(yCurve * vRange, xCurve * hRange, 0f);
                    }
                    else
                        m_Rotation = Quaternion.identity;

					m_FadeTime = 0f;
				}
			}
			else
				FadeOut ();
		}

		void FadeOut ()
		{
			m_FadeTime += Time.deltaTime;
			if (m_FadeTime > 0.25f)
			{
				m_Rotation = Quaternion.identity;
				m_Position = Vector3.zero;
			}
			else
			{
				m_Rotation = Quaternion.Lerp (m_Rotation, Quaternion.identity, 0.25f);
				m_Position = Vector3.Lerp (m_Position, Vector3.zero, 0.25f);
			}
			m_CyclePositionX = 0f;
			m_CyclePositionY = 0f;
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_CycleXKey, m_CyclePositionX);
            writer.WriteValue(k_CycleYKey, m_CyclePositionY);
            writer.WriteValue(k_TimeKey, m_Time);
            writer.WriteValue(k_FadeKey, m_FadeTime);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_CycleXKey, out m_CyclePositionX, m_CyclePositionX);
            reader.TryReadValue(k_CycleYKey, out m_CyclePositionY, m_CyclePositionY);
            reader.TryReadValue(k_TimeKey, out m_Time, m_Time);
            reader.TryReadValue(k_FadeKey, out m_FadeTime, m_FadeTime);
        }
    }
}
