using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    public abstract class NeoFpsWizard : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<NeoFpsWizardStep> m_StepValues = new List<NeoFpsWizardStep>();
        [SerializeField] private List<string> m_StepKeys = new List<string>();
        
        private NeoFpsWizardStep m_CurrentStep = null;
        private NeoFpsWizardStepEditor m_CurrentStepEditor = null;
        private List<string> m_OrderedSteps = new List<string>();
        
        protected abstract void RegisterSteps();
        protected abstract string[] GetRootSteps();
        public abstract void CreateItem();
        public abstract string GetDefaultTemplateFilename();

        public Dictionary<string, NeoFpsWizardStep> steps
        {
            get;
            private set;
        }

        void OnValidate()
        {
            RegisterSteps();
        }

        public void CheckStartingState()
        {
            foreach (var step in steps.Values)
            {
                if (step != null)
                    step.CheckStartingState(this);
            }
        }

        public void OnBeforeSerialize()
        { }

        public void OnAfterDeserialize()
        {
            // Clear dictionary
            if (steps == null)
                steps = new Dictionary<string, NeoFpsWizardStep>();
            else
                steps.Clear();

            // Check steps & keys match for dictionary
            if (m_StepKeys.Count != m_StepValues.Count)
            {
                Debug.LogError("Step value and key counts do not match");
                m_StepValues.Clear();
                m_StepKeys.Clear();
            }

            // Rebuild dictionary
            for (int i = 0; i < m_StepValues.Count; ++i)
                steps.Add(m_StepKeys[i], m_StepValues[i]);
        }

        void Awake()
        {
            OnAfterDeserialize();
            RegisterSteps();
            CheckStartingState();
        }

        protected void RegisterStep<T>(string key) where T : NeoFpsWizardStep
        {
            if (!steps.ContainsKey(key))
            {
                var step = CreateInstance<T>();
                step.name = key;
                step.wizard = this;

                m_StepValues.Add(step);
                m_StepKeys.Add(key);
                steps.Add(key, step);
            }
        }

        public void ResetSteps()
        {
            m_StepValues.Clear();
            m_StepKeys.Clear();
            OnAfterDeserialize();
            RegisterSteps();
        }

        public string GetStepTitle(string key)
        {
            if (steps.ContainsKey(key))
                return steps[key].displayName;
            else
                return "Step Not Found";
        }

        public void DoLayoutStep(string step)
        {
            if (steps.ContainsKey(step))
            {
                var nextStep = steps[step];
                if (m_CurrentStep != nextStep)
                {
                    m_CurrentStep = nextStep;
                    m_CurrentStepEditor = Editor.CreateEditor(nextStep) as NeoFpsWizardStepEditor;
                }
            }

            if (m_CurrentStepEditor != null)
                m_CurrentStepEditor.LayoutEditor(this);
            else
                Debug.LogError("Step editor not found with key: " + step);
        }

        public string DoLayoutSummary(bool hub)
        {
            string result = "summary";

            var rootSteps = GetRootSteps();
            foreach (var s in rootSteps)
            {
                var selected = DoLayoutSummaryRecursive(s, 0, hub);
                if (selected != null)
                    result = selected;
            }

            return result;
        }

        string DoLayoutSummaryRecursive(string step, int indent, bool hub)
        {
            string result = null;
            if (steps.ContainsKey(step))
            {
                var s = steps[step];
                if (s.LayoutSummary(this, indent, hub))
                    result = step;

                var children = s.GetNextSteps();
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        var selected = DoLayoutSummaryRecursive(child, indent + 1, hub);
                        if (selected != null)
                            result = selected;
                    }
                }
            }
            else
                Debug.LogError("Step editor not found with key: " + step);

            return result;
        }

        public string GetStartingStep()
        {
            var rootSteps = GetRootSteps();
            foreach (var step in rootSteps)
            {
                var incomplete = GetIncompleteStepRecursive(step);
                if (incomplete != null)
                    return incomplete;
            }

            return "summary";
        }

        public string GetIncompleteStepRecursive(string step)
        {
            if (steps.ContainsKey(step))
            {
                var info = steps[step];
                if (!info.CheckCanContinue(this))
                    return step;

                var children = info.GetNextSteps();
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        var incomplete = GetIncompleteStepRecursive(child);
                        if (incomplete != null)
                            return incomplete;
                    }
                }

                return null;
            }
            else
                return step;
        }

        public bool CheckCanComplete()
        {
            var rootSteps = GetRootSteps();
            foreach (var step in rootSteps)
            {
                if (!CheckCanContinueRecursive(step))
                    return false;
            }

            return true;
        }

        public bool CheckCanContinue(string step)
        {
            if (steps.ContainsKey(step))
                return steps[step].CheckCanContinue(this);
            else
                return false;
        }

        bool CheckCanContinueRecursive(string step)
        {
            if (steps.ContainsKey(step))
            {
                // Check this step
                var s = steps[step];
                if (!s.CheckCanContinue(this))
                    return false;

                // Check its children
                var children = s.GetNextSteps();
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        if (!CheckCanContinueRecursive(child))
                            return false;
                    }
                }

                return true;
            }
            else
                return false;
        }

        public List<string> GetOrderedSteps()
        {
            m_OrderedSteps.Clear();

            var rootSteps = GetRootSteps();
            foreach (var step in rootSteps)
                AppendStepsRecursive(step);

            return m_OrderedSteps;
        }

        void AppendStepsRecursive(string step)
        {
            if (!steps.ContainsKey(step))
                return;
            if (m_OrderedSteps.IndexOf(step) != -1)
            {
                Debug.LogError("Attempting to add wizard step multiple times. This will cause an infinite loop. Step: " + step);
                return;
            }

            m_OrderedSteps.Add(step);

            var children = steps[step].GetNextSteps();
            if (children != null)
            {
                foreach (var child in children)
                    AppendStepsRecursive(child);
            }
        }

        public bool SaveAsTemplate()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Save wizard template",
                GetDefaultTemplateFilename(),
                "asset",
                "Save this wizard setup as a template for future use.");

            if (path.Length != 0)
            {
                AssetDatabase.CreateAsset(this, path);
                for (int i = 0; i < m_StepValues.Count; ++i)
                {
                    if (m_StepValues[i] != null)
                    {
                        AssetDatabase.AddObjectToAsset(m_StepValues[i], this);
                        m_StepValues[i].hideFlags |= HideFlags.HideInHierarchy;
                    }
                }
                AssetDatabase.SaveAssets();
                return true;
            }

            return false;
        }

        public NeoFpsWizard Clone()
        {
            var result = Instantiate(this);
            for (int i = 0; i < m_StepValues.Count; ++i)
                result.m_StepValues[i] = Instantiate(m_StepValues[i]);
            result.OnAfterDeserialize();
            return result;
        }
    }
}