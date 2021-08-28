using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/surfacesref-mb-particlesystemhitfxbehaviour.html")]
    public class PooledObjectHitFxBehaviour : BaseHitFxBehaviour
    {
        [SerializeField, Tooltip("The pooled objects to choose from")]
        private PooledObject[] m_Prototypes = { };

        public override bool forceInitialise { get { return false; } }

        private void Awake()
        {
            // Check how many non-null entries exist
            int valid = 0;
            for (int i = 0; i < m_Prototypes.Length; ++i)
            {
                if (m_Prototypes[i] != null)
                    ++valid;
            }
            
            // Clean up the array if it contains null
            if (valid != m_Prototypes.Length)
            {
                int itr = 0;
                var replacement = new PooledObject[valid];
                for (int i = 0; i < m_Prototypes.Length; ++i)
                {
                    if (m_Prototypes[i] != null)
                        replacement[itr++] = m_Prototypes[i];
                }
                m_Prototypes = replacement;
            }
        }

        public override void OnActiveSceneChange()
        {
            // Nothing required, as pooled objects are per-scene
        }

        public override void Hit(GameObject hitObject, Vector3 position, Vector3 normal)
        {
            Hit(hitObject, position, normal, 1f);
        }

        public override void Hit(GameObject hitObject, Vector3 position, Vector3 normal, float size)
        {
            // Get the prototype to use
            PooledObject prototype = null;
            switch (m_Prototypes.Length)
            {
                case 0:
                    return;
                case 1:
                    prototype = m_Prototypes[0];
                    break;
                default:
                    prototype = m_Prototypes[Random.Range(0, m_Prototypes.Length)];
                    break;
            }

            // Place the pooled object and activate
            PoolManager.GetPooledObject<PooledObject>(prototype, position, Quaternion.FromToRotation(Vector3.forward, normal), new Vector3(size, size, size));
        }

        public override void Hit(GameObject hitObject, Vector3 position, Vector3 normal, Vector3 ray, float size)
        {
            Hit(hitObject, position, normal, size);
        }

        public override void Hit(GameObject hitObject, Vector3 position, Vector3 normal, Vector3 ray, float size, bool decal)
        {
            Hit(hitObject, position, normal, size);
        }
    }
}
