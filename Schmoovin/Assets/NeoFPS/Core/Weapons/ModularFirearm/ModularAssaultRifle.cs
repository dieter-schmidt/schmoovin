using System;
using System.Collections;
using System.Collections.Generic;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-modularassaultrifle.html")]
    public class ModularAssaultRifle : ModularFirearm
	{
		[Header ("Assault Rifle")]

		[SerializeField, Tooltip("The different fire modes.")]
        private FireMode[] m_Modes = new FireMode[0];

        [SerializeField, Tooltip("Which fire mode should be used on start.")]
        private int m_StartIndex = 0;

		[SerializeField, Tooltip("The audio clip to play when the weapon mode is switched.")]
        private AudioClip m_SwitchAudio = null;

        [Serializable]
		private struct FireMode
        {
            #pragma warning disable 0649

            [Tooltip("The name of the fire mode for the HUD.")]
			public string name;
			[Tooltip("The trigger to activate.")]
			public BaseTriggerBehaviour trigger;

            #pragma warning restore 0649
        }

        private int m_CurrentTrigger = 0;

		#if UNITY_EDITOR
		void OnValidate ()
		{
			if (m_Modes.Length > 0)
				m_StartIndex = Mathf.Clamp (m_StartIndex, 0, m_Modes.Length);
			else
				m_StartIndex = 0;
		}
		#endif

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("The ModularAssaultRifle component is deprecated. Please replace it with a standard ModularFirearm component and FirearmModeSwitcher for the trigger switching. See the sample assault rifles for an example");
        }

        public override string mode
        {
            get
            {
                if (m_Modes != null || m_Modes.Length == 0)
                    return m_Modes[m_CurrentTrigger].name;
                else
                    return string.Empty;
            }
        }

        protected override void GetStartingModeInternal ()
		{
			m_CurrentTrigger = m_StartIndex;
		}

		protected override bool SwitchModeInternal ()
		{
			++m_CurrentTrigger;
			if (m_CurrentTrigger == m_Modes.Length)
				m_CurrentTrigger = 0;

			m_Modes [m_CurrentTrigger].trigger.enabled = true;

            PlaySound(m_SwitchAudio);

            return true;
        }

        #region INeoSerializableComponent IMPLEMENTATION

        private static readonly NeoSerializationKey k_ModeKey = new NeoSerializationKey("mode");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);
            writer.WriteValue(k_ModeKey, m_CurrentTrigger);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);
            int result = 0;
            if (reader.TryReadValue(k_ModeKey, out result, 0))
            {
                m_CurrentTrigger = result - 1;
                SwitchMode();
                // Should this just set the values?
                // Actually switching fires events, etc
            }
        }

        #endregion
    }
}