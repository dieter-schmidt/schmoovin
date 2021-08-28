using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/interactionref-mb-interactiveobjectcornermarkers.html")]
	[RequireComponent (typeof (InteractiveObject))]
	public class InteractiveObjectCornerMarkers : MonoBehaviour
	{
		[SerializeField, Tooltip("The box colliders of the interactive object.")]
		private BoxCollider[] m_BoxColliders = new BoxCollider[0];

        [SerializeField, Tooltip("The prefab to use for the corner objects. 8 instances of this will be instantiated and placed at the corners of the box.")]
		private GameObject m_CornerObject = null;

        private IInteractiveObject m_Interactable = null;
        private Transform[] m_Corners = null;

		void OnValidate ()
		{
			if (m_BoxColliders == null || m_BoxColliders.Length == 0)
                m_BoxColliders = GetComponentsInChildren<BoxCollider> (true);
		}

		void Start ()
		{
			Transform t = transform;

			m_Interactable = GetComponent <IInteractiveObject> ();
			m_Interactable.onCursorEnter += Show;
			m_Interactable.onCursorExit += Hide;

            // Allocate transform array
            int boxCount = 0;
            for (int i = 0; i < m_BoxColliders.Length; ++i)
            {
                if (m_BoxColliders[i] != null)
                    ++boxCount;
            }
            m_Corners = new Transform[boxCount * 8];

            // Iterate through & add corners
            int itr = 0;
            for (int i = 0; i < m_BoxColliders.Length; ++i)
            {
                var box = m_BoxColliders[i];
                if (box == null)
                    continue;

                int startIndex = itr * 8;
                for (int j = 0; j < 8; ++j)
                {
                    m_Corners[startIndex + j] = Instantiate(m_CornerObject).transform;
                    m_Corners[startIndex + j].SetParent(box.transform, false);
                }

                m_Corners[startIndex + 0].localPosition = new Vector3(
                    box.center.x + box.size.x * 0.5f,
                    box.center.y + box.size.y * 0.5f,
                    box.center.z + box.size.z * 0.5f
                );
                m_Corners[startIndex + 0].localRotation = Quaternion.Euler(new Vector3(90f, 180f, 0f));

                m_Corners[startIndex + 1].localPosition = new Vector3(
                    box.center.x - box.size.x * 0.5f,
                    box.center.y + box.size.y * 0.5f,
                    box.center.z + box.size.z * 0.5f
                );
                m_Corners[startIndex + 1].localRotation = Quaternion.Euler(new Vector3(90f, 90f, 0f));

                m_Corners[startIndex + 2].localPosition = new Vector3(
                    box.center.x - box.size.x * 0.5f,
                    box.center.y - box.size.y * 0.5f,
                    box.center.z + box.size.z * 0.5f
                );
                m_Corners[startIndex + 2].localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));

                m_Corners[startIndex + 3].localPosition = new Vector3(
                    box.center.x + box.size.x * 0.5f,
                    box.center.y - box.size.y * 0.5f,
                    box.center.z + box.size.z * 0.5f
                );
                m_Corners[startIndex + 3].localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));

                m_Corners[startIndex + 4].localPosition = new Vector3(
                    box.center.x + box.size.x * 0.5f,
                    box.center.y + box.size.y * 0.5f,
                    box.center.z - box.size.z * 0.5f
                );
                m_Corners[startIndex + 4].localRotation = Quaternion.Euler(new Vector3(90f, -90f, 0f));

                m_Corners[startIndex + 5].localPosition = new Vector3(
                    box.center.x - box.size.x * 0.5f,
                    box.center.y + box.size.y * 0.5f,
                    box.center.z - box.size.z * 0.5f
                );
                m_Corners[startIndex + 5].localRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));

                m_Corners[startIndex + 6].localPosition = new Vector3(
                    box.center.x - box.size.x * 0.5f,
                    box.center.y - box.size.y * 0.5f,
                    box.center.z - box.size.z * 0.5f
                );
                m_Corners[startIndex + 6].localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));

                m_Corners[startIndex + 7].localPosition = new Vector3(
                    box.center.x + box.size.x * 0.5f,
                    box.center.y - box.size.y * 0.5f,
                    box.center.z - box.size.z * 0.5f
                );
                m_Corners[startIndex + 7].localRotation = Quaternion.Euler(new Vector3(0f, -90f, 0f));

                // Fix scale
                for (int j = 0; j < 8; ++j)
                {
                    //m_Corners[startIndex + j].SetParent(t, true);
                    var s = m_Corners[startIndex + j].lossyScale;
                    m_Corners[startIndex + j].localScale = new Vector3 (
                        Mathf.Abs(1f / s.x), Mathf.Abs(1f / s.y), Mathf.Abs(1f / s.z)
                        );
                }

                ++itr;
            }
			Hide ();
		}

		public void Show ()
		{
			for (int i = 0; i < m_Corners.Length; ++i)
				m_Corners [i].gameObject.SetActive (true);
		}

		public void Hide ()
        {
            for (int i = 0; i < m_Corners.Length; ++i)
				m_Corners [i].gameObject.SetActive (false);
		}
	}
}