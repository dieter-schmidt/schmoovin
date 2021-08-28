using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.PlayerCharacter
{
    class PlayerCharacterStaminaStep : NeoFpsWizardStep
    {
        [Tooltip("The type of stamina or breathing effects that the character uses.")]
        public int staminaAndBreathing = 0;

        [Tooltip("The time in seconds between breaths.")]
        public float breathInterval = 4f;
        [Range(0f, 1f), Tooltip("The strength of the character's breathing (0 = non-existant, 1 = heaving/panting).")]
        public float breathStrength = 0.25f;

        [Tooltip("The current stamina of the character. This acts as the starting stamina and changes at runtime.")]
        public float stamina = 100f;
        [Tooltip("The maximum stamina of the character.")]
        public float maxStamina = 100f;
        [Tooltip("The rate that stamina increases over time when no drains are applied.")]
        public float staminaRefreshRate = 10f;

        [Tooltip("Should the character's movement speed be modified by the stamina.")]
        public bool modifyMovementSpeed = true;
        [Tooltip("The name of the motion data property on the motion graph that defines walk speed.")]
        public string walkSpeedData = string.Empty;
        [Tooltip("The name of the motion data property on the motion graph that defines walk speed when aiming.")]
        public string aimWalkSpeedData = string.Empty;
        [Tooltip("The name of the motion data property on the motion graph that defines sprint speed.")]
        public string sprintSpeedData = string.Empty;
        [Tooltip("The name of the motion data property on the motion graph that defines sprint speed when aiming.")]
        public string aimSprintSpeedData = string.Empty;
        [Tooltip("The name of the motion data property on the motion graph that defines crouch movement speed.")]
        public string crouchSpeedData = string.Empty;
        [Tooltip("The name of the motion data property on the motion graph that defines crouch movement speed when aiming.")]
        public string aimCrouchSpeedData = string.Empty;
        [Tooltip("The lowest walk speed to blend between based on stamina.")]
        public float minWalkMultiplier = 0.5f;
        [Tooltip("The highest walk speed to blend between based on stamina.")]
        public float minSprintMultiplier = 0.5f;
        [Tooltip("The lowest sprint speed to blend between based on stamina.")]
        public float minCrouchMultiplier = 0.5f;

        [Tooltip("The time in seconds between breaths (when breathing slow).")]
        public float breatheSlowInterval = 5f;
        [Tooltip("The time in seconds between breaths (when breathing fast).")]
        public float breatheFastInterval = 1f;

        [Tooltip("Should the character suffer an exhaustion effect on hitting a specific stamina threshold. The other properties will be hidden if this is false.")]
        public bool useExhaustion = true;
        [Tooltip("The stamina level below which the character will become exhausted.")]
        public float exhaustionThreshold = 0f;
        [Tooltip("The character will stop being exhausted once their stamina has recovered above this value.")]
        public float recoverThreshold = 50f;
        [Tooltip("The name of the switch motion graph parameter that the graph uses as a condition for preventing sprinting.")]
        public string exhaustedMotionParameter = string.Empty;
        [Tooltip("The name of the switch motion graph parameter that the character input handler sets to tell the motion graph to start sprinting.")]
        public string sprintMotionParameter = string.Empty;

        private bool m_CanContinue = false;
        private bool m_FoldoutMgKeys = false;

        static readonly string[] staminaTypeOptions =
        {
            "None. The player character has not stamina or fatigue effects, and not breathing animations.",
            "Full stamina system. The character can be fatigued by moving around the scene and aiming down sights, affecting their movement speed and breathing.",
            "Simple breathing. The character has a fixed breathing rate, which drives procedural animation on their weapons."
        };

        static readonly string[] staminaTypeSummaries =
        {
            "None",
            "Full stamina system",
            "Simple breathing"
        };

        public override string displayName
        {
            get { return "Stamina"; }
        }

        void OnValidate()
        {
            breathInterval = Mathf.Clamp(breathInterval, 1f, 4f);

            // Stamina
            maxStamina = Mathf.Clamp(maxStamina, 1f, 100000f);
            stamina = Mathf.Clamp(stamina, 1f, 100000f);
            staminaRefreshRate = Mathf.Clamp(staminaRefreshRate, 0.1f, 100000f);

            // Movement
            minWalkMultiplier = Mathf.Clamp01(minWalkMultiplier);
            minSprintMultiplier = Mathf.Clamp01(minSprintMultiplier);
            minCrouchMultiplier = Mathf.Clamp01(minCrouchMultiplier);

            // Breathing
            breatheSlowInterval = Mathf.Clamp(breatheSlowInterval, 1f, 20f);
            breatheFastInterval = Mathf.Clamp(breatheFastInterval, 0.1f, 10f);

            // Exhaustion
            exhaustionThreshold = Mathf.Clamp(exhaustionThreshold, 1f, maxStamina - 1f);
            recoverThreshold = Mathf.Clamp(recoverThreshold, exhaustionThreshold + 1f, maxStamina);
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            var charStep = wizard.steps[PlayerCharacterWizardSteps.controller] as PlayerCharacterControllerStep;
            var motionGraph = charStep.motionGraph;

            NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("staminaAndBreathing"), staminaTypeOptions);
            switch (staminaAndBreathing)
            {
                case 1: // Full stamina
                    {
                        NeoFpsEditorGUI.Header("Stamina");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("stamina"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStamina"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaRefreshRate"));

                        NeoFpsEditorGUI.Header("Breathing");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("breatheSlowInterval"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("breatheFastInterval"));

                        NeoFpsEditorGUI.Header("Exhaustion");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("useExhaustion"));
                        if (useExhaustion)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("exhaustionThreshold"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("recoverThreshold"));
                            if (motionGraph != null)
                            {
                                NeoFpsEditorGUI.MotionGraphSwitchParamKeyField(serializedObject.FindProperty("exhaustedMotionParameter"), motionGraph);
                                NeoFpsEditorGUI.MotionGraphSwitchParamKeyField(serializedObject.FindProperty("sprintMotionParameter"), motionGraph);
                            }
                        }

                        NeoFpsEditorGUI.Header("Movement");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("modifyMovementSpeed"));
                        if (modifyMovementSpeed && motionGraph != null)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("minWalkMultiplier"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("minSprintMultiplier"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("minCrouchMultiplier"));

                            NeoFpsEditorGUI.MotionGraphFloatDataKeyField(serializedObject.FindProperty("walkSpeedData"), motionGraph);
                            NeoFpsEditorGUI.MotionGraphFloatDataKeyField(serializedObject.FindProperty("aimWalkSpeedData"), motionGraph);
                            NeoFpsEditorGUI.MotionGraphFloatDataKeyField(serializedObject.FindProperty("sprintSpeedData"), motionGraph);
                            NeoFpsEditorGUI.MotionGraphFloatDataKeyField(serializedObject.FindProperty("aimSprintSpeedData"), motionGraph);
                            NeoFpsEditorGUI.MotionGraphFloatDataKeyField(serializedObject.FindProperty("crouchSpeedData"), motionGraph);
                            NeoFpsEditorGUI.MotionGraphFloatDataKeyField(serializedObject.FindProperty("aimCrouchSpeedData"), motionGraph);
                        }

                        EditorGUILayout.Space();
                        m_FoldoutMgKeys = NeoFpsEditorGUI.ShowMotionGraphKeys(motionGraph, m_FoldoutMgKeys, true, true);
                    }
                    break;
                case 2: // Fixed breathing
                    {
                        NeoFpsEditorGUI.Header("Breathing");

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("breathInterval"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("breathStrength"));
                    }
                    break;
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.MultiChoiceSummary("staminaAndBreathing", staminaAndBreathing, staminaTypeSummaries);
            switch (staminaAndBreathing)
            {
                case 1: // Full stamina
                    {
                        GUILayout.Space(4);

                        WizardGUI.DoSummary("stamina", stamina);
                        WizardGUI.DoSummary("maxStamina", maxStamina);
                        WizardGUI.DoSummary("staminaRefreshRate", staminaRefreshRate);

                        GUILayout.Space(4);

                        WizardGUI.DoSummary("breatheSlowInterval", breatheSlowInterval);
                        WizardGUI.DoSummary("breatheFastInterval", breatheFastInterval);

                        GUILayout.Space(4);

                        WizardGUI.DoSummary("useExhaustion", useExhaustion);
                        if (useExhaustion)
                        {
                            WizardGUI.DoSummary("exhaustionThreshold", exhaustionThreshold);
                            WizardGUI.DoSummary("recoverThreshold", recoverThreshold);
                        }

                        GUILayout.Space(4);

                        WizardGUI.DoSummary("modifyMovementSpeed", modifyMovementSpeed);
                    }
                    break;
                case 2: // Fixed breathing
                    {
                        GUILayout.Space(4);

                        WizardGUI.DoSummary("breathInterval", breathInterval);
                        WizardGUI.DoSummary("breathStrength", breathStrength);
                    }
                    break;
            }
        }
    }
}
