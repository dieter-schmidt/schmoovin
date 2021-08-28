using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor.Hub.Pages.ItemCreationWizards.Pickups;
using NeoSaveGames.Serialization;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public class PickupWizard : NeoFpsWizard
    {
        static readonly string[] k_RootSteps = new string[] { PickupWizardSteps.root };

        public override string GetDefaultTemplateFilename()
        {
            return "NewPickupTemplate";
        }

        protected override string[] GetRootSteps()
        {
            return k_RootSteps;
        }

        protected override void RegisterSteps()
        {
            RegisterStep<PickupWizardRootStep>(PickupWizardSteps.root);
            RegisterStep<WieldableDropSetup>(PickupWizardSteps.wieldable);
            RegisterStep<FirearmDropSetup>(PickupWizardSteps.firearm);
            RegisterStep<InventoryPickupSetup>(PickupWizardSteps.inventoryItem);
            RegisterStep<InventoryMultiPickupSetup>(PickupWizardSteps.inventoryMultiItem);
            RegisterStep<HealthPackSetup>(PickupWizardSteps.health);
            RegisterStep<ShieldBoosterSetup>(PickupWizardSteps.shield);
            RegisterStep<KeyRingPickupSetup>(PickupWizardSteps.keyRing);
            RegisterStep<AudioVisualSetup>(PickupWizardSteps.audioVisual);
            RegisterStep<AudioVisualFirearmSetup>(PickupWizardSteps.audioVisualFirearm);
        }

        public override void CreateItem()
        {
            // Get the save folder
            var folderPath = WizardUtility.GetPrefabOutputFolder();
            if (folderPath != null)
            {
                var root = steps[PickupWizardSteps.root] as PickupWizardRootStep;
                switch (root.pickupType)
                {
                    case 0: // Modular firearm drop
                        CreateWieldableDrop(root, folderPath);
                        break;
                    case 1: // Wieldable drop
                        CreateFirearmDrop(root, folderPath);
                        break;
                    case 2: // Inventory item pickup
                        CreateInventoryItemPickup(root, folderPath);
                        break;
                    case 3: // Multi-iten inventory pickup
                        CreateInventoryMultiItemPickup(root, folderPath);
                        break;
                    case 4: // Health pack
                        CreateHealthPack(root, folderPath);
                        break;
                    case 5: // Shield booster
                        CreateShieldBooster(root, folderPath);
                        break;
                    case 6: // Key ring
                        CreateKeyRingPickup(root, folderPath);
                        break;
                }
            }
        }

        void CreateFirearmDrop(PickupWizardRootStep root, string folderPath)
        {
            var setup = steps[PickupWizardSteps.firearm] as FirearmDropSetup;
            var audioVisual = steps[PickupWizardSteps.audioVisualFirearm] as AudioVisualFirearmSetup;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
                prefabName = "WeaponPickup_" + prefabName;

            // Get prefab path
            var path = string.Format("{0}/{1}.prefab", folderPath, prefabName);
            if (!root.overwriteExisting)
                path = AssetDatabase.GenerateUniqueAssetPath(path);

            // Create root object
            GameObject rootObject = new GameObject(prefabName);
            AddAudio(audioVisual, audioVisual.pickupAudio, rootObject);

            // Add display object
            var displayObject = GetDisplayObject(audioVisual, rootObject.transform, "Render", true);
            displayObject.transform.SetParent(rootObject.transform);

            // Fill out the pickup
            var pickup = SetUpInteractivePickupTrigger<InteractivePickup>(root, audioVisual, rootObject, displayObject);
            var pickupSO = new SerializedObject(pickup);

            var item = setup.firearmPrefab.GetComponent<FpsInventoryWieldable>();
            pickupSO.FindProperty("m_Item").objectReferenceValue = item;
            pickupSO.ApplyModifiedPropertiesWithoutUndo();

            // Add the ammo object
            var ammoObject = new GameObject("AmmoPickup");
            AddAudio(audioVisual, audioVisual.ammoAudio, ammoObject);
            ammoObject.transform.SetParent(rootObject.transform);
            ammoObject.layer = PhysicsFilter.LayerIndex.TriggerZones;
            var collider = ammoObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.25f;
            collider.center = rootObject.GetComponent<Collider>().bounds.center;
            ammoObject.AddComponent<PickupTriggerZone>();

            var ammo = ammoObject.AddComponent<FpsInventoryAmmo>();
            var ammoSO = new SerializedObject(ammo);
            ammoSO.FindProperty("m_AmmoType").objectReferenceValue = setup.ammoType;
            ammoSO.ApplyModifiedPropertiesWithoutUndo();

            var ammoPickup = ammoObject.AddComponent<ModularFirearmAmmoPickup>();
            var ammoPickupSO = new SerializedObject(ammoPickup);
            ammoPickupSO.FindProperty("m_WeaponPickup").objectReferenceValue = pickup;
            ammoPickupSO.FindProperty("m_AmmoObject").objectReferenceValue = ammo;

            // Get the ammo mesh
            if (audioVisual.displayObjectType != 0)
            {
                var ammoMesh = audioVisual.ammoMesh;
                if (ammoMesh != null)
                {
                    int renderType = audioVisual.displayObjectType;
                    if (renderType > 0)
                    {
                        GameObject model = null;
                        if (renderType == 1)
                            model = audioVisual.modelPrefab;
                        else
                            model = audioVisual.displayPrefab;

                        ammoPickupSO.FindProperty("m_DisplayMesh").objectReferenceValue = WizardUtility.GetRelativeGameObject(model, displayObject, ammoMesh.gameObject);

                        //// Get the child index values to recreate path
                        //var indices = new List<int>();
                        //var itr = ammoMesh.transform;
                        //while (itr != model && itr != null)
                        //{
                        //    indices.Add(itr.GetSiblingIndex());
                        //    itr = itr.parent;
                        //}

                        //// Walk back up from display object to equivalent
                        //itr = displayObject.transform;
                        //for (int i = indices.Count - 1; i >= 0; --i)
                        //    itr = itr.GetChild(indices[i]);

                        //// Assign as display mesh
                        //ammoPickupSO.FindProperty("m_DisplayMesh").objectReferenceValue = itr.gameObject;
                    }
                }
            }
            ammoPickupSO.ApplyModifiedPropertiesWithoutUndo();

            // Fill out the drop
            var drop = rootObject.AddComponent<ModularFirearmDrop>();
            var dropSO = new SerializedObject(drop);

            dropSO.FindProperty("m_RigidBody").objectReferenceValue = rootObject.GetComponent<Rigidbody>();
            dropSO.FindProperty("m_Pickup").objectReferenceValue = pickup;
            dropSO.FindProperty("m_AmmoPickup").objectReferenceValue = ammoPickup;
            dropSO.ApplyModifiedPropertiesWithoutUndo();

            // Add highlight
            int highlight = audioVisual.highlight;
            if (highlight == 1)
                AddCornerMarkers(rootObject, rootObject.GetComponent<BoxCollider>());
            if (highlight == 2)
                rootObject.AddComponent<InteractiveObjectMaterialMarker>();

            // Add save games
            rootObject.AddComponent<NeoSerializedGameObject>();
            ammoObject.AddComponent<NeoSerializedGameObject>();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            // Add to wieldable
            if (setup.addToGun)
            {
                var itemSO = new SerializedObject(item);
                itemSO.FindProperty("m_DropObject").objectReferenceValue = prefab;
                itemSO.ApplyModifiedPropertiesWithoutUndo();
            }

            Debug.Log("Created modular firearm drop");
        }

        void CreateWieldableDrop(PickupWizardRootStep root, string folderPath)
        {
            var setup = steps[PickupWizardSteps.wieldable] as WieldableDropSetup;
            var audioVisual = steps[PickupWizardSteps.audioVisual] as AudioVisualSetup;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
                prefabName = "WeaponPickup_" + prefabName;

            // Get prefab path
            var path = string.Format("{0}/{1}.prefab", folderPath, prefabName);
            if (!root.overwriteExisting)
                path = AssetDatabase.GenerateUniqueAssetPath(path);

            // Create root object
            GameObject rootObject = new GameObject(prefabName);
            AddAudio(audioVisual, audioVisual.pickupAudio, rootObject);

            // Add display object
            var displayObject = GetDisplayObject(audioVisual, rootObject.transform, "Render", true);
            displayObject.transform.SetParent(rootObject.transform);

            // Fill out the pickup
            var pickup = SetUpInteractivePickupTrigger<InteractivePickup>(root, audioVisual, rootObject, displayObject);
            var pickupSO = new SerializedObject(pickup);

            var item = setup.wieldablePrefab.GetComponent<FpsInventoryWieldable>();
            pickupSO.FindProperty("m_Item").objectReferenceValue = item;
            pickupSO.ApplyModifiedPropertiesWithoutUndo();

            // Fill out the drop
            var drop = rootObject.AddComponent<FpsInventoryWieldableDrop>();
            var dropSO = new SerializedObject(drop);
            dropSO.FindProperty("m_RigidBody").objectReferenceValue = rootObject.GetComponent<Rigidbody>();
            dropSO.FindProperty("m_Pickup").objectReferenceValue = pickup;
            dropSO.ApplyModifiedPropertiesWithoutUndo();

            // Add highlight
            int highlight = audioVisual.highlight;
            if (highlight == 1)
                AddCornerMarkers(rootObject, rootObject.GetComponent<BoxCollider>());
            if (highlight == 2)
                rootObject.AddComponent<InteractiveObjectMaterialMarker>();

            // Add save games
            rootObject.AddComponent<NeoSerializedGameObject>();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            // Add to wieldable
            if (setup.addToWieldable)
            {
                var itemSO = new SerializedObject(item);
                itemSO.FindProperty("m_DropObject").objectReferenceValue = prefab;
                itemSO.ApplyModifiedPropertiesWithoutUndo();
            }

            Debug.Log("Created wieldable drop");
        }

        void CreateInventoryItemPickup(PickupWizardRootStep root, string folderPath)
        {
            var setup = steps[PickupWizardSteps.inventoryItem] as InventoryPickupSetup;
            var audioVisual = steps[PickupWizardSteps.audioVisual] as AudioVisualSetup;

            bool interactive = root.interactionType == 0;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
            {
                if (interactive)
                    prefabName = "InteractivePickup_" + prefabName;
                else
                    prefabName = "ContactPickup_" + prefabName;
            }

            // Get prefab path
            var path = string.Format("{0}/{1}.prefab", folderPath, prefabName);
            if (!root.overwriteExisting)
                path = AssetDatabase.GenerateUniqueAssetPath(path);

            // Create root object
            GameObject rootObject = new GameObject(prefabName);
            AddAudio(audioVisual, audioVisual.pickupAudio, rootObject);

            // Add display object
            var displayObject = GetDisplayObject(audioVisual, rootObject.transform, "Render");
            displayObject.transform.SetParent(rootObject.transform);

            // Fill out the multi-item pickup
            var pickup = rootObject.AddComponent<InventoryItemPickup>();
            var pickupSO = new SerializedObject(pickup);

            var item = setup.inventoryItem;
            pickupSO.FindProperty("m_ItemPrefab").objectReferenceValue = item;
            pickupSO.FindProperty("m_SpawnOnAwake").boolValue = setup.spawnOnAwake;
            pickupSO.FindProperty("m_ConsumeResult").enumValueIndex = setup.consumeResult;
            pickupSO.FindProperty("m_RespawnDuration").floatValue = setup.respawnDuration;
            pickupSO.FindProperty("m_DisplayMesh").objectReferenceValue = displayObject;

            pickupSO.ApplyModifiedPropertiesWithoutUndo();

            // Set up contact / interactive pickup triggers
            if (interactive)
                SetUpInteractivePickupTrigger<InteractivePickupTrigger>(root, audioVisual, rootObject, displayObject);
            else
                SetUpContactPickupTrigger(rootObject, displayObject);

            if (interactive)
            {
                // Add highlight
                int highlight = audioVisual.highlight;
                if (highlight == 1)
                    AddCornerMarkers(rootObject, rootObject.GetComponent<BoxCollider>());
                if (highlight == 2)
                    rootObject.AddComponent<InteractiveObjectMaterialMarker>();
            }

            // Add save games
            rootObject.AddComponent<NeoSerializedGameObject>();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            Debug.Log("Created inventory item pickup");
        }

        void CreateInventoryMultiItemPickup(PickupWizardRootStep root, string folderPath)
        {
            var setup = steps[PickupWizardSteps.inventoryMultiItem] as InventoryMultiPickupSetup;
            var audioVisual = steps[PickupWizardSteps.audioVisual] as AudioVisualSetup;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
                prefabName = "MultiPickup_" + prefabName;

            // Get prefab path
            var path = string.Format("{0}/{1}.prefab", folderPath, prefabName);
            if (!root.overwriteExisting)
                path = AssetDatabase.GenerateUniqueAssetPath(path);

            // Create root object
            GameObject rootObject = GetDisplayObject(audioVisual, null, prefabName);
            AddAudio(audioVisual, audioVisual.pickupAudio, rootObject);

            // Fill out the multi-item pickup
            var pickup = rootObject.AddComponent<InteractiveMultiPickup>();
            var pickupSO = new SerializedObject(pickup);
            
            if (setup.inventoryItems.Length > 0)
            {
                var itemsProp = pickupSO.FindProperty("m_Items");
                itemsProp.arraySize = setup.inventoryItems.Length;
                for (int i = 0; i < setup.inventoryItems.Length; ++i)
                    itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = setup.inventoryItems[i];
            }
            pickupSO.FindProperty("m_Replenish").boolValue = setup.replenishItems;
            pickupSO.FindProperty("m_TooltipName").stringValue = audioVisual.tooltipName;
            pickupSO.FindProperty("m_TooltipAction").stringValue = audioVisual.tooltipAction;
            pickupSO.ApplyModifiedPropertiesWithoutUndo();

            // Add interactive object trigger
            var interactiveObject = new GameObject("Interaction");
            interactiveObject.layer = PhysicsFilter.LayerIndex.InteractiveObjects;
            interactiveObject.transform.SetParent(rootObject.transform);
            var rbc = rootObject.AddComponent<BoxCollider>();
            var ibc = interactiveObject.AddComponent<BoxCollider>();
            ibc.center = rbc.center;
            ibc.size = rbc.size + new Vector3(0.05f, 0.05f, 0.05f);
            ibc.isTrigger = true;
            DestroyImmediate(rbc);

            // Add highlight
            int highlight = audioVisual.highlight;
            if (highlight == 1)
                AddCornerMarkers(rootObject, ibc);
            if (highlight == 2)
                rootObject.AddComponent<InteractiveObjectMaterialMarker>();

            // Add save games
            rootObject.AddComponent<NeoSerializedGameObject>();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            Debug.Log("Created inventory multi-item pickup: " + path);
        }

        void CreateHealthPack(PickupWizardRootStep root, string folderPath)
        {
            var setup = steps[PickupWizardSteps.health] as HealthPackSetup;
            var audioVisual = steps[PickupWizardSteps.audioVisual] as AudioVisualSetup;

            bool interactive = root.interactionType == 0;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
            {
                if (interactive)
                    prefabName = "InteractiveHealthPack_" + prefabName;
                else
                    prefabName = "ContactHealthPack_" + prefabName;
            }

            // Get prefab path
            var path = string.Format("{0}/{1}.prefab", folderPath, prefabName);
            if (!root.overwriteExisting)
                path = AssetDatabase.GenerateUniqueAssetPath(path);

            // Create root object
            GameObject rootObject = new GameObject(prefabName);
            AddAudio(audioVisual, audioVisual.pickupAudio, rootObject);

            // Add display object
            var displayObject = GetDisplayObject(audioVisual, rootObject.transform, "Render");
            displayObject.transform.SetParent(rootObject.transform);

            // Fill out the multi-item pickup
            var pickup = rootObject.AddComponent<HealthPickup>();
            var pickupSO = new SerializedObject(pickup);

            int healType = setup.healType;
            pickupSO.FindProperty("m_HealType").enumValueIndex = healType;
            if (healType == 0)
                pickupSO.FindProperty("m_HealAmount").floatValue = setup.healAmount;
            else
                pickupSO.FindProperty("m_HealAmount").floatValue = setup.healFactor;
            pickupSO.FindProperty("m_SingleUse").boolValue = (setup.consumeType == 0);
            pickupSO.FindProperty("m_ConsumeResult").enumValueIndex = setup.consumeResult;
            pickupSO.FindProperty("m_RespawnDuration").floatValue = setup.respawnDuration;
            pickupSO.FindProperty("m_DisplayMesh").objectReferenceValue = displayObject;

            pickupSO.ApplyModifiedPropertiesWithoutUndo();

            // Set up contact / interactive pickup triggers
            if (interactive)
                SetUpInteractivePickupTrigger<InteractivePickupTrigger>(root, audioVisual, rootObject, displayObject);
            else
                SetUpContactPickupTrigger(rootObject, displayObject);

            if (interactive)
            {
                // Add highlight
                int highlight = audioVisual.highlight;
                if (highlight == 1)
                    AddCornerMarkers(rootObject, rootObject.GetComponent<BoxCollider>());
                if (highlight == 2)
                    rootObject.AddComponent<InteractiveObjectMaterialMarker>();
            }

            // Add save games
            rootObject.AddComponent<NeoSerializedGameObject>();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            Debug.Log("Created health pack");
        }

        void CreateShieldBooster(PickupWizardRootStep root, string folderPath)
        {
            var setup = steps[PickupWizardSteps.shield] as ShieldBoosterSetup;
            var audioVisual = steps[PickupWizardSteps.audioVisual] as AudioVisualSetup;

            bool interactive = root.interactionType == 0;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
            {
                if (interactive)
                    prefabName = "InteractiveShieldBoost_" + prefabName;
                else
                    prefabName = "ContactShieldBoost_" + prefabName;
            }

            // Get prefab path
            var path = string.Format("{0}/{1}.prefab", folderPath, prefabName);
            if (!root.overwriteExisting)
                path = AssetDatabase.GenerateUniqueAssetPath(path);

            // Create root object
            GameObject rootObject = new GameObject(prefabName);
            AddAudio(audioVisual, audioVisual.pickupAudio, rootObject);

            // Add display object
            var displayObject = GetDisplayObject(audioVisual, rootObject.transform, "Render");
            displayObject.transform.SetParent(rootObject.transform);

            // Fill out the multi-item pickup
            var pickup = rootObject.AddComponent<ShieldPickup>();
            var pickupSO = new SerializedObject(pickup);

            pickupSO.FindProperty("m_StepCount").intValue = setup.stepCount;
            pickupSO.FindProperty("m_SingleUse").boolValue = (setup.consumeType == 0);
            pickupSO.FindProperty("m_ConsumeResult").enumValueIndex = setup.consumeResult;
            pickupSO.FindProperty("m_RespawnDuration").floatValue = setup.respawnDuration;
            pickupSO.FindProperty("m_DisplayMesh").objectReferenceValue = displayObject;

            pickupSO.ApplyModifiedPropertiesWithoutUndo();

            // Set up contact / interactive pickup triggers
            if (interactive)
                SetUpInteractivePickupTrigger<InteractivePickupTrigger>(root, audioVisual, rootObject, displayObject);
            else
                SetUpContactPickupTrigger(rootObject, displayObject);

            if (interactive)
            {
                // Add highlight
                int highlight = audioVisual.highlight;
                if (highlight == 1)
                    AddCornerMarkers(rootObject, rootObject.GetComponent<BoxCollider>());
                if (highlight == 2)
                    rootObject.AddComponent<InteractiveObjectMaterialMarker>();
            }

            // Add save games
            rootObject.AddComponent<NeoSerializedGameObject>();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            Debug.Log("Created shield booster");
        }

        void CreateKeyRingPickup(PickupWizardRootStep root, string folderPath)
        {
            var setup = steps[PickupWizardSteps.keyRing] as KeyRingPickupSetup;
            var audioVisual = steps[PickupWizardSteps.audioVisual] as AudioVisualSetup;

            bool interactive = root.interactionType == 0;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
            {
                if (interactive)
                    prefabName = "InteractiveKeyRingPickup_" + prefabName;
                else
                    prefabName = "ContactKeyRingPickup_" + prefabName;
            }

            // Get prefab path
            var path = string.Format("{0}/{1}.prefab", folderPath, prefabName);
            if (!root.overwriteExisting)
                path = AssetDatabase.GenerateUniqueAssetPath(path);

            // Create root object
            GameObject rootObject = new GameObject(prefabName);
            AddAudio(audioVisual, audioVisual.pickupAudio, rootObject);

            // Add display object
            var displayObject = GetDisplayObject(audioVisual, rootObject.transform, "Render");
            displayObject.transform.SetParent(rootObject.transform);

            // Fill out the key ring pickup
            var pickup = rootObject.AddComponent<KeyRingPickup>();
            var pickupSO = new SerializedObject(pickup);

            pickupSO.FindProperty("m_Root").objectReferenceValue = rootObject.transform;
            pickupSO.FindProperty("m_KeyRingPrefab").objectReferenceValue = setup.keyRingPrefab;

            var keyCodesProp = pickupSO.FindProperty("m_KeyCodes");
            keyCodesProp.arraySize = setup.keyCodes.Length;
            for (int i = 0; i < setup.keyCodes.Length; ++i)
                keyCodesProp.GetArrayElementAtIndex(i).stringValue = setup.keyCodes[i];

            pickupSO.ApplyModifiedPropertiesWithoutUndo();

            // Set up contact / interactive pickup triggers
            if (interactive)
                SetUpInteractivePickupTrigger<InteractivePickupTrigger>(root, audioVisual, rootObject, displayObject);
            else
                SetUpContactPickupTrigger(rootObject, displayObject);

            if (interactive)
            {
                // Add highlight
                int highlight = audioVisual.highlight;
                if (highlight == 1)
                    AddCornerMarkers(rootObject, rootObject.GetComponent<BoxCollider>());
                if (highlight == 2)
                    rootObject.AddComponent<InteractiveObjectMaterialMarker>();
            }

            // Add save games
            rootObject.AddComponent<NeoSerializedGameObject>();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);

            Debug.Log("Created key-ring pickup");
        }

        T SetUpInteractivePickupTrigger<T>(PickupWizardRootStep rootData, AudioVisualSetup avData, GameObject rootObject, GameObject displayObject) where T : Component
        {
            var renderers = displayObject.GetComponentsInChildren<Renderer>(true);

            // Get bounds
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; ++i)
                bounds.Encapsulate(renderers[i].bounds);
            bounds.Expand(0.05f);

            // Add interactive object collider
            rootObject.layer = PhysicsFilter.LayerIndex.InteractiveObjects;
            var interactionCollider = rootObject.AddComponent<BoxCollider>();
            interactionCollider.center = bounds.center;
            interactionCollider.size = bounds.size;
            interactionCollider.isTrigger = true;

            // Add interaction pickup trigger
            var result = rootObject.AddComponent<T>();
            var so = new SerializedObject(result);
            so.FindProperty("m_TooltipName").stringValue = avData.tooltipName;
            so.FindProperty("m_TooltipAction").stringValue = avData.tooltipAction;

            float holdDuration = 0f;
            if (rootData.tapOrHold == 1)
                holdDuration = rootData.holdDuration;
            so.FindProperty("m_HoldDuration").floatValue = holdDuration;

            so.ApplyModifiedPropertiesWithoutUndo();

            return result;
        }

        void SetUpContactPickupTrigger(GameObject rootObject, GameObject displayObject)
        {
            var renderers = displayObject.GetComponentsInChildren<Renderer>(true);

            // Get bounds
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; ++i)
                bounds.Encapsulate(renderers[i].bounds);
            bounds.Expand(0.05f);

            // Add contact collider
            rootObject.layer = PhysicsFilter.LayerIndex.TriggerZones;
            var contactCollider = rootObject.AddComponent<SphereCollider>();
            contactCollider.center = bounds.center;
            contactCollider.radius = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;
            contactCollider.isTrigger = true;

            // Add contact pickup trigger
            rootObject.AddComponent<PickupTriggerZone>();
        }

        GameObject GetDisplayObject(AudioVisualSetup avStep, Transform parent, string objectName, bool forceSmallObject = false)
        {
            GameObject renderObject = null;
            switch (avStep.displayObjectType)
            {
                case 0: // Mesh
                    {
                        renderObject = new GameObject(objectName);

                        var meshFilter = renderObject.AddComponent<MeshFilter>();
                        meshFilter.mesh = avStep.renderMesh;
                        var meshRenderer = renderObject.AddComponent<MeshRenderer>();
                        meshRenderer.material = avStep.material;
                    }
                    break;
                case 1: // Model
                    {
                        renderObject = Instantiate(avStep.modelPrefab);
                        renderObject.name = objectName;

                        if (avStep.useCustomMaterial)
                        {
                            var meshRenderer = renderObject.GetComponentInChildren<MeshRenderer>();
                            if (meshRenderer != null)
                                meshRenderer.material = avStep.material;
                        }
                    }
                    break;
                case 2: // Prefab
                    {
                        renderObject = Instantiate(avStep.displayPrefab);
                        renderObject.name = objectName;
                    }
                    break;
            }
            if (parent != null)
                renderObject.transform.SetParent(parent);
            renderObject.transform.position = Vector3.zero;

            int physicsType = 2; // Small dynamic object
            if (!forceSmallObject)
                physicsType = avStep.physicsType;

            switch (physicsType)
            {
                case 1: // Static Large
                    {
                        // Set layer
                        renderObject.layer = PhysicsFilter.LayerIndex.EnvironmentDetail;

                        // Add child object for physics
                        var physicsObject = new GameObject("PhysicsRough");
                        physicsObject.transform.SetParent(renderObject.transform);
                        physicsObject.transform.SetAsLastSibling();
                        physicsObject.layer = PhysicsFilter.LayerIndex.EnvironmentRough;

                        // Add collider
                        AddCollider(avStep, renderObject, physicsObject);

                        return renderObject;
                    }
                case 2: // Dynamic Small
                    {
                        // Set layer
                        renderObject.layer = PhysicsFilter.LayerIndex.SmallDynamicObjects;

                        // Add collider
                        AddCollider(avStep, renderObject, renderObject);

                        // Add rigidbody
                        var rbObject = parent == null ? renderObject : parent.gameObject;
                        var rb = rbObject.AddComponent<Rigidbody>();
                        rb.mass = avStep.mass;

                        // Add surface
                        var so = new SerializedObject(rbObject.AddComponent<SimpleSurface>());
                        so.FindProperty("m_Surface").FindPropertyRelative("m_Value").intValue = avStep.surfaceMaterial;
                        so.ApplyModifiedPropertiesWithoutUndo();

                        return renderObject;
                    }
                case 3: // Dynamic Large
                    {
                        // Set layer
                        renderObject.layer = PhysicsFilter.LayerIndex.DynamicProps;

                        // Add collider
                        AddCollider(avStep, renderObject, renderObject);

                        // Add rigidbody
                        var rbObject = parent == null ? renderObject : parent.gameObject;
                        var rb = rbObject.AddComponent<Rigidbody>();
                        rb.mass = avStep.mass;
                        rb.interpolation = RigidbodyInterpolation.Interpolate;

                        // Add surface
                        var so = new SerializedObject(rbObject.AddComponent<SimpleSurface>());
                        so.FindProperty("m_Surface").FindPropertyRelative("m_Value").intValue = avStep.surfaceMaterial;
                        so.ApplyModifiedPropertiesWithoutUndo();

                        return renderObject;
                    }
                default:
                    renderObject.layer = PhysicsFilter.LayerIndex.EnvironmentDetail;
                    return renderObject;
            }
        }

        void AddCollider(AudioVisualSetup avStep, GameObject renderObject, GameObject physicsObject)
        {
            // Get bounds
            var renderers = renderObject.GetComponentsInChildren<Renderer>(true);

            // Add collider
            switch (avStep.colliderType)
            {
                case 0: // Box
                    {
                        var bounds = renderers[0].bounds;
                        for (int i = 1; i < renderers.Length; ++i)
                            bounds.Encapsulate(renderers[i].bounds);

                        var pc = physicsObject.AddComponent<BoxCollider>();
                        pc.center = bounds.center;
                        pc.size = bounds.size;
                    }
                    break;
                case 1: // Sphere
                    {
                        if (physicsObject == renderObject)
                        {
                            var pc = physicsObject.AddComponent<SphereCollider>();
                        }
                        else
                        {
                            var rc = renderers[0].gameObject.AddComponent<SphereCollider>();
                            var pc = physicsObject.AddComponent<SphereCollider>();
                            pc.center = rc.center;
                            pc.radius = rc.radius;
                            DestroyImmediate(rc);
                        }
                    }
                    break;
                case 2: // Capsule
                    {
                        if (physicsObject == renderObject)
                        {
                            var pc = physicsObject.AddComponent<CapsuleCollider>();
                        }
                        else
                        {
                            var rc = renderers[0].gameObject.AddComponent<CapsuleCollider>();
                            var pc = physicsObject.AddComponent<CapsuleCollider>();
                            pc.center = rc.center;
                            pc.radius = rc.radius;
                            pc.height = rc.height;
                            pc.direction = rc.direction;
                            DestroyImmediate(rc);
                        }
                    }
                    break;
                case 3: // Mesh (Convex)
                    {
                        var pc = physicsObject.AddComponent<MeshCollider>();
                        pc.sharedMesh = avStep.colliderMesh;
                        pc.convex = true;
                    }
                    break;
                case 4: // Mesh (Non-Convex)
                    {
                        var pc = physicsObject.AddComponent<MeshCollider>();
                        pc.sharedMesh = avStep.colliderMesh;
                        pc.convex = false;
                    }
                    break;
            }
        }

        void AddAudio(AudioVisualSetup avStep, AudioClip clip, GameObject targetObject)
        {
            // Add audio source
            var source = targetObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.playOnAwake = false;

            // Set output mixer group from audio manager
            var guids = AssetDatabase.FindAssets("t:NeoFpsAudioManager");
            if (guids != null && guids.Length != 0)
            {
                var audioMgr = AssetDatabase.LoadAssetAtPath<NeoFpsAudioManager>(AssetDatabase.GUIDToAssetPath(guids[0]));
                var audioMgrSO = new SerializedObject(audioMgr);
                source.outputAudioMixerGroup = audioMgrSO.FindProperty("m_SpatialEffectsGroup").objectReferenceValue as AudioMixerGroup;
            }

            // Add contact audio handler if required
            if (targetObject.GetComponent<Rigidbody>() != null && targetObject.GetComponent<BaseSurface>() != null && avStep.physicsType > 1)
                targetObject.AddComponent<SurfaceContactAudioHandler>();
        }

        void AddCornerMarkers(GameObject gameObject, Collider interactionCollider)
        {
            var markers = gameObject.AddComponent<InteractiveObjectCornerMarkers>();
            var markersSO = new SerializedObject(markers);

            // Set the collider to highlight
            var colliders = markersSO.FindProperty("m_BoxColliders");
            colliders.arraySize = 1;
            colliders.GetArrayElementAtIndex(0).objectReferenceValue = interactionCollider;

            // Set the prefab
            var guids = AssetDatabase.FindAssets("t:GameObject HighlightCorner");
            if (guids != null && guids.Length > 0)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
                markersSO.FindProperty("m_CornerObject").objectReferenceValue = prefab;
            }

            markersSO.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}