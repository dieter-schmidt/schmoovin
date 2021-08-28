using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor;
using NeoFPSEditor.Hub;
using UnityEngine;
using UnityEditor;
using NeoFPS.Constants;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ModularFirearms
{
    public class ModularFirearmAimingStep : NeoFpsWizardStep
    {
        [SerializeField, Tooltip("The aim-down sights style of the firearm.")]
        private int m_AimerType = -1;
        [SerializeField, Tooltip("Does the firearm switch to the HUD scope instantly, or after the weapon has been raised.")]
        private bool m_Instant = false;

        [Tooltip("An audio clip to play when the weapon is raised.")]
        public AudioClip aimUpAudio = null;
        [Tooltip("An audio clip to play when the weapon is lowered.")]
        public AudioClip aimDownAudio = null;
        [Range(0f, 1f), Tooltip("The highest accuracy possible when firing from the hip.")]
        public float hipAccuracyCap = 1f;
        [Range(0f, 1f), Tooltip("The highest accuracy possible when aiming down sights.")]
        public float aimedAccuracyCap = 1f;
        [Tooltip("Should the weapon be lowered when reloading or can it stay aimed.")]
        public bool canAimWhileReloading = false;

        [Tooltip("How to specify the offset position for the weapon when aiming down sights.")]
        public int aimPositionOption = -1;
        [Tooltip("An object in the weapon view model's hierarchy that represents where the camera should align when aiming down sights.")]
        public GameObject aimTarget = null;
        [Tooltip("The aim offset relative to the root transform. The gizmo in the scene viewport should align with the weapon sights.")]
        public Vector3 aimPosition = Vector3.zero;
        [Tooltip("The aim rotation relative to the root transform. The gizmo in the scene viewport should align with the weapon sights.")]
        public Vector3 aimRotation = Vector3.zero;
        [Range(0.1f, 1.5f), Tooltip("A multiplier for the camera FoV for aim zoom.")]
        public float fovMultiplier = 0.75f;
        [Range(0f, 2f), Tooltip("The time it takes to reach full aim, or return to zero aim.")]
        public float aimTime = 0.25f;
        [Tooltip("If true then the gun cannot fire while transitioning in and out of aim mode. This is used to prevent gunshots interrupting the animation.")]
        public bool blockTrigger = true;

        [Tooltip("The HUD scope key")]
        public string hudScopeKey = string.Empty;

        [Range(0f, 1f), Tooltip("A multiplier for weapon procedural position (to reduce severity while aiming).")]
        public float positionSpringMultiplier = 0.25f;
        [Range(0f, 1f), Tooltip("A multiplier for weapon procedural rotation (to reduce severity while aiming).")]
        public float rotationSpringMultiplier = 0.5f;

        [Tooltip("The crosshair to use when aiming down sights.")]
        public FpsCrosshair crosshairAiming = FpsCrosshair.None;
        [Tooltip("The crosshair to use when not aiming down sights.")]
        public FpsCrosshair crosshairHipFire = FpsCrosshair.Default;

        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "AimingSetup"; }
        }

        static readonly string[] aimerTypeOptions =
        {
            "Weapon-move aimer. This version raises the weapon to align it with the camera when aiming down sights. It can also zoom.",
            "Scoped aimer. This version uses a UI based scope.",
            "Animated aimer. This aimer sets an animator bool parameter to true when aiming down sights, and false when returning to hip-fire.",
            "Head-move aimer. This aimer moves the camera to align with the weapon. This can also include tilting the camera.",
            "None. The weapon can only be hip-fired, and you can't aim down sights."
        };

        static readonly string[] aimerTypeSummaries =
        {
            "Semi-Auto",
            "Automatic",
            "Burst",
            "Charged"
        };

        static readonly string[] aimerPositionOptions =
        {
            "Use a child object",
            "Manual offsets",
            "Do not move"
        };

        public enum AimerModule
        {
            None,
            WeaponMoveAimer,
            ScopedAimer,
            InstantScopedAimer,
            AnimationOnlyAimer,
            HeadMoveAimer
        }

        public AimerModule aimerModule
        {
            get
            {
                switch (m_AimerType)
                {
                    case 0:
                        return AimerModule.WeaponMoveAimer;
                    case 1:
                        if (m_Instant)
                            return AimerModule.InstantScopedAimer;
                        else
                            return AimerModule.ScopedAimer;
                    case 2:
                        return AimerModule.AnimationOnlyAimer;
                    case 3:
                        return AimerModule.HeadMoveAimer;
                }
                return AimerModule.None;
            }
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = m_AimerType != -1;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            var root = wizard.steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;

            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("m_AimerType"), aimerTypeOptions);
            if (m_AimerType == 1)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Instant"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("aimUpAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("aimDownAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hipAccuracyCap"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("aimedAccuracyCap"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canAimWhileReloading"));

            switch (aimerModule)
            {
                case AimerModule.WeaponMoveAimer:
                    {
                        // Aim position
                        m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("aimPositionOption"), aimerPositionOptions);
                        switch (aimPositionOption)
                        {
                            case 0: // Child object
                                m_CanContinue &= NeoFpsEditorGUI.RequiredGameObjectInHierarchyField(serializedObject.FindProperty("aimTarget"), root.viewModel.transform, false);
                                break;
                            case 1: // Manual
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("aimPosition"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("aimRotation"));
                                break;
                        }

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fovMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("aimTime"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("blockTrigger"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("positionSpringMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpringMultiplier"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshairHipFire"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshairAiming"));
                    }
                    break;
                case AimerModule.ScopedAimer:
                    {
                        // Aim position
                        m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("aimPositionOption"), aimerPositionOptions);
                        switch (aimPositionOption)
                        {
                            case 0: // Child object
                                m_CanContinue &= NeoFpsEditorGUI.RequiredGameObjectInHierarchyField(serializedObject.FindProperty("aimTarget"), root.viewModel.transform, false);
                                break;
                            case 1: // Manual
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("aimPosition"));
                                break;
                        }

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hudScopeKey"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fovMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("aimTime"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("blockTrigger"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("positionSpringMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpringMultiplier"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshairHipFire"));
                    }
                    break;
                case AimerModule.InstantScopedAimer:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("hudScopeKey"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fovMultiplier"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("positionSpringMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpringMultiplier"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshairHipFire"));
                    }
                    break;
                case AimerModule.AnimationOnlyAimer:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fovMultiplier"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("positionSpringMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpringMultiplier"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshairHipFire"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshairAiming"));
                    }
                    break;
                case AimerModule.HeadMoveAimer:
                    {
                        // Aim position
                        m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("aimPositionOption"), aimerPositionOptions);
                        switch (aimPositionOption)
                        {
                            case 0: // Child object
                                m_CanContinue &= NeoFpsEditorGUI.RequiredGameObjectInHierarchyField(serializedObject.FindProperty("aimTarget"), root.viewModel.transform, false);
                                break;
                            case 1: // Manual
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("aimPosition"));
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("aimRotation"));
                                break;
                        }

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fovMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("aimTime"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("blockTrigger"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("positionSpringMultiplier"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpringMultiplier"));

                        GUILayout.Space(4);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshairHipFire"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshairAiming"));
                    }
                    break;
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.MultiChoiceSummary("m_AimerType", m_AimerType, aimerTypeSummaries);
            if (m_AimerType == 1)
                WizardGUI.DoSummary("m_Instant", m_Instant);

            WizardGUI.ObjectSummary("aimUpAudio", aimUpAudio);
            WizardGUI.ObjectSummary("aimDownAudio", aimDownAudio);
            WizardGUI.DoSummary("hipAccuracyCap", hipAccuracyCap);
            WizardGUI.DoSummary("aimedAccuracyCap", aimedAccuracyCap);
            WizardGUI.DoSummary("canAimWhileReloading", canAimWhileReloading);

            switch (aimerModule)
            {
                case AimerModule.WeaponMoveAimer:
                    {
                        switch (aimPositionOption)
                        {
                            case 0: // Child object
                                WizardGUI.ObjectSummary("aimTarget", aimTarget);
                                break;
                            case 1: // Manual
                                WizardGUI.DoSummary("aimPosition", aimPosition);
                                WizardGUI.DoSummary("aimRotation", aimRotation);
                                break;
                        }

                        GUILayout.Space(4);

                        WizardGUI.DoSummary("fovMultiplier", fovMultiplier);
                        WizardGUI.DoSummary("aimTime", aimTime);
                        WizardGUI.DoSummary("blockTrigger", blockTrigger);
                        WizardGUI.DoSummary("positionSpringMultiplier", positionSpringMultiplier);
                        WizardGUI.DoSummary("rotationSpringMultiplier", rotationSpringMultiplier);
                        WizardGUI.DoSummary("crosshairHipFire", crosshairHipFire.ToString());
                        WizardGUI.DoSummary("crosshairAiming", crosshairAiming.ToString());
                    }
                    break;
                case AimerModule.ScopedAimer:
                    {
                        switch (aimPositionOption)
                        {
                            case 0: // Child object
                                WizardGUI.ObjectSummary("aimTarget", aimTarget);
                                break;
                            case 1: // Manual
                                WizardGUI.DoSummary("aimPosition", aimPosition);
                                break;
                        }

                        GUILayout.Space(4);

                        WizardGUI.DoSummary("hudScopeKey", hudScopeKey);
                        WizardGUI.DoSummary("fovMultiplier", fovMultiplier);
                        WizardGUI.DoSummary("aimTime", aimTime);
                        WizardGUI.DoSummary("blockTrigger", blockTrigger);
                        WizardGUI.DoSummary("positionSpringMultiplier", positionSpringMultiplier);
                        WizardGUI.DoSummary("rotationSpringMultiplier", rotationSpringMultiplier);
                        WizardGUI.DoSummary("crosshairHipFire", crosshairHipFire.ToString());
                    }
                    break;
                case AimerModule.InstantScopedAimer:
                    {
                        WizardGUI.DoSummary("hudScopeKey", hudScopeKey);
                        WizardGUI.DoSummary("fovMultiplier", fovMultiplier);
                        WizardGUI.DoSummary("positionSpringMultiplier", positionSpringMultiplier);
                        WizardGUI.DoSummary("rotationSpringMultiplier", rotationSpringMultiplier);
                        WizardGUI.DoSummary("crosshairHipFire", crosshairHipFire.ToString());
                    }
                    break;
                case AimerModule.AnimationOnlyAimer:
                    {
                        WizardGUI.DoSummary("fovMultiplier", fovMultiplier);
                        WizardGUI.DoSummary("positionSpringMultiplier", positionSpringMultiplier);
                        WizardGUI.DoSummary("rotationSpringMultiplier", rotationSpringMultiplier);
                        WizardGUI.DoSummary("crosshairHipFire", crosshairHipFire.ToString());
                        WizardGUI.DoSummary("crosshairAiming", crosshairAiming.ToString());
                    }
                    break;
                case AimerModule.HeadMoveAimer:
                    {
                        switch (aimPositionOption)
                        {
                            case 0: // Child object
                                WizardGUI.ObjectSummary("aimTarget", aimTarget);
                                break;
                            case 1: // Manual
                                WizardGUI.DoSummary("aimPosition", aimPosition);
                                WizardGUI.DoSummary("aimRotation", aimRotation);
                                break;
                        }

                        GUILayout.Space(4);

                        WizardGUI.DoSummary("fovMultiplier", fovMultiplier);
                        WizardGUI.DoSummary("aimTime", aimTime);
                        WizardGUI.DoSummary("blockTrigger", blockTrigger);
                        WizardGUI.DoSummary("positionSpringMultiplier", positionSpringMultiplier);
                        WizardGUI.DoSummary("rotationSpringMultiplier", rotationSpringMultiplier);
                        WizardGUI.DoSummary("crosshairHipFire", crosshairHipFire.ToString());
                        WizardGUI.DoSummary("crosshairAiming", crosshairAiming.ToString());
                    }
                    break;
            }
        }
    }
}
