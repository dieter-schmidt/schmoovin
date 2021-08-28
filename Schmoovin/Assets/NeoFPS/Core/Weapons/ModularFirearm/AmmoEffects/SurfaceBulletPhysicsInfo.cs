using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using System;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-so-surfacebulletphysicsinfo.html")]
    [CreateAssetMenu(fileName = "SurfaceBulletPhysicsInfo", menuName = "NeoFPS/Surface Bullet-Physics Info", order = NeoFpsMenuPriorities.ungrouped_surfaceBulletPhysics)]
    public class SurfaceBulletPhysicsInfo : ScriptableObject
    {
        [SerializeField, Tooltip("The speed at which the penetration values below are accurate. As speed drops away, projectiles cannot penetrate as far.")]
        private float m_PenetrationSpeed = 850f;
        [SerializeField, Tooltip("Penetration properties for each surface type")]
        private PenetrationInfo[] m_Penetration = { };
        [SerializeField, Tooltip("Ricochet properties for each surface type")]
        private RicochetInfo[] m_Ricochet = { };

#if UNITY_EDITOR
        [SerializeField] private bool[] m_Expand = { };
#endif

        [Serializable]
        public struct PenetrationInfo
        {
			[SerializeField, Tooltip("Can the projectile penetrate this surface type.")]
            private bool m_CanPenetrate;
            [SerializeField, Tooltip("The maximum depth the projectile can penetrate this surface when travelling at the \"Penetration Speed\" specified above.")]
            private float m_PenetrationDepth;
            [SerializeField, Tooltip("The maximum angle of incidence with the surface where a projectile can penetrate.")]
            private float m_MaxPenetrationAngle;
            [SerializeField, Range(0f, 1f), Tooltip("A 0-1 value representing how much the projectile direction can be altered by moving through the surface.")]
            private float m_MaxDeflection;
			
            public bool canPenetrate { get { return m_CanPenetrate; } }
            public float penetrationDepth { get { return m_PenetrationDepth; } }
            public float maxPenetrationAngle { get { return m_MaxPenetrationAngle; } }
            public float maxDeflection { get { return m_MaxDeflection; } }

            public PenetrationInfo(bool canPenetrate)
            {
                m_CanPenetrate = canPenetrate;
                m_PenetrationDepth = 0.1f;
                m_MaxPenetrationAngle = 40f;
                m_MaxDeflection = 0f;
            }
        }

        [Serializable]
        public struct RicochetInfo
        { 
            [SerializeField, Tooltip("Can the projectile ricochet off this surface.")]
            private bool m_CanRicochet;
            [SerializeField, Tooltip("The speed below which a projectile cannot ricochet.")]
            private float m_MinRicochetSpeed;
            [SerializeField, Tooltip("The minimum angle of incidence that a projectile will ricochet. Below this and the projectile will penetrate or be destroyed.")]
            private float m_MinRicochetAngle;
            [SerializeField, Tooltip("The angle of incidence where the projectile is deflected with the least speed loss.")]
            private float m_StrongRicochetAngle;
            [SerializeField, Tooltip("The multiplier applied to the projectile speed when it hits at the minimum angle (as close to straight on as it can ricochet).")]
            private float m_MinSpeedMultiplier;
            [SerializeField, Tooltip("The multiplier applied to the projectile speed when it hits at the strong ricochet angle from the surface.")]
            private float m_MaxSpeedMultiplier;
            [SerializeField, Range(0f, 1f), Tooltip("Surface friction is used to add randomness to the ricocheted projectile's direction. Lower friction means a more predictable ricochet.")]
            private float m_SurfaceFriction;

            public bool canRicochet { get { return m_CanRicochet; } }
            public float minRicochetSpeed { get { return m_MinRicochetSpeed; } }
            public float minRicochetAngle { get { return m_MinRicochetAngle; } }
            public float strongRicochetAngle { get { return m_StrongRicochetAngle; } }
            public float surfaceFriction { get { return m_SurfaceFriction; } }
            public float minSpeedMultiplier { get { return m_MinSpeedMultiplier; } }
            public float maxSpeedMultiplier { get { return m_MaxSpeedMultiplier; } }

            public RicochetInfo(bool canPenetrate)
            {
                m_CanRicochet = canPenetrate;
                m_MinRicochetSpeed = 50f;
                m_MinRicochetAngle = 45f;
                m_StrongRicochetAngle = 75f;
                m_MinSpeedMultiplier = 0.1f;
                m_MaxSpeedMultiplier = 0.9f;
                m_SurfaceFriction = 0f;
            }
        }

        public float penetrationSpeed
        {
            get { return m_PenetrationSpeed; }
        }

        public PenetrationInfo[] penetration
        {
            get { return m_Penetration; }
        }

        public RicochetInfo[] ricochet
        {
            get { return m_Ricochet; }
        }
        
        public void OnValidate()
        {
            if (FpsSurfaceMaterial.count != m_Penetration.Length || FpsSurfaceMaterial.count != m_Ricochet.Length)
            {
                var p = new PenetrationInfo[FpsSurfaceMaterial.count];
                var r = new RicochetInfo[FpsSurfaceMaterial.count];

                // Update penetration elements
                int index = 0;
                for (; index < m_Penetration.Length && index < FpsSurfaceMaterial.count; ++index)
                    p[index] = m_Penetration[index];
                for (; index < FpsSurfaceMaterial.count; ++index)
                    p[index] = new PenetrationInfo(false);
                m_Penetration = p;

                // Update ricochet elements
                index = 0;
                for (; index < m_Ricochet.Length && index < FpsSurfaceMaterial.count; ++index)
                    r[index] = m_Ricochet[index];
                for (; index < FpsSurfaceMaterial.count; ++index)
                    r[index] = new RicochetInfo(false);
                m_Ricochet = r;

#if UNITY_EDITOR
                var e = new bool[FpsSurfaceMaterial.count];
                // Update expanded
                index = 0;
                for (; index < m_Expand.Length && index < FpsSurfaceMaterial.count; ++index)
                    e[index] = m_Expand[index];
                for (; index < FpsSurfaceMaterial.count; ++index)
                    e[index] = true;

                m_Expand = e;
#endif
            }
}
    }
}