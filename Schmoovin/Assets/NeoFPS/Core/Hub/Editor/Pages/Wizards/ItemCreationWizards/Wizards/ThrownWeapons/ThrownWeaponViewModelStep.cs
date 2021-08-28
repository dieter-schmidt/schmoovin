using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using UnityEditor.Animations;
using NeoFPS.Constants;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ThrownWeapons
{
    class ThrownWeaponViewModelStep : NeoFpsWizardStep
    {
        static readonly string[] k_ExistingAnimStep = new string[] { ThrownWeaponWizardSteps.existingAnim };
        static readonly string[] k_CreateAnimStep = new string[] { ThrownWeaponWizardSteps.createAnim };

        [Tooltip("The object to show for the first person view-model of the weapon.")]
        public GameObject renderGeometry = null;
        [Tooltip("Expand the valid view model prefab type to any prefab.")]
        public bool allowNonModelPrefabs = false;
        [Tooltip("How the animator controller should be set up for the weapon.")]
        public int animatorControllerSetup = -1;
        [Tooltip("The held projectile, prior to throwing. This will be disabled and swapped for the spawned projectile at the correct time.")]
        public GameObject heldItem = null;
        [Tooltip("The spwan point transform for the projectile when performing a light (secondary) throw.")]
        public GameObject projectileSpawnLight = null;
        [Tooltip("The spwan point transform for the projectile when performing a strong (primary) throw.")]
        public GameObject projectileSpawnHeavy = null;
        [Tooltip("Does the weapon have sprint animations, and what kind.")]
        public int sprintAnimations = 0;

        private bool m_CanContinue = false;

        public static class DataKeys
        {
            public const string renderGeo = "renderGeo";
            public const string heldItem = "heldItem";
            public const string animatorOption = "animatorOption";
            public const string sprintOption = "sprintOption";
        }

        static readonly string[] animatorOptions =
        {
            "Use an existing animator controller.",
            "No animator controller (add one later).",
            "Create a new animator controller by specifying clips."
        };

        static readonly string[] animatorSummaries =
        {
            "Use existing",
            "None (do later)",
            "Create a new one"
        };

        static readonly string[] sprintOptions =
        {
            "No sprint animations required.",
            "Add procedural sprint animations.",
            "Use keyframed animation clips."
        };

        static readonly string[] sprintSummaries =
        {
            "None",
            "Procedural",
            "Keyframed"
        };

        public override string displayName
        {
            get { return "View-Model"; }
        }

        public override string[] GetNextSteps()
        {
            switch (animatorControllerSetup)
            {
                case 0:
                    return k_ExistingAnimStep;
                case 2:
                    return k_CreateAnimStep;
                default:
                    return null;
            }
        }

        public bool CheckCanContinueNew(NeoFpsWizard root)
        {
            return renderGeometry != null && animatorControllerSetup != -1;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            // View model
            if (allowNonModelPrefabs)
                m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabField(serializedObject.FindProperty("renderGeometry"));
            else
                m_CanContinue &= NeoFpsEditorGUI.RequiredModelPrefabField(serializedObject.FindProperty("renderGeometry"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allowNonModelPrefabs"));
            if (!m_CanContinue)
                return;

            if (renderGeometry == null || renderGeometry.GetComponentInChildren<Animator>() == null)
            {
                EditorGUILayout.HelpBox("You need to select a render geo object with an animator component to choose the animation options.", MessageType.Warning);
            }
            else
            {
                m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("animatorControllerSetup"), animatorOptions);
                NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("sprintAnimations"), sprintOptions);
            }

            NeoFpsEditorGUI.Header("Hierarchy");

            NeoFpsEditorGUI.GameObjectInHierarchyField(serializedObject.FindProperty("heldItem"), (renderGeometry != null) ? renderGeometry.transform : null, false);
            NeoFpsEditorGUI.GameObjectInHierarchyField(serializedObject.FindProperty("projectileSpawnLight"), (renderGeometry != null) ? renderGeometry.transform : null, false);
            NeoFpsEditorGUI.GameObjectInHierarchyField(serializedObject.FindProperty("projectileSpawnHeavy"), (renderGeometry != null) ? renderGeometry.transform : null, false);
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("Render Geometry", renderGeometry);
            WizardGUI.MultiChoiceSummary("Animator Controller", animatorControllerSetup, animatorSummaries);
            WizardGUI.MultiChoiceSummary("Sprint Animations", sprintAnimations, sprintSummaries);
            WizardGUI.ObjectSummary("Held Item", heldItem);
            WizardGUI.ObjectSummary("Projectile Spawn Light", projectileSpawnLight);
            WizardGUI.ObjectSummary("Projectile Spawn Heavy", projectileSpawnHeavy);
        }
    }
}
