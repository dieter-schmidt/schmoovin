using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.InteractiveObjects
{
    class InteractiveObjectRootStep : NeoFpsWizardStep
    {
        [Tooltip("The name to use for the final prefab.")]
        public string prefabName = "NewInteractiveObject";
        [Tooltip("Automatically add prefixes such as the pickup type to the prefab name.")]
        public bool autoPrefix = true;
        [Tooltip("Overwrite the existing prefab or generate a unique name and create a new one.")]
        public bool overwriteExisting = true;

        [Tooltip("Should you tap or hold the use button to interact with the pickup.")]
        public int tapOrHold = -1;
        [Tooltip("The amount of time the player must hold the use button to interact with the pickup.")]
        public float holdDuration = 0.5f;

        [Tooltip("The name of the object as it appears in the HUD interaction tooltip.")]
        public string tooltipName = "New Interactive Object";
        [Tooltip("The action performed on the object when you interact with it as it appears in the HUD interaction tooltip.")]
        public string tooltipAction = "interact";

        private bool m_CanContinue = false;

        static readonly string[] tapOrHoldOptions =
        {
            "Instant. The pickup will be consumed as soon as the use button is pressed.",
            "Hold. The player must hold use for a set amount of time to consume the item."
        };

        static readonly string[] tapOrHoldSummaries =
        {
            "Instant",
            "Hold"
        };

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = !string.IsNullOrWhiteSpace(prefabName) && tapOrHold != -1;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            NeoFpsEditorGUI.Header("Output");

            m_CanContinue &= NeoFpsEditorGUI.RequiredStringField(serializedObject.FindProperty("prefabName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoPrefix"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("overwriteExisting"));

            NeoFpsEditorGUI.Header("Interaction");

            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("tapOrHold"), tapOrHoldOptions);
            if (tapOrHold == 1)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("holdDuration"));

            NeoFpsEditorGUI.Header("Tooltip");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("tooltipName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tooltipAction"));

            EditorGUILayout.Space();

            // Should I add simple animation and audio triggers (besides the OnUse audio)?
            EditorGUILayout.HelpBox("Once the prefab has been created, you can add your own components, and use the following events to trigger actions:\n- OnUsed\n- OnCursorEnter\n- OnCursorExit", MessageType.Info);
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.DoSummary("Prefab Name", prefabName);
            WizardGUI.DoSummary("Auto Prefix", autoPrefix);
            WizardGUI.DoSummary("Overwrite Existing", overwriteExisting);

            EditorGUILayout.Space();

            WizardGUI.MultiChoiceSummary("Tap Or Hold", tapOrHold, tapOrHoldOptions);
            if (tapOrHold == 1)
                WizardGUI.DoSummary("Hold Duration", holdDuration);

            EditorGUILayout.Space();

            WizardGUI.DoSummary("Tooltip Name", tooltipName);
            WizardGUI.DoSummary("Tooltip Action", tooltipAction);
        }

        public override string displayName
        {
            get { return "Root"; }
        }
    }
}
