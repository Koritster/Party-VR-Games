using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using XRMultiplayer.MiniGames;
using XRMultiplayer;

public class MiniGameNetworked : NetworkBehaviour, INetworkMinigame
{
    private MiniGameBase m_MinigameBase;
    private MiniGameManager m_MinigameManager;
    private PlayerLocalInfo m_playerLocalInfo;
    private List<TextMeshProUGUI> txt_PlayerName = new List<TextMeshProUGUI>();

    public virtual void Start()
    {
        m_MinigameBase = GetComponent<MiniGameBase>();
        m_MinigameManager = GetComponent<MiniGameManager>();
        m_playerLocalInfo = m_MinigameManager.localPlayer;

        for(int i = 0; i < m_MinigameManager.m_Scores.Length; i++)
        {
            txt_PlayerName.Add(m_MinigameManager.m_Scores[i].GetComponentInChildren<TextMeshProUGUI>());
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