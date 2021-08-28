using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using UnityEditor.Animations;
using NeoFPS.Constants;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ThrownWeapons
{
    class ThrownWeaponAudioStep : NeoFpsWizardStep
    {
        [Tooltip("The audio clip to play for the primary attack (heavy throw).")]
        public AudioClip heavyThrowAudio = null;
        [Tooltip("The audio clip to play for the secondary attack (light throw).")]
        public AudioClip lightThrowAudio = null;
        [Tooltip("The audio clip to play when the weapon is selected and raised.")]
        public AudioClip selectAudio = null;

        public static class DataKeys
        {
            public const string selectAudio = "selectAudio";
            public const string lightThrowAudio = "lightThrowAudio";
            public const string heavyThrowAudio = "heavyThrowAudio";
        }

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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("heavyThrowAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lightThrowAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectAudio"));
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("Heavy Throw Audio", heavyThrowAudio);
            WizardGUI.ObjectSummary("Light Throw Audio", lightThrowAudio);
            WizardGUI.ObjectSummary("Select Audio", selectAudio);
        }
    }
}
