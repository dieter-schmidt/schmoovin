using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.PlayerCharacter
{
    class PlayerCharacterInputStep : NeoFpsWizardStep
    {
        [Tooltip("Use the neofps character movement input class.")]
        public bool neoMovementInput = true;

        [Tooltip("Should double tapping a move direction set the dodge direction and trigger properties.")]
        public bool enableDodging = false;
        //[Tooltip("Does holding the jump button charge up a jump or does the character dodge as soon as the button is pressed.")]
        //public bool enableChargedJump = false;
        [Tooltip("Toggle leaning or hold to lean.")]
        public bool toggleLean = true;

        [Tooltip("The key to the jump trigger property in the character motion graph.")]
        public string jumpKey = "jump";
        [Tooltip("The key to the jump charge float property in the character motion graph.")]
        public string jumpChargeKey = "jumpCharge";
        [Tooltip("The key to the jump hold switch property in the character motion graph.")]
        public string jumpHoldKey = "jumpHold";
        [Tooltip("The key to the crouch switch property in the character motion graph.")]
        public string crouchKey = "crouch";
        [Tooltip("The key to the crouch hold switch property in the character motion graph.")]
        public string crouchHoldKey = "crouchHold";
        [Tooltip("The key to the sprint switch property in the character motion graph.")]
        public string sprintKey = "sprint";
        [Tooltip("The key to the dodge left trigger property in the character motion graph.")]
        public string dodgeLeftKey = "dodgeLeft";
        [Tooltip("The key to the dodge right trigger property in the character motion graph.")]
        public string dodgeRightKey = "dodgeRight";

        [Tooltip("Use the neofps inventory input class.")]
        public bool neoInventoryInput = true;

        private bool m_CanContinue = false;
        private bool m_FoldoutMgKeys = false;

        public override string displayName
        {
            get { return "Input"; }
        }

        public override bool CheckCanContinue(NeoFpsWizard wizard)
        {
            return m_CanContinue;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            var charStep = wizard.steps[PlayerCharacterWizardSteps.controller] as PlayerCharacterControllerStep;
            var motionGraph = charStep.motionGraph;

            NeoFpsEditorGUI.Header("Movement");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("neoMovementInput"));
            if (neoMovementInput && motionGraph != null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("enableDodging"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("enableChargedJump"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toggleLean"));

                NeoFpsEditorGUI.MotionGraphTriggerParamKeyField(serializedObject.FindProperty("jumpKey"), motionGraph);
                NeoFpsEditorGUI.MotionGraphSwitchParamKeyField(serializedObject.FindProperty("jumpHoldKey"), motionGraph);
                //if (enableChargedJump)
                //    NeoFpsEditorGUI.MotionGraphFloatParamKeyField(serializedObject.FindProperty("jumpChargeKey"), motionGraph);

                NeoFpsEditorGUI.MotionGraphSwitchParamKeyField(serializedObject.FindProperty("sprintKey"), motionGraph);
                NeoFpsEditorGUI.MotionGraphSwitchParamKeyField(serializedObject.FindProperty("crouchKey"), motionGraph);
                NeoFpsEditorGUI.MotionGraphSwitchParamKeyField(serializedObject.FindProperty("crouchHoldKey"), motionGraph);

                if (enableDodging)
                {
                    NeoFpsEditorGUI.MotionGraphTriggerParamKeyField(serializedObject.FindProperty("dodgeLeftKey"), motionGraph);
                    NeoFpsEditorGUI.MotionGraphTriggerParamKeyField(serializedObject.FindProperty("dodgeRightKey"), motionGraph);
                }
            }

            NeoFpsEditorGUI.Header("Inventory");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("neoInventoryInput"));

            EditorGUILayout.Space();
            m_FoldoutMgKeys = NeoFpsEditorGUI.ShowMotionGraphKeys(motionGraph, m_FoldoutMgKeys, true, false);
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.DoSummary("neoMovementInput", neoMovementInput);
            if (neoMovementInput)
            {
                WizardGUI.DoSummary("enableDodging", neoMovementInput);
                WizardGUI.DoSummary("enableChargedJump", neoMovementInput);
                WizardGUI.DoSummary("toggleLean", neoMovementInput);

                WizardGUI.DoSummary("jumpKey", neoMovementInput);
                WizardGUI.DoSummary("jumpHoldKey", neoMovementInput);
                //if (enableChargedJump)
                //    WizardGUI.DoSummary("jumpChargeKey", neoMovementInput);

                WizardGUI.DoSummary("sprintKey", neoMovementInput);
                WizardGUI.DoSummary("crouchKey", neoMovementInput);
                WizardGUI.DoSummary("crouchHoldKey", neoMovementInput);

                if (enableDodging)
                {
                    WizardGUI.DoSummary("dodgeLeftKey", neoMovementInput);
                    WizardGUI.DoSummary("dodgeRightKey", neoMovementInput);
                }
            }
            WizardGUI.DoSummary("neoInventoryInput", neoInventoryInput);
        }
    }
}
