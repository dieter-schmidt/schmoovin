using NeoFPS;
using NeoFPSEditor.Hub.Pages.ItemCreationWizards.PlayerCharacter;
using UnityEngine;
using UnityEditor;
using NeoFPS.Constants;
using NeoSaveGames.Serialization;
using NeoCC;
using NeoFPS.CharacterMotion;
using NeoFPS.SinglePlayer;
using NeoSaveGames;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public class PlayerCharacterWizard : NeoFpsWizard
    {
        static readonly string[] k_RootSteps = new string[]
        {
            PlayerCharacterWizardSteps.root,
            PlayerCharacterWizardSteps.controller,
            PlayerCharacterWizardSteps.health,
            PlayerCharacterWizardSteps.inventory,
            PlayerCharacterWizardSteps.input,
            PlayerCharacterWizardSteps.stamina
        };

        protected override string[] GetRootSteps()
        {
            return k_RootSteps;
        }

        public override string GetDefaultTemplateFilename()
        {
            return "NewPlayerCharacterTemplate";
        }

        protected override void RegisterSteps()
        {
            RegisterStep<PlayerCharacterRootStep>(PlayerCharacterWizardSteps.root);
            RegisterStep<PlayerCharacterControllerStep>(PlayerCharacterWizardSteps.controller);
            RegisterStep<PlayerCharacterHealthStep>(PlayerCharacterWizardSteps.health);
            RegisterStep<PlayerCharacterInventoryStep>(PlayerCharacterWizardSteps.inventory);
            RegisterStep<PlayerCharacterInputStep>(PlayerCharacterWizardSteps.input);
            RegisterStep<PlayerCharacterStaminaStep>(PlayerCharacterWizardSteps.stamina);
        }

        public override void CreateItem()
        {
            // Get the save folder
            var folderPath = WizardUtility.GetPrefabOutputFolder();
            if (folderPath == null)
                return;

            var rootStep = steps[PlayerCharacterWizardSteps.root] as PlayerCharacterRootStep;
            var controllerStep = steps[PlayerCharacterWizardSteps.controller] as PlayerCharacterControllerStep;
            var healthStep = steps[PlayerCharacterWizardSteps.health] as PlayerCharacterHealthStep;
            var inventoryStep = steps[PlayerCharacterWizardSteps.inventory] as PlayerCharacterInventoryStep;
            var inputStep = steps[PlayerCharacterWizardSteps.input] as PlayerCharacterInputStep;
            var staminaStep = steps[PlayerCharacterWizardSteps.stamina] as PlayerCharacterStaminaStep;

            // Get prefab name
            string prefabName = rootStep.prefabName;
            if (rootStep.autoPrefix)
                prefabName = "Character_" + prefabName;

            // Get prefab path
            var path = WizardUtility.GetPrefabPath(prefabName, folderPath, rootStep.overwriteExisting);

            // Create objects
            var objectRoot = new GameObject(prefabName);
            var objectUpperBodyRoot = new GameObject("UpperBodyRoot");
            var objectAimer = new GameObject("Aimer");
            var objectBodySpring = new GameObject("BodySpring");
            var objectFpCameraRoot = new GameObject("FirstPersonCameraRoot");
            var objectFpCamera = new GameObject("FirstPersonCamera");
            var objectItemRoot = new GameObject("ItemRoot");
            var objectDropTransform = new GameObject("DropTransform");
            var objectBodyAudioSources = new GameObject("BodyAudioSources");
            var objectFootRoot = new GameObject("FootRoot");

            // Build hierarchy
            GameObjectUtility.SetParentAndAlign(objectUpperBodyRoot, objectRoot);
            GameObjectUtility.SetParentAndAlign(objectAimer, objectUpperBodyRoot);
            GameObjectUtility.SetParentAndAlign(objectBodySpring, objectAimer);
            GameObjectUtility.SetParentAndAlign(objectFpCameraRoot, objectBodySpring);
            GameObjectUtility.SetParentAndAlign(objectFpCamera, objectFpCameraRoot);
            GameObjectUtility.SetParentAndAlign(objectItemRoot, objectBodySpring);
            GameObjectUtility.SetParentAndAlign(objectDropTransform, objectItemRoot);
            GameObjectUtility.SetParentAndAlign(objectFootRoot, objectRoot);
            GameObjectUtility.SetParentAndAlign(objectBodyAudioSources, objectUpperBodyRoot);

            // layers & tags
            objectRoot.layer = PhysicsFilter.LayerIndex.CharacterControllers;
            objectFpCameraRoot.layer = PhysicsFilter.LayerIndex.CharacterFirstPerson;
            objectFpCamera.layer = PhysicsFilter.LayerIndex.CharacterFirstPerson;
            objectFpCamera.tag = "MainCamera";

            // Offset
            objectUpperBodyRoot.transform.localPosition = new Vector3(0f, rootStep.eyeLine, 0f);
            objectDropTransform.transform.localPosition = new Vector3(0f, -0.1f, 0.1f);

            // Body additive transforms
            var bodyTransformHandler = objectBodySpring.AddComponent<AdditiveTransformHandler>();
            objectBodySpring.AddComponent<BodyLean>();
            objectBodySpring.AddComponent<BodyTilt>();
            var shake = objectBodySpring.AddComponent<CameraShake>();
            var shakeSO = new SerializedObject(shake);
            shakeSO.FindProperty("m_ShakeDistance").vector3Value = new Vector3(0.075f, 0.1f, 0f);
            shakeSO.FindProperty("m_ShakeTwist").vector3Value = new Vector3(0.25f, 0.15f, 1f);
            shakeSO.FindProperty("m_ContinuousDamping").floatValue = 0.5f;
            shakeSO.ApplyModifiedPropertiesWithoutUndo();

            // Camera
            var camera = objectFpCamera.AddComponent<Camera>();
            camera.nearClipPlane = 0.01f;
            camera.fieldOfView = 50.625f;
            var listener = objectFpCamera.AddComponent<AudioListener>();
            listener.enabled = false;

#if UNITY_POST_PROCESSING_STACK_V2
            objectFpCamera.AddComponent<PostProcessLayerFix>();
#endif

            // Head cam + additives
            var fpCamera = objectFpCameraRoot.AddComponent<FirstPersonCamera>();
            var fpCameraSO = new SerializedObject(fpCamera);
            fpCameraSO.FindProperty("m_Camera").objectReferenceValue = camera;
            fpCameraSO.FindProperty("m_AudioListener").objectReferenceValue = listener;
            fpCameraSO.FindProperty("m_AimTransform").objectReferenceValue = objectFpCamera.transform;
            fpCameraSO.FindProperty("m_OffsetTransform").objectReferenceValue = objectFpCameraRoot.transform;
            fpCameraSO.ApplyModifiedPropertiesWithoutUndo();
            var headTransformHandler = objectFpCameraRoot.AddComponent<AdditiveTransformHandler>();
            var headTransformHandlerSO = new SerializedObject(bodyTransformHandler);
            headTransformHandlerSO.FindProperty("m_TargetTransform").objectReferenceValue = objectFpCamera.transform;
            headTransformHandlerSO.ApplyModifiedPropertiesWithoutUndo();
            var headBob = objectFpCameraRoot.AddComponent<PositionBob>();
            // Add default bob data to position bob
            var guids = AssetDatabase.FindAssets("DefaultBobData");
            if (guids != null && guids.Length > 0)
            {
                var headBobSO = new SerializedObject(headBob);
                headBobSO.FindProperty("m_BobData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PositionBobData>(AssetDatabase.GUIDToAssetPath(guids[0]));
                headBobSO.ApplyModifiedPropertiesWithoutUndo();
            }
            var headKicker = objectFpCameraRoot.AddComponent<AdditiveKicker>();
            objectFpCameraRoot.AddComponent<CharacterEventKickTrigger>();
            objectFpCameraRoot.AddComponent<TransformMatcher>();
            shake = objectFpCameraRoot.AddComponent<CameraShake>();
            shakeSO = new SerializedObject(shake);
            shakeSO.FindProperty("m_ShakeDistance").vector3Value = new Vector3(0.002f, 0.002f, 0.002f);
            shakeSO.FindProperty("m_ShakeTwist").vector3Value = new Vector3(0.25f, 0.15f, 0.5f);
            shakeSO.FindProperty("m_ContinuousDamping").floatValue = 0.5f;
            shakeSO.ApplyModifiedPropertiesWithoutUndo();

            // Base components
            var bodyCapsule = objectRoot.AddComponent<CapsuleCollider>(); // Capsule
            bodyCapsule.height = rootStep.characterHeight;
            bodyCapsule.center = new Vector3(0f, rootStep.characterHeight * 0.5f, 0f);
            bodyCapsule.radius = rootStep.characterWidth * 0.5f;
            bodyCapsule.isTrigger = true;
            var rigidbody = objectRoot.AddComponent<Rigidbody>(); // Rigidbody
            rigidbody.mass = rootStep.characterMass;
            rigidbody.isKinematic = true;
            var characterController = objectRoot.AddComponent<NeoCharacterController>(); // NeoCharacterController
            var characterControllerSO = new SerializedObject(characterController);
            characterControllerSO.FindProperty("m_SlopeLimit").floatValue = controllerStep.slopeLimit;
            characterControllerSO.FindProperty("m_SlopeFriction").floatValue = controllerStep.slopeFriction;
            characterControllerSO.FindProperty("m_LedgeFriction").floatValue = controllerStep.ledgeFriction;
            characterControllerSO.FindProperty("m_WallAngle").floatValue = controllerStep.wallAngle;
            characterControllerSO.FindProperty("m_StepHeight").floatValue = controllerStep.stepHeight;
            characterControllerSO.FindProperty("m_GroundSnapHeight").floatValue = controllerStep.groundSnapHeight;
            characterControllerSO.FindProperty("m_PushRigidbodies").boolValue = controllerStep.pushRigidbodies;
            characterControllerSO.FindProperty("m_LowRigidbodyPushMass").floatValue = controllerStep.lowRigidbodyPushMass;
            characterControllerSO.FindProperty("m_MaxRigidbodyPushMass").floatValue = controllerStep.maxRigidbodyPushMass;
            characterControllerSO.FindProperty("m_RigidbodyPush").floatValue = controllerStep.rigidbodyPush;
            characterControllerSO.FindProperty("m_PushedByCharacters").boolValue = controllerStep.pushedByCharacters;
            characterControllerSO.FindProperty("m_PushCharacters").boolValue = controllerStep.pushCharacters;
            characterControllerSO.FindProperty("m_CharacterPush").floatValue = controllerStep.characterPush;
            characterControllerSO.FindProperty("m_Gravity").vector3Value = new Vector3(0f, -controllerStep.gravity, 0f);
            characterControllerSO.ApplyModifiedPropertiesWithoutUndo();
            var motionController = objectRoot.AddComponent<MotionController>(); // MotionController
            var motionControllerSO = new SerializedObject(motionController);
            motionControllerSO.FindProperty("m_MotionGraph").objectReferenceValue = controllerStep.motionGraph;
            motionControllerSO.FindProperty("m_UseCrouchJump").boolValue = controllerStep.useCrouchJump;
            motionControllerSO.FindProperty("m_UpperBodyRoot").objectReferenceValue = objectUpperBodyRoot.transform;
            motionControllerSO.ApplyModifiedPropertiesWithoutUndo();
            var aimController = objectRoot.AddComponent<MouseAndGamepadAimController>(); // AimController
            var aimControllerSO = new SerializedObject(aimController);
            aimControllerSO.FindProperty("m_YawTransform").objectReferenceValue = objectUpperBodyRoot.transform;
            aimControllerSO.FindProperty("m_PitchTransform").objectReferenceValue = objectAimer.transform;
            aimControllerSO.ApplyModifiedPropertiesWithoutUndo();
            var soloCharacter = objectRoot.AddComponent<FpsSoloCharacter>();
            var soloCharacterSO = new SerializedObject(soloCharacter);
            soloCharacterSO.FindProperty("m_HeadTransformHandler").objectReferenceValue = headTransformHandler;
            soloCharacterSO.FindProperty("m_BodyTransformHandler").objectReferenceValue = bodyTransformHandler;
            soloCharacterSO.FindProperty("m_ApplyFallDamage").boolValue = healthStep.applyFallDamage;
            soloCharacterSO.FindProperty("m_SoftLandings").objectReferenceValue = rootStep.softLandingAudio;
            soloCharacterSO.FindProperty("m_HardLandings").objectReferenceValue = rootStep.hardLandingAudio;
            soloCharacterSO.ApplyModifiedPropertiesWithoutUndo();
            objectRoot.AddComponent<CharacterInteractionHandler>();

            // Audio
            var characterAudio = objectRoot.AddComponent<FpsCharacterAudioHandler>();
            var characterAudioSO = new SerializedObject(characterAudio);
            characterAudioSO.FindProperty("m_AudioData").objectReferenceValue = rootStep.characterAudioData;
            // Set output mixer group from audio manager
            guids = AssetDatabase.FindAssets("t:NeoFpsAudioManager");
            if (guids != null && guids.Length != 0)
            {
                var audioMgr = AssetDatabase.LoadAssetAtPath<NeoFpsAudioManager>(AssetDatabase.GUIDToAssetPath(guids[0]));
                var audioMgrSO = new SerializedObject(audioMgr);
                characterAudioSO.FindProperty("m_MixerGroup").objectReferenceValue = audioMgrSO.FindProperty("m_SpatialEffectsGroup").objectReferenceValue;
                guids = null;
            }
            var audioProp = characterAudioSO.FindProperty("m_OneShotSources");
            audioProp.arraySize = FpsCharacterAudio.count;
            audioProp.GetArrayElementAtIndex(0).objectReferenceValue = WizardUtility.AddAudioSource(objectFpCamera);
            audioProp.GetArrayElementAtIndex(1).objectReferenceValue = WizardUtility.AddAudioSource(objectBodyAudioSources);
            audioProp.GetArrayElementAtIndex(2).objectReferenceValue = WizardUtility.AddAudioSource(objectFootRoot);
            audioProp = characterAudioSO.FindProperty("m_LoopSources");
            audioProp.arraySize = FpsCharacterAudio.count;
            var loopSource = WizardUtility.AddAudioSource(objectFpCamera);
            loopSource.loop = true;
            audioProp.GetArrayElementAtIndex(0).objectReferenceValue = loopSource;
            loopSource = WizardUtility.AddAudioSource(objectBodyAudioSources);
            loopSource.loop = true;
            audioProp.GetArrayElementAtIndex(1).objectReferenceValue = loopSource;
            loopSource = WizardUtility.AddAudioSource(objectFootRoot);
            loopSource.loop = true;
            audioProp.GetArrayElementAtIndex(2).objectReferenceValue = loopSource;
            objectFpCamera.AddComponent<AudioTimeScalePitchBend>();
            objectBodyAudioSources.AddComponent<AudioTimeScalePitchBend>();
            objectFootRoot.AddComponent<AudioTimeScalePitchBend>();
            characterAudioSO.ApplyModifiedPropertiesWithoutUndo();

            // Health
            Component healthComponent = null;
            Component shieldComponent = null;
            if (healthStep.useNeoHealthSystems)
            {
                if (healthStep.healthRecharges)
                {
                    var healthManager = objectRoot.AddComponent<RechargingHealthManager>();
                    var healthManagerSO = new SerializedObject(healthManager);
                    healthManagerSO.FindProperty("m_Health").floatValue = healthStep.health;
                    healthManagerSO.FindProperty("m_HealthMax").floatValue = healthStep.maxHealth;
                    healthManagerSO.FindProperty("m_RechargeRate").floatValue = healthStep.rechargeRate;
                    healthManagerSO.FindProperty("m_RechargeDelay").floatValue = healthStep.rechargeDelay;
                    healthManagerSO.FindProperty("m_InterruptDamage").floatValue = healthStep.interruptDamage;
                    healthManagerSO.ApplyModifiedPropertiesWithoutUndo();
                    healthComponent = healthManager;
                }
                else
                {
                    var healthManager = objectRoot.AddComponent<BasicHealthManager>();
                    var healthManagerSO = new SerializedObject(healthManager);
                    healthManagerSO.FindProperty("m_Health").floatValue = healthStep.health;
                    healthManagerSO.FindProperty("m_HealthMax").floatValue = healthStep.maxHealth;
                    healthManagerSO.ApplyModifiedPropertiesWithoutUndo();
                    healthComponent = healthManager;
                }

                if (healthStep.useShieldSystem)
                {
                    var shield = objectRoot.AddComponent<ShieldManager>();
                    var shieldSO = new SerializedObject(shield);
                    shieldSO.FindProperty("m_Shield").floatValue = healthStep.shield;
                    shieldSO.FindProperty("m_StepCapacity").floatValue = healthStep.stepCapacity;
                    shieldSO.FindProperty("m_StepCount").intValue = healthStep.stepCount;
                    shieldSO.FindProperty("m_DamageMitigation").floatValue = healthStep.shieldMitigation;
                    shieldSO.FindProperty("m_ChargeRate").floatValue = healthStep.shieldChargeRate;
                    shieldSO.FindProperty("m_ChargeDelay").floatValue = healthStep.shieldChargeDelay;
                    shieldSO.FindProperty("m_CanBreakStep1").boolValue = healthStep.canBreakStep1;
                    shieldSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // Inventory
            Component inventoryComponent = null;
            SerializedObject inventorySO = null;
            FpsInventoryItemBase[] startingItems = null;
            switch (inventoryStep.inventory)
            {
                case PlayerCharacterInventoryStep.InventoryBehaviour.QuickSwitch:
                    {
                        startingItems = inventoryStep.startingItemsStandard;

                        var inventory = objectRoot.AddComponent<FpsInventoryQuickSwitch>();
                        inventoryComponent = inventory;
                        inventorySO = new SerializedObject(inventory);
                        inventorySO.FindProperty("m_DuplicateBehaviour").enumValueIndex = (int)inventoryStep.duplicateBehaviour;
                    }
                    break;
                case PlayerCharacterInventoryStep.InventoryBehaviour.Swappable:
                    {
                        startingItems = inventoryStep.startingItemsSwappable;

                        var inventory = objectRoot.AddComponent<FpsInventorySwappable>();
                        inventoryComponent = inventory;
                        inventorySO = new SerializedObject(inventory);
                        inventorySO.FindProperty("m_SwapAction").enumValueIndex = (int)inventoryStep.swapAction;

                        var arrayProp = inventorySO.FindProperty("m_GroupSizes");
                        arrayProp.arraySize = inventoryStep.groupSizes.Length;
                        for (int i = 0; i < inventoryStep.groupSizes.Length; ++i)
                            arrayProp.GetArrayElementAtIndex(i).intValue = inventoryStep.groupSizes[i];
                    }
                    break;
                case PlayerCharacterInventoryStep.InventoryBehaviour.Stacked:
                    {
                        startingItems = inventoryStep.startingItemsStandard;

                        var inventory = objectRoot.AddComponent<FpsInventoryStacked>();
                        inventoryComponent = inventory;
                        inventorySO = new SerializedObject(inventory);
                        inventorySO.FindProperty("m_DuplicateBehaviour").enumValueIndex = (int)inventoryStep.duplicateBehaviour;
                        inventorySO.FindProperty("m_MaxStackSize").intValue = inventoryStep.maxStackSize;
                    }
                    break;
            }

            if (inventorySO != null)
            {
                inventorySO.FindProperty("m_WieldableRoot").objectReferenceValue = objectItemRoot.transform;
                objectItemRoot.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                inventorySO.FindProperty("m_DropTransform").objectReferenceValue = objectDropTransform.transform;
                inventorySO.FindProperty("m_DropVelocity").vector3Value = inventoryStep.dropVelocity;
                inventorySO.FindProperty("m_SlotCount").intValue = inventoryStep.slotCount;
                inventorySO.FindProperty("m_StartingSlotChoice").enumValueIndex = (int)inventoryStep.startingSlotChoice;

                var arrayProp = inventorySO.FindProperty("m_StartingOrder");
                arrayProp.arraySize = inventoryStep.startingOrder.Length;
                for (int i = 0; i < inventoryStep.startingOrder.Length; ++i)
                    arrayProp.GetArrayElementAtIndex(i).intValue = inventoryStep.startingOrder[i];

                inventorySO.FindProperty("m_BackupItem").objectReferenceValue = inventoryStep.backupItem;
                
                arrayProp = inventorySO.FindProperty("m_StartingItems");
                arrayProp.arraySize = startingItems.Length;
                for (int i = 0; i < startingItems.Length; ++i)
                    arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = startingItems[i];

                inventorySO.ApplyModifiedPropertiesWithoutUndo();
            }

            // Stamina
            Component staminaComponent = null;
            switch (staminaStep.staminaAndBreathing)
            {
                case 1: // Full
                    {
                        var stamina = objectRoot.AddComponent<StaminaSystem>();
                        staminaComponent = stamina;
                        var staminaSO = new SerializedObject(stamina);
                        // Basics
                        staminaSO.FindProperty("m_Stamina").floatValue = staminaStep.stamina;
                        staminaSO.FindProperty("m_MaxStamina").floatValue = staminaStep.maxStamina;
                        staminaSO.FindProperty("m_StaminaRefreshRate").floatValue = staminaStep.staminaRefreshRate;
                        // Breathing
                        staminaSO.FindProperty("m_BreatheSlowInterval").floatValue = staminaStep.breatheSlowInterval;
                        staminaSO.FindProperty("m_BreatheFastInterval").floatValue = staminaStep.breatheFastInterval;
                        // Exhaustion
                        staminaSO.FindProperty("m_UseExhaustion").boolValue = staminaStep.useExhaustion;
                        staminaSO.FindProperty("m_ExhaustionThreshold").floatValue = staminaStep.exhaustionThreshold;
                        staminaSO.FindProperty("m_RecoverThreshold").floatValue = staminaStep.recoverThreshold;
                        staminaSO.FindProperty("m_ExhaustedMotionParameter").stringValue = staminaStep.exhaustedMotionParameter;
                        staminaSO.FindProperty("m_SprintMotionParameter").stringValue = staminaStep.sprintMotionParameter;
                        // Movement
                        staminaSO.FindProperty("m_AffectMovementSpeed").boolValue = staminaStep.modifyMovementSpeed;
                        staminaSO.FindProperty("m_WalkSpeedData").stringValue = staminaStep.walkSpeedData;
                        staminaSO.FindProperty("m_AimWalkSpeedData").stringValue = staminaStep.aimWalkSpeedData;
                        staminaSO.FindProperty("m_SprintSpeedData").stringValue = staminaStep.sprintSpeedData;
                        staminaSO.FindProperty("m_AimSprintSpeedData").stringValue = staminaStep.aimSprintSpeedData;
                        staminaSO.FindProperty("m_CrouchSpeedData").stringValue = staminaStep.crouchSpeedData;
                        staminaSO.FindProperty("m_AimCrouchSpeedData").stringValue = staminaStep.aimCrouchSpeedData;
                        staminaSO.FindProperty("m_MinWalkMultiplier").floatValue = staminaStep.minWalkMultiplier;
                        staminaSO.FindProperty("m_MinSprintMultiplier").floatValue = staminaStep.minSprintMultiplier;
                        staminaSO.FindProperty("m_MinCrouchMultiplier").floatValue = staminaStep.minCrouchMultiplier;
                        staminaSO.ApplyModifiedPropertiesWithoutUndo();
                    }
                    break;
                case 2: // Simple
                    {
                        var breath = objectRoot.AddComponent<SimpleBreathHandler>();
                        var breathSO = new SerializedObject(breath);
                        breathSO.FindProperty("m_BreathInterval").floatValue = staminaStep.breathInterval;
                        breathSO.FindProperty("m_BreathStrength").floatValue = staminaStep.breathStrength;
                        breathSO.ApplyModifiedPropertiesWithoutUndo();
                    }
                    break;
            }

            // Input
            if (inputStep.neoMovementInput)
            {
                var inputSO = new SerializedObject(objectRoot.AddComponent<InputCharacterMotion>());
                inputSO.FindProperty("m_EnableDodging").boolValue = inputStep.enableDodging;
                //inputSO.FindProperty("m_EnableChargedJump").boolValue = inputStep.enableChargedJump;
                inputSO.FindProperty("m_ToggleLean").boolValue = inputStep.toggleLean;
                inputSO.FindProperty("m_JumpKey").stringValue = inputStep.jumpKey;
                inputSO.FindProperty("m_JumpChargeKey").stringValue = inputStep.jumpChargeKey;
                inputSO.FindProperty("m_JumpHoldKey").stringValue = inputStep.jumpHoldKey;
                inputSO.FindProperty("m_CrouchKey").stringValue = inputStep.crouchKey;
                inputSO.FindProperty("m_CrouchHoldKey").stringValue = inputStep.crouchHoldKey;
                inputSO.FindProperty("m_SprintKey").stringValue = inputStep.sprintKey;
                inputSO.FindProperty("m_DodgeLeftKey").stringValue = inputStep.dodgeLeftKey;
                inputSO.FindProperty("m_DodgeRightKey").stringValue = inputStep.dodgeRightKey;
                inputSO.ApplyModifiedPropertiesWithoutUndo();
            }
            if (inputStep.neoInventoryInput)
            {
                objectRoot.AddComponent<InputInventory>();
            }

            // Add damage colliders
            if (healthStep.useNeoHealthSystems)
            {
                // Create
                var objectHeadDamageCollider = new GameObject("HeadDamageCollider");
                var objectUpperBodyDamageCollider = new GameObject("UpperBodyDamageCollider");
                var objectLowerBodyDamageCollider = new GameObject("LowerBodyDamageCollider");

                // Align
                GameObjectUtility.SetParentAndAlign(objectHeadDamageCollider, objectBodySpring);
                GameObjectUtility.SetParentAndAlign(objectUpperBodyDamageCollider, objectUpperBodyRoot);
                GameObjectUtility.SetParentAndAlign(objectLowerBodyDamageCollider, objectFootRoot);

                // Layers
                objectHeadDamageCollider.layer = PhysicsFilter.LayerIndex.CharacterPhysics;
                objectUpperBodyDamageCollider.layer = PhysicsFilter.LayerIndex.CharacterPhysics;
                objectLowerBodyDamageCollider.layer = PhysicsFilter.LayerIndex.CharacterPhysics;

                // Offsets
                objectUpperBodyDamageCollider.transform.localPosition = new Vector3(0f, -0.325f * rootStep.characterHeight, 0f);
                objectLowerBodyDamageCollider.transform.localPosition = new Vector3(0f, 0.25f * rootStep.characterHeight, 0f);

                // Colliders
                var sphere = objectHeadDamageCollider.AddComponent<SphereCollider>();
                sphere.radius = healthStep.headRadius;
                var capsule = objectUpperBodyDamageCollider.AddComponent<CapsuleCollider>();
                capsule.radius = healthStep.bodyRadius;
                capsule.height = rootStep.characterHeight * 0.5f;
                capsule = objectLowerBodyDamageCollider.AddComponent<CapsuleCollider>();
                capsule.radius = healthStep.legsRadius;
                capsule.height = rootStep.characterHeight * 0.5f;

                // Head damage handler
                switch (healthStep.headDamageHandler)
                {
                    case PlayerCharacterHealthStep.DamageHandlerType.Basic:
                        {
                            var handler = objectHeadDamageCollider.AddComponent<BasicDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.headDamageMultiplier;
                            handlerSO.FindProperty("m_Critical").boolValue = healthStep.headShotsAreCritical;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                    case PlayerCharacterHealthStep.DamageHandlerType.Armoured:
                        {
                            var handler = objectHeadDamageCollider.AddComponent<ArmouredDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.headDamageMultiplier;
                            handlerSO.FindProperty("m_Critical").boolValue = healthStep.headShotsAreCritical;
                            handlerSO.FindProperty("m_InventoryKey").FindPropertyRelative("m_Value").intValue = healthStep.headInventoryKey;
                            handlerSO.FindProperty("m_DamageMitigation").floatValue = healthStep.headArmourMitigation;
                            handlerSO.FindProperty("m_ArmourDamageMultiplier").floatValue = healthStep.headArmourMultiplier;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                    case PlayerCharacterHealthStep.DamageHandlerType.Shielded:
                        {
                            var handler = objectHeadDamageCollider.AddComponent<ShieldedDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.headDamageMultiplier;
                            handlerSO.FindProperty("m_Critical").boolValue = healthStep.headShotsAreCritical;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                    case PlayerCharacterHealthStep.DamageHandlerType.ArmouredAndShielded:
                        {
                            var handler = objectHeadDamageCollider.AddComponent<ShieldedArmouredDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.headDamageMultiplier;
                            handlerSO.FindProperty("m_Critical").boolValue = healthStep.headShotsAreCritical;
                            handlerSO.FindProperty("m_InventoryKey").FindPropertyRelative("m_Value").intValue = healthStep.headInventoryKey;
                            handlerSO.FindProperty("m_DamageMitigation").floatValue = healthStep.headArmourMitigation;
                            handlerSO.FindProperty("m_ArmourDamageMultiplier").floatValue = healthStep.headArmourMultiplier;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                }

                // Upper body damage handler
                switch (healthStep.bodyDamageHandler)
                {
                    case PlayerCharacterHealthStep.DamageHandlerType.Basic:
                        {
                            var handler = objectUpperBodyDamageCollider.AddComponent<BasicDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.bodyDamageMultiplier;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                    case PlayerCharacterHealthStep.DamageHandlerType.Armoured:
                        {
                            var handler = objectUpperBodyDamageCollider.AddComponent<ArmouredDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.bodyDamageMultiplier;
                            handlerSO.FindProperty("m_InventoryKey").FindPropertyRelative("m_Value").intValue = healthStep.bodyInventoryKey;
                            handlerSO.FindProperty("m_DamageMitigation").floatValue = healthStep.bodyArmourMitigation;
                            handlerSO.FindProperty("m_ArmourDamageMultiplier").floatValue = healthStep.bodyArmourMultiplier;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                    case PlayerCharacterHealthStep.DamageHandlerType.Shielded:
                        {
                            var handler = objectUpperBodyDamageCollider.AddComponent<ShieldedDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.bodyDamageMultiplier;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                    case PlayerCharacterHealthStep.DamageHandlerType.ArmouredAndShielded:
                        {
                            var handler = objectUpperBodyDamageCollider.AddComponent<ShieldedArmouredDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.bodyDamageMultiplier;
                            handlerSO.FindProperty("m_InventoryKey").FindPropertyRelative("m_Value").intValue = healthStep.bodyInventoryKey;
                            handlerSO.FindProperty("m_DamageMitigation").floatValue = healthStep.bodyArmourMitigation;
                            handlerSO.FindProperty("m_ArmourDamageMultiplier").floatValue = healthStep.bodyArmourMultiplier;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                }

                // Lower body damage handler
                switch (healthStep.legsDamageHandler)
                {
                    case PlayerCharacterHealthStep.DamageHandlerType.Basic:
                        {
                            var handler = objectLowerBodyDamageCollider.AddComponent<BasicDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.legsDamageMultiplier;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                    case PlayerCharacterHealthStep.DamageHandlerType.Armoured:
                        {
                            var handler = objectLowerBodyDamageCollider.AddComponent<ArmouredDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.legsDamageMultiplier;
                            handlerSO.FindProperty("m_InventoryKey").FindPropertyRelative("m_Value").intValue = healthStep.legsInventoryKey;
                            handlerSO.FindProperty("m_DamageMitigation").floatValue = healthStep.legsArmourMitigation;
                            handlerSO.FindProperty("m_ArmourDamageMultiplier").floatValue = healthStep.legsArmourMultiplier;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                    case PlayerCharacterHealthStep.DamageHandlerType.Shielded:
                        {
                            var handler = objectLowerBodyDamageCollider.AddComponent<ShieldedDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.legsDamageMultiplier;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                    case PlayerCharacterHealthStep.DamageHandlerType.ArmouredAndShielded:
                        {
                            var handler = objectLowerBodyDamageCollider.AddComponent<ShieldedArmouredDamageHandler>();
                            var handlerSO = new SerializedObject(handler);
                            handlerSO.FindProperty("m_Multiplier").floatValue = healthStep.legsDamageMultiplier;
                            handlerSO.FindProperty("m_InventoryKey").FindPropertyRelative("m_Value").intValue = healthStep.legsInventoryKey;
                            handlerSO.FindProperty("m_DamageMitigation").floatValue = healthStep.legsArmourMitigation;
                            handlerSO.FindProperty("m_ArmourDamageMultiplier").floatValue = healthStep.legsArmourMultiplier;
                            handlerSO.ApplyModifiedPropertiesWithoutUndo();
                        }
                        break;
                }

                // Surfaces
                WizardUtility.AddSimpleSurface(objectHeadDamageCollider, FpsSurfaceMaterial.Flesh);
                WizardUtility.AddSimpleSurface(objectUpperBodyDamageCollider, FpsSurfaceMaterial.Flesh);
                WizardUtility.AddSimpleSurface(objectLowerBodyDamageCollider, FpsSurfaceMaterial.Flesh);

                // Impact handlers
                objectUpperBodyDamageCollider.AddComponent<CharacterImpactHandler>();
                objectLowerBodyDamageCollider.AddComponent<CharacterImpactHandler>();
                var impact = objectHeadDamageCollider.AddComponent<ImpactHandlerKickTrigger>();
                var impactSO = new SerializedObject(impact);
                impactSO.FindProperty("m_Kicker").objectReferenceValue = headKicker;
                impactSO.ApplyModifiedPropertiesWithoutUndo();
            }

            // Add AI seekers
            if (rootStep.addAiSeekerTargets)
            {
                // Create
                var objectHeadSeeker = new GameObject("Seeker_01_Head");
                var objectUpperBodySeeker = new GameObject("Seeker_02_UpperBody");
                var objectLowerBodySeeker = new GameObject("Seeker_03_LowerBody");

                // Align
                GameObjectUtility.SetParentAndAlign(objectUpperBodySeeker, objectFootRoot);
                GameObjectUtility.SetParentAndAlign(objectLowerBodySeeker, objectFootRoot);
                GameObjectUtility.SetParentAndAlign(objectHeadSeeker, objectBodySpring);

                // Layers
                objectHeadSeeker.layer = PhysicsFilter.LayerIndex.AiVisibility;
                objectUpperBodySeeker.layer = PhysicsFilter.LayerIndex.AiVisibility;
                objectLowerBodySeeker.layer = PhysicsFilter.LayerIndex.AiVisibility;

                // Offsets
                objectUpperBodySeeker.transform.localPosition = new Vector3(0f, rootStep.characterHeight * 0.4f, 0f);
                objectLowerBodySeeker.transform.localPosition = new Vector3(0f, rootStep.characterHeight * 0.1f, 0f);

                // Colliders
                var sphere = objectHeadSeeker.AddComponent<SphereCollider>();
                sphere.radius = 0.1f;
                sphere = objectUpperBodySeeker.AddComponent<SphereCollider>();
                sphere.radius = 0.1f;
                sphere = objectLowerBodySeeker.AddComponent<SphereCollider>();
                sphere.radius = 0.1f;
            }

            // Save games
            var nsgo = objectRoot.AddComponent<NeoSerializedGameObject>();
            var nsgoSO = new SerializedObject(nsgo);
            var overridesProp = nsgoSO.FindProperty("m_Overrides");
            overridesProp.arraySize = 1;
            var persistenceProp = overridesProp.GetArrayElementAtIndex(0);
            persistenceProp.FindPropertyRelative("m_SaveMode").intValue = SaveMode.Persistence;
            persistenceProp.FindPropertyRelative("m_Position").enumValueIndex = 3;
            persistenceProp.FindPropertyRelative("m_Rotation").enumValueIndex = 3;
            persistenceProp.FindPropertyRelative("m_FilterNeoComponents").enumValueIndex = 2;
            var neoComponents = persistenceProp.FindPropertyRelative("m_NeoComponents");
            SerializedArrayUtility.Add(neoComponents, soloCharacter);
            if (healthComponent != null)
                SerializedArrayUtility.Add(neoComponents, healthComponent);
            if (shieldComponent != null)
                SerializedArrayUtility.Add(neoComponents, shieldComponent);
            if (inventoryComponent != null)
                SerializedArrayUtility.Add(neoComponents, inventoryComponent);
            if (staminaComponent != null)
                SerializedArrayUtility.Add(neoComponents, staminaComponent);
            nsgoSO.ApplyModifiedPropertiesWithoutUndo();

            nsgo = objectUpperBodyRoot.AddComponent<NeoSerializedGameObject>();
            nsgoSO = new SerializedObject(nsgo);
            nsgoSO.FindProperty("m_Rotation").enumValueIndex = 2;
            overridesProp = nsgoSO.FindProperty("m_Overrides");
            overridesProp.arraySize = 1;
            persistenceProp = overridesProp.GetArrayElementAtIndex(0);
            persistenceProp.FindPropertyRelative("m_SaveMode").intValue = SaveMode.Persistence;
            persistenceProp.FindPropertyRelative("m_Position").enumValueIndex = 3;
            persistenceProp.FindPropertyRelative("m_Rotation").enumValueIndex = 3;
            nsgoSO.ApplyModifiedPropertiesWithoutUndo();

            nsgo = objectAimer.AddComponent<NeoSerializedGameObject>();
            nsgoSO = new SerializedObject(nsgo);
            nsgoSO.FindProperty("m_Position").enumValueIndex = 2;
            nsgoSO.FindProperty("m_Rotation").enumValueIndex = 2;
            nsgoSO.ApplyModifiedPropertiesWithoutUndo();

            nsgo = objectItemRoot.AddComponent<NeoSerializedGameObject>();
            nsgoSO = new SerializedObject(nsgo);
            nsgoSO.FindProperty("m_Position").enumValueIndex = 2;
            nsgoSO.FindProperty("m_Rotation").enumValueIndex = 2;
            nsgoSO.ApplyModifiedPropertiesWithoutUndo();
            var itemRootNsgo = nsgo;

            nsgo = objectBodySpring.AddComponent<NeoSerializedGameObject>();
            nsgoSO = new SerializedObject(nsgo);
            nsgoSO.FindProperty("m_Position").enumValueIndex = 2;
            nsgoSO.FindProperty("m_Rotation").enumValueIndex = 2;
            nsgoSO.FindProperty("m_FilterChildObjects").enumValueIndex = 1;
            SerializedArrayUtility.Add(nsgoSO.FindProperty("m_ChildObjects"), itemRootNsgo);
            nsgoSO.ApplyModifiedPropertiesWithoutUndo();

            nsgo = objectFpCameraRoot.AddComponent<NeoSerializedGameObject>();
            nsgoSO = new SerializedObject(nsgo);
            nsgoSO.FindProperty("m_Position").enumValueIndex = 2;
            nsgoSO.FindProperty("m_Rotation").enumValueIndex = 2;
            nsgoSO.ApplyModifiedPropertiesWithoutUndo();


            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(objectRoot, path);
            AssetDatabase.Refresh();
            DestroyImmediate(objectRoot);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            Debug.Log("Created Player Character");
        }
    }
}