using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.MotionData;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.PlayerCharacter
{
    class PlayerCharacterControllerStep : NeoFpsWizardStep
    {
        [Tooltip("The motion graph for the controller to use (a unique instance will be instantiated from this).")]
        public MotionGraphContainer motionGraph = null;
        [Tooltip("If this is enabled, then the collider will provide an offset that can be used to provide extra height to a jump so it appears the legs are tucked up instead of the head ducked down.")]
        public bool useCrouchJump = true;

        [Tooltip("The maximum distance above the ground to apply a \"sticky\" downforce on the frame after leaving the ground.")]
        public float slopeLimit = 45f;
        [Range(0f, 1f), Tooltip("The friction of ground contacts when standiong on a slope. At 1, all downward velocity will be cancelled out. At 0, the character will slide down the slope.")]
        public float slopeFriction = 1f;
        [Range(0f, 1f), Tooltip("The friction of ground contacts when overhanging a ledge. At 1, the character will not slide off the ledge.")]
        public float ledgeFriction = 0f;
        [Tooltip("The angle (in degrees) from the vertical for a surface to be considered a wall.")]
        public float wallAngle = 5f;
        [Tooltip("The character will traverse any ledge up to their radius in height. If the step is equal to or below the step height then the character will not lose any horizontal speed when stepping up, and any vertical movement does not count to the character's velocity calculations.")]
        public float stepHeight = 0.3f;
        [Tooltip("The maximum distance above the ground to apply a \"sticky\" downforce on the frame after leaving the ground in certain conditions. This prevents leaving the ground when stepping onto down-slopes or off low steps.")]
        public float groundSnapHeight = 0.3f;
        [Tooltip("Do not apply forces to non-kinematic rigidbodies if false.")]
        public bool pushRigidbodies = true;
        [Tooltip("Any rigidbodies this mass or below will be pushed with the full push multiplier. Above this and it drops off to zero at max mass.")]
        public float lowRigidbodyPushMass = 10f;
        [Tooltip("Any rigidbodies above this mass will have zero force applied to them.")]
        public float maxRigidbodyPushMass = 200f;
        [Tooltip("A multiplier for the push force at or below the minimum push mass. At normal gravity with no physics materials applied, a 1m box will be on the threshold of moving when this is set to 10. Higher will push the box up to the character's velocity with greater acceleration.")]
        public float rigidbodyPush = 20f;
        [Tooltip("Can this character be pushed by other INeoCharacterControllers.")]
        public bool pushedByCharacters = true;
        [Tooltip("Can this character push other INeoCharacterControllers.")]
        public bool pushCharacters = true;
        [Tooltip("A multiplier for the push force when pushing characters at or below this characters mass. Drops to 0 when approaching mass push mass.")]
        public float characterPush = 2.5f;
        [Tooltip("Rigidbodies on these layers will be treated as moving platforms and influence the character movement. These must be included in the layer collision matrix for this object's layer.")]
        public float gravity = 9.8f;

        const float k_ClampMaxSlopeLow = 30f;
        const float k_ClampWallAngleLow = 0.5f;
        const float k_ClampWallAngleHigh = 5f;
        const float k_ClampMaxGroundingDistanceHigh = 0.5f;
        const float k_ClampMinRbPushMassHigh = 50f;
        const float k_ClampMaxRbPushMassHigh = 1000f;
        const float k_ClampMinRbPushHigh = 50f;
        const float k_ClampMaxCharPushLow = 0.1f;
        const float k_ClampMaxCharPushHigh = 20f;

        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "Character Controller"; }
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            wallAngle = Mathf.Clamp(wallAngle, k_ClampWallAngleLow, k_ClampWallAngleHigh);
            groundSnapHeight = Mathf.Clamp(groundSnapHeight, 0f, k_ClampMaxGroundingDistanceHigh);
            lowRigidbodyPushMass = Mathf.Clamp(lowRigidbodyPushMass, 0f, k_ClampMinRbPushMassHigh);
            maxRigidbodyPushMass = Mathf.Clamp(maxRigidbodyPushMass, lowRigidbodyPushMass, k_ClampMaxRbPushMassHigh);
            rigidbodyPush = Mathf.Clamp(rigidbodyPush, 1f, k_ClampMinRbPushHigh);
            characterPush = Mathf.Clamp(characterPush, k_ClampMaxCharPushLow, k_ClampMaxCharPushHigh);

            // Clamp properties that rely on others
            float clampMaxSlopeHigh = 90f - wallAngle;
            slopeLimit = Mathf.Clamp(slopeLimit, k_ClampMaxSlopeLow, clampMaxSlopeHigh);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            m_CanContinue &= NeoFpsEditorGUI.RequiredAssetField<MotionGraphContainer>(serializedObject.FindProperty("motionGraph"));

            NeoFpsEditorGUI.Header("Jumping And Falling");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("useCrouchJump"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"));

            NeoFpsEditorGUI.Header("Slopes And Steps");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("slopeLimit"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("slopeFriction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ledgeFriction"));

            float oldSH = serializedObject.FindProperty("stepHeight").floatValue;
            float sh = EditorGUILayout.DelayedFloatField("Step Height", oldSH);
            if (sh != oldSH)
            {
                var root = wizard.steps[PlayerCharacterWizardSteps.root] as PlayerCharacterRootStep;

                float clampMaxSlopeHigh = 90f - wallAngle;
                float r = root.characterWidth / 2f;
                float clampStepOffsetHigh = r - (r - 0.005f) * Mathf.Cos(Mathf.Deg2Rad * clampMaxSlopeHigh);
                sh = Mathf.Clamp(sh, 0f, clampStepOffsetHigh);

                serializedObject.FindProperty("stepHeight").floatValue = sh;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("groundSnapHeight"));

            NeoFpsEditorGUI.Header("Interaction With Objects");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("pushRigidbodies"));
            if (pushRigidbodies)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lowRigidbodyPushMass"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRigidbodyPushMass"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rigidbodyPush"));
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pushedByCharacters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pushCharacters"));
            if (pushCharacters)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("characterPush"));
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("motionGraph", motionGraph);

            GUILayout.Space(4);

            WizardGUI.DoSummary("useCrouchJump", useCrouchJump);
            WizardGUI.DoSummary("gravity", gravity);

            GUILayout.Space(4);

            WizardGUI.DoSummary("slopeLimit", slopeLimit);
            WizardGUI.DoSummary("wallAngle", wallAngle);
            WizardGUI.DoSummary("slopeFriction", slopeFriction);
            WizardGUI.DoSummary("ledgeFriction", ledgeFriction);
            WizardGUI.DoSummary("stepHeight", stepHeight);
            WizardGUI.DoSummary("groundSnapHeight", groundSnapHeight);

            GUILayout.Space(4);

            WizardGUI.DoSummary("pushRigidbodies", pushRigidbodies);
            if (pushRigidbodies)
            {
                WizardGUI.DoSummary("lowRigidbodyPushMass", lowRigidbodyPushMass);
                WizardGUI.DoSummary("maxRigidbodyPushMass", maxRigidbodyPushMass);
                WizardGUI.DoSummary("rigidbodyPush", rigidbodyPush);
            }
            WizardGUI.DoSummary("pushedByCharacters", pushedByCharacters);
            WizardGUI.DoSummary("pushCharacters", pushCharacters);
            if (pushCharacters)
                WizardGUI.DoSummary("characterPush", characterPush);
        }
    }
}
