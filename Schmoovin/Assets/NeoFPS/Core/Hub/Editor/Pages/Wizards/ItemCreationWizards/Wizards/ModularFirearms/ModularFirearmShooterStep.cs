using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor;
using NeoFPSEditor.Hub;
using UnityEngine;
using UnityEditor;
using NeoFPSEditor.ModularFirearms;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ModularFirearms
{
    public class ModularFirearmShooterStep : NeoFpsWizardStep
    {
        [Tooltip("How gunshots are implemented.")]
        public int shooterStyle = -1;
        [Tooltip("The grouping (pattern) of the shots.")]
        public int shotGrouping = -1;

        [Tooltip("The transform that the bullet actually fires from (Z-Forwards).")]
        public GameObject muzzleTip = null;
        [Tooltip("The minimum angle from forward (in degrees) of the shot (at full accuracy).")]
        public float minAccuracySpread = 0.5f;
        [Tooltip("The miaximum angle from forward (in degrees) of the shot (at zero accuracy).")]
        public float maxAccuracySpread = 10f;

        [Tooltip("How many pellets are fired each shot.")]
        public int bulletCount = 20;
        [Tooltip("The spread of the cone in degrees.")]
        public float cone = 5f;
        [Tooltip("How many pellets are required per tracer line.")]
        public int shotsPerTracer = 2;

        [Tooltip("The optional pooled tracer prototype to use (must implement the IPooledHitscanTrail interface).")]
        public PooledObject pooledTracer = null;
        [Tooltip("How size (thickness/radius) of the tracer line.")]
        public float tracerSize = 0.025f;
        [Tooltip("How long the tracer line will last.")]
        public float tracerDuration = 0.025f;
        
        [Tooltip("The projectile to spawn.")]
        public PooledObject projectilePrefab = null;        
        [Tooltip("The speed of the projectile.")]
        public float muzzleSpeed = 100f;        
        [Tooltip("The gravity for the projectile.")]
        public float gravity = 9.8f;

        [Tooltip("The different points of the pattern as looking straight at the target. Clamped to the -1m to 1m range on both axes.")]
        public Vector2[] patternPoints = {
            new Vector2(0f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(-0.5f, 0f),
            new Vector2(0f, 0.5f),
            new Vector2(0f, -0.5f)
        };
        [Tooltip("The distance from the muzzle tip at which the pattern will match the diagram.")]
        public float patternDistance = 50f;

        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "Shooter Setup"; }
        }

        static readonly string[] shooterStyleOptions =
        {
            "Hit-scan hit detection. Uses raycasts when fired, instantly hitting the target.",
            "Projectile based. Spawns projectiles, which move until they hit something."
        };

        static readonly string[] shooterStyleSummaries =
        {
            "Hit-scan",
            "Projectile"
        };

        static readonly string[] groupingOptions =
        {
            "Single shot. Fires a single bullet each shot.",
            "Random spread. Fires multiple bullets in a cone with each shot.",
            "Fixed Pattern. Fires multiple bullets in a set pattern that spread with distance from the source."
        };

        static readonly string[] groupingSummaries =
        {
            "Single shot",
            "Random spread",
            "Fixed Pattern"
        };

        public enum ShooterModule
        {
            Undefined,
            Hitscan,
            Ballistic,
            SpreadHitscan,
            SpreadBallistic,
            PatternHitscan,
            PatternBallistic
        }

        public ShooterModule shooterModule
        {
            get
            {
                if (shooterStyle == -1 || shotGrouping == -1)
                    return ShooterModule.Undefined;

                switch (shotGrouping)
                {
                    case 0:
                        if (shooterStyle == 0)
                            return ShooterModule.Hitscan;
                        else
                            return ShooterModule.Ballistic;
                    case 1:
                        if (shooterStyle == 0)
                            return ShooterModule.SpreadHitscan;
                        else
                            return ShooterModule.SpreadBallistic;
                    case 2:
                        if (shooterStyle == 0)
                            return ShooterModule.PatternHitscan;
                        else
                            return ShooterModule.PatternBallistic;
                }

                return ShooterModule.Undefined;
            }
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = shooterStyle != -1 && shotGrouping != -1;
            if (shooterStyle == 1)
                m_CanContinue &= projectilePrefab;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            if (tracerDuration < 0f)
                tracerDuration = 0f;
            tracerSize = Mathf.Clamp(tracerSize, 0.001f, 0.25f);
            minAccuracySpread = Mathf.Clamp(minAccuracySpread, 0f, 45f);
            maxAccuracySpread = Mathf.Clamp(maxAccuracySpread, 0f, 45f);

            if (muzzleSpeed < 1f)
                muzzleSpeed = 1f;
            if (gravity < 0f)
                gravity = 0f;

            if (patternDistance < 1)
                patternDistance = 1;
            for (int i = 0; i < patternPoints.Length; ++i)
            {
                patternPoints[i].x = Mathf.Clamp(patternPoints[i].x, -1f, 1f);
                patternPoints[i].y = Mathf.Clamp(patternPoints[i].y, -1f, 1f);
            }

            if (bulletCount < 2)
                bulletCount = 2;
            if (shotsPerTracer < 0)
                shotsPerTracer = 0;
            cone = Mathf.Clamp(cone, 0.1f, 90f);
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;
            var root = wizard.steps[ModularFirearmWizardSteps.root] as ModularFirearmRootStep;

            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("shooterStyle"), shooterStyleOptions);
            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("shotGrouping"), groupingOptions);

            // Muzzle tip
            NeoFpsEditorGUI.GameObjectInHierarchyField(serializedObject.FindProperty("muzzleTip"), root.weaponObject.transform, false);
            if (muzzleTip == null)
                NeoFpsEditorGUI.MiniInfo("An object will be automatically created under the view model's weapon object.");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("minAccuracySpread"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAccuracySpread"));
            
            switch (shooterModule)
            {
                case ShooterModule.Hitscan:
                    {
                        DrawHitscanTrailProps(serializedObject);
                    }
                    break;
                case ShooterModule.Ballistic:
                    {
                        m_CanContinue &= DrawProjectileProps(serializedObject);
                    }
                    break;
                case ShooterModule.SpreadHitscan:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletCount"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("cone"));
                        DrawHitscanTrailProps(serializedObject);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shotsPerTracer"));
                    }
                    break;
                case ShooterModule.SpreadBallistic:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletCount"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("cone"));
                        m_CanContinue &= DrawProjectileProps(serializedObject);
                    }
                    break;
                case ShooterModule.PatternHitscan:
                    {
                        PatternShooterEditorBase.DrawPatternProperty(this, serializedObject.FindProperty("patternPoints"), serializedObject.FindProperty("patternDistance"));
                        DrawHitscanTrailProps(serializedObject);
                    }
                    break;
                case ShooterModule.PatternBallistic:
                    {
                        PatternShooterEditorBase.DrawPatternProperty(this, serializedObject.FindProperty("patternPoints"), serializedObject.FindProperty("patternDistance"));
                        m_CanContinue &= DrawProjectileProps(serializedObject);
                    }
                    break;
            }
        }

        void DrawHitscanTrailProps(SerializedObject serializedObject)
        {
            NeoFpsEditorGUI.PrefabComponentField<PooledObject>(serializedObject.FindProperty("pooledTracer"), (obj) => { return obj.GetComponent<IPooledHitscanTrail>() != null; });
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tracerSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tracerDuration"));
        }

        bool DrawProjectileProps(SerializedObject serializedObject)
        {
            bool result = NeoFpsEditorGUI.RequiredPrefabComponentField<PooledObject>(serializedObject.FindProperty("projectilePrefab"), (obj) => { return obj.GetComponent<IProjectile>() != null; });
            EditorGUILayout.PropertyField(serializedObject.FindProperty("muzzleSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"));
            return result;
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.MultiChoiceSummary("shooterStyle", shooterStyle, shooterStyleSummaries);
            WizardGUI.MultiChoiceSummary("shotGrouping", shotGrouping, groupingSummaries);

            // Muzzle tip
            WizardGUI.ObjectSummary("muzzleTip", muzzleTip);
            WizardGUI.DoSummary("minAccuracySpread", minAccuracySpread);
            WizardGUI.DoSummary("maxAccuracySpread", maxAccuracySpread);

            switch (shooterModule)
            {
                case ShooterModule.Hitscan:
                    {
                        SummariseHitscanTrailProps();
                    }
                    break;
                case ShooterModule.Ballistic:
                    {
                        SummariseProjectileProps();
                    }
                    break;
                case ShooterModule.SpreadHitscan:
                    {
                        WizardGUI.DoSummary("bulletCount", bulletCount);
                        WizardGUI.DoSummary("cone", cone);
                        SummariseHitscanTrailProps();
                        WizardGUI.DoSummary("shotsPerTracer", shotsPerTracer);
                    }
                    break;
                case ShooterModule.SpreadBallistic:
                    {
                        WizardGUI.DoSummary("bulletCount", bulletCount);
                        WizardGUI.DoSummary("cone", cone);
                        SummariseProjectileProps();
                    }
                    break;
                case ShooterModule.PatternHitscan:
                    {
                        WizardGUI.DoSummary("patternDistance", patternDistance);
                        SummariseHitscanTrailProps();
                    }
                    break;
                case ShooterModule.PatternBallistic:
                    {
                        WizardGUI.DoSummary("patternDistance", patternDistance);
                        SummariseProjectileProps();
                    }
                    break;
            }
        }

        void SummariseHitscanTrailProps()
        {
            WizardGUI.ObjectSummary("pooledTracer", pooledTracer);
            WizardGUI.DoSummary("tracerSize", tracerSize);
            WizardGUI.DoSummary("tracerDuration", tracerDuration);
        }

        void SummariseProjectileProps()
        {
            WizardGUI.ObjectSummary("projectilePrefab", projectilePrefab);
            WizardGUI.DoSummary("muzzleSpeed", muzzleSpeed);
            WizardGUI.DoSummary("gravity", gravity);
        }
    }
}
