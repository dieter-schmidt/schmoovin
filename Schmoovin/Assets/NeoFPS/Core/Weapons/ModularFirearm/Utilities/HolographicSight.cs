using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-holographicsight.html")]
    public class HolographicSight : MonoBehaviour, IOpticsBrightnessControl, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The base colour of the reticule")]
        private Color m_Color = Color.red;
        [SerializeField, NeoObjectInHierarchyField(false, required = true), Tooltip("The game object with the reticule mesh attached. This should be placed directly in front of the weapon at the desired distance")]
        private GameObject m_Reticule = null;
        [SerializeField, Tooltip("A series of brightness values that can be cycled through with the \"Optics Brightness +/-\" inputs")]
        private float[] m_BrightnessSettings = new float[] { 0.6f, 0.7f, 0.775f, 0.85f, 0.925f, 1f };
        [SerializeField, Tooltip("The index of the starting brightness setting from the above array")]
        private int m_BrightnessSetting = 4;

        private static readonly NeoSerializationKey k_ColourKey = new NeoSerializationKey("colour");
        private static readonly NeoSerializationKey k_BrightnessKey = new NeoSerializationKey("brightness");

        private Mesh m_ReticuleMesh = null;
        private Color32[] m_ReticuleColours = null;

        public float brightness
        {
            get { return m_Color.a; }
            set
            {
                // Brightness is colour alpha
                value = Mathf.Clamp01(value);
                m_Color.a = value;

                // Apply the colour
                OnColourChanged();
            }
        }

        public Color reticuleColor
        {
            get { return m_Color; }
            set
            {
                m_Color = value;
                OnColourChanged();
            }
        }

        public Material reticuleMaterial
        {
            get { return m_Reticule.GetComponent<Renderer>().material; }
            set { m_Reticule.GetComponent<Renderer>().material = value; }
        }

        public void SetBrightness(int index)
        {
            m_BrightnessSetting = Mathf.Clamp(index, 0, m_BrightnessSettings.Length - 1);
            brightness = m_BrightnessSettings[m_BrightnessSetting];
        }

        public void IncrementBrightness(bool looping = false)
        {
            if (looping)
            {
                ++m_BrightnessSetting;
                if (m_BrightnessSetting >= m_BrightnessSettings.Length)
                    m_BrightnessSetting = 0;
                brightness = m_BrightnessSettings[m_BrightnessSetting];
            }
            else
            {
                if (m_BrightnessSetting < m_BrightnessSettings.Length - 1)
                {
                    ++m_BrightnessSetting;
                    brightness = m_BrightnessSettings[m_BrightnessSetting];
                }
            }
        }

        public void DecrementBrightness(bool looping = false)
        {
            if (looping)
            {
                --m_BrightnessSetting;
                if (m_BrightnessSetting < 0)
                    m_BrightnessSetting = m_BrightnessSettings.Length - 1;
                brightness = m_BrightnessSettings[m_BrightnessSetting];
            }
            else
            {
                if (m_BrightnessSetting > 0)
                {
                    --m_BrightnessSetting;
                    brightness = m_BrightnessSettings[m_BrightnessSetting];
                }
            }
        }

        private void OnValidate()
        {
            // Make sure there's always at least 1 brightness setting
            if (m_BrightnessSettings.Length == 0)
                m_BrightnessSettings = new float[] { 1f };

            // Make sure brightness settings are ascending within 0-1 range
            for (int i = 0; i < m_BrightnessSettings.Length; ++i)
            {
                // Clamp lower limit
                if (m_BrightnessSettings[i] > 1f)
                    m_BrightnessSettings[i] = 1f;

                if (i == 0)
                {
                    // Clamp to 0
                    if (m_BrightnessSettings[i] < 0f)
                        m_BrightnessSettings[i] = 0f;
                }
                else
                {
                    // Clamp to previous value
                    if (m_BrightnessSettings[i] < m_BrightnessSettings[i - 1])
                        m_BrightnessSettings[i] = m_BrightnessSettings[i - 1];
                }
            }

            m_BrightnessSetting = Mathf.Clamp(m_BrightnessSetting, 0, m_BrightnessSettings.Length - 1);
        }

        private void Awake()
        {
            var meshFilter = m_Reticule.GetComponent<MeshFilter>();
            if (meshFilter != null)
                m_ReticuleMesh = meshFilter.mesh;
            else
                Debug.LogError("Holosight reticule does not have a mesh: " + name);

            m_ReticuleColours = new Color32[4];
        }

        private void Start()
        {
            OnColourChanged();
            SetBrightness(m_BrightnessSetting);
        }

        void OnColourChanged()
        {
            // Set the flare mesh vertex colours
            if (m_ReticuleMesh != null)
            {
                Color32 c32 = m_Color;
                m_ReticuleColours[0] = c32;
                m_ReticuleColours[1] = c32;
                m_ReticuleColours[2] = c32;
                m_ReticuleColours[3] = c32;
                m_ReticuleMesh.colors32 = m_ReticuleColours;
            };
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_ColourKey, m_Color);
            writer.WriteValue(k_BrightnessKey, m_BrightnessSetting);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_ColourKey, out m_Color, m_Color);
            reader.TryReadValue(k_BrightnessKey, out m_BrightnessSetting, m_BrightnessSetting);
        }
    }
}
