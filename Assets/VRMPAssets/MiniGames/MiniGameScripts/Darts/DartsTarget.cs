using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using XRMultiplayer;
using XRMultiplayer.MiniGames;

public class DartsTarget : NetworkBehaviour
{
    [SerializeField] private int basePoints;
    [SerializeField] private int multiply;
    [SerializeField] private int idPlayer;

    private DartsMinigame dartsManager;
    private DartsNetworked dartsNetworked;
    private DartTextPooler dartTxtPool;
    private GameObject newTxt;

    private void Start()
    {
        dartsManager = GetComponentInParent<DartsMinigame>();
        dartsNetworked = GetComponentInParent<DartsNetworked>();
    }

    public void OnHitRegister()
    {
        int points = basePoints * multiply;
        dartsNetworked.LocalPlayerHitServerRpc(idPlayer, points);
    }
}
