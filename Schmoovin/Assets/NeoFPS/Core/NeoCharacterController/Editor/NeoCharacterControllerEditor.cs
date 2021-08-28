using UnityEngine;
using UnityEditor;
using NeoCC;

namespace NeoCCEditor
{
    [CustomEditor (typeof (NeoCharacterController), true)]
    public class NeoCharacterControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Heading (Collisions)
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Collisions", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DepenetrationMask"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SkinWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SlopeLimit"));            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SlopeFriction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LedgeFriction"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_WallAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StepHeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StepMaxAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GroundHitLookahead"));

            // Ground snapping
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StickToGround"));
            if (serializedObject.FindProperty("m_StickToGround").boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GroundSnapHeight"));
                --EditorGUI.indentLevel;
            }
            
            // Heading (Rigidbodies & characters)
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rigidbody & Character Interaction", EditorStyles.boldLabel);

            // Rigidbody pushing
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PushRigidbodies"));
            if (serializedObject.FindProperty("m_PushRigidbodies").boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LowRigidbodyPushMass"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MaxRigidbodyPushMass"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RigidbodyPush"));
                --EditorGUI.indentLevel;
            }

            // Character pushing
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PushedByCharacters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PushCharacters"));
            if (serializedObject.FindProperty("m_PushCharacters").boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CharacterPush"));
                --EditorGUI.indentLevel;
            }

            // Heading (Platforms)
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Moving Platforms", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InheritPlatformYaw"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InheritPlatformVelocity"));

            // Heading (Gravity)
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gravity", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Gravity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OrientUpWithGravity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UpSmoothing"));

            serializedObject.ApplyModifiedProperties();

            //DrawDefaultInspector();
        }
    }
}