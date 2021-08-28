using NeoFPS.CharacterMotion.Parameters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPSEditor.CharacterMotion.Debugger
{
    [Serializable]
    public struct DebugTriggerParameter
    {
        public string title;
        public TriggerParameter parameter;
        public bool[] buffer;

        public DebugTriggerParameter(TriggerParameter p, int count)
        {
            parameter = p;
            title = parameter.name;
            buffer = new bool[count];
        }

        public void RecordValue(int index)
        {
            buffer[index] = parameter.PeekTrigger();
        }
    }

    [Serializable]
    public struct DebugSwitchParameter
    {
        public string title;
        public SwitchParameter parameter;
        public bool[] buffer;

        public DebugSwitchParameter(SwitchParameter p, int count)
        {
            parameter = p;
            title = parameter.name;
            buffer = new bool[count];
        }

        public void RecordValue (int index)
        {
            buffer[index] = parameter.on;
        }
    }

    [Serializable]
    public struct DebugTransformParameter
    {
        public string title;
        public TransformParameter parameter;
        public string[] buffer;

        public DebugTransformParameter(TransformParameter p, int count)
        {
            parameter = p;
            title = parameter.name;
            buffer = new string[count];
        }

        public void RecordValue(int index)
        {
            if (parameter.value == null)
                buffer[index] = "<null>";
            else
                buffer[index] = parameter.value.name;
        }
    }

    [Serializable]
    public struct DebugIntParameter
    {
        public string title;
        public IntParameter parameter;
        public int[] buffer;

        public DebugIntParameter(IntParameter p, int count)
        {
            parameter = p;
            title = parameter.name;
            buffer = new int[count];
        }

        public void RecordValue(int index)
        {
            buffer[index] = parameter.value;
        }
    }

    [Serializable]
    public struct DebugFloatParameter
    {
        public string title;
        public FloatParameter parameter;
        public float[] buffer;

        public DebugFloatParameter(FloatParameter p, int count)
        {
            parameter = p;
            title = parameter.name;
            buffer = new float[count];
        }

        public void RecordValue(int index)
        {
            buffer[index] = parameter.value;
        }
    }

    [Serializable]
    public struct DebugVectorParameter
    {
        public string title;
        public VectorParameter parameter;
        public Vector3[] buffer;

        public DebugVectorParameter(VectorParameter p, int count)
        {
            parameter = p;
            title = parameter.name;
            buffer = new Vector3[count];
        }

        public void RecordValue(int index)
        {
            buffer[index] = parameter.value;
        }
    }
}