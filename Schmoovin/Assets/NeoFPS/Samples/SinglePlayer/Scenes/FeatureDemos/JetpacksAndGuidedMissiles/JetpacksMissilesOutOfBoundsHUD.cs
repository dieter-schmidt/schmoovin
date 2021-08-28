using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.SinglePlayer;
using UnityEngine.Events;

namespace NeoFPS.Samples.SinglePlayer
{
    public class JetpacksMissilesOutOfBoundsHUD : MonoBehaviour
    {
        private void Awake()
        {
            JetpacksMissilesOutOfBounds.onIsOutOfBoundsChanged += OnIsOutOfBoundsChanged;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            JetpacksMissilesOutOfBounds.onIsOutOfBoundsChanged -= OnIsOutOfBoundsChanged;
        }

        void OnIsOutOfBoundsChanged(bool oob)
        {
            gameObject.SetActive(oob);
        }
    }
}