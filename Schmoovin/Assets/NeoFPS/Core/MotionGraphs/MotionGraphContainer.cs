using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoFPS.CharacterMotion.Parameters;
using NeoFPS.CharacterMotion.MotionData;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion
{
    public class MotionGraphContainer : ScriptableObject, IMotionGraphElement, INeoSerializableObject
    {
        [SerializeField] private List<MotionGraphParameter> m_Parameters = new List<MotionGraphParameter>();
        [SerializeField] private List<MotionGraphDataBase> m_Data = new List<MotionGraphDataBase>();
        [SerializeField] private MotionGraph m_RootNode = null;

        private Dictionary<int, FloatParameter> m_FloatProperties = new Dictionary<int, FloatParameter>();
        private Dictionary<int, IntParameter> m_IntProperties = new Dictionary<int, IntParameter>();
        private Dictionary<int, TriggerParameter> m_Triggers = new Dictionary<int, TriggerParameter>();
        private Dictionary<int, SwitchParameter> m_SwitchProperties = new Dictionary<int, SwitchParameter>();
        private Dictionary<int, TransformParameter> m_TransformProperties = new Dictionary<int, TransformParameter>();
        private Dictionary<int, VectorParameter> m_VectorProperties = new Dictionary<int, VectorParameter>();
        private Dictionary<int, EventParameter> m_EventProperties = new Dictionary<int, EventParameter>();
        private TriggerParameter[] m_TriggerArray = null;

        private static MotionGraphCloner s_Cloner = new MotionGraphCloner();

        private IMotionController m_Controller = null;

#if UNITY_EDITOR
        public void OnValidate()
        {
            for (int i = m_Parameters.Count; i > 0; --i)
            {
                if (m_Parameters[i - 1] == null)
                    m_Parameters.RemoveAt(i - 1);
            }
            for (int i = m_Data.Count; i > 0; --i)
            {
                if (m_Data[i - 1] == null)
                    m_Data.RemoveAt(i - 1);
            }

            // Set hide flags
            foreach (var p in m_Parameters)
                p.hideFlags |= HideFlags.HideInHierarchy;
            foreach (var d in m_Data)
                d.hideFlags |= HideFlags.HideInHierarchy;
        }
#endif

        public MotionGraph rootNode
        {
            get { return m_RootNode; }
        }

        public MotionGraphContainer DeepCopy()
        {
            var clone = s_Cloner.CloneGraph(this);
            s_Cloner.Clear();
            return clone;
        }

        public void CheckReferences(IMotionGraphMap map)
        {
            m_RootNode = map.Swap(m_RootNode);
            for (int i = 0; i < m_Parameters.Count; ++i)
                m_Parameters[i] = map.Swap(m_Parameters[i]);
            for (int i = 0; i < m_Data.Count; ++i)
                m_Data[i] = map.Swap(m_Data[i]);
        }

        public void Initialise(IMotionController c)
        {
            m_Controller = c;

            // Scan properties to fill dictionaries
            foreach (MotionGraphParameter parameter in m_Parameters)
            {
                var trigger = parameter as TriggerParameter;
                if (trigger != null)
                {
                    m_Triggers.Add(Animator.StringToHash(trigger.name), trigger);
                    continue;
                }

                var intP = parameter as IntParameter;
                if (intP != null)
                {
                    m_IntProperties.Add(Animator.StringToHash(intP.name), intP);
                    continue;
                }

                var floatP = parameter as FloatParameter;
                if (floatP != null)
                {
                    m_FloatProperties.Add(Animator.StringToHash(floatP.name), floatP);
                    continue;
                }

                var transformP = parameter as TransformParameter;
                if (transformP != null)
                {
                    m_TransformProperties.Add(Animator.StringToHash(transformP.name), transformP);
                    continue;
                }

                var switchP = parameter as SwitchParameter;
                if (switchP != null)
                {
                    m_SwitchProperties.Add(Animator.StringToHash(switchP.name), switchP);
                    continue;
                }

                var vectorP = parameter as VectorParameter;
                if (vectorP != null)
                {
                    m_VectorProperties.Add(Animator.StringToHash(vectorP.name), vectorP);
                    continue;
                }

                var eventP = parameter as EventParameter;
                if (eventP != null)
                {
                    m_EventProperties.Add(Animator.StringToHash(eventP.name), eventP);
                }
            }

            // Store trigger array (used for resetting)
            m_TriggerArray = new TriggerParameter[m_Triggers.Count];
            int i = 0;
            foreach (var tp in m_Triggers.Values)
                m_TriggerArray[i++] = tp;

            rootNode.Initialise(c);
        }

        public MotionGraphState GetStateFromKey(int key)
        {
            return m_RootNode.GetStateFromKey(key);
        }

        public void SetTrigger(int hash)
        {
            TriggerParameter property = null;
            if (m_Triggers.TryGetValue(hash, out property))
                property.Trigger();
        }

        public void SetTrigger(string propertyName)
        {
            SetTrigger(Animator.StringToHash(propertyName));
        }

        public TriggerParameter GetTriggerProperty(int hash)
        {
            TriggerParameter property = null;
            if (m_Triggers.TryGetValue(hash, out property))
                return property;
            return null;
        }

        public TriggerParameter GetTriggerProperty(string propertyName)
        {
            return GetTriggerProperty(Animator.StringToHash(propertyName));
        }

        public void ResetCheckedTriggers()
        {
            for (int i = 0; i < m_TriggerArray.Length; ++i)
            {
                if (m_TriggerArray[i].wasChecked)
                    m_TriggerArray[i].ResetValue();
            }
        }

        public int GetInt(int hash)
        {
            IntParameter property = null;
            if (m_IntProperties.TryGetValue(hash, out property))
                return property.value;
            else
                return 0;
        }

        public int GetInt(string propertyName)
        {
            return GetInt(Animator.StringToHash(propertyName));
        }

        public void SetInt(int hash, int value)
        {
            IntParameter property = null;
            if (m_IntProperties.TryGetValue(hash, out property))
                property.value = value;
        }

        public IntParameter GetIntProperty(int hash)
        {
            IntParameter property = null;
            if (m_IntProperties.TryGetValue(hash, out property))
                return property;
            return null;
        }

        public IntParameter GetIntProperty(string propertyName)
        {
            return GetIntProperty(Animator.StringToHash(propertyName));
        }

        public float GetFloat(int hash)
        {
            FloatParameter property = null;
            if (m_FloatProperties.TryGetValue(hash, out property))
                return property.value;
            else
                return 0;
        }

        public float GetFloat(string propertyName)
        {
            return GetFloat(Animator.StringToHash(propertyName));
        }

        public void SetFloat(int hash, float value)
        {
            FloatParameter property = null;
            if (m_FloatProperties.TryGetValue(hash, out property))
                property.value = value;
        }

        public void SetFloat(string propertyName, float value)
        {
            SetFloat(Animator.StringToHash(propertyName), value);
        }

        public FloatParameter GetFloatProperty(int hash)
        {
            FloatParameter property = null;
            if (m_FloatProperties.TryGetValue(hash, out property))
                return property;
            return null;
        }

        public FloatParameter GetFloatProperty(string propertyName)
        {
            return GetFloatProperty(Animator.StringToHash(propertyName));
        }

        public Transform GetTransform(int hash)
        {
            TransformParameter property = null;
            if (m_TransformProperties.TryGetValue(hash, out property))
                return property.value;
            else
                return null;
        }

        public Transform GetTransform(string propertyName)
        {
            return GetTransform(Animator.StringToHash(propertyName));
        }

        public void SetTransform(int hash, Transform value)
        {
            TransformParameter property = null;
            if (m_TransformProperties.TryGetValue(hash, out property))
                property.value = value;
        }

        public void SetTransform(string propertyName, Transform value)
        {
            SetTransform(Animator.StringToHash(propertyName), value);
        }

        public TransformParameter GetTransformProperty(int hash)
        {
            TransformParameter property = null;
            if (m_TransformProperties.TryGetValue(hash, out property))
                return property;
            return null;
        }

        public TransformParameter GetTransformProperty(string propertyName)
        {
            return GetTransformProperty(Animator.StringToHash(propertyName));
        }

        public bool GetSwitch(int hash)
        {
            SwitchParameter property = null;
            if (m_SwitchProperties.TryGetValue(hash, out property))
                return property.on;
            else
                return false;
        }

        public bool GetSwitch(string propertyName)
        {
            return GetSwitch(Animator.StringToHash(propertyName));
        }

        public void SetSwitch(int hash, bool value)
        {
            SwitchParameter property = null;
            if (m_SwitchProperties.TryGetValue(hash, out property))
                property.on = value;
        }

        public void SetSwitch(string propertyName, bool value)
        {
            SetSwitch(Animator.StringToHash(propertyName), value);
        }

        public SwitchParameter GetSwitchProperty(int hash)
        {
            SwitchParameter property = null;
            if (m_SwitchProperties.TryGetValue(hash, out property))
                return property;
            return null;
        }

        public SwitchParameter GetSwitchProperty(string propertyName)
        {
            return GetSwitchProperty(Animator.StringToHash(propertyName));
        }

        public Vector3 GetVector(int hash)
        {
            VectorParameter property = null;
            if (m_VectorProperties.TryGetValue(hash, out property))
                return property.value;
            else
                return Vector3.zero;
        }

        public Vector3 GetVector(string propertyName)
        {
            return GetVector(Animator.StringToHash(propertyName));
        }

        public void SetVector(int hash, Vector3 value)
        {
            VectorParameter property = null;
            if (m_VectorProperties.TryGetValue(hash, out property))
                property.value = value;
        }

        public VectorParameter GetVectorProperty(int hash)
        {
            VectorParameter property = null;
            if (m_VectorProperties.TryGetValue(hash, out property))
                return property;
            return null;
        }

        public VectorParameter GetVectorProperty(string propertyName)
        {
            return GetVectorProperty(Animator.StringToHash(propertyName));
        }

        public void AddEventListener(int hash, UnityAction listener)
        {
            EventParameter property = null;
            if (m_EventProperties.TryGetValue(hash, out property))
                property.AddListener(listener);
        }

        public void AddEventListener(string propertyName, UnityAction listener)
        {
            AddEventListener(Animator.StringToHash(propertyName), listener);
        }

        public void RemoveEventListener(int hash, UnityAction listener)
        {
            EventParameter property = null;
            if (m_EventProperties.TryGetValue(hash, out property))
                property.RemoveListener(listener);
        }

        public void RemoveEventListener(string propertyName, UnityAction listener)
        {
            RemoveEventListener(Animator.StringToHash(propertyName), listener);
        }

        public EventParameter GetEventProperty(int hash)
        {
            EventParameter property = null;
            if (m_EventProperties.TryGetValue(hash, out property))
                return property;
            return null;
        }

        public EventParameter GetEventProperty(string propertyName)
        {
            return GetEventProperty(Animator.StringToHash(propertyName));
        }

        public void CollectParameters(List<MotionGraphParameter> list)
        {
            list.AddRange(m_Parameters);
        }

        public void CollectIntParameters(List<IntParameter> list)
        {
            foreach (MotionGraphParameter property in m_Parameters)
            {
                var cast = property as IntParameter;
                if (cast != null)
                    list.Add(cast);
            }
            list.Sort((IntParameter lhs, IntParameter rhs) =>
            {
                return string.Compare(lhs.name, rhs.name);
            }
            );
        }

        public void CollectFloatParameters(List<FloatParameter> list)
        {
            foreach (MotionGraphParameter property in m_Parameters)
            {
                var cast = property as FloatParameter;
                if (cast != null)
                    list.Add(cast);
            }
            list.Sort((FloatParameter lhs, FloatParameter rhs) =>
            {
                return string.Compare(lhs.name, rhs.name);
            }
            );
        }

        public void CollectTriggerParameters(List<TriggerParameter> list)
        {
            foreach (MotionGraphParameter property in m_Parameters)
            {
                var cast = property as TriggerParameter;
                if (cast != null)
                    list.Add(cast);
            }
            list.Sort((TriggerParameter lhs, TriggerParameter rhs) =>
            {
                return string.Compare(lhs.name, rhs.name);
            }
            );
        }

        public void CollectTransformParameters(List<TransformParameter> list)
        {
            foreach (MotionGraphParameter property in m_Parameters)
            {
                var cast = property as TransformParameter;
                if (cast != null)
                    list.Add(cast);
            }
            list.Sort((TransformParameter lhs, TransformParameter rhs) =>
            {
                return string.Compare(lhs.name, rhs.name);
            }
            );
        }

        public void CollectSwitchParameters(List<SwitchParameter> list)
        {
            foreach (MotionGraphParameter property in m_Parameters)
            {
                var cast = property as SwitchParameter;
                if (cast != null)
                    list.Add(cast);
            }
            list.Sort((SwitchParameter lhs, SwitchParameter rhs) =>
            {
                return string.Compare(lhs.name, rhs.name);
            }
            );
        }

        public void CollectVectorParameters(List<VectorParameter> list)
        {
            foreach (MotionGraphParameter property in m_Parameters)
            {
                var cast = property as VectorParameter;
                if (cast != null)
                    list.Add(cast);
            }
            list.Sort((VectorParameter lhs, VectorParameter rhs) =>
            {
                return string.Compare(lhs.name, rhs.name);
            }
            );
        }

        public void CollectEventParameters(List<EventParameter> list)
        {
            foreach (MotionGraphParameter property in m_Parameters)
            {
                var cast = property as EventParameter;
                if (cast != null)
                    list.Add(cast);
            }
            list.Sort((EventParameter lhs, EventParameter rhs) =>
            {
                return string.Compare(lhs.name, rhs.name);
            }
            );
        }

        public void CollectData(List<MotionGraphDataBase> list, bool sorted = true)
        {
            list.AddRange(m_Data);
            if (sorted)
            {
                list.Sort((MotionGraphDataBase lhs, MotionGraphDataBase rhs) =>
                {
                    return string.Compare(lhs.name, rhs.name);
                });
            }
        }
        public void CollectBoolData(List<BoolData> list, bool sorted = true)
        {
            foreach (MotionGraphDataBase data in m_Data)
            {
                var cast = data as BoolData;
                if (cast != null)
                    list.Add(cast);
            }
            if (sorted)
            {
                list.Sort((BoolData lhs, BoolData rhs) =>
                {
                    return string.Compare(lhs.name, rhs.name);
                });
            }
        }

        public void CollectIntData(List<IntData> list, bool sorted = true)
        {
            foreach (MotionGraphDataBase data in m_Data)
            {
                var cast = data as IntData;
                if (cast != null)
                    list.Add(cast);
            }
            if (sorted)
            {
                list.Sort((IntData lhs, IntData rhs) =>
                {
                    return string.Compare(lhs.name, rhs.name);
                });
            }
        }

        public void CollectFloatData(List<FloatData> list, bool sorted = true)
        {
            foreach (MotionGraphDataBase data in m_Data)
            {
                var cast = data as FloatData;
                if (cast != null)
                    list.Add(cast);
            }
            if (sorted)
            {
                list.Sort((FloatData lhs, FloatData rhs) =>
                {
                    return string.Compare(lhs.name, rhs.name);
                });
            }
        }

        public void AddDataOverrides(IMotionGraphDataOverride o)
        {
            for (int i = 0; i < m_Data.Count; ++i)
                m_Data[i].AddOverride(o);
        }

        public void RemoveDataOverrides(IMotionGraphDataOverride o)
        {
            for (int i = 0; i < m_Data.Count; ++i)
                m_Data[i].RemoveOverride(o);
        }

        [Obsolete("ApplyDataOverrides is has been replaced with Add/RemoveDataOverrides which allows for stacking.")]
        public void ApplyDataOverrides(IMotionGraphDataOverride o)
        {
            for (int i = 0; i < m_Data.Count; ++i)
                m_Data[i].AddOverride(o);
        }

        public void WriteProperties(INeoSerializer writer)
        {
            // Save parameters
            foreach (var pair in m_FloatProperties)
                writer.WriteValue(pair.Key, pair.Value.value);
            foreach (var pair in m_IntProperties)
                writer.WriteValue(pair.Key, pair.Value.value);
            foreach (var pair in m_Triggers)
                writer.WriteValue(pair.Key, pair.Value.PeekTrigger());
            foreach (var pair in m_SwitchProperties)
                writer.WriteValue(pair.Key, pair.Value.on);

            if (m_TransformProperties.Count > 0)
            {
                var owner = m_Controller.GetComponent<NeoSerializedGameObject>();
                if (owner != null)
                {
                    foreach (var pair in m_TransformProperties)
                        writer.WriteTransformReference(pair.Key, pair.Value.value, owner);
                }
            }
        }

        public void ReadProperties(INeoDeserializer reader)
        {
            // Load parameters
            foreach (var pair in m_FloatProperties)
            {
                float result;
                if (reader.TryReadValue(pair.Key, out result, 0f))
                    pair.Value.value = result;
            }
            foreach (var pair in m_IntProperties)
            {
                int result;
                if (reader.TryReadValue(pair.Key, out result, 0))
                    pair.Value.value = result;
            }
            foreach (var pair in m_Triggers)
            {
                bool result;
                if (reader.TryReadValue(pair.Key, out result, false))
                {
                    if (result)
                        pair.Value.Trigger();
                    else
                        pair.Value.ResetValue();
                }
            }
            foreach (var pair in m_SwitchProperties)
            {
                bool result;
                if (reader.TryReadValue(pair.Key, out result, false))
                    pair.Value.on = result;
            }

            if (m_TransformProperties.Count > 0)
            {
                var owner = m_Controller.GetComponent<NeoSerializedGameObject>();
                if (owner != null)
                {
                    foreach (var pair in m_TransformProperties)
                    {
                        Transform result;
                        if (reader.TryReadTransformReference(pair.Key, out result, owner))
                            pair.Value.value = result;
                    }
                }
            }
        }
    }
}