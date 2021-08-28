using NeoFPS.Constants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeoFPS
{
    [CreateAssetMenu(fileName = "FpsManager_Surfaces", menuName = "NeoFPS/Managers/Surface Manager", order = NeoFpsMenuPriorities.manager_surfaces)]
    [HelpURL("https://docs.neofps.com/manual/surfacesref-so-poolmanager.html")]
    public class SurfaceManager : NeoFpsManager<SurfaceManager>
    {
        private static RuntimeBehaviour s_ProxyBehaviour = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadSurfaceManager()
        {
            GetInstance("FpsManager_Surfaces");
        }

        protected override void Initialise()
        {
            s_ProxyBehaviour = GetBehaviourProxy<RuntimeBehaviour>();
        }

        [Header("VFX")]

        [SerializeField, RequiredObjectProperty, Tooltip("The impact special effects for things like bullet hits.")]
        private SurfaceHitFxData m_ImpactEffects = null;

        [SerializeField, Tooltip("Which physics layers will show decals.")]
        private LayerMask m_DecalLayers = PhysicsFilter.LayerFilter.EnvironmentDetail;

        [Header("Audio")]

        [SerializeField, RequiredObjectProperty, Tooltip("The audio library for impact audio, eg. bullet hits.")]
        private SurfaceAudioData m_ImpactAudio = null;

        [SerializeField, Tooltip("Should impact audio be louder for heavier hits.")]
        private bool m_ScaleVolumeToHitSize = false;

        [Tooltip("The maximum distance from the player character to play impact audio.")]
        [SerializeField] private float m_MaxImpactAudioDistance = 20f;

        private static Camera s_CurrentMainCamera = null;
        private float m_MaxAudioDistanceSqrd = 0f;

        public static SurfaceFxOverrides currentOverrides
        {
            get;
            private set;
        }

        public static LayerMask decalLayers
        {
            get
            {
                if (instance != null)
                    return instance.m_DecalLayers;
                else
                    return PhysicsFilter.LayerFilter.EnvironmentDetail;
            }
        }

        void OnValidate()
        {
            if (m_MaxImpactAudioDistance < 10f)
                m_MaxImpactAudioDistance = 10f;
        }

        public override bool IsValid()
        {
            return m_ImpactEffects != null && m_ImpactAudio != null;
        }

        private void Awake()
        {
            m_MaxAudioDistanceSqrd = m_MaxImpactAudioDistance * m_MaxImpactAudioDistance;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (s_ProxyBehaviour != null)
            {
                Destroy(s_ProxyBehaviour);
                s_ProxyBehaviour = null;
            }
        }

        class RuntimeBehaviour : MonoBehaviour
        {
            public BaseHitFxBehaviour[] impactEffectInstances
            {
                get;
                private set;
            }

            private Transform m_EffectsParent = null;

            void Awake()
            {
                SetSurfaceHitFx(instance.m_ImpactEffects);
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
            }

            void OnDestroy()
            {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            }

            void OnActiveSceneChanged(Scene s1, Scene s2)
            {
                for (int i = 0; i < impactEffectInstances.Length; ++i)
                {
                    if (impactEffectInstances[i] != null)
                        impactEffectInstances[i].OnActiveSceneChange();
                }
            }

            IEnumerator DelayedInitEffects()
            {
                yield return null;

                for (int i = 0; i < impactEffectInstances.Length; ++i)
                {
                    if (impactEffectInstances[i] != null && impactEffectInstances[i].forceInitialise)
                        impactEffectInstances[i].Hit(null, new Vector3(0f, -10000f, 0f), Vector3.up);
                }
            }

            public void SetSurfaceHitFx(SurfaceHitFxData data)
            {
                if (data == null)
                    return;

                if (m_EffectsParent == null)
                {
                    // Create effects parent object
                    var go = new GameObject("SurfaceManagerEffects");
                    m_EffectsParent = go.transform;
                    m_EffectsParent.SetParent(transform);
                }

                // Allocate array
                if (impactEffectInstances == null)
                    impactEffectInstances = new BaseHitFxBehaviour[FpsSurfaceMaterial.count];

                // Instantiate surface effects
                for (int i = 0; i < impactEffectInstances.Length; ++i)
                {
                    var effect = data.GetImpactEffect((FpsSurfaceMaterial)i);
                    if (effect != null)
                    {
                        // Destroy existing effect
                        if (impactEffectInstances[i] != null)
                            Destroy(impactEffectInstances[i].gameObject);
                        // Instantiate new effect
                        impactEffectInstances[i] = Instantiate(effect, m_EffectsParent);
                    }
                }

                // Initialise effects on the next frame (not compulsory but prevents frame hitch when first used)
                StartCoroutine(DelayedInitEffects());
            }
        }

        public static void ApplyOverrides(SurfaceFxOverrides overrides)
        {
            if (instance == null)
                return;

            currentOverrides = overrides;
            if (s_ProxyBehaviour != null)
                s_ProxyBehaviour.SetSurfaceHitFx(overrides.impactEffects);
        }

        public static void RemoveOverrides(SurfaceFxOverrides overrides)
        {
            if (instance == null)
                return;

            if (currentOverrides == overrides)
                currentOverrides = null;
            if (s_ProxyBehaviour != null)
                s_ProxyBehaviour.SetSurfaceHitFx(instance.m_ImpactEffects);
        }

        public static void ShowBulletHit(RaycastHit hit, Vector3 rayDirection, float size, bool rigidBody = false)
        {
            if (instance == null)
                return;

            if (hit.transform == null)
            {
                Debug.LogError("SurfaceManager.ShowBulletHit() called with invalid RaycastHit data. Are you checking the raycast actually hit before sending?");
                return;
            }

            FpsSurfaceMaterial surfaceMaterial = FpsSurfaceMaterial.Default;

            // Get the surface material using BaseSurface behaviour (if present - default if not / invalid)
            BaseSurface surface = hit.transform.GetComponentInParent<BaseSurface>();
            if (surface != null)
                surfaceMaterial = surface.GetSurface(hit);

            // Show bullet hit
            bool decal = !rigidBody && (instance.m_DecalLayers & (1 << hit.collider.gameObject.layer)) != 0;
            s_ProxyBehaviour.impactEffectInstances[surfaceMaterial].Hit(hit.transform.gameObject, hit.point, hit.normal, rayDirection, size, decal);
            
			// Get the main camera (Camera.main sucks, but I can't assume all users will add a camera tracking component to any relevant camera)
			if (s_CurrentMainCamera == null || !s_CurrentMainCamera.enabled)
				s_CurrentMainCamera = Camera.main;
			
            // Play bullet hit sound effect if close enough to camera
            if (instance.m_ImpactAudio != null && (s_CurrentMainCamera.transform.position - hit.point).sqrMagnitude <= instance.m_MaxAudioDistanceSqrd)
            {
                float volume = 1f;
                AudioClip clip = null;

                // Get audio clip & volume
                if (currentOverrides != null)
                {
                    clip = currentOverrides.impactAudio.GetAudioClip(surfaceMaterial, out volume);
                    if (clip == null)
                        clip = instance.m_ImpactAudio.GetAudioClip(surfaceMaterial, out volume);
                }
                else
                    clip = instance.m_ImpactAudio.GetAudioClip(surfaceMaterial, out volume);

                if (instance.m_ScaleVolumeToHitSize)
                    volume *= size;

                // Play the audio
                NeoFpsAudioManager.PlayEffectAudioAtPosition(clip, hit.point, volume);
            }
        }

        public static void PlayImpactNoiseAtPosition(FpsSurfaceMaterial surfaceMaterial, Vector3 position, float volume)
        {
            if (instance == null || instance.m_ImpactAudio == null)
                return;

            // Get audio clip
            float clipVolume = 1f;
            AudioClip clip = instance.m_ImpactAudio.GetAudioClip(surfaceMaterial, out clipVolume);
            volume *= clipVolume;

            // Play the audio
            NeoFpsAudioManager.PlayEffectAudioAtPosition(clip, position, volume);
        }
    }
}