using UnityEngine;
using System.Collections;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-objectswapejector.html")]
    public class ObjectSwapEjector : BaseEjectorBehaviour
    {
        [Header("Ejector Settings")]

        [SerializeField, RequiredObjectProperty, Tooltip("The transform of the object to replace.")]
        private Transform m_TargetTransform = null;

        [SerializeField, NeoPrefabField(typeof(IBulletCasing), required = true), Tooltip("The pooled object to replace the animated weapon shell with.")]
        private PooledObject m_ShellPrefab = null;

        [SerializeField, Tooltip("Should the shell be ejected the moment the weapon fires.")]
        private bool m_EjectOnFire = false;

        [SerializeField, Tooltip("The time between the ejector being triggered and the shells being swapped.")]
        private float m_Delay = 0f;

        public override bool ejectOnFire { get { return m_EjectOnFire; } }

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
            if (m_ShellPrefab != null && m_TargetTransform != null)
                StartCoroutine(EjectCoroutine());
        }

        IEnumerator EjectCoroutine()
        {
            // Delay if required
            if (m_Delay > Mathf.Epsilon)
                yield return new WaitForSeconds(m_Delay);

            // Record position & rotation
            Vector3 oldPos = m_TargetTransform.position;
            Vector3 oldFwd = m_TargetTransform.forward;

            // Wait 1 frame
            yield return null;

            // Get new position & rotation
            Vector3 newPos = m_TargetTransform.position;
            Vector3 newFwd = m_TargetTransform.forward;

            // Spawn casing
            bool player = firearm.wielder != null && firearm.wielder.isLocalPlayerControlled;
            Vector3 scale = (player) ? m_TargetTransform.lossyScale : Vector3.one;
            IBulletCasing casing = PoolManager.GetPooledObject<IBulletCasing>(
                m_ShellPrefab,
                newPos,
                Quaternion.LookRotation(newFwd),
                scale
            );

            // Set the casing flying
            float inverseDeltaTime = 1f / Time.deltaTime;
            casing.Eject(
                (newPos - oldPos) * inverseDeltaTime,
                Quaternion.FromToRotation(oldFwd, newFwd).eulerAngles * inverseDeltaTime,
                player
            );

            // Disable target object
            m_TargetTransform.gameObject.SetActive(false);
        }
    }
}