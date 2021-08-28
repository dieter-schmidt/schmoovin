using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS.Constants;
using UnityEditor.Events;
using UnityEngine.Events;
using NeoFPS;
using UnityEditor.Animations;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.InteractiveObjects
{
    class InteractiveObjectAudioVisualStep : NeoFpsWizardStep
    {
        [Tooltip("What type of object do you want to use to represent the pickup")]
        public int displayObjectType = -1;
        [Tooltip("The mesh to use for the pickup's display object.")]
        public Mesh renderMesh = null;
        [Tooltip("The material to use on the mesh renderer.")]
        public Material material = null;
        [Tooltip("The geometry object to show for the pickup.")]
        public GameObject modelPrefab = null;
        [Tooltip("Set a material to apply to the render geometry instead of the material it already uses.")]
        public bool useCustomMaterial = false;
        [Tooltip("The object to show for the pickup.")]
        public GameObject displayPrefab = null;
        [Tooltip("The type of interaction highlight to use when aiming at the object.")]
        public int highlight = 0;

        [Tooltip("The audio clip to play on interacting with the object.")]
        public AudioClip interactionAudio = null;
        [Tooltip("The audio clip to play when the player looks at the object.")]
        public AudioClip lookAtAudio = null;
        [Tooltip("The audio clip to play when the player looks at the object.")]
        public AudioClip lookAwayAudio = null;

        [Tooltip("The animator controller to apply to the display object's Animator component.")]
        public AnimatorController animatorController = null;
        [Tooltip("The name of a trigger parameter in the attached Animator. This will be fired when you use the object.")]
        public string useAnimationTrigger = string.Empty;
        [Tooltip("The name of a bool parameter in the attached Animator. This will be set to true when you look at the object up close, and false when you look away.")]
        public string lookAnimationBool = string.Empty;

        [Tooltip("How the object is physically represented in the world.")]
        public int physicsType = -1;
        [Tooltip("The physical mass (kg) of the pickup object.")]
        public float mass = 1f;
        [Tooltip("The type of collider the pickup object should use.")]
        public int colliderType = 0;
        [Tooltip("The mesh to use for the pickup's collider.")]
        public Mesh colliderMesh = null;
        [Tooltip("The surface material of the object, used by the NeoFPS surface manager for hit effects and audio.")]
        public FpsSurfaceMaterial surfaceMaterial = FpsSurfaceMaterial.Default;

        private bool m_CanContinue = false;
        
        static readonly string[] renderTypeOptions =
        {
            "Mesh. This allows you to select an individual mesh from within a geometry asset such as an FBX.",
            "Model. This allows you to select a geometry asset such as an FBX to use as is.",
            "Prefab. This allows you to select any prefab object, regardless of whether it has a mesh renderer."
        };

        static readonly string[] renderTypeSummaries =
        {
            "Mesh",
            "Model",
            "Prefab"
        };

        static readonly string[] physicsTypeOptions =
        {
            "No physics. The pickup will be static in the scene and characters (including the player) pass right through it.",
            "Static physics. The pickup will be static in the scene and characters will collide with and walk over it.",
            "Small Dynamic Object. The pickup is influenced by gravity, collides with the environment and can be shot, but characters will pass through it.",
            "Large Dynamic Object. The pickup is influenced by gravity, collides with the ground, can be shot and pushed, and collides with characters."
        };

        static readonly string[] physicsTypeSummaries =
        {
            "No physics",
            "Static physics",
            "Small Dynamic Object",
            "Large Dynamic Object"
        };

        static readonly string[] colliderOptionsStatic =
        {
            "Box",
            "Sphere",
            "Capsule",
            "Mesh (Convex)",
            "Mesh (Non-Convex)"
        };

        static readonly string[] colliderOptionsDynamic =
        {
            "Box",
            "Sphere",
            "Capsule",
            "Mesh (Convex)"
        };

        static readonly string[] highlightOptions =
        {
            "None. The object is interactive, but will not be highlighted when you aim at it.",
            "Corner Markers. The object will show markers over the corners of its trigger box collider when you aim at it.",
            "Material. If the object uses a valid highlight material such as **, then it will shine when you aim at it."
        };

        static readonly string[] highlightSummaries =
        {
            "None",
            "Corner Markers",
            "Material"
        };

        public override string displayName
        {
            get { return "Audio / Visuals Setup"; }
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            // Check display options
            switch (displayObjectType)
            {
                case -1:
                    m_CanContinue = false;
                    break;
                case 0: // Mesh
                    if (renderMesh == null || material == null)
                        m_CanContinue = false;
                    break;
                case 1: // Model
                    if (modelPrefab == null)
                        m_CanContinue = false;
                    if (useCustomMaterial && material == null)
                        m_CanContinue = false;
                    break;
                case 2: // Prefab
                    if (displayPrefab == null)
                        m_CanContinue = false;
                    break;
            }

            if (physicsType > 0 && colliderType > 2 && colliderMesh == null)
                m_CanContinue = false;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            NeoFpsEditorGUI.Header("Render");

            Animator animator = null;

            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("displayObjectType"), renderTypeOptions);
            switch (displayObjectType)
            {
                case 0: // Mesh
                    {
                        m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("renderMesh"));
                        m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("material"));
                    }
                    break;
                case 1: // Model
                    {
                        m_CanContinue &= NeoFpsEditorGUI.RequiredModelPrefabField(serializedObject.FindProperty("modelPrefab"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("useCustomMaterial"));
                        if (useCustomMaterial)
                            m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("material"));
                        if (modelPrefab != null)
                            animator = modelPrefab.GetComponent<Animator>();
                    }
                    break;
                case 2: // Prefab
                    {
                        m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabField(serializedObject.FindProperty("displayPrefab"));
                        if (displayPrefab != null)
                            animator = displayPrefab.GetComponent<Animator>();
                    }
                    break;
            }

            NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("highlight"), highlightOptions);

            if (animator != null)
            {
                NeoFpsEditorGUI.Header("Animation");

                m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("animatorController"));
                if (animatorController != null)
                {
                    NeoFpsEditorGUI.AnimatorTriggerKeyField(serializedObject.FindProperty("useAnimationTrigger"), animatorController);
                    NeoFpsEditorGUI.AnimatorBoolKeyField(serializedObject.FindProperty("lookAnimationBool"), animatorController);
                }
            }

            NeoFpsEditorGUI.Header("Audio");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("interactionAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lookAtAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lookAwayAudio"));

            NeoFpsEditorGUI.Header("Physics");

            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("physicsType"), physicsTypeOptions);
            if (physicsType > 1)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mass"));
            if (physicsType > 0)
            {
                NeoFpsEditorGUI.DropdownField(serializedObject.FindProperty("colliderType"), physicsType < 2 ? colliderOptionsStatic : colliderOptionsDynamic);
                if (colliderType > 2)
                    m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("colliderMesh"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceMaterial"));
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.MultiChoiceSummary("Display Object Type", displayObjectType, renderTypeSummaries);
            switch (displayObjectType)
            {
                case 0: // Mesh
                    {
                        WizardGUI.ObjectSummary("Render Mesh", renderMesh);
                        WizardGUI.ObjectSummary("Material", material);
                    }
                    break;
                case 1: // Model
                    {
                        WizardGUI.ObjectSummary("Model Prefab", modelPrefab);
                        WizardGUI.DoSummary("Use Custom Material", useCustomMaterial);
                        if (useCustomMaterial)
                            WizardGUI.ObjectSummary("Material", material);
                    }
                    break;
                case 2: // Prefab
                    {
                        WizardGUI.ObjectSummary("Display Prefab", displayPrefab);
                    }
                    break;
            }
            
            WizardGUI.MultiChoiceSummary("Highlight", highlight, highlightSummaries);

            EditorGUILayout.Space();

            WizardGUI.ObjectSummary("Animator Controller", animatorController);
            WizardGUI.ObjectSummary("Use Animation Trigger", useAnimationTrigger);
            WizardGUI.ObjectSummary("Look Animation Bool", lookAnimationBool);

            EditorGUILayout.Space();

            WizardGUI.ObjectSummary("Interaction Audio", interactionAudio);
            WizardGUI.ObjectSummary("Look At Audio", lookAtAudio);
            WizardGUI.ObjectSummary("Look Away Audio", lookAwayAudio);

            EditorGUILayout.Space();
            
            WizardGUI.MultiChoiceSummary("Physics Type", physicsType, physicsTypeSummaries);
            if (physicsType > 1)
                WizardGUI.DoSummary("Mass", mass);
            if (physicsType > 0)
            {
                WizardGUI.MultiChoiceSummary("Collider Type", colliderType, colliderOptionsStatic);
                if (colliderType > 2)
                    WizardGUI.ObjectSummary("Collider Mesh", colliderMesh);
                WizardGUI.MultiChoiceSummary("Surface Material", surfaceMaterial, FpsSurfaceMaterial.names);
            }
        }
    }
}
