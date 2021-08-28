using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(WeaponMoveAimer))]
    public class WeaponMoveAimerEditor : BaseAimerEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            base.OnInspectorGUIInternal();

            // Check
            var offsetProp = serializedObject.FindProperty("m_AimOffset");
            EditorGUILayout.PropertyField(offsetProp);
            if (offsetProp.objectReferenceValue == null)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimPosition"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimRotation"));
                --EditorGUI.indentLevel;
            }

            // Field of View
            ShowFoVOptions();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PositionTransition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RotationTransition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PositionSpringMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RotationSpringMultiplier"));

            var aimKeyProp = serializedObject.FindProperty("m_AimAnimBool");
            EditorGUILayout.PropertyField(aimKeyProp);
            if (!string.IsNullOrEmpty(aimKeyProp.stringValue))
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BlockTrigger"));
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CrosshairUp"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CrosshairDown"));
        }

        void ShowFoVOptions()
        {
            var lockInputToFoV = serializedObject.FindProperty("lockInputToFoV");

            EditorGUILayout.LabelField("Field of View");
            EditorGUILayout.BeginHorizontal();
            ++EditorGUI.indentLevel;

            // Sliders
            EditorGUILayout.BeginVertical();
            if (lockInputToFoV.boolValue)
            {
                var fov = serializedObject.FindProperty("m_FovMultiplier");
                EditorGUILayout.PropertyField(fov);
                var input = serializedObject.FindProperty("m_InputMultiplier");
                input.floatValue = fov.floatValue;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(input);
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FovMultiplier"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputMultiplier"));
            }
            EditorGUILayout.EndVertical();

            var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(16f));
            rect.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 0.5f;

            // Lock button
            if (lockInputToFoV.boolValue)
            {
                if (GUI.Button(rect, EditorGUIUtility.FindTexture("LockIcon-On"), "IconButton"))
                    lockInputToFoV.boolValue = false;
            }
            else
            {
                if (GUI.Button(rect, EditorGUIUtility.FindTexture("LockIcon"), "IconButton"))
                    lockInputToFoV.boolValue = true;
            }

            --EditorGUI.indentLevel;
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2f);
        }

        protected override void SetGizmoProperties()
        {
            var firearm = ((WeaponMoveAimer)target).GetComponentInParent<IModularFirearm>();
            if (firearm != null)
            {
                gizmoRootTransform = firearm.transform;

                var aimTransform = serializedObject.FindProperty("m_AimOffset").objectReferenceValue as Transform;
                if (aimTransform != null)
                {
                    var inverse = Quaternion.Inverse(gizmoRootTransform.rotation);
                    gizmoRotationOffset = inverse * aimTransform.rotation;
                    gizmoPositionOffset = inverse * (gizmoRootTransform.position - aimTransform.position);
                }
                else
                {
                    gizmoRotationOffset = Quaternion.Euler(serializedObject.FindProperty("m_AimRotation").vector3Value);
                    gizmoPositionOffset = serializedObject.FindProperty("m_AimPosition").vector3Value;
                }
            }
            else
            {
                gizmoRootTransform = null;
                gizmoPositionOffset = Vector3.zero;
                gizmoRotationOffset = Quaternion.identity;
            }
        }

        public static void BuildAimOffsetFromTransform(SerializedProperty aimPosProp, SerializedProperty aimRotProp, Transform firearmTransform, Transform aimTargetTransform)
        {
            Quaternion inverse = Quaternion.Inverse(firearmTransform.rotation);
            Quaternion aimRotation = inverse * aimTargetTransform.rotation;//Quaternion.Inverse(t.rotation);

            Vector3 scale = firearmTransform.lossyScale;
            Vector3 result = firearmTransform.position - aimTargetTransform.position;
            result.x /= scale.x;
            result.y /= scale.y;
            result.z /= scale.z;
            //result = aimRotation * result;
            aimPosProp.vector3Value = result;
            
            aimRotProp.vector3Value = aimRotation.eulerAngles;
        }
    }
}