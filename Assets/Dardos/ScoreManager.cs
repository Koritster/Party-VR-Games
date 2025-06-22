using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ScoreManager : MonoBehaviour
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
        // Actualizar UI (esto se sincroniza automáticamente con NetworkVariables)
        if (playerScoreTexts.Length > 0)
            playerScoreTexts[0].text = "Jugador 1: " + player1Score.Value;

        if (playerScoreTexts.Length > 1)
            playerScoreTexts[1].text = "Jugador 2: " + player2Score.Value;
    }
}
