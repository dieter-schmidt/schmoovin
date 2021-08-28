using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.PlayerCharacter
{
    class PlayerCharacterRootStep : NeoFpsWizardStep
    {
        [Tooltip("The name to use for the final prefab.")]
        public string prefabName = "NewCharacter";
        [Tooltip("Automatically add prefixes such as the pickup type to the prefab name.")]
        public bool autoPrefix = true;
        [Tooltip("Overwrite the existing prefab or generate a unique name and create a new one.")]
        public bool overwriteExisting = true;

        [Delayed, Tooltip("The height of the character capsule.")]
        public float characterHeight = 2f;
        [Delayed, Tooltip("The height of the character capsule.")]
        public float eyeLine = 1.75f;
        [Delayed, Tooltip("The width of the character capsule (2 * radius).")]
        public float characterWidth = 1f;
        [Delayed, Tooltip("The mass in kg of the character.")]
        public float characterMass = 80f;

        [Tooltip("The character audio library to use.")]
        public FpsCharacterAudioData characterAudioData = null;
        [Tooltip("Surface audio library used to trigger the correct sound when the character lands below the \"hard landing\" threshold.")]
        public SurfaceAudioData softLandingAudio = null;
        [Tooltip("Surface audio library used to trigger the correct sound when the character makes a heavy landing.")]
        public SurfaceAudioData hardLandingAudio = null;

        [Tooltip("Add AI seeker targets (used for turrets atm).")]
        public bool addAiSeekerTargets = true;

        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "Root"; }
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            characterHeight = Mathf.Clamp(characterHeight, 0.5f, 10f);
            eyeLine = Mathf.Clamp(eyeLine, characterHeight * 0.5f + 0.05f, characterHeight - 0.05f);
            characterWidth = Mathf.Clamp(characterWidth, 0.25f, characterHeight);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            m_CanContinue &= WizardUtility.InspectOutputInfo(serializedObject);

            NeoFpsEditorGUI.Header("Dimensions");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterHeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eyeLine"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterMass"));

            NeoFpsEditorGUI.Header("Audio");

            NeoFpsEditorGUI.AssetField<FpsCharacterAudioData>(serializedObject.FindProperty("characterAudioData"));
            NeoFpsEditorGUI.AssetField<SurfaceAudioData>(serializedObject.FindProperty("softLandingAudio"));
            NeoFpsEditorGUI.AssetField<SurfaceAudioData>(serializedObject.FindProperty("hardLandingAudio"));
            
            NeoFpsEditorGUI.Header("AI");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("addAiSeekerTargets"));
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.DoSummary("Prefab Name", prefabName);
            WizardGUI.DoSummary("Auto Prefix", autoPrefix);
            WizardGUI.DoSummary("Overwrite Existing", overwriteExisting);

            GUILayout.Space(4);

            WizardGUI.DoSummary("characterHeight", characterHeight);
            WizardGUI.DoSummary("characterWidth", characterWidth);
            WizardGUI.DoSummary("characterMass", characterMass);

            GUILayout.Space(4);

            WizardGUI.ObjectSummary("characterAudioData", characterAudioData);
            WizardGUI.ObjectSummary("softLandingAudio", softLandingAudio);
            WizardGUI.ObjectSummary("hardLandingAudio", hardLandingAudio);

            GUILayout.Space(4);
       
            WizardGUI.DoSummary("addAiSeekerTargets", addAiSeekerTargets);
        }
    }
}
