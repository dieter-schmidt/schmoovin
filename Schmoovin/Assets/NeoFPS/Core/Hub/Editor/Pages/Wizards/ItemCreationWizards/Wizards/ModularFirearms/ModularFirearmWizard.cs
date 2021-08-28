using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using UnityEditorInternal;
using NeoFPS.CharacterMotion;
using NeoFPSEditor.Hub.Pages.ItemCreationWizards.ModularFirearms;
using NeoSaveGames.Serialization;
using NeoFPS;
using NeoSaveGames;
using NeoFPSEditor.ModularFirearms;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public class ModularFirearmWizard : NeoFpsWizard
    {
        static readonly string[] k_RootSteps = new string[]
        {
            ModularFirearmWizardSteps.root,
            ModularFirearmWizardSteps.shooter,
            ModularFirearmWizardSteps.trigger,
            ModularFirearmWizardSteps.ammoEffect,
            ModularFirearmWizardSteps.recoil,
            ModularFirearmWizardSteps.muzzleEffect,
            ModularFirearmWizardSteps.reload,
            ModularFirearmWizardSteps.shellEject,
            ModularFirearmWizardSteps.aiming
        };

        static readonly string[] k_RootStepsWithAnimation = new string[]
        {
            ModularFirearmWizardSteps.root,
            ModularFirearmWizardSteps.shooter,
            ModularFirearmWizardSteps.trigger,
            ModularFirearmWizardSteps.ammoEffect,
            ModularFirearmWizardSteps.recoil,
            ModularFirearmWizardSteps.muzzleEffect,
            ModularFirearmWizardSteps.reload,
            ModularFirearmWizardSteps.shellEject,
            ModularFirearmWizardSteps.aiming,
            ModularFirearmWizardSteps.animations
        };

        protected override string[] GetRootSteps()
        {
            var root = steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            if (root.animationController != null)
                return k_RootStepsWithAnimation;
            else
                return k_RootSteps;
        }

        public override string GetDefaultTemplateFilename()
        {
            return "NewModularFirearmTemplate";
        }

        protected override void RegisterSteps()
        {
            RegisterStep<ModularFirearmRootStep>(ModularFirearmWizardSteps.root);
            RegisterStep<ModularFirearmShooterStep>(ModularFirearmWizardSteps.shooter);
            RegisterStep<ModularFirearmTriggerStep>(ModularFirearmWizardSteps.trigger);
            RegisterStep<ModularFirearmAmmoEffectStep>(ModularFirearmWizardSteps.ammoEffect);
            RegisterStep<ModularFirearmRecoilStep>(ModularFirearmWizardSteps.recoil);
            RegisterStep<ModularFirearmMuzzleEffectStep>(ModularFirearmWizardSteps.muzzleEffect);
            RegisterStep<ModularFirearmReloadStep>(ModularFirearmWizardSteps.reload);
            RegisterStep<ModularFirearmShellEjectStep>(ModularFirearmWizardSteps.shellEject);
            RegisterStep<ModularFirearmAimingStep>(ModularFirearmWizardSteps.aiming);
            RegisterStep<ModularFirearmAnimationsStep>(ModularFirearmWizardSteps.animations);
        }

        public override void CreateItem()
        {
            // Get the save folder
            var folderPath = WizardUtility.GetPrefabOutputFolder();
            if (folderPath == null)
                return;

            var root = steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            var animationsStep = steps[ModularFirearmWizardSteps.animations] as ModularFirearmAnimationsStep;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
                prefabName = "Firearm_" + prefabName;

            // Get prefab path
            var path = WizardUtility.GetPrefabPath(prefabName, folderPath, root.overwriteExisting);

            // Create root object
            GameObject rootObject = new GameObject(prefabName);
            var audioSource = WizardUtility.AddAudioSource(rootObject);

            // Add the firearm object
            var firearm = rootObject.AddComponent<ModularFirearm>();

            // Animations
            var firearmSO = new SerializedObject(firearm);
            firearmSO.FindProperty("m_FireAnimTrigger").stringValue = animationsStep.fireTrigger;
            firearmSO.FindProperty("m_RaiseAnimTrigger").stringValue = animationsStep.raiseTrigger;
            firearmSO.FindProperty("m_RaiseDuration").floatValue = animationsStep.raiseDuration;
            firearmSO.FindProperty("m_LowerAnimTrigger").stringValue = animationsStep.lowerTrigger;
            firearmSO.FindProperty("m_DeselectDuration").floatValue = animationsStep.lowerDuration;
            firearmSO.ApplyModifiedProperties();

            // Add the additive transform handler
            var springObject = WizardUtility.AddWieldableSpringObject(rootObject);

            // Add the display object
            var displayObject = AddDisplayObject(rootObject, springObject, root.usesAnimationEvents);
            ApplyFirstPersonLayerRecursive(displayObject.transform);

            // Get the weapon object from the view model hierarchy
            var weaponObject = WizardUtility.GetRelativeGameObject(root.viewModel, displayObject, root.weaponObject);

            // Add a components root object
            var componentsObject = new GameObject("Components");
            var componentsTransform = componentsObject.transform;
            componentsTransform.SetParent(weaponObject.transform);
            componentsTransform.localPosition = Vector3.zero;
            componentsTransform.localRotation = Quaternion.identity;
            componentsTransform.localScale = Vector3.one;

            // Modules
            var shooter = AddShooter(rootObject, displayObject, componentsObject);
            var trigger = AddTrigger(rootObject, root.animationController != null);
            var ammoEffect = AddAmmoEffect(rootObject);
            var ammoPool = AddAmmo(rootObject, ammoEffect);
            var reloader = AddReloader(rootObject, root.animationController != null);
            var recoil = AddRecoil(rootObject);
            var muzzleEffect = AddMuzzleEffect(rootObject, displayObject, componentsObject);
            var shellEject = AddShellEject(rootObject, displayObject, componentsObject);
            var aimer = AddAimer(rootObject, displayObject);

            // Overheat
            if (root.overheat)
            {
                var overheat = rootObject.AddComponent<FirearmOverheat>();
                var overheatSO = new SerializedObject(overheat);

                if (root.glowRenderer != null)
                {
                    var glowRenderer = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, root.glowRenderer);
                    if (root.glowMaterial != null)
                        glowRenderer.sharedMaterial = root.glowMaterial;

                    overheatSO.FindProperty("m_GlowRenderer").objectReferenceValue = glowRenderer;
                    overheatSO.FindProperty("m_GlowThreshold").floatValue = root.glowThreshold;
                }

                if (root.hazePrefab != null)
                {
                    var hazeObject = Instantiate(root.hazePrefab);
                    hazeObject.transform.SetParent(componentsTransform, false);

                    overheatSO.FindProperty("m_HazeRenderer").objectReferenceValue = hazeObject;
                    overheatSO.FindProperty("m_HazeThreshold").floatValue = root.hazeThreshold;
                }

                overheatSO.FindProperty("m_HeatPerShot").floatValue = root.heatPerShot;
                overheatSO.FindProperty("m_HeatLostPerSecond").floatValue = root.heatLostPerSecond;
                overheatSO.FindProperty("m_Damping").floatValue = root.damping;
                overheatSO.FindProperty("m_DoOverheat").boolValue = root.doOverheat;
                overheatSO.FindProperty("m_CoolingThreshold").floatValue = root.coolingThreshold;
                overheatSO.FindProperty("m_OverheatSound").objectReferenceValue = root.overheatSound;

                overheatSO.ApplyModifiedPropertiesWithoutUndo();
            }

            // Input
            rootObject.AddComponent<InputFirearm>();

            // Inventory
            root.AddInventoryToObject(rootObject);

            // Sprinting
            switch (root.sprintingStyle)
            {
                case 1: // Procedural
                    {
                        var sprintHandler = rootObject.AddComponent<ProceduralFirearmSprintHandler>();
                        var sprintHandlerSO = new SerializedObject(sprintHandler);
                        sprintHandlerSO.FindProperty("m_FullStrengthSpeed").floatValue = root.fullStrengthSpeed;
                        sprintHandlerSO.FindProperty("m_ActionOnAim").enumValueIndex = (int)root.sprintActionOnAim;
                        sprintHandlerSO.FindProperty("m_ActionOnReload").enumValueIndex = (int)root.sprintActionOnReload;
                        sprintHandlerSO.FindProperty("m_ActionOnFire").enumValueIndex = (int)root.sprintActionOnFire;
                        sprintHandlerSO.FindProperty("m_MinFireDuration").floatValue = root.minSprintInterrupt;
                        sprintHandlerSO.ApplyModifiedPropertiesWithoutUndo();
                    }
                    break;
                case 2: // Keyframed
                    {
                        var sprintHandler = rootObject.AddComponent<AnimatedFirearmSprintHandler>();
                        var sprintHandlerSO = new SerializedObject(sprintHandler);
                        sprintHandlerSO.FindProperty("m_UnscaledSprintMoveSpeed").floatValue = animationsStep.unscaledSprintMoveSpeed;
                        sprintHandlerSO.FindProperty("m_MaxSpeed").floatValue = root.maxSpeed;
                        sprintHandlerSO.FindProperty("m_BlendZeroSpeed").floatValue = root.blendZeroSpeed;
                        sprintHandlerSO.FindProperty("m_BlendFullSpeed").floatValue = root.blendFullSpeed;
                        sprintHandlerSO.FindProperty("m_ActionOnAim").enumValueIndex = (int)root.sprintActionOnAim;
                        sprintHandlerSO.FindProperty("m_ActionOnReload").enumValueIndex = (int)root.sprintActionOnReload;
                        sprintHandlerSO.FindProperty("m_ActionOnFire").enumValueIndex = (int)root.sprintActionOnFire;
                        sprintHandlerSO.FindProperty("m_MinFireDuration").floatValue = root.minSprintInterrupt;
                        sprintHandlerSO.FindProperty("m_SprintBoolParameter").stringValue = animationsStep.sprintBool;
                        sprintHandlerSO.FindProperty("m_SpeedFloatParameter").stringValue = animationsStep.sprintSpeedFloat;
                        sprintHandlerSO.FindProperty("m_BlendFloatParameter").stringValue = animationsStep.sprintBlendFloat;
                        sprintHandlerSO.ApplyModifiedPropertiesWithoutUndo();
                    }
                    break;
            }

            // Save Games
            rootObject.AddComponent<NeoSerializedGameObject>();
            var nsgo = springObject.AddComponent<NeoSerializedGameObject>();
            // Spring (disable transform save)
            var nsgoSO = new SerializedObject(nsgo);
            nsgoSO.FindProperty("m_Position").enumValueIndex = 2;
            nsgoSO.FindProperty("m_Rotation").enumValueIndex = 2;
            nsgoSO.ApplyModifiedPropertiesWithoutUndo();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            Debug.Log("Created Modular Firearm");
        }

        void ApplyFirstPersonLayerRecursive(Transform t)
        {
            t.gameObject.layer = PhysicsFilter.LayerIndex.WieldablesFirstPerson;
            foreach (Transform child in t)
                ApplyFirstPersonLayerRecursive(child);
        }

        GameObject AddDisplayObject(GameObject rootObject, GameObject springObject, bool useAnimationEvents)
        {
            var root = steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;

            var displayObject = Instantiate(root.viewModel);
            displayObject.transform.SetParent(springObject.transform);

            // Set animator
            var animator = displayObject.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                // Assign animator controller
                if (root.animationController != null)
                {
                    var animatorSO = new SerializedObject(animator);
                    animatorSO.FindProperty("m_Controller").objectReferenceValue = root.animationController;
                    animatorSO.ApplyModifiedPropertiesWithoutUndo();
                }

                if (useAnimationEvents)
                    displayObject.AddComponent<FirearmAnimEventsHandler>();

                // Add animator to save system
                var vmSave = displayObject.AddComponent<NeoSerializedGameObject>();
                var vmSaveSO = new SerializedObject(vmSave);
                SerializedArrayUtility.Add(vmSaveSO.FindProperty("m_OtherComponents"), animator, true);
                var overridesProp = vmSaveSO.FindProperty("m_Overrides");
                overridesProp.arraySize = 1;
                var persistenceProp = overridesProp.GetArrayElementAtIndex(0);
                persistenceProp.FindPropertyRelative("m_SaveMode").intValue = SaveMode.Persistence;
                persistenceProp.FindPropertyRelative("m_OverrideOtherComponents").boolValue = true;
                vmSaveSO.ApplyModifiedPropertiesWithoutUndo();
            }

            // Align to camera (and add animation)
            if (root.cameraObject != null)
            {
                Transform cameraTransform = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, root.cameraObject.transform);             

                // Get the difference and move the child
                Vector3 diff = cameraTransform.position - rootObject.transform.position;
                displayObject.transform.position -= diff;

                // Add the transform match setter if required
                if (root.cameraIsAnimated)
                {
                    // Create the anchor object from the camera transform
                    var anchor = new GameObject("CameraAnchor").transform;
                    anchor.SetParent(springObject.transform, true);
                    anchor.position = cameraTransform.position;
                    anchor.rotation = cameraTransform.rotation;

                    // Add the component and set up
                    var setter = rootObject.AddComponent<FirearmTransformMatchSetter>();
                    var setterSO = new SerializedObject(setter);
                    setterSO.FindProperty("m_Target").objectReferenceValue = cameraTransform;
                    setterSO.FindProperty("m_RelativeTo").objectReferenceValue = anchor;
                    setterSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            return displayObject;
        }

        IShooter AddShooter(GameObject rootObject, GameObject displayObject, GameObject componentsObject)
        {
            var root = steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            var shooterStep = steps[ModularFirearmWizardSteps.shooter] as ModularFirearmShooterStep;

            // Add the shooter
            switch (shooterStep.shooterModule)
            {
                case ModularFirearmShooterStep.ShooterModule.Hitscan:
                    {
                        var shooter = rootObject.AddComponent<HitscanShooter>();
                        var shooterSO = new SerializedObject(shooter);

                        if (shooterStep.muzzleTip != null)
                        {
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, shooterStep.muzzleTip.transform);
                        }
                        else
                        {
                            var muzzleTipObject = new GameObject("Muzzle Tip");
                            muzzleTipObject.transform.SetParent(componentsObject.transform);
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = muzzleTipObject;
                        }

                        shooterSO.FindProperty("m_MinimumSpread").floatValue = shooterStep.minAccuracySpread;
                        shooterSO.FindProperty("m_MaximumSpread").floatValue = shooterStep.maxAccuracySpread;

                        if (shooterStep.pooledTracer != null)
                        {
                            shooterSO.FindProperty("m_TracerPrototype").objectReferenceValue = shooterStep.pooledTracer;
                            shooterSO.FindProperty("m_TracerSize").floatValue = shooterStep.tracerSize;
                            shooterSO.FindProperty("m_TracerDuration").floatValue = shooterStep.tracerDuration;
                        }

                        shooterSO.ApplyModifiedPropertiesWithoutUndo();

                        return shooter;
                    }
                case ModularFirearmShooterStep.ShooterModule.Ballistic:
                    {
                        var shooter = rootObject.AddComponent<BallisticShooter>();
                        var shooterSO = new SerializedObject(shooter);

                        if (shooterStep.muzzleTip != null)
                        {
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, shooterStep.muzzleTip.transform);
                        }
                        else
                        {
                            var muzzleTipObject = new GameObject("Muzzle Tip");
                            muzzleTipObject.transform.SetParent(componentsObject.transform);
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = muzzleTipObject;
                        }

                        shooterSO.FindProperty("m_ProjectilePrefab").objectReferenceValue = shooterStep.projectilePrefab;
                        shooterSO.FindProperty("m_MinimumSpread").floatValue = shooterStep.minAccuracySpread;
                        shooterSO.FindProperty("m_MaximumSpread").floatValue = shooterStep.maxAccuracySpread;
                        shooterSO.FindProperty("m_MuzzleSpeed").floatValue = shooterStep.muzzleSpeed;
                        shooterSO.FindProperty("m_Gravity").floatValue = shooterStep.gravity;
                        
                        shooterSO.ApplyModifiedPropertiesWithoutUndo();

                        return shooter;
                    }
                case ModularFirearmShooterStep.ShooterModule.SpreadHitscan:
                    {
                        var shooter = rootObject.AddComponent<SpreadHitscanShooter>();
                        var shooterSO = new SerializedObject(shooter);

                        if (shooterStep.muzzleTip != null)
                        {
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, shooterStep.muzzleTip.transform);
                        }
                        else
                        {
                            var muzzleTipObject = new GameObject("Muzzle Tip");
                            muzzleTipObject.transform.SetParent(componentsObject.transform);
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = muzzleTipObject;
                        }

                        shooterSO.FindProperty("m_MinAimOffset").floatValue = shooterStep.minAccuracySpread;
                        shooterSO.FindProperty("m_MaxAimOffset").floatValue = shooterStep.maxAccuracySpread;
                        shooterSO.FindProperty("m_BulletCount").intValue = shooterStep.bulletCount;
                        shooterSO.FindProperty("m_Cone").floatValue = shooterStep.cone;

                        if (shooterStep.pooledTracer != null)
                        {
                            shooterSO.FindProperty("m_TracerPrototype").objectReferenceValue = shooterStep.pooledTracer;
                            shooterSO.FindProperty("m_TracerSize").floatValue = shooterStep.tracerSize;
                            shooterSO.FindProperty("m_TracerDuration").floatValue = shooterStep.tracerDuration;
                            shooterSO.FindProperty("m_ShotsPerTracer").floatValue = shooterStep.shotsPerTracer;
                        }

                        shooterSO.ApplyModifiedPropertiesWithoutUndo();

                        return shooter;
                    }
                case ModularFirearmShooterStep.ShooterModule.SpreadBallistic:
                    {
                        var shooter = rootObject.AddComponent<SpreadBallisticShooter>();
                        var shooterSO = new SerializedObject(shooter);

                        if (shooterStep.muzzleTip != null)
                        {
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, shooterStep.muzzleTip.transform);
                        }
                        else
                        {
                            var muzzleTipObject = new GameObject("Muzzle Tip");
                            muzzleTipObject.transform.SetParent(componentsObject.transform);
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = muzzleTipObject;
                        }

                        shooterSO.FindProperty("m_ProjectilePrefab").objectReferenceValue = shooterStep.projectilePrefab;
                        shooterSO.FindProperty("m_MinAimOffset").floatValue = shooterStep.minAccuracySpread;
                        shooterSO.FindProperty("m_MaxAimOffset").floatValue = shooterStep.maxAccuracySpread;
                        shooterSO.FindProperty("m_MuzzleSpeed").floatValue = shooterStep.muzzleSpeed;
                        shooterSO.FindProperty("m_BulletCount").intValue = shooterStep.bulletCount;
                        shooterSO.FindProperty("m_Cone").floatValue = shooterStep.cone;
                        shooterSO.FindProperty("m_Gravity").floatValue = shooterStep.gravity;

                        shooterSO.ApplyModifiedPropertiesWithoutUndo();

                        return shooter;
                    }
                case ModularFirearmShooterStep.ShooterModule.PatternHitscan:
                    {
                        var shooter = rootObject.AddComponent<PatternHitscanShooter>();
                        var shooterSO = new SerializedObject(shooter);

                        if (shooterStep.muzzleTip != null)
                        {
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, shooterStep.muzzleTip.transform);
                        }
                        else
                        {
                            var muzzleTipObject = new GameObject("Muzzle Tip");
                            muzzleTipObject.transform.SetParent(componentsObject.transform);
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = muzzleTipObject;
                        }

                        shooterSO.FindProperty("m_MinAimOffset").floatValue = shooterStep.minAccuracySpread;
                        shooterSO.FindProperty("m_MaxAimOffset").floatValue = shooterStep.maxAccuracySpread;
                        shooterSO.FindProperty("m_PatternDistance").floatValue = shooterStep.patternDistance;

                        var pointsProp = shooterSO.FindProperty("m_PatternPoints");
                        pointsProp.arraySize = shooterStep.patternPoints.Length;
                        for (int i = 0; i < shooterStep.patternPoints.Length; ++i)
                            pointsProp.GetArrayElementAtIndex(i).vector2Value = shooterStep.patternPoints[i];

                        if (shooterStep.pooledTracer != null)
                        {
                            shooterSO.FindProperty("m_TracerPrototype").objectReferenceValue = shooterStep.pooledTracer;
                            shooterSO.FindProperty("m_TracerSize").floatValue = shooterStep.tracerSize;
                            shooterSO.FindProperty("m_TracerDuration").floatValue = shooterStep.tracerDuration;
                        }

                        shooterSO.ApplyModifiedPropertiesWithoutUndo();

                        return shooter;
                    }
                case ModularFirearmShooterStep.ShooterModule.PatternBallistic:
                    {
                        var shooter = rootObject.AddComponent<PatternBallisticShooter>();
                        var shooterSO = new SerializedObject(shooter);

                        if (shooterStep.muzzleTip != null)
                        {
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, shooterStep.muzzleTip.transform);
                        }
                        else
                        {
                            var muzzleTipObject = new GameObject("Muzzle Tip");
                            muzzleTipObject.transform.SetParent(componentsObject.transform);
                            shooterSO.FindProperty("m_MuzzleTip").objectReferenceValue = muzzleTipObject;
                        }

                        shooterSO.FindProperty("m_ProjectilePrefab").objectReferenceValue = shooterStep.projectilePrefab;
                        shooterSO.FindProperty("m_MinAimOffset").floatValue = shooterStep.minAccuracySpread;
                        shooterSO.FindProperty("m_MaxAimOffset").floatValue = shooterStep.maxAccuracySpread;
                        shooterSO.FindProperty("m_MuzzleSpeed").floatValue = shooterStep.muzzleSpeed;
                        shooterSO.FindProperty("m_Gravity").floatValue = shooterStep.gravity;
                        shooterSO.FindProperty("m_PatternDistance").floatValue = shooterStep.patternDistance;

                        var pointsProp = shooterSO.FindProperty("m_PatternPoints");
                        pointsProp.arraySize = shooterStep.patternPoints.Length;
                        for (int i = 0; i < shooterStep.patternPoints.Length; ++i)
                            pointsProp.GetArrayElementAtIndex(i).vector2Value = shooterStep.patternPoints[i];

                        shooterSO.ApplyModifiedPropertiesWithoutUndo();

                        return shooter;
                    }
            }
            return null;
        }

        ITrigger AddTrigger(GameObject rootObject, bool customAnimations)
        {
            var triggerStep = steps[ModularFirearmWizardSteps.trigger] as ModularFirearmTriggerStep;

            switch (triggerStep.triggerModule)
            {
                case ModularFirearmTriggerStep.TriggerModule.SemiAuto:
                    {
                        var trigger = rootObject.AddComponent<SemiAutoTrigger>();
                        var triggerSO = new SerializedObject(trigger);

                        triggerSO.FindProperty("m_Cooldown").intValue = triggerStep.cooldown;
                        triggerSO.FindProperty("m_RepeatDelay").intValue = triggerStep.repeatDelay;

                        triggerSO.ApplyModifiedPropertiesWithoutUndo();

                        return trigger;
                    }
                case ModularFirearmTriggerStep.TriggerModule.Automatic:
                    {
                        var trigger = rootObject.AddComponent<AutomaticTrigger>();
                        var triggerSO = new SerializedObject(trigger);

                        triggerSO.FindProperty("m_ShotSpacing").intValue = triggerStep.shotSpacing;
                        if (customAnimations)
                        {
                            var animations = steps[ModularFirearmWizardSteps.animations] as ModularFirearmAnimationsStep;
                            triggerSO.FindProperty("m_TriggerHoldAnimKey").stringValue = animations.triggerHoldBool;
                        }

                        triggerSO.ApplyModifiedPropertiesWithoutUndo();

                        return trigger;
                    }
                case ModularFirearmTriggerStep.TriggerModule.Burst:
                    {
                        var trigger = rootObject.AddComponent<BurstFireTrigger>();
                        var triggerSO = new SerializedObject(trigger);
                        
                        triggerSO.FindProperty("m_BurstSize").intValue = triggerStep.burstSize;
                        triggerSO.FindProperty("m_BurstSpacing").intValue = triggerStep.burstSpacing;
                        triggerSO.FindProperty("m_MinRepeatDelay").intValue = triggerStep.minRepeatDelay;
                        triggerSO.FindProperty("m_RepeatOnTriggerHold").boolValue = triggerStep.repeatOnTriggerHold;
                        triggerSO.FindProperty("m_HoldRepeatDelay").intValue = triggerStep.holdRepeatDelay;
                        triggerSO.FindProperty("m_CancelOnRelease").boolValue = triggerStep.cancelOnRelease;
                        if (customAnimations)
                        {
                            var animations = steps[ModularFirearmWizardSteps.animations] as ModularFirearmAnimationsStep;
                            triggerSO.FindProperty("m_TriggerHoldAnimKey").stringValue = animations.triggerHoldBool;
                        }

                        triggerSO.ApplyModifiedPropertiesWithoutUndo();

                        return trigger;
                    }
                case ModularFirearmTriggerStep.TriggerModule.Charged:
                    {
                        var trigger = rootObject.AddComponent<ChargedTrigger>();
                        var triggerSO = new SerializedObject(trigger);

                        triggerSO.FindProperty("m_ChargeDuration").floatValue = triggerStep.chargeDuration;
                        triggerSO.FindProperty("m_UnchargeDuration").floatValue = triggerStep.unchargeDuration;
                        triggerSO.FindProperty("m_Repeat").boolValue = triggerStep.canRepeat;
                        triggerSO.FindProperty("m_RepeatDelay").floatValue = triggerStep.repeatDelay;

                        var chargeSource = WizardUtility.AddAudioSource(rootObject);
                        triggerSO.FindProperty("m_AudioSource").objectReferenceValue = chargeSource;
                        triggerSO.FindProperty("m_TriggerAudioCharge").objectReferenceValue = triggerStep.triggerAudioCharge;
                        triggerSO.FindProperty("m_TriggerAudioRelease").objectReferenceValue = triggerStep.triggerAudioRelease;
                        
                        triggerSO.ApplyModifiedPropertiesWithoutUndo();

                        return trigger;
                    }
                case ModularFirearmTriggerStep.TriggerModule.Queued:
                    {
                        var trigger = rootObject.AddComponent<QueuedTrigger>();
                        var triggerSO = new SerializedObject(trigger);

                        triggerSO.FindProperty("m_QueueSpacing").intValue = triggerStep.queueSpacing;
                        triggerSO.FindProperty("m_DelayFirst").boolValue = triggerStep.delayFirst;
                        triggerSO.FindProperty("m_MaxQueueSize").intValue = triggerStep.maxQueueSize;
                        triggerSO.FindProperty("m_BurstSpacing").intValue = triggerStep.burstSpacing;
                        triggerSO.FindProperty("m_RepeatDelay").intValue = triggerStep.minRepeatDelay;

                        triggerSO.ApplyModifiedPropertiesWithoutUndo();

                        return trigger;
                    }
                case ModularFirearmTriggerStep.TriggerModule.TargetLock:
                    {
                        var trigger = rootObject.AddComponent<TargetLockTrigger>();
                        var triggerSO = new SerializedObject(trigger);

                        triggerSO.FindProperty("m_DetectionTag").stringValue = triggerStep.detectionTag;
                        triggerSO.FindProperty("m_DetectionLayers").intValue = triggerStep.detectionLayers;
                        triggerSO.FindProperty("m_LockOnTime").floatValue = triggerStep.lockOnTime;
                        triggerSO.FindProperty("m_DetectionConeAngle").floatValue = triggerStep.detectionConeAngle;
                        triggerSO.FindProperty("m_DetectionRange").floatValue = triggerStep.detectionRange;
                        triggerSO.FindProperty("m_RequireLineOfSight").boolValue = triggerStep.requireLineOfSight;
                        triggerSO.FindProperty("m_BlockingLayers").intValue = triggerStep.blockingLayers;
                        triggerSO.FindProperty("m_Memory").floatValue = triggerStep.memory;

                        triggerSO.ApplyModifiedPropertiesWithoutUndo();

                        return trigger;
                    }
                case ModularFirearmTriggerStep.TriggerModule.MultiTargetLock:
                    {
                        var trigger = rootObject.AddComponent<MultiTargetLockTrigger>();
                        var triggerSO = new SerializedObject(trigger);

                        triggerSO.FindProperty("m_DetectionTag").stringValue = triggerStep.detectionTag;
                        triggerSO.FindProperty("m_DetectionLayers").intValue = triggerStep.detectionLayers;
                        triggerSO.FindProperty("m_DetectionConeAngle").floatValue = triggerStep.detectionConeAngle;
                        triggerSO.FindProperty("m_DetectionRange").floatValue = triggerStep.detectionRange;
                        triggerSO.FindProperty("m_LockSpacing").intValue = triggerStep.lockSpacing;
                        triggerSO.FindProperty("m_LockRetrySpacing").intValue = triggerStep.lockRetrySpacing;
                        triggerSO.FindProperty("m_RequireLineOfSight").boolValue = triggerStep.requireLineOfSight;
                        triggerSO.FindProperty("m_BlockingLayers").intValue = triggerStep.blockingLayers;
                        triggerSO.FindProperty("m_DelayFirst").boolValue = triggerStep.delayFirst;
                        triggerSO.FindProperty("m_MaxQueueSize").intValue = triggerStep.maxQueueSize;
                        triggerSO.FindProperty("m_BurstSpacing").intValue = triggerStep.burstSpacing;
                        triggerSO.FindProperty("m_RepeatDelay").intValue = triggerStep.minRepeatDelay;

                        triggerSO.ApplyModifiedPropertiesWithoutUndo();

                        return trigger;
                    }
            }

            return null;
        }

        BaseAmmoEffect AddAmmoEffect(GameObject rootObject)
        {
            var ammoEffectStep = steps[ModularFirearmWizardSteps.ammoEffect] as ModularFirearmAmmoEffectStep;
            var shooterStep = steps[ModularFirearmWizardSteps.shooter] as ModularFirearmShooterStep;

            switch (ammoEffectStep.GetImpactAmmoEffect(this))
            {
                case ModularFirearmAmmoEffectStep.ImpactEffectModule.PenetratingHitscan:
                    {
                        // Get primary and secondary sub effects
                        var primary = AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[0], ammoEffectStep.primaryAmmoEffect);
                        var secondary = AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[1], ammoEffectStep.secondaryAmmoEffect);

                        var effect = rootObject.AddComponent<PenetratingHitscanAmmoEffect>();
                        SerializedObject effectSO = new SerializedObject(effect);

                        effectSO.FindProperty("m_InitialHitEffect").objectReferenceValue = primary;
                        effectSO.FindProperty("m_SecondHitEffect").objectReferenceValue = secondary;
                        effectSO.FindProperty("m_MaxPenetration").floatValue = ammoEffectStep.maxPenetration;
                        effectSO.FindProperty("m_MaxScatterAngle").floatValue = ammoEffectStep.maxScatterAngle;
                        effectSO.FindProperty("m_ExitEffectSize").floatValue = ammoEffectStep.exitEffectSize;
                        effectSO.FindProperty("m_TracerPrototype").objectReferenceValue = ammoEffectStep.tracerPrototype;
                        effectSO.FindProperty("m_TracerSize").floatValue = ammoEffectStep.tracerSize;
                        effectSO.FindProperty("m_TracerDuration").floatValue = ammoEffectStep.tracerDuration;

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmAmmoEffectStep.ImpactEffectModule.PenetratingBallistic:
                    {
                        // Get primary and secondary sub effects
                        var primary = AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[0], ammoEffectStep.primaryAmmoEffect);
                        var secondary = AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[1], ammoEffectStep.secondaryAmmoEffect);

                        var effect = rootObject.AddComponent<PenetratingProjectileAmmoEffect>();
                        SerializedObject effectSO = new SerializedObject(effect);

                        effectSO.FindProperty("m_InitialHitEffect").objectReferenceValue = primary;
                        effectSO.FindProperty("m_SecondHitEffect").objectReferenceValue = secondary;
                        effectSO.FindProperty("m_MaxPenetration").floatValue = ammoEffectStep.maxPenetration;
                        effectSO.FindProperty("m_MaxScatterAngle").floatValue = ammoEffectStep.maxScatterAngle;
                        effectSO.FindProperty("m_ExitEffectSize").floatValue = ammoEffectStep.exitEffectSize;
                        effectSO.FindProperty("m_ProjectilePrefab").objectReferenceValue = ammoEffectStep.projectilePrefab;
                        effectSO.FindProperty("m_Gravity").floatValue = ammoEffectStep.gravity;

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmAmmoEffectStep.ImpactEffectModule.RicochetHitscan:
                    {
                        // Get primary and secondary sub effects
                        var primary = AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[0], ammoEffectStep.primaryAmmoEffect);
                        var secondary = AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[1], ammoEffectStep.secondaryAmmoEffect);

                        var effect = rootObject.AddComponent<RicochetHitscanAmmoEffect>();
                        SerializedObject effectSO = new SerializedObject(effect);

                        effectSO.FindProperty("m_HitEffect").objectReferenceValue = primary;
                        effectSO.FindProperty("m_RicochetEffect").objectReferenceValue = secondary;
                        effectSO.FindProperty("m_NormalVsDeflection").floatValue = ammoEffectStep.normalVsDeflection;
                        effectSO.FindProperty("m_MaxScatterAngle").floatValue = ammoEffectStep.maxScatterAngle;
                        effectSO.FindProperty("m_SplitCount").intValue = ammoEffectStep.splitCount;
                        effectSO.FindProperty("m_TracerPrototype").objectReferenceValue = ammoEffectStep.tracerPrototype;
                        effectSO.FindProperty("m_TracerSize").floatValue = ammoEffectStep.tracerSize;
                        effectSO.FindProperty("m_TracerDuration").floatValue = ammoEffectStep.tracerDuration;

                        return effect;
                    }
                case ModularFirearmAmmoEffectStep.ImpactEffectModule.RicochetBallistic:
                    {
                        // Get primary and secondary sub effects
                        var primary = AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[0], ammoEffectStep.primaryAmmoEffect);
                        var secondary = AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[1], ammoEffectStep.secondaryAmmoEffect);
                        
                        var effect = rootObject.AddComponent<RicochetProjectileAmmoEffect>();
                        SerializedObject effectSO = new SerializedObject(effect);

                        effectSO.FindProperty("m_HitEffect").objectReferenceValue = primary;
                        effectSO.FindProperty("m_RicochetEffect").objectReferenceValue = secondary;
                        effectSO.FindProperty("m_NormalVsDeflection").floatValue = ammoEffectStep.normalVsDeflection;
                        effectSO.FindProperty("m_MaxScatterAngle").floatValue = ammoEffectStep.maxScatterAngle;
                        effectSO.FindProperty("m_SplitCount").intValue = ammoEffectStep.splitCount;
                        effectSO.FindProperty("m_ProjectilePrefab").objectReferenceValue = ammoEffectStep.projectilePrefab;
                        effectSO.FindProperty("m_Gravity").floatValue = ammoEffectStep.gravity;

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmAmmoEffectStep.ImpactEffectModule.Surface:
                    {
                        // Get primary and secondary sub effects
                        var primary = AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[0], ammoEffectStep.primaryAmmoEffect);
                        var secondary = AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[1], ammoEffectStep.secondaryAmmoEffect);

                        // Get impact effect
                        var effect = rootObject.AddComponent<SurfaceBulletPhysicsAmmoEffect>();
                        SerializedObject effectSO = new SerializedObject(effect);

                        effectSO.FindProperty("m_HitEffect").objectReferenceValue = primary;
                        effectSO.FindProperty("m_SurfacePhysics").objectReferenceValue = ammoEffectStep.surfacePhysics;
                        effectSO.FindProperty("m_MaxRicochetScatter").floatValue = ammoEffectStep.maxRicochetScatter;
                        effectSO.FindProperty("m_MaxPenetrationDeflect").floatValue = ammoEffectStep.maxPenetrationDeflect;
                        effectSO.FindProperty("m_ExitEffectSize").floatValue = ammoEffectStep.exitEffectSize;
                        effectSO.FindProperty("m_Recursive").boolValue = ammoEffectStep.recursive;
                        effectSO.FindProperty("m_ProjectilePrefab").objectReferenceValue = ammoEffectStep.projectilePrefab;
                        effectSO.FindProperty("m_Gravity").floatValue = ammoEffectStep.gravity;

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmAmmoEffectStep.ImpactEffectModule.None:
                    {
                        return AddAmmoSubEffect(rootObject, ammoEffectStep.effectData[0], ammoEffectStep.primaryAmmoEffect);
                    }
            }

            return null;
        }

        BaseAmmoEffect AddAmmoSubEffect(GameObject rootObject, ModularFirearmAmmoEffectStep.AmmoEffectInfo ammoInfo, ModularFirearmAmmoEffectStep.AmmoEffectModule module)
        {
            switch (module)
            {
                case ModularFirearmAmmoEffectStep.AmmoEffectModule.Bullet:
                    {
                        var effect = rootObject.AddComponent<BulletAmmoEffect>();
                        var effectSO = new SerializedObject(effect);

                        effectSO.FindProperty("m_DamageType").intValue = (int)ammoInfo.damageType;
                        effectSO.FindProperty("m_Damage").floatValue = ammoInfo.damage;
                        effectSO.FindProperty("m_BulletSize").floatValue = ammoInfo.bulletSize;
                        effectSO.FindProperty("m_ImpactForce").floatValue = ammoInfo.impactForce;

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmAmmoEffectStep.AmmoEffectModule.AdvancedBullet:
                    {
                        var effect = rootObject.AddComponent<AdvancedBulletAmmoEffect>();
                        var effectSO = new SerializedObject(effect);

                        effectSO.FindProperty("m_DamageType").intValue = (int)ammoInfo.damageType;
                        effectSO.FindProperty("m_RandomiseDamage").boolValue = ammoInfo.randomiseDamage;
                        effectSO.FindProperty("m_MinDamage").floatValue = ammoInfo.minDamage;
                        effectSO.FindProperty("m_MaxDamage").floatValue = ammoInfo.maxDamage;
                        effectSO.FindProperty("m_FalloffMode").enumValueIndex = (int)ammoInfo.falloffMode;
                        effectSO.FindProperty("m_FalloffSettingLower").floatValue = ammoInfo.falloffSettingLower;
                        effectSO.FindProperty("m_FalloffSettingUpper").floatValue = ammoInfo.falloffSettingUpper;
                        effectSO.FindProperty("m_BulletSize").floatValue = ammoInfo.bulletSize;
                        effectSO.FindProperty("m_ImpactForce").floatValue = ammoInfo.impactForce;
                        
                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmAmmoEffectStep.AmmoEffectModule.Explosion:
                    {
                        var effect = rootObject.AddComponent<PooledExplosionAmmoEffect>();
                        var effectSO = new SerializedObject(effect);

                        effectSO.FindProperty("m_Explosion").objectReferenceValue = ammoInfo.explosion;
                        effectSO.FindProperty("m_Damage").floatValue = ammoInfo.damage;
                        effectSO.FindProperty("m_MaxForce").floatValue = ammoInfo.maxForce;
                        effectSO.FindProperty("m_NormalOffset").floatValue = ammoInfo.normalOffset;
                        
                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmAmmoEffectStep.AmmoEffectModule.Particle:
                    {
                        var effect = rootObject.AddComponent<ParticleAmmoEffect>();
                        var effectSO = new SerializedObject(effect);

                        effectSO.FindProperty("m_DamageType").intValue = (int)ammoInfo.damageType;
                        effectSO.FindProperty("m_ImpactEffect").objectReferenceValue = ammoInfo.impactEffect;
                        effectSO.FindProperty("m_Damage").floatValue = ammoInfo.damage;
                        effectSO.FindProperty("m_ImpactForce").floatValue = ammoInfo.impactForce;

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
            }
            return null;
        }

        IAmmo AddAmmo(GameObject rootObject, BaseAmmoEffect ammoEffect)
        {
            var reloadStep = steps[ModularFirearmWizardSteps.reload] as ModularFirearmReloadStep;

            switch (reloadStep.ammoModule)
            {
                case ModularFirearmReloadStep.AmmoModule.Custom:
                    {
                        var ammo = rootObject.AddComponent<CustomAmmo>();
                        var ammoSO = new SerializedObject(ammo);

                        ammoSO.FindProperty("m_PrintableName").stringValue = reloadStep.printableName;
                        ammoSO.FindProperty("m_StartingAmmo").intValue = reloadStep.startingAmmo;
                        ammoSO.FindProperty("m_MaxAmmo").intValue = reloadStep.maxAmmo;
                        ammoSO.FindProperty("m_Effect").objectReferenceValue = ammoEffect;

                        ammoSO.ApplyModifiedPropertiesWithoutUndo();

                        return ammo;
                    }
                case ModularFirearmReloadStep.AmmoModule.Inventory:
                    {
                        var ammo = rootObject.AddComponent<SharedPoolAmmo>();
                        var ammoSO = new SerializedObject(ammo);

                        ammoSO.FindProperty("m_AmmoType").objectReferenceValue = reloadStep.sharedAmmoType;
                        ammoSO.FindProperty("m_Effect").objectReferenceValue = ammoEffect;

                        ammoSO.ApplyModifiedPropertiesWithoutUndo();

                        return ammo;
                    }
                case ModularFirearmReloadStep.AmmoModule.Infinite:
                    {
                        var ammo = rootObject.AddComponent<InfiniteAmmo>();
                        var ammoSO = new SerializedObject(ammo);

                        ammoSO.FindProperty("m_PrintableName").stringValue = reloadStep.printableName;
                        ammoSO.FindProperty("m_FixedSize").intValue = reloadStep.fixedSize;
                        ammoSO.FindProperty("m_Effect").objectReferenceValue = ammoEffect;

                        ammoSO.ApplyModifiedPropertiesWithoutUndo();

                        return ammo;
                    }
                case ModularFirearmReloadStep.AmmoModule.Recharging:
                    {
                        var ammo = rootObject.AddComponent<RechargingAmmo>();
                        var ammoSO = new SerializedObject(ammo);

                        ammoSO.FindProperty("m_PrintableName").stringValue = reloadStep.printableName;
                        ammoSO.FindProperty("m_StartingAmmo").intValue = reloadStep.startingAmmo;
                        ammoSO.FindProperty("m_MaxAmmo").intValue = reloadStep.maxAmmo;
                        ammoSO.FindProperty("m_Effect").objectReferenceValue = ammoEffect;
                        ammoSO.FindProperty("m_RechargeSpacing").floatValue = reloadStep.rechargeSpacing;
                        ammoSO.FindProperty("m_RechargeAmount").intValue = reloadStep.rechargeAmount;
                        ammoSO.FindProperty("m_ResetOnChange").boolValue = reloadStep.resetOnChange;

                        ammoSO.ApplyModifiedPropertiesWithoutUndo();

                        return ammo;
                    }
            }

            return null;
        }

        IReloader AddReloader(GameObject rootObject, bool customAnimations)
        {
            var reloadStep = steps[ModularFirearmWizardSteps.reload] as ModularFirearmReloadStep;

            IReloader result = null;
            switch (reloadStep.reloaderModule)
            {
                case ModularFirearmReloadStep.ReloaderModule.Simple:
                    {
                        var reloader = rootObject.AddComponent<SimpleReloader>();
                        var reloaderSO = new SerializedObject(reloader);

                        reloaderSO.FindProperty("m_MagazineSize").intValue = reloadStep.magazineSize;
                        reloaderSO.FindProperty("m_StartingMagazine").intValue = reloadStep.startingMagazine;
                        reloaderSO.FindProperty("m_ReloadDuration").floatValue = reloadStep.reloadDuration;
                        reloaderSO.FindProperty("m_ReloadAudio").objectReferenceValue = reloadStep.reloadAudio;
                        if (customAnimations)
                        {
                            var animations = steps[ModularFirearmWizardSteps.animations] as ModularFirearmAnimationsStep;
                            reloaderSO.FindProperty("m_ReloadAnimTrigger").stringValue = animations.reloadTrigger;
                        }

                        reloaderSO.ApplyModifiedPropertiesWithoutUndo();

                        result = reloader;
                        break;
                    }
                case ModularFirearmReloadStep.ReloaderModule.Chambered:
                    {
                        var reloader = rootObject.AddComponent<ChamberedReloader>();
                        var reloaderSO = new SerializedObject(reloader);

                        reloaderSO.FindProperty("m_MagazineSize").intValue = reloadStep.magazineSize;
                        reloaderSO.FindProperty("m_StartingMagazine").intValue = reloadStep.startingMagazine;
                        reloaderSO.FindProperty("m_ReloadDuration").floatValue = reloadStep.reloadDuration;
                        reloaderSO.FindProperty("m_ReloadAudio").objectReferenceValue = reloadStep.reloadAudio;
                        reloaderSO.FindProperty("m_ReloadDurationEmpty").floatValue = reloadStep.reloadDurationEmpty;
                        reloaderSO.FindProperty("m_ReloadAudioEmpty").objectReferenceValue = reloadStep.reloadAudioEmpty;
                        if (customAnimations)
                        {
                            var animations = steps[ModularFirearmWizardSteps.animations] as ModularFirearmAnimationsStep;
                            reloaderSO.FindProperty("m_ReloadAnimTrigger").stringValue = animations.reloadTrigger;
                            reloaderSO.FindProperty("m_EmptyAnimBool").stringValue = animations.chamberEmptyBool;
                        }

                        reloaderSO.ApplyModifiedPropertiesWithoutUndo();

                        result = reloader;
                        break;
                    }

                case ModularFirearmReloadStep.ReloaderModule.Incremental:
                    {
                        var reloader = rootObject.AddComponent<IncrementalReloader>();
                        var reloaderSO = new SerializedObject(reloader);

                        reloaderSO.FindProperty("m_MagazineSize").intValue = reloadStep.magazineSize;
                        reloaderSO.FindProperty("m_StartingMagazine").intValue = reloadStep.startingMagazine;
                        reloaderSO.FindProperty("m_RoundsPerIncrement").intValue = reloadStep.roundsPerIncrement;
                        reloaderSO.FindProperty("m_ReloadStartDuration").floatValue = reloadStep.reloadStartDuration;
                        reloaderSO.FindProperty("m_ReloadIncrementDuration").floatValue = reloadStep.reloadIncrementDuration;
                        reloaderSO.FindProperty("m_ReloadEndDuration").floatValue = reloadStep.reloadEndDuration;
                        reloaderSO.FindProperty("m_ReloadAudioStart").objectReferenceValue = reloadStep.reloadAudioStart;
                        reloaderSO.FindProperty("m_ReloadAudioIncrement").objectReferenceValue = reloadStep.reloadAudioIncrement;
                        reloaderSO.FindProperty("m_ReloadAudioEnd").objectReferenceValue = reloadStep.reloadAudioEnd;
                        if (customAnimations)
                        {
                            var animations = steps[ModularFirearmWizardSteps.animations] as ModularFirearmAnimationsStep;
                            reloaderSO.FindProperty("m_ReloadAnimTrigger").stringValue = animations.reloadTrigger;
                            reloaderSO.FindProperty("m_ReloadAnimCountProp").stringValue = animations.reloadCountInt;
                        }

                        reloaderSO.ApplyModifiedPropertiesWithoutUndo();

                        result = reloader;
                        break;
                    }
                case ModularFirearmReloadStep.ReloaderModule.Passthrough:
                    {
                        var reloader = rootObject.AddComponent<PassthroughReloader>();
                        result = reloader;
                        break;
                    }
            }

            if (reloadStep.countDownLastRounds)
            {
                var countdown = rootObject.AddComponent<ReloaderCountdown>();
                var countdownSO = new SerializedObject(countdown);

                countdownSO.FindProperty("m_ExtendSequence").boolValue = reloadStep.extendSequence;

                var arrayProp = countdownSO.FindProperty("m_CountdownAudio");
                arrayProp.arraySize = reloadStep.countdownAudio.Length;
                for (int i = 0; i < reloadStep.countdownAudio.Length; ++i)
                {
                    var prop = arrayProp.GetArrayElementAtIndex(i);
                    prop.FindPropertyRelative("clip").objectReferenceValue = reloadStep.countdownAudio[i].clip;
                    prop.FindPropertyRelative("volume").floatValue = reloadStep.countdownAudio[i].volume;
                }

                countdownSO.ApplyModifiedPropertiesWithoutUndo();
            }

            return result;
        }

        IRecoilHandler AddRecoil(GameObject rootObject)
        {
            var recoilStep = steps[ModularFirearmWizardSteps.recoil] as ModularFirearmRecoilStep;

            if (recoilStep.useSpringRecoil)
            {
                var recoil = rootObject.AddComponent<BetterSpringRecoilHandler>();
                var recoilSO = new SerializedObject(recoil);
                recoilSO.FindProperty("m_HipAccuracyKick").floatValue = recoilStep.hipAccuracyKick;
                recoilSO.FindProperty("m_HipAccuracyRecover").floatValue = recoilStep.hipAccuracyRecover;
                recoilSO.FindProperty("m_SightedAccuracyKick").floatValue = recoilStep.sightedAccuracyKick;
                recoilSO.FindProperty("m_SightedAccuracyRecover").floatValue = recoilStep.sightedAccuracyRecover;

                var recoilProp = recoilSO.FindProperty("m_HipFireRecoil");
                recoilProp.FindPropertyRelative("recoilAngle").floatValue = recoilStep.hipFireRecoil.recoilAngle;
                recoilProp.FindPropertyRelative("wander").floatValue = recoilStep.hipFireRecoil.wander;
                recoilProp.FindPropertyRelative("horizontalMultiplier").floatValue = recoilStep.hipFireRecoil.horizontalMultiplier;
                recoilProp.FindPropertyRelative("verticalDivergence").floatValue = recoilStep.hipFireRecoil.verticalDivergence;
                recoilProp.FindPropertyRelative("horizontalDivergence").floatValue = recoilStep.hipFireRecoil.horizontalDivergence;
                recoilProp.FindPropertyRelative("pushBack").floatValue = recoilStep.hipFireRecoil.pushBack;
                recoilProp.FindPropertyRelative("maxPushBack").floatValue = recoilStep.hipFireRecoil.maxPushBack;
                recoilProp.FindPropertyRelative("jiggle").floatValue = recoilStep.hipFireRecoil.jiggle;
                recoilProp.FindPropertyRelative("duration").floatValue = recoilStep.hipFireRecoil.duration;

                recoilProp = recoilSO.FindProperty("m_AimedRecoil");
                recoilProp.FindPropertyRelative("recoilAngle").floatValue = recoilStep.aimedRecoil.recoilAngle;
                recoilProp.FindPropertyRelative("wander").floatValue = recoilStep.aimedRecoil.wander;
                recoilProp.FindPropertyRelative("horizontalMultiplier").floatValue = recoilStep.aimedRecoil.horizontalMultiplier;
                recoilProp.FindPropertyRelative("verticalDivergence").floatValue = recoilStep.aimedRecoil.verticalDivergence;
                recoilProp.FindPropertyRelative("horizontalDivergence").floatValue = recoilStep.aimedRecoil.horizontalDivergence;
                recoilProp.FindPropertyRelative("pushBack").floatValue = recoilStep.aimedRecoil.pushBack;
                recoilProp.FindPropertyRelative("maxPushBack").floatValue = recoilStep.aimedRecoil.maxPushBack;
                recoilProp.FindPropertyRelative("jiggle").floatValue = recoilStep.aimedRecoil.jiggle;
                recoilProp.FindPropertyRelative("duration").floatValue = recoilStep.aimedRecoil.duration;

                recoilSO.ApplyModifiedPropertiesWithoutUndo();

                return recoil;
            }
            else
            {
                var recoil = rootObject.AddComponent<AccuracyOnlyRecoilHandler>();
                var recoilSO = new SerializedObject(recoil);
                recoilSO.FindProperty("m_HipAccuracyKick").floatValue = recoilStep.hipAccuracyKick;
                recoilSO.FindProperty("m_HipAccuracyRecover").floatValue = recoilStep.hipAccuracyRecover;
                recoilSO.FindProperty("m_SightedAccuracyKick").floatValue = recoilStep.sightedAccuracyKick;
                recoilSO.FindProperty("m_SightedAccuracyRecover").floatValue = recoilStep.sightedAccuracyRecover;
                recoilSO.ApplyModifiedPropertiesWithoutUndo();

                return recoil;
            }
        }

        IAimer AddAimer(GameObject rootObject, GameObject displayObject)
        {
            var root = steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            var aimingStep = steps[ModularFirearmWizardSteps.aiming] as ModularFirearmAimingStep;

            bool customAnimations = root.animationController != null;

            IAimer result = null;
            switch (aimingStep.aimerModule)
            {
                case ModularFirearmAimingStep.AimerModule.WeaponMoveAimer:
                    {
                        var aimer = rootObject.AddComponent<WeaponMoveAimer>();
                        var aimerSO = new SerializedObject(aimer);

                        aimerSO.FindProperty("m_AimUpAudio").objectReferenceValue = aimingStep.aimUpAudio;
                        aimerSO.FindProperty("m_AimDownAudio").objectReferenceValue = aimingStep.aimDownAudio;
                        aimerSO.FindProperty("m_HipAccuracyCap").floatValue = aimingStep.hipAccuracyCap;
                        aimerSO.FindProperty("m_AimedAccuracyCap").floatValue = aimingStep.aimedAccuracyCap;
                        aimerSO.FindProperty("m_CanAimWhileReloading").boolValue = aimingStep.canAimWhileReloading;
                        aimerSO.FindProperty("m_FovMultiplier").floatValue = aimingStep.fovMultiplier;
                        aimerSO.FindProperty("m_AimTime").floatValue = aimingStep.aimTime;
                        aimerSO.FindProperty("m_PositionSpringMultiplier").floatValue = aimingStep.positionSpringMultiplier;
                        aimerSO.FindProperty("m_RotationSpringMultiplier").floatValue = aimingStep.rotationSpringMultiplier;
                        aimerSO.FindProperty("m_BlockTrigger").boolValue = aimingStep.blockTrigger;
                        aimerSO.FindProperty("m_CrosshairUp").FindPropertyRelative("m_Value").intValue = aimingStep.crosshairAiming;
                        aimerSO.FindProperty("m_CrosshairDown").FindPropertyRelative("m_Value").intValue = aimingStep.crosshairHipFire;

                        switch (aimingStep.aimPositionOption)
                        {
                            case 0: // anchor
                                if (aimingStep.aimTarget != null)
                                {
                                    WeaponMoveAimerEditor.BuildAimOffsetFromTransform(
                                        aimerSO.FindProperty("m_AimPosition"),
                                        aimerSO.FindProperty("m_AimRotation"),
                                        rootObject.transform.transform,
                                        WizardUtility.GetRelativeComponent(root.viewModel, displayObject, aimingStep.aimTarget.transform)
                                        );
                                }
                                break;
                            case 1: // offsets
                                aimerSO.FindProperty("m_AimPosition").vector3Value = aimingStep.aimPosition;
                                aimerSO.FindProperty("m_AimRotation").vector3Value = aimingStep.aimRotation;
                                break;
                        }

                        if (customAnimations)
                        {
                            var animations = steps[ModularFirearmWizardSteps.animations] as ModularFirearmAnimationsStep;
                            aimerSO.FindProperty("m_AimAnimBool").stringValue = animations.aimBool;
                        }
                        
                        aimerSO.ApplyModifiedProperties();

                        result = aimer;
                        break;
                    }
                case ModularFirearmAimingStep.AimerModule.ScopedAimer:
                    {
                        var aimer = rootObject.AddComponent<ScopedAimer>();
                        var aimerSO = new SerializedObject(aimer);

                        aimerSO.FindProperty("m_AimUpAudio").objectReferenceValue = aimingStep.aimUpAudio;
                        aimerSO.FindProperty("m_AimDownAudio").objectReferenceValue = aimingStep.aimDownAudio;
                        aimerSO.FindProperty("m_HipAccuracyCap").floatValue = aimingStep.hipAccuracyCap;
                        aimerSO.FindProperty("m_AimedAccuracyCap").floatValue = aimingStep.aimedAccuracyCap;
                        aimerSO.FindProperty("m_CanAimWhileReloading").boolValue = aimingStep.canAimWhileReloading;
                        aimerSO.FindProperty("m_HudScopeKey").stringValue = aimingStep.hudScopeKey;
                        aimerSO.FindProperty("m_FovMultiplier").floatValue = aimingStep.fovMultiplier;
                        aimerSO.FindProperty("m_AimTime").floatValue = aimingStep.aimTime;
                        aimerSO.FindProperty("m_PositionSpringMultiplier").floatValue = aimingStep.positionSpringMultiplier;
                        aimerSO.FindProperty("m_RotationSpringMultiplier").floatValue = aimingStep.rotationSpringMultiplier;
                        aimerSO.FindProperty("m_BlockTrigger").boolValue = aimingStep.blockTrigger;
                        aimerSO.FindProperty("m_CrosshairDown").FindPropertyRelative("m_Value").intValue = aimingStep.crosshairHipFire;

                        switch (aimingStep.aimPositionOption)
                        {
                            case 0: // anchor
                                if (aimingStep.aimTarget != null)
                                {
                                    ScopedAimerEditor.BuildAimOffsetFromTransform(
                                        aimerSO.FindProperty("m_AimOffset"),
                                        rootObject.transform.transform,
                                        WizardUtility.GetRelativeComponent(root.viewModel, displayObject, aimingStep.aimTarget.transform)
                                        );
                                }
                                break;
                            case 1: // offsets
                                aimerSO.FindProperty("m_AimOffset").vector3Value = aimingStep.aimPosition;
                                break;
                        }

                        if (customAnimations)
                        {
                            var animations = steps[ModularFirearmWizardSteps.animations] as ModularFirearmAnimationsStep;
                            aimerSO.FindProperty("m_AimAnimBool").stringValue = animations.aimBool;
                        }

                        aimerSO.ApplyModifiedProperties();

                        result = aimer;
                        break;
                    }
                case ModularFirearmAimingStep.AimerModule.InstantScopedAimer:
                    {
                        var aimer = rootObject.AddComponent<InstantScopedAimer>();
                        var aimerSO = new SerializedObject(aimer);

                        aimerSO.FindProperty("m_AimUpAudio").objectReferenceValue = aimingStep.aimUpAudio;
                        aimerSO.FindProperty("m_AimDownAudio").objectReferenceValue = aimingStep.aimDownAudio;
                        aimerSO.FindProperty("m_HipAccuracyCap").floatValue = aimingStep.hipAccuracyCap;
                        aimerSO.FindProperty("m_AimedAccuracyCap").floatValue = aimingStep.aimedAccuracyCap;
                        aimerSO.FindProperty("m_CanAimWhileReloading").boolValue = aimingStep.canAimWhileReloading;
                        aimerSO.FindProperty("m_HudScopeKey").stringValue = aimingStep.hudScopeKey;
                        aimerSO.FindProperty("m_FovMultiplier").floatValue = aimingStep.fovMultiplier;
                        aimerSO.FindProperty("m_PositionSpringMultiplier").floatValue = aimingStep.positionSpringMultiplier;
                        aimerSO.FindProperty("m_RotationSpringMultiplier").floatValue = aimingStep.rotationSpringMultiplier;
                        aimerSO.FindProperty("m_CrosshairDown").FindPropertyRelative("m_Value").intValue = aimingStep.crosshairHipFire;
                        
                        aimerSO.ApplyModifiedProperties();

                        result = aimer;
                        break;
                    }
                case ModularFirearmAimingStep.AimerModule.HeadMoveAimer:
                    {
                        var aimer = rootObject.AddComponent<HeadMoveAimer>();
                        var aimerSO = new SerializedObject(aimer);

                        aimerSO.FindProperty("m_AimUpAudio").objectReferenceValue = aimingStep.aimUpAudio;
                        aimerSO.FindProperty("m_AimDownAudio").objectReferenceValue = aimingStep.aimDownAudio;
                        aimerSO.FindProperty("m_HipAccuracyCap").floatValue = aimingStep.hipAccuracyCap;
                        aimerSO.FindProperty("m_AimedAccuracyCap").floatValue = aimingStep.aimedAccuracyCap;
                        aimerSO.FindProperty("m_CanAimWhileReloading").boolValue = aimingStep.canAimWhileReloading;
                        aimerSO.FindProperty("m_FovMultiplier").floatValue = aimingStep.fovMultiplier;
                        aimerSO.FindProperty("m_AimTime").floatValue = aimingStep.aimTime;
                        aimerSO.FindProperty("m_PositionSpringMultiplier").floatValue = aimingStep.positionSpringMultiplier;
                        aimerSO.FindProperty("m_RotationSpringMultiplier").floatValue = aimingStep.rotationSpringMultiplier;
                        aimerSO.FindProperty("m_BlockTrigger").boolValue = aimingStep.blockTrigger;
                        aimerSO.FindProperty("m_CrosshairUp").FindPropertyRelative("m_Value").intValue = aimingStep.crosshairAiming;
                        aimerSO.FindProperty("m_CrosshairDown").FindPropertyRelative("m_Value").intValue = aimingStep.crosshairHipFire;

                        switch (aimingStep.aimPositionOption)
                        {
                            case 0: // anchor
                                if (aimingStep.aimTarget != null)
                                {
                                    HeadMoveAimerEditor.BuildAimOffsetFromTransform(
                                        aimerSO.FindProperty("m_AimPositionOffset"),
                                        aimerSO.FindProperty("m_AimRotationOffset"),
                                        rootObject.transform.transform,
                                        WizardUtility.GetRelativeComponent(root.viewModel, displayObject, aimingStep.aimTarget.transform)
                                        );
                                }
                                break;
                            case 1: // offsets
                                aimerSO.FindProperty("m_AimPositionOffset").vector3Value = aimingStep.aimPosition;
                                aimerSO.FindProperty("m_AimRotationOffset").vector3Value = aimingStep.aimRotation;
                                break;
                        }


                        if (customAnimations)
                        {
                            var animations = steps[ModularFirearmWizardSteps.animations] as ModularFirearmAnimationsStep;
                            aimerSO.FindProperty("m_AimAnimBool").stringValue = animations.aimBool;
                        }
                        
                        aimerSO.ApplyModifiedProperties();

                        result = aimer;
                        break;
                    }
                case ModularFirearmAimingStep.AimerModule.AnimationOnlyAimer:
                    {
                        var aimer = rootObject.AddComponent<AnimOnlyAimer>();
                        var aimerSO = new SerializedObject(aimer);

                        aimerSO.FindProperty("m_AimUpAudio").objectReferenceValue = aimingStep.aimUpAudio;
                        aimerSO.FindProperty("m_AimDownAudio").objectReferenceValue = aimingStep.aimDownAudio;
                        aimerSO.FindProperty("m_HipAccuracyCap").floatValue = aimingStep.hipAccuracyCap;
                        aimerSO.FindProperty("m_AimedAccuracyCap").floatValue = aimingStep.aimedAccuracyCap;
                        aimerSO.FindProperty("m_CanAimWhileReloading").boolValue = aimingStep.canAimWhileReloading;
                        aimerSO.FindProperty("m_AimTime").floatValue = aimingStep.aimTime;
                        aimerSO.FindProperty("m_BlockTrigger").boolValue = aimingStep.blockTrigger;

                        if (customAnimations)
                        {
                            var animations = steps[ModularFirearmWizardSteps.animations] as ModularFirearmAnimationsStep;
                            aimerSO.FindProperty("m_AimAnimBool").stringValue = animations.aimBool;
                        }

                        aimerSO.ApplyModifiedProperties();

                        result = aimer;
                        break;
                    }
            }

            if (root.aimingFatigue)
            {
                var fatigue = rootObject.AddComponent<FirearmAimFatigue>();
                var fatigueSO = new SerializedObject(fatigue);

                fatigueSO.FindProperty("m_StaminaLoss").floatValue = root.staminaLoss;
                fatigueSO.FindProperty("m_StaminaTarget").floatValue = root.staminaTarget;
                fatigueSO.FindProperty("m_StaminaFalloff").floatValue = root.staminaFalloff;
                
                fatigueSO.ApplyModifiedProperties();
            }

            return result;
        }

        IMuzzleEffect AddMuzzleEffect(GameObject rootObject, GameObject displayObject, GameObject componentObject)
        {
            var root = steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            var muzzleEffectStep = steps[ModularFirearmWizardSteps.muzzleEffect] as ModularFirearmMuzzleEffectStep;

            switch (muzzleEffectStep.muzzleEffectModule)
            {
                case ModularFirearmMuzzleEffectStep.MuzzleEffectModule.BasicGameObject:
                    {
                        var effect = rootObject.AddComponent<BasicGameObjectMuzzleEffect>();
                        var effectSO = new SerializedObject(effect);

                        switch (muzzleEffectStep.muzzleEffectObjectType)
                        {
                            case 0: // Prefab
                                {
                                    var muzzleFlash = Instantiate(muzzleEffectStep.muzzleFlashPrefab);
                                    muzzleFlash.transform.SetParent(componentObject.transform, false);
                                    effectSO.FindProperty("m_MuzzleFlash").objectReferenceValue = muzzleFlash;
                                }
                                break;
                            case 1: // Child Object
                                effectSO.FindProperty("m_MuzzleFlash").objectReferenceValue = WizardUtility.GetRelativeGameObject(root.viewModel, displayObject, muzzleEffectStep.muzzleFlashObject);
                                break;
                            case 2: // Mesh
                                {
                                    var muzzleFlash = new GameObject("MuzzleFlash");
                                    muzzleFlash.transform.SetParent(componentObject.transform, false);
                                    var meshFilter = muzzleFlash.AddComponent<MeshFilter>();
                                    meshFilter.mesh = muzzleEffectStep.muzzleFlashMesh;
                                    var meshRenderer = muzzleFlash.AddComponent<MeshRenderer>();
                                    meshRenderer.sharedMaterial = muzzleEffectStep.muzzleFlashMaterial;
                                    effectSO.FindProperty("m_MuzzleFlash").objectReferenceValue = muzzleFlash;
                                }
                                break;
                        }

                        effectSO.FindProperty("m_MuzzleFlashDuration").floatValue = muzzleEffectStep.muzzleFlashDuration;

                        var audioProp = effectSO.FindProperty("m_FiringSounds");
                        audioProp.arraySize = muzzleEffectStep.firingSounds.Length;
                        for (int i = 0; i < muzzleEffectStep.firingSounds.Length; ++i)
                            audioProp.GetArrayElementAtIndex(i).objectReferenceValue = muzzleEffectStep.firingSounds[i];

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmMuzzleEffectStep.MuzzleEffectModule.RandomObject:
                    {
                        var effect = rootObject.AddComponent<RandomObjectMuzzleEffect>();
                        var effectSO = new SerializedObject(effect);

                        switch (muzzleEffectStep.muzzleEffectObjectType)
                        {
                            case 0: // Prefab
                                {
                                    for (int i = 0; i < muzzleEffectStep.muzzleFlashPrefabs.Length; ++i)
                                    {
                                        var muzzleFlash = Instantiate(muzzleEffectStep.muzzleFlashPrefabs[i]);
                                        muzzleFlash.transform.SetParent(componentObject.transform, false);
                                        SerializedArrayUtility.Add(effectSO.FindProperty("m_MuzzleFlashes"), muzzleFlash, true);
                                    }
                                }
                                break;
                            case 1: // Child Object
                                {
                                    for (int i = 0; i < muzzleEffectStep.muzzleFlashObjects.Length; ++i)
                                        SerializedArrayUtility.Add(effectSO.FindProperty("m_MuzzleFlashes"), WizardUtility.GetRelativeGameObject(root.viewModel, displayObject, muzzleEffectStep.muzzleFlashObjects[i]), true);
                                }
                                break;
                            case 2: // Mesh
                                {
                                    for (int i = 0; i < muzzleEffectStep.muzzleFlashMeshes.Length; ++i)
                                    {
                                        var muzzleFlash = new GameObject("MuzzleFlash");
                                        muzzleFlash.transform.SetParent(componentObject.transform, false);
                                        var meshFilter = muzzleFlash.AddComponent<MeshFilter>();
                                        meshFilter.mesh = muzzleEffectStep.muzzleFlashMeshes[i];
                                        var meshRenderer = muzzleFlash.AddComponent<MeshRenderer>();
                                        meshRenderer.sharedMaterial = muzzleEffectStep.muzzleFlashMaterial;
                                        SerializedArrayUtility.Add(effectSO.FindProperty("m_MuzzleFlashes"), muzzleFlash, true);
                                    }
                                }
                                break;
                        }

                        effectSO.FindProperty("m_MuzzleFlashDuration").floatValue = muzzleEffectStep.muzzleFlashDuration;

                        var audioProp = effectSO.FindProperty("m_FiringSounds");
                        audioProp.arraySize = muzzleEffectStep.firingSounds.Length;
                        for (int i = 0; i < muzzleEffectStep.firingSounds.Length; ++i)
                            audioProp.GetArrayElementAtIndex(i).objectReferenceValue = muzzleEffectStep.firingSounds[i];

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmMuzzleEffectStep.MuzzleEffectModule.ParticleSystem:
                    {
                        var effect = rootObject.AddComponent<SimpleParticleMuzzleEffect>();
                        var effectSO = new SerializedObject(effect);

                        // Get particle system
                        if (muzzleEffectStep.particleSystemOption == 0)
                        {
                            var muzzleEffectObject = Instantiate(muzzleEffectStep.particleSystem);
                            effectSO.FindProperty("m_ParticleSystem").objectReferenceValue = muzzleEffectObject;
                            muzzleEffectObject.transform.SetParent(componentObject.transform, false);
                        }
                        else
                            effectSO.FindProperty("m_ParticleSystem").objectReferenceValue = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, muzzleEffectStep.particleSystem);

                        var audioProp = effectSO.FindProperty("m_FiringSounds");
                        audioProp.arraySize = muzzleEffectStep.firingSounds.Length;
                        for (int i = 0; i < muzzleEffectStep.firingSounds.Length; ++i)
                            audioProp.GetArrayElementAtIndex(i).objectReferenceValue = muzzleEffectStep.firingSounds[i];

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmMuzzleEffectStep.MuzzleEffectModule.AdvancedParticleSystem:
                    {
                        GameObject muzzleEffectObject = null;
                        if (muzzleEffectStep.advancedParticleSystemPrefab != null)
                        {
                            muzzleEffectObject = Instantiate(muzzleEffectStep.advancedParticleSystemPrefab);
                            muzzleEffectObject.transform.SetParent(componentObject.transform, false);

                            var advEffect = muzzleEffectObject.GetComponent<AdvancedParticleMuzzleEffect>();
                            if (advEffect != null)
                                return advEffect;
                        }

                        var effect = rootObject.AddComponent<AdvancedParticleMuzzleEffect>();
                        var effectSO = new SerializedObject(effect);

                        if (muzzleEffectObject == null)
                        {
                            muzzleEffectObject = new GameObject("MuzzleFlash");
                            muzzleEffectObject.transform.SetParent(componentObject.transform, false);

                            var particleObject = new GameObject("ExampleParticleSystem");
                            muzzleEffectObject.transform.SetParent(muzzleEffectObject.transform, false);
                            var particleSystem = particleObject.AddComponent<ParticleSystem>();

                            var effectsArray = effectSO.FindProperty("m_ParticleSystems");
                            effectsArray.arraySize = 1;
                            var effectProp = effectsArray.GetArrayElementAtIndex(0);
                            effectProp.FindPropertyRelative("particleSystem").objectReferenceValue = particleSystem;
                            effectProp.FindPropertyRelative("space").enumValueIndex = 0;
                        }
                        else
                        {
                            var effectsArray = effectSO.FindProperty("m_ParticleSystems");
                            effectsArray.arraySize = muzzleEffectStep.particleSystems.Length;
                            for (int i = 0; i < muzzleEffectStep.particleSystems.Length; ++i)
                            {
                                var effectProp = effectsArray.GetArrayElementAtIndex(i);
                                effectProp.FindPropertyRelative("particleSystem").objectReferenceValue = WizardUtility.GetRelativeComponent(
                                    muzzleEffectStep.advancedParticleSystemPrefab,
                                    muzzleEffectObject,
                                    muzzleEffectStep.particleSystems[i].particleSystem);
                                effectProp.FindPropertyRelative("space").enumValueIndex = (int)muzzleEffectStep.particleSystems[i].space;
                            }
                        }

                        effectSO.FindProperty("m_EffectTransform").objectReferenceValue = muzzleEffectObject.transform;
                        effectSO.FindProperty("m_FollowDuration").floatValue = muzzleEffectStep.followDuration;

                        var audioProp = effectSO.FindProperty("m_FiringSounds");
                        audioProp.arraySize = muzzleEffectStep.firingSounds.Length;
                        for (int i = 0; i < muzzleEffectStep.firingSounds.Length; ++i)
                            audioProp.GetArrayElementAtIndex(i).objectReferenceValue = muzzleEffectStep.firingSounds[i];

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
                case ModularFirearmMuzzleEffectStep.MuzzleEffectModule.AudioOnly:
                    {
                        var effect = rootObject.AddComponent<AudioOnlyMuzzleEffect>();
                        var effectSO = new SerializedObject(effect);

                        var audioProp = effectSO.FindProperty("m_FiringSounds");
                        audioProp.arraySize = muzzleEffectStep.firingSounds.Length;
                        for (int i = 0; i < muzzleEffectStep.firingSounds.Length; ++i)
                            audioProp.GetArrayElementAtIndex(i).objectReferenceValue = muzzleEffectStep.firingSounds[i];

                        effectSO.ApplyModifiedPropertiesWithoutUndo();

                        return effect;
                    }
            }

            return null;
        }

        IEjector AddShellEject(GameObject rootObject, GameObject displayObject, GameObject componentObject)
        {
            var root = steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            var shellEjectStep = steps[ModularFirearmWizardSteps.shellEject] as ModularFirearmShellEjectStep;

            switch (shellEjectStep.shellEjectModule)
            {
                case ModularFirearmShellEjectStep.ShellEjectModule.Standard:
                    {
                        var ejector = rootObject.AddComponent<StandardShellEject>();
                        var ejectorSO = new SerializedObject(ejector);

                        if (shellEjectStep.shellEjectPoint != null)
                        {
                            ejectorSO.FindProperty("m_ShellEjectProxy").objectReferenceValue = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, shellEjectStep.shellEjectPoint.transform);
                        }
                        else
                        {
                            var proxy = new GameObject("ShellEjectPoint");
                            proxy.transform.SetParent(componentObject.transform, false);
                            ejectorSO.FindProperty("m_ShellEjectProxy").objectReferenceValue = proxy.transform;
                        }

                        ejectorSO.FindProperty("m_ShellPrefab").objectReferenceValue = shellEjectStep.shellPrefab;
                        ejectorSO.FindProperty("m_Delay").floatValue = shellEjectStep.delay;
                        ejectorSO.FindProperty("m_OutSpeed").floatValue = shellEjectStep.outSpeed;
                        ejectorSO.FindProperty("m_BackSpeed").floatValue = shellEjectStep.backSpeed;
                        ejectorSO.FindProperty("m_InheritVelocity").floatValue = shellEjectStep.inheritVelocity;
                        
                        ejectorSO.ApplyModifiedPropertiesWithoutUndo();

                        return ejector;
                    }
                case ModularFirearmShellEjectStep.ShellEjectModule.ParticleSystem:
                    {
                        var ejector = rootObject.AddComponent<ParticleSystemShellEject>();
                        var ejectorSO = new SerializedObject(ejector);

                        var psRoot = Instantiate(shellEjectStep.particleSystemsPrefab);
                        psRoot.transform.SetParent(componentObject.transform, false);
                        var particleSystems = psRoot.GetComponentsInChildren<ParticleSystem>();
                        if (particleSystems == null || particleSystems.Length == 0)
                        {
                            psRoot.AddComponent<ParticleSystem>();
                            particleSystems = psRoot.GetComponents<ParticleSystem>();
                        }

                        var arrayProp = ejectorSO.FindProperty("m_ParticleSystems");
                        arrayProp.arraySize = particleSystems.Length;
                        for (int i = 0; i < particleSystems.Length; ++i)
                            arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = particleSystems[i];

                        ejectorSO.ApplyModifiedPropertiesWithoutUndo();

                        return ejector;
                    }
                case ModularFirearmShellEjectStep.ShellEjectModule.ObjectSwap:
                    {
                        var ejector = rootObject.AddComponent<ObjectSwapEjector>();
                        var ejectorSO = new SerializedObject(ejector);

                        ejectorSO.FindProperty("m_TargetTransform").objectReferenceValue = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, shellEjectStep.objectToReplace.transform);
                        ejectorSO.FindProperty("m_ShellPrefab").objectReferenceValue = shellEjectStep.shellPrefab;
                        ejectorSO.FindProperty("m_EjectOnFire").boolValue = shellEjectStep.ejectOnFire;
                        ejectorSO.FindProperty("m_Delay").floatValue = shellEjectStep.delay;

                        ejectorSO.ApplyModifiedPropertiesWithoutUndo();

                        return ejector;
                    }
                case ModularFirearmShellEjectStep.ShellEjectModule.MultiObjectSwap:
                    {
                        var ejector = rootObject.AddComponent<MultiObjectSwapEjector>();
                        var ejectorSO = new SerializedObject(ejector);

                        var arrayProp = ejectorSO.FindProperty("m_TargetTransforms");
                        arrayProp.arraySize = shellEjectStep.objectsToReplace.Length;
                        for (int i = 0; i < shellEjectStep.objectsToReplace.Length; ++i)
                        {
                            arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = WizardUtility.GetRelativeComponent(root.viewModel, displayObject, shellEjectStep.objectsToReplace[i].transform);
                        }

                        ejectorSO.FindProperty("m_ShellPrefab").objectReferenceValue = shellEjectStep.shellPrefab;
                        ejectorSO.FindProperty("m_EjectOnFire").boolValue = shellEjectStep.ejectOnFire;
                        ejectorSO.FindProperty("m_SwapInactive").boolValue = shellEjectStep.swapInactive;
                        ejectorSO.FindProperty("m_Delay").floatValue = shellEjectStep.delay;

                        ejectorSO.ApplyModifiedPropertiesWithoutUndo();

                        return ejector;
                    }
            }

            return null;
        }
    }
}
 