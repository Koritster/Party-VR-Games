using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer.MiniGames;

public class DartsTarget : MonoBehaviour
{
    [SerializeField] private int basePoints;
    [SerializeField] private int multiply;

    private DartsMinigame dartsManager;

    private void Awake()
    {
        dartsManager= GetComponentInParent<DartsMinigame>();
        Debug.Log(dartsManager);
    }

    public void OnHitRegister()
    {
        int points = basePoints * multiply;
        dartsManager.LocalPlayerHit(points);
    }
}
