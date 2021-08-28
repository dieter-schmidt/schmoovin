using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards
{
    [CustomEditor(typeof(NeoFpsWizard), true)]
    public class NeoFpsWizardEditor : Editor
    {
        public event UnityAction onResetWizard = null;
        public event UnityAction onSaveAsTemplate = null;

        public string currentStep = null;

        private Vector2 m_ScrollPosition = Vector2.zero;

        public override void OnInspectorGUI()
        {
            DoLayoutInline(false);
        }

        public void DoLayoutInline(bool showHubControls)
        {
            serializedObject.UpdateIfRequiredOrScript();

            var cast = target as NeoFpsWizard;

            if (currentStep == null)
                currentStep = cast.GetStartingStep();

            using (new EditorGUILayout.VerticalScope())
            {
                switch (currentStep)
                {
                    case null:
                        EditorGUILayout.HelpBox("There was an error getting the correct wizard step", MessageType.Error);
                        break;
                    case "summary":
                        {
                            // Draw step title
                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);
                            EditorGUILayout.Space();

                            var width = EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth;

                            // Draw step inside scroll view
                            using (var scroll = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
                            {
                                using (new EditorGUILayout.VerticalScope(GUILayout.Width(width - 8)))
                                {
                                    m_ScrollPosition = scroll.scrollPosition;
                                    try
                                    {
                                        currentStep = cast.DoLayoutSummary(showHubControls);
                                    }
                                    catch (System.Exception e)
                                    {
                                        if (e.GetType() == typeof(ExitGUIException))
                                            throw (e);
                                        else
                                            Debug.LogError("Exception caught in item creation wizard: " + e.Message);
                                    }
                                }
                                GUILayout.FlexibleSpace();
                            }

                            // Step controls
                            GUILayout.Space(4);
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (showHubControls)
                                {
                                    // Reset
                                    if (GUILayout.Button("Reset", GUILayout.Width(100)))
                                    {
                                        if (onResetWizard != null)
                                            onResetWizard();
                                        else
                                        {
                                            cast.ResetSteps();
                                            serializedObject.ApplyModifiedProperties();
                                            currentStep = null;
                                            throw new ExitGUIException();
                                        }
                                    }

                                    GUILayout.Space(20);

                                    if (GUILayout.Button("Save As Template"))
                                    {
                                        if (cast.SaveAsTemplate() && onSaveAsTemplate != null)
                                            onSaveAsTemplate();
                                    }
                                }

                                if (GUILayout.Button("Create Item"))
                                {
                                    cast.CreateItem();
                                }
                            }
                            GUILayout.Space(4);
                        }
                        break;
                    default:
                        {
                            // Draw step title
                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("Current Step: " + cast.GetStepTitle(currentStep), EditorStyles.boldLabel);
                            EditorGUILayout.Space();

                            // Draw step inside scroll view
                            using (var scroll = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
                            {
                                m_ScrollPosition = scroll.scrollPosition;
                                try
                                {
                                    cast.DoLayoutStep(currentStep);
                                }
                                catch (System.Exception e)
                                {
                                    if (e.GetType() == typeof(ExitGUIException))
                                        throw (e);
                                    else
                                        Debug.LogError("Exception caught in item creation wizard: " + e.Message);
                                }

                                GUILayout.FlexibleSpace();

                            }

                            // Step controls
                            GUILayout.Space(4);
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (showHubControls)
                                {
                                    // Reset
                                    if (GUILayout.Button("Reset", GUILayout.Width(100)))
                                    {
                                        if (onResetWizard != null)
                                            onResetWizard();
                                        else
                                        {
                                            cast.ResetSteps();
                                            throw new ExitGUIException();
                                        }
                                    }

                                    GUILayout.Space(20);
                                }

                                var ordered = cast.GetOrderedSteps();
                                int currentStepIndex = ordered.IndexOf(currentStep);

                                // Previous step
                                GUI.enabled = currentStepIndex > 0;
                                if (GUILayout.Button("Previous"))
                                {
                                    m_ScrollPosition = Vector2.zero;
                                    currentStep = ordered[currentStepIndex - 1];
                                }

                                // Next step
                                GUI.enabled = currentStepIndex < (ordered.Count - 1) && cast.CheckCanContinue(currentStep);
                                if (GUILayout.Button("Next"))
                                {
                                    m_ScrollPosition = Vector2.zero;
                                    currentStep = ordered[currentStepIndex + 1];
                                }

                                GUILayout.Space(20);

                                // Complete
                                GUI.enabled = cast.CheckCanComplete();
                                if (GUILayout.Button("Complete", GUILayout.Width(100)))
                                {
                                    m_ScrollPosition = Vector2.zero;
                                    currentStep = "summary";
                                }

                                GUI.enabled = true;
                            }
                            GUILayout.Space(4);
                        }
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}