using System.Collections;
using UnityEngine;
using NeoSaveGames;

namespace NeoFPS.Samples
{
	public class SinglePlayerNavControls : MenuNavControls
	{
        [SerializeField, Tooltip("The continue game button (disable if no auto saves exist)")]
        private MultiInputButton m_ContinueButton = null;
        [SerializeField, Tooltip("The load game button (disable if no saves exist)")]
        private MultiInputButton m_LoadButton = null;

        private bool m_CanContinue = false;
        bool canContinue
        {
            get { return m_CanContinue; }
            set
            {
                if (m_CanContinue != value)
                {
                    m_CanContinue = value;
                    m_ContinueButton.interactable = value;
                    m_ContinueButton.RefreshInteractable();
                }
            }
        }

        public override void Show()
        {
            base.Show();

            // Check for available saves
            if (m_LoadButton != null)
                m_LoadButton.interactable = SaveGameManager.hasAvailableSaves;

            if (m_ContinueButton != null)
            {
                // Check if can continue (this can block so do it intermittently)
                canContinue = SaveGameManager.canContinue;
                if (!canContinue)
                    StartCoroutine(UpdateContinueButton());
                
                m_ContinueButton.onClick.AddListener(OnClickContinue);
            }
        }

        public override void Hide()
        {
            // Remove continue event listener
            if (m_ContinueButton != null)
                m_ContinueButton.onClick.RemoveListener(OnClickContinue);

            base.Hide();
        }

        IEnumerator UpdateContinueButton()
        {
            float t = 0f;
            while (t < 0.25f)
            {
                yield return null;
                t += Time.unscaledDeltaTime;
            }

            while (!canContinue)
            {
                t = 0f;
                while (t < 0.5f)
                {
                    yield return null;
                    t += Time.unscaledDeltaTime;
                }
                canContinue = SaveGameManager.canContinue;
            }
        }

        public void OnClickContinue()
        {
            SaveGameManager.Continue();
            canContinue = false;
        }
    }
}