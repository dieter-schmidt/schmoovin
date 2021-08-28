using UnityEngine;
using System.Collections;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-standardshelleject.html")]
	public class StandardShellEject : BaseEjectorBehaviour
    {
        [Header("Ejector Settings")]

        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("A proxy transform where ejected shells will be spawned.")]
		private Transform m_ShellEjectProxy = null;

        [SerializeField, NeoPrefabField(typeof(IBulletCasing), required = true), Tooltip("The shell prefab object to spawn.")]
        private PooledObject m_ShellPrefab = null;

        [SerializeField, Tooltip("The delay type between firing and ejecting a shell.")]
        private FirearmDelayType m_DelayType = FirearmDelayType.None;

        [SerializeField, Tooltip("The delay time between firing and ejecting a shell if the delay type is set to elapsed time.")]
        private float m_Delay = 0f;

        [SerializeField, Tooltip("The ejected shell speed directly out from the ejector.")]
        private float m_OutSpeed = 5f;

        [SerializeField, Tooltip("The ejected shell speed back over the wielder's shoulder.")]
        private float m_BackSpeed = 1f;

        [SerializeField, Tooltip("How much of the character's velocity should be added to the ejected shells.")]
        private float m_InheritVelocity = 0.25f;

        [SerializeField, Tooltip("The minimum angular velocity on each axis (will be picked at random between this an B).")]
        private Vector3 m_AngularVelocityA = Vector3.zero;

        [SerializeField, Tooltip("The maximum angular velocity on each axis (will be picked at random between this an A).")]
        private Vector3 m_AngularVelocityB = Vector3.zero;
        
		public override bool ejectOnFire { get { return m_DelayType != FirearmDelayType.ExternalTrigger; } }

        private WaitForEndOfFrame m_WaitForEndOfFrame = new WaitForEndOfFrame();

        #if UNITY_EDITOR
        void OnValidate ()
		{
			m_Delay = Mathf.Clamp (m_Delay, 0f, 5f);
			m_OutSpeed = Mathf.Clamp (m_OutSpeed, 0f, 10f);
			m_BackSpeed = Mathf.Clamp (m_BackSpeed, 0f, 5f);
			m_InheritVelocity = Mathf.Clamp01 (m_InheritVelocity);

			// Check shell prefab is valid
			if (m_ShellPrefab != null && m_ShellPrefab.GetComponent<IBulletCasing> () == null)
			{
				Debug.Log ("Shell prefab must have IBulletCasing component attached: " + m_ShellPrefab.name);
				m_ShellPrefab = null;
			}
		}
        #endif

        public override bool isModuleValid
        {
            get { return m_ShellEjectProxy != null && m_ShellPrefab != null; }
        }

        public override void Eject ()
		{
            // Uses a coroutine to check it's on the update frame
            StartCoroutine(EjectCoroutine());
		}

		void DoEject ()
		{
			if (m_ShellPrefab != null)
			{
                // Calculate the velocity (including inheriting from character motion controller
				Vector3 velocity = m_ShellEjectProxy.up * m_OutSpeed;
				if (m_BackSpeed > 0f)
					velocity += m_ShellEjectProxy.forward * -m_BackSpeed;

                Vector3 characterVelocity = Vector3.zero;

                if (firearm.wielder != null)
                {
                    characterVelocity = firearm.wielder.motionController.characterController.velocity;
                    if (m_InheritVelocity > 0f)
                        velocity += characterVelocity * m_InheritVelocity;
                }
                characterVelocity = Vector3.zero;

                // Check if player
                bool player = firearm.wielder != null && firearm.wielder.isLocalPlayerControlled;
                Vector3 scale = (player) ? m_ShellEjectProxy.lossyScale : Vector3.one;

                // Spawn a shell at the position
                IBulletCasing casing = PoolManager.GetPooledObject<IBulletCasing> (
                    m_ShellPrefab,
                    m_ShellEjectProxy.position + (velocity * Time.fixedDeltaTime),
                    Quaternion.LookRotation (m_ShellEjectProxy.forward),
                    scale
                );

                // Set the casing flying
				casing.Eject (
					velocity,
					new Vector3 (
						Random.Range (m_AngularVelocityA.x, m_AngularVelocityB.x),
						Random.Range (m_AngularVelocityA.y, m_AngularVelocityB.y),
						Random.Range (m_AngularVelocityA.z, m_AngularVelocityB.z)
					),
                    player
				);
			}
		}

        IEnumerator EjectCoroutine()
        {
            switch (m_DelayType)
            {
                case FirearmDelayType.None:
                    yield return m_WaitForEndOfFrame;
                    DoEject();
                    break;
                case FirearmDelayType.ElapsedTime:
                    if (m_Delay > 0f)
                        yield return new WaitForSeconds(m_Delay);
                    yield return m_WaitForEndOfFrame;
                    DoEject();
                    break;
                case FirearmDelayType.ExternalTrigger:
                    yield return m_WaitForEndOfFrame;
                    DoEject();
                    break;
            }
        }
	}
}