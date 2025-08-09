using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using XRMultiplayer.MiniGames;
using XRMultiplayer;

public class MiniGameNetworked : NetworkBehaviour, INetworkMinigame
{
    public MiniGameBase m_MinigameBase;
    public MiniGameManager m_MinigameManager;
    public PlayerLocalInfo m_playerLocalInfo;
    public GameObject[] m_Scores;
    public List<TextMeshProUGUI> txt_PlayerNames = new List<TextMeshProUGUI>();

    public int playerId;

    public virtual void Start()
    {
        m_MinigameBase = GetComponent<MiniGameBase>();
        m_MinigameManager = GetComponent<MiniGameManager>();
        m_playerLocalInfo = m_MinigameManager.localPlayer;

        m_Scores = m_MinigameManager.m_Scores;

        for(int i = 0; i < m_Scores.Length; i++)
        {
            TextMeshProUGUI[] texts = m_Scores[i].GetComponentsInChildren<TextMeshProUGUI>();

            foreach (TextMeshProUGUI text in texts)
            {
                if (text.CompareTag("Player Name Text"))
                {
                    txt_PlayerNames[i] = text;
                }
            }
        }
    }

    public virtual void StartGame()
    {

    }
}

public interface INetworkMinigame
{
    public void StartGame();
}