using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPSEditor.Hub.Pages.ItemCreationWizards.InteractiveObjects;
using NeoFPS;
using UnityEngine.Events;
using UnityEngine.Audio;
using UnityEditor.Events;
using NeoSaveGames.Serialization;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public class InteractiveObjectWizard : NeoFpsWizard
    {
        static readonly string[] k_RootSteps = new string[] { InteractiveObjectWizardSteps.root, InteractiveObjectWizardSteps.audioVisual };

        protected override string[] GetRootSteps()
        {
            return k_RootSteps;
        }

        public override string GetDefaultTemplateFilename()
        {
            return "NewInteractiveObjectTemplate";
        }
        
        protected override void RegisterSteps()
        {
            RegisterStep<InteractiveObjectRootStep>(InteractiveObjectWizardSteps.root);
            RegisterStep<InteractiveObjectAudioVisualStep>(InteractiveObjectWizardSteps.audioVisual);
        }

        public override void CreateItem()
        {
            // Get the save folder
            var folderPath = WizardUtility.GetPrefabOutputFolder();
            if (folderPath == null)
                return;

            var root = steps[InteractiveObjectWizardSteps.root] as InteractiveObjectRootStep;
            var audioVisual = steps[InteractiveObjectWizardSteps.audioVisual] as InteractiveObjectAudioVisualStep;

            // Get prefab name
            string prefabName = root.prefabName;
            if (root.autoPrefix)
                prefabName = "Interactive_" + prefabName;

            // Get prefab path
            var path = string.Format("{0}/{1}.prefab", folderPath, prefabName);
            if (!root.overwriteExisting)
                path = AssetDatabase.GenerateUniqueAssetPath(path);

            // Create root object
            var rootObject = new GameObject(prefabName);
            var displayObject = GetDisplayObject(audioVisual, rootObject.transform, prefabName);

            // Add trigger collider to root
            var rbc = rootObject.AddComponent<BoxCollider>();
            rbc.isTrigger = true;

            // Set up interactive object
            rootObject.layer = PhysicsFilter.LayerIndex.InteractiveObjects;
            var interactiveObject = rootObject.AddComponent<InteractiveObject>();
            var interactiveObjectSO = new SerializedObject(interactiveObject);
            interactiveObjectSO.FindProperty("m_TooltipName").stringValue = root.tooltipName;
            interactiveObjectSO.FindProperty("m_TooltipAction").stringValue = root.tooltipAction;
            interactiveObjectSO.ApplyModifiedPropertiesWithoutUndo();

            // Add highlight
            int highlight = audioVisual.highlight;
            if (highlight == 1)
                AddCornerMarkers(rootObject, rbc);
            if (highlight == 2)
                rootObject.AddComponent<InteractiveObjectMaterialMarker>();

            // Add audio
            if (audioVisual.interactionAudio != null || audioVisual.lookAtAudio != null || audioVisual.lookAwayAudio != null)
            {
                var audioSource = AddAudio(audioVisual, rootObject);
                if (audioVisual.interactionAudio != null)
                    UnityEventTools.AddObjectPersistentListener(interactiveObject.onUsedUnityEvent, audioSource.PlayOneShot, audioVisual.interactionAudio);
                if (audioVisual.lookAtAudio != null)
                    UnityEventTools.AddObjectPersistentListener(interactiveObject.onCursorEnterUnityEvent, audioSource.PlayOneShot, audioVisual.lookAtAudio);
                if (audioVisual.lookAwayAudio != null)
                    UnityEventTools.AddObjectPersistentListener(interactiveObject.onCursorExitUnityEvent, audioSource.PlayOneShot, audioVisual.lookAwayAudio);
            }

            // Add animations
            if (audioVisual.animatorController != null && (!string.IsNullOrEmpty(audioVisual.useAnimationTrigger) || !string.IsNullOrEmpty(audioVisual.lookAnimationBool)))
            {
                var animator = displayObject.GetComponent<Animator>();
                if (animator != null)
                {
                    var animatorSO = new SerializedObject(animator);
                    animatorSO.FindProperty("m_Controller").objectReferenceValue = audioVisual.animatorController;
                    animatorSO.ApplyModifiedProperties();

                    // Use animation trigger
                    if (!string.IsNullOrEmpty(audioVisual.useAnimationTrigger))
                        UnityEventTools.AddStringPersistentListener(interactiveObject.onUsedUnityEvent, animator.SetTrigger, audioVisual.useAnimationTrigger);

                    // Look at / away animation bool
                    if (!string.IsNullOrEmpty(audioVisual.lookAnimationBool))
                    {
                        var helper = displayObject.AddComponent<AnimatorUnityEventHelper>();
                        var helperSO = new SerializedObject(helper);
                        helperSO.FindProperty("m_BoolKey").stringValue = audioVisual.lookAnimationBool;
                        helperSO.ApplyModifiedPropertiesWithoutUndo();

                        UnityEventTools.AddVoidPersistentListener(interactiveObject.onCursorEnterUnityEvent, helper.SetBoolTrue);
                        UnityEventTools.AddVoidPersistentListener(interactiveObject.onCursorExitUnityEvent, helper.SetBoolFalse);
                    }

                    var nsgo = displayObject.AddComponent<NeoSerializedGameObject>();
                    var nsgoSO = new SerializedObject(nsgo);
                    SerializedArrayUtility.Add(nsgoSO.FindProperty("m_OtherComponents"), animator);
                    nsgoSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // Add save games
            rootObject.AddComponent<NeoSerializedGameObject>();

            // Create prefab and highlight in project view
            var prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, path);
            AssetDatabase.Refresh();
            DestroyImmediate(rootObject);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);
            
            Debug.Log("Creating Interactive Object");
        }

        AudioSource AddAudio(InteractiveObjectAudioVisualStep avStep, GameObject targetObject)
        {
            // Add audio source
            var source = targetObject.AddComponent<AudioSource>();
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

            return source;
        }

        GameObject GetDisplayObject(InteractiveObjectAudioVisualStep avStep, Transform parent, string objectName)
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
            
            switch (avStep.physicsType)
            {
                case 1: // Static Large
                    {
                        // Set layer
                        renderObject.layer = PhysicsFilter.LayerIndex.EnvironmentDetail;

                        // Add child object for physics
                        var physicsObject = new GameObject("PhysicsRough");
                        physicsObject.transform.SetParent(renderObject.transform);
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

        void AddCollider(InteractiveObjectAudioVisualStep avStep, GameObject renderObject, GameObject physicsObject)
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