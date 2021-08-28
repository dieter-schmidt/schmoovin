using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.CharacterMotion.Conditions
{
    [MotionGraphElement("Character/Input Vector")]
    public class InputVectorCondition : MotionGraphCondition
    {
        [SerializeField] private InputComponent m_Compare = InputComponent.Magnitude;
        [SerializeField] private float m_CompareValue = 0.5f;
        [SerializeField] private Comparison m_Comparison = Comparison.GreaterThan;

        public enum InputComponent
        {
            Magnitude,
            InputY,
            InputX,
            AbsoluteY,
            AbsoluteX
        }

        public enum Comparison
        {
            GreaterThan,
            LessThan
        }

        public override void OnValidate()
        {
            base.OnValidate();

            if (m_Compare == InputComponent.Magnitude)
                m_CompareValue = Mathf.Clamp(m_CompareValue, 0f, 1f);
            else
                m_CompareValue = Mathf.Clamp(m_CompareValue, -1f, 1f);
        }

        public override bool CheckCondition(MotionGraphConnectable connectable)
        {
            switch (m_Compare)
            {
                case InputComponent.Magnitude:
                    {
                        if (m_Comparison == Comparison.GreaterThan)
                            return controller.inputMoveScale > m_CompareValue;
                        else
                            return controller.inputMoveScale < m_CompareValue;
                    }
                case InputComponent.InputX:
                    {
                        if (m_Comparison == Comparison.GreaterThan)
                            return controller.inputMoveScale * controller.inputMoveDirection.x > m_CompareValue;
                        else
                            return controller.inputMoveScale * controller.inputMoveDirection.x < m_CompareValue;
                    }
                case InputComponent.InputY:
                    {
                        if (m_Comparison == Comparison.GreaterThan)
                            return controller.inputMoveScale * controller.inputMoveDirection.y > m_CompareValue;
                        else
                            return controller.inputMoveScale * controller.inputMoveDirection.y < m_CompareValue;
                    }
                case InputComponent.AbsoluteX:
                    {
                        if (m_Comparison == Comparison.GreaterThan)
                            return Mathf.Abs(controller.inputMoveScale * controller.inputMoveDirection.x) > m_CompareValue;
                        else
                            return Mathf.Abs(controller.inputMoveScale * controller.inputMoveDirection.x) < m_CompareValue;
                    }
                case InputComponent.AbsoluteY:
                    {
                        if (m_Comparison == Comparison.GreaterThan)
                            return Mathf.Abs(controller.inputMoveScale * controller.inputMoveDirection.y) > m_CompareValue;
                        else
                            return Mathf.Abs(controller.inputMoveScale * controller.inputMoveDirection.y) < m_CompareValue;
                    }
            }
            return false;
        }
    }
}