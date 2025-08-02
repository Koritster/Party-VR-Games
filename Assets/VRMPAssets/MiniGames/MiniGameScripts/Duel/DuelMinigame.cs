using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


namespace XRMultiplayer.MiniGames
{
    public class DuelMinigame : MiniGameBase
    {
        int currentPlayerScore;

        DuelNetworked ref_DuelNetworked;

        public override void Start()
        {
            base.Start();
            ref_DuelNetworked = GetComponent<DuelNetworked>();

            ref_DuelNetworked.StartGame();
        }

        public override void StartGame()
        {
            base.StartGame();
            ref_DuelNetworked.StartRound();
        }

        public override void FinishGame(bool submitScore = true)
        {
            Debug.Log("Finalizando juego");
            base.FinishGame(submitScore);
        }

        public void ShowInteractables(bool show)
        {
            foreach(XRBaseInteractable interactable in m_GameInteractables)
            {
                interactable.gameObject.SetActive(show);
            }
        }
    }
}
