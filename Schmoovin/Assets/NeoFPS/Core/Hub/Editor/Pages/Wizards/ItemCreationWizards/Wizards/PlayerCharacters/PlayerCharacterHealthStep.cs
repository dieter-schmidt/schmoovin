using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;
using NeoFPS.Constants;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.PlayerCharacter
{
    class PlayerCharacterHealthStep : NeoFpsWizardStep
    {
        [Tooltip("Use the NeoFPS health systems, or a custom solution.")]
        public bool useNeoHealthSystems = true;

        [Delayed, Tooltip("The starting health of the character.")]
        public float health = 100f;
        [Delayed, Tooltip("The maximum health of the character.")]
        public float maxHealth = 100f;
        [Tooltip("Does the character health recharge after a brief delay.")]
        public bool healthRecharges = false;
        [Tooltip("The recharge speed for health regeneration.")]
        public float rechargeRate = 5f;
        [Tooltip("The delay between taking damage and starting health regen.")]
        public float rechargeDelay = 5f;
        [Delayed, Tooltip("Health recharge will be interrupted if damage greater than this amount is received.")]
        public float interruptDamage = 1f;
        [Tooltip("Does the character take damage from landing after long falls.")]
        public bool applyFallDamage = true;

        [Tooltip("The type of damage handler to use.")]
        public DamageHandlerType headDamageHandler = DamageHandlerType.Basic;
        [Tooltip("The radius of the head sphere.")]
        public float headRadius = 0.2f;
        [Tooltip("A multiplier applied to head damage.")]
        public float headDamageMultiplier = 2f;
        [Tooltip("Does unshielded / armoured damage to the head count as a critical hit.")]
        public bool headShotsAreCritical = true;
        [Tooltip("The inventory key of the armour type")]
        public FpsInventoryKey headInventoryKey = FpsInventoryKey.ArmourHelmet;
        [Range(0f, 1f), Tooltip("The amount of damage the armour should nullify")]
        public float headArmourMitigation = 0.75f;
        [Tooltip("A multiplier used to modify how much armour is destroyed by the incoming damage.")]
        public float headArmourMultiplier = 0.5f;

        [Tooltip("The type of damage handler to use.")]
        public DamageHandlerType bodyDamageHandler = DamageHandlerType.Basic;
        [Tooltip("The radius of the body capsule.")]
        public float bodyRadius = 0.35f;
        [Tooltip("A multiplier applied to body damage.")]
        public float bodyDamageMultiplier = 1f;
        [Tooltip("The inventory key of the armour type")]
        public FpsInventoryKey bodyInventoryKey = FpsInventoryKey.ArmourBody;
        [Range(0f, 1f), Tooltip("The amount of damage the armour should nullify")]
        public float bodyArmourMitigation = 0.75f;
        [Tooltip("A multiplier used to modify how much armour is destroyed by the incoming damage.")]
        public float bodyArmourMultiplier = 0.5f;

        [Tooltip("The type of damage handler to use.")]
        public DamageHandlerType legsDamageHandler = DamageHandlerType.Basic;
        [Tooltip("The radius of the legs capsule.")]
        public float legsRadius = 0.25f;
        [Tooltip("A multiplier applied to leg damage.")]
        public float legsDamageMultiplier = 0.75f;
        [Tooltip("The inventory key of the armour type")]
        public FpsInventoryKey legsInventoryKey = FpsInventoryKey.ArmourBody;
        [Range(0f, 1f), Tooltip("The amount of damage the armour should nullify")]
        public float legsArmourMitigation = 0.75f;
        [Tooltip("A multiplier used to modify how much armour is destroyed by the incoming damage.")]
        public float legsArmourMultiplier = 0.5f;
        
        [Tooltip("The starting shield amount.")]
        public float shield = 100f;
        [Tooltip("The shield capacity of each shield step / block.")]
        public float stepCapacity = 100f;
        [Tooltip("The number of shield steps / blocks.")]
        public int stepCount = 1;
        [Range(0f, 1f), Tooltip("The amount of damage (multiplier) that the shield negates.")]
        public float shieldMitigation = 1f;
        [Tooltip("The recharge speed for shield regeneration.")]
        public float shieldChargeRate = 5f;
        [Tooltip("The delay between taking damage and starting shield regen.")]
        public float shieldChargeDelay = 5f;
        [Tooltip("Shield steps only recharge if the shield value is greater than their starting level. If this property is false, step 1 will always recharge, even if it hits zero.")]
        public bool canBreakStep1 = true;

        public bool useShieldSystem
        {
            get
            {
                return headDamageHandler == DamageHandlerType.Shielded || headDamageHandler == DamageHandlerType.ArmouredAndShielded ||
                    bodyDamageHandler == DamageHandlerType.Shielded || bodyDamageHandler == DamageHandlerType.ArmouredAndShielded ||
                    legsDamageHandler == DamageHandlerType.Shielded || legsDamageHandler == DamageHandlerType.ArmouredAndShielded;
            } 
        }

        public enum DamageHandlerType
        {
            Basic,
            Armoured,
            Shielded,
            ArmouredAndShielded
        }
        
        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "Health & Damage"; }
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            maxHealth = Mathf.Clamp(maxHealth, 1f, 10000f);
            health = Mathf.Clamp(health, 1f, maxHealth);
            rechargeRate = Mathf.Clamp(rechargeRate, 0f, 1000f);
            rechargeDelay = Mathf.Clamp(rechargeDelay, 0f, 300f);
            interruptDamage = Mathf.Clamp(interruptDamage, 0f, maxHealth - 1f);

            shield = Mathf.Clamp(shield, 1f, 10000f);
            stepCapacity = Mathf.Clamp(stepCapacity, 1f, 10000f);
            stepCount = Mathf.Clamp(stepCount, 1, 99);
            shieldChargeRate = Mathf.Clamp(shieldChargeRate, 0f, 1000f);
            shieldChargeDelay = Mathf.Clamp(shieldChargeDelay, 0f, 300f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("useNeoHealthSystems"));
            if (useNeoHealthSystems)
            {
                NeoFpsEditorGUI.Header("Health");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("healthRecharges"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("applyFallDamage"));
                if (healthRecharges)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rechargeRate"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rechargeDelay"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("interruptDamage"));
                }

                NeoFpsEditorGUI.Header("Head Damage");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("headDamageHandler"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("headRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("headDamageMultiplier"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("headShotsAreCritical"));
                if (headDamageHandler == DamageHandlerType.Armoured || headDamageHandler == DamageHandlerType.ArmouredAndShielded)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("headInventoryKey"));
                    if (headInventoryKey == FpsInventoryKey.Undefined)
                        m_CanContinue = false;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("headArmourMitigation"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("headArmourMultiplier"));
                }

                NeoFpsEditorGUI.Header("Body Damage");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyDamageHandler"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyDamageMultiplier"));
                if (bodyDamageHandler == DamageHandlerType.Armoured || bodyDamageHandler == DamageHandlerType.ArmouredAndShielded)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyInventoryKey"));
                    if (bodyInventoryKey == FpsInventoryKey.Undefined)
                        m_CanContinue = false;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyArmourMitigation"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyArmourMultiplier"));
                }

                NeoFpsEditorGUI.Header("Legs Damage");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("legsDamageHandler"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("legsRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("legsDamageMultiplier"));
                if (legsDamageHandler == DamageHandlerType.Armoured || legsDamageHandler == DamageHandlerType.ArmouredAndShielded)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("legsInventoryKey"));
                    if (legsInventoryKey == FpsInventoryKey.Undefined)
                        m_CanContinue = false;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("legsArmourMitigation"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("legsArmourMultiplier"));
                }

                if (useShieldSystem)
                {
                    NeoFpsEditorGUI.Header("Shields");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shield"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stepCapacity"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stepCount"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldMitigation"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldChargeRate"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldChargeDelay"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("canBreakStep1"));
                }
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.DoSummary("useNeoHealthSystems", useNeoHealthSystems);
            if (useNeoHealthSystems)
            {
                GUILayout.Space(4);

                WizardGUI.DoSummary("health", health);
                WizardGUI.DoSummary("maxHealth", maxHealth);
                WizardGUI.DoSummary("healthRecharges", healthRecharges);
                WizardGUI.DoSummary("applyFallDamage", applyFallDamage);
                if (healthRecharges)
                {
                    WizardGUI.DoSummary("rechargeRate", rechargeRate);
                    WizardGUI.DoSummary("rechargeDelay", rechargeDelay);
                    WizardGUI.DoSummary("interruptDamage", interruptDamage);
                }

                GUILayout.Space(4);

                WizardGUI.DoSummary("headDamageHandler", headDamageHandler.ToString());
                WizardGUI.DoSummary("headRadius", headRadius);
                WizardGUI.DoSummary("headDamageMultiplier", headDamageMultiplier);
                WizardGUI.DoSummary("headShotsAreCritical", headShotsAreCritical);
                if (headDamageHandler == DamageHandlerType.Armoured || headDamageHandler == DamageHandlerType.ArmouredAndShielded)
                {
                    WizardGUI.DoSummary("headInventoryKey", headInventoryKey.ToString());
                    WizardGUI.DoSummary("headArmourMitigation", headArmourMitigation);
                    WizardGUI.DoSummary("headArmourMultiplier", headArmourMultiplier);
                }

                GUILayout.Space(4);

                WizardGUI.DoSummary("bodyDamageHandler", bodyDamageHandler.ToString());
                WizardGUI.DoSummary("bodyRadius", bodyRadius);
                WizardGUI.DoSummary("bodyDamageMultiplier", bodyDamageMultiplier);
                if (bodyDamageHandler == DamageHandlerType.Armoured || bodyDamageHandler == DamageHandlerType.ArmouredAndShielded)
                {
                    WizardGUI.DoSummary("bodyInventoryKey", bodyInventoryKey.ToString());
                    WizardGUI.DoSummary("bodyArmourMitigation", bodyArmourMitigation);
                    WizardGUI.DoSummary("bodyArmourMultiplier", bodyArmourMultiplier);
                }

                GUILayout.Space(4);

                WizardGUI.DoSummary("legsDamageHandler", legsDamageHandler.ToString());
                WizardGUI.DoSummary("legsRadius", legsRadius);
                WizardGUI.DoSummary("legsDamageMultiplier", legsDamageMultiplier);
                if (legsDamageHandler == DamageHandlerType.Armoured || legsDamageHandler == DamageHandlerType.ArmouredAndShielded)
                {
                    WizardGUI.DoSummary("legsInventoryKey", legsInventoryKey.ToString());
                    WizardGUI.DoSummary("legsArmourMitigation", legsArmourMitigation);
                    WizardGUI.DoSummary("legsArmourMultiplier", legsArmourMultiplier);
                }
            }
        }
    }
}
