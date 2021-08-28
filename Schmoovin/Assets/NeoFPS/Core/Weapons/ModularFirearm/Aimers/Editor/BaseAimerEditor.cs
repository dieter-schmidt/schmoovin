using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor (typeof(BaseAimerBehaviour), true)]
    public class BaseAimerEditor : BaseFirearmModuleBehaviourEditor
    {
        protected Transform gizmoRootTransform = null;
        protected Vector3 gizmoPositionOffset = Vector3.zero;
        protected Quaternion gizmoRotationOffset = Quaternion.identity;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SetGizmoProperties();
        }

        protected override void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimUpAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimDownAudio"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_HipAccuracyCap"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AimedAccuracyCap"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanAimWhileReloading"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnAimUp"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnAimDown"));
        }

        protected virtual void SetGizmoProperties()
        {

        }

        protected void OnSceneGUI()
        {
            if (gizmoRootTransform == null)
                return;
            
            Color c = Handles.color;

            Vector3 start = gizmoRootTransform.position + (gizmoRootTransform.rotation * -gizmoPositionOffset);
            Quaternion rotation = gizmoRootTransform.rotation * gizmoRotationOffset;

            Handles.color = Color.green;
            Handles.DrawLine(start, start + (rotation * new Vector3(0f, 0.05f, 0f)));

            Handles.color = Color.red;
            Handles.DrawLine(start, start + (rotation * new Vector3(0.05f, 0f, 0f)));

            Handles.color = Color.blue;
            Vector3 end = start + (rotation * new Vector3(0f, 0f, 1f));
            Handles.DrawLine(start, end);
            Vector3 p1 = end + (rotation * new Vector3(0.025f, 0f, -0.04f));
            Vector3 p2 = end + (rotation * new Vector3(0f, 0.025f, -0.04f));
            Vector3 p3 = end + (rotation * new Vector3(-0.025f, 0f, -0.04f));
            Vector3 p4 = end + (rotation * new Vector3(0f, -0.025f, -0.04f));
            Handles.DrawLine(end, p1);
            Handles.DrawLine(end, p2);
            Handles.DrawLine(end, p3);
            Handles.DrawLine(end, p4);
            Handles.DrawLine(p1, p2);
            Handles.DrawLine(p2, p3);
            Handles.DrawLine(p3, p4);
            Handles.DrawLine(p4, p1);

            Handles.color = c;
        }
    }
}
