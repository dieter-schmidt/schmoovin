using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.Pickups
{
    public class ShieldBoosterSetup : NeoFpsWizardStep
    {
        [Tooltip("The number of shield steps to restore")]
        public int stepCount = 1;
        [Tooltip("When the shield booster should be consumed")]
        public int consumeType = 0;
        [Tooltip("What to do with the shield booster once it is consumed")]
        public int consumeResult = 0;
        [Tooltip("The amount of time before the pickup resets and respawns")]
        public float respawnDuration = 5f;

        static readonly string[] consumeTypeOptions =
        {
            "Single-use. As soon as any of the shield steps are used, the entire shield booster is consumed.",
            "Multi-use. The shield booster can be used until it runs out, and only then will it be consumed.",
        };

        static readonly string[] consumeResultOptions =
        {
            "Destroy. This completely removes the pickup from the world when consumed.",
            "Disable. This disables the render geo and trigger colliders, allowing you to re-enable them later.",
            "Respawn. This disables the render geo and trigger colliders, and then re-enables them after a set duration."
        };

        static readonly string[] consumeTypeSummary =
        {
            "Single-use",
            "Multi-use",
        };

        static readonly string[] consumeResultSummary =
        {
            "Destroy",
            "Disable",
            "Respawn"
        };

        public override string displayName
        {
            get { return "Shield Booster Setup"; }
        }

        public override bool CheckCanContinue(NeoFpsWizard wizard)
        {
            return true;
        }

        void OnValidate()
        {
            stepCount = Mathf.Clamp(stepCount, 1, 100);
            respawnDuration = Mathf.Clamp(respawnDuration, 1f, 3600f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stepCount"));
            NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("consumeType"), consumeTypeOptions);
            NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("consumeResult"), consumeResultOptions);
            if (consumeResult == 2)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("respawnDuration"));
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.DoSummary("Step Count", stepCount);
            WizardGUI.MultiChoiceSummary("Consume Type", consumeType, consumeTypeSummary);
            WizardGUI.MultiChoiceSummary("Consume Result", consumeResult, consumeResultSummary);
            if (consumeResult == 2)
                WizardGUI.DoSummary("Respawn Duration", respawnDuration);
        }

        public override string[] GetNextSteps()
        {
            return null;
        }
    }
}