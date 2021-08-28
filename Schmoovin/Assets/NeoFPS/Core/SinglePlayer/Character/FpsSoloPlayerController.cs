using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using NeoSaveGames.Serialization;

namespace NeoFPS.SinglePlayer
{
    [HelpURL("https://docs.neofps.com/manual/fpcharactersref-mb-fpssoloplayercontroller.html")]
    public class FpsSoloPlayerController : BaseController
    {
        public static FpsSoloPlayerController localPlayer
        {
            get;
            private set;
        }

        public override bool isPlayer
		{
			get { return true; }
		}

        void Awake()
        {
            if (localPlayer == null)
                localPlayer = this;
            else
                Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (localPlayer == this)
                localPlayer = null;
        }
    }
}