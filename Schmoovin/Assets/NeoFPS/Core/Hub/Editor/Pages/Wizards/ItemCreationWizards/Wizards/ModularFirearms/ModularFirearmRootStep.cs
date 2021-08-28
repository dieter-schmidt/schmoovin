using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor;
using NeoFPSEditor.Hub;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using NeoFPS.Constants;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public class ModularFirearmRootStep : WieldableRootStep
    {
        [Tooltip("The name to use for the final prefab.")]
        public string prefabName = "NewFirearm";
        [Tooltip("Automatically add prefixes such as the pickup type to the prefab name.")]
        public bool autoPrefix = true;
        [Tooltip("Overwrite the existing prefab or generate a unique name and create a new one.")]
        public bool overwriteExisting = true;

        [Tooltip("The model to use for the view model of the weapon.")]
        public GameObject viewModel = null;
        [Tooltip("Expand the valid view model prefab type to any prefab.")]
        public bool allowNonModelPrefabs = false;
        [Tooltip("The actual weapon object inside the view model hierarchy. This is used to place transforms for muzzle tip, etc if they don't already exist.")]
        public GameObject weaponObject = null;
        [Tooltip("The animator controller to assign to the animator on the weapon view model.")]
        public AnimatorController animationController = null;
        [Tooltip("Should a basic animation event handler be added to the firearm view-model?")]
        public bool usesAnimationEvents = false;

        [Tooltip("An optional camera object in the view model hierarchy. Used to align to the NeoFPS camera, and apply camera animations.")]
        public GameObject cameraObject = null;
        [Tooltip("If the view model camera is animated, then setting this to true will apply the components required to apply this animation using the NeoFPS additive transform system.")]
        public bool cameraIsAnimated = false;
        
        [Tooltip("Does sprinting affect the firearm, and how is that animated.")]
        public int sprintingStyle = 0;
        [Delayed, SerializeField, Tooltip("The movement speed at which the procedural sprint animation is at full strength.")]
        public float fullStrengthSpeed = 12f;
        [Delayed, SerializeField, Tooltip("A maximum speed clamp for the character when used to calculate the animation speed multiplier.")]
        public float maxSpeed = 15f;
        [Delayed, SerializeField, Tooltip("The speed below which the light sprint animation will be 100% used. Above this, the heavy animation is blended in.")]
        public float blendZeroSpeed = 5f;
        [Delayed, SerializeField, Tooltip("The speed above which the he\vy sprint animation will be 100% used. Below this, the light animation is blended in.")]
        public float blendFullSpeed = 10f;
        [Tooltip("What to do when the firearms enters / exits ADS. You can pause the animation while aiming, or block sprinting entirely.")]
        public SprintInterruptAction sprintActionOnAim = SprintInterruptAction.StopAnimation;
        [Tooltip("What to do when the firearm is reloaded. You can pause the animation while reloading, or block sprinting entirely.")]
        public SprintInterruptAction sprintActionOnReload = SprintInterruptAction.StopAnimation;
        [Tooltip("What to do when the firearm trigger is pulled while sprinting. You can pause the animation while, or stop sprinting until the trigger is released (both of these have a slight delay before firing to allow the weapon to be aligned). You can also block the firearm from firing at all while sprinting.")]
        public SprintFireAction sprintActionOnFire = SprintFireAction.StopAnimation;
        [Tooltip("The minimum amount of time the firearm sprint animation will be paused or sprinting blocked when the trigger is pulled. Prevents rapid tapping of the trigger popping in and out of sprint")]
        public float minSprintInterrupt = 0.5f;

        [Tooltip("Should aiming down sights drain the player character's stamina. Requires the wielder to have a StaminaSystem component attached.")]
        public bool aimingFatigue = false;
        [Tooltip("The stamina loss per second when aiming down sights.")]
        public float staminaLoss = 10f;
        [Tooltip("The stamina level to drain down to.")]
        public float staminaTarget = 25f;
        [Tooltip("Stamina drain fades when approaching the target, starting at this falloff value above it.")]
        public float staminaFalloff = 25f;

        [Tooltip("Should the firearm overheat when fired continuously.")]
        public bool overheat = false;
        [Tooltip("The mesh renderer in the weapon hierarchy that glows as the weapon heats up.")]
        public MeshRenderer glowRenderer = null;
        [Tooltip("The material to use on the glow renderer (must use a shader with the _Glow property, such as the NeoFPS/Standard/GlowMetallic or GlowSpecular shaders).")]
        public Material glowMaterial = null;
        [Tooltip("The heat level required before the weapon starts to glow.")]
        public float glowThreshold = 0.25f;
        [Tooltip("The mesh renderer of the glow material.")]
        public MeshRenderer hazePrefab = null;
        [Tooltip("The heat level required before the weapon starts to glow.")]
        public float hazeThreshold = 0.1f;
        [Tooltip("The amount of heat to add with each shot of the weapon. When this reaches 1, the gun must cool down before it can fire again.")]
        public float heatPerShot = 0.02f;
        [Tooltip("The amount of heat that is dissipated per second. The weapon will never overheat if this is higher than the heat per shot multiplied by rate of fire (rounds per second).")]
        public float heatLostPerSecond = 0.2f;
        [Range(0f, 1f), Tooltip("An event that is fired when the heat hits the max level.")]
        public float damping = 0.5f;
        [Tooltip("If true, then once the weapon reaches max heat the weapon will overheat, blocking the trigger until it has cooled down to a set threshold.")]
        public bool doOverheat = true;
        [Tooltip("Once overheated, the weapon must cool to this heat level before it can fire again.")]
        public float coolingThreshold = 0.25f;
        [Tooltip("The audio clip to play once max heat is hit and the trigger is blocked.")]
        public AudioClip overheatSound = null;

        private bool m_CanContinue = false;
        
        static readonly string[] sprintingOptions =
        {
            "No Sprinting Animation. The firearm should not be affected by sprinting.",
            "Procedural Sprinting. Sprinting uses the additive transform and stance systems with a spring and curve based animation.",
            "Keyframed Sprinting. Sprinting is tied into parameters on the firearm's animator component."
        };

        static readonly string[] sprintingSummaries =
        {
            "No Sprinting Animation",
            "Procedural Sprinting",
            "Keyframed Sprinting"
        };

        public override string displayName
        {
            get { return "Modular Firearm"; }
        }

        protected override bool canHoldMultiple
        {
            get { return false; }
        }

        protected override FpsSwappableCategory GetDefaultCategory()
        {
            return FpsSwappableCategory.Firearm;
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = !string.IsNullOrWhiteSpace(prefabName);
            m_CanContinue &= viewModel != null && weaponObject != null;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            maxSpeed = Mathf.Clamp(maxSpeed, 1f, 50f);
            blendZeroSpeed = Mathf.Clamp(blendZeroSpeed, 1f, blendFullSpeed);
            blendFullSpeed = Mathf.Clamp(blendFullSpeed, blendZeroSpeed, maxSpeed);
            minSprintInterrupt = Mathf.Clamp(minSprintInterrupt, 0f, 5f);
            staminaLoss = Mathf.Clamp(staminaLoss, 0f, 999f);
            staminaTarget = Mathf.Clamp(staminaTarget, 0f, 100f);
            staminaFalloff = Mathf.Clamp(staminaFalloff, 0f, 100f);
            glowThreshold = Mathf.Clamp01(glowThreshold);
            hazeThreshold = Mathf.Clamp01(hazeThreshold);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            NeoFpsEditorGUI.Header("Output");

            m_CanContinue &= WizardUtility.InspectOutputInfo(serializedObject);

            NeoFpsEditorGUI.Header("View Model");

            // View model
            if (allowNonModelPrefabs)
                m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabField(serializedObject.FindProperty("viewModel"));
            else
                m_CanContinue &= NeoFpsEditorGUI.RequiredModelPrefabField(serializedObject.FindProperty("viewModel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allowNonModelPrefabs"));
            if (!m_CanContinue)
                return;

            m_CanContinue &= NeoFpsEditorGUI.RequiredGameObjectInHierarchyField(serializedObject.FindProperty("weaponObject"), viewModel.transform, true);
            if (viewModel.GetComponentInChildren<Animator>() != null)
            {
                m_CanContinue &= NeoFpsEditorGUI.RequiredAssetField<AnimatorController>(serializedObject.FindProperty("animationController"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("usesAnimationEvents"));               
            }
            else
                NeoFpsEditorGUI.MiniWarning("This model is not set up for animations. It does not have an Animator component.");

            NeoFpsEditorGUI.Header("Camera");

            EditorGUILayout.HelpBox("NeoFPS places weapons relative to the first person camera. If your weapon view model uses a humanoid avatar or is not centered on the camera, then you can choose an object in its hierarchy to align to." +
                "This can also be used to apply camera animations in the view model to the NeoFPS character's camera.", MessageType.Info);

            NeoFpsEditorGUI.GameObjectInHierarchyField(serializedObject.FindProperty("cameraObject"), viewModel != null ? viewModel.transform : null, false);
            if (cameraObject != null)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraIsAnimated"));

            NeoFpsEditorGUI.Header("Inventory");

            m_CanContinue &= InspectInventoryOptions(serializedObject);

            // Sprinting
            NeoFpsEditorGUI.Header("Sprinting");
            NeoFpsEditorGUI.MultiChoiceField(null, serializedObject.FindProperty("sprintingStyle"), sprintingOptions);
            switch(sprintingStyle)
            {
                case 1: // Procedural
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fullStrengthSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sprintActionOnAim"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sprintActionOnReload"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sprintActionOnFire"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("minSprintInterrupt"));
                    break;
                case 2: // Keyframed
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blendZeroSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("blendFullSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sprintActionOnAim"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sprintActionOnReload"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sprintActionOnFire"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("minSprintInterrupt"));
                    break;
            }
            
            // Aim Fatigue
            NeoFpsEditorGUI.Header("Fatigue");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("aimingFatigue"));
            if (aimingFatigue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaLoss"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaTarget"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaFalloff"));
            }

            // Overheat
            NeoFpsEditorGUI.Header("Overheat");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("overheat"));
            if (overheat)
            {
                EditorGUILayout.HelpBox("The material for the glow renderer must use a shader with the _Glow property:" +
                    "\n- NeoFPS/Standard/GlowMetallic" +
                    "\n- NeoFPS/Standard/GlowSpecular" +
                    "\n\nThe material for the haze prefab's renderer must use a shader with the _HazeIntensity property:" +
                    "\nNeoFPS/Standard/HeatHaze.", MessageType.Info);

                // Get the glow renderer and grab material if not already set
                NeoFpsEditorGUI.ComponentInHierarchyField<MeshRenderer>(serializedObject.FindProperty("glowRenderer"), (weaponObject != null) ? weaponObject.transform : null, true);
                if (glowRenderer != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("glowMaterial"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("glowThreshold"));
                }
                NeoFpsEditorGUI.PrefabComponentField<MeshRenderer>(serializedObject.FindProperty("hazePrefab"));
                if (hazePrefab != null)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hazeThreshold"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("heatPerShot"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("heatLostPerSecond"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("damping"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("doOverheat"));
                if (doOverheat)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("coolingThreshold"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("overheatSound"));
                }
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.DoSummary("Prefab Name", prefabName);
            WizardGUI.DoSummary("Auto Prefix", autoPrefix);
            WizardGUI.DoSummary("Overwrite Existing", overwriteExisting);

            GUILayout.Space(4);

            WizardGUI.ObjectSummary("viewModel", viewModel);
            WizardGUI.ObjectSummary("weaponObject", weaponObject);
            WizardGUI.ObjectSummary("animationController", animationController);
            WizardGUI.DoSummary("usesAnimationEvents", usesAnimationEvents);

            GUILayout.Space(4);

            WizardGUI.ObjectSummary("cameraObject", cameraObject);
            if (cameraObject != null)
                WizardGUI.DoSummary("cameraIsAnimated", cameraIsAnimated);

            GUILayout.Space(4);

            SummariseInventoryOptions();

            GUILayout.Space(4);

            WizardGUI.MultiChoiceSummary("sprintingStyle", sprintingStyle, sprintingSummaries);
            switch (sprintingStyle)
            {
                case 1: // Procedural
                    WizardGUI.DoSummary("fullStrengthSpeed", fullStrengthSpeed);
                    WizardGUI.DoSummary("sprintActionOnAim", sprintActionOnAim.ToString());
                    WizardGUI.DoSummary("sprintActionOnReload", sprintActionOnReload.ToString());
                    WizardGUI.DoSummary("sprintActionOnFire", sprintActionOnFire.ToString());
                    WizardGUI.DoSummary("minSprintInterrupt", minSprintInterrupt);
                    break;
                case 2: // Keyframed
                    WizardGUI.DoSummary("maxSpeed", maxSpeed);
                    WizardGUI.DoSummary("blendZeroSpeed", blendZeroSpeed);
                    WizardGUI.DoSummary("blendFullSpeed", blendFullSpeed);
                    WizardGUI.DoSummary("sprintActionOnAim", sprintActionOnAim.ToString());
                    WizardGUI.DoSummary("sprintActionOnReload", sprintActionOnReload.ToString());
                    WizardGUI.DoSummary("sprintActionOnFire", sprintActionOnFire.ToString());
                    WizardGUI.DoSummary("minSprintInterrupt", minSprintInterrupt);
                    break;
            }

            GUILayout.Space(4);

            WizardGUI.DoSummary("aimingFatigue", aimingFatigue);
            if (aimingFatigue)
            {
                WizardGUI.DoSummary("staminaLoss", staminaLoss);
                WizardGUI.DoSummary("staminaTarget", staminaTarget);
                WizardGUI.DoSummary("staminaFalloff", staminaFalloff);
            }

            GUILayout.Space(4);

            WizardGUI.DoSummary("overheat", overheat);
            if (overheat)
            {
                WizardGUI.ObjectSummary("glowRenderer", glowRenderer);
                if (glowRenderer != null)
                {
                    WizardGUI.ObjectSummary("glowMaterial", glowMaterial);
                    WizardGUI.DoSummary("glowMaterial", glowMaterial);
                }
                WizardGUI.ObjectSummary("hazePrefab", hazePrefab);
                if (hazePrefab != null)
                    WizardGUI.DoSummary("hazeThreshold", hazeThreshold);
                WizardGUI.DoSummary("heatPerShot", heatPerShot);
                WizardGUI.DoSummary("heatLostPerSecond", heatLostPerSecond);
                WizardGUI.DoSummary("damping", damping);
                WizardGUI.DoSummary("doOverheat", doOverheat);
                if (doOverheat)
                {
                    WizardGUI.DoSummary("coolingThreshold", coolingThreshold);
                    WizardGUI.ObjectSummary("overheatSound", overheatSound);
                }
            }
        }
    }
}
