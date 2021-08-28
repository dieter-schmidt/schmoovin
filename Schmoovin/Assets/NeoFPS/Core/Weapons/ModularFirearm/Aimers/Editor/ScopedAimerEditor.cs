using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(ScopedAimer))]
    public class ScopedAimerEditor : BaseAimerEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            base.OnInspectorGUIInternal();

            // Get root and offset props
            var offsetProp = serializedObject.FindProperty("m_AimOffset");

            EditorGUILayout.PropertyField(offsetProp);

            var firearm = ((ScopedAimer)target).GetComponent<IModularFirearm>();
            if (firearm != null)
            {
                ++EditorGUI.indentLevel;
                Transform t = EditorGUILayout.ObjectField("From Transform", null, typeof(Transform), true) as Transform;
                --EditorGUI.indentLevel;
                if (t != null)
                    BuildAimOffsetFromTransform(offsetProp, firearm.transform, t);
            }
            else
            {
                NeoFpsEditorGUI.MiniInfo("Can only calculate offset when attached to a firearm");
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HudScopeKey"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FovMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimTime"));
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

        protected override void SetGizmoProperties()
        {
            gizmoRootTransform = ((ScopedAimer)target).transform;
            gizmoPositionOffset = serializedObject.FindProperty("m_AimOffset").vector3Value;
        }

        public static void BuildAimOffsetFromTransform(SerializedProperty offsetProp, Transform firearmTransform, Transform targetTransform)
        {
            Quaternion inverse = Quaternion.Inverse(firearmTransform.rotation);
            Vector3 scale = firearmTransform.lossyScale;
            Vector3 result = firearmTransform.position - targetTransform.position;
            result.x /= scale.x;
            result.y /= scale.y;
            result.z /= scale.z;
            result = inverse * result;
            offsetProp.vector3Value = result;
        }
    }
}