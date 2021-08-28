using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS.ModularFirearms;

namespace NeoFPSEditor.ModularFirearms
{
    [CustomEditor(typeof(AudioOnlyMuzzleEffect))]
    public class AudioOnlyMuzzleEffectEditor : BaseMuzzleEffectEditor
    {
        protected override void OnInspectorGUIInternal()
        {
            DrawAudioProperties();
        }
    }
}