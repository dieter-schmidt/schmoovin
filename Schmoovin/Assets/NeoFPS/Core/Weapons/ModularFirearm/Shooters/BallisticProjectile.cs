using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-ballisticprojectile.html")]
	public class BallisticProjectile : BaseBallisticProjectile
	{
        [SerializeField, Tooltip("The time after the bullet hits an object before it is returned to the pool (allows trail renderers to complete).")]
        private float m_RecycleDelay = 0.5f;
        
        private bool m_Release = false;
        private float m_Timeout = 0f;
        
		public override void Fire (Vector3 position, Vector3 velocity, float gravity, IAmmoEffect effect, Transform ignoreRoot, LayerMask layers, IDamageSource damageSource = null, bool wait1 = false)
        {
            // Reset pooling
            m_Release = false;
            m_Timeout = m_RecycleDelay;

            base.Fire(position, velocity, gravity, effect, ignoreRoot, layers, damageSource, wait1);
		}

		protected override void FixedUpdate ()
		{
            if (m_Release)
            {
                if (m_RecycleDelay <= 0f)
                    ReleaseProjectile();
                else
                {
                    if (meshRenderer != null && meshRenderer.enabled)
                        meshRenderer.enabled = false;
                    m_Timeout -= Time.deltaTime;
                    if (m_Timeout < 0f)
                        ReleaseProjectile();
                }
            }
            else
                base.FixedUpdate();
		}

        protected override void OnHit(RaycastHit hit)
        {
            m_Release = true;
        }
        
		protected override void Update ()
		{
            if (!m_Release)
                base.Update();
		}
        
        private static readonly NeoSerializationKey k_ReleaseKey = new NeoSerializationKey("release");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            
            writer.WriteValue(k_ReleaseKey, m_Release);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_ReleaseKey, out m_Release, m_Release);

            base.ReadProperties(reader, nsgo);
        }
    }
}