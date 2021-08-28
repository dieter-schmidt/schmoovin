using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.Pickups
{
    public class HealthPackSetup : NeoFpsWizardStep
    {
        [Tooltip("How is the heal amount applied")]
        public int healType = 0;
        [Tooltip("The max amount of health to add to the character")]
        public float healAmount = 25f;
        [Tooltip("The health factor to add to the character")]
        public float healFactor = 0.25f;
        [Tooltip("At what point is the pickup consumed")]
        public int consumeType = 1;
        [Tooltip("What to do with the pickup once consumed")]
        public int consumeResult = 0;
        [Tooltip("The amount of time before the pickup resets and respawns")]
        public float respawnDuration = 5f;

        static readonly string[] healOptions =
        {
            "Fixed value. This restores a set amount of health.",
            "Factor. This restores health as a factor of max health (1 restores all, 0.5 restores 50% of total health).",
            "Missing factor. This restores health as a factor of max health minus current health (1 restores all missing health, 0.5 restores half the missing health)."
        };

        static readonly string[] consumeTypeOptions =
        {
            "Single-use. As soon as any of the health is used, the health pickup is consumed.",
            "Multi-use. The health pickup can be used until it runs out, and only then will it be consumed.",
        };

        static readonly string[] consumeResultOptions =
        {
            "Destroy. This completely removes the pickup from the world when consumed.",
            "Disable. This disables the render geo and trigger colliders, allowing you to re-enable them later.",
            "Respawn. This disables the render geo and trigger colliders, and then re-enables them after a set duration."
        };

        static readonly string[] healSummary =
        {
            "Fixed value",
            "Factor",
            "Missing factor"
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
            get { return "Health Pack Setup"; }
        }

        public override bool CheckCanContinue(NeoFpsWizard wizard)
        {
            return true;
        }

        void OnValidate()
        {
            if (healAmount < 1f)
                healAmount = 1f;
            healFactor = Mathf.Clamp01(healFactor);
            respawnDuration = Mathf.Clamp(respawnDuration, 1f, 3600f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            // Heal type & amount
            NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("healType"), healOptions);
            if (healType == 0)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("healAmount"));
            else
                EditorGUILayout.PropertyField(serializedObject.FindProperty("healFactor"));

            // Consume
            NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("consumeType"), consumeTypeOptions);
            NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("consumeResult"), consumeResultOptions);
            if (consumeResult == 2)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("respawnDuration"));
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            // Heal type & amount
            WizardGUI.MultiChoiceSummary("Heal Type", healType, healSummary);
            if (healType == 0)
                WizardGUI.DoSummary("Heal Amount", healAmount);
            else
                WizardGUI.DoSummary("Heal Factor", healFactor);

            // Consume
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