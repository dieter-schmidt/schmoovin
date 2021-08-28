using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using UnityEngine.SceneManagement;

namespace NeoFPS.SinglePlayer
{
    [HelpURL("https://docs.neofps.com/manual/fpcharactersref-mb-fpsprototypeplayercontroller.html")]
    public class FpsPrototypePlayerController : BaseController
    {
        private Coroutine m_RespawnCoroutine = null;

        public static FpsPrototypePlayerController localPlayer
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

        private void Start()
        {
            var c = GetComponent<ICharacter>();
            if (c != null)
            {
                c.controller = this;
                c.onIsAliveChanged += OnIsAliveChanged;
            }
        }

        void OnDestroy()
        {
            if (localPlayer == this)
                localPlayer = null;
        }

        void OnIsAliveChanged(ICharacter character, bool alive)
        {
            if (!alive)
                m_RespawnCoroutine = StartCoroutine(ReloadLevel());
            else
            {
                if (m_RespawnCoroutine != null)
                {
                    StopCoroutine(m_RespawnCoroutine);
                    m_RespawnCoroutine = null;
                }
            }
        }

        IEnumerator ReloadLevel()
        {
            float timer = 0f;
            while (timer < 5f)
            {
                yield return null;
                timer += Time.unscaledDeltaTime;
            }

            m_RespawnCoroutine = null;

            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}