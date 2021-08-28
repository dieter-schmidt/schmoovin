using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPSEditor.Hub.Pages.ItemCreationWizards.ThrownWeapons;
using NeoFPS;
using UnityEditor.Animations;
using NeoSaveGames.Serialization;
using UnityEngine.Audio;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public class ThrownWeaponWizard : NeoFpsWizard
    {
        static readonly string[] k_RootSteps = new string[] { ThrownWeaponWizardSteps.root, ThrownWeaponWizardSteps.viewModel, ThrownWeaponWizardSteps.audio };

        protected override string[] GetRootSteps()
        {
            return k_RootSteps;
        }

        public override string GetDefaultTemplateFilename()
        {
            return "NewThrownWeaponTemplate";
        }

        protected override void RegisterSteps()
        {
            RegisterStep<ThrownWeaponRootStep>(ThrownWeaponWizardSteps.root);
            RegisterStep<ThrownWeaponViewModelStep>(ThrownWeaponWizardSteps.viewModel);
            RegisterStep<ThrownWeaponExistingAnimatorStep>(ThrownWeaponWizardSteps.existingAnim);
            RegisterStep<ThrownWeaponNewAnimatorStep>(ThrownWeaponWizardSteps.createAnim);
            RegisterStep<ThrownWeaponAudioStep>(ThrownWeaponWizardSteps.audio);
        }

        public override void CreateItem()
        {
            // Get the save folder
            var folderPath = WizardUtility.GetPrefabOutputFolder();
            if (folderPath == null)
                return;

            var root = steps[ThrownWeaponWizardSteps.root] as ThrownWeaponRootStep;
            var viewModel = steps[ThrownWeaponWizardSteps.viewModel] as ThrownWeaponViewModelStep;
            var audio = steps[ThrownWeaponWizardSteps.audio] as ThrownWeaponAudioStep;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
                prefabName = "ThrownWeapon_" + prefabName;

            // Get prefab path
            var path = WizardUtility.GetPrefabPath(prefabName, folderPath, root.overwriteExisting);

            // Create root object
            GameObject rootObject = new GameObject(prefabName);
            var audioSource = WizardUtility.AddAudioSource(rootObject);

            // Melee setup
            var thrownWeapon = rootObject.AddComponent<ThrownWeapon>();
            SerializedObject thrownWeaponSO = new SerializedObject(thrownWeapon);

            thrownWeaponSO.FindProperty("m_SpawnedProjectile").objectReferenceValue = root.spawnedProjectile;
            thrownWeaponSO.FindProperty("m_InheritVelocity").floatValue = root.inheritVelocity;
            thrownWeaponSO.FindProperty("m_SpawnTimeWeak").floatValue = root.spawnTimeLight;
            thrownWeaponSO.FindProperty("m_SpawnTimeStrong").floatValue = root.spawnTimeHeavy;
            thrownWeaponSO.FindProperty("m_ThrowSpeedWeak").floatValue = root.throwSpeedLight;
            thrownWeaponSO.FindProperty("m_ThrowSpeedStrong").floatValue = root.throwSpeedHeavy;
            thrownWeaponSO.FindProperty("m_ThrowDurationWeak").floatValue = root.throwDurationLight;
            thrownWeaponSO.FindProperty("m_ThrowDurationStrong").floatValue = root.throwDurationHeavy;
            thrownWeaponSO.FindProperty("m_Crosshair").FindPropertyRelative("m_Value").intValue = root.crosshair;

            thrownWeaponSO.FindProperty("m_AudioSelect").objectReferenceValue = audio.selectAudio;
            thrownWeaponSO.FindProperty("m_AudioThrowLight").objectReferenceValue = audio.lightThrowAudio;
            thrownWeaponSO.FindProperty("m_AudioThrowHeavy").objectReferenceValue = audio.heavyThrowAudio;
            
            // Stance and sprint animation handlers
            rootObject.AddComponent<ThrownWieldableStanceManager>();
            BaseSprintAnimationHandler sprintHandler = null;
            switch (viewModel.sprintAnimations)
            {
                case 1:
                    sprintHandler = rootObject.AddComponent<ProceduralThrownSprintHandler>();
                    break;
                case 2:
                    sprintHandler = rootObject.AddComponent<AnimatedThrownSprintHandler>();
                    break;
            }

            // Set up spring animation
            GameObject springObject = WizardUtility.AddWieldableSpringObject(rootObject);

            // Set up animation
            AnimatorController animatorController = null;
            switch (viewModel.animatorControllerSetup)
            {
                case 0: // Use existing
                    {
                        var animSetup = steps[ThrownWeaponWizardSteps.existingAnim] as ThrownWeaponExistingAnimatorStep;
                        animatorController = animSetup.animatorController;

                        thrownWeaponSO.FindProperty("m_AnimKeyDraw").stringValue = animSetup.drawAnimatorTrigger;
                        if (!string.IsNullOrEmpty(animSetup.drawAnimatorTrigger))
                            thrownWeaponSO.FindProperty("m_DrawDuration").floatValue = animSetup.raiseDuration;
                        else
                            thrownWeaponSO.FindProperty("m_DrawDuration").floatValue = 0f;

                        thrownWeaponSO.FindProperty("m_AnimKeyLower").stringValue = animSetup.lowerAnimatorTrigger;
                        if (!string.IsNullOrEmpty(animSetup.lowerAnimatorTrigger))
                            thrownWeaponSO.FindProperty("m_LowerDuration").floatValue = animSetup.lowerDuration;
                        else
                            thrownWeaponSO.FindProperty("m_LowerDuration").floatValue = 0f;

                        thrownWeaponSO.FindProperty("m_AnimKeyLightThrow").stringValue = animSetup.lightThrowAnimatorTrigger;
                        thrownWeaponSO.FindProperty("m_AnimKeyHeavyThrow").stringValue = animSetup.heavyThrowAnimatorTrigger;

                        if (viewModel.sprintAnimations == 2)
                        {
                            var sprintSO = new SerializedObject(sprintHandler);
                            sprintSO.FindProperty("m_SprintBoolParameter").stringValue = animSetup.sprintAnimatorBoolParameter;
                            sprintSO.FindProperty("m_SpeedFloatParameter").stringValue = animSetup.sprintSpeedAnimatorFloatParameter;
                            sprintSO.FindProperty("m_BlendFloatParameter").stringValue = animSetup.sprintBlendAnimatorFloatParameter;
                            sprintSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                    }
                    break;

                case 1: // None
                    {

                    }
                    break;

                case 2: // Create new animator
                    {
                        var animSetup = steps[ThrownWeaponWizardSteps.createAnim] as ThrownWeaponNewAnimatorStep;

                        // Get prefab path
                        var animControllerPath = string.Format("{0}/{1}_AnimatorController.controller", folderPath, prefabName);
                        animatorController = AnimatorController.CreateAnimatorControllerAtPath(animControllerPath);
                        var rootStateMachine = animatorController.layers[0].stateMachine;

                        // Idle
                        var idleState = rootStateMachine.AddState("Idle");
                        idleState.motion = animSetup.idleAnimation;

                        // Light Throw
                        if (animSetup.lightThrowAnimation != null)
                        {
                            animatorController.AddParameter("ThrowLight", AnimatorControllerParameterType.Trigger);
                            var state = rootStateMachine.AddState("Light Throw");
                            state.motion = animSetup.lightThrowAnimation;

                            var entry = rootStateMachine.AddAnyStateTransition(state);
                            entry.AddCondition(AnimatorConditionMode.If, 0f, "ThrowLight");
                            entry.hasExitTime = false;
                            var exit = state.AddTransition(idleState);
                            exit.hasExitTime = true;
                            exit.hasFixedDuration = true;
                            exit.duration = 0.25f;
                            exit.exitTime = WizardUtility.GetExitTime(animSetup.lightThrowAnimation, 0.25f);
                        }
                        else
                            thrownWeaponSO.FindProperty("m_TriggerAttack").stringValue = string.Empty;

                        // Heavy Throw
                        if (animSetup.heavyThrowAnimation != null)
                        {
                            animatorController.AddParameter("ThrowHeavy", AnimatorControllerParameterType.Trigger);
                            var state = rootStateMachine.AddState("Heavy Throw");
                            state.motion = animSetup.heavyThrowAnimation;

                            var entry = rootStateMachine.AddAnyStateTransition(state);
                            entry.AddCondition(AnimatorConditionMode.If, 0f, "ThrowHeavy");
                            entry.hasExitTime = false;
                            var exit = state.AddTransition(idleState);
                            exit.hasExitTime = true;
                            exit.hasFixedDuration = true;
                            exit.duration = 0.25f;
                            exit.exitTime = WizardUtility.GetExitTime(animSetup.heavyThrowAnimation, 0.25f);
                        }
                        else
                            thrownWeaponSO.FindProperty("m_TriggerAttackHit").stringValue = string.Empty;

                        // Draw weapon
                        if (animSetup.drawWeaponAnimation != null)
                        {
                            animatorController.AddParameter("Draw", AnimatorControllerParameterType.Trigger);
                            thrownWeaponSO.FindProperty("m_DrawDuration").floatValue = animSetup.raiseDuration;

                            var state = rootStateMachine.AddState("Draw");
                            state.motion = animSetup.drawWeaponAnimation;
                            rootStateMachine.defaultState = state;

                            var entry = rootStateMachine.AddAnyStateTransition(state);
                            entry.AddCondition(AnimatorConditionMode.If, 0f, "Draw");
                            entry.hasExitTime = false;
                            entry.hasFixedDuration = true;
                            entry.duration = 0f;
                            var exit = state.AddTransition(idleState);
                            exit.hasExitTime = true;
                            exit.hasFixedDuration = true;
                            exit.duration = 0.25f;
                            exit.exitTime = WizardUtility.GetExitTime(animSetup.drawWeaponAnimation, 0.25f);
                        }
                        else
                        {
                            thrownWeaponSO.FindProperty("m_AnimKeyDraw").stringValue = string.Empty;
                            thrownWeaponSO.FindProperty("m_DrawDuration").floatValue = 0f;
                        }

                        // Lower weapon
                        if (animSetup.lowerWeaponAnimation != null)
                        {
                            animatorController.AddParameter("Lower", AnimatorControllerParameterType.Trigger);
                            thrownWeaponSO.FindProperty("m_AnimKeyLower").stringValue = "Lower";
                            thrownWeaponSO.FindProperty("m_LowerDuration").floatValue = animSetup.lowerDuration;

                            var state = rootStateMachine.AddState("Lower");
                            state.motion = animSetup.lowerWeaponAnimation;

                            var entry = rootStateMachine.AddAnyStateTransition(state);
                            entry.AddCondition(AnimatorConditionMode.If, 0f, "Lower");
                            entry.hasExitTime = false;
                            entry.hasFixedDuration = true;
                            entry.duration = 0f;
                            var exit = state.AddTransition(idleState);
                            exit.hasExitTime = true;
                            exit.hasFixedDuration = true;
                            exit.duration = 0f;
                            exit.exitTime = 1f;
                        }

                        // Sprinting
                        if (viewModel.sprintAnimations == 2)
                        {
                            animatorController.AddParameter("Sprint", AnimatorControllerParameterType.Bool);
                            var speedParameter = new AnimatorControllerParameter();
                            speedParameter.defaultFloat = 1f;
                            speedParameter.name = "SprintSpeed";
                            speedParameter.type = AnimatorControllerParameterType.Float;
                            animatorController.AddParameter(speedParameter);

                            // Is sprint a state or blend tree
                            AnimatorState sprintState = null;
                            if (animSetup.sprintFastAnimation != null)
                            {
                                // Add sprint speed parameter
                                animatorController.AddParameter("SprintBlend", AnimatorControllerParameterType.Float);

                                // Add sprint blend tree
                                sprintState = rootStateMachine.AddState("Sprint");
                                var blendTree = new BlendTree();
                                sprintState.motion = blendTree;
                                sprintState.speedParameter = "SprintSpeed";
                                blendTree.blendType = BlendTreeType.Simple1D;
                                blendTree.blendParameter = "SprintBlend";
                                blendTree.AddChild(animSetup.sprintAnimation, 0f);
                                blendTree.AddChild(animSetup.sprintFastAnimation, 1f);

                                var sprintSO = new SerializedObject(sprintHandler);
                                sprintSO.FindProperty("m_UnscaledSprintMoveSpeed").floatValue = animSetup.sprintClipSpeed;
                                sprintSO.ApplyModifiedPropertiesWithoutUndo();
                            }
                            else
                            {
                                // Add sprint state
                                sprintState = rootStateMachine.AddState("Sprint");
                                sprintState.motion = animSetup.sprintAnimation;
                                sprintState.speedParameter = "SprintSpeed";

                                // Sort handler
                                var sprintSO = new SerializedObject(sprintHandler);
                                sprintSO.FindProperty("m_BlendFloatParameter").stringValue = string.Empty;
                                sprintSO.FindProperty("m_UnscaledSprintMoveSpeed").floatValue = animSetup.sprintClipSpeed;
                                sprintSO.ApplyModifiedPropertiesWithoutUndo();
                            }

                            // Sort transitions
                            var entry = idleState.AddTransition(sprintState);
                            var exit = sprintState.AddTransition(idleState);
                            entry.AddCondition(AnimatorConditionMode.If, 0f, "Sprint");
                            entry.hasExitTime = false;
                            exit.AddCondition(AnimatorConditionMode.IfNot, 0f, "Sprint");
                            exit.hasExitTime = false;
                        }
                    }
                    break;
            }

            // Input
            rootObject.AddComponent<InputThrownWeapon>();

            // Inventory
            root.AddInventoryToObject(rootObject);

            // Save Games
            rootObject.AddComponent<NeoSerializedGameObject>();
            springObject.AddComponent<NeoSerializedGameObject>();

            // View Model
            var viewModelObject = Instantiate(viewModel.renderGeometry);
            viewModelObject.transform.SetParent(springObject.transform, false);
            var vmAnimator = viewModelObject.GetComponentInChildren<Animator>();
            if (vmAnimator != null)
            {
                // Assign animator controller
                if (animatorController != null)
                {
                    var vmAnimatorSO = new SerializedObject(vmAnimator);
                    vmAnimatorSO.FindProperty("m_Controller").objectReferenceValue = animatorController;
                    vmAnimatorSO.ApplyModifiedPropertiesWithoutUndo();
                }

                // Add animator to save system
                var vmSave = viewModelObject.AddComponent<NeoSerializedGameObject>();
                var vmSaveSO = new SerializedObject(vmSave);
                SerializedArrayUtility.Add(vmSaveSO.FindProperty("m_OtherComponents"), vmAnimator, true);
                vmSaveSO.ApplyModifiedPropertiesWithoutUndo();
                
                thrownWeaponSO.FindProperty("m_Animator").objectReferenceValue = vmAnimator;
            }

            // View model held item
            if (viewModel.heldItem != null)
            {
                thrownWeaponSO.FindProperty("m_HeldObject").objectReferenceValue = WizardUtility.GetRelativeGameObject(viewModel.renderGeometry, viewModelObject, viewModel.heldItem);
            }

            // View model projectile spawn point (light)
            if (viewModel.projectileSpawnLight != null)
            {
                thrownWeaponSO.FindProperty("m_ProjectileSpawnPointWeak").objectReferenceValue = WizardUtility.GetRelativeGameObject(viewModel.renderGeometry, viewModelObject, viewModel.projectileSpawnLight);
            }
            else
            {
                var go = new GameObject("ProjectileSpawn_Light");
                go.transform.SetParent(viewModelObject.transform);
                go.transform.localPosition = new Vector3(0f, -0.1f, 0.1f);
                thrownWeaponSO.FindProperty("m_ProjectileSpawnPointWeak").objectReferenceValue = go.transform;
            }

            // View model projectile spawn point (heavy)
            if (viewModel.projectileSpawnLight != null)
            {
                thrownWeaponSO.FindProperty("m_ProjectileSpawnPointWeak").objectReferenceValue = WizardUtility.GetRelativeGameObject(viewModel.renderGeometry, viewModelObject, viewModel.projectileSpawnHeavy);
            }
            else
            {
                var go = new GameObject("ProjectileSpawn_Heavy");
                go.transform.SetParent(viewModelObject.transform);
                go.transform.localPosition = new Vector3(0f, -0.1f, 0.1f);
                thrownWeaponSO.FindProperty("m_ProjectileSpawnPointStrong").objectReferenceValue = go.transform;
            }

            thrownWeaponSO.ApplyModifiedPropertiesWithoutUndo();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            Debug.Log("Created Thrown Weapon");
        }
    }
}