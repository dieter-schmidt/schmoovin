using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class ReplaceObject : MonoBehaviour
    {
        [SerializeField, Tooltip("The object to replace this one with.")]
        private GameObject m_SwapPrefab = null;

        private NeoSerializedGameObject m_NSGO = null;

        private void Awake()
        {
            m_NSGO = GetComponent<NeoSerializedGameObject>();
        }

        public void Replace()
        {
            if (m_SwapPrefab != null)
            {
                var t = transform;

                // Instantiate prefab
                Transform instance = null;

                // Parent with save system
                bool parented = false;
                if (m_NSGO != null)
                {
                    // Instantiate under parent if found
                    var parentNSGO = m_NSGO.GetParent();
                    if (parentNSGO != null)
                    {
                        instance = parentNSGO.InstantiatePrefab(m_SwapPrefab.transform);
                        parented = true;
                    }
                    else
                    {
                        // Or parent under scene if not
                        if (m_NSGO.serializedScene != null)
                        {
                            instance = m_NSGO.serializedScene.InstantiatePrefab(m_SwapPrefab.transform);
                            parented = true;
                        }
                    }
                }

                // Parent without save system if required
                if (!parented)
                {
                    instance = Instantiate(t.parent);
                    instance.SetParent(t.parent);
                }

                // Set local transform
                instance.localPosition = t.localPosition;
                instance.localRotation = t.localRotation;
                instance.localScale = t.localScale;

                Destroy(gameObject);
            }
        }
    }
}