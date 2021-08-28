using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class WorldSpaceHudMarkerBase : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The number of markers that will be pooled on start.")]
        private int m_MaxMarkers = 8;

        [SerializeField, Tooltip("How many markers to add if the pool limit is reached.")]
        private int m_ExpandAmount = 8;

        private RectTransform m_FullRect = null;
        private List<WorldSpaceHudMarkerItemBase> m_InactiveMarkers = new List<WorldSpaceHudMarkerItemBase>();
        private List<WorldSpaceHudMarkerItemBase> m_ActiveMarkers = new List<WorldSpaceHudMarkerItemBase>();
        private Camera m_Camera = null;
        private Transform m_CameraTransform = null;

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            if (character != null)
            {
                m_Camera = character.fpCamera.unityCamera;
                m_CameraTransform = m_Camera.transform;
            }
            else
            {
                m_Camera = null;
                m_CameraTransform = null;
            }
        }

        protected override void Start()
        {
            base.Start();

            var marker = GetComponentInChildren<WorldSpaceHudMarkerItemBase>();
            if (marker == null)
            {
                Debug.LogError("No marker item found in world space HUD markers setup: " + gameObject);
                gameObject.SetActive(false);
            }
            else
            {
                m_InactiveMarkers.Add(marker);

                // Force marker transform to anchor nicely
                var markerTransform = marker.transform as RectTransform;
                markerTransform.anchorMin = Vector2.zero;
                markerTransform.anchorMax = Vector2.zero;

                for (int i = 1; i < m_MaxMarkers; ++i)
                    m_InactiveMarkers.Add(Instantiate(marker, Vector3.zero, Quaternion.identity, markerTransform.parent));

                for (int i = 0; i < m_MaxMarkers; ++i)
                    m_InactiveMarkers[i].gameObject.SetActive(false);

                m_FullRect = transform as RectTransform;

            }
        }

        protected virtual void LateUpdate()
        {
            // Track objects
            if (m_Camera != null)
            {
                var cameraPosition = m_CameraTransform.position;

                for (int i = 0; i < m_ActiveMarkers.Count; ++i)
                {
                    var marker = m_ActiveMarkers[i];

                    var worldPosition = marker.GetWorldPosition();
                    var viewportPosition = m_Camera.WorldToViewportPoint(worldPosition);

                    // Check if should hide marker (z is distance in front of camera)
                    bool hide = viewportPosition.z < 0f;
                    hide |= viewportPosition.x < 0f || viewportPosition.x > 1f;
                    hide |= viewportPosition.y < 0f || viewportPosition.y > 1f;

                    if (hide && marker.gameObject.activeSelf)
                        marker.gameObject.SetActive(false);
                    if (!hide && !marker.gameObject.activeSelf)
                        marker.gameObject.SetActive(true);

                    // Set the position and remove the screen offset
                    marker.localTransform.anchoredPosition = new Vector2(viewportPosition.x * m_FullRect.rect.width, viewportPosition.y * m_FullRect.rect.height);
                }
            }
        }

        protected WorldSpaceHudMarkerItemBase GetMarker()
        {
            int last = m_InactiveMarkers.Count - 1;
            if (last < 0)
            {
                // Expand the pool if allowed
                if (m_ExpandAmount > 0 && m_ActiveMarkers.Count > 0)
                {
                    for (int i = 0; i < m_ExpandAmount; ++i)
                    {
                        var newMarker = Instantiate(m_ActiveMarkers[0], Vector3.zero, Quaternion.identity, m_ActiveMarkers[0].transform.parent);
                        newMarker.gameObject.SetActive(false);
                        m_InactiveMarkers.Add(newMarker);
                    }
                    last = m_InactiveMarkers.Count - 1;
                }
                else
                    return null;
            }

            var marker = m_InactiveMarkers[last];
            marker.gameObject.SetActive(true);
            m_ActiveMarkers.Add(marker);
            m_InactiveMarkers.RemoveAt(last);

            return marker;
        }

        protected void ReleaseMarker(WorldSpaceHudMarkerItemBase marker)
        {
            int index = m_ActiveMarkers.IndexOf(marker);
            if (index != -1)
            {
                marker.gameObject.SetActive(false);
                m_InactiveMarkers.Add(marker);
                m_ActiveMarkers.RemoveAt(index);
            }
        }

        protected void ClearMarkers()
        {
            for (int i = 0; i < m_ActiveMarkers.Count; ++i)
            {
                var marker = m_ActiveMarkers[i];
                if (marker != null)
                {
                    marker.gameObject.SetActive(false);
                    m_InactiveMarkers.Add(marker);
                }
            }
            m_ActiveMarkers.Clear();
        }
    }
}