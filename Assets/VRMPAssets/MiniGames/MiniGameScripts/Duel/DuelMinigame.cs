using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XRMultiplayer.MiniGames
{
    public class DuelMinigame : MiniGameBase
    {
        int currentPlayerScore;

        public override void StartGame()
        {
            base.StartGame();
        }

        public override void FinishGame(bool submitScore = true)
        {
            base.FinishGame(submitScore);
        }
    }
}
