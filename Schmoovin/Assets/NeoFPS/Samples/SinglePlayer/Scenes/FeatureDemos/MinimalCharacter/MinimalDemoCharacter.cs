using UnityEngine;
using NeoCC;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/samplesref-mb-minimaldemocharacter.html")]
    [RequireComponent(typeof(NeoCharacterController))]
    [RequireComponent(typeof (MotionController))]
    [RequireComponent(typeof (MouseAndGamepadAimController))]
	public class MinimalDemoCharacter : MonoBehaviour
    {
		[Header ("Mouse Aim")]

        [SerializeField, Tooltip("The mouse aim sensitivity.")]
        private float m_MouseAimMultiplier = 5f;
        
        [SerializeField, Tooltip("Is the mouse vertical aim rotation flipped.")]
        private bool m_MouseInvertY = false;

        [Header("Motion Graph Properties")]

		[SerializeField, Tooltip("The key to the jump trigger property in the character motion graph.")]
        private string m_JumpKey = "jump";

		[SerializeField, Tooltip("The key to the crouch switch property in the character motion graph.")]
        private string m_CrouchKey = "crouch";

		[SerializeField, Tooltip("The key to the sprint switch property in the character motion graph.")]
        private string m_SprintKey = "sprint";

        private MotionController m_MotionController = null;
        private MouseAndGamepadAimController m_Aimer = null;

        private bool m_Initialised = false;

        private TriggerParameter m_JumpTrigger = null;
        private SwitchParameter m_CrouchProperty = null;
        private SwitchParameter m_SprintProperty = null;

        void Awake ()
		{
			m_Aimer = GetComponent<MouseAndGamepadAimController> ();
            m_MotionController = GetComponent<MotionController>();
        }
        
        void Update()
        {
            if (!m_Initialised)
            {
                MotionGraphContainer motionGraph = m_MotionController.motionGraph;
                m_JumpTrigger = motionGraph.GetTriggerProperty(m_JumpKey);
                m_CrouchProperty = motionGraph.GetSwitchProperty(m_CrouchKey);
                m_SprintProperty = motionGraph.GetSwitchProperty(m_SprintKey);

                m_Initialised = true;
            }

            // Aim input
            float aimYMultiplier = m_MouseInvertY ? -1f : 1f;
            m_Aimer.HandleMouseInput(new Vector2(
                Input.GetAxis("Mouse X") * m_MouseAimMultiplier,
                Input.GetAxis("Mouse Y") * m_MouseAimMultiplier * aimYMultiplier
            ));

            // Movement input
            Vector2 move = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
                move.y += 1f;
            if (Input.GetKey(KeyCode.S))
                move.y -= 1f;
            if (Input.GetKey(KeyCode.A))
                move.x -= 1f;
            if (Input.GetKey(KeyCode.D))
                move.x += 1f;
            
            float mag = Mathf.Clamp01(move.magnitude);
            if (mag > Mathf.Epsilon)
				move.Normalize();

            m_MotionController.inputMoveDirection = move;
            m_MotionController.inputMoveScale = mag;

            // Movement modifiers 
            if (m_SprintProperty != null)
                m_SprintProperty.SetInput(false, Input.GetKey(KeyCode.LeftShift));
            if (m_CrouchProperty != null)
                m_CrouchProperty.SetInput(false, Input.GetKey(KeyCode.LeftControl));

            // Jump
            if (m_JumpTrigger != null && Input.GetKeyDown (KeyCode.Space))
                m_JumpTrigger.Trigger();
        }
	}
}