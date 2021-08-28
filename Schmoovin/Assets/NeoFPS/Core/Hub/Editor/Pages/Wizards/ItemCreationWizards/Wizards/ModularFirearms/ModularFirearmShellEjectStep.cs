using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor;
using NeoFPSEditor.Hub;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ModularFirearms
{
    public class ModularFirearmShellEjectStep : NeoFpsWizardStep
    {
        [SerializeField, Tooltip("How empty cartridges are ejected from the gun.")]
        private int m_ShellEject = 0;

        [Tooltip("A proxy transform where ejected shells will be spawned.")]
        public GameObject shellEjectPoint = null;
        [Tooltip("The shell prefab object to spawn.")]
        public PooledObject shellPrefab = null;
        [Tooltip("The delay time between firing and ejecting a shell if the delay type is set to elapsed time.")]
        public float delay = 0f;
        [Tooltip("The ejected shell speed directly out from the ejector.")]
        public float outSpeed = 5f;
        [Tooltip("The ejected shell speed back over the wielder's shoulder.")]
        public float backSpeed = 1f;
        [Range (0f, 1f), Tooltip("How much of the character's velocity should be added to the ejected shells.")]
        public float inheritVelocity = 0.25f;

        [Tooltip("A prefab with one or more particle systems.")]
        public GameObject particleSystemsPrefab = null;
        [SerializeField, HideInInspector]
        private GameObject m_PreviousPrefab = null;

        [Tooltip("The transform of the object to replace.")]
        public GameObject objectToReplace = null;
        [Tooltip("Should the shell be ejected the moment the weapon fires.")]
        public bool ejectOnFire = false;

        [Tooltip("The transform of the object to replace.")]
        public GameObject[] objectsToReplace = null;
        [Tooltip("Should the ejector swap animated shells that are inactive or ignore them.")]
        public bool swapInactive = false;

        private bool m_CanContinue = false;
        private ReorderableList m_ObjectsToReplaceList = null;

        public override string displayName
        {
            get { return "Shell Ejector Setup"; }
        }
        
        static readonly string[] shellEjectOptions =
        {
            "None. No shell casings will be spawned when the weapon fires.",
            "Pooled object spawner. Spawns objects at the set transform position and rotation and fires them away from the gun.",
            "Particle system. Calls Emit() on a particle system to spawn casings. Use sub-emitters to combine effects.",
            "Swap Object. Disables an object within the weapon hierarchy and spawns a pooled shell casing at the same position and rotation.",
            "Swap multiple objects. Replaces multiple objects within the weapon hierarchy with pooled shell casings."
        };

        static readonly string[] shellEjectSummaries =
        {
            "None",
            "Pooled object spawner",
            "Particle system",
            "Swap Object",
            "Swap multiple objects"
        };

        public enum ShellEjectModule
        {
            None,
            Standard,
            ParticleSystem,
            ObjectSwap,
            MultiObjectSwap
        }
        
        public ShellEjectModule shellEjectModule
        {
            get { return (ShellEjectModule)m_ShellEject; }
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = true;
            switch (shellEjectModule)
            {
                case ShellEjectModule.Standard:
                    m_CanContinue &= shellEjectPoint != null && shellPrefab != null;
                    break;
                case ShellEjectModule.ParticleSystem:
                    m_CanContinue &= particleSystemsPrefab != null;
                    break;
                case ShellEjectModule.ObjectSwap:
                    m_CanContinue &= objectToReplace != null;
                    break;
            }
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            delay = Mathf.Clamp(delay, 0f, 5f);
            outSpeed = Mathf.Clamp(outSpeed, 0f, 10f);
            backSpeed = Mathf.Clamp(backSpeed, 0f, 5f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;
            var root = wizard.steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;
            
            NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("m_ShellEject"), shellEjectOptions);

            switch (shellEjectModule)
            {
                case ShellEjectModule.Standard:
                    {
                        NeoFpsEditorGUI.GameObjectInHierarchyField(serializedObject.FindProperty("shellEjectPoint"), root.weaponObject.transform, false);
                        if (shellEjectPoint == null)
                            NeoFpsEditorGUI.MiniInfo("An object will be automatically created under the view model's weapon object.");
                        m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabComponentField<PooledObject>(serializedObject.FindProperty("shellPrefab"), (obj) => { return obj.GetComponent<IBulletCasing>() != null; });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("delay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("outSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("backSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("inheritVelocity"));
                    }
                    break;
                case ShellEjectModule.ParticleSystem:
                    {
                        m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabField(serializedObject.FindProperty("particleSystemsPrefab"));
                        if (particleSystemsPrefab == null)
                            NeoFpsEditorGUI.MiniInfo("A placeholder particle system object will be added under the weapon object");
                        else
                            NeoFpsEditorGUI.MiniInfo("An instance of the prefab will be added to the weapon, and its particle systems used.");
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("delay"));
                    }
                    break;
                case ShellEjectModule.ObjectSwap:
                    {
                        m_CanContinue &= NeoFpsEditorGUI.RequiredGameObjectInHierarchyField(serializedObject.FindProperty("objectToReplace"), root.viewModel.transform, false);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ejectOnFire"));
                        if (ejectOnFire)
                        {
                            ++EditorGUI.indentLevel;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("delay"));
                            --EditorGUI.indentLevel;
                        }
                    }
                    break;
                case ShellEjectModule.MultiObjectSwap:
                    {
                        if (m_ObjectsToReplaceList == null)
                            m_ObjectsToReplaceList = NeoFpsEditorGUI.GetGameObjectInHierarchyList(serializedObject.FindProperty("objectsToReplace"), () =>
                            { return root.viewModel.transform; }, false);
                        m_ObjectsToReplaceList.DoLayoutList();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("swapInactive"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ejectOnFire"));
                        if (ejectOnFire)
                        {
                            ++EditorGUI.indentLevel;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("delay"));
                            --EditorGUI.indentLevel;
                        }
                    }
                    break;
            }
            m_PreviousPrefab = particleSystemsPrefab;
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.MultiChoiceSummary("m_ShellEject", m_ShellEject, shellEjectSummaries);

            switch (shellEjectModule)
            {
                case ShellEjectModule.Standard:
                    {
                        WizardGUI.ObjectSummary("shellEjectPoint", shellEjectPoint);
                        WizardGUI.ObjectSummary("shellPrefab", shellPrefab);
                        WizardGUI.DoSummary("delay", delay);
                        WizardGUI.DoSummary("outSpeed", outSpeed);
                        WizardGUI.DoSummary("backSpeed", backSpeed);
                        WizardGUI.DoSummary("inheritVelocity", inheritVelocity);
                    }
                    break;
                case ShellEjectModule.ParticleSystem:
                    {
                        WizardGUI.ObjectSummary("particleSystemsPrefab", particleSystemsPrefab);
                        WizardGUI.DoSummary("delay", delay);
                    }
                    break;
                case ShellEjectModule.ObjectSwap:
                    {
                        WizardGUI.ObjectSummary("objectToReplace", objectToReplace);
                        WizardGUI.DoSummary("ejectOnFire", ejectOnFire);
                        if (ejectOnFire)
                            WizardGUI.DoSummary("delay", delay);
                    }
                    break;
                case ShellEjectModule.MultiObjectSwap:
                    {
                        WizardGUI.ObjectListSummary("objectsToReplace", objectsToReplace);
                        WizardGUI.DoSummary("swapInactive", swapInactive);
                        WizardGUI.DoSummary("ejectOnFire", ejectOnFire);
                        if (ejectOnFire)
                            WizardGUI.DoSummary("delay", delay);
                    }
                    break;
            }
        }
    }
}
