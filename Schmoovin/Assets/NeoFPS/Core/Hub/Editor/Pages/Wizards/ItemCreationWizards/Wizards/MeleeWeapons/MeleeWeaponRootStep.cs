using UnityEngine;
using UnityEditor;
using NeoFPS.Constants;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.MeleeWeapons
{
    class MeleeWeaponRootStep : WieldableRootStep
    {
        [Tooltip("The name to use for the final prefab.")]
        public string prefabName = "NewMeleeWeapon";
        [Tooltip("Automatically add prefixes such as the pickup type to the prefab name.")]
        public bool autoPrefix = true;
        [Tooltip("Overwrite the existing prefab or generate a unique name and create a new one.")]
        public bool overwriteExisting = true;

        [Tooltip("The damage to apply when an attack hits.")]
        public float damage = 10f;
        [Tooltip("The force to apply to the hit object.")]
        public float force = 10f;
        [Tooltip("The raycast distance for the strike.")]
        public float range = 1f;
        [Tooltip("The time between starting the attack animation and damage being applied.")]
        public float attackDelay = 0.25f;
        [Tooltip("The recovery time after a hit.")]
        public float attackRecover = 0.5f;
        [Tooltip("The crosshair to show with the weapon raised.")]
        public FpsCrosshair crosshair = FpsCrosshair.Default;
        
        public override string displayName
        {
            get { return "Melee Weapon"; }
        }

        protected override bool canHoldMultiple
        {
            get { return false; }
        }

        protected override FpsSwappableCategory GetDefaultCategory()
        {
            return FpsSwappableCategory.Melee;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return inventoryType != -1 && itemKey != 0;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            damage = Mathf.Clamp(damage, 0f, float.MaxValue);
            force = Mathf.Clamp(force, 0f, 1000f);
            range = Mathf.Clamp(range, 0.1f, 10f);
            attackDelay = Mathf.Clamp(attackDelay, 0f, 10f);
            attackRecover = Mathf.Clamp(attackRecover, 0.1f, 10f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            NeoFpsEditorGUI.Header("Output");

            bool valid = WizardUtility.InspectOutputInfo(serializedObject);

            NeoFpsEditorGUI.Header("Weapon");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("force"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("range"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackDelay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRecover"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshair"));

            NeoFpsEditorGUI.Header("Inventory");

            InspectInventoryOptions(serializedObject);
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.DoSummary("Prefab Name", prefabName);
            WizardGUI.DoSummary("Auto Prefix", autoPrefix);
            WizardGUI.DoSummary("Overwrite Existing", overwriteExisting);

            GUILayout.Space(4);

            WizardGUI.DoSummary("Damage", damage);
            WizardGUI.DoSummary("Force", force);
            WizardGUI.DoSummary("Range", range);
            WizardGUI.DoSummary("Attack Delay", attackDelay);
            WizardGUI.DoSummary("Attack Recover", attackRecover);
            WizardGUI.MultiChoiceSummary("Crosshair", crosshair, FpsCrosshair.names);

            GUILayout.Space(4);

            SummariseInventoryOptions();
        }
    }
}
