using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPSEditor.CharacterMotion.Debugger
{
    public class MotionControllerDebugBuffer : ScriptableObject
    {
        [SerializeField] private MotionControllerDebugSnapshot[] m_Snapshots = null;
        [SerializeField] private int m_BufferSize = 512;
        [SerializeField] private int m_CurrentIndex = -1;
        [SerializeField] private int m_CurrentCount = 0;
        [SerializeField] private DebugTriggerParameter[] m_TriggerParameters = null;
        [SerializeField] private DebugSwitchParameter[] m_SwitchParameters = null;
        [SerializeField] private DebugIntParameter[] m_IntParameters = null;
        [SerializeField] private DebugFloatParameter[] m_FloatParameters = null;
        [SerializeField] private DebugTransformParameter[] m_TransformParameters = null;
        [SerializeField] private DebugVectorParameter[] m_VectorParameters = null;

        private int m_FrameCount = 0;

        public event Action onTicked;

        public int bufferSize
        {
            get { return m_BufferSize; }
            set
            {
                value = Mathf.Clamp(value, 10, 1024);
                if (m_BufferSize != value)
                {
                    m_BufferSize = value;
                    ResetBuffer();
                }
            }
        }

        public int count
        {
            get { return m_CurrentCount; }
        }

        public DebugTriggerParameter[] triggerParameters { get { return m_TriggerParameters; } }
        public DebugSwitchParameter[] switchParameters { get { return m_SwitchParameters; } }
        public DebugIntParameter[] intParameters { get { return m_IntParameters; } }
        public DebugFloatParameter[] floatParameters { get { return m_FloatParameters; } }
        public DebugTransformParameter[] transformParameters { get { return m_TransformParameters; } }
        public DebugVectorParameter[] vectorParameters { get { return m_VectorParameters; } }

        private void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
        }

        public void ResetBuffer()
        {
            m_Snapshots = new MotionControllerDebugSnapshot[m_BufferSize];
            m_CurrentIndex = -1;
            m_CurrentCount = 0;
            m_FrameCount = 0;
        }

        public void GetParametersFromController(MotionController controller)
        {
            // Set up trigger parameters
            List<TriggerParameter> collectedTriggers = new List<TriggerParameter>();
            controller.motionGraph.CollectTriggerParameters(collectedTriggers);
            m_TriggerParameters = new DebugTriggerParameter[collectedTriggers.Count];
            for (int i = 0; i < collectedTriggers.Count; ++i)
                m_TriggerParameters[i] = new DebugTriggerParameter(collectedTriggers[i], m_BufferSize);

            // Set up switch parameters
            List<SwitchParameter> collectedSwitches = new List<SwitchParameter>();
            controller.motionGraph.CollectSwitchParameters(collectedSwitches);
            m_SwitchParameters = new DebugSwitchParameter[collectedSwitches.Count];
            for (int i = 0; i < collectedSwitches.Count; ++i)
                m_SwitchParameters[i] = new DebugSwitchParameter(collectedSwitches[i], m_BufferSize);

            // Set up int parameters
            List<IntParameter> collectedInts = new List<IntParameter>();
            controller.motionGraph.CollectIntParameters(collectedInts);
            m_IntParameters = new DebugIntParameter[collectedInts.Count];
            for (int i = 0; i < collectedInts.Count; ++i)
                m_IntParameters[i] = new DebugIntParameter(collectedInts[i], m_BufferSize);

            // Set up float parameters
            List<FloatParameter> collectedFloats = new List<FloatParameter>();
            controller.motionGraph.CollectFloatParameters(collectedFloats);
            m_FloatParameters = new DebugFloatParameter[collectedFloats.Count];
            for (int i = 0; i < collectedFloats.Count; ++i)
                m_FloatParameters[i] = new DebugFloatParameter(collectedFloats[i], m_BufferSize);

            // Set up transform parameters
            List<TransformParameter> collectedTransforms = new List<TransformParameter>();
            controller.motionGraph.CollectTransformParameters(collectedTransforms);
            m_TransformParameters = new DebugTransformParameter[collectedTransforms.Count];
            for (int i = 0; i < collectedTransforms.Count; ++i)
                m_TransformParameters[i] = new DebugTransformParameter(collectedTransforms[i], m_BufferSize);

            // Set up vector parameters
            List<VectorParameter> collectedVectors = new List<VectorParameter>();
            controller.motionGraph.CollectVectorParameters(collectedVectors);
            m_VectorParameters = new DebugVectorParameter[collectedVectors.Count];
            for (int i = 0; i < collectedVectors.Count; ++i)
                m_VectorParameters[i] = new DebugVectorParameter(collectedVectors[i], m_BufferSize);
        }

        public void GetSnapshotFromController(MotionController controller, Vector3 targetMove, bool applyGravity, bool stickToGround)
        {
            IncrementIndex();
            m_Snapshots[m_CurrentIndex].frame = m_FrameCount++;
            m_Snapshots[m_CurrentIndex].state = controller.currentState.name;
            m_Snapshots[m_CurrentIndex].stateType = controller.currentState.GetType().Name;
            m_Snapshots[m_CurrentIndex].targetMove = targetMove;
            m_Snapshots[m_CurrentIndex].inputDirection = controller.inputMoveDirection;
            m_Snapshots[m_CurrentIndex].inputScale = controller.inputMoveScale;
            
            var t = controller.localTransform;
            m_Snapshots[m_CurrentIndex].position = t.position;
            m_Snapshots[m_CurrentIndex].rotation = t.rotation;

            var ncc = controller.characterController;
            m_Snapshots[m_CurrentIndex].previousMove = ncc.lastFrameMove;
            m_Snapshots[m_CurrentIndex].isGrounded = ncc.isGrounded;
            m_Snapshots[m_CurrentIndex].velocity = ncc.velocity;
            m_Snapshots[m_CurrentIndex].rawVelocity = ncc.rawVelocity;
            m_Snapshots[m_CurrentIndex].targetVelocity = ncc.targetVelocity;
            m_Snapshots[m_CurrentIndex].groundNormal = ncc.groundNormal;
            m_Snapshots[m_CurrentIndex].groundSurfaceNormal = ncc.groundSurfaceNormal;
            m_Snapshots[m_CurrentIndex].upTarget = ncc.up;
            m_Snapshots[m_CurrentIndex].ledgeFriction = ncc.ledgeFriction;
            m_Snapshots[m_CurrentIndex].slopeFriction = ncc.slopeFriction;
            m_Snapshots[m_CurrentIndex].radius = ncc.radius;
            m_Snapshots[m_CurrentIndex].height = ncc.height;
            m_Snapshots[m_CurrentIndex].depenetrations = ncc.debugDepenetrationCount;
            m_Snapshots[m_CurrentIndex].moveIterations = ncc.debugMoveIterations;
            m_Snapshots[m_CurrentIndex].collisionFlags = ncc.collisionFlags;
            m_Snapshots[m_CurrentIndex].snapToGround = ncc.debugSnapToGround && stickToGround;
            m_Snapshots[m_CurrentIndex].groundSnapHeight = ncc.groundSnapHeight;
            m_Snapshots[m_CurrentIndex].applyGravity = applyGravity;
            m_Snapshots[m_CurrentIndex].gravity = ncc.gravity;
            m_Snapshots[m_CurrentIndex].ignoreExternalForces = ncc.ignoreExternalForces;
            m_Snapshots[m_CurrentIndex].externalForceMove = ncc.debugExternalForceMove;

            var platform = ncc.platform as Component;
            if (platform != null)
                m_Snapshots[m_CurrentIndex].platform = platform.name;
            else
                m_Snapshots[m_CurrentIndex].platform = "<None>";
            m_Snapshots[m_CurrentIndex].ignorePlatforms = ncc.ignorePlatforms;

            // Record parameter values
            if (m_TriggerParameters != null)
            {
                for (int i = 0; i < m_TriggerParameters.Length; ++i)
                    m_TriggerParameters[i].RecordValue(m_CurrentIndex);
                for (int i = 0; i < m_SwitchParameters.Length; ++i)
                    m_SwitchParameters[i].RecordValue(m_CurrentIndex);
                for (int i = 0; i < m_IntParameters.Length; ++i)
                    m_IntParameters[i].RecordValue(m_CurrentIndex);
                for (int i = 0; i < m_FloatParameters.Length; ++i)
                    m_FloatParameters[i].RecordValue(m_CurrentIndex);
                for (int i = 0; i < m_TransformParameters.Length; ++i)
                    m_TransformParameters[i].RecordValue(m_CurrentIndex);
                for (int i = 0; i < m_VectorParameters.Length; ++i)
                    m_VectorParameters[i].RecordValue(m_CurrentIndex);
            }

            if (onTicked != null)
                onTicked();
        }

        int WrapIndex(int index)
        {
            if (index < 0)
                return index + m_Snapshots.Length;
            if (index >= m_Snapshots.Length)
                return index - m_Snapshots.Length;
            return index;
        }

        void IncrementIndex()
        {
            m_CurrentIndex = WrapIndex(++m_CurrentIndex);
            if (m_CurrentCount < m_Snapshots.Length)
                ++m_CurrentCount;
        }

        public MotionControllerDebugSnapshot GetLatestSnapshot()
        {
            return m_Snapshots[m_CurrentIndex];
        }

        public MotionControllerDebugSnapshot GetSnapshot (int offset)
        {
            if (offset >= m_FrameCount)
                offset = m_FrameCount - 1;

            int index = WrapIndex(m_CurrentIndex - offset);

            return m_Snapshots[index];
        }

        public int GetFrameNumber (int offset)
        {
            if (offset >= m_FrameCount)
                offset = m_FrameCount - 1;

            int index = WrapIndex(m_CurrentIndex - offset);

            return m_Snapshots[index].frame;
        }

        public int GetIndexFromOffset (int offset)
        {
            if (offset >= m_FrameCount)
                offset = m_FrameCount - 1;

            return WrapIndex(m_CurrentIndex - offset);
        }

        public int GetValues(GraphContents valueType, float[] output)
        {
            if (output == null)
                return 0;

            for (int i = 0; i < m_BufferSize; ++i)
            {
                int index = WrapIndex(m_CurrentIndex - i);
                if (i < m_CurrentCount)
                {
                    switch (valueType)
                    {
                        case GraphContents.Speed:
                            output[i] = m_Snapshots[index].velocity.magnitude;
                            break;
                        case GraphContents.RawSpeed:
                            output[i] = m_Snapshots[index].rawVelocity.magnitude;
                            break;
                        case GraphContents.HorizontalSpeed:
                            output[i] = Vector3.ProjectOnPlane(m_Snapshots[index].velocity, m_Snapshots[index].upTarget).magnitude;
                            break;
                        case GraphContents.RawHorizontalSpeed:
                            output[i] = Vector3.ProjectOnPlane(m_Snapshots[index].rawVelocity, m_Snapshots[index].upTarget).magnitude;
                            break;
                        case GraphContents.UpVelocity:
                            output[i] = Vector3.Dot(m_Snapshots[index].velocity, m_Snapshots[index].upTarget);
                            break;
                        case GraphContents.RawUpVelocity:
                            output[i] = Vector3.Dot(m_Snapshots[index].rawVelocity, m_Snapshots[index].upTarget);
                            break;
                        case GraphContents.WorldHeight:
                            output[i] = m_Snapshots[index].position.y;
                            break;
                        case GraphContents.GroundSlope:
                            {
                                if (m_Snapshots[index].isGrounded)
                                    output[i] = Vector3.Angle(m_Snapshots[index].groundNormal, m_Snapshots[i].upTarget);
                                else
                                    output[i] = 0f;
                            }
                            break;
                        case GraphContents.GroundSurfaceSlope:
                            {
                                if (m_Snapshots[index].isGrounded)
                                    output[i] = Vector3.Angle(m_Snapshots[index].groundSurfaceNormal, m_Snapshots[i].upTarget);
                                else
                                    output[i] = 0f;
                            }
                            break;
                        case GraphContents.InputScale:
                            output[i] = m_Snapshots[index].inputScale;
                            break;
                        case GraphContents.ExternalForceMagnitude:
                            output[i] = m_Snapshots[index].externalForceMove.magnitude;
                            break;
                        default:
                            Debug.LogError("Requesting non-floating point values with an floating point output array");
                            return 0;
                    }
                }
                else
                    output[i] = 0f;
            }
            return m_CurrentCount;
        }

        public int GetValues(GraphContents valueType, int[] output)
        {
            if (output == null)
                return 0;

            for (int i = 0; i < m_BufferSize; ++i)
            {
                int index = WrapIndex(m_CurrentIndex - i);
                if (i < m_CurrentCount)
                {
                    switch (valueType)
                    {
                        case GraphContents.Depenetrations:
                            output[i] = m_Snapshots[index].depenetrations;
                            break;
                        case GraphContents.MoveIterations:
                            output[i] = m_Snapshots[index].moveIterations;
                            break;
                        default:
                            Debug.LogError("Requesting non-integer values with an integer output array");
                            return 0;
                    }
                }
                else
                    output[i] = 0;
            }
            return m_CurrentCount;
        }

        public int GetValues(GraphContents valueType, bool[] output)
        {
            if (output == null)
                return 0;

            for (int i = 0; i < m_BufferSize; ++i)
            {
                int index = WrapIndex(m_CurrentIndex - i);
                if (i < m_CurrentCount)
                {
                    switch (valueType)
                    {
                        case GraphContents.IsGrounded:
                            output[i] = m_Snapshots[index].isGrounded;
                            break;
                        default:
                            Debug.LogError("Requesting non-bool values with an bool output array");
                            return 0;
                    }
                }
                else
                    output[i] = false;
            }
            return m_CurrentCount;
        }

        public int GetValues(GraphContents valueType, string[] output)
        {
            if (output == null)
                return 0;

            for (int i = 0; i < m_BufferSize; ++i)
            {
                //int index = WrapIndex(m_CurrentIndex - i);
                if (i < m_CurrentCount)
                {
                    switch (valueType)
                    {
                        //case GraphContents.State:
                        //    output[i] = m_Snapshots[index].state;
                        //    break;
                        default:
                            Debug.LogError("Requesting non-string values with a string output array");
                            return 0;
                    }
                }
                else
                    output[i] = null;
            }
            return m_CurrentCount;
        }

        //private void OnDestroy()
        //{
        //    Debug.Log("Buffer Destroyed");
        //}
    }
}