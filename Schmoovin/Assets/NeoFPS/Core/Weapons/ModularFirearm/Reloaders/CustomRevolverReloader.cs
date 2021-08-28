using System.Collections;
using System.Collections.Generic;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
	[HelpURL("https://docs.neofps.com/manual/weaponsref-mb-customrevolverreloader.html")]
	public class CustomRevolverReloader : SimpleReloader
	{
        [Header("Revolver properties")]

        [SerializeField, Tooltip("The empty shells in the revolver cylinder.")]
        private GameObject[] m_EmptyShells = new GameObject[0];

        [SerializeField, Tooltip("The unused shells in the revolver speed loader.")]
        private GameObject[] m_LoaderShells = new GameObject[0];

		private int m_PreviousBulletCount = 0;
        private int m_BulletCount = 0;
		public int bulletCount
		{
			get { return m_BulletCount; }
			set
			{
				m_BulletCount = value;
				for (int i = 0; i < m_EmptyShells.Length; ++i)
				{
					m_EmptyShells [i].SetActive (i < m_BulletCount);
					m_LoaderShells [i].SetActive (i < m_BulletCount);
				}
				m_PreviousBulletCount = m_BulletCount;
			}
		}

		protected override void Awake ()
		{
			base.Awake ();
			m_PreviousBulletCount = startingMagazine;
		}

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

			if (m_EmptyShells.Length != magazineSize)
			{
				GameObject[] temp = new GameObject[magazineSize];
				for (int i = 0; i < m_EmptyShells.Length && i < magazineSize; ++i)
					temp [i] = m_EmptyShells [i];
				m_EmptyShells = temp;
			}
			if (m_LoaderShells.Length != magazineSize)
			{
				GameObject[] temp = new GameObject[magazineSize];
				for (int i = 0; i < m_LoaderShells.Length && i < magazineSize; ++i)
					temp [i] = m_LoaderShells [i];
				m_LoaderShells = temp;
			}
		}
#endif

		public override Waitable Reload ()
		{
			bulletCount = m_PreviousBulletCount;
			return base.Reload ();
		}

		public override void ManualReloadPartial ()
		{
			base.ManualReloadPartial ();
			bulletCount = Mathf.Min (magazineSize, firearm.ammo.currentAmmo + currentMagazine);
		}

        private static readonly NeoSerializationKey k_BulletCountKey = new NeoSerializationKey("bulletCount");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            writer.WriteValue(k_BulletCountKey, bulletCount);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
            int intResult = 0;
            if (reader.TryReadValue(k_BulletCountKey, out intResult, 0))
                bulletCount = intResult;
        }
    }
}