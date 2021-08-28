using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;
using NeoFPS.Constants;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.Pickups
{
    public class AudioVisualFirearmSetup : AudioVisualSetup
    {
        [Tooltip("The ammo mesh will be disabled if all the ammo in the firearm is picked up.")]
        public MeshRenderer ammoMesh = null;
        [Tooltip("The audio clip to play on removing the ammo from the pickup.")]
        public AudioClip ammoAudio = null;

        protected override void DoLayoutRenderOptions(SerializedObject serializedObject, int pickupType, int interactionType)
        {
            base.DoLayoutRenderOptions(serializedObject, pickupType, interactionType);
            switch (displayObjectType)
            {
                case 1: // Model
                    NeoFpsEditorGUI.ComponentInHierarchyField<MeshRenderer>(serializedObject.FindProperty("ammoMesh"), (modelPrefab != null) ? modelPrefab.transform : null, false);
                    break;
                case 2: // GameObject
                    NeoFpsEditorGUI.ComponentInHierarchyField<MeshRenderer>(serializedObject.FindProperty("ammoMesh"), (displayPrefab != null) ? displayPrefab.transform : null, false);
                    break;
            }
        }

        protected override void DoLayoutAudioOptions(SerializedObject serializedObject, int pickupType, int interactionType)
        {
            base.DoLayoutAudioOptions(serializedObject, pickupType, interactionType);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoAudio"));
        }

        protected override void RenderSummary(int pickupType, int interactionType)
        {
            base.RenderSummary(pickupType, interactionType);
            if (displayObjectType > 0)
                WizardGUI.ObjectSummary("Ammo Mesh", ammoMesh);
        }

        protected override void AudioSummary(int pickupType, int interactionType)
        {
            base.AudioSummary(pickupType, interactionType);
            WizardGUI.ObjectSummary("Ammo Audio", ammoAudio);
        }
    }
}