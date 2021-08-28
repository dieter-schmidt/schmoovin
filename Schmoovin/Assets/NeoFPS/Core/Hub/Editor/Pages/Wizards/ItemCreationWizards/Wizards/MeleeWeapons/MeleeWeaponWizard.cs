using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPSEditor.Hub.Pages.ItemCreationWizards.MeleeWeapons;
using NeoFPS;
using UnityEngine.Audio;
using NeoSaveGames.Serialization;
using UnityEditor.Animations;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public class MeleeWeaponWizard : NeoFpsWizard
    {
        static readonly string[] k_RootSteps = new string[] { MeleeWeaponWizardSteps.root, MeleeWeaponWizardSteps.viewModel, MeleeWeaponWizardSteps.audio };

        protected override string[] GetRootSteps()
        {
            return k_RootSteps;
        }

        public override string GetDefaultTemplateFilename()
        {
            return "NewMeleeWeaponTemplate";
        }

        protected override void RegisterSteps()
        {
            RegisterStep<MeleeWeaponRootStep>(MeleeWeaponWizardSteps.root);
            RegisterStep<MeleeWeaponViewModelStep>(MeleeWeaponWizardSteps.viewModel);
            RegisterStep<MeleeWeaponExistingAnimatorStep>(MeleeWeaponWizardSteps.existingAnim);
            RegisterStep<MeleeWeaponNewAnimatorStep>(MeleeWeaponWizardSteps.createAnim);
            RegisterStep<MeleeWeaponAudioStep>(MeleeWeaponWizardSteps.audio);
        }

        public override void CreateItem()
        {
            // Get the save folder
            var folderPath = WizardUtility.GetPrefabOutputFolder();
            if (folderPath == null)
                return;

            var root = steps[MeleeWeaponWizardSteps.root] as MeleeWeaponRootStep;
            var viewModel = steps[MeleeWeaponWizardSteps.viewModel] as MeleeWeaponViewModelStep;
            var audio = steps[MeleeWeaponWizardSteps.audio] as MeleeWeaponAudioStep;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
                prefabName = "MeleeWeapon_" + prefabName;

            // Get prefab path
            var path = WizardUtility.GetPrefabPath(prefabName, folderPath, root.overwriteExisting);

            // Create root object
            GameObject rootObject = new GameObject(prefabName);
            var audioSource = WizardUtility.AddAudioSource(rootObject);

            // Melee setup
            var meleeWeapon = rootObject.AddComponent<MeleeWeapon>();
            SerializedObject meleeWeaponSO = new SerializedObject(meleeWeapon);
            meleeWeaponSO.FindProperty("m_Damage").floatValue = root.damage;
            meleeWeaponSO.FindProperty("m_ImpactForce").floatValue = root.force;
            meleeWeaponSO.FindProperty("m_Range").floatValue = root.range;
            meleeWeaponSO.FindProperty("m_Delay").floatValue = root.attackDelay;
            meleeWeaponSO.FindProperty("m_RecoverTime").floatValue = root.attackRecover;
            meleeWeaponSO.FindProperty("m_Crosshair").FindPropertyRelative("m_Value").intValue = root.crosshair;
            meleeWeaponSO.FindProperty("m_AudioSelect").objectReferenceValue = audio.selectAudio;
            meleeWeaponSO.FindProperty("m_AudioAttack").objectReferenceValue = audio.attackAudio;
            meleeWeaponSO.FindProperty("m_AudioBlockRaise").objectReferenceValue = audio.blockRaiseAudio;
            meleeWeaponSO.FindProperty("m_AudioBlockLower").objectReferenceValue = audio.blockLowerAudio;

            rootObject.AddComponent<MeleeWieldableStanceManager>();
            BaseSprintAnimationHandler sprintHandler = null;
            switch (viewModel.sprintAnimations)
            {
                case 1:
                    sprintHandler = rootObject.AddComponent<ProceduralMeleeSprintHandler>();
                    break;
                case 2:
                    sprintHandler = rootObject.AddComponent<AnimatedMeleeSprintHandler>();
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
                        var animSetup = steps[MeleeWeaponWizardSteps.existingAnim] as MeleeWeaponExistingAnimatorStep;
                        animatorController = animSetup.animatorController;

                        meleeWeaponSO.FindProperty("m_TriggerDraw").stringValue = animSetup.drawAnimatorTrigger;
                        if (!string.IsNullOrEmpty(animSetup.drawAnimatorTrigger))
                            meleeWeaponSO.FindProperty("m_RaiseDuration").floatValue = animSetup.raiseDuration;
                        meleeWeaponSO.FindProperty("m_TriggerLower").stringValue = animSetup.lowerAnimatorTrigger;
                        if (!string.IsNullOrEmpty(animSetup.lowerAnimatorTrigger))
                            meleeWeaponSO.FindProperty("m_LowerDuration").floatValue = animSetup.lowerDuration;
                        else
                            meleeWeaponSO.FindProperty("m_LowerDuration").floatValue = 0f;
                        meleeWeaponSO.FindProperty("m_TriggerAttack").stringValue = animSetup.attackAnimatorTrigger;
                        meleeWeaponSO.FindProperty("m_TriggerAttackHit").stringValue = animSetup.attackHitAnimatorTrigger;
                        meleeWeaponSO.FindProperty("m_BoolBlock").stringValue = animSetup.blockAnimatorBoolParameter;

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
                        var animSetup = steps[MeleeWeaponWizardSteps.createAnim] as MeleeWeaponNewAnimatorStep;

                        // Get prefab path
                        var animControllerPath = string.Format("{0}/{1}_AnimatorController.controller", folderPath, prefabName);
                        animatorController = AnimatorController.CreateAnimatorControllerAtPath(animControllerPath);
                        var rootStateMachine = animatorController.layers[0].stateMachine;

                        // Idle
                        var idleState = rootStateMachine.AddState("Idle");
                        idleState.motion = animSetup.idleAnimation;

                        // Attack
                        if (animSetup.attackAnimation != null)
                        {
                            animatorController.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
                            var state = rootStateMachine.AddState("Attack");
                            state.motion = animSetup.attackAnimation;

                            var entry = rootStateMachine.AddAnyStateTransition(state);
                            entry.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
                            entry.hasExitTime = false;
                            var exit = state.AddTransition(idleState);
                            exit.hasExitTime = true;
                            exit.hasFixedDuration = true;
                            exit.duration = 0.25f;
                            exit.exitTime = WizardUtility.GetExitTime(animSetup.attackAnimation, 0.25f);
                        }
                        else
                            meleeWeaponSO.FindProperty("m_TriggerAttack").stringValue = string.Empty;

                        // Attack Hit
                        if (animSetup.attackHitAnimation != null)
                        {
                            animatorController.AddParameter("AttackHit", AnimatorControllerParameterType.Trigger);
                            var state = rootStateMachine.AddState("Attack Hit");
                            state.motion = animSetup.attackHitAnimation;

                            var entry = rootStateMachine.AddAnyStateTransition(state);
                            entry.AddCondition(AnimatorConditionMode.If, 0f, "AttackHit");
                            entry.hasExitTime = false;
                            entry.hasFixedDuration = true;
                            entry.duration = 0f;
                            var exit = state.AddTransition(idleState);
                            exit.hasExitTime = true;
                            exit.hasFixedDuration = true;
                            exit.duration = 0.25f;
                            exit.exitTime = WizardUtility.GetExitTime(animSetup.attackHitAnimation, 0.25f);
                        }
                        else
                            meleeWeaponSO.FindProperty("m_TriggerAttackHit").stringValue = string.Empty;

                        // Draw weapon
                        if (animSetup.drawWeaponAnimation != null)
                        {
                            animatorController.AddParameter("Draw", AnimatorControllerParameterType.Trigger);
                            meleeWeaponSO.FindProperty("m_RaiseDuration").floatValue = animSetup.raiseDuration;

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
                            meleeWeaponSO.FindProperty("m_TriggerDraw").stringValue = string.Empty;
                            meleeWeaponSO.FindProperty("m_RaiseDuration").floatValue = 0f;
                        }

                        // Lower weapon
                        if (animSetup.lowerWeaponAnimation != null)
                        {
                            animatorController.AddParameter("Lower", AnimatorControllerParameterType.Trigger);
                            meleeWeaponSO.FindProperty("m_TriggerLower").stringValue = "Lower";
                            meleeWeaponSO.FindProperty("m_LowerDuration").floatValue = animSetup.lowerDuration;

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

                        // Block
                        if (animSetup.blockIdleAnimation != null)
                        {
                            animatorController.AddParameter("Block", AnimatorControllerParameterType.Bool);

                            if (animSetup.blockRaiseAnimation != null && animSetup.blockLowerAnimation != null)
                            {
                                AnimatorState blockEnterState = rootStateMachine.AddState("Block Raise");
                                AnimatorState blockIdleState = rootStateMachine.AddState("Block Idle");
                                AnimatorState blockExitState = rootStateMachine.AddState("Block Lower");
                                blockEnterState.motion = animSetup.blockRaiseAnimation;
                                blockIdleState.motion = animSetup.blockIdleAnimation;
                                blockExitState.motion = animSetup.blockLowerAnimation;

                                // Sort transitions
                                var idleToEntry = idleState.AddTransition(blockEnterState);
                                idleToEntry.AddCondition(AnimatorConditionMode.If, 0f, "Block");
                                idleToEntry.hasExitTime = false;

                                var entryToBlock = blockEnterState.AddTransition(blockIdleState);
                                entryToBlock.hasExitTime = true;
                                entryToBlock.hasFixedDuration = true;
                                entryToBlock.duration = 0.25f;
                                entryToBlock.exitTime = WizardUtility.GetExitTime(animSetup.blockRaiseAnimation, 0.25f);

                                var blockToExit = blockIdleState.AddTransition(blockExitState);
                                blockToExit.AddCondition(AnimatorConditionMode.IfNot, 0f, "Block");
                                blockToExit.hasExitTime = false;

                                var exitToIdle = blockExitState.AddTransition(idleState);
                                exitToIdle.hasExitTime = true;
                                exitToIdle.hasFixedDuration = true;
                                exitToIdle.duration = 0.25f;
                                exitToIdle.exitTime = WizardUtility.GetExitTime(animSetup.blockLowerAnimation, 0.25f);
                            }
                            else
                            {
                                AnimatorState blockState = rootStateMachine.AddState("Block");
                                blockState.motion = animSetup.blockIdleAnimation;

                                // Sort transitions
                                var entry = idleState.AddTransition(blockState);
                                var exit = blockState.AddTransition(idleState);
                                entry.AddCondition(AnimatorConditionMode.If, 0f, "Block");
                                exit.AddCondition(AnimatorConditionMode.IfNot, 0f, "Block");
                            }
                        }
                        else
                            meleeWeaponSO.FindProperty("m_BoolBlock").stringValue = string.Empty;

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
            rootObject.AddComponent<InputMeleeWeapon>();

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

                meleeWeaponSO.FindProperty("m_Animator").objectReferenceValue = vmAnimator;
            }

            meleeWeaponSO.ApplyModifiedPropertiesWithoutUndo();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            Debug.Log("Created Melee Weapon");
        }
    }
}