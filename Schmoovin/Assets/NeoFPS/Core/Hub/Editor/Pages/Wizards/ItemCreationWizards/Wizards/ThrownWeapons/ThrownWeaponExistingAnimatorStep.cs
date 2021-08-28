using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ThrownWeapons
{
    class ThrownWeaponExistingAnimatorStep : NeoFpsWizardStep
    {
        [Tooltip("The animator controller for the weapon view-model.")]
        public AnimatorController animatorController = null;

        [Tooltip("The name of the animator trigger parameter for triggering the primary (heavy) attack animation.")]
        public string heavyThrowAnimatorTrigger = "ThrowHeavy";
        [Tooltip("The name of the animator trigger parameter for triggering the secondary (light) attack animation.")]
        public string lightThrowAnimatorTrigger = "ThrowLight";

        [Tooltip("The name of the animator trigger used when the weapon is drawn.")]
        public string drawAnimatorTrigger = "Draw";
        [Tooltip("The time taken to raise the item on selection.")]
        public float raiseDuration = 0.5f;
        [Tooltip("The name of the animator trigger used when the weapon is lowered.")]
        public string lowerAnimatorTrigger = string.Empty;
        [Tooltip("The time taken to lower the item on deselection.")]
        public float lowerDuration = 0f;

        [Tooltip("The name of the animator bool parameter used to signal when sprinting.")]
        public string sprintAnimatorBoolParameter = "Sprint";
        [Tooltip("The name of the animator float parameter used to specify the playback speed of the sprint animation.")]
        public string sprintSpeedAnimatorFloatParameter = "SprintSpeed";
        [Tooltip("The name of the animator float parameter used to specify the strength of the sprint animation.")]
        public string sprintBlendAnimatorFloatParameter = "SprintBlend";

        [SerializeField, HideInInspector] private bool m_FoldoutAnimatorKeys = true;

        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "Existing Animator Controller Setup"; }
        }

        void OnValidate()
        {
            raiseDuration = Mathf.Clamp(raiseDuration, 0f, 5f);
            lowerDuration = Mathf.Clamp(lowerDuration, 0f, 5f);
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            if (animatorController == null)
                m_CanContinue = false;
            if (string.IsNullOrWhiteSpace(heavyThrowAnimatorTrigger))
                m_CanContinue = false;
            if (string.IsNullOrWhiteSpace(lightThrowAnimatorTrigger))
                m_CanContinue = false;

            var viewModel = wizard.steps[ThrownWeaponWizardSteps.viewModel] as ThrownWeaponViewModelStep;
            if (viewModel.sprintAnimations == 2)
            {
                if (string.IsNullOrWhiteSpace(sprintAnimatorBoolParameter))
                    m_CanContinue = false;
                if (string.IsNullOrWhiteSpace(sprintSpeedAnimatorFloatParameter))
                    m_CanContinue = false;
            }
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("animatorController"));
            if (animatorController != null)
            {
                m_CanContinue &= NeoFpsEditorGUI.RequiredAnimatorTriggerKeyField(serializedObject.FindProperty("heavyThrowAnimatorTrigger"), animatorController);
                m_CanContinue &= NeoFpsEditorGUI.RequiredAnimatorTriggerKeyField(serializedObject.FindProperty("lightThrowAnimatorTrigger"), animatorController);

                NeoFpsEditorGUI.AnimatorTriggerKeyField(serializedObject.FindProperty("drawAnimatorTrigger"), animatorController);
                if (drawAnimatorTrigger != string.Empty)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("raiseDuration"));
                NeoFpsEditorGUI.AnimatorTriggerKeyField(serializedObject.FindProperty("lowerAnimatorTrigger"), animatorController);
                if (lowerAnimatorTrigger != string.Empty)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lowerDuration"));

                // Spring
                var viewModel = wizard.steps[ThrownWeaponWizardSteps.viewModel] as ThrownWeaponViewModelStep;
                if (viewModel.sprintAnimations == 2)
                {
                    m_CanContinue &= NeoFpsEditorGUI.RequiredAnimatorBoolKeyField(serializedObject.FindProperty("sprintAnimatorBoolParameter"), animatorController);
                    m_CanContinue &= NeoFpsEditorGUI.RequiredAnimatorFloatKeyField(serializedObject.FindProperty("sprintSpeedAnimatorFloatParameter"), animatorController);
                    NeoFpsEditorGUI.AnimatorFloatKeyField(serializedObject.FindProperty("sprintBlendAnimatorFloatParameter"), animatorController);
                }

                EditorGUILayout.Space();

                // Show the available animator keys at the bottom
                m_FoldoutAnimatorKeys = NeoFpsEditorGUI.ShowAnimatorKeys(animatorController, m_FoldoutAnimatorKeys);
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("Animator Controller", animatorController);

            WizardGUI.DoSummary("Heavy Throw Animator Trigger", heavyThrowAnimatorTrigger);
            WizardGUI.DoSummary("Light Throw Animator Trigger", lightThrowAnimatorTrigger);

            WizardGUI.DoSummary("Draw Animator Trigger", drawAnimatorTrigger);
            if (drawAnimatorTrigger != string.Empty)
                WizardGUI.DoSummary("Raise Duration", raiseDuration);
            WizardGUI.DoSummary("Lower Animator Trigger", lowerAnimatorTrigger);
            if (lowerAnimatorTrigger != string.Empty)
                WizardGUI.DoSummary("Lower Duration", lowerDuration);

            // Spring
            var viewModel = wizard.steps[ThrownWeaponWizardSteps.viewModel] as ThrownWeaponViewModelStep;
            if (viewModel.sprintAnimations == 2)
            {
                WizardGUI.DoSummary("Sprint Animator Bool Parameter", sprintAnimatorBoolParameter);
                WizardGUI.DoSummary("sprintSpeedAnimatorFloatParameter", sprintSpeedAnimatorFloatParameter);
                WizardGUI.DoSummary("sprintBlendAnimatorFloatParameter", sprintBlendAnimatorFloatParameter);
            }
        }
    }
}
