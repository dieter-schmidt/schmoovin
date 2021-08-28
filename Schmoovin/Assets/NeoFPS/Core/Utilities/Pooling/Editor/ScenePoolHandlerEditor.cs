using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using NeoFPS;

namespace NeoFPSEditor
{
	[CustomEditor (typeof (ScenePoolHandler))]
	public class ScenePoolHandlerEditor : BasePoolInfoEditor
	{
        protected override SerializedProperty GetPoolInfoArrayProperty()
        {
            return serializedObject.FindProperty("m_ScenePools");
        }

        public override void OnInspectorGUI ()
		{
			serializedObject.Update ();

            EditorGUILayout.LabelField("Starting Pools", EditorStyles.boldLabel);
            DoLayoutPoolInfo();

			serializedObject.ApplyModifiedProperties ();
		}
	}
}