using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-shieldeddamagehandler.html")]
    public class ShieldedDamageHandler : BasicDamageHandler
    {
        private IShieldManager m_ShieldManager = null;

        protected override void Awake()
        {
            base.Awake();
            m_ShieldManager = GetComponentInParent<IShieldManager>();
        }

        bool GetShieldedDamage(ref float damage, DamageType t)
        {
            if (m_ShieldManager != null)
                damage = m_ShieldManager.GetShieldedDamage(damage, t);
            return damage > 0f;
        }
        
        public override DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source)
        {
            if (GetShieldedDamage(ref damage, source.outDamageFilter.GetDamageType()))
                return base.AddDamage(damage, hit, source);
            else
                return DamageResult.Blocked;
        }

        public override DamageResult AddDamage(float damage, RaycastHit hit)
        {
            if (GetShieldedDamage(ref damage, DamageType.Default))
                return base.AddDamage(damage, hit);
            else
                return DamageResult.Blocked;
        }

        public override DamageResult AddDamage(float damage, IDamageSource source)
        {
            if (GetShieldedDamage(ref damage, source.outDamageFilter.GetDamageType()))
                return base.AddDamage(damage, source);
            else
                return DamageResult.Blocked;
        }

        public override DamageResult AddDamage(float damage)
        {
            if (GetShieldedDamage(ref damage, DamageType.Default))
                return base.AddDamage(damage);
            else
                return DamageResult.Blocked;
        }
    }
}
