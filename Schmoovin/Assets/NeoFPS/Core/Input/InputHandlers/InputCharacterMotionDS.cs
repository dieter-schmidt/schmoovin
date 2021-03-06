using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using MoreMountains.Feedbacks;

namespace NeoFPS
{
	[HelpURL("https://docs.neofps.com/manual/inputref-mb-inputcharactermotion.html")]
	[RequireComponent (typeof (ICharacter))]
	public class InputCharacterMotionDS : CharacterInputBase
    {
		[Header ("Features")]

		[SerializeField, Tooltip("Should double tapping a move direction set the dodge direction and trigger properties.")]
        private bool m_EnableDodging = true;

        [SerializeField, Range(0.1f, 1f), Tooltip("How close together (seconds) do the direction buttons have to be tapped to dodge.")]
        private float m_DodgeTimeout = 0.25f;

        [SerializeField, Tooltip("Does holding the jump button charge up a jump or does the character dodge as soon as the button is pressed.")]
        private bool m_EnableChargedJump = false;

        [SerializeField, Range(0.1f, 51f), Tooltip("The time it takes to charge up a full power jump if charged jumps are enabled.")]
        private float m_JumpChargeTime = 0.25f;

        [SerializeField, Range(0.1f, 51f), Tooltip("The time it takes to charge up a full power burst if charged bursts are enabled.")]
        private float m_BurstChargeTime = 1f;

        [SerializeField, Tooltip("Toggle leaning or hold to lean.")]
        private bool m_ToggleLean = true;

        [SerializeField, Tooltip("Cancel sprinting if no direction is pressed (or analogue direction is very low).")]
        private bool m_NoInputCancelsSprint = false;

        [Header("Motion Graph Properties")]

		[SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Trigger), Tooltip("The key to the jump trigger property in the character motion graph.")]
        private string m_JumpKey = "jump";

