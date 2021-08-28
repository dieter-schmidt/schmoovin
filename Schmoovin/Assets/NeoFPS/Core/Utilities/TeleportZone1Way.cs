using UnityEngine;
using NeoCC;
using System.Collections;
using UnityEngine.Events;

namespace NeoFPS
{
    public class TeleportZone1Way : CharacterTriggerZone
    {
        [SerializeField, Tooltip("The transform to teleport to. The character will match its position and orientation.")]
        private Transform m_TargetTransform = null;
        [SerializeField, Tooltip("The time between entering the trigger zone and teleporting. Allows for effects and feedback to communicate what's going on.")]
        private float m_TeleportDelay = 0f;
        [SerializeField, Tooltip("An event fired as soon as a character enters the teleport trigger zone.")]
        private UnityEvent m_OnEntered = null;
        [SerializeField, Tooltip("An event fired if the character leaves before the teleport delay is over.")]
        private UnityEvent m_OnCancelled = null;
        [SerializeField, Tooltip("An event fired after the character is teleported.")]
        private UnityEvent m_OnTeleported = null;

        private Coroutine m_DelayedTeleport = null;

        private void OnValidate()
        {
            m_TeleportDelay = Mathf.Clamp(m_TeleportDelay, 0f, 10f);
        }

        protected override void OnCharacterEntered(ICharacter c)
        {
            base.OnCharacterEntered(c);

            if (m_TargetTransform != null)
            {
                m_OnEntered.Invoke();

                var ncc = c.GetComponent<NeoCharacterController>();
                if (m_TeleportDelay == 0f)
                {
                    ncc.Teleport(m_TargetTransform.position, m_TargetTransform.rotation, false);
                    m_OnTeleported.Invoke();
                }
                else
                    m_DelayedTeleport = StartCoroutine(DelayedTeleport(ncc));
            }
        }

        protected override void OnCharacterExited(ICharacter c)
        {
            base.OnCharacterExited(c);

            if (m_DelayedTeleport != null)
            {
                StopCoroutine(m_DelayedTeleport);
                m_OnCancelled.Invoke();
            }
        }

        IEnumerator DelayedTeleport(NeoCharacterController ncc)
        {
            // Wait
            float timer = 0f;
            while (timer < m_TeleportDelay)
            {
                yield return null;
                timer += Time.deltaTime;
            }

            m_DelayedTeleport = null;

            // Teleport
            ncc.Teleport(m_TargetTransform.position, m_TargetTransform.rotation, false);
            m_OnTeleported.Invoke();
        }
    }
}
