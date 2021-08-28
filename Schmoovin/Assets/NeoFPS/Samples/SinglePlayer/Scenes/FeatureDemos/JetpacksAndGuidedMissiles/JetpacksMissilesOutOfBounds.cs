using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.SinglePlayer;
using UnityEngine.Events;

namespace NeoFPS.Samples.SinglePlayer
{
    public class JetpacksMissilesOutOfBounds : MonoBehaviour
    {
        [SerializeField]
        private float m_BoundsWidth = 480f;
        [SerializeField]
        private float m_WarningDuration = 5f;
        [SerializeField]
        private float m_ShotSpacing = 5f;
        [SerializeField]
        private Transform[] m_TurretTransforms = { };
        [SerializeField]
        private float m_TurnAngle = 0.5f;

        public static event UnityAction<bool> onIsOutOfBoundsChanged;

        private ModularFirearm[] m_TurretFirearms = null;
        private Transform m_CharacterTransform = null;
        private bool m_OutOfBounds = false;
        private float m_OutOfBoundsTimer = 0f;
        private int m_TriggerCounter = 0;
        private ModularFirearm m_TriggeredTurret = null;

        private void Start()
        {
            // Get firearms
            m_TurretFirearms = new ModularFirearm[m_TurretTransforms.Length];
            for (int i = 0; i < m_TurretTransforms.Length; ++i)
                m_TurretFirearms[i] = m_TurretTransforms[i].GetComponentInParent<ModularFirearm>();

            FpsSoloCharacter.onLocalPlayerCharacterChange += OnPlayerCharacterChanged;
            OnPlayerCharacterChanged(FpsSoloCharacter.localPlayerCharacter);
        }

        private void OnDestroy()
        {
            FpsSoloCharacter.onLocalPlayerCharacterChange -= OnPlayerCharacterChanged;
        }

        private void FixedUpdate()
        {
            // Release trigger on turret
            if (m_TriggeredTurret != null && --m_TriggerCounter <= 0)
            {
                m_TriggeredTurret.trigger.Release();
                m_TriggeredTurret = null;
            }

            var outOfBounds = false;
            if (m_CharacterTransform != null)
            {
                var characterPosition = m_CharacterTransform.position;

                // Rotate turrets
                for (int i = 0; i < m_TurretTransforms.Length; ++i)
                {
                    var t = m_TurretTransforms[i];
                    var offset = t.position - characterPosition;
                    offset.y = 0f;
                    t.rotation = Quaternion.RotateTowards(m_TurretTransforms[i].rotation, Quaternion.LookRotation(offset), m_TurnAngle);
                }

                // Check if currently out of bounds
                if (Mathf.Abs(characterPosition.x) > m_BoundsWidth * 0.5f || Mathf.Abs(characterPosition.z) > m_BoundsWidth * 0.5f)
                    outOfBounds = true;
            }

            // Set out of bounds state
            if (outOfBounds)
            {
                if (!m_OutOfBounds)
                {
                    m_OutOfBounds = true;
                    if (onIsOutOfBoundsChanged != null)
                        onIsOutOfBoundsChanged(true);
                }

                // Shoot
                m_OutOfBoundsTimer -= Time.deltaTime;
                if (m_OutOfBoundsTimer <= 0f)
                {
                    m_OutOfBoundsTimer = m_ShotSpacing;
                    m_TriggeredTurret = GetClosestTurretFirearm();
                    if (m_TriggeredTurret != null)
                    {
                        m_TriggeredTurret.trigger.Press();
                        m_TriggerCounter = 50;
                    }
                }
            }
            else
            {
                if (m_OutOfBounds)
                {
                    m_OutOfBounds = false;
                    if (onIsOutOfBoundsChanged != null)
                        onIsOutOfBoundsChanged(false);
                }
                m_OutOfBoundsTimer = m_WarningDuration;
            }
        }

        void OnPlayerCharacterChanged(ICharacter character)
        {
            if (character as Component != null)
                m_CharacterTransform = character.transform;
            else
                m_CharacterTransform = null;
        }

        ModularFirearm GetClosestTurretFirearm()
        {
            if (m_CharacterTransform == null)
                return null;

            Vector3 characterPosition = m_CharacterTransform.position;
            float closestSqrDistance = float.MaxValue;
            ModularFirearm closest = null;

            for (int i = 0; i < m_TurretTransforms.Length; ++i)
            {
                var sqrDistance = (characterPosition - m_TurretTransforms[i].position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closest = m_TurretFirearms[i];
                }
            }
            return closest;
        }
    }
}