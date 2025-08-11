using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRMultiplayer.MiniGames
{
    public class DartsMinigame : MiniGameBase
    {
        MiniGameNetworked m_MinigameNetworked;

        public override void Start()
        {
            base.Start();
            m_MinigameNetworked = GetComponent<MiniGameNetworked>();
        }

        public override void StartGame()
        {
            Debug.Log("Iniciando minijuego desde DartsMinigame");

            base.StartGame();

            m_MinigameNetworked.StartGame();
        }

        public override void FinishGame(string name, string score = "")
        {
            Debug.LogWarning("Finalizando juego desde DartsMinigame");

            base.FinishGame(name, score);
        }
    }
}
