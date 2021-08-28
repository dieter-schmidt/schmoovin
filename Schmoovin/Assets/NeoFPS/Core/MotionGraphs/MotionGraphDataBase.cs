using UnityEngine;

namespace NeoFPS.CharacterMotion
{
    public class MotionGraphDataBase : ScriptableObject, IMotionGraphElement
    {
        [SerializeField] private int m_DataID = -1;

        public int dataID
        {
            get { return m_DataID; }
        }

        void OnValidate ()
        {
            if (m_DataID == -1)
                m_DataID = GetInstanceID();
        }

        public void CheckReferences(IMotionGraphMap map) { }

        public virtual void AddOverride(IMotionGraphDataOverride over)
        {
        }

        public virtual void RemoveOverride(IMotionGraphDataOverride over)
        {
        }
    }
}