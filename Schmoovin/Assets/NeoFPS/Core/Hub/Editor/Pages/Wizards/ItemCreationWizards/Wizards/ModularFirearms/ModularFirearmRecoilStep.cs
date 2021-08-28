using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor;
using NeoFPSEditor.Hub;
using UnityEngine;
using UnityEditor;
using NeoFPSEditor.ModularFirearms;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ModularFirearms
{
    public class ModularFirearmRecoilStep : NeoFpsWizardStep
    {
        [SerializeField, Tooltip("The accuracy decrement per shot in hip fire mode (accuracy has a 0-1 range).")]
        private float m_HipAccuracyKick = 0.075f;
        [SerializeField, Tooltip("The accuracy recovered per second in hip fire mode (accuracy has a 0-1 range).")]
        private float m_HipAccuracyRecover = 0.75f;
        [SerializeField, Tooltip("The accuracy decrement per shot in sighted fire mode (accuracy has a 0-1 range).")]
        private float m_SightedAccuracyKick = 0.025f;
        [SerializeField, Tooltip("The accuracy recovered per second in sighted fire mode (accuracy has a 0-1 range).")]
        private float m_SightedAccuracyRecover = 0.5f;

        [Tooltip("Use procedural spring animation for recoil, or only use animation triggers.")]
        public bool useSpringRecoil = true;
        
        public BetterSpringRecoilHandler.RecoilProfile hipFireRecoil = new BetterSpringRecoilHandler.RecoilProfile();
        public BetterSpringRecoilHandler.RecoilProfile aimedRecoil = new BetterSpringRecoilHandler.RecoilProfile();
        
        public override string displayName
        {
            get { return "Recoil Setup"; }
        }

        public float hipAccuracyKick { get { return m_HipAccuracyKick; } }
        public float hipAccuracyRecover { get { return m_HipAccuracyRecover; } }
        public float sightedAccuracyKick { get { return m_SightedAccuracyKick; } }
        public float sightedAccuracyRecover { get { return m_SightedAccuracyRecover; } }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return true;
        }

        void OnValidate()
        {
            m_HipAccuracyKick = Mathf.Clamp01(m_HipAccuracyKick);
            m_HipAccuracyRecover = Mathf.Clamp(m_HipAccuracyRecover, 0.01f, 10f);
            m_SightedAccuracyKick = Mathf.Clamp01(m_SightedAccuracyKick);
            m_SightedAccuracyRecover = Mathf.Clamp(m_SightedAccuracyRecover, 0.01f, 10f);

            hipFireRecoil.OnValidate();
            aimedRecoil.OnValidate();
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            BaseRecoilHandlerBehaviourEditor.InspectAccuracy(serializedObject);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("useSpringRecoil"));
            if (useSpringRecoil)
            {
                EditorGUILayout.Space();
                BetterSpringRecoilHandlerEditor.InspectRecoilProfile(serializedObject.FindProperty("hipFireRecoil"), "Hip-Fire Recoil", false);
                BetterSpringRecoilHandlerEditor.InspectRecoilProfile(serializedObject.FindProperty("aimedRecoil"), "Aim Down Sights Recoil", false);
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.DoSummary("m_HipAccuracyKick", m_HipAccuracyKick);
            WizardGUI.DoSummary("m_HipAccuracyRecover", m_HipAccuracyRecover);
            WizardGUI.DoSummary("m_SightedAccuracyKick", m_SightedAccuracyKick);
            WizardGUI.DoSummary("m_SightedAccuracyRecover", m_SightedAccuracyRecover);

            GUILayout.Space(4);

            WizardGUI.DoSummary("useSpringRecoil", useSpringRecoil);
            if (useSpringRecoil)
            {
                WizardGUI.DoSummary("Hip-Fire Recoil", "...");
                WizardGUI.DoSummary("Aim Down Sights Recoil", "...");
            }
        }
    }
}
