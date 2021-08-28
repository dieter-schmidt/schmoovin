using NeoFPS.Constants;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/healthref-mb-armoureddamagehandler.html")]
    public class ArmouredDamageHandler : BasicDamageHandler
    {
        [Header("Armour")]

        [SerializeField, Tooltip("The inventory key of the armour type")]
        private FpsInventoryKey m_InventoryKey = FpsInventoryKey.ArmourBody;

        [SerializeField, Range(0f, 1f), Tooltip("The amount of damage the armour should nullify")]
        private float m_DamageMitigation = 1f;

        [SerializeField, Tooltip("A multiplier used to modify how much armour is destroyed by the incoming damage.")]
        private float m_ArmourDamageMultiplier = 0.5f;

        private IInventory m_Inventory = null;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            m_ArmourDamageMultiplier = Mathf.Clamp(m_ArmourDamageMultiplier, 0f, 100f);
        }
#endif

        bool GetDamageAfterArmour(ref float damage, DamageType t)
        {
            // Initial checks
            if (m_Inventory == null)
                return true;

            // Get the armour item from the inventory
            var item = m_Inventory.GetItem(m_InventoryKey);
            if (item == null || item.quantity == 0)
                return true;
            
            // Get mitigated damage amount
            float mitigated = damage * m_DamageMitigation;

            // Scale shield damage
            int armourDamage = Mathf.CeilToInt(mitigated * m_ArmourDamageMultiplier);

            // Clamp mitigated damage to shield
            if (armourDamage > item.quantity)
                armourDamage = item.quantity;

            // Set new shield value
            item.quantity -= armourDamage;

            // Reverse damage scale (to calculate absorbed)
            mitigated = armourDamage / m_ArmourDamageMultiplier;

            // Modify damage
            damage -= mitigated;

            return damage > 0f;
        }

        protected override void Awake()
        {
            base.Awake();
            m_Inventory = GetComponentInParent<IInventory>();
        }

        public override DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source)
        {
            if (GetDamageAfterArmour(ref damage, source.outDamageFilter.GetDamageType()))
                return base.AddDamage(damage, hit, source);
            else
                return DamageResult.Blocked;
        }

        public override DamageResult AddDamage(float damage, RaycastHit hit)
        {
            if (GetDamageAfterArmour(ref damage, DamageType.Default))
                return base.AddDamage(damage, hit);
            else
                return DamageResult.Blocked;
        }

        public override DamageResult AddDamage(float damage, IDamageSource source)
        {
            if (GetDamageAfterArmour(ref damage, source.outDamageFilter.GetDamageType()))
                return base.AddDamage(damage, source);
            else
                return DamageResult.Blocked;
        }

        public override DamageResult AddDamage(float damage)
        {
            if (GetDamageAfterArmour(ref damage, DamageType.Default))
                return base.AddDamage(damage);
            else
                return DamageResult.Blocked;
        }
    }
}