using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor;
using NeoFPSEditor.Hub;
using UnityEngine;
using UnityEditor;
using System;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ModularFirearms
{
    public class ModularFirearmReloadStep : NeoFpsWizardStep
    {
        [SerializeField, Tooltip("How the weapon's ammo is stored. This is the ammo pool that the magazine is re-filled from each time the firearm is reloaded.")]
        private int m_AmmoType = -1;
        [Tooltip("The ammo type to use.")]
        public SharedAmmoType sharedAmmoType = null;
        [Tooltip("The name to show on the HUD.")]
        public string printableName = string.Empty;
        [Delayed, Tooltip("The amount of ammo the weapon starts with.")]
        public int startingAmmo = 100;
        [Delayed, Tooltip("The maximum amount of ammo the weapon can carry.")]
        public int maxAmmo = 100;
        [Tooltip("The ammo quantity available to any reloaders - must be >= to the magazine size.")]
        public int fixedSize = 999;

        [Tooltip("The firearm's magazine. This handles the storage of the ammo actually in the gun, as well as how it is reloaded.")]
        public int m_ReloaderType = -1;

        [Delayed, Tooltip("The number of rounds that can be fit in the magazine at once.")]
        public int magazineSize = 1;
        [Delayed, Tooltip("The number of rounds in the magazine on initialisation.")]
        public int startingMagazine = 1;
        [Tooltip("The time taken to reload.")]
        public float reloadDuration = 2f;
        [Tooltip("The audio clip to play while reloading.")]
        public AudioClip reloadAudio = null;
        [Tooltip("The time in seconds between recharge steps.")]
        public float rechargeSpacing = 3f;
        [Tooltip("The amount of ammo to add each recharge step.")]
        public int rechargeAmount = 100;
        [Tooltip("Should the recharge timer reset each time the ammo quantity changes. If not, the recharge timer will start as soon as the ammo dips below max and stop when it is filled to maximum again.")]
        public bool resetOnChange = false;

        [Tooltip("The time taken to reload if reloading from empty.")]
        public float reloadDurationEmpty = 3f;
        [Tooltip("The audio clip to play if reloading from empty.")]
        public AudioClip reloadAudioEmpty = null;

        [Tooltip("The number of rounds to load into the gun each increment.")]
        public int roundsPerIncrement = 1;
        [Tooltip("The time from starting the reload animations to adding the first shell.")]
        public float reloadStartDuration = 1f;
        [Tooltip("The time between shells in the reload animation.")]
        public float reloadIncrementDuration = 1f;
        [Tooltip("The time from the last shell being added to the reload animation completing.")]
        public float reloadEndDuration = 1f;
        [Tooltip("The audio clip to play when the reload starts.")]
        public AudioClip reloadAudioStart = null;
        [Tooltip("The audio clip to play during an increment of the reload.")]
        public AudioClip reloadAudioIncrement = null;
        [Tooltip("The audio clip to play when the reload ends.")]
        public AudioClip reloadAudioEnd = null;

        [Tooltip("The ReloaderCountdown behaviour blends in sound effects as the magazine approaches empty. This can be used to communicate when running out of ammo, or can be used for effects such as the last round ping of an M1 Garand.")]
        public bool countDownLastRounds = false;

        [Tooltip("The audio clips to play as ammo is consumed. First element is for the last round in the magazine, second = penultimate, and so on.")]
        public CountdownAudio[] countdownAudio = new CountdownAudio[0];
        [Tooltip("If set, then the last clip will be used for all ammo until the count is within the sequence.")]
        public bool extendSequence = false;

        [Serializable]
        public struct CountdownAudio
        {
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
        }

        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "Ammo Type & Reload Setup"; }
        }
        
        static readonly string[] ammoTypeOptions =
        {
            "Inventory based. Ammo is taken from the character's inventory when reloading. This allows you to share ammo across multiple weapons.",
            "Custom ammo. The ammo is unique to this specific weapon. You could pick up another instance of the same weapon, and its ammo would have a different count.",
            "Infinite. The weapon's ammo pool is never depleted.",
            "Recharging. Ammo is unique to this weapon and recharges over time."
        };

        static readonly string[] ammoTypeSummaries =
        {
            "Inventory based",
            "Custom ammo",
            "Infinite",
            "Recharging"
        };

        static readonly string[] reloaderTypeOptions =
        {
            "Simple. The reloader triggers a reload animation, and after a set period, the magazine is re-filled.",
            "Chambered. The reloader has a separate reload animation if there is no round currently chambered.",
            "Incremental. The rounds are reloaded into the magazine in increments of one or more. This process can be interrupted by firing.",
            "Passthrough. When the firearm fires, the round is taken directly from the weapon's ammo pool, instead of a magazine. This means you never need to hit the reload button."
        };

        static readonly string[] reloaderTypeSummaries =
        {
            "Inventory based",
            "Custom ammo",
            "Infinite"
        };

        public enum AmmoModule
        {
            Undefined,
            Inventory,
            Custom,
            Infinite,
            Recharging
        }

        public enum ReloaderModule
        {
            Undefined,
            Simple,
            Chambered,
            Incremental,
            Passthrough
        }

        public AmmoModule ammoModule
        {
            get { return (AmmoModule)(m_AmmoType + 1); }
        }

        public ReloaderModule reloaderModule
        {
            get { return (ReloaderModule)(m_ReloaderType + 1); }
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = m_AmmoType != -1 && m_ReloaderType != -1;
            if (ammoModule == AmmoModule.Inventory)
                m_CanContinue &= sharedAmmoType != null;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            if (fixedSize < 1)
                fixedSize = 1;
            if (maxAmmo < 0)
                maxAmmo = 0;
            startingAmmo = Mathf.Clamp(startingAmmo, 0, maxAmmo);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("m_AmmoType"), ammoTypeOptions);

            switch (ammoModule)
            {
                case AmmoModule.Inventory:
                    m_CanContinue &= NeoFpsEditorGUI.RequiredAssetField<SharedAmmoType>(serializedObject.FindProperty("sharedAmmoType"));
                    break;
                case AmmoModule.Custom:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("printableName"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startingAmmo"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAmmo"));
                    break;
                case AmmoModule.Infinite:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("printableName"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fixedSize"));
                    break;
                case AmmoModule.Recharging:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("printableName"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startingAmmo"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAmmo"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rechargeSpacing"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rechargeAmount"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("resetOnChange"));
                    break;
            }

            EditorGUILayout.Space();

            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("m_ReloaderType"), reloaderTypeOptions);

            switch (reloaderModule)
            {
                case ReloaderModule.Simple:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startingMagazine"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadDuration"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadAudio"));
                    break;
                case ReloaderModule.Chambered:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startingMagazine"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadDuration"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadDurationEmpty"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadAudio"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadAudioEmpty"));
                    break;
                case ReloaderModule.Incremental:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startingMagazine"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("roundsPerIncrement"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadStartDuration"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadIncrementDuration"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadEndDuration"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadAudioStart"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadAudioIncrement"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadAudioEnd"));
                    break;
                case ReloaderModule.Passthrough:
                    break;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("countDownLastRounds"));
            if (countDownLastRounds)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("countdownAudio"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("extendSequence"));
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.MultiChoiceSummary("m_AmmoType", m_AmmoType, ammoTypeSummaries);

            switch (ammoModule)
            {
                case AmmoModule.Inventory:
                    WizardGUI.ObjectSummary("sharedAmmoType", sharedAmmoType);
                    break;
                case AmmoModule.Custom:
                    WizardGUI.DoSummary("printableName", printableName);
                    WizardGUI.DoSummary("startingAmmo", startingAmmo);
                    WizardGUI.DoSummary("maxAmmo", maxAmmo);
                    break;
                case AmmoModule.Infinite:
                    WizardGUI.DoSummary("printableName", printableName);
                    WizardGUI.DoSummary("fixedSize", fixedSize);
                    break;
                case AmmoModule.Recharging:
                    WizardGUI.DoSummary("printableName", printableName);
                    WizardGUI.DoSummary("startingAmmo", startingAmmo);
                    WizardGUI.DoSummary("maxAmmo", maxAmmo);
                    WizardGUI.DoSummary("rechargeSpacing", rechargeSpacing);
                    WizardGUI.DoSummary("rechargeAmount", rechargeAmount);
                    WizardGUI.DoSummary("resetOnChange", resetOnChange);
                    break;
            }

            GUILayout.Space(4);

            WizardGUI.MultiChoiceSummary("m_ReloaderType", m_ReloaderType, reloaderTypeSummaries);

            switch (reloaderModule)
            {
                case ReloaderModule.Simple:
                    WizardGUI.DoSummary("magazineSize", magazineSize);
                    WizardGUI.DoSummary("startingMagazine", startingMagazine);
                    WizardGUI.DoSummary("reloadDuration", reloadDuration);
                    WizardGUI.DoSummary("reloadAudio", reloadAudio);
                    break;
                case ReloaderModule.Chambered:
                    WizardGUI.DoSummary("magazineSize", magazineSize);
                    WizardGUI.DoSummary("startingMagazine", startingMagazine);
                    WizardGUI.DoSummary("reloadDuration", reloadDuration);
                    WizardGUI.DoSummary("reloadDurationEmpty", reloadDurationEmpty);
                    WizardGUI.DoSummary("reloadAudio", reloadAudio);
                    WizardGUI.DoSummary("reloadAudioEmpty", reloadAudioEmpty);
                    break;
                case ReloaderModule.Incremental:
                    WizardGUI.DoSummary("magazineSize", magazineSize);
                    WizardGUI.DoSummary("startingMagazine", startingMagazine);
                    WizardGUI.DoSummary("roundsPerIncrement", roundsPerIncrement);
                    WizardGUI.DoSummary("reloadStartDuration", reloadStartDuration);
                    WizardGUI.DoSummary("reloadIncrementDuration", reloadIncrementDuration);
                    WizardGUI.DoSummary("reloadEndDuration", reloadEndDuration);
                    WizardGUI.DoSummary("reloadAudioStart", reloadAudioStart);
                    WizardGUI.DoSummary("reloadAudioIncrement", reloadAudioIncrement);
                    WizardGUI.DoSummary("reloadAudioEnd", reloadAudioEnd);
                    break;
                case ReloaderModule.Passthrough:
                    break;
            }

            GUILayout.Space(4);

            WizardGUI.DoSummary("countDownLastRounds", countDownLastRounds);
            if (countDownLastRounds)
            {
                WizardGUI.DoSummary("countdownAudio", "...");
                WizardGUI.DoSummary("extendSequence", extendSequence);
            }
        }
    }
}
