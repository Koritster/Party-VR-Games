using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRMultiplayer.MiniGames
{
    public class DartsMinigame : MiniGameBase
    {
        DartsNetworked m_DartsNetworked;

        private void Awake()
        {
            m_DartsNetworked = GetComponent<DartsNetworked>();
        }

        public override void StartGame()
        {
            base.StartGame();

            m_DartsNetworked.StartGame();
        }

        public override void FinishGame(string name, string score = "")
        {
            base.FinishGame(name, score);
        }
    }
}
