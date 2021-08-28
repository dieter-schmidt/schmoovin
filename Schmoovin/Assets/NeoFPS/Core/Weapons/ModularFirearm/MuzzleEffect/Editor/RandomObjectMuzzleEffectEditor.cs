using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(RandomObjectMuzzleEffect))]
    public class RandomObjectMuzzleEffectEditor : BaseMuzzleEffectEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            var cast = target as RandomObjectMuzzleEffect;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MuzzleFlashes"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MuzzleFlashDuration"));

            int minShotSpacing = cast.GetMinShotSpacing();
            if (minShotSpacing == -1)
                NeoFpsEditorGUI.MiniError("No muzzle flash objects");
            else
                NeoFpsEditorGUI.MiniInfo("Minimum shot spacing: " + minShotSpacing + " fixed frames.");

            DrawAudioProperties();
        }
    }
}