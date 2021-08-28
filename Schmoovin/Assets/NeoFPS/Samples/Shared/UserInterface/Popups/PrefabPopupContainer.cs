using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace NeoFPS.Samples
{
    public class PrefabPopupContainer : BasePopup
    {
        private static PrefabPopupContainer k_Instance = null;

        private IPrefabPopup m_Popup = null;

        public override void Initialise(BaseMenu menu)
        {
            base.Initialise(menu);
            k_Instance = this;
        }

        void OnDestroy()
        {
            if (k_Instance == this)
                k_Instance = null;
        }

        public static T ShowPrefabPopup<T>(T prefab) where T : MonoBehaviour, IPrefabPopup
        {
            if (k_Instance == null || prefab == null)
                return null;

            // Instantiate and parent
            var instance = Instantiate(prefab);
            var rt = instance.transform as RectTransform;
            rt.SetParent(k_Instance.transform);
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;

            // Store
            k_Instance.m_Popup = instance;

            // Initialise
            k_Instance.m_Popup.OnShow(k_Instance.menu);
            k_Instance.startingSelection = k_Instance.m_Popup.startingSelection;

            // Show
            k_Instance.menu.ShowPopup(k_Instance);

            return instance;
        }

        public override void Hide()
        {
            base.Hide();

            var mb = m_Popup as MonoBehaviour;
            if (mb != null)
            {
                Destroy(mb.gameObject);
                m_Popup = null;
                startingSelection = null;
            }
        }

        public override void Back()
        {
            base.Back();
            if (m_Popup != null)
                m_Popup.Back();
        }
    }
}