using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRMultiplayer.MiniGames
{
    public class DartsMinigame : MiniGameBase
    {
        int currentPlayerScore = 0;

        public override void StartGame()
        {
            base.StartGame();
        }

        public override void FinishGame(bool submitScore = true)
        {
            base.FinishGame(submitScore);
        }

        public void LocalPlayerHit(int points)
        {
            currentPlayerScore += points;
            m_MiniGameManager.SubmitScoreServerRpc(currentPlayerScore, XRINetworkPlayer.LocalPlayer.OwnerClientId);
        }
    }
}
