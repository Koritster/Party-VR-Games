using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XRMultiplayer.MiniGames
{
    public class DuelMinigame : MiniGameBase
    {
        DuelNetworked ref_DuelNetworked;

        public override void Start()
        {
            base.Start();
            ref_DuelNetworked = GetComponent<DuelNetworked>();
        }

        public override void StartGame()
        {
            base.StartGame();
            ref_DuelNetworked.StartGame();
        }

        public override void FinishGame(string name, string score = "")
        {
            Debug.Log("Finalizando juego");
            base.FinishGame(name, score);
        }
    }
}
