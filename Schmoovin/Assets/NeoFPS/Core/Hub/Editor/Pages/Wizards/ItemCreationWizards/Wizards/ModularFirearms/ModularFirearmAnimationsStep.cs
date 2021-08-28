using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor;
using NeoFPSEditor.Hub;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ModularFirearms
{
    public class ModularFirearmAnimationsStep : NeoFpsWizardStep
    {
        private bool m_CanContinue = false;

        [Tooltip("The trigger for the fire animation (blank = no animation).")]
        public string fireTrigger = "Fire";
        [Tooltip("The trigger for the weapon raise animation (blank = no animation).")]
        public string raiseTrigger = "Draw";
        [Tooltip("The duration in seconds for raising the weapon.")]
        public float raiseDuration = 0.5f;
        [Tooltip("The trigger for the weapon lower animation (blank = no animation).")]
        public string lowerTrigger = string.Empty;
        [Tooltip("The time taken to lower the item on deselection.")]
        public float lowerDuration = 0.25f;
        [Tooltip("The animator controller trigger key for the reload animation.")]
        public string reloadTrigger = "Reload";
        [Tooltip("The key to an animator bool parameter to set when the weapon is empty")]
        public string chamberEmptyBool = "Empty";
        [Tooltip("The animator controller parameter key for the reload count.")]
        public string reloadCountInt = "ReloadCount";
        [Tooltip("The bool animator property key to set while the trigger is pressed.")]
        public string triggerHoldBool = "TriggerHold";
        [Tooltip("The animator parameter key for a bool used to control aiming state in animations.")]
        public string aimBool = "";
        [Tooltip("The speed the character is moving for the sprint animations to sync up at 1x play speed.")]
        public float unscaledSprintMoveSpeed = 10f;
        [Tooltip("A bool parameter on the animator to signify when the weapon enters or exits sprint.")]
        public string sprintBool = "Sprint";
        [Tooltip("A float parameter on the animator to set the playback speed of the sprint animation.")]
        public string sprintSpeedFloat = "SprintSpeed";
        [Tooltip("A float parameter on the animator to set the blend between light and heavy sprinting.")]
        public string sprintBlendFloat = "SprintBlend";

        [SerializeField, HideInInspector] private bool m_FoldoutAnimatorKeys = true;

        public override string displayName
        {
            get { return "Animations"; }
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            var reloadStep = wizard.steps[ModularFirearmWizardSteps.reload] as ModularFirearmReloadStep;
            switch (reloadStep.reloaderModule)
            {
                case ModularFirearmReloadStep.ReloaderModule.Simple:
                    m_CanContinue &= !string.IsNullOrWhiteSpace("reloadTrigger");
                    break;
                case ModularFirearmReloadStep.ReloaderModule.Chambered:
                    m_CanContinue &= !string.IsNullOrWhiteSpace("reloadTrigger");
                    m_CanContinue &= !string.IsNullOrWhiteSpace("chamberEmptyBool");
                    break;
                case ModularFirearmReloadStep.ReloaderModule.Incremental:
                    m_CanContinue &= !string.IsNullOrWhiteSpace("reloadTrigger");
                    m_CanContinue &= !string.IsNullOrWhiteSpace("reloadCountInt");
                    break;
            }

            var root = wizard.steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            if (root.sprintingStyle == 2)
                m_CanContinue &= !string.IsNullOrWhiteSpace("sprintBool") && !string.IsNullOrWhiteSpace("sprintSpeedFloat");
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            raiseDuration = Mathf.Clamp(raiseDuration, 0.1f, 5f);
            lowerDuration = Mathf.Clamp(lowerDuration, 0.1f, 5f);
            unscaledSprintMoveSpeed = Mathf.Clamp(unscaledSprintMoveSpeed, 1f, 50f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            var root = wizard.steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            var animController = root.animationController;
            if (animController != null)
            {
                m_CanContinue = true;

                // How to get animations from everything?
                var triggerStep = wizard.steps[ModularFirearmWizardSteps.trigger] as ModularFirearmTriggerStep;
                var reloadStep = wizard.steps[ModularFirearmWizardSteps.reload] as ModularFirearmReloadStep;
                var aimingStep = wizard.steps[ModularFirearmWizardSteps.aiming] as ModularFirearmAimingStep;

                NeoFpsEditorGUI.AnimatorTriggerKeyField(serializedObject.FindProperty("fireTrigger"), animController, true);
                NeoFpsEditorGUI.AnimatorTriggerKeyField(serializedObject.FindProperty("raiseTrigger"), animController, true);
                if (!string.IsNullOrWhiteSpace(raiseTrigger))
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("raiseDuration"));
                NeoFpsEditorGUI.AnimatorTriggerKeyField(serializedObject.FindProperty("lowerTrigger"), animController, true);
                if (!string.IsNullOrWhiteSpace(lowerTrigger))
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("lowerDuration"));

                GUILayout.Space(4);

                switch(reloadStep.reloaderModule)
                {
                    case ModularFirearmReloadStep.ReloaderModule.Simple:
                        m_CanContinue &= NeoFpsEditorGUI.AnimatorTriggerKeyField(serializedObject.FindProperty("reloadTrigger"), animController, true);
                        break;
                    case ModularFirearmReloadStep.ReloaderModule.Chambered:
                        m_CanContinue &= NeoFpsEditorGUI.AnimatorTriggerKeyField(serializedObject.FindProperty("reloadTrigger"), animController, true);
                        m_CanContinue &= NeoFpsEditorGUI.AnimatorBoolKeyField(serializedObject.FindProperty("chamberEmptyBool"), animController, true);
                        break;
                    case ModularFirearmReloadStep.ReloaderModule.Incremental:
                        m_CanContinue &= NeoFpsEditorGUI.AnimatorTriggerKeyField(serializedObject.FindProperty("reloadTrigger"), animController, true);
                        m_CanContinue &= NeoFpsEditorGUI.AnimatorIntKeyField(serializedObject.FindProperty("reloadCountInt"), animController, true);
                        break;
                }

                GUILayout.Space(4);
                
                switch (triggerStep.triggerModule)
                {
                    case ModularFirearmTriggerStep.TriggerModule.Automatic:
                        NeoFpsEditorGUI.AnimatorBoolKeyField(serializedObject.FindProperty("triggerHoldBool"), animController, true);
                        break;
                    case ModularFirearmTriggerStep.TriggerModule.Burst:
                        NeoFpsEditorGUI.AnimatorBoolKeyField(serializedObject.FindProperty("triggerHoldBool"), animController, true);
                        break;
                }

                GUILayout.Space(4);

                if (aimingStep.aimerModule != ModularFirearmAimingStep.AimerModule.None && aimingStep.aimerModule != ModularFirearmAimingStep.AimerModule.InstantScopedAimer)
                    NeoFpsEditorGUI.AnimatorBoolKeyField(serializedObject.FindProperty("aimBool"), animController, true);
                
                GUILayout.Space(4);

                if (root.sprintingStyle == 2)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("unscaledSprintMoveSpeed"));
                    m_CanContinue &= NeoFpsEditorGUI.RequiredAnimatorBoolKeyField(serializedObject.FindProperty("sprintBool"), animController);
                    m_CanContinue &= NeoFpsEditorGUI.RequiredAnimatorFloatKeyField(serializedObject.FindProperty("sprintSpeedFloat"), animController);
                    NeoFpsEditorGUI.AnimatorFloatKeyField(serializedObject.FindProperty("sprintBlendFloat"), animController);
                }

                EditorGUILayout.Space();

                // Show the available animator keys at the bottom
                m_FoldoutAnimatorKeys = NeoFpsEditorGUI.ShowAnimatorKeys(animController, m_FoldoutAnimatorKeys);
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            var root = wizard.steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            if (root.animationController != null)
            {
                // How to get animations from everything?
                var triggerStep = wizard.steps[ModularFirearmWizardSteps.trigger] as ModularFirearmTriggerStep;
                var reloadStep = wizard.steps[ModularFirearmWizardSteps.reload] as ModularFirearmReloadStep;
                var aimingStep = wizard.steps[ModularFirearmWizardSteps.aiming] as ModularFirearmAimingStep;

                WizardGUI.DoSummary("fireTrigger", fireTrigger);
                WizardGUI.DoSummary("raiseTrigger", raiseTrigger);
                if (!string.IsNullOrWhiteSpace(raiseTrigger))
                    WizardGUI.DoSummary("raiseDuration", raiseDuration);
                WizardGUI.DoSummary("lowerTrigger", lowerTrigger);
                if (!string.IsNullOrWhiteSpace(lowerTrigger))
                    WizardGUI.DoSummary("lowerDuration", lowerDuration);
                
                GUILayout.Space(4);

                switch (reloadStep.reloaderModule)
                {
                    case ModularFirearmReloadStep.ReloaderModule.Simple:
                        WizardGUI.DoSummary("reloadTrigger", reloadTrigger);
                        break;
                    case ModularFirearmReloadStep.ReloaderModule.Chambered:
                        WizardGUI.DoSummary("reloadTrigger", reloadTrigger);
                        WizardGUI.DoSummary("chamberEmptyBool", chamberEmptyBool);
                        break;
                    case ModularFirearmReloadStep.ReloaderModule.Incremental:
                        WizardGUI.DoSummary("reloadTrigger", reloadTrigger);
                        WizardGUI.DoSummary("reloadCountInt", reloadCountInt);
                        break;
                }

                GUILayout.Space(4);

                switch (triggerStep.triggerModule)
                {
                    case ModularFirearmTriggerStep.TriggerModule.Automatic:
                        WizardGUI.DoSummary("triggerHoldBool", triggerHoldBool);
                        break;
                    case ModularFirearmTriggerStep.TriggerModule.Burst:
                        WizardGUI.DoSummary("triggerHoldBool", triggerHoldBool);
                        break;
                }

                GUILayout.Space(4);

                if (aimingStep.aimerModule != ModularFirearmAimingStep.AimerModule.None && aimingStep.aimerModule != ModularFirearmAimingStep.AimerModule.InstantScopedAimer)
                    WizardGUI.DoSummary("aimBool", aimBool);
                
                if (root.sprintingStyle == 2)
                {
                    GUILayout.Space(4);

                    WizardGUI.DoSummary("unscaledSprintMoveSpeed", unscaledSprintMoveSpeed);
                    WizardGUI.DoSummary("sprintBool", sprintBool);
                    WizardGUI.DoSummary("sprintSpeedFloat", sprintSpeedFloat);
                    WizardGUI.DoSummary("sprintBlendFloat", sprintBlendFloat);
                }
            }
        }
    }
}
