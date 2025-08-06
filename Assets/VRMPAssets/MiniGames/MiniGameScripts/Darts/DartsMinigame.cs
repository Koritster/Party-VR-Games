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
            Debug.Log("Iniciando minijuego desde DartsMinigame");

            base.StartGame();

            m_DartsNetworked.StartGame();
        }

        public override void FinishGame(string name, string score = "")
        {
            Debug.LogWarning("Finalizando juego desde DartsMinigame");

            base.FinishGame(name, score);
        }
    }
}
