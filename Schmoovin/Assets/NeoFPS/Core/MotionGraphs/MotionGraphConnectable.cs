using System;
using UnityEngine;
using System.Collections.Generic;
using NeoCC;
using NeoSaveGames.Serialization;

namespace NeoFPS.CharacterMotion
{
    public abstract class MotionGraphConnectable : ScriptableObject, IMotionGraphElement, INeoSerializableObject
    {
        private const float k_PositionClamp = 2000f;

#if UNITY_EDITOR
        [HideInInspector] public Vector2 uiPosition = Vector2.zero;
        [HideInInspector] public string defaultName = "NewState";

        Dictionary<int, MotionGraphConnectable> m_KeyMap = null;
        protected bool CheckSerializationKey(int key, MotionGraphConnectable connectable)
        {
            // Check if not initialised
            if (key == 0)
                return false;

            // Values are tracked in the root
            if (parent == null)
            {
                if (m_KeyMap == null)
                    m_KeyMap = new Dictionary<int, MotionGraphConnectable>();

                // Check if it's already in use
                MotionGraphConnectable found = null;
                if (m_KeyMap.TryGetValue(key, out found))
                {
                    return (found == connectable);
                }
                else
                {
                    // Track key
                    m_KeyMap.Add(key, connectable);
                    return true;
                }
            }
            else
                return parent.CheckSerializationKey(key, connectable);
        }
#endif

        [SerializeField, HideInInspector] private MotionGraph m_Parent = null;
        [SerializeField, HideInInspector] private List<MotionGraphConnection> m_Connections = new List<MotionGraphConnection>();
        [SerializeField, HideInInspector] private List<MotionGraphBehaviour> m_Behaviours = new List<MotionGraphBehaviour>();
        [SerializeField] private int m_SerializationKey = 0;

        private static readonly NeoSerializationKey k_ElapsedKey = new NeoSerializationKey("elapsed");
        private static readonly NeoSerializationKey k_EnabledKey = new NeoSerializationKey("enabled");

        public bool active
        {
            get;
            private set;
        }

        public float elapsedTime
        {
            get;
            private set; 
        }

        public int serializationKey
        {
            get { return m_SerializationKey; }
        }

        public virtual void OnValidate ()
        {
            // Remove invalid transitions
            for (int i = m_Connections.Count; i > 0; --i)
            {
                if (m_Connections[i - 1] == null)
                    m_Connections.RemoveAt(i - 1);
                else
                {
                    if (!m_Connections[i - 1].isValid)
                    {
                        DestroyImmediate(m_Connections[i - 1], true);
                        m_Connections.RemoveAt(i - 1);
                    }
                    else
                        m_Connections[i - 1].OnValidate();
                }
            }
            // Remove invalid behaviours
            for (int i = m_Behaviours.Count; i > 0; --i)
            {
                if (m_Behaviours[i - 1] == null)
                    m_Behaviours.RemoveAt(i - 1);
                else
                    m_Behaviours[i - 1].OnValidate();
            }

#if UNITY_EDITOR

            // Clamp position
            uiPosition.x = Mathf.Clamp(uiPosition.x, -k_PositionClamp, k_PositionClamp);
            uiPosition.y = Mathf.Clamp(uiPosition.y, -k_PositionClamp, k_PositionClamp);

            // Check serialization key
            if (parent != null)
            {
                while (!CheckSerializationKey(m_SerializationKey, this))
                    m_SerializationKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }

            // Check behaviour serialization keys
            List<int> behaviourKeys = new List<int>();
            for (int i = 0; i < m_Behaviours.Count; ++i)
            {
                if (m_Behaviours[i].serializationKey == 0 || behaviourKeys.Contains(m_Behaviours[i].serializationKey))
                    m_Behaviours[i].GenerateSerializationKey();
            }

#endif
        }

        public MotionGraph parent
        {
            get { return m_Parent; }
        }

        public IMotionController controller
        {
            get;
            private set;
        }

        public INeoCharacterController characterController
        {
            get;
            private set;
        }

        public List<MotionGraphConnection> connections
        {
            get { return m_Connections; }
        }

        public List<MotionGraphBehaviour> behaviours
        {
            get { return m_Behaviours; }
        }

        public virtual void Initialise (IMotionController c)
        {
            controller = c;
            characterController = c.characterController;
            for (int i = 0; i < m_Connections.Count; ++i)
                m_Connections[i].Initialise(c);
            for (int i = 0; i < m_Behaviours.Count; ++i)
                m_Behaviours[i].Initialise(this);
        }

        public virtual bool CheckCanEnter()
        {
            return true;
        }

        public virtual void CheckReferences (IMotionGraphMap map)
        {
            m_Parent = map.Swap(m_Parent);
            for (int i = 0; i < m_Connections.Count; ++i)
                m_Connections[i] = map.Swap(m_Connections[i]);
            for (int i = 0; i < m_Behaviours.Count; ++i)
                m_Behaviours[i] = map.Swap(m_Behaviours[i]);
        }

		public virtual void Update ()
		{
			elapsedTime += Time.deltaTime;
			for (int i = 0; i < m_Behaviours.Count; ++i)
			{
				if (m_Behaviours[i].enabled)
					m_Behaviours[i].Update();
			}
		}

        public virtual void OnEnter ()
        {
            active = true;
            elapsedTime = 0f;
            for (int i = 0; i < m_Behaviours.Count; ++i)
			{
				if (m_Behaviours[i].enabled)
					m_Behaviours[i].OnEnter();
			}
        }

        public virtual void OnExit ()
        {
            for (int i = 0; i < m_Behaviours.Count; ++i)
			{
				if (m_Behaviours[i].enabled)
					m_Behaviours[i].OnExit();
			}
            elapsedTime = 0f;
            active = false;
        }

        public bool IsChildOf (MotionGraph graph)
        {
            if (parent == graph)
                return true;
            if (parent == null)
                return false;
            return parent.IsChildOf(graph);
        }

        public static MotionGraph GetSharedParent (MotionGraphConnectable c1, MotionGraphConnectable c2)
        {
            if (c1 == null || c2 == null)
                return null;

            MotionGraph p = c1.parent;
            while (p != null)
            {
                if (c2.IsChildOf(p))
                    return p;
                p = p.parent;
            }
            return null;
        }

        public virtual void WriteProperties(INeoSerializer writer)
        {
            // Write properties
            writer.WriteValue(k_ElapsedKey, elapsedTime);

            // Write behaviours
            for(int i = 0; i < m_Behaviours.Count; ++i)
            {
                writer.PushContext(SerializationContext.ObjectNeoSerialized, m_Behaviours[i].serializationKey);
                writer.WriteValue(k_EnabledKey, m_Behaviours[i].enabled);
                m_Behaviours[i].WriteProperties(writer);
                writer.PopContext(SerializationContext.ObjectNeoSerialized);
            }

            // Write parent
            if (parent != null)
            {
                writer.PushContext(SerializationContext.ObjectNeoSerialized, 0);
                parent.WriteProperties(writer);
                writer.PopContext(SerializationContext.ObjectNeoSerialized);
            }
        }

        public virtual void ReadProperties(INeoDeserializer reader)
        {
            Debug.Log("Reading connectable: " + name);

            // Read properties
            float floatResult;
            if (reader.TryReadValue(k_ElapsedKey, out floatResult, elapsedTime))
                elapsedTime = floatResult;

            // Read behaviours
            for (int i = 0; i < m_Behaviours.Count; ++i)
            {
                if (reader.PushContext(SerializationContext.ObjectNeoSerialized, m_Behaviours[i].serializationKey))
                {
                    try
                    {
                        bool behaviourEnabled;
                        if (reader.TryReadValue(k_EnabledKey, out behaviourEnabled, true))
                            m_Behaviours[i].enabled = behaviourEnabled;
                        m_Behaviours[i].ReadProperties(reader);
                    }
                    finally
                    {
                        reader.PopContext(SerializationContext.ObjectNeoSerialized, m_Behaviours[i].serializationKey);
                    }
                }
            }

            // Read parent
            if (parent != null)
            {
                if (reader.PushContext(SerializationContext.ObjectNeoSerialized, 0))
                {
                    try
                    {
                        parent.ReadProperties(reader);
                    }
                    finally
                    {
                        reader.PopContext(SerializationContext.ObjectNeoSerialized, 0);
                    }
                }
            }

            Debug.Log("Reading connectable completed");
        }
    }
}