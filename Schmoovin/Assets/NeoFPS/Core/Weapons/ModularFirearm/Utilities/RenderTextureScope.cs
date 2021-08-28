using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-rendertexturescope.html")]
    public class RenderTextureScope : MonoBehaviour, INeoSerializableComponent
    {
        [Header("Camera")]
        [SerializeField, NeoObjectInHierarchyField(false, required = true), Tooltip("The scope camera")]
        private Camera m_Camera = null;
        [SerializeField, Range(1f, 90f), Tooltip("The field of view of the scope camera")]
        private float m_FOV = 20f;
        [SerializeField, Range(0.1f, 2f), Tooltip("The amount of the screen height the scope takes up (multiplier). Smaller screen coverage needs a smaller render texture.")]
        private float m_ScreenCoverage = 1f;
        [SerializeField, Range(0f, 1f), Tooltip("Rotate the camera to adapt to the parallax effect. At 1, the image will track with the scope ring. At 0, the scope ring will be completely detached from the image.")]
        private float m_AngleCompensation = 0.75f;

        [Header("Render Texture Material")]
        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The mesh renderer of the scope lens which will receive the render texture")]
        private MeshRenderer m_MeshRenderer = null;
        [SerializeField, Tooltip("Material Index is used to specify which material to modify from a multi-part mesh")]
        private int m_MaterialIndex = 0;
        [SerializeField, Tooltip("The angle in degrees off the axis of the scope where the camera stops rendering and the material becomes completely opaque")]
        private float m_OpaqueAngle = 25;

        [Header("Reticule")]
        [SerializeField, NeoObjectInHierarchyField(false), Tooltip("The transform of the reticule geometry. Local position (0,0,0) should be centered on the camera and slightly in front.")]
        private Transform m_ReticuleTransform = null;

        [Header("Scope Ring (Inner)")]
        [SerializeField, Range(0.25f, 2f), Tooltip("The start of the scope ring in terms of the normalised radius. At 1, the ring is a circle that touches the edges of the texture")]
        private float m_ScopeRingNormalised = 0.75f;
        [SerializeField, Range(0f, 1f), Tooltip("The focus level of the scope ring. Setting this lower will expand the blur of the ring out from the normalised start radius")]
        private float m_ScopeRingFocus = 0.25f;

        [Header("Parallax Effect")]
        [SerializeField, NeoObjectInHierarchyField(true), Tooltip("A transform that represents the lens of the scope that the player looks into. Used to determine the off-axis angle for parallax and fade")]
        private Transform m_FocalPoint = null;
        [SerializeField, Tooltip("An angle range from the scope axis where there is no parallax. This prevents the reticule from going off center when almost dead on ")]
        private float m_ParallaxDeadzone = 1f;
        [SerializeField, Range(0f, 1f), Tooltip("The amount of parallax to add to the scope ring as the eye-line moves away from the scope axis")]
        private float m_ScopeRingParallax = 0.2f;
        [SerializeField, Range(0f, 1f), Tooltip("The amount of parallax to add to the reticule as the eye-line moves away from the scope axis. Values above the scope ring parallax will make the reticule seem like it is further forward than the ring.")]
        private float m_ReticuleParallax = 0.2f;

        private static readonly NeoSerializationKey k_SerializationKey_FoV = new NeoSerializationKey("fov");

        // Constants
        const string k_ShaderParameterTransparency = "_ScopeTransparency";
        const string k_ShaderParameterParallax = "_ScopeParallax";
        const string k_ShaderParameterRingNormalised = "_ScopeRingNormalised";
        const string k_ShaderParameterRingFocus = "_ScopeRingFocus";

        private RenderTexture m_RenderTexture = null;
        private MaterialPropertyBlock m_PropertyBlock;
        private Vector2 m_Parallax = Vector2.zero;
        private float m_Transparency = 0f;
        private float m_ReticuleDepth = 0f;
        private float m_ReticuleScale = 0f;
        private float m_AdjustedDeadZone = 0f;
        private bool m_Dirty = false;
        private bool m_Force = false;

        private int m_KeyTransparency = 0;
        private int m_KeyParallax = 0;
        private int m_KeyRingNormalised = 0;
        private int m_KeyRingFocus = 0;

        public float fov
        {
            get { return m_FOV; }
            set
            {
                // Clamp the value and check if changed
                float clamped = Mathf.Clamp(value, 1f, 90f);
                if (clamped != m_FOV || m_Force)
                {
                    m_FOV = clamped;
                    UpdateCameraFoV();
                }
            }
        }

        public float transparency
        {
            get { return m_Transparency; }
            set
            {
                // Clamp the value and check if changed
                float clamped = Mathf.Clamp01(value);
                if (clamped != m_Transparency || m_Force)
                {
                    // Set camera active state if required
                    if (clamped == 0f)
                        m_Camera.gameObject.SetActive(false);
                    else
                    {
                        if (m_Transparency == 0f)
                            m_Camera.gameObject.SetActive(true);
                    }

                    // Set the property
                    m_Transparency = clamped;
                    m_PropertyBlock.SetFloat(m_KeyTransparency, m_Transparency);

                    // Set the dirty flag
                    m_Dirty = true;
                }
            }
        }

        public float scopeRingNormalised
        {
            get { return m_ScopeRingNormalised; }
            set
            {
                // Clamp the value and check if changed
                float clamped = Mathf.Clamp(value, 0.25f, 2f);
                if (m_ScopeRingNormalised != clamped || m_Force)
                {
                    // Set the property
                    m_ScopeRingNormalised = clamped;
                    m_PropertyBlock.SetFloat(m_KeyRingNormalised, m_ScopeRingNormalised);

                    m_Dirty = true;
                }
            }
        }

        public float scopeRingFocus
        {
            get { return m_ScopeRingFocus; }
            set
            {
                // Clamp the value and check if changed
                float clamped = Mathf.Clamp01(value);
                if (m_ScopeRingFocus != clamped || m_Force)
                {
                    // Set the property
                    m_ScopeRingFocus = clamped;
                    m_PropertyBlock.SetFloat(m_KeyRingFocus, m_ScopeRingFocus);

                    m_Dirty = true;
                }
            }
        }

        public Vector2 parallax
        {
            get { return m_Parallax; }
            set
            {
                if (!Mathf.Approximately(value.x, m_Parallax.x) ||
                    !Mathf.Approximately(value.y, m_Parallax.y) ||
                    m_Force)
                {
                    m_Parallax = value;

                    // Apply offset to reticule
                    if (m_ReticuleParallax > 0.001f)
                    {
                        Vector3 parallaxPosition = m_Parallax * m_ReticuleParallax * m_ReticuleScale;
                        parallaxPosition.z = m_ReticuleDepth;
                        if (m_ReticuleTransform != null)
                            m_ReticuleTransform.localPosition = parallaxPosition;
                    }


                    // Apply offset to shader ring
                    if (m_ScopeRingParallax > 0.001f)
                    {
                        Vector4 ringParallax = new Vector4(
                                m_Parallax.x * m_ScopeRingParallax,
                                m_Parallax.y * m_ScopeRingParallax,
                                0f, 0f
                            );

                        m_PropertyBlock.SetVector(m_KeyParallax, ringParallax);

                        // Apply offset to camera rotation
                        if (m_AngleCompensation > 0.001f)
                        {
                            float fullAngle = m_FOV * m_AngleCompensation * 0.25f;
                            m_Camera.transform.localRotation = Quaternion.Euler(m_Parallax.y * fullAngle, m_Parallax.x * fullAngle, 0f);
                        }

                        m_Dirty = true;
                    }
                }
            }
        }


        private void OnValidate()
        {
            if (m_Camera == null)
                m_Camera = GetComponentInChildren<Camera>();
            if (m_MeshRenderer == null)
                m_MeshRenderer = GetComponentInChildren<MeshRenderer>();
            if (m_MeshRenderer != null)
                m_MaterialIndex = Mathf.Clamp(m_MaterialIndex, 0, m_MeshRenderer.sharedMaterials.Length);
            if (m_FocalPoint == null)
                m_FocalPoint = transform;
            m_OpaqueAngle = Mathf.Clamp(m_OpaqueAngle, 5f, 60f);
            m_ParallaxDeadzone = Mathf.Clamp(m_ParallaxDeadzone, 0f, 5f);
        }

        void Awake()
        {
            if (m_ReticuleTransform != null)
            {
                m_ReticuleDepth = m_ReticuleTransform.localPosition.z;
                m_ReticuleTransform.gameObject.SetActive(false);
            }
            m_AdjustedDeadZone = Mathf.Sin(m_ParallaxDeadzone * Mathf.Deg2Rad);
        }

        private void Start()
        {
            Initialise();
        }

        private void OnEnable()
        {
            if (m_Camera != null && m_ReticuleTransform != null)
            {
                Camera.onPostRender += OnCameraPostRender;
            }
        }

        private void OnDisable()
        {
            if (m_Camera != null && m_ReticuleTransform != null)
            {
                Camera.onPostRender -= OnCameraPostRender;
            }
        }

        void UpdateCameraFoV()
        {
            // Apply to camera
            if (m_Camera != null)
                m_Camera.fieldOfView = m_FOV;

            // Calculate reticule scale
            if (m_ReticuleTransform != null)
            {
                m_ReticuleScale = Mathf.Tan(Mathf.Deg2Rad * m_FOV * 0.5f) * m_ReticuleDepth * 2f;
                m_ReticuleTransform.localScale = new Vector3(m_ReticuleScale, m_ReticuleScale, m_ReticuleScale);
            }
        }

        private void Update()
        {
            if (FirstPersonCamera.current != null && m_FocalPoint != null)
            {
                // Get vector to camera point
                Vector3 direction = Vector3.Normalize(m_FocalPoint.position - FirstPersonCamera.current.transform.position);
                float angle = Vector3.Angle(m_FocalPoint.forward, direction);

                // Calculate transparency
                float transparencyLerp = angle / m_OpaqueAngle;
                transparency = 1f - transparencyLerp * transparencyLerp;

                // Parallax with deadzone
                direction = Quaternion.Inverse(m_FocalPoint.rotation) * direction;
                parallax = new Vector2(ApplyDeadzone(direction.x), ApplyDeadzone(direction.y)) * -10f;
            }
            else
            {
                transparency = 0f;
                parallax = Vector2.zero;
            }

            if (m_Dirty && m_Camera != null && m_MeshRenderer != null)
            {
                // Set the properties to the material
                m_MeshRenderer.SetPropertyBlock(m_PropertyBlock, m_MaterialIndex);
                m_Dirty = false;
            }

            if (m_ReticuleTransform != null)
                m_ReticuleTransform.gameObject.SetActive(true);
        }

        float ApplyDeadzone(float value)
        {
            float result = Mathf.Abs(value) - m_AdjustedDeadZone;
            if (result < 0f)
                result = 0f;
            else
                result *= Mathf.Sign(value);
            return result;
        }

        private void OnDestroy()
        {
            if (m_RenderTexture != null)
                Destroy(m_RenderTexture);
        }

        void Initialise()
        {
            if (m_Camera != null && m_MeshRenderer != null)
            {
                // Get the render texture dimensions (nearest power of 2 to resolution height)
                var resolutionY = (int)(Screen.currentResolution.height * m_ScreenCoverage);
                int dimension = 256;
                while (true)
                {
                    int next = dimension * 2;
                    if (next >= resolutionY)
                        break;
                    else
                        dimension = next;
                }

                // Allocate render texture to camera
                // Should rt be static to prevent multiple?
                m_RenderTexture = new RenderTexture(dimension, dimension, 24);
                m_Camera.targetTexture = m_RenderTexture;

                // Set up renderer property block
                m_PropertyBlock = new MaterialPropertyBlock();
                m_MeshRenderer.GetPropertyBlock(m_PropertyBlock, m_MaterialIndex);
                
                // Apply render texture
                m_PropertyBlock.SetTexture("_RenderTexture", m_RenderTexture);
                
                // Get property ids
                m_KeyTransparency = Shader.PropertyToID(k_ShaderParameterTransparency);
                m_KeyParallax = Shader.PropertyToID(k_ShaderParameterParallax);
                m_KeyRingNormalised = Shader.PropertyToID(k_ShaderParameterRingNormalised);
                m_KeyRingFocus = Shader.PropertyToID(k_ShaderParameterRingFocus);
                
                // Apply starting properties
                m_Force = true;
                transparency = 0f;
                UpdateCameraFoV();
                scopeRingNormalised = m_ScopeRingNormalised;
                scopeRingFocus = m_ScopeRingFocus;
                parallax = Vector2.zero;
                m_Force = false;

                // Set the properties to the material
                m_MeshRenderer.SetPropertyBlock(m_PropertyBlock, m_MaterialIndex);
            }
        }

        void OnCameraPostRender(Camera cam)
        {
            if (cam == m_Camera)
            {
                // Disable reticule
                m_ReticuleTransform.gameObject.SetActive(false);
            }
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_SerializationKey_FoV, fov);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            float result;
            if (reader.TryReadValue(k_SerializationKey_FoV, out result, fov))
                fov = result;
        }
    }
}