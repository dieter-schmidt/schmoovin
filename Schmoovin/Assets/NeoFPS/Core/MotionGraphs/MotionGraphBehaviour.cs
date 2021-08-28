using NeoSaveGames.Serialization;
using System;
using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public class MotionGraphBehaviour : ScriptableObject, IMotionGraphElement, INeoSerializableObject
    {
        [SerializeField] private bool m_Active = true;
        [SerializeField] private int m_SerializationKey = 0;

#if UNITY_EDITOR
        public bool expanded = true;
        
        public void GenerateSerializationKey()
        {
            m_SerializationKey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
#endif

        public IMotionController controller
        {
            get;
            private set;
        }

        public MotionGraphConnectable owner
        {
            get;
            private set;
        }

        public bool enabled
        {
            get { return m_Active; }
            set
			{
				if (owner.active)
				{
					if (!m_Active && value)
						OnEnter();
					if (m_Active && !value)
						OnExit();
				}
				m_Active = value;
			}
        }

        public int serializationKey
        {
            get { return m_SerializationKey; }
        }

        public virtual void Initialise (MotionGraphConnectable o)
        {
            owner = o;
            controller = o.controller;
        }
		
        public virtual void OnEnter ()
        {
        }

        public virtual void OnExit ()
        {
        }		

        public virtual void Update ()
        {
        }

        public virtual void CheckReferences(IMotionGraphMap map)
        {
        }

        public virtual void OnValidate ()
        {
        }

        public virtual void WriteProperties(INeoSerializer writer)
        {
        }

        public virtual void ReadProperties(INeoDeserializer reader)
        {
        }
    }
}