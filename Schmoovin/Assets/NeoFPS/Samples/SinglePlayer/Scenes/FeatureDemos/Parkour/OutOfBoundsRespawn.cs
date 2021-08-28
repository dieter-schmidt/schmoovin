using NeoFPS.SinglePlayer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.Samples.SinglePlayer
{
    public class OutOfBoundsRespawn : MonoBehaviour
    {
        [SerializeField] private float m_RespawnHeight = -25f;

        private void Update()
        {
            var gameMode = FpsGameMode.current as FpsSoloGameMinimal;
            if (gameMode != null && gameMode.player != null && gameMode.playerCharacter != null)
            {
                if (gameMode.playerCharacter.motionController.localTransform.position.y < m_RespawnHeight)
                    gameMode.Respawn(gameMode.player);
            }
        }
    }
}