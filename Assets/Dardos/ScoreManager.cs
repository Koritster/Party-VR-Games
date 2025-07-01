using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

// Cambia MonoBehaviour por NetworkBehaviour
public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;

    public TextMeshProUGUI[] playerScoreTexts;
    private NetworkVariable<int> player1Score = new NetworkVariable<int>();
    private NetworkVariable<int> player2Score = new NetworkVariable<int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateScore(ulong clientId, int score)
    {
        if (IsServer)
        {
            if (clientId == 0)
                player1Score.Value += score;
            else
                player2Score.Value += score;
        }
    }

    private void Update()
    {
    }
}
