using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion.MotionData
{
    public class MotionGraphDataOverrideAsset : ScriptableObject, IMotionGraphDataOverride
    {
#pragma warning disable 0414
        [SerializeField] private MotionGraphContainer m_Graph = null;
#pragma warning restore 0414

        [SerializeField] private FloatDataOverride[] m_FloatOverrides = new FloatDataOverride[0];
        [SerializeField] private IntDataOverride[] m_IntOverrides = new IntDataOverride[0];
        [SerializeField] private BoolDataOverride[] m_BoolOverrides = new BoolDataOverride[0];

        [Serializable]
        public class BoolDataOverride
        {
            [SerializeField] private bool m_Value = false;
            [SerializeField] private int m_DataID = 0;

            public int dataID
            {
                get { return m_DataID; }
            }

            public bool GetValue(bool input)
            {
                return m_Value;
            }

            public BoolDataOverride()
            { }

            public BoolDataOverride (int id, bool v)
            {
                m_DataID = id;
                m_Value = v;
            }
        }

        [Serializable]
        public class FloatDataOverride
        {
            [SerializeField] private float m_Value = 0f;
            [SerializeField] private int m_DataID = 0;

            public int dataID
            {
                get { return m_DataID; }
            }

            public float GetValue(float input)
            {
                return m_Value;
            }

            public FloatDataOverride()
            { }

            public FloatDataOverride(int id, float v)
            {
                m_DataID = id;
                m_Value = v;
            }
        }

        [Serializable]
        public class IntDataOverride
        {
            [SerializeField] private int m_Value = 0;
            [SerializeField] private int m_DataID = 0;

            public int dataID
            {
                get { return m_DataID; }
            }

            public int GetValue(int input)
            {
                return m_Value;
            }

            public IntDataOverride()
            { }

            public IntDataOverride(int id, int v)
            {
                m_DataID = id;
                m_Value = v;
            }
        }

        public Func<bool, bool> GetBoolOverride(BoolData data)
        {
            for (int i = 0; i < m_BoolOverrides.Length; ++i)
            {
                if (m_BoolOverrides[i].dataID == data.dataID)
                    return m_BoolOverrides[i].GetValue;
            }
            return null;
        }

        public Func<float, float> GetFloatOverride(FloatData data)
        {
            for (int i = 0; i < m_FloatOverrides.Length; ++i)
            {
                if (m_FloatOverrides[i].dataID == data.dataID)
                    return m_FloatOverrides[i].GetValue;
            }
            return null;
        }

        public Func<int, int> GetIntOverride(IntData data)
        {
            for (int i = 0; i < m_IntOverrides.Length; ++i)
            {
                if (m_IntOverrides[i].dataID == data.dataID)
                    return m_IntOverrides[i].GetValue;
            }
            return null;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            CheckOverrides();
        }

        public void CheckOverrides()
        {
            if (m_Graph == null)
                return;

            List<MotionGraphDataBase> data = new List<MotionGraphDataBase>();
            m_Graph.CollectData(data);

            List<FloatData> fd = new List<FloatData>();
            List<IntData> id = new List<IntData>();
            List<BoolData> bd = new List<BoolData>();

            foreach (var d in data)
            {
                if (d is FloatData)
                {
                    fd.Add(d as FloatData);
                    continue;
                }
                if (d is BoolData)
                {
                    bd.Add(d as BoolData);
                    continue;
                }
                if (d is IntData)
                {
                    id.Add(d as IntData);
                    continue;
                }
            }

            ValidateFloats(fd);
            ValidateInts(id);
            ValidateBools(bd);
        }

        void ValidateFloats(List<FloatData> data)
        {
            List<FloatDataOverride> result = new List<FloatDataOverride>();
            foreach (var d in data)
            {
                // Find the corresponding override
                FloatDataOverride add = null;
                foreach (var over in m_FloatOverrides)
                {
                    if (over.dataID == d.dataID)
                    {
                        add = over;
                        break;
                    }
                }

                // Create if not found
                if (add == null)
                    add = new FloatDataOverride(d.dataID, d.value);
                result.Add(add);
            }

            // Assign
            m_FloatOverrides = result.ToArray();
        }

        void ValidateInts(List<IntData> data)
        {
            List<IntDataOverride> result = new List<IntDataOverride>();
            foreach (var d in data)
            {
                // Find the corresponding override
                IntDataOverride add = null;
                foreach (var over in m_IntOverrides)
                {
                    if (over.dataID == d.dataID)
                    {
                        add = over;
                        break;
                    }
                }

                // Create if not found
                if (add == null)
                    add = new IntDataOverride(d.dataID, d.value);
                result.Add(add);
            }

            // Assign
            m_IntOverrides = result.ToArray();
        }

        void ValidateBools(List<BoolData> data)
        {
            List<BoolDataOverride> result = new List<BoolDataOverride>();
            foreach (var d in data)
            {
                // Find the corresponding override
                BoolDataOverride add = null;
                foreach (var over in m_BoolOverrides)
                {
                    if (over.dataID == d.dataID)
                    {
                        add = over;
                        break;
                    }
                }

                // Create if not found
                if (add == null)
                    add = new BoolDataOverride(d.dataID, d.value);
                result.Add(add);
            }

            // Assign
            m_BoolOverrides = result.ToArray();
        }
#endif
    }
}
