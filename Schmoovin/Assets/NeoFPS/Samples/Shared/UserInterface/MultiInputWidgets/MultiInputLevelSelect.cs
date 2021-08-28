using NeoSaveGames.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeoFPS.Samples
{
	public class MultiInputLevelSelect : MultiInputWidget, ISubmitHandler, IPointerClickHandler
	{
        [SerializeField] private Image m_ScreenshotImage = null;

        private Sprite m_Screenshot = null;

        public enum SceneLoadMode
        {
            Index,
            Name
        }

        public string level
		{
            get;
            set;
		}

		public Sprite screenshot
		{
			get { return m_Screenshot; }
			set
			{
				m_Screenshot = value;
				if (m_ScreenshotImage != null)
					m_ScreenshotImage.sprite = m_Screenshot;
			}
		}

		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			if (m_ScreenshotImage != null)
				m_ScreenshotImage.sprite = m_Screenshot;
		}
		#endif

		protected override bool customHeight
		{
			get { return true; }
		}

		public void OnPointerClick (PointerEventData eventData)
		{
			Press ();
		}
		public void OnSubmit (BaseEventData eventData)
		{
			Press ();
		}

		private void Press ()
		{
			PlayAudio (MenuAudio.ClickValid);
            NeoSceneManager.LoadScene(level);
		}
	}
}

