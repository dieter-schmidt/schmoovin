using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.Pickups
{
    public class PickupWizardRootStep : NeoFpsWizardStep
    {
        [Tooltip("The name to use for the final prefab.")]
        public string prefabName = "MyPickup";
        [Tooltip("Automatically add prefixes such as the pickup type to the prefab name.")]
        public bool autoPrefix = true;
        [Tooltip("Overwrite the existing prefab or generate a unique name and create a new one.")]
        public bool overwriteExisting = true;
        [Tooltip("The type of pickup to create.")]
        public int pickupType = -1;
        [SerializeField, Tooltip("How to interact with the pickup.")]
        private int m_InteractionType = 0;
        [Tooltip("Should you tap or hold the use button to interact with the pickup.")]
        public int tapOrHold = 1;
        [Tooltip("The amount of time the player must hold the use button to interact with the pickup.")]
        public float holdDuration = 0.5f;

        static readonly string[] k_FirearmStep = new string[] { PickupWizardSteps.firearm, PickupWizardSteps.audioVisualFirearm };
        static readonly string[] k_WeaponStep = new string[] { PickupWizardSteps.wieldable, PickupWizardSteps.audioVisual };
        static readonly string[] k_InventoryStep = new string[] { PickupWizardSteps.inventoryItem, PickupWizardSteps.audioVisual };
        static readonly string[] k_MultiInventoryStep = new string[] { PickupWizardSteps.inventoryMultiItem, PickupWizardSteps.audioVisual };
        static readonly string[] k_KeyRingStep = new string[] { PickupWizardSteps.keyRing, PickupWizardSteps.audioVisual };
        static readonly string[] k_HealthStep = new string[] { PickupWizardSteps.health, PickupWizardSteps.audioVisual };
        static readonly string[] k_ShieldStep = new string[] { PickupWizardSteps.shield, PickupWizardSteps.audioVisual };

        private bool m_CanContinue = false;

        public int interactionType
        {
            get
            {
                if (pickupType == 2 || pickupType > 3)
                    return m_InteractionType;
                else
                    return 0;
            }
        }

        static readonly string[] pickupTypeOptions =
        {
            "Wieldable item drop. This is used for items like melee and thrown weapons that are held and used. It is basically an inventory item pickup that can also be dropped again.",
            "Modular firearm drop / pickup. This is a specialised version of the wieldable drops that also tracks the ammo count in the firearm and allows it to be picked up separately to the weapon.",
            "Inventory item pickup. This can include multiple of the same item.",
            "Multi-item inventory pickup. This is used for pickups such as ammo crates.",
            "Health pack. This restores health to a character's health manager.",
            "Shield booster. This allows you to recharge individual steps of your energy shield.",
            "Key ring. This contains one or more keys to open locks."
        };

        static readonly string[] pickupTypeSummaries =
        {
            "Wieldable item drop",
            "Modular firearm drop / pickup",
            "Inventory item pickup",
            "Multi-item inventory pickup",
            "Health pack",
            "Shield booster",
            "Key ring"
        };

        static readonly string[] interactionOptions =
        {
            "Interactive object. This requires the player to look at the pickup and hit \"Use\".",
            "Contact based. This requires the player to walk over the pickup."
        };

        static readonly string[] interactionSummaries =
        {
            "Interactive object",
            "Contact based"
        };

        static readonly string[] tapOrHoldOptions =
        {
            "Instant. The pickup will be consumed as soon as the use button is pressed.",
            "Hold. The player must hold use for a set amount of time to consume the item."
        };

        static readonly string[] tapOrHoldSummaries =
        {
            "Instant",
            "Hold"
        };

        public override string displayName
        {
            get { return "Pickup Options"; }
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = !string.IsNullOrEmpty(prefabName) && pickupType != -1;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            holdDuration = Mathf.Clamp(holdDuration, 0f, 10f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            NeoFpsEditorGUI.Header("Output");

            m_CanContinue &= WizardUtility.InspectOutputInfo(serializedObject);

            NeoFpsEditorGUI.Header("Options");
            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("pickupType"), pickupTypeOptions);

            bool showInteractionType = (pickupType == 2 || pickupType > 3);
            if (showInteractionType)
                NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("m_InteractionType"), interactionOptions);

            if (!showInteractionType || interactionType == 0)
            {
                NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("tapOrHold"), tapOrHoldOptions);
                if (tapOrHold == 1)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("holdDuration"));
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.DoSummary("Prefab Name", prefabName);
            WizardGUI.DoSummary("Auto Prefix", autoPrefix);
            WizardGUI.DoSummary("Overwrite Existing", overwriteExisting);

            EditorGUILayout.Space();

            WizardGUI.MultiChoiceSummary("Pickup Type", pickupType, pickupTypeSummaries);

            bool showInteractionType = (pickupType == 2 || pickupType > 3);
            if (showInteractionType)
                WizardGUI.MultiChoiceSummary("Interaction Type", interactionType, interactionSummaries);

            if (!showInteractionType || interactionType == 0)
            {
                WizardGUI.MultiChoiceSummary("Tap Or Hold", tapOrHold, tapOrHoldOptions);
                if (tapOrHold == 1)
                    WizardGUI.DoSummary("Hold Duration", holdDuration);
            }
        }

        public override string[] GetNextSteps()
        {
            switch (pickupType)
            {
                case 0: // Wieldable drop
                    return k_WeaponStep;
                case 1: // Modular firearm drop
                    return k_FirearmStep;
                case 2: // Inventory item pickup
                    return k_InventoryStep;
                case 3: // Multi-iten inventory pickup
                    return k_MultiInventoryStep;
                case 4: // Health pack
                    return k_HealthStep;
                case 5: // Shield booster
                    return k_ShieldStep;
                case 6: // Key ring
                    return k_KeyRingStep;
                default:
                    return null;
            }
        }
    }
}