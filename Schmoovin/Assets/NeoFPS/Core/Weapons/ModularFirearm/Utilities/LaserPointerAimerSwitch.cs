using NeoSaveGames;
using NeoSaveGames.Serialization;
using System;
using System.Collections;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [RequireComponent(typeof(WieldableLaserPointer))]
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-laserpointeraimerswitch.html")]
    public class LaserPointerAimerSwitch : MonoBehaviour, INeoSerializableComponent
    {
        [SerializeField, RequiredObjectProperty, Tooltip("The aimer module associated with this laser pointer. This should be disabled on start.")]
        private BaseAimerBehaviour m_LaserAimerModule = null;

        [SerializeField, Tooltip("The minimum accuracy of the firearm when the laser is switched on.")]
        private float m_LaserMinAccuracy = 0.85f;

        private ModularFirearm m_Firearm = null;
        private BaseAimerBehaviour m_Aimer = null;
        private WieldableLaserPointer m_Laser = null;

        private IEnumerator Start()
        {
            yield return null;

            // Get the active aimer component
            m_Firearm = GetComponentInParent<ModularFirearm>();
            if (m_Aimer == null)
            {
                m_Aimer = m_Firearm.aimer as BaseAimerBehaviour;
                if (m_Aimer == m_LaserAimerModule)
                {
                    Debug.LogError("LaserPointerAimerSwitch requires that the laser aimer be inactive on start, and the aimer to replace is active.");
                    m_Aimer = null;
                }
            }

            if (m_Aimer != null)
            {
                m_Laser = GetComponent<WieldableLaserPointer>();
                m_Laser.onToggleOn += OnToggleLaserOn;
                m_Laser.onToggleOff += OnToggleLaserOff;

                if (m_Laser.on)
                    OnToggleLaserOn();
                else
                    OnToggleLaserOff();
            }
        }

        private void OnToggleLaserOn()
        {
            if (m_LaserAimerModule != null)
                m_LaserAimerModule.Enable();
            m_Firearm.minAccuracy = m_LaserMinAccuracy;
        }

        private void OnToggleLaserOff()
        {
            m_Aimer.Enable();
            m_Firearm.minAccuracy = 0f;
        }

        static readonly NeoSerializationKey k_AimerKey = new NeoSerializationKey("aimer");

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_Aimer != null)
                writer.WriteComponentReference(k_AimerKey, m_Aimer, nsgo);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            BaseAimerBehaviour aimer;
            if (reader.TryReadComponentReference(k_AimerKey, out aimer, nsgo))
                m_Aimer = aimer;
        }
    }
}
