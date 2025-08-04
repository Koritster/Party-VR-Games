using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer;
using XRMultiplayer.MiniGames;

public class MinigameBasketball : MiniGameBase
{
    int currentPlayerScore = 0;
    public override void StartGame()
    {
        base.StartGame();

        //reset the current player score
        currentPlayerScore = 0;
    }

    public override void FinishGame(string name, string score = "")
    {
        base.FinishGame(name, score);

        //add functionality for when game finishes
    }

    public void localPlayerHitTarget(int targetPoints)
    {
        currentPlayerScore += targetPoints;
        m_MiniGameManager.SubmitScoreServerRpc(currentPlayerScore, XRINetworkPlayer.LocalPlayer.OwnerClientId);
    }

    //Creating a method for the player score
}
