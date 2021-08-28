using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ThrownWeapons
{
    class ThrownWeaponNewAnimatorStep : NeoFpsWizardStep
    {
        [Tooltip("The looping idle animation.")]
        public AnimationClip idleAnimation = null;

        [Tooltip("The primary (heavy throw) animation clip.")]
        public AnimationClip heavyThrowAnimation = null;
        [Tooltip("The secondary (light throw) animation clip.")]
        public AnimationClip lightThrowAnimation = null;

        [Tooltip("The weapon draw animation. Player whenever the weapon is selected.")]
        public AnimationClip drawWeaponAnimation = null;
        [Tooltip("The time taken to raise the item on selection.")]
        public float raiseDuration = 0.5f;
        [Tooltip("An optional weapon lower animation. If this is null, the weapon will be switched instantly.")]
        public AnimationClip lowerWeaponAnimation = null;
        [Tooltip("The time taken to lower the item on deselection.")]
        public float lowerDuration = 0f;

        [Tooltip("The animation to play when entering a blocked state. Should end in the block-idle pose.")]
        public AnimationClip blockRaiseAnimation = null;
        [Tooltip("The looping idle animation while blocking.")]
        public AnimationClip blockIdleAnimation = null;
        [Tooltip("The animation to play when exiting the blocked state. Should start in block-idle and end in idle.")]
        public AnimationClip blockLowerAnimation = null;

        [Tooltip("A looping animation used when sprinting.")]
        public AnimationClip sprintAnimation = null;
        [Tooltip("An optional looping sprint animation that is blended into at higher speeds.")]
        public AnimationClip sprintFastAnimation = null;
        [Tooltip("The movement speed in m/s that the sprint animations are tuned for. This will be used to calculate an animation play speed to match.")]
        public float sprintClipSpeed = 10f;

        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "Create New Animator Controller"; }
        }

        public bool CheckCanContinueNew(NeoFpsWizard root)
        {
            return heavyThrowAnimation != null && lightThrowAnimation != null;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("idleAnimation"));

            m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("heavyThrowAnimation"));
            m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("lightThrowAnimation"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("drawWeaponAnimation"));
            if (drawWeaponAnimation != null)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("raiseDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lowerWeaponAnimation"));
            if (lowerWeaponAnimation != null)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lowerDuration"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("blockRaiseAnimation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blockIdleAnimation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blockLowerAnimation"));

            var viewModel = wizard.steps[ThrownWeaponWizardSteps.viewModel] as ThrownWeaponViewModelStep;
            if (viewModel.sprintAnimations == 2)
            {
                m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("sprintAnimation"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sprintFastAnimation"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sprintClipSpeed"));
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("Idle Animation", idleAnimation);

            WizardGUI.ObjectSummary("Heavy Throw Animation", heavyThrowAnimation);
            WizardGUI.ObjectSummary("Light Throw Animation", lightThrowAnimation);

            WizardGUI.ObjectSummary("Draw Weapon Animation", drawWeaponAnimation);
            if (drawWeaponAnimation != null)
                WizardGUI.DoSummary("Raise Duration", raiseDuration);
            WizardGUI.ObjectSummary("Lower Weapon Animation", lowerWeaponAnimation);
            if (lowerWeaponAnimation != null)
                WizardGUI.DoSummary("Lower Duration", lowerDuration);

            WizardGUI.ObjectSummary("Block Raise Animation", blockRaiseAnimation);
            WizardGUI.ObjectSummary("Block Idle Animation", blockIdleAnimation);
            WizardGUI.ObjectSummary("Block Lower Animation", blockLowerAnimation);

            var viewModel = wizard.steps[ThrownWeaponWizardSteps.viewModel] as ThrownWeaponViewModelStep;
            if (viewModel.sprintAnimations == 2)
            {
                WizardGUI.ObjectSummary("Sprint Animation", sprintAnimation);
                WizardGUI.ObjectSummary("Sprint Fast Animation", sprintFastAnimation);
                WizardGUI.DoSummary("Sprint Clip Speed", sprintClipSpeed);
            }
        }
    }
}
