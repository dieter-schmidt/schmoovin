using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
    [HelpURL("http://docs.neofps.com/manual/samples-ui.html")]
    [CreateAssetMenu (fileName = "UiStyle", menuName = "NeoFPS/UI Style", order = NeoFpsMenuPriorities.ungrouped_uiStyle)]
	public class UiStyle : ScriptableObject
	{
        [SerializeField] private float m_HeightSmall = 0f;
        [SerializeField] private float m_HeightLarge = 0f;
		[SerializeField] private Sprite m_Background = null;
        [SerializeField] private float m_InputPulseDuration = 0.15f;
		[SerializeField] private ColourInfo m_Colours = ColourInfo.defaultValue;
		[SerializeField] private TextInfo m_TextInfo = TextInfo.defaultValue;
        [SerializeField] private Icons m_Icons = new Icons();
        [SerializeField] private SoundEffects m_SoundEffects = new SoundEffects();

		public float heightSmall
		{
			get { return m_HeightSmall; }
		}
		public float heightLarge
		{
			get { return m_HeightLarge; }
		}

		public float inputPulseDuration
		{
			get { return m_InputPulseDuration; }
		}

		public Sprite background
		{
			get { return m_Background; }
		}

		public ColourInfo colours
		{
			get { return m_Colours; }
		}

		public TextInfo textInfo
		{
			get { return m_TextInfo; }
		}

		public Icons icons
		{
			get { return m_Icons; }
		}

		public SoundEffects soundEffects
		{
			get { return m_SoundEffects; }
		}

		[Serializable]
		public struct ColourInfo
		{
			public Color normal;
			public Color highlighted;
			public Color focussed;
			public Color pressed;
			public Color disabled;

			public static ColourInfo defaultValue = new ColourInfo {
				normal = new Color (1f,1f,1f,0.85f),
				highlighted = Color.white,
				focussed = new Color (0.75f,1f,1f,1f),
				pressed = new Color (0.75f,1f,1f,0.85f),
				disabled = new Color (1f,1f,1f,0.25f),
			};
		}

		[Serializable]
		public struct TextInfo
		{
			public Font font;
			public Color headerColour;
			public int headerFontSize;
			public Color descriptionColour;
			public int descriptionFontSize;
			public Color controlColour;
			public int controlFontSize;

			public static TextInfo defaultValue = new TextInfo {
				headerColour = Color.white,
				headerFontSize = 24,
				descriptionColour = Color.white,
				descriptionFontSize = 16,
				controlColour = Color.white,
				controlFontSize = 24
			};
		}

		[Serializable]
		public struct Icons
		{
			public Sprite plus;
			public Sprite minus;
			public Sprite left;
			public Sprite right;
			public Sprite expand;
			public Sprite collapse;
			public Sprite locked;
			public Sprite alert;
		}

		[Serializable]
		public struct SoundEffects
		{
			public AudioClip move;
			public AudioClip press;
			public AudioClip error;
		}
	}
}