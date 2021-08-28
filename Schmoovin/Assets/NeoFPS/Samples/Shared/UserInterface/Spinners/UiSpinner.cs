using System.Collections;
using UnityEngine;

namespace NeoFPS.Samples
{
    public abstract class UiSpinner : MonoBehaviour
    {
        [SerializeField]
        private bool m_StartVisible = true;

        public abstract void Tick();
        protected abstract IEnumerator ShowCoroutine();
        protected abstract IEnumerator HideCoroutine();

        protected virtual void Awake()
        {
            if (!m_StartVisible)
                gameObject.SetActive(false);
        }

        public void Show()
        {
            StopAllCoroutines();
            gameObject.SetActive(true);
            StartCoroutine(ShowCoroutine());
        }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(HideCoroutineInternal());
        }

        private void Update()
        {
            Tick();
        }

        private IEnumerator HideCoroutineInternal()
        {
            yield return StartCoroutine(HideCoroutine());
            gameObject.SetActive(false);
        }
    }
}