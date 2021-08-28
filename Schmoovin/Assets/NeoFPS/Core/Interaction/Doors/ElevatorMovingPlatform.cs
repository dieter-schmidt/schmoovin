using UnityEngine;

namespace NeoFPS
{
    public class ElevatorMovingPlatform : BaseMovingPlatform
    {
        private ElevatorController m_Controller = null;
        private Vector3 m_StartPosition = Vector3.zero;
        private Vector3 m_Up = Vector3.zero;
        private float m_FloorHeight = 0f;

        public void Initialise (ElevatorController controller, float floorHeight)
        {
            m_Controller = controller;
            m_FloorHeight = floorHeight;

            Transform t = transform;
            m_StartPosition = t.position;
            m_Up = t.up;
        }

        protected override Vector3 GetNextPosition()
        {
            if (m_Controller == null)
                return fixedPosition;

            return m_StartPosition + m_Up * m_FloorHeight * m_Controller.currentFloorProgress;
        }

        protected override Quaternion GetNextRotation()
        {
            return fixedRotation;
        }
    }
}