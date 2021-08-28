using UnityEngine;
using System.Collections;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-multiobjectswapejector.html")]
    public class MultiObjectSwapEjector : BaseEjectorBehaviour
    {
        [Header("Ejector Settings")]

        [SerializeField, Tooltip("A proxy transform where ejected shells will be spawned.")]
        private Transform[] m_TargetTransforms = new Transform[0];

        [SerializeField, NeoPrefabField(typeof(IBulletCasing), required = true), Tooltip("The pooled object to replace the animated weapon shells with.")]
        private PooledObject m_ShellPrefab = null;

        [SerializeField, Tooltip("Should the shells be ejected the moment the weapon fires.")]
        private bool m_EjectOnFire = false;

        [SerializeField, Tooltip("Should the ejector swap animated shells that are inactive or ignore them.")]
        private bool m_SwapInactive = false;

        [SerializeField, Tooltip("The time between the ejector being triggered and the shells being swapped.")]
        private float m_Delay = 0f;

        public override bool ejectOnFire { get { return m_EjectOnFire; } }

        private Vector3[] m_OldPositions = null;
        private Vector3[] m_OldForwards = null;

#if UNITY_EDITOR
        void OnValidate()
        {
            m_Delay = Mathf.Clamp(m_Delay, 0f, 10f);

            // Check shell prefab is valid
            if (m_ShellPrefab != null && m_ShellPrefab.GetComponent<IBulletCasing>() == null)
            {
                Debug.LogError("Shell prefab must have IBulletCasing component attached: " + m_ShellPrefab.name);
                m_ShellPrefab = null;
            }
        }
#endif

        public override bool isModuleValid
        {
            get { return m_ShellPrefab != null; }
        }

        public override void Eject()
        {
            // Uses a coroutine to check it's on the update frame
            if (m_ShellPrefab != null && m_TargetTransforms.Length > 0)
            {
                if (m_OldPositions == null)
                    m_OldPositions = new Vector3[m_TargetTransforms.Length];
                if (m_OldForwards == null)
                    m_OldForwards = new Vector3[m_TargetTransforms.Length];
                StartCoroutine(EjectCoroutine());
            }
        }

        IEnumerator EjectCoroutine()
        {
            // Delay if required
            if (m_Delay > Mathf.Epsilon)
                yield return new WaitForSeconds(m_Delay);

            // Record position & rotation
            for (int i = 0; i < m_TargetTransforms.Length; ++i)
            {
                if (m_TargetTransforms[i] == null)
                    continue;
                m_OldPositions[i] = m_TargetTransforms[i].position;
                m_OldForwards[i] = m_TargetTransforms[i].forward;
            }

            // Wait 1 frame
            yield return null;

            // Check if player
            bool player = firearm.wielder != null && firearm.wielder.isLocalPlayerControlled;

            // Process objects
            for (int i = 0; i < m_TargetTransforms.Length; ++i)
            {
                if (m_TargetTransforms[i] == null)
                    continue;

                // Check if target is inactive
                GameObject go = m_TargetTransforms[i].gameObject;
                if (!m_SwapInactive && m_TargetTransforms[i].gameObject.activeSelf != true)
                    continue;

                // Get new position & rotation
                Vector3 newPos = m_TargetTransforms[i].position;
                Vector3 newFwd = m_TargetTransforms[i].forward;

                // Spawn casing
                Vector3 scale = (player) ? m_TargetTransforms[i].lossyScale : Vector3.one;
                IBulletCasing casing = PoolManager.GetPooledObject<IBulletCasing>(
                    m_ShellPrefab,
                    newPos,
                    Quaternion.LookRotation(newFwd),
                    scale
                );

                // Set the casing flying
                float inverseDeltaTime = 1f / Time.deltaTime;
                casing.Eject(
                    (newPos - m_OldPositions[i]) * inverseDeltaTime,
                    Quaternion.FromToRotation(m_OldForwards[i], newFwd).eulerAngles * inverseDeltaTime,
                    player
                );

                // Disable target object
                go.SetActive(false);
            }
        }
    }
}