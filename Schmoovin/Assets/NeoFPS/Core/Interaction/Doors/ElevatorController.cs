using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-elevatorcontroller.html")]
    public class ElevatorController : MonoBehaviour, INeoSerializableComponent
    {
		[SerializeField, Tooltip("The floor the elevator cab starts on. It is best to move the cab to this position in the editor to prevent it jumping there instantly which can cause problems if there are dynamic objects in the cab.")]
        private int m_StartingFloor = 0;

		[SerializeField, Tooltip("The movement speed of the cab.")]
        private float m_CabSpeed = 1f;

		[SerializeField, Tooltip("The delay between reaching a floor and opening the doors.")]
        private float m_CabOpenDelay = 0.5f;

		[SerializeField, Tooltip("The distance between floors.")]
        private float m_FloorHeight = 4f;

		[SerializeField, Tooltip("The duration the elevator doors will remain open unless interrupted.")]
        private float m_DoorOpenDuration = 7.5f;

		[SerializeField, Tooltip("The moving platform of the elevator cab.")]
        private ElevatorMovingPlatform m_Cab = null;

        [SerializeField, Tooltip("The doors for each floor.")]
        private DoorBase[] m_Floors = new DoorBase[0];

		[SerializeField, Tooltip("An event that is invoked every time the cab switches floors.")]
        private FloorChangeEvent m_OnFloorChange = null;

        [Serializable]
        public class FloorChangeEvent : UnityEvent<int> { }

        private static readonly NeoSerializationKey k_ProgressKey = new NeoSerializationKey("progress");
        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");
        private static readonly NeoSerializationKey k_TargetFloorKey = new NeoSerializationKey("targetFloor");
        private static readonly NeoSerializationKey k_StateKey = new NeoSerializationKey("state");

        private WaitForFixedUpdate m_WaitForFixedUpdate = null;
        private Coroutine m_AnimationCoroutine = null;
        private float m_FloorsPerSecond = 0f;
        private float m_Timer = 0f;
        private float m_TargetFloor = 0f;
        private State m_State = State.Idle;

        private enum State
        {
            Idle,
            CloseDoors,
            MoveCab,
            CabOpenDelay,
            OpenDoors,
            DoorOpenTimeout,
            CloseDoorsFinal
        }

        public float floorHeight
        {
            get { return m_FloorHeight; }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            m_StartingFloor = Mathf.Clamp(m_StartingFloor, 0, m_Floors.Length);
            m_CabSpeed = Mathf.Clamp(m_CabSpeed, 0.1f, 5f);
            m_CabOpenDelay = Mathf.Clamp(m_CabOpenDelay, 0f, 3f);
            m_FloorHeight = Mathf.Clamp(m_FloorHeight, 2f, 20f);
            m_DoorOpenDuration = Mathf.Clamp(m_DoorOpenDuration, 3f, 60f);

            if (m_Cab == null)
                m_Cab = GetComponentInChildren<ElevatorMovingPlatform>();
        }
#endif

        private int m_CurrentFloorIndex = 0;
        public int currentFloorIndex
        {
            get { return m_CurrentFloorIndex; }
            private set
            {
                if (m_CurrentFloorIndex != value)
                {
                    m_CurrentFloorIndex = value;
                    m_OnFloorChange.Invoke(m_CurrentFloorIndex);
                }
            }
        }

        private float m_CurrentFloorProgress = 0f;
        public float currentFloorProgress
        {
            get { return m_CurrentFloorProgress; }
            private set
            {
                m_CurrentFloorProgress = Mathf.Clamp(value, 0f, m_Floors.Length);
                currentFloorIndex = Mathf.RoundToInt(m_CurrentFloorProgress);
            }
        }

		void Start()
		{
            if (m_Cab != null)
                m_Cab.Initialise(this, m_FloorHeight);
            m_FloorsPerSecond = m_CabSpeed / m_FloorHeight;
            currentFloorProgress = m_StartingFloor;
            m_WaitForFixedUpdate = new WaitForFixedUpdate();
        }

        public void PressFloorButton(int floorIndex)
        {
			if (floorIndex < 0 || floorIndex >= m_Floors.Length || m_Floors[floorIndex] == null)
                return;

            if (m_AnimationCoroutine != null)
                StopCoroutine(m_AnimationCoroutine);

            float diff = Mathf.Abs(m_CurrentFloorProgress - floorIndex);
            if (diff <= Mathf.Epsilon)
                m_AnimationCoroutine = StartCoroutine(OpenDoors());
            else
            {
                m_TargetFloor = floorIndex;
                m_AnimationCoroutine = StartCoroutine(CloseDoors(false));
            }
        }
        
        IEnumerator CloseDoors(bool complete)
        {
            m_State = (complete) ? State.CloseDoorsFinal : State.CloseDoors;

            // Close the door (retry until closed)
            DoorBase door = m_Floors[currentFloorIndex];
            while (door.state != DoorState.Closed)
            {
                // Trigger close
                door.Close();

                // Timeout
                float timer = 0f;
                while (timer < m_DoorOpenDuration)
                {
                    yield return null;
                    timer += Time.deltaTime;

                    // If successfully closed, carry on
                    if (door.state == DoorState.Closed)
                        break;
                }
                // Failed to close, retry
            }

            if (complete)
            {
                m_AnimationCoroutine = null;
                m_State = State.Idle;
            }
            else
                m_AnimationCoroutine = StartCoroutine(MoveCab(0f));
        }
        
        IEnumerator MoveCab(float timer)
        {
            m_State = State.MoveCab;
            m_Timer = timer;
            
            // Up or down?
            if (currentFloorProgress < m_TargetFloor)
            {
                while (true)
                {
                    yield return m_WaitForFixedUpdate;

                    // Increment progress
                    float newValue = currentFloorProgress + Time.deltaTime * m_FloorsPerSecond;
                    if (newValue > m_TargetFloor)
                    {
                        // Reached target
                        currentFloorProgress = m_TargetFloor;
                        break;
                    }
                    else
                        currentFloorProgress = newValue;
                }
            }
            else
            {
                while (true)
                {
                    yield return m_WaitForFixedUpdate;

                    // Decrement progress
                    float newValue = currentFloorProgress - Time.deltaTime * m_FloorsPerSecond;
                    if (newValue < m_TargetFloor)
                    {
                        // Reached target
                        currentFloorProgress = m_TargetFloor;
                        break;
                    }
                    else
                        currentFloorProgress = newValue;
                }
            }

            // delay door opening
            m_AnimationCoroutine = StartCoroutine (CabOpenDelay(0f));
        }

        IEnumerator CabOpenDelay(float timer)
        {
            m_State = State.CabOpenDelay;

            m_Timer = timer;
            while (m_Timer < m_CabOpenDelay)
            {
                yield return null;
                m_Timer += Time.deltaTime;
            }

            // Open the doors
            m_AnimationCoroutine = StartCoroutine(OpenDoors());
        }

        IEnumerator OpenDoors()
        {
            m_State = State.OpenDoors;

            // Open the door
            DoorBase door = m_Floors[currentFloorIndex];
            door.Open();

            // Wait until open
            while (door.state != DoorState.Open)
                yield return null;

            // Wait for door open duration
            m_AnimationCoroutine = StartCoroutine(DoorOpenTimeout(0f));
        }

        IEnumerator DoorOpenTimeout(float timer)
        {
            m_State = State.DoorOpenTimeout;

            m_Timer = timer;
            while (m_Timer < m_DoorOpenDuration)
            {
                yield return null;
                m_Timer += Time.deltaTime;
            }

            // Close the doors
            m_AnimationCoroutine = StartCoroutine(CloseDoors(true));
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_ProgressKey, currentFloorProgress);
            writer.WriteValue(k_TimerKey, m_Timer);
            writer.WriteValue(k_TargetFloorKey, m_TargetFloor);
            writer.WriteValue(k_StateKey, (int)m_State);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            float floatResult = 0f;
            if (reader.TryReadValue(k_ProgressKey, out floatResult, currentFloorProgress))
                currentFloorProgress = floatResult;

            // Recreate coroutine state
            int state = 0;
            reader.TryReadValue(k_StateKey, out state, 0);
            m_State = (State)state;
            switch(m_State)
            {
                case State.CloseDoors:
                    m_AnimationCoroutine = StartCoroutine(CloseDoors(false));
                    break;
                case State.CloseDoorsFinal:
                    m_AnimationCoroutine = StartCoroutine(CloseDoors(true));
                    break;
                case State.OpenDoors:
                    m_AnimationCoroutine = StartCoroutine(OpenDoors());
                    break;
                case State.CabOpenDelay:
                    reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer);
                    m_AnimationCoroutine = StartCoroutine(CabOpenDelay(m_Timer));
                    break;
                case State.DoorOpenTimeout:
                    reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer);
                    m_AnimationCoroutine = StartCoroutine(DoorOpenTimeout(m_Timer));
                    break;
                case State.MoveCab:
                    reader.TryReadValue(k_TimerKey, out m_Timer, m_Timer);
                    reader.TryReadValue(k_TargetFloorKey, out m_Timer, m_Timer);
                    m_AnimationCoroutine = StartCoroutine(MoveCab(m_Timer));
                    break;
            }
        }
    }
}