using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace NeoFPS
{
    [CreateAssetMenu(fileName = "FpsManager_Audio", menuName = "NeoFPS/Managers/Audio Manager", order = NeoFpsMenuPriorities.manager_audio)]
    [HelpURL("https://docs.neofps.com/manual/audioref-so-neofpsaudiomanager.html")]
    public class NeoFpsAudioManager : NeoFpsManager<NeoFpsAudioManager>
    {
        private RuntimeBehaviour m_ProxyBehaviour = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadAudioManager()
        {
            GetInstance("FpsManager_Audio");
            //s_ProxyBehaviour.InitialisePooledAudio();
        }

        protected override void Initialise()
        {
            m_ProxyBehaviour = GetBehaviourProxy<RuntimeBehaviour>();
        }

        [Header ("Mixer")]

        [SerializeField, RequiredObjectProperty, Tooltip("The audio mixer for the project.")]
        private AudioMixer m_Mixer = null;

        [Header("Mixer Groups")]

        [SerializeField, RequiredObjectProperty, Tooltip("The audio mixer group that controls the overall volume.")]
        private AudioMixerGroup m_MasterGroup = null;

        [SerializeField, RequiredObjectProperty, Tooltip("The audio mixer group used to control volumes and filters for spatial sound effects.")]
        private AudioMixerGroup m_SpatialEffectsGroup = null;

        [SerializeField, RequiredObjectProperty, Tooltip("The audio mixer group used to control volumes and filters for UI sound effects.")]
        private AudioMixerGroup m_UiEffectsGroup = null;

        [SerializeField, RequiredObjectProperty, Tooltip("The audio mixer group used to control volumes and filters for ambient sound effects and looping ambient audio.")]
        private AudioMixerGroup m_AmbienceGroup = null;

        [SerializeField, RequiredObjectProperty, Tooltip("The audio mixer group used to control volumes and filters for music.")]
        private AudioMixerGroup m_MusicGroup = null;

        [Header ("Mixer Parameters")]

        [SerializeField, Tooltip("The name of the master volume parameter on the audio mixer.")]
        private string m_MasterVolumeKey = "MasterVolume";

        [SerializeField, Tooltip("The name of the volume parameter that controls sound effects volume.")]
        private string m_EffectsVolumeKey = "EffectsVolume";

        [SerializeField, Tooltip("The name of the volume parameter on the audio mixer for ambient loops and effects.")]
        private string m_AmbienceVolumeKey = "AmbienceVolume";

        [SerializeField, Tooltip("The name of the volume parameter on the audio mixer for the music audio.")]
        private string m_MusicVolumeKey = "MusicVolume";

        [Header("Pooled Sources")]

        [SerializeField, Tooltip("The number of pooled audio sources for spatial sound effects.")]
        private int m_NumSpatialEffectSources = 40;

        [SerializeField, NeoPrefabField(required = true), Tooltip("An optional prefab for the spatial effects sources. If not provided then the objects will be created from scratch.")]
        private AudioSource m_SpatialSourcePrefab = null;

        [SerializeField, Tooltip("The number of pooled audio sources for ambient sound effects.")]
        private int m_NumAmbienceOneShotSources = 5;

        [SerializeField, NeoPrefabField(required = true), Tooltip("An optional prefab for the ambience effects sources. If not provided then the objects will be created from scratch.")]
        private AudioSource m_AmbienceSourcePrefab = null;

        private static int s_CurrentSpatialEffectSourceIndex = -1;
        private static int s_CurrentAmbienceOneShotSourceIndex = -1;

        public static AudioMixerGroup masterGroup
        {
            get { return instance.m_MasterGroup; }
        }

        public static AudioMixerGroup spatialEffectsGroup
        {
            get { return instance.m_SpatialEffectsGroup; }
        }

        public static AudioMixerGroup uiEffectsGroup
        {
            get { return instance.m_UiEffectsGroup; }
        }

        public static AudioMixerGroup ambienceGroup
        {
            get { return instance.m_AmbienceGroup; }
        }

        public static AudioMixerGroup musicGroup
        {
            get { return instance.m_MusicGroup; }
        }

        void SetMasterVolume(float v)
        {
            if (v < 0.001)
                m_Mixer.SetFloat(m_MasterVolumeKey, -80f);
            else
                m_Mixer.SetFloat(m_MasterVolumeKey, Mathf.Log10(v) * 20f);
        }

        void SetEffectsVolume(float v)
        {
            if (v < 0.001)
                m_Mixer.SetFloat(m_EffectsVolumeKey, -80f);
            else
                m_Mixer.SetFloat(m_EffectsVolumeKey, Mathf.Log10(v) * 20f);
        }

        void SetAmbienceVolume(float v)
        {
            if (v < 0.001)
                m_Mixer.SetFloat(m_AmbienceVolumeKey, -80f);
            else
                m_Mixer.SetFloat(m_AmbienceVolumeKey, Mathf.Log10(v) * 20f);
        }

        void SetMusicVolume(float v)
        {
            if (v < 0.001)
                m_Mixer.SetFloat(m_MusicVolumeKey, -80f);
            else
                m_Mixer.SetFloat(m_MusicVolumeKey, Mathf.Log10(v) * 20f);
        }

        void OnValidate()
        {
            if (m_NumSpatialEffectSources < 1)
                m_NumSpatialEffectSources = 1;
            if (m_NumAmbienceOneShotSources < 0)
                m_NumAmbienceOneShotSources = 0;
        }

        public override bool IsValid()
        {
            return m_Mixer != null &&
                m_MasterGroup != null &&
                m_SpatialEffectsGroup != null &&
                m_AmbienceGroup != null &&
                m_MusicGroup != null &&
                !string.IsNullOrEmpty(m_MasterVolumeKey) &&
                !string.IsNullOrEmpty(m_EffectsVolumeKey) &&
                !string.IsNullOrEmpty(m_AmbienceVolumeKey) &&
                !string.IsNullOrEmpty(m_MusicVolumeKey);
        }

        class RuntimeBehaviour : MonoBehaviour
        {
            IEnumerator Start()
            {
                yield return new WaitForSecondsRealtime(0.1f);

                // Set volumes
                instance.SetMasterVolume(FpsSettings.audio.masterVolume);
                instance.SetEffectsVolume(FpsSettings.audio.effectsVolume);
                instance.SetAmbienceVolume(FpsSettings.audio.ambienceVolume);
                instance.SetMusicVolume(FpsSettings.audio.musicVolume);

                // Attach listeners
                FpsSettings.audio.onMasterVolumeChanged += instance.SetMasterVolume;
                FpsSettings.audio.onEffectsVolumeChanged += instance.SetEffectsVolume;
                FpsSettings.audio.onAmbienceVolumeChanged += instance.SetAmbienceVolume;
                FpsSettings.audio.onMusicVolumeChanged += instance.SetMusicVolume;
            }

            void Awake()
            {
                // Create audio sources root
                var root = new GameObject("AudioManagerSources").transform;
                root.SetParent(transform);

                // Create spatial effects source
                if (instance.m_NumSpatialEffectSources > 0)
                {
                    // Allocate list
                    int count = instance.m_NumSpatialEffectSources;
                    spatialEffectsSources = new List<AudioSource>(instance.m_NumSpatialEffectSources);

                    AudioSource prototype = instance.m_SpatialSourcePrefab;
                    if (prototype == null)
                    {
                        // Create the prototype
                        var sourceGO = new GameObject("SpatialEffectsSource (1)");
                        sourceGO.transform.SetParent(root);
                        prototype = sourceGO.AddComponent<AudioSource>();
                        prototype.outputAudioMixerGroup = spatialEffectsGroup;
                        prototype.playOnAwake = false;
                        prototype.bypassListenerEffects = false;
                        prototype.bypassReverbZones = false;
                        prototype.bypassEffects = false;
                        prototype.loop = false;
                        prototype.spatialBlend = 1f;
                        spatialEffectsSources.Add(prototype);
                        --count;
                    }

                    // Duplicate
                    for (int i = 0; i < count; ++i)
                    {
                        var duplicate = Instantiate(prototype);
                        duplicate.transform.SetParent(root);
                        spatialEffectsSources.Add(duplicate);
                    }
                }

                // Create Ambience one-shot sources
                if (instance.m_NumAmbienceOneShotSources > 0)
                {
                    // Allocate list
                    int count = instance.m_NumAmbienceOneShotSources;
                    ambienceSources = new List<AudioSource>(instance.m_NumAmbienceOneShotSources);

                    AudioSource prototype = instance.m_AmbienceSourcePrefab;
                    if (prototype == null)
                    {
                        var sourceGO = new GameObject("AmbienceOneShotSource (1)");
                        sourceGO.transform.SetParent(root);
                        prototype = sourceGO.AddComponent<AudioSource>();
                        prototype.outputAudioMixerGroup = ambienceGroup;
                        prototype.playOnAwake = false;
                        prototype.bypassListenerEffects = false;
                        prototype.bypassReverbZones = false;
                        prototype.bypassEffects = false;
                        prototype.loop = false;
                        prototype.spatialBlend = 1f;
                        ambienceSources.Add(prototype);
                        --count;
                    }

                    // Duplicate
                    for (int i = 0; i < count; ++i)
                    {
                        var duplicate = Instantiate(prototype);
                        duplicate.transform.SetParent(root);
                        ambienceSources.Add(duplicate);
                    }
                }
            }

            public List<AudioSource> spatialEffectsSources
            {
                get;
                private set;
            }

            public List<AudioSource> ambienceSources
            {
                get;
                private set;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_ProxyBehaviour != null)
            {
                Destroy(m_ProxyBehaviour);
                m_ProxyBehaviour = null;
            }
        }

        public static void PlayEffectAudioAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            // Check sources exist
            var sourceList = instance.m_ProxyBehaviour.spatialEffectsSources;
            if (sourceList == null)
                return;

            // Get the index (looping)
            if (++s_CurrentSpatialEffectSourceIndex == sourceList.Count)
                s_CurrentSpatialEffectSourceIndex = 0;

            // Position and play
            var source = sourceList[s_CurrentSpatialEffectSourceIndex];
            source.transform.position = position;
            source.PlayOneShot(clip, volume);
        }

        public static void PlayAmbienceAudioAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
        {
            // Check sources exist
            var sourceList = instance.m_ProxyBehaviour.ambienceSources;
            if (sourceList == null)
                return;

            // Get the index (looping)
            if (++s_CurrentAmbienceOneShotSourceIndex == sourceList.Count)
                s_CurrentAmbienceOneShotSourceIndex = 0;

            // Position and play
            var source = sourceList[s_CurrentAmbienceOneShotSourceIndex];
            source.transform.position = position;
            source.PlayOneShot(clip, volume);
        }
    }
}
