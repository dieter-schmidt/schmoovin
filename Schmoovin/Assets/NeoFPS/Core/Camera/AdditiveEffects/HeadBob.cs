using UnityEngine;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-headbob.html")]
	public class HeadBob : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
		[SerializeField, Tooltip("The maximum position offset along the x-axis in either direction.")]
        private float m_HorizontalBobRange = 0.01f;

        [SerializeField, Tooltip("The maximum position offset along the y-axis in either direction.")]
        private float m_VerticalBobRange = 0.005f;

        [SerializeField, Tooltip("The curve over one step cycle for the weapon bob.")]
        private AnimationCurve m_BobCurve = new AnimationCurve(
			new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
			new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
			new Keyframe(2f, 0f)); // sin curve for head bob

        [SerializeField, Tooltip("The name of a float parameter on the character motion graph that sets the bob interval distance.")]
        private string m_BobIntervalParamKey = "bobInterval";

        [SerializeField, Range(0.5f, 10f), Tooltip("The distance travelled for one full bob cycle.")]
        private float m_BobInterval = 3;

        [SerializeField, Range(0f, 5f), Tooltip("At or below this speed the bob will be scaled to zero.")]
        private float m_MinLerpSpeed = 0.5f;

        [SerializeField, Range(0.25f, 10f), Tooltip("At or above this speed the bob will have its full effect.")]
        private float m_MaxLerpSpeed = 2f;

        private static readonly NeoSerializationKey k_CycleXKey = new NeoSerializationKey("cycleX");
        private static readonly NeoSerializationKey k_CycleYKey = new NeoSerializationKey("cycleY");
        private static readonly NeoSerializationKey k_TimeKey = new NeoSerializationKey("time");
        private static readonly NeoSerializationKey k_FadeKey = new NeoSerializationKey("fade");

        private const float k_FadeDuration = 1f;
		private const float k_FadeLerp = 0.05f;

        private FloatParameter m_BobIntervalParameter = null;
        private float m_CyclePositionX = 0f;
		private float m_CyclePositionY = 0f;
		private float m_Time = 0f;
		private float m_FadeTime = 0f;
        private float m_PreviousSpeed = 0f;

		private IAdditiveTransformHandler m_Handler = null;
		private MotionController m_Controller = null;

		public Quaternion rotation
		{
			get { return Quaternion.identity; }
		}
        
		public Vector3 position
		{
            get;
            private set;
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
        void OnValidate()
        {
            m_HorizontalBobRange = Mathf.Clamp(m_HorizontalBobRange, 0f, 0.5f);
            m_VerticalBobRange = Mathf.Clamp(m_VerticalBobRange, 0f, 0.5f);
        }
#endif

        void Start ()
		{
			// get the length of the curve in time
			m_Time = m_BobCurve[m_BobCurve.length - 1].time;

            // Get the motion graph parameter if the key is set
            if (m_Controller != null)
            {
                if (!string.IsNullOrEmpty(m_BobIntervalParamKey))
                    m_BobIntervalParameter = m_Controller.motionGraph.GetFloatProperty(m_BobIntervalParamKey);
            }
        }

		void Awake ()
        {
            m_Controller = GetComponentInParent <MotionController> ();
			m_Handler = GetComponent<IAdditiveTransformHandler>();
		}

		void OnEnable ()
		{
			m_Handler.ApplyAdditiveEffect (this);
		}

		void OnDisable ()
		{
			m_Handler.RemoveAdditiveEffect (this);
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
                    float hRange = Mathf.Lerp(0f, m_HorizontalBobRange, lerp);
                    float vRange = Mathf.Lerp(0f, m_VerticalBobRange, lerp);

                    position = new Vector3 (xCurve * hRange, yCurve * vRange, 0f);

					m_FadeTime = 0f;
				}
			}
			else
				FadeOut ();
		}
		
		void FadeOut ()
		{
			m_FadeTime += Time.deltaTime;
			if (m_FadeTime > k_FadeDuration)
                position = Vector3.zero;
			else
                position = Vector3.Lerp (position, Vector3.zero, k_FadeLerp);
				
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
