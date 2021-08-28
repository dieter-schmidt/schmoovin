using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using UnityEditor.Animations;
using NeoFPS.Constants;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.MeleeWeapons
{
    class MeleeWeaponAudioStep : NeoFpsWizardStep
    {
        [Tooltip("The audio clip to play when the weapon attacks.")]
        public AudioClip attackAudio = null;
        [Tooltip("The audio clip to play when the weapon is selected and raised.")]
        public AudioClip selectAudio = null;
        [Tooltip("The audio clip to play when the weapon is raised into a block state.")]
        public AudioClip blockRaiseAudio = null;
        [Tooltip("The audio clip to play when the weapon is lowered from a block state.")]
        public AudioClip blockLowerAudio = null;

        public override string displayName
        {
            get { return "Audio"; }
        }
        
        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return true;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blockRaiseAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blockLowerAudio"));
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("Attack Audio", attackAudio);
            WizardGUI.ObjectSummary("Select Audio", selectAudio);
            WizardGUI.ObjectSummary("Block Raise Audio", blockRaiseAudio);
            WizardGUI.ObjectSummary("Block Lower Audio", blockLowerAudio);
        }
    }
}
