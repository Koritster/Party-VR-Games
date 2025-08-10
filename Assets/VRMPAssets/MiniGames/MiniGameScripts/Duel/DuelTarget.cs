using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using XRMultiplayer.MiniGames;

public class DuelTarget : MonoBehaviour
{
    [SerializeField] private DuelMinigame duelManager;
    [SerializeField] private DuelNetworked duelNetworked;

    void Awake()
    {
        Debug.Log("Soy una Diana. Acabo de aparecer");
    }

    public void OnHitRegister(int playerId)
    {
        duelNetworked.LocalPlayerHitServerRpc(playerId, 1);
    }
}
