using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPSEditor;
using NeoFPSEditor.Hub;
using UnityEngine;
using UnityEditor;

namespace NeoFPSEditor.Hub.Pages.ItemCreationWizards.ModularFirearms
{
    public class ModularFirearmTriggerStep : NeoFpsWizardStep
    {
        [SerializeField, Tooltip("The trigger style of the firearm.")]
        private int m_TriggerType = -1;

        // Burst
        [Tooltip("Cooldown between trigger pulls (number of fixed update frames).")]
        public int cooldown = 0;
        [Tooltip("How many fixed update frames before firing again (0 = requires fresh trigger press).")]
        public int repeatDelay = 0;

        // Burst
        [Tooltip("How many fixed update frames between shots.")]
        public int shotSpacing = 5;

        // Burst
        [Tooltip("The number of shots in a burst.")]
        public int burstSize = 3;
        [Tooltip("Cooldown between shots in a burst (number of fixed update frames).")]
        public int burstSpacing = 5;
        [Tooltip("The minimum spacing from ending a burst to starting a new one. Prevents rapid clicking firing at a higher fire rate than the burst itself.")]
        public int minRepeatDelay = 8;
        [Tooltip("Does holding the trigger fire a new burst after a set duration.")]
        public bool repeatOnTriggerHold = false;
        [Tooltip("How many fixed update frames from last shot of a burst before starting another.")]
        public int holdRepeatDelay = 50;
        [Tooltip("Is the burst cancelled when the trigger is released?")]
        public bool cancelOnRelease = false;

        // Charged
        [Tooltip("How long does it take to charge the trigger.")]
        public float chargeDuration = 0.5f;
        [Tooltip("How long does it take to uncharge the trigger, assuming it hasn't gone off.")]
        public float unchargeDuration = 0.5f;
        [Tooltip("Once the shot is fired, start charging the next shot.")]
        public bool canRepeat = false;
        [Tooltip("The audio clip to play on charge.")]
        public AudioClip triggerAudioCharge = null;
        [Tooltip("The audio clip to play on release.")]
        public AudioClip triggerAudioRelease = null;

        // Queue
        [Tooltip("The time between adding new shots to the queue.")]
        public int queueSpacing = 25;
        [Tooltip("Should the enqueued shot be instant, or delayed by the queue spacing.")]
        public bool delayFirst = false;
        [Tooltip("The maximum number of shots that can be enqueued.")]
        public int maxQueueSize = 6;

        // Target Lock
        [Tooltip("The object tag to home in on.")]
        public string detectionTag = "AI";
        [Tooltip("The layers to check for colliders on.")]
        public LayerMask detectionLayers = PhysicsFilter.Masks.Characters;
        [Tooltip("The time it takes to achieve a full lock and be able to fire.")]
        public float lockOnTime = 2f;
        [Tooltip("The angle of the cone within which objects can be detected.")]
        public float detectionConeAngle = 10f;
        [Tooltip("The maximum distance that objects can be detected from the wielder.")]
        public float detectionRange = 200f;
        [Tooltip("Does a target lock require an unobstructed line of sight to the target.")]
        public bool requireLineOfSight = true;
        [Tooltip("The layers that can obstruct line of sight to the target.")]
        public LayerMask blockingLayers = PhysicsFilter.Masks.BulletBlockers;
        [Tooltip("How long after the trigger fires does the trigger remember the target. This can be useful for shooters that shoot multiple projectiles.")]
        public float memory = 0.1f;

        // Multi-Target Lock
        [Tooltip("The time between queueing target locks.")]
        public int lockSpacing = 50;
        [Tooltip("The time between failing to find a target and trying again.")]
        public int lockRetrySpacing = 25;

        private bool m_CanContinue = false;

        public override string displayName
        {
            get { return "Trigger Setup"; }
        }

        static readonly string[] triggerTypeOptions =
        {
            "Semi-Auto. Similar to most pistols, pulling the trigger fires one round. You must release the trigger and then pull again to fire another. This can also be set up to repeat slowly.",
            "Automatic. As used in machine-guns, the shot resets the trigger once a new round has been inserted and it will fire again if the trigger is held.",
            "Burst. Fires a set number of bullets in sequence and then the trigger must be released and pulled again to fire the next burst. Can be set to stop firing if the trigger is released before the burst completes, or to keep firing regardless.",
            "Charged. Holding the trigger builds a charge. Once the charge reaches the maximum, the gun fires. Releasing the trigger before the charge hits maximum, and it will wind back down to zero.",
            "Queued. Holding the trigger queues up multiple shots. Releasing the trigger fires them in a burst. Reload to cancel.",
            "Target Lock. Holding the trigger will lock onto a target. Releasing will fire. Combine this with a ballistic shooter and guided projectile.",
            "Multi-Target Lock. Holding the trigger will queue up multiple target locks. Releasing will fire at each one. Combine this with a ballistic shooter and guided projectile."
        };

        static readonly string[] triggerTypeSummaries =
        {
            "Semi-Auto",
            "Automatic",
            "Burst",
            "Charged",
            "Queued",
            "Target Lock",
            "Multi-Target Lock"
        };

        public enum TriggerModule
        {
            Undefined,
            SemiAuto,
            Automatic,
            Burst,
            Charged,
            Queued,
            TargetLock,
            MultiTargetLock
        }

        public TriggerModule triggerModule
        {
            get { return (TriggerModule)(m_TriggerType + 1); }
        }

        public override void CheckStartingState(NeoFpsWizard wizard)
        {
            m_CanContinue = m_TriggerType != -1;
        }

        public override bool CheckCanContinue(NeoFpsWizard root)
        {
            return m_CanContinue;
        }

        void OnValidate()
        {
            if (shotSpacing < 1)
                shotSpacing = 1;
            if (burstSize < 2)
                burstSize = 2;
            if (burstSpacing < 1)
                burstSpacing = 1;
            if (minRepeatDelay < burstSpacing)
                minRepeatDelay = burstSpacing;
            if (holdRepeatDelay < minRepeatDelay)
                holdRepeatDelay = minRepeatDelay;
            if (cooldown < 0)
                cooldown = 0;
            if (repeatDelay < 0)
                repeatDelay = 0;
            if (chargeDuration < 0.1f)
                chargeDuration = 0.1f;
            if (unchargeDuration < 0f)
                unchargeDuration = 0f;
        }

        protected override void OnInspectorGUI(SerializedObject serializedObject, NeoFpsWizard wizard)
        {
            m_CanContinue = true;

            m_CanContinue &= NeoFpsEditorGUI.RequiredMultiChoiceField(serializedObject.FindProperty("m_TriggerType"), triggerTypeOptions);

            switch (triggerModule)
            {
                case TriggerModule.SemiAuto:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldown"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("repeatDelay"));
                    }
                    break;
                case TriggerModule.Automatic:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shotSpacing"));
                        ShowRateOfFire(shotSpacing);
                    }
                    break;
                case TriggerModule.Burst:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("burstSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("burstSpacing"));
                        ShowRateOfFire(burstSpacing);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("minRepeatDelay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("repeatOnTriggerHold"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("holdRepeatDelay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("cancelOnRelease"));
                    }
                    break;
                case TriggerModule.Charged:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("chargeDuration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("unchargeDuration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("canRepeat"));
                        if (canRepeat)
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("repeatDelay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerAudioCharge"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerAudioRelease"));
                    }
                    break;
                case TriggerModule.Queued:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("queueSpacing"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("delayFirst"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxQueueSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("burstSpacing"));
                        ShowRateOfFire(burstSpacing);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("minRepeatDelay"));
                    }
                    break;
                case TriggerModule.TargetLock:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionTag"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionLayers"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("lockOnTime"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionConeAngle"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionRange"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("requireLineOfSight"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("blockingLayers"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("memory"));
                    }
                    break;
                case TriggerModule.MultiTargetLock:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionTag"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionLayers"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionConeAngle"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("detectionRange"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("lockSpacing"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("lockRetrySpacing"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("requireLineOfSight"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("blockingLayers"));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("delayFirst"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxQueueSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("burstSpacing"));
                        ShowRateOfFire(burstSpacing);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("minRepeatDelay"));
                    }
                    break;
            }
        }

        void ShowRateOfFire(int spacing)
        {
            // Show rate of fire from spacing
            float ticksPerSecond = 1f / Time.fixedDeltaTime;
            float floatSpacing = spacing;
            EditorGUILayout.HelpBox(
                string.Format(
                    "A shot spacing of {0} gives rate of fire:\n{1} rounds per second, {2} rounds per minute",
                    spacing,
                    ticksPerSecond / floatSpacing,
                    60f * ticksPerSecond / floatSpacing
                ),
                MessageType.Info
                );
        }

        protected override void OnSummaryGUI(NeoFpsWizard wizard)
        {
            WizardGUI.MultiChoiceSummary("m_TriggerType", m_TriggerType, triggerTypeSummaries);

            switch (triggerModule)
            {
                case TriggerModule.SemiAuto:
                    {
                        WizardGUI.DoSummary("cooldown", cooldown);
                        WizardGUI.DoSummary("repeatDelay", repeatDelay);
                    }
                    break;
                case TriggerModule.Automatic:
                    {
                        WizardGUI.DoSummary("shotSpacing", shotSpacing);
                    }
                    break;
                case TriggerModule.Burst:
                    {
                        WizardGUI.DoSummary("burstSize", burstSize);
                        WizardGUI.DoSummary("burstSpacing", burstSpacing);
                        WizardGUI.DoSummary("minRepeatDelay", minRepeatDelay);
                        WizardGUI.DoSummary("repeatOnTriggerHold", repeatOnTriggerHold);
                        WizardGUI.DoSummary("holdRepeatDelay", holdRepeatDelay);
                        WizardGUI.DoSummary("cancelOnRelease", cancelOnRelease);
                    }
                    break;
                case TriggerModule.Charged:
                    {
                        WizardGUI.DoSummary("chargeDuration", chargeDuration);
                        WizardGUI.DoSummary("unchargeDuration", unchargeDuration);
                        WizardGUI.DoSummary("canRepeat", canRepeat);
                        if (canRepeat)
                            WizardGUI.DoSummary("repeatDelay", repeatDelay);
                        WizardGUI.ObjectSummary("triggerAudioCharge", triggerAudioCharge);
                        WizardGUI.ObjectSummary("triggerAudioRelease", triggerAudioRelease);
                    }
                    break;
                case TriggerModule.Queued:
                    {
                        WizardGUI.DoSummary("queueSpacing", queueSpacing);
                        WizardGUI.DoSummary("delayFirst", delayFirst);
                        WizardGUI.DoSummary("maxQueueSize", maxQueueSize);
                        WizardGUI.DoSummary("burstSpacing", burstSpacing);
                        WizardGUI.DoSummary("minRepeatDelay", minRepeatDelay);
                    }
                    break;
                case TriggerModule.TargetLock:
                    {
                        WizardGUI.DoSummary("detectionTag", detectionTag);
                        WizardGUI.DoSummary("detectionLayers", detectionLayers);
                        WizardGUI.DoSummary("lockOnTime", lockOnTime);
                        WizardGUI.DoSummary("detectionConeAngle", detectionConeAngle);
                        WizardGUI.DoSummary("detectionRange", detectionRange);
                        WizardGUI.DoSummary("requireLineOfSight", requireLineOfSight);
                        WizardGUI.DoSummary("blockingLayers", blockingLayers);
                        WizardGUI.DoSummary("memory", memory);
                    }
                    break;
                case TriggerModule.MultiTargetLock:
                    {
                        WizardGUI.DoSummary("detectionTag", detectionTag);
                        WizardGUI.DoSummary("detectionLayers", detectionLayers);
                        WizardGUI.DoSummary("detectionConeAngle", detectionConeAngle);
                        WizardGUI.DoSummary("detectionRange", detectionRange);
                        WizardGUI.DoSummary("lockSpacing", lockSpacing);
                        WizardGUI.DoSummary("lockRetrySpacing", lockRetrySpacing);
                        WizardGUI.DoSummary("requireLineOfSight", requireLineOfSight);
                        WizardGUI.DoSummary("blockingLayers", blockingLayers);
                        WizardGUI.DoSummary("delayFirst", delayFirst);
                        WizardGUI.DoSummary("maxQueueSize", maxQueueSize);
                        WizardGUI.DoSummary("burstSpacing", burstSpacing);
                        WizardGUI.DoSummary("minRepeatDelay", minRepeatDelay);
                    }
                    break;
            }
        }
    }
}
