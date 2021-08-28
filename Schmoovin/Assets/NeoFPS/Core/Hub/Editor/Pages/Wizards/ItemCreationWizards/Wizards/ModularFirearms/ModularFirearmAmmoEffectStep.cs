using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor;
using NeoFPSEditor.Hub;
using UnityEngine;
using UnityEditor;
using System;
using static NeoFPS.ModularFirearms.RicochetProjectileAmmoEffect;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ModularFirearms
{
    public class ModularFirearmAmmoEffectStep : NeoFpsWizardStep
    {
        [SerializeField, Tooltip("What the firearm does when it hits a target.")]
        private int m_EffectType = -1;
        [SerializeField, Tooltip("What happens next after hitting a target and applying the damage effect.")]
        private int m_ImpactBehaviour = 0;
        [SerializeField, Tooltip("The effect to apply to the shot after it has ricocheted or penetrated.")]
        private int m_FollowUpEffect = -1;
        [SerializeField]
        public AmmoEffectInfo[] effectData = new AmmoEffectInfo[2];

        [Tooltip("The maximum thickness of object the bullet can penetrate.")]
        public float maxPenetration = 0.15f;
        [Tooltip("Randomises the deflected bullet direction within this cone angle")]
        public float maxScatterAngle = 15f;
        [Tooltip("Uses the surface system to show a bullet hit effect on exit. Set this to zero if you don't want it to happen.")]
        public float exitEffectSize = 1f;
        [Tooltip("The optional pooled tracer prototype to use (must implement the IPooledHitscanTrail interface)")]
        public PooledObject tracerPrototype = null;
        [Tooltip("How size (thickness/radius) of the tracer line")]
        public float tracerSize = 0.05f;
        [Tooltip("How long the tracer line will last")]
        public float tracerDuration = 0.05f;
        [Tooltip("The projectile to spawn.")]
        public PooledObject projectilePrefab = null;
        [Tooltip("The gravity for the projectile.")]
        public float gravity = 9.8f;

        [Range(0f, 1f), Tooltip("A blending value based on ricocheting directly along the hit normal (0) vs the reflection of the inbound ray (1)")]
        public float normalVsDeflection = 1f;
        [Tooltip("The number of shots to split the ricochet into (1 is no split).")]
        public int splitCount = 1;
        [Tooltip("How the ricochet speed of the projectile is calculated.")]
        public RicochetSpeedMode speedMode = RicochetSpeedMode.Multiplier;
        [Tooltip("The minimum speed setting of the projectile (dependent on the mode).")]
        public float minSpeedSetting = 0.75f;
        [Tooltip("The maximum speed setting of the projectile (dependent on the mode).")]
        public float maxSpeedSetting = 0.75f;

        [Tooltip("The per-surface bullet physics info")]
        public SurfaceBulletPhysicsInfo surfacePhysics = null;
        [Tooltip("Randomises the deflected bullet direction within this cone angle, dependent on surface settings")]
        public float maxRicochetScatter = 30f;
        [Tooltip("Randomises the penetrating bullet direction within this cone angle, dependent on surface settings")]
        public float maxPenetrationDeflect = 15f;
        [Tooltip("Should the bullet keep ricocheting / penetrating after the first time until it has slowed or travelled far enough")]
        public bool recursive = true;

        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "Ammo Hit Effect Setup"; }
        }

        static readonly string[] effectTypeOptions =
        {
            "Bullet effect. Applies force and damage to the hit object, and spawns a particle effect based on its surface.",
            "Advanced bullet effect. Damage and force fall off based on range or speed.",
            "Explosive. Spawns a pooled explosion object at the impact point which deals damage and applies force.",
            "Particle effect. Applies force and damage to the hit object, and spawns a specific pooled particle effect object."
        };

        static readonly string[] effectTypeSummaries =
        {
            "Bullet effect",
            "Advanced bullet effect",
            "Explosive",
            "Particle effect"
        };

        static readonly string[] impactBehaviourOptions =
        {
            "Standard. When the bullet hits, the effect is applied, end of story.",
            "Simple ricochet. The bullet will apply its effect and then bounce of the surface. It can also be set to split into multiple shots on ricochet.",
            "Simple penetration. The bullet will apply its effect, and then pass through the object based on its thickness.",
            "Complex surface based. The bullet can ricochet or penetrate based on the surface it hits and other factors such as angle of incidence and projectile speed."
        };

        static readonly string[] impactBehaviourSummaries =
        {
            "Standard",
            "Simple ricochet",
            "Simple penetration",
            "Complex surface based"
        };

        private const string k_NameRangeLower = "Effective Range";
        private const string k_TooltipRangeLower = "The range up to which the bullet does full damage";
        private const string k_NameRangeUpper = "Ineffective Range";
        private const string k_TooltipRangeUpper = "The range over which the bullet does zero damage";
        private const string k_NameSpeedLower = "Minimum Speed";
        private const string k_TooltipSpeedLower = "The speed below which the bullet does zero damage";
        private const string k_NameSpeedUpper = "Effective Speed";
        private const string k_TooltipSpeedUpper = "The speed above which the bullet does full damage";

        public enum AmmoEffectModule
        {
            Undefined,
            Bullet,
            AdvancedBullet,
            Explosion,
            Particle
        }
        
        public enum ImpactEffectModule
        {
            None,
            PenetratingHitscan,
            PenetratingBallistic,
            RicochetHitscan,
            RicochetBallistic,
            Surface
        }

        [Serializable]
        public class AmmoEffectInfo
        {
            // Shared
            [Tooltip("The type of damage the weapon should do with this ammo.")]
            public DamageType damageType = DamageType.Default;
            [Tooltip("The damage to apply to the object that is hit (if it has a damage handler). For explosion effects, this will fall off further from the center.")]
            public float damage = 5f;
            [Tooltip("The force to apply to the rigidbody or impact handler that is hit. For explosion effects, this will fall off further from the center.")]
            public float impactForce = 20f;

            // Bullet
            [Range(0.1f, 2f), Tooltip("The force to apply to the rigidbody or impact handler that is hit. For explosion effects, this will fall off further from the center.")]
            public float bulletSize = 1;

            // Particle
            [Tooltip("The object to spawn at the impact location")]
            public ParticleImpactEffect impactEffect = null;

            // Explosion
            [RequiredObjectProperty, Tooltip("The explosion object to spawn at the impact location")]
            public PooledExplosion explosion = null;
            [Tooltip("The maximum force to be imparted onto objects in the area of effect. Requires either a Rigidbody or an impact handler.")]
            public float maxForce = 15f;
            [Range(0f, 0.1f), Tooltip("An offset from the hit point along its normal to spawn the explosion. Prevents the explosion from appearing embedded in the surface.")]
            public float normalOffset = 0f;

            // Advanced bullet
            [Tooltip("Damage is randomised withing a set min/max range")]
            public bool randomiseDamage = true;
            [Tooltip("The minimum damage the bullet does before falloff is applied.")]
            public float minDamage = 15f;
            [Tooltip("The maximum damage the bullet does before falloff is applied.")]
            public float maxDamage = 25f;
            [Tooltip("How to apply damage falloff (none, distance based or speed based).")]
            public AdvancedBulletAmmoEffect.FalloffMode falloffMode = AdvancedBulletAmmoEffect.FalloffMode.Range;
            [Tooltip("Either range at full damage, or speed at 0 damage depending on mode.")]
            public float falloffSettingLower = 100f;
            [Tooltip("Either range where the bullet does 0 damage, or speed where it does full damage depending on mode.")]
            public float falloffSettingUpper = 200f;

            public void OnValidate()
            {
                if (damage < 0f)
                    damage = 0f;
                if (bulletSize < 0.1f)
                    bulletSize = 0.1f;
                if (minDamage < 0f)
                    minDamage = 0f;
                if (maxDamage < 0f)
                    maxDamage = 0f;
                if (falloffSettingLower < 0f)
                    falloffSettingLower = 0f;
                if (falloffSettingUpper < 0.1f)
                    falloffSettingUpper = 0.1f;
                if (impactForce < 0f)
                    impactForce = 0f;
            }
        }

        public AmmoEffectModule primaryAmmoEffect
        {
            get { return (AmmoEffectModule)(m_EffectType + 1); }
        }

        public ImpactEffectModule GetImpactAmmoEffect(NeoFpsWizard wizard)
        {
            switch (m_ImpactBehaviour)
            {
                case 1:
                    {
                        var shooterStep = wizard.steps[ModularFirearmWizardSteps.shooter] as ModularFirearmShooterStep;
                        if (shooterStep.shooterStyle == 0)
                            return ImpactEffectModule.RicochetHitscan;
                        else
                            return ImpactEffectModule.RicochetBallistic;
                    }
                case 2:
                    {
                        var shooterStep = wizard.steps[ModularFirearmWizardSteps.shooter] as ModularFirearmShooterStep;
                        if (shooterStep.shooterStyle == 0)
                            return ImpactEffectModule.PenetratingHitscan;
                        else
                            return ImpactEffectModule.PenetratingBallistic;
                    }
                case 3:
                    return ImpactEffectModule.Surface;
            }
            return ImpactEffectModule.None;
        }

        public AmmoEffectModule secondaryAmmoEffect
        {
            get
            {
                if (m_ImpactBehaviour == 1 || m_ImpactBehaviour == 2)
                    return (AmmoEffectModule)(m_FollowUpEffect + 1);
                else
                    return AmmoEffectModule.Undefined;
            }
        }

        void OnValidate()
        {
            if (exitEffectSize < 0f)
                exitEffectSize = 0f;
            maxPenetration = Mathf.Clamp(maxPenetration, 0.01f, 1f);
            maxScatterAngle = Mathf.Clamp(maxScatterAngle, 0f, 45f);
            splitCount = Mathf.Clamp(splitCount, 1, 10000);
            if (effectData == null || effectData.Length != 2)
                effectData = new AmmoEffectInfo[2];
            effectData[0].OnValidate();
            effectData[1].OnValidate();
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            if (m_EffectType == -1 || effectData.Length != 2)
            {
                m_CanContinue = false;
                return;
            }

            switch (primaryAmmoEffect)
            {
                case AmmoEffectModule.Explosion:
                    m_CanContinue &= effectData[0].explosion != null;
                    break;
                case AmmoEffectModule.Particle:
                    m_CanContinue &= effectData[0].impactEffect != null;
                    break;
            }

            if (m_ImpactBehaviour == 1 || m_ImpactBehaviour == 2)
                m_CanContinue &= m_FollowUpEffect != -1;

            switch (secondaryAmmoEffect)
            {
                case AmmoEffectModule.Explosion:
                    m_CanContinue &= effectData[1].explosion != null;
                    break;
                case AmmoEffectModule.Particle:
                    m_CanContinue &= effectData[1].impactEffect != null;
                    break;
            }

            var shooterStep = wizard.steps[ModularFirearmWizardSteps.shooter] as ModularFirearmShooterStep;
            if (shooterStep.shooterStyle == 1)
                m_CanContinue &= projectilePrefab != null;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            var dataProp = serializedObject.FindProperty("effectData");

            NeoFpsEditorGUI.Header("Primary Ammo Effect");

            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("m_EffectType"), effectTypeOptions);
            m_CanContinue &= InspectAmmoEffectInfo(dataProp.GetArrayElementAtIndex(0), primaryAmmoEffect);

            NeoFpsEditorGUI.Header("Behaviour On Impact");

            NeoFpsEditorGUI.MultiChoiceField(serializedObject.FindProperty("m_ImpactBehaviour"), impactBehaviourOptions);

            switch (GetImpactAmmoEffect(wizard))
            {
                case ImpactEffectModule.PenetratingHitscan:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxPenetration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxScatterAngle"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitEffectSize"));
                        NeoFpsEditorGUI.PrefabComponentField<PooledObject>(serializedObject.FindProperty("tracerPrototype"), (obj) => { return obj.GetComponent<IPooledHitscanTrail>() != null; });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("tracerSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("tracerDuration"));

                        break;
                    }
                case ImpactEffectModule.PenetratingBallistic:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxPenetration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxScatterAngle"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitEffectSize"));
                        m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabComponentField<PooledObject>(serializedObject.FindProperty("projectilePrefab"), (obj) => { return obj.GetComponent<IProjectile>() != null; });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"));
                        
                        break;
                    }
                case ImpactEffectModule.RicochetHitscan:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("normalVsDeflection"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxScatterAngle"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("splitCount"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("speedMode"));
                        NeoFpsEditorGUI.PrefabComponentField<PooledObject>(serializedObject.FindProperty("tracerPrototype"), (obj) => { return obj.GetComponent<IPooledHitscanTrail>() != null; });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("tracerSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("tracerDuration"));

                        break;
                    }
                case ImpactEffectModule.RicochetBallistic:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("normalVsDeflection"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxScatterAngle"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("splitCount"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("speedMode"));
                        ProjectileRicochetAmmoEffectEditor.InspectSpeedSettings(serializedObject.FindProperty("minSpeedSetting"), serializedObject.FindProperty("maxSpeedSetting"), speedMode);
                        m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabComponentField<PooledObject>(serializedObject.FindProperty("projectilePrefab"), (obj) => { return obj.GetComponent<IProjectile>() != null; });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"));

                        break;
                    }
                case ImpactEffectModule.Surface:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("surfacePhysics"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRicochetScatter"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxPenetrationDeflect"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("exitEffectSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("recursive"));
                        m_CanContinue &= NeoFpsEditorGUI.RequiredPrefabComponentField<PooledObject>(serializedObject.FindProperty("projectilePrefab"), (obj) => { return obj.GetComponent<IProjectile>() != null; });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"));
                        
                        break;
                    }
            }

            if (m_ImpactBehaviour == 1 || m_ImpactBehaviour == 2)
            {
                NeoFpsEditorGUI.Header("Follow Up Effect");

                m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("m_FollowUpEffect"), effectTypeOptions);
                m_CanContinue &= InspectAmmoEffectInfo(dataProp.GetArrayElementAtIndex(1), secondaryAmmoEffect);
            }
        }

        bool InspectAmmoEffectInfo(SerializedProperty prop, AmmoEffectModule module)
        {
            var valid = true;

            switch (module)
            {
                case AmmoEffectModule.Bullet:
                    {
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("damage"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("damageType"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("impactForce"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("bulletSize"));
                    }
                    break;
                case AmmoEffectModule.AdvancedBullet:
                    {
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("damageType"));
                        var randomiseProp = prop.FindPropertyRelative("randomiseDamage");
                        EditorGUILayout.PropertyField(randomiseProp);
                        if (randomiseProp.boolValue)
                        {
                            EditorGUILayout.PropertyField(prop.FindPropertyRelative("minDamage"));
                            EditorGUILayout.PropertyField(prop.FindPropertyRelative("maxDamage"));
                        }
                        else
                            EditorGUILayout.PropertyField(prop.FindPropertyRelative("damage"));

                        var falloffMode = prop.FindPropertyRelative("falloffMode");
                        EditorGUILayout.PropertyField(falloffMode);
                        switch (falloffMode.enumValueIndex)
                        {
                            // 0 = no falloff
                            case 1: // Range based
                                {
                                    var lower = prop.FindPropertyRelative("falloffSettingLower");
                                    var upper = prop.FindPropertyRelative("falloffSettingUpper");

                                    EditorGUILayout.DelayedFloatField(lower, new GUIContent(k_NameRangeLower, k_TooltipRangeLower));
                                    if (lower.floatValue > upper.floatValue)
                                        upper.floatValue = lower.floatValue;

                                    EditorGUILayout.DelayedFloatField(upper, new GUIContent(k_NameRangeUpper, k_TooltipRangeUpper));
                                    if (lower.floatValue > upper.floatValue)
                                        lower.floatValue = upper.floatValue;
                                }
                                break;
                            case 2: // Speed based
                                {
                                    var lower = prop.FindPropertyRelative("falloffSettingLower");
                                    var upper = prop.FindPropertyRelative("falloffSettingUpper");

                                    EditorGUILayout.DelayedFloatField(upper, new GUIContent(k_NameSpeedUpper, k_TooltipSpeedUpper));
                                    if (lower.floatValue > upper.floatValue)
                                        lower.floatValue = upper.floatValue;

                                    EditorGUILayout.DelayedFloatField(lower, new GUIContent(k_NameSpeedLower, k_TooltipSpeedLower));
                                    if (lower.floatValue > upper.floatValue)
                                        upper.floatValue = lower.floatValue;
                                }
                                break;
                        }

                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("impactForce"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("bulletSize"));
                    }
                    break;
                case AmmoEffectModule.Explosion:
                    {
                        valid &= NeoFpsEditorGUI.RequiredObjectField(prop.FindPropertyRelative("explosion"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("damageType"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("damage"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("maxForce"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("normalOffset"));
                    }
                    break;
                case AmmoEffectModule.Particle:
                    {
                        valid &= NeoFpsEditorGUI.RequiredObjectField(prop.FindPropertyRelative("impactEffect"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("damageType"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("damage"));
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative("impactForce"));
                    }
                    break;
            }

            return valid;
        }

        void SummariseAmmoEffectInfo(AmmoEffectInfo info, AmmoEffectModule module)
        {
            switch (module)
            {
                case AmmoEffectModule.Bullet:
                    {
                        WizardGUI.DoSummary("damage", info.damage);
                        WizardGUI.DoSummary("impactForce", info.impactForce);
                        WizardGUI.DoSummary("bulletSize", info.bulletSize);
                    }
                    break;
                case AmmoEffectModule.AdvancedBullet:
                    {
                        WizardGUI.DoSummary("randomiseDamage", info.randomiseDamage);
                        if (info.randomiseDamage)
                        {
                            WizardGUI.DoSummary("minDamage", info.minDamage);
                            WizardGUI.DoSummary("maxDamage", info.maxDamage);
                        }
                        else
                            WizardGUI.DoSummary("damage", info.damage);

                        WizardGUI.DoSummary("falloffMode", info.falloffMode.ToString());
                        switch (info.falloffMode)
                        {
                            // 0 = no falloff
                            case AdvancedBulletAmmoEffect.FalloffMode.Range: // Range based
                                {
                                    WizardGUI.DoSummary(k_NameRangeLower, info.falloffSettingLower);
                                    WizardGUI.DoSummary(k_NameRangeUpper, info.falloffSettingUpper);
                                }
                                break;
                            case AdvancedBulletAmmoEffect.FalloffMode.Speed: // Speed based
                                {
                                    WizardGUI.DoSummary(k_NameSpeedUpper, info.falloffSettingUpper);
                                    WizardGUI.DoSummary(k_NameSpeedLower, info.falloffSettingLower);
                                }
                                break;
                        }

                        WizardGUI.DoSummary("impactForce", info.impactForce);
                        WizardGUI.DoSummary("bulletSize", info.bulletSize);
                    }
                    break;
                case AmmoEffectModule.Explosion:
                    {
                        WizardGUI.ObjectSummary("explosion", info.explosion);
                        WizardGUI.DoSummary("damage", info.damage);
                        WizardGUI.DoSummary("maxForce", info.maxForce);
                        WizardGUI.DoSummary("normalOffset", info.normalOffset);
                    }
                    break;
                case AmmoEffectModule.Particle:
                    {
                        WizardGUI.ObjectSummary("impactEffect", info.impactEffect);
                        WizardGUI.DoSummary("damage", info.damage);
                        WizardGUI.DoSummary("impactForce", info.impactForce);
                    }
                    break;
            }
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.MultiChoiceSummary("m_EffectType", m_EffectType, effectTypeSummaries);
            WizardGUI.MultiChoiceSummary("m_ImpactBehaviour", m_ImpactBehaviour, impactBehaviourSummaries);
            if (m_ImpactBehaviour == 1 || m_ImpactBehaviour == 2)
                WizardGUI.MultiChoiceSummary("m_FollowUpEffect", m_FollowUpEffect, effectTypeSummaries);
        }
    }
}
