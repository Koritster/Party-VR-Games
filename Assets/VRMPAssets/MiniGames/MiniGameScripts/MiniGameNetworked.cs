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

    public virtual void Start()
    {
        m_MinigameBase = GetComponent<MiniGameBase>();
        m_MinigameManager = GetComponent<MiniGameManager>();
        m_playerLocalInfo = m_MinigameManager.localPlayer;
    }

    public void StartGame()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc()
    {
        /*Debug.Log("Buscando si existe el jugador " + m_playerLocalInfo.m_PlayerId);

        if (XRINetworkGameManager.Instance.GetPlayerByID(m_playerLocalInfo.m_PlayerId, out XRINetworkPlayer m_localPlayer))
        {
            Debug.Log("Si existe");

            int scoreIndex = -1;
            for (int i = 0; i < m_MinigameManager.m_Scores.Length; i++)
            {
                Debug.Log($"{m_MinigameManager.m_Scores[i]} == {m_scoreGO}? {m_MinigameManager.m_Scores[i] == m_scoreGO}");

                if (m_MinigameManager.m_Scores[i] == m_scoreGO)
                {
                    scoreIndex = i;
                    Debug.Log("Se encontró un indice!");
                    break;
                }
            }

            Debug.Log(scoreIndex);

            RegisterPlayerClientRpc(m_playerLocalInfo.m_PlayerId, scoreIndex);
        }*/

        RegisterPlayerClientRpc(1, 1);
    }

    [ClientRpc]
    public void RegisterPlayerClientRpc(ulong playerId, int scoreIndex)
    {
        /*Debug.Log("Regsitrando el usuario a todos los clientes...");

        if (XRINetworkGameManager.Instance.GetPlayerByID(playerId, out XRINetworkPlayer m_localPlayer))
        {
            if (scoreIndex < 0 || scoreIndex >= m_MinigameManager.m_Scores.Length)
            {
                Debug.LogError("Problema con el indice");
                return;
            }

            Debug.LogWarning($"Registrando al jugador {m_localPlayer.playerName}");

            //Set names
            TextMeshProUGUI[] texts = m_MinigameManager.m_Scores[scoreIndex].GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI scoreTxt = null;

            foreach (TextMeshProUGUI text in texts)
            {
                if (text.CompareTag("Player Name Text"))
                {
                    text.text = m_localPlayer.name;
                }
                else if (text.CompareTag("Score Text"))
                {
                    scoreTxt = text.GetComponent<TextMeshProUGUI>();
                }
            }

            //Add player to dictionary
            playerPoints.Add(m_localPlayer, new Points(0, scoreTxt));
        }*/
    }
}

public interface INetworkMinigame
{
    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc();

    [ClientRpc]
    public void RegisterPlayerClientRpc(ulong playerId, int scoreIndex);

    public void StartGame();
}