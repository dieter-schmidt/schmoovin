using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;

namespace NeoFPSEditor
{
	[CustomEditor (typeof (PoolManager))]
	public class PoolManagerEditor : BasePoolInfoEditor
    {
        protected override SerializedProperty GetPoolInfoArrayProperty()
        {
            return serializedObject.FindProperty("m_SharedPools");
        }

        public override void OnInspectorGUI ()
		{
			serializedObject.Update ();

            NeoFpsEditorGUI.PrefabComponentField<ScenePoolHandler>(serializedObject.FindProperty("m_ScenePoolHandlerPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DefaultRuntimePoolSize"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shared Pools", EditorStyles.boldLabel);
            DoLayoutPoolInfo();

			serializedObject.ApplyModifiedProperties ();
		}
	}
}