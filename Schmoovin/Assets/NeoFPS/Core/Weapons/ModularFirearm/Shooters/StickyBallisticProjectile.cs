using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine.Animations;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-stickyballisticprojectile.html")]
    [RequireComponent(typeof(ParentConstraint))]
    public class StickyBallisticProjectile : BaseBallisticProjectile, IDamageSource
    {
        [SerializeField, NeoObjectInHierarchyField(false, required = true), Tooltip("")]
        private Transform m_OffsetTransform = null;

        [SerializeField, Tooltip("")]
        private float m_MaxLifetime = 300f;

        private ParentConstraint m_Constraint = null;
        private float m_Timeout = 0f;

        public DamageFilter outDamageFilter
        {
            get;
            set;
        }

        public IController controller
        {
            get;
            private set;
        }

        public Transform damageSourceTransform
        {
            get;
            private set;
        }

        public string description
        {
            get;
            private set;
        }

        protected override void Awake()
        {
            base.Awake();
            m_Constraint = GetComponent<ParentConstraint>();
        }

        public override void Fire(Vector3 position, Vector3 velocity, float gravity, IAmmoEffect effect, Transform ignoreRoot, LayerMask layers, IDamageSource damageSource = null, bool wait1 = false)
        {
            if (m_OffsetTransform != null)
            {
                m_OffsetTransform.localPosition = Vector3.zero;
                m_OffsetTransform.localRotation = Quaternion.identity;
            }

            outDamageFilter = damageSource.outDamageFilter;
            controller = damageSource.controller;
            damageSourceTransform = damageSource.damageSourceTransform;
            description = damageSource.description;

            base.Fire(position, velocity, gravity, effect, ignoreRoot, layers, damageSource, wait1);
        }

        private void OnDisable()
        {
            if (m_Constraint.constraintActive)
            {
                m_Timeout = 0f;
                m_Constraint.constraintActive = false;
                m_Constraint.RemoveSource(0);
            }
        }

        protected override void FixedUpdate()
        {
            if (m_Timeout > 0f)
            {
                m_Timeout -= Time.deltaTime;
                if (m_Timeout < 0f)
                    ReleaseProjectile();
                else if (m_Constraint.constraintActive)
                {
                    if (m_Constraint.GetSource(0).sourceTransform == null)
                        ReleaseProjectile();
                }
            }
            else
                base.FixedUpdate();
        }

        protected override void OnHit(RaycastHit hit)
        {
            m_Timeout = m_MaxLifetime;

            // Get the relative postion & rotation
            var p = hit.point;
            var r = localTransform.rotation;
            var t = hit.collider.transform;

            var cs = new ConstraintSource();
            cs.sourceTransform = t;
            cs.weight = 1f;
            m_Constraint.AddSource(cs);
            m_Constraint.constraintActive = true;

            if (m_OffsetTransform != null)
            {
                localTransform.position = t.position;
                localTransform.rotation = t.rotation;
                m_OffsetTransform.position = p;
                m_OffsetTransform.rotation = r;
            }
        }

        private static readonly NeoSerializationKey k_TimeoutKey = new NeoSerializationKey("timeout");
        private static readonly NeoSerializationKey k_Constraint = new NeoSerializationKey("constraint");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_Timeout > 0f)
            {
                writer.WriteValue(k_TimeoutKey, m_Timeout);
                if (m_Constraint.sourceCount > 0)
                {
                    var source = m_Constraint.GetSource(0).sourceTransform;
                    var sourceNsgo = source.GetComponent<NeoSerializedGameObject>();
                    if (sourceNsgo != null)
                        writer.WriteNeoSerializedGameObjectReference(k_Constraint, sourceNsgo, nsgo);
                }
            }
            else
                base.WriteProperties(writer, nsgo, saveMode);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            if (reader.TryReadValue(k_TimeoutKey, out m_Timeout, m_Timeout))
            {
                NeoSerializedGameObject constraint;
                if (reader.TryReadNeoSerializedGameObjectReference(k_Constraint, out constraint, nsgo))
                {
                    var cs = new ConstraintSource();
                    cs.sourceTransform = constraint.transform;
                    cs.weight = 1f;
                    m_Constraint.AddSource(cs);
                    m_Constraint.constraintActive = true;
                }
            }
            else
                base.ReadProperties(reader, nsgo);
        }
    }
}