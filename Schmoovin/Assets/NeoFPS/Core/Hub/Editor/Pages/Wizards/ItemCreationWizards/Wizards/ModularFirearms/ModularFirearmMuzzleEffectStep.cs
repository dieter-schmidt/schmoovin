using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor;
using NeoFPSEditor.Hub;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ModularFirearms
{
    public class ModularFirearmMuzzleEffectStep : NeoFpsWizardStep
    {
        [SerializeField, Tooltip("The muzzle flare VFX type to use.")]
        private int m_MuzzleEffect = -1;

        [Tooltip("How the muzzle effect objects are assigned.")]
        public int muzzleEffectObjectType = -1;
        [Tooltip("The muzzle flash prefab.")]
        public GameObject muzzleFlashPrefab = null;
        [Tooltip("The muzzle flash prefabs.")]
        public GameObject[] muzzleFlashPrefabs = { };
        [Tooltip("The muzzle flash game object (child of the view model weapon).")]
        public GameObject muzzleFlashObject = null;
        [Tooltip("The muzzle flash game objects (children of the view model weapon).")]
        public GameObject[] muzzleFlashObjects = { };
        [Tooltip("The muzzle flash mesh.")]
        public Mesh muzzleFlashMesh = null;
        [Tooltip("The muzzle flash mesh material.")]
        public Material muzzleFlashMaterial = null;
        [Tooltip("The muzzle flash meshes.")]
        public Mesh[] muzzleFlashMeshes = { };
        [Range(0f, 1f), Tooltip("The duration the flash should remain visible in seconds.")]
        public float muzzleFlashDuration = 0.05f;

        [Tooltip("The particle system to play.")]
        public ParticleSystem particleSystem = null;
        [Tooltip("The particle system type.")]
        public int particleSystemOption = -1;

        [Tooltip("The effect transform will be reparented under the character so it persists between weapon switches and handles character movement better.")]
        public Transform effectTransform = null;
        [Tooltip("The amount of time after a shot, that the particle effect transform will sync with the weapon. If you have particle systems that emit over time then ensure this duration is long enough to cover that.")]
        public float followDuration = 0f;
        [Tooltip("The prototype prefab for the advanced particle system.")]
        public GameObject advancedParticleSystemPrefab = null;
        [SerializeField, HideInInspector] private GameObject m_PreviousPrefab = null;
        [Tooltip("The particle systems to play.")]
        public AdvancedParticleMuzzleEffect.ParticlesInfo[] particleSystems = { };

        [Tooltip("The audio clips to use when firing. Chosen at random.")]
        public AudioClip[] firingSounds = { };

        private bool m_CanContinue = false;
        private ReorderableList m_FlashPrefabsList = null;
        private ReorderableList m_FlashObjectsList = null;
        private ReorderableList m_FlashMeshesList = null;
        private ReorderableList m_GunshotAudioList = null;

        public override string displayName
        {
            get { return "Muzzle Effects Setup"; }
        }
        
        static readonly string[] muzzleEffectOptions =
        {
            "GameObject. Enables a gameobject for a very brief period and then disables it again. This can work well for simple low-poly or toon style effects.",
            "Multiple objects. Picks a random gameobject from a group, enables it for a set period and then disables it again. This can be used with particle systems or simple objects for a variety of styles.",
            "Particle system. Calls Emit() on the specified particle system for every shot. Use sub-emitters to combine effects.",
            "Advanced particles. A more complex particle system based effect, which can simulate particles in character space, and allows particles to persist when the weapon is switched.",
            "Audio Only. Plays a sound on firing, but has no visual effect. Useful for weapons like bows."
        };

        static readonly string[] muzzleEffectSummaries =
        {
            "GameObject",
            "Multiple objects",
            "Particle system",
            "Advanced particles",
            "Audio Only"
        };

        static readonly string[] muzzleFlashObjectOptions =
        {
            "Prefab",
            "Child Object",
            "Mesh"
        };

        static readonly string[] particleSystemOptions =
        {
            "Prefab",
            "Child Object"
        };

        public enum MuzzleEffectModule
        {
            Undefined,
            BasicGameObject,
            RandomObject,
            ParticleSystem,
            AdvancedParticleSystem,
            AudioOnly
        }

        public MuzzleEffectModule muzzleEffectModule
        {
            get { return (MuzzleEffectModule)(m_MuzzleEffect + 1); }
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            m_CanContinue &= m_MuzzleEffect != -1;

            switch (muzzleEffectModule)
            {
                case MuzzleEffectModule.BasicGameObject:
                    {
                        m_CanContinue &= muzzleEffectObjectType != -1;
                        switch (muzzleEffectObjectType)
                        {
                            case 0: // Prefab
                                m_CanContinue &= muzzleFlashPrefab != null;
                                break;
                            case 1: // Child
                                m_CanContinue &= muzzleFlashObject != null;
                                break;
                            case 2: // Mesh
                                m_CanContinue &= muzzleFlashMesh != null;
                                m_CanContinue &= muzzleFlashMaterial != null;
                                break;
                        }
                    }
                    break;
                case MuzzleEffectModule.RandomObject:
                    {
                        m_CanContinue &= muzzleEffectObjectType != -1;
                        if (muzzleEffectObjectType == 2)
                            m_CanContinue &= muzzleFlashMaterial != null;
                    }
                    break;
                case MuzzleEffectModule.ParticleSystem:
                    {
                        m_CanContinue &= particleSystem != null;
                    }
                    break;
                case MuzzleEffectModule.AdvancedParticleSystem:
                    {
                        m_CanContinue &= advancedParticleSystemPrefab != null;
                    }
                    break;
            }
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            followDuration = Mathf.Clamp(followDuration, 0f, 10f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;
            var root = wizard.steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            
            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("m_MuzzleEffect"), muzzleEffectOptions);

            bool assignAudio = true;
            switch (muzzleEffectModule)
            {
                case MuzzleEffectModule.BasicGameObject:
                    {
                        m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("muzzleEffectObjectType"), muzzleFlashObjectOptions);
                        switch(muzzleEffectObjectType)
                        {
                            case 0: // Prefab
                                m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabField(serializedObject.FindProperty("muzzleFlashPrefab"));
                                break;
                            case 1: // Child
                                m_CanContinue &= NeoFpsEditorGUI.RequiredGameObjectInHierarchyField(serializedObject.FindProperty("muzzleFlashObject"), root.weaponObject.transform, false);
                                break;
                            case 2: // Mesh
                                m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("muzzleFlashMesh"));
                                m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("muzzleFlashMaterial"));
                                break;
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("muzzleFlashDuration"));
    }
                    break;
                case MuzzleEffectModule.RandomObject:
                    {
                        m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("muzzleEffectObjectType"), muzzleFlashObjectOptions);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("muzzleFlashDuration"));
                        switch (muzzleEffectObjectType)
                        {
                            case 0: // Prefab
                                if (m_FlashPrefabsList == null)
                                    m_FlashPrefabsList = NeoFpsEditorGUI.GetPrefabList(serializedObject.FindProperty("muzzleFlashPrefabs"));
                                m_FlashPrefabsList.DoLayoutList();
                                break;
                            case 1: // Child
                                if (m_FlashObjectsList == null)
                                    m_FlashObjectsList = NeoFpsEditorGUI.GetGameObjectInHierarchyList(serializedObject.FindProperty("muzzleFlashObjects"), ()=> {
                                        return root.weaponObject != null ? root.weaponObject.transform : null;
                                    }, false);
                                m_FlashObjectsList.DoLayoutList();
                                break;
                            case 2: // Mesh
                                if (m_FlashMeshesList == null)
                                    m_FlashMeshesList = NeoFpsEditorGUI.GetObjectList(serializedObject.FindProperty("muzzleFlashMeshes"));
                                m_FlashMeshesList.DoLayoutList();
                                m_CanContinue &= NeoFpsEditorGUI.RequiredObjectField(serializedObject.FindProperty("muzzleFlashMaterial"));
                                break;
                        }
                    }
                    break;
                case MuzzleEffectModule.ParticleSystem:
                    {
                        m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("particleSystemOption"), particleSystemOptions);
                        switch (particleSystemOption)
                        {
                            case 0:
                                m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabComponentField<ParticleSystem>(serializedObject.FindProperty("particleSystem"));
                                break;
                            case 1:
                                m_CanContinue &= NeoFpsEditorGUI.RequiredComponentInHierarchyField<ParticleSystem>(serializedObject.FindProperty("particleSystem"), root.weaponObject.transform, true);
                                break;
                        }
                    }
                    break;
                case MuzzleEffectModule.AdvancedParticleSystem:
                    {
                        m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabField(serializedObject.FindProperty("advancedParticleSystemPrefab"), (obj) => { return obj.GetComponentInChildren<ParticleSystem>() != null; });
                        if (advancedParticleSystemPrefab == null)
                            NeoFpsEditorGUI.MiniInfo("A placeholder muzzle flash object will be added under the weapon object");
                        else
                        {
                            if (advancedParticleSystemPrefab.GetComponent<AdvancedParticleMuzzleEffect>() != null)
                            {
                                NeoFpsEditorGUI.MiniInfo("The prefab contains an AdvancedParticleMuzzleEffect module component, and will be instantiated under the weapon object.");
                                assignAudio = false;
                            }
                            else
                            {
                                NeoFpsEditorGUI.MiniInfo("The prefab contains multiple Unity particle systems. You can set them up in the list below.");

                                // Set up particle systems
                                var psProp = serializedObject.FindProperty("particleSystems");
                                if (m_PreviousPrefab != advancedParticleSystemPrefab)
                                {
                                    var psChildren = advancedParticleSystemPrefab.GetComponentsInChildren<ParticleSystem>();
                                    psProp.arraySize = psChildren.Length;
                                    for (int i = 0; i < psChildren.Length; ++i)
                                    {
                                        var info = psProp.GetArrayElementAtIndex(i);
                                        info.FindPropertyRelative("particleSystem").objectReferenceValue = psChildren[i];
                                        info.FindPropertyRelative("space").enumValueIndex = 0;
                                    }
                                }

                                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                                {
                                    for (int i = 0; i < psProp.arraySize; ++i)
                                    {
                                        var info = psProp.GetArrayElementAtIndex(i);
                                        NeoFpsEditorGUI.ComponentInHierarchyField<ParticleSystem>(info.FindPropertyRelative("particleSystem"), advancedParticleSystemPrefab.transform, true);
                                        ++EditorGUI.indentLevel;
                                        EditorGUILayout.PropertyField(info.FindPropertyRelative("space"));
                                        --EditorGUI.indentLevel;
                                    }
                                }

                                EditorGUILayout.PropertyField(serializedObject.FindProperty("followDuration"));
                            }
                        }
                    }
                    break;
            }
            m_PreviousPrefab = advancedParticleSystemPrefab;

            EditorGUILayout.Space();

            if (assignAudio)
            {
                if (m_GunshotAudioList == null)
                    m_GunshotAudioList = NeoFpsEditorGUI.GetObjectList(serializedObject.FindProperty("firingSounds"));
                m_GunshotAudioList.DoLayoutList();
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.MultiChoiceSummary("m_MuzzleEffect", m_MuzzleEffect, muzzleEffectSummaries);

            switch (muzzleEffectModule)
            {
                case MuzzleEffectModule.BasicGameObject:
                    {
                        WizardGUI.MultiChoiceSummary("muzzleEffectObjectType", muzzleEffectObjectType, muzzleFlashObjectOptions);
                        switch (muzzleEffectObjectType)
                        {
                            case 0: // Prefab
                                WizardGUI.ObjectSummary("muzzleFlashPrefab", muzzleFlashPrefab);
                                break;
                            case 1: // Child
                                WizardGUI.ObjectSummary("muzzleFlashObject", muzzleFlashObject);
                                break;
                            case 2: // Mesh
                                WizardGUI.ObjectSummary("muzzleFlashMesh", muzzleFlashMesh);
                                break;
                        }
                        WizardGUI.DoSummary("muzzleFlashDuration", muzzleFlashDuration);
                    }
                    break;
                case MuzzleEffectModule.RandomObject:
                    {
                        WizardGUI.MultiChoiceSummary("muzzleEffectObjectType", muzzleEffectObjectType, muzzleFlashObjectOptions);
                        switch (muzzleEffectObjectType)
                        {
                            case 0: // Prefab
                                WizardGUI.ObjectListSummary("muzzleFlashPrefabs", muzzleFlashPrefabs);
                                break;
                            case 1: // Child
                                WizardGUI.ObjectListSummary("muzzleFlashObjects", muzzleFlashObjects);
                                break;
                            case 2: // Mesh
                                WizardGUI.ObjectListSummary("muzzleFlashMeshes", muzzleFlashMeshes);
                                break;
                        }
                    }
                    break;
                case MuzzleEffectModule.ParticleSystem:
                    {
                        WizardGUI.ObjectSummary("particleSystem", particleSystem);
                    }
                    break;
                case MuzzleEffectModule.AdvancedParticleSystem:
                    {
                        WizardGUI.ObjectSummary("advancedParticleSystemPrefab", advancedParticleSystemPrefab);
                        if (advancedParticleSystemPrefab != null && advancedParticleSystemPrefab.GetComponent<AdvancedParticleMuzzleEffect>() == null)
                        {
                            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                for (int i = 0; i < particleSystems.Length; ++i)
                                {
                                    WizardGUI.ObjectSummary("particleSystem", particleSystems[i].particleSystem);
                                    ++EditorGUI.indentLevel;
                                    WizardGUI.DoSummary("space", particleSystems[i].space.ToString());
                                    --EditorGUI.indentLevel;
                                }
                            }

                            WizardGUI.DoSummary("followDuration", followDuration);
                        }
                    }
                    break;
            }

            GUILayout.Space(4);
            
            WizardGUI.ObjectListSummary("firingSounds", firingSounds);
        }
    }
}