		[SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Float), Tooltip("The key to the jump charge float property in the character motion graph.")]
        private string m_JumpChargeKey = "jumpCharge";

        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Switch), Tooltip("The key to the jump hold float property in the character motion graph.")]
        private string m_JumpHoldKey = "jumpHold";

        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Switch), Tooltip("The key to the crouch switch property in the character motion graph.")]
        private string m_CrouchKey = "crouch";

        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Switch), Tooltip("The key to the crouch hold switch property in the character motion graph.")]
        private string m_CrouchHoldKey = "crouchHold";

        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Switch), Tooltip("The key to the sprint switch property in the character motion graph.")]
        private string m_SprintKey = "sprint";

        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Switch), Tooltip("The key to the sprint hold switch property in the character motion graph.")]
        private string m_SprintHoldKey = "sprintHold";

        //DS
        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Trigger), Tooltip("The key to the sprint trigger property in the character motion graph.")]
        private string m_SprintTriggerKey = "sprintTrigger";
        //DS

        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Trigger), Tooltip("The key to the dodge left trigger property in the character motion graph.")]
        private string m_DodgeLeftKey = "dodgeLeft";

        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Trigger), Tooltip("The key to the dodge right trigger property in the character motion graph.")]
        private string m_DodgeRightKey = "dodgeRight";

        //DS
        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Trigger), Tooltip("The key to the sidestep right trigger property in the character motion graph.")]
        private string m_SidestepRightKey = "sidestepRight";

        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Trigger), Tooltip("The key to the sidestep left trigger property in the character motion graph.")]
        private string m_SidestepLeftKey = "sidestepLeft";
        //DS

        //[SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Trigger), Tooltip("The key to the ability trigger property in the character motion graph.")]
        //private string m_AbilityKey = "ability";

        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Switch), Tooltip("The key to the ability switch property in the character motion graph.")]
        private string m_AbilityKey = "ability";

        //Ground Pound (new)
        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Trigger), Tooltip("The key to the ground pound trigger property in the character motion graph.")]
        private string m_GroundPoundKey = "groundPound";

        //Burst Charge(new)
        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Switch), Tooltip("The key to the burst charge trigger property in the character motion graph.")]
        private string m_BurstChargeKey = "burstCharge";

        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Float), Tooltip("The key to the burst charge float property in the character motion graph.")]
        private string m_BurstChargeFloatKey = "burstChargeFloat";

        //Burst Release(new)
        [SerializeField, MotionGraphParameterKey(MotionGraphParameterType.Trigger), Tooltip("The key to the burst charge trigger property in the character motion graph.")]
        private string m_BurstReleaseKey = "burstRelease";

        private CharacterInteractionHandler m_InteractionMgr = null;
		private MouseAndGamepadAimController m_Aimer = null;
        private BodyLean m_BodyLean = null;

        private bool m_Initialised = false;

        private TriggerParameter m_JumpTrigger = null;
        private FloatParameter m_JumpChargeProperty = null;
        private SwitchParameter m_JumpHoldProperty = null;
        private SwitchParameter m_CrouchProperty = null;
        private SwitchParameter m_CrouchHoldProperty = null;
        private SwitchParameter m_SprintProperty = null;
        private SwitchParameter m_SprintHoldProperty = null;
        //DS
        private TriggerParameter m_SprintTriggerProperty = null;
        private TriggerParameter m_SidestepLeftTrigger = null;
        private TriggerParameter m_SidestepRightTrigger = null;
        //DS
        private TriggerParameter m_DodgeLeftTrigger = null;
        private TriggerParameter m_DodgeRightTrigger = null;
        //private TriggerParameter m_AbilityTrigger = null;
        private TriggerParameter m_GroundPoundTrigger = null;
        private SwitchParameter m_AbilityProperty = null;
        private SwitchParameter m_BurstChargeProperty = null;
        private FloatParameter m_BurstChargeFloatProperty = null;
        private TriggerParameter m_BurstReleaseTrigger = null;

        private float m_DodgeLeftTimer = 0f;
        private float m_DodgeRightTimer = 0f;

        private string m_BurstChargeLengthKey = "burstChargeLength";
        private float m_BurstChargeLength = 0f;
        private float m_BurstChargeTimer = 0f;
        private int m_BurstLevel = 0;

        protected override void OnAwake()
        {
            base.OnAwake();

            m_InteractionMgr = GetComponent<CharacterInteractionHandler> ();
			m_Aimer = GetComponent<MouseAndGamepadAimController> ();

            if (m_Character.bodyTransformHandler != null)
                m_BodyLean = m_Character.bodyTransformHandler.GetComponent<BodyLean>();
        }

        void CheckMotionGraphConnection()
        {
            if (!m_Initialised)
            {
                MotionGraphContainer motionGraph = m_Character.motionController.motionGraph;
                m_JumpTrigger = motionGraph.GetTriggerProperty(m_JumpKey);
                m_JumpChargeProperty = motionGraph.GetFloatProperty(m_JumpChargeKey);
                m_JumpHoldProperty = motionGraph.GetSwitchProperty(m_JumpHoldKey);
                m_CrouchProperty = motionGraph.GetSwitchProperty(m_CrouchKey);
                m_CrouchHoldProperty = motionGraph.GetSwitchProperty(m_CrouchHoldKey);
                m_SprintProperty = motionGraph.GetSwitchProperty(m_SprintKey);
                //DS
                m_SprintTriggerProperty = motionGraph.GetTriggerProperty(m_SprintTriggerKey);
                m_SidestepLeftTrigger = motionGraph.GetTriggerProperty(m_SidestepLeftKey);
                m_SidestepRightTrigger = motionGraph.GetTriggerProperty(m_SidestepRightKey);
                //DS
                m_SprintHoldProperty = motionGraph.GetSwitchProperty(m_SprintHoldKey);
                m_DodgeLeftTrigger = motionGraph.GetTriggerProperty(m_DodgeLeftKey);
                m_DodgeRightTrigger = motionGraph.GetTriggerProperty(m_DodgeRightKey);
                //m_AbilityTrigger = motionGraph.GetTriggerProperty(m_AbilityKey);
                m_GroundPoundTrigger = motionGraph.GetTriggerProperty(m_GroundPoundKey);
                m_AbilityProperty = motionGraph.GetSwitchProperty(m_AbilityKey);
                m_BurstChargeProperty = motionGraph.GetSwitchProperty(m_BurstChargeKey);
                m_BurstChargeFloatProperty = motionGraph.GetFloatProperty(m_BurstChargeFloatKey);
                m_BurstReleaseTrigger = motionGraph.GetTriggerProperty(m_BurstReleaseKey);

                //non properties
                m_BurstChargeLength = motionGraph.GetFloat(Animator.StringToHash(m_BurstChargeLengthKey));
                m_BurstChargeTimer = m_BurstChargeLength;

                m_Initialised = true;
            }
        }

        protected override void OnGainFocus()
        {
            base.OnGainFocus();

            // Capture mouse cursor
            NeoFpsInputManager.captureMouseCursor = true;
        }

        protected override void OnLoseFocus()
        {
            m_Character.motionController.inputMoveDirection = Vector2.zero;
            m_Character.motionController.inputMoveScale = 0f;
            if (m_JumpHoldProperty != null)
                m_JumpHoldProperty.Hold(false);

            // Capture mouse cursor
            NeoFpsInputManager.captureMouseCursor = false;
        }

        protected override void UpdateInput()
        {

            CheckMotionGraphConnection();

            // Aim input
            m_Aimer.HandleMouseInput(new Vector2(
                GetAxis(FpsInputAxis.MouseX),
                GetAxis(FpsInputAxis.MouseY)
            ));
            m_Aimer.HandleAnalogInput(new Vector2(
                GetAxis(FpsInputAxis.LookX),
                GetAxis(FpsInputAxis.LookY)
            ));

            // Movement input
            Vector2 move = new Vector2(
                GetAxis(FpsInputAxis.MoveX),
                GetAxis(FpsInputAxis.MoveY)
            );
            if (GetButton(FpsInputButton.Forward))
                move.y += 1f;
            if (GetButton(FpsInputButton.Backward))
                move.y -= 1f;
            if (GetButton(FpsInputButton.Left))
                move.x -= 1f;
            if (GetButton(FpsInputButton.Right))
                move.x += 1f;

            float mag = Mathf.Clamp01(move.magnitude);
            if (mag > Mathf.Epsilon)
                move.Normalize();

            m_Character.motionController.inputMoveDirection = move;
            m_Character.motionController.inputMoveScale = mag;

            // Movement modifiers
            bool sprintPress = GetButtonDown(FpsInputButton.SprintToggle);
            //DS
            bool sprintTrigger = GetButtonDown(FpsInputButton.SprintToggle);
            //DS
            bool sprintHold = GetButton(FpsInputButton.Sprint);
            bool crouchPress = GetButtonDown(FpsInputButton.CrouchToggle);
            bool crouchHold = GetButton(FpsInputButton.Crouch);

            if (m_SprintProperty != null)
            {
                // Cancel sprinting on no input
                if (m_NoInputCancelsSprint && mag < 0.1f)
                {
                    sprintPress = false;
                    m_SprintProperty.on = false;
                }

                m_SprintProperty.SetInput(
                    sprintPress,
                    sprintHold
                );

                //DS
                if (sprintTrigger)
                {
                    m_SprintTriggerProperty.Trigger();
                }

                    
                //DS

                // Cancel sprinting if crouching
                if (crouchPress || crouchHold)
                    m_SprintProperty.on = false;
            }
            if (m_SprintHoldProperty != null)
                m_SprintHoldProperty.on = sprintHold || GetButton(FpsInputButton.SprintToggle);

            if (m_CrouchProperty != null)
            {
                m_CrouchProperty.SetInput(
                    crouchPress,
                    crouchHold
                );

                // Cancel crouching if sprinting
                if (sprintPress || sprintHold)
                    m_CrouchProperty.on = false;
            }
            if (m_CrouchHoldProperty != null)
                m_CrouchHoldProperty.Hold(crouchHold || GetButton(FpsInputButton.CrouchToggle));

            // Jump
            if (m_JumpTrigger != null)
            {
                if (m_EnableChargedJump && m_JumpChargeProperty != null)
                {
                    if (GetButtonDown(FpsInputButton.Jump))
                        m_JumpChargeProperty.value = 0f;

                    if (GetButton(FpsInputButton.Jump))
                        m_JumpChargeProperty.value = Mathf.Clamp01(m_JumpChargeProperty.value + (Time.deltaTime / m_JumpChargeTime));

                    if (GetButtonUp(FpsInputButton.Jump)) {
                        m_JumpTrigger.Trigger();
                        //DS START
                        //GetComponent<MMFeedbackHandler>().triggerMMFeedbacks();
                        //motionGraph.GetSwitch(Animator.StringToHash("groundPoundStarted"))
                        //DS END
                    }
                }
                else
                {
                    if (GetButtonDown(FpsInputButton.Jump))
                    {
                        m_JumpTrigger.Trigger();
                        //DS START
                        //GetComponent<MMFeedbackHandler>().triggerMMFeedbacks();
                        //gpJumpElapsed = 0f;
                        //DS END
                    }
                }
            }
            if (m_JumpHoldProperty != null)
                m_JumpHoldProperty.Hold(GetButton(FpsInputButton.Jump));

            // Dodge
            if (m_EnableDodging && m_DodgeLeftTrigger != null && m_DodgeRightTrigger != null)
            {
                if (GetButtonDown(FpsInputButton.Left))
                {
                    if (m_DodgeLeftTimer > Mathf.Epsilon)
                        m_DodgeLeftTrigger.Trigger();
                    else
                        m_DodgeLeftTimer = m_DodgeTimeout;
                }
                if (GetButtonDown(FpsInputButton.Right))
                {
                    if (m_DodgeRightTimer > Mathf.Epsilon)
                        m_DodgeRightTrigger.Trigger();
                    else
                        m_DodgeRightTimer = m_DodgeTimeout;
                }

                // Modify timers
                m_DodgeLeftTimer -= Time.deltaTime;
                if (m_DodgeLeftTimer < 0f)
                    m_DodgeLeftTimer = 0f;
                m_DodgeRightTimer -= Time.deltaTime;
                if (m_DodgeRightTimer < 0f)
                    m_DodgeRightTimer = 0f;
            }

            //DS
            // Sidestep
            if (m_SidestepLeftTrigger != null && m_SidestepRightTrigger != null)
            {
                if (GetButtonDown(FpsInputButton.PrevWeapon))
                {
                    m_SidestepLeftTrigger.Trigger();
                }
                else if (GetButtonDown(FpsInputButton.NextWeapon))
                {
                    m_SidestepRightTrigger.Trigger();
                }
            }
            //DS

            // Ground Pound
            if (m_GroundPoundTrigger != null)
            {
                if (GetButtonDown(FpsInputButton.Use))
                    m_GroundPoundTrigger.Trigger();
            }

            // Ability
            //if (m_AbilityTrigger != null)
            //{
            //    if (GetButtonDown(FpsInputButton.Ability))
            //        m_AbilityTrigger.Trigger();
            //}
            if (m_AbilityProperty != null)
                m_AbilityProperty.Hold(GetButton(FpsInputButton.Ability));

            // Burst Charge
            bool burstChargeHold = GetButton(FpsInputButton.LeanLeft);
            bool burstChargeReleased = GetButtonUp(FpsInputButton.LeanLeft);
            bool burstChargePressed = GetButtonDown(FpsInputButton.LeanLeft);

            //if (m_BurstChargeProperty != null) 
            //{ 
            //    if (burstChargeReleased)
            //    {
            //        if (m_BurstChargeTimer <= 0f)
            //        {
            //            m_BurstReleaseTrigger.Trigger();
            //        }
            //        m_BurstChargeTimer = m_BurstChargeLength;
            //    }
            //    else if (burstChargeHold)
            //    {
            //        //charge timer
            //        m_BurstChargeTimer -= Time.deltaTime;
            //        m_BurstChargeProperty.on = burstChargeHold;
            //    }
            //    //Debug.Log(m_BurstChargeTimer);
            //}     
            //if (m_BurstChargeProperty != null)
            //{
            //    if (burstChargeReleased)
            //    {
            //        //if (m_BurstChargeTimer <= 0f)
            //        //{
            //        //set burst jump scale based on how long button is pressed before releasing
            //        m_Character.motionController.motionGraph.SetFloat(Animator.StringToHash("burstScale"), 1 - (Mathf.Max(m_BurstChargeTimer, 0f)/m_BurstChargeLength));
            //        m_BurstReleaseTrigger.Trigger();
            //        //}
            //        m_BurstChargeTimer = m_BurstChargeLength;
            //    }
            //    else if (burstChargeHold)
            //    {
            //        //charge timer
            //        m_BurstChargeTimer -= Time.deltaTime;
            //        m_BurstChargeProperty.on = burstChargeHold;
            //    }
            //    //Debug.Log(m_BurstChargeTimer);
            //}
            if (m_BurstReleaseTrigger != null)
            {
                if (m_BurstChargeFloatProperty != null)
                {
                    if (GetButtonDown(FpsInputButton.LeanLeft))
                        m_BurstChargeFloatProperty.value = 0f;

                    if (GetButton(FpsInputButton.LeanLeft))
                    {
                        m_BurstChargeFloatProperty.value = Mathf.Clamp01(m_BurstChargeFloatProperty.value + (Time.deltaTime / m_BurstChargeTime));
                        if (m_BurstChargeFloatProperty.value >= 1f && m_BurstLevel == 1)
                        {
                            m_BurstLevel = 2;
                            GameObject.Find("AudioBurstCharge2").GetComponent<AudioSource>().Play();
                        } 
                        else if (m_BurstChargeFloatProperty.value >= 0.5f && m_BurstLevel == 0)
                        {
                            m_BurstLevel = 1;
                            GameObject.Find("AudioBurstCharge1").GetComponent<AudioSource>().Play();
                        }    
                    }
                        


                    if (GetButtonUp(FpsInputButton.LeanLeft))
                    {
                        //clamp burst to half or full strength
                        if (m_BurstChargeFloatProperty.value > 0.5f)
                        {
                            if (m_BurstChargeFloatProperty.value < 1f)
                            {
                                m_BurstChargeFloatProperty.value = 0.75f;
                            }
                            m_BurstReleaseTrigger.Trigger();
                            m_BurstLevel = 0;
                        }


                        //m_BurstReleaseTrigger.Trigger();
                        //DS START
                        //MMFeedbacks playerFeedbacks = (MMFeedbacks)GameObject.Find("MMFeedbacksPlayer").GetComponent<MMFeedbacks>();
                        //playerFeedbacks.PlayFeedbacks();
                        //motionGraph.GetSwitch(Animator.StringToHash("groundPoundStarted"))
                        //DS END
                    }
                }
                else
                {
                    if (GetButtonDown(FpsInputButton.LeanLeft))
                    {
                        m_BurstReleaseTrigger.Trigger();
                        //DS START
                        //gpJumpElapsed = 0f;
                        //DS END
                    }
                }
            }


            // Lean
            if (m_BodyLean != null)
            {
                if (m_ToggleLean)
                {
                    if (GetButtonDown(FpsInputButton.LeanLeft))
                    {
                        if (Mathf.Abs(m_BodyLean.targetLean) > 0.9f)
                            m_BodyLean.ResetLean();
                        else
                            m_BodyLean.LeanLeft(1f);
                    }
                    if (GetButtonDown(FpsInputButton.LeanRight))
                    {
                        if (Mathf.Abs(m_BodyLean.targetLean) > 0.9f)
                            m_BodyLean.ResetLean();
                        else
                            m_BodyLean.LeanRight(1f);
                    }
                }
                else
                {
                    int lean = 0;
                    if (GetButton(FpsInputButton.LeanLeft))
                        --lean;
                    if (GetButton(FpsInputButton.LeanRight))
                        ++lean;

                    switch (lean)
                    {
                        case -1:
                            m_BodyLean.LeanLeft(1f);
                            break;
                        case 1:
                            m_BodyLean.LeanRight(1f);
                            break;
                        case 0:
                            m_BodyLean.ResetLean();
                            break;
                    }
                }
            }

            // Interact / Use
            if (GetButtonDown (FpsInputButton.Use))
				m_InteractionMgr.InteractPress ();
			if (GetButtonUp (FpsInputButton.Use))
				m_InteractionMgr.InteractRelease ();
        }
	}
}