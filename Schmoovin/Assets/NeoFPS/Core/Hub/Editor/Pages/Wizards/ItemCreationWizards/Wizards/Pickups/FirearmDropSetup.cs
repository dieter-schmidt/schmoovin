using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.Pickups
{
    public class FirearmDropSetup : NeoFpsWizardStep
    {
        [Tooltip("The firearm prefab to add to the character inventory on pickup")]
        public GameObject firearmPrefab = null;
        [Tooltip("Modify the inventory item component on the gun prefab to point at this drop")]
        public bool addToGun = false;
        [Tooltip("The ammo type the firearm uses")]
        public SharedAmmoType ammoType = null;

        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "Modular Firearm Drop Setup"; }
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = firearmPrefab != null && ammoType != null;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabField(serializedObject.FindProperty("firearmPrefab"), (obj) => { return obj.GetComponent<IModularFirearm>() != null; });
            EditorGUILayout.PropertyField(serializedObject.FindProperty("addToGun"));
            m_CanContinue &= NeoFpsEditorGUI.RequiredAssetField<SharedAmmoType>(serializedObject.FindProperty("ammoType"));
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.ObjectSummary("Firearm Prefab", firearmPrefab);
            WizardGUI.DoSummary("Add To Gun", addToGun);
            WizardGUI.ObjectSummary("Ammo Type", ammoType);
        }

        public override string[] GetNextSteps()
        {
            return null;
        }

        static bool FilterFirearm(GameObject obj)
        {
            return obj != null && obj.GetComponent<IModularFirearm>() != null && obj.GetComponent<FpsInventoryWieldable>() != null;
        }
    }
}