using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-wieldablelaserpointer.html")]
    public class WieldableLaserPointer : MonoBehaviour, IWieldableFlashlight, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The colour of the laser.")]
        private Color m_LaserColour = Color.red;
        [SerializeField, Tooltip("Should the laser be on from start.")]
        private bool m_StartOn = false;
        [SerializeField, Tooltip("Should crosshair be hidden while the laser is on.")]
        private bool m_HideCrosshair = true;

        [Header("Beam")]

        [SerializeField, NeoObjectInHierarchyField(true, required = true), Tooltip("The transform the laser will be projected along (forwards).")]
        private Transform m_LaserSource = null;
        [SerializeField, Tooltip("The furthest distance the laser pointer will be visible.")]
        private float m_MaxLaserDistance = 100f;
        [SerializeField, Range(0f, 1f), Tooltip("The alpha falloff of the beam over its length.")]
        private float m_BeamFalloff = 0.25f;

        [Header("Flare")]

        [SerializeField, Tooltip("The size of the laser hit point flare.")]
        private float m_FlareSize = 0.2f;
        [SerializeField, Tooltip("The distance the flare should be pushed forward from the point of impact.")]
        private float m_FlareOffset = 0.05f;
        [SerializeField, Tooltip("The material to use for the laser impact flare.")]
        private Material m_FlareMaterial = null;

        [Header("Events")]

        [SerializeField, Tooltip("An event fired when the laser is switched on")]
        private UnityEvent m_OnToggleOn = null;
        [SerializeField, Tooltip("An event fired when the laser is switched off")]
        private UnityEvent m_OnToggleOff = null;

        private Transform m_RootTransform = null;
        private Transform m_FlareTransform = null;
        private LineRenderer m_LineRenderer = null;
        private Color32[] m_FlareColours = null;
        private Mesh m_FlareMesh = null;
        private RaycastHit m_Hit = new RaycastHit();
        private bool m_On = false;
        private ICrosshairDriver m_CrosshairDriver = null;

        private float m_Brightness = 1f;
        public float brightness
        {
            get { return m_Brightness; }
            set
            {
                // Get the colour with brightness applied to alpha
                m_Brightness = Mathf.Clamp01(value);
                m_LaserColour.a = m_Brightness;

                // Apply the colour
                OnColourChanged();
            }
        }

        public event UnityAction onToggleOn
        {
            add { m_OnToggleOn.AddListener(value); }
            remove { m_OnToggleOn.RemoveListener(value); }
        }

        public event UnityAction onToggleOff
        {
            add { m_OnToggleOff.AddListener(value); }
            remove { m_OnToggleOff.RemoveListener(value); }
        }

        public bool on
        {
            get { return m_On; }
            set
            {
                m_On = value;
                if (m_On)
                {
                    if (m_LineRenderer != null)
                        m_LineRenderer.enabled = true;
                    if (m_HideCrosshair && m_CrosshairDriver != null)
                        m_CrosshairDriver.HideCrosshair();
                    m_OnToggleOn.Invoke();
                }
                else
                {
                    if (m_LineRenderer != null)
                        m_LineRenderer.enabled = false;
                    if (m_FlareTransform != null)
                        m_FlareTransform.gameObject.SetActive(false);
                    if (m_HideCrosshair && m_CrosshairDriver != null)
                        m_CrosshairDriver.ShowCrosshair();
                    m_OnToggleOff.Invoke();
                }
            }
        }

        public Color laserColor
        {
            get { return m_LaserColour; }
            set
            {
                m_LaserColour = value;
                OnColourChanged();
            }
        }

        public bool CheckDoesHit(out Vector3 hitPoint)
        {
            if (m_FlareTransform != null && m_FlareTransform.gameObject.activeSelf)
            {
                hitPoint = m_FlareTransform.position;
                return true;
            }
            else
            {
                hitPoint = Vector3.zero;
                return false;
            }
        }

        void OnValidate()
        {
            if (m_LaserSource == null)
                m_LaserSource = transform;
            m_FlareSize = Mathf.Clamp(m_FlareSize, 0.001f, 1f);
        }

        void Awake()
        {
            if (m_LaserSource == null)
                m_LaserSource = transform;
            m_CrosshairDriver = GetComponentInParent<ICrosshairDriver>();

            // Set up line renderer
            m_LineRenderer = GetComponentInChildren<LineRenderer>();
        }

        void OnEnable()
        {
            m_RootTransform = transform.root;
        }

        void OnDisable()
        {
            on = false;
        }

        void Start()
        {
            // Create flare (impact point) object
            var flareGO = new GameObject("LaserFlare");
            m_FlareTransform = flareGO.transform;
            m_FlareTransform.SetParent(m_LaserSource);

            // Add mesh filter
            var meshFilter = flareGO.AddComponent<MeshFilter>();
            m_FlareMesh = new Mesh();

            // Set vertices
            float halfSize = m_FlareSize * 0.5f;
            m_FlareMesh.vertices = new Vector3[] {
                new Vector3(-halfSize, -halfSize, m_FlareOffset),
                new Vector3(halfSize, -halfSize, m_FlareOffset),
                new Vector3(halfSize, halfSize, m_FlareOffset),
                new Vector3(-halfSize, halfSize, m_FlareOffset)
            };

            // Set triangles
            m_FlareMesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };

            // Set normals
            m_FlareMesh.normals = new Vector3[]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };

            // Set UVs
            m_FlareMesh.uv = new Vector2[4]
            {
                  new Vector2(0, 0),
                  new Vector2(1, 0),
                  new Vector2(1, 1),
                  new Vector2(0, 1)
            };

            // Set up vertex colours
            m_FlareColours = new Color32[4];

            // Apply to mesh
            meshFilter.mesh = m_FlareMesh;

            // Add renderer and set material
            var meshRenderer = flareGO.AddComponent<MeshRenderer>();
            meshRenderer.material = m_FlareMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.allowOcclusionWhenDynamic = false;

            // Disable the object (only enable when hitting an object in range)
            flareGO.SetActive(false);

            // Apply the colours
            OnColourChanged();

            // Switch on/off
            on = m_StartOn;
        }

        void LateUpdate()
        {
            if (!m_On)
                return;

            Vector3 startPoint = m_LaserSource.position;
            Vector3 direction = m_LaserSource.forward;

            Ray ray = new Ray(startPoint, direction);
            Vector3 endPoint;
            bool didHit = PhysicsExtensions.RaycastNonAllocSingle(ray, out m_Hit, m_MaxLaserDistance, PhysicsFilter.Masks.BulletBlockers, m_RootTransform, QueryTriggerInteraction.Ignore);
            if (didHit)
            {
                endPoint = m_Hit.point;
                if (m_FlareTransform != null)
                {
                    m_FlareTransform.position = endPoint + direction * -0.0025f;
                    m_FlareTransform.LookAt(startPoint);
                    m_FlareTransform.gameObject.SetActive(true);
                }
            }
            else
            {
                endPoint = startPoint + (direction * m_MaxLaserDistance);
                if (m_FlareTransform != null)
                    m_FlareTransform.gameObject.SetActive(false);
            }

            // Update line renderer
            if (m_LineRenderer != null)
            {
                m_LineRenderer.SetPosition(0, startPoint);
                m_LineRenderer.SetPosition(1, endPoint);
            }
        }

        public void Toggle()
        {
            on = !on;
        }

        void OnColourChanged()
        {
            // Set the line renderer colours
            if (m_LineRenderer != null)
            {
                // Start
                m_LineRenderer.startColor = m_LaserColour;

                // End
                var c = m_LaserColour;
                c *= (1f - m_BeamFalloff);
                m_LineRenderer.endColor = c;
            }

            // Set the flare mesh vertex colours
            if (m_FlareMesh != null)
            {
                Color32 c32 = m_LaserColour;
                m_FlareColours[0] = c32;
                m_FlareColours[1] = c32;
                m_FlareColours[2] = c32;
                m_FlareColours[3] = c32;
                m_FlareMesh.colors32 = m_FlareColours;
            };
        }

        static readonly NeoSerializationKey k_ColourKey = new NeoSerializationKey("colour");
        static readonly NeoSerializationKey k_OnKey = new NeoSerializationKey("on");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_ColourKey, m_LaserColour);
            writer.WriteValue(k_OnKey, m_On);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_ColourKey, out m_LaserColour, m_LaserColour);
            reader.TryReadValue(k_OnKey, out m_StartOn, m_StartOn);
        }
    }
}