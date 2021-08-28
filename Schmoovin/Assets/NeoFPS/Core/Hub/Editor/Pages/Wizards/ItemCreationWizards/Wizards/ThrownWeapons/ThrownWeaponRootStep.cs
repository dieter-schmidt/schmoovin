using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using UnityEditor.Animations;
using NeoFPS.Constants;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ThrownWeapons
{
    class ThrownWeaponRootStep : WieldableRootStep
    {
        [Tooltip("The name to use for the final prefab.")]
        public string prefabName = "NewMeleeWeapon";
        [Tooltip("Automatically add prefixes such as the pickup type to the prefab name.")]
        public bool autoPrefix = true;
        [Tooltip("Overwrite the existing prefab or generate a unique name and create a new one.")]
        public bool overwriteExisting = true;

        [Tooltip("The pooled projectile object to spawn when the weapon is thrown.")]
        public PooledObject spawnedProjectile = null;
        [Tooltip("Should the projectile inherit velocity from the character throwing it.")]
        public float inheritVelocity = 0.5f;
        [Tooltip("The time after the attack is initiated that the projectile is spawned.")]
        public float spawnTimeLight = 0.5f;
        [Tooltip("The time after the attack is initiated that the projectile is spawned.")]
        public float spawnTimeHeavy = 0.5f;
        [Tooltip("The speed of the projectile once it's spawned.")]
        public float throwSpeedLight = 5f;
        [Tooltip("The speed of the projectile once it's spawned.")]
        public float throwSpeedHeavy = 7.5f;
        [Tooltip("The amount of time it takes to complete a full attack cycle.")]
        public float throwDurationLight = 3f;
        [Tooltip("The amount of time it takes to complete a full attack cycle.")]
        public float throwDurationHeavy = 3f;
        [Tooltip("The crosshair to show with the weapon raised.")]
        public FpsCrosshair crosshair = FpsCrosshair.Default;

        private bool m_CanContinue = false;
        
        public override string displayName
        {
            get { return "Thrown Weapon"; }
        }

        protected override bool canHoldMultiple
        {
            get { return true; }
        }

        protected override FpsSwappableCategory GetDefaultCategory()
        {
            return FpsSwappableCategory.Thrown;
        }

        public bool CheckCanContinueNew(NeoFpsWizard root)
        {
            switch(inventoryType)
            {
                case -1:
                    return false;
                case 0:
                    if (itemKey == FpsInventoryKey.Undefined || slotIndex == -1 || displayImage == null)
                        return false;
                    break;
                case 1:
                    if (displayImage == null)
                        return false;
                    break;
                case 2:
                    if (itemKey == FpsInventoryKey.Undefined || inventoryStackIndex == -1 || displayImage == null)
                        return false;
                    break;
            }

            return spawnedProjectile != null;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            inheritVelocity = Mathf.Clamp(inheritVelocity, 0f, 1f);
            spawnTimeLight = Mathf.Clamp(inheritVelocity, 0f, 10f);
            spawnTimeHeavy = Mathf.Clamp(inheritVelocity, 0f, 10f);
            throwSpeedLight = Mathf.Clamp(inheritVelocity, 0.1f, float.MaxValue);
            throwSpeedHeavy = Mathf.Clamp(inheritVelocity, 0.1f, float.MaxValue);
            throwDurationLight = Mathf.Clamp(inheritVelocity, 0f, 10f);
            throwDurationHeavy = Mathf.Clamp(inheritVelocity, 0f, 10f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            NeoFpsEditorGUI.Header("Output");

            m_CanContinue &= WizardUtility.InspectOutputInfo(serializedObject);

            NeoFpsEditorGUI.Header("Weapon");

            m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabComponentField<PooledObject>(serializedObject.FindProperty("spawnedProjectile"), (po) => { return po.GetComponent<ThrownWeaponProjectile>() != null; });
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inheritVelocity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnTimeLight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnTimeHeavy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("throwSpeedLight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("throwSpeedHeavy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("throwDurationLight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("throwDurationHeavy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("crosshair"));

            NeoFpsEditorGUI.Header("Inventory");

            m_CanContinue &= InspectInventoryOptions(serializedObject);
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.DoSummary("Prefab Name", prefabName);
            WizardGUI.DoSummary("Auto Prefix", autoPrefix);
            WizardGUI.DoSummary("Overwrite Existing", overwriteExisting);

            GUILayout.Space(4);

            WizardGUI.ObjectSummary("Spawned Projectile", spawnedProjectile);
            WizardGUI.DoSummary("Inherit Velocity", inheritVelocity);
            WizardGUI.DoSummary("Spawn Time Light", spawnTimeLight);
            WizardGUI.DoSummary("Spawn Time Heavy", spawnTimeHeavy);
            WizardGUI.DoSummary("Throw Speed Light", throwSpeedLight);
            WizardGUI.DoSummary("Throw Speed Heavy", throwSpeedHeavy);
            WizardGUI.DoSummary("Throw Duration Light", throwDurationLight);
            WizardGUI.DoSummary("Throw Duration Heavy", throwDurationHeavy);
            WizardGUI.MultiChoiceSummary("Crosshair", crosshair, FpsCrosshair.names);

            GUILayout.Space(4);

            SummariseInventoryOptions();
        }
    }
}
