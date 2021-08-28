using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NeoFPS;

namespace NeoFPSEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof (ContactLadder))]
    public class ContactLadderEditor : LadderEditor
    {
    }
}