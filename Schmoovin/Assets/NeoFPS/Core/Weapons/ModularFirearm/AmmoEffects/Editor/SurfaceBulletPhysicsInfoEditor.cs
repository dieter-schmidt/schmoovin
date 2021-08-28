using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;
using NeoFPS.Constants;

namespace NeoFPSEditor
{
    [CustomEditor(typeof(SurfaceBulletPhysicsInfo), true)]
    public class SurfaceBulletPhysicsInfoEditor : Editor
    {
        class Clipboard
        {
            public bool canPenetrate = false;
            public float penetrationDepth = 0f;
            public float maxPenetrationAngle = 0f;
            public bool canRicochet = false;
            public float minRicochetSpeed = 0f;
            public float minRicochetAngle = 0f;
            public float strongRicochetAngle = 0f;
            public float minSpeedMultiplier = 0f;
            public float surfaceFriction = 0f;
        }

        private static Clipboard s_Clipboard = null;

        private void Awake()
        {
            (target as SurfaceBulletPhysicsInfo).OnValidate();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PenetrationSpeed"));

            var penetration = serializedObject.FindProperty("m_Penetration");
            var ricochet = serializedObject.FindProperty("m_Ricochet");
            var expanded = serializedObject.FindProperty("m_Expand");

            for (int i = 0; i < Mathf.Min(penetration.arraySize, ricochet.arraySize); ++i)
                DrawEntry(penetration, ricochet, expanded, i);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawEntry(SerializedProperty penetration, SerializedProperty ricochet, SerializedProperty expanded, int index)
        {
            var p = penetration.GetArrayElementAtIndex(index);
            var r = ricochet.GetArrayElementAtIndex(index);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            ++EditorGUI.indentLevel;
            if (NeoFpsEditorGUI.FoldoutProperty(expanded.GetArrayElementAtIndex(index), FpsSurfaceMaterial.names[index]))
            {
                GUILayout.Space(4);

                // BULLET PENETRATION
                EditorGUILayout.LabelField("Penetration", EditorStyles.boldLabel);

                if (NeoFpsEditorGUI.TogglePropertyField(p.FindPropertyRelative("m_CanPenetrate")))
                {
                    // Penetration depth
                    NeoFpsEditorGUI.CallbackFloatPropertyField(p.FindPropertyRelative("m_PenetrationDepth"), result =>
                    {
                        // Clamp values
                        return Mathf.Clamp(result, 0f, 5f);
                    });

                    // Penetration angle limit
                    NeoFpsEditorGUI.CallbackFloatPropertyField(p.FindPropertyRelative("m_MaxPenetrationAngle"), result =>
                    {
                        // Clamp values
                        result = Mathf.Clamp(result, 5f, 85f);

                        // Modify affected properties
                        NeoFpsEditorUtility.ClampMinFloatProperty(r.FindPropertyRelative("m_MinRicochetAngle"), result + 0.1f);
                        NeoFpsEditorUtility.ClampMinFloatProperty(r.FindPropertyRelative("m_StrongRicochetAngle"), result + 0.1f);

                        return result;
                    });

                    // Deflection
                    EditorGUILayout.PropertyField(p.FindPropertyRelative("m_MaxDeflection"));
                }

                GUILayout.Space(4);

                // RICOCHET
                EditorGUILayout.LabelField("Ricochet", EditorStyles.boldLabel);

                if (NeoFpsEditorGUI.TogglePropertyField(r.FindPropertyRelative("m_CanRicochet")))
                {
                    // Minimum ricochet speed multplier (when hitting at the min angle)
                    NeoFpsEditorGUI.CallbackFloatPropertyField(r.FindPropertyRelative("m_MinRicochetSpeed"), result =>
                    {
                        // Clamp value
                        if (result < 1f)
                            result = 1f;
                        return result;
                    });

                    // Minimum ricochet speed multplier (when hitting at the min angle)
                    NeoFpsEditorGUI.CallbackFloatPropertyField(r.FindPropertyRelative("m_MinRicochetAngle"), result =>
                    {
                        // Clamp values
                        result = Mathf.Clamp(result, 5f, 85f);

                        // Modify affected properties
                        NeoFpsEditorUtility.ClampMinFloatProperty(r.FindPropertyRelative("m_StrongRicochetAngle"), result + 0.1f);
                        NeoFpsEditorUtility.ClampMaxFloatProperty(p.FindPropertyRelative("m_MaxPenetrationAngle"), result - 0.1f);

                        return result;
                    });

                    // Minimum ricochet speed multplier (when hitting at the min angle)
                    NeoFpsEditorGUI.CallbackFloatPropertyField(r.FindPropertyRelative("m_StrongRicochetAngle"), result =>
                    {
                        // Clamp values
                        result = Mathf.Clamp(result, 10f, 89f);

                        // Modify affected properties
                        NeoFpsEditorUtility.ClampMaxFloatProperty(r.FindPropertyRelative("m_MinRicochetAngle"), result - 0.1f);
                        NeoFpsEditorUtility.ClampMaxFloatProperty(p.FindPropertyRelative("m_MaxPenetrationAngle"), result - 0.1f);

                        return result;
                    });

                    // Minimum ricochet speed multplier (when hitting at the min angle)
                    NeoFpsEditorGUI.CallbackFloatPropertyField(r.FindPropertyRelative("m_MinSpeedMultiplier"), result =>
                    {
                        // Clamp value
                        result = Mathf.Clamp(result, 0.001f, 0.8f);

                        // Modify affected properties
                        NeoFpsEditorUtility.ClampMinFloatProperty(r.FindPropertyRelative("m_MaxSpeedMultiplier"), result + 0.01f);

                        return result;
                    });

                    // Maximum ricochet speed multplier (when hitting at the min angle)
                    NeoFpsEditorGUI.CallbackFloatPropertyField(r.FindPropertyRelative("m_MaxSpeedMultiplier"), result =>
                    {
                        // Clamp value
                        result = Mathf.Clamp(result, 0.05f, 0.9f);

                        // Modify affected properties
                        NeoFpsEditorUtility.ClampMaxFloatProperty(r.FindPropertyRelative("m_MinSpeedMultiplier"), result - 0.01f);

                        return result;
                    });

                    // Surface friction
                    EditorGUILayout.PropertyField(r.FindPropertyRelative("m_SurfaceFriction"));
                }

                GUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();

                // Copy
                if (GUILayout.Button("Copy"))
                {
                    s_Clipboard = new Clipboard();
                    s_Clipboard.canPenetrate = p.FindPropertyRelative("m_CanPenetrate").boolValue;
                    s_Clipboard.penetrationDepth = p.FindPropertyRelative("m_PenetrationDepth").floatValue;
                    s_Clipboard.maxPenetrationAngle = p.FindPropertyRelative("m_MaxPenetrationAngle").floatValue;
                    s_Clipboard.canRicochet = r.FindPropertyRelative("m_CanRicochet").boolValue;
                    s_Clipboard.minRicochetSpeed = r.FindPropertyRelative("m_MinRicochetSpeed").floatValue;
                    s_Clipboard.minRicochetAngle = r.FindPropertyRelative("m_MinRicochetAngle").floatValue;
                    s_Clipboard.strongRicochetAngle = r.FindPropertyRelative("m_StrongRicochetAngle").floatValue;
                    s_Clipboard.minSpeedMultiplier = r.FindPropertyRelative("m_MinSpeedMultiplier").floatValue;
                    s_Clipboard.surfaceFriction = r.FindPropertyRelative("m_SurfaceFriction").floatValue;
                }

                // Paste
                if (s_Clipboard == null)
                    GUI.enabled = false;
                if (GUILayout.Button("Paste"))
                {
                    p.FindPropertyRelative("m_CanPenetrate").boolValue = s_Clipboard.canPenetrate;
                    p.FindPropertyRelative("m_PenetrationDepth").floatValue = s_Clipboard.penetrationDepth;
                    p.FindPropertyRelative("m_MaxPenetrationAngle").floatValue = s_Clipboard.maxPenetrationAngle;
                    r.FindPropertyRelative("m_CanRicochet").boolValue = s_Clipboard.canRicochet;
                    r.FindPropertyRelative("m_MinRicochetSpeed").floatValue = s_Clipboard.minRicochetSpeed;
                    r.FindPropertyRelative("m_MinRicochetAngle").floatValue = s_Clipboard.minRicochetAngle;
                    r.FindPropertyRelative("m_StrongRicochetAngle").floatValue = s_Clipboard.strongRicochetAngle;
                    r.FindPropertyRelative("m_MinSpeedMultiplier").floatValue = s_Clipboard.minSpeedMultiplier;
                    r.FindPropertyRelative("m_SurfaceFriction").floatValue = s_Clipboard.surfaceFriction;
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(2);
            }
            --EditorGUI.indentLevel;

            EditorGUILayout.EndVertical();
        }

        void SetDefaultValues(SerializedProperty penetration, SerializedProperty ricochet, int index)
        {
            var p = penetration.GetArrayElementAtIndex(index);
            p.FindPropertyRelative("m_CanPenetrate").boolValue = false;
            p.FindPropertyRelative("m_PenetrationDepth").floatValue = 0.1f;
            p.FindPropertyRelative("m_MaxPenetrationAngle").floatValue = 45f;

            var r = ricochet.GetArrayElementAtIndex(index);
            r.FindPropertyRelative("m_CanRicochet").boolValue = false;
            r.FindPropertyRelative("m_MinRicochetAngle").floatValue = 45f;
            r.FindPropertyRelative("m_StrongRicochetAngle").floatValue = 80f;
            r.FindPropertyRelative("m_MinSpeedMultiplier").floatValue = 0.1f;
            r.FindPropertyRelative("m_SurfaceFriction").floatValue = 0f;
        }
    }
}