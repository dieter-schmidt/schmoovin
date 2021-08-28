using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using UnityEditor.Animations;
using NeoFPS.Constants;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.MeleeWeapons
{
    class MeleeWeaponViewModelStep : NeoFpsWizardStep
    {
        static readonly string[] k_ExistingAnimStep = new string[] { MeleeWeaponWizardSteps.existingAnim };
        static readonly string[] k_CreateAnimStep = new string[] { MeleeWeaponWizardSteps.createAnim };

        [Tooltip("The object to show for the first person view-model of the weapon.")]
        public GameObject renderGeometry = null;
        [Tooltip("Expand the valid view model prefab type to any prefab.")]
        public bool allowNonModelPrefabs = false;
        [Tooltip("How the animator controller should be set up for the weapon.")]
        public int animatorControllerSetup = -1;
        [Tooltip("Does the weapon have sprint animations, and what kind.")]
        public int sprintAnimations = 0;

        private bool m_CanContinue = false;
        
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

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = renderGeometry != null && animatorControllerSetup != -1;
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
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("Render Geometry", renderGeometry);
            WizardGUI.MultiChoiceSummary("Animator Controller", animatorControllerSetup, animatorSummaries);
            WizardGUI.MultiChoiceSummary("Sprint Animations", sprintAnimations, sprintSummaries);
        }
    }
}
