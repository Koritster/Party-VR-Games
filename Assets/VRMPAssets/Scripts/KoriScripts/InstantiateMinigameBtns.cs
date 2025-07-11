using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer.MiniGames;

public class InstantiateMinigameBtns : MonoBehaviour
{
    [SerializeField] private GameObject[] minigamesGO;
    [SerializeField] private GameObject btnPrefab;
    [SerializeField] private GameObject btnHolder;
 
    private void Start()
    {
        minigamesGO = GameObject.FindGameObjectsWithTag("Minigame");

        foreach (GameObject game in minigamesGO)
        {
            MiniGameManager mgManager = game.GetComponent<MiniGameManager>();
            MiniGameBase mgBase = mgManager.currentMiniGame;


        }
    }
}
