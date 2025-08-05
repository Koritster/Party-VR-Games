using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using XRMultiplayer;
using XRMultiplayer.MiniGames;

public class DartsNetworked : NetworkBehaviour
{
    [SerializeField] private float timeLenght;
    [SerializeField] private GameObject clock;
    [SerializeField] private Gradient clockColors;

    private float currentTime;
    
    private DartsMinigame m_DartsMinigame;
    private MiniGameManager m_MinigameManager;
    private GameObject m_scoreGO;
    private PlayerLocalInfo m_playerLocalInfo;

    [Serializable]
    private class Points
    {
        public int points;
        public TextMeshProUGUI scoreTxt;

        public Points(int m_points, TextMeshProUGUI m_scoreTxt)
        {
            this.points = m_points;
            this.scoreTxt = m_scoreTxt;
            this.scoreTxt.text = "Score: 0";
        }

        public void AddPoints(int m_points)
        {
            points += m_points;
            scoreTxt.text = points.ToString();
        }
    }

    private Dictionary<XRINetworkPlayer, Points> playerPoints = new Dictionary<XRINetworkPlayer, Points>();

    private NetworkVariable<float> timer = new NetworkVariable<float>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Start()
    {
        m_DartsMinigame = GetComponent<DartsMinigame>();
        m_MinigameManager = GetComponent<MiniGameManager>();
        m_playerLocalInfo = m_MinigameManager.localPlayer;
    }

    private bool inGame;

    public void StartGame()
    {
        Debug.Log("Iniciando juego de dardos");

        timer.OnValueChanged += UpdateTimer;

        m_scoreGO = m_playerLocalInfo.m_Score;

        RegisterPlayerServerRpc();

        if (IsServer)
        {
            timer.Value = timeLenght;
        }

        inGame = true;
    }

    private void UpdateTimer(float oldValue, float newValue)
    {
        //Cambiar el color del reloj
        float t = newValue / timeLenght;

        Color color = clockColors.Evaluate(t);
        clock.GetComponent<Renderer>().material.color = color;

        Debug.Log(newValue);
    }

    private void Update()
    {
        if (!IsServer || !inGame) return;

        //Actualizar timer solo si está en juego
        timer.Value -= Time.deltaTime;

        if (timer.Value <= 0f)
        {
            Debug.Log("El tiempo a terminado");

            timer.Value = 0f;

            //Terminar el juego
            int winnerPoints = 0;
            string winnerName = "ERROR";

            for(int i = 0; i < playerPoints.Count; i++)
            {
                if (playerPoints.ElementAt(i).Value.points > winnerPoints)
                {
                    winnerPoints = playerPoints.ElementAt(i).Value.points;
                    winnerName = playerPoints.ElementAt(i).Key.playerName;
                }
            }

            m_DartsMinigame.FinishGame(winnerName, winnerPoints.ToString());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc()
    {
        Debug.Log("Buscando si existe el jugador " + m_playerLocalInfo.m_PlayerId);

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
        }
    }

    [ClientRpc]
    private void RegisterPlayerClientRpc(ulong playerId, int scoreIndex)
    {
        Debug.Log("Regsitrando el usuario a todos los clientes...");

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
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LocalPlayerHitServerRpc(int points)
    {
        if (XRINetworkGameManager.Instance.GetPlayerByID(m_playerLocalInfo.m_PlayerId, out XRINetworkPlayer m_localPlayer))
        {
            LocalPlayerHitClientRpc(m_playerLocalInfo.m_PlayerId, points);
        }
    }

    [ClientRpc]
    private void LocalPlayerHitClientRpc(ulong playerId, int points)
    {
        if (XRINetworkGameManager.Instance.GetPlayerByID(playerId, out XRINetworkPlayer m_localPlayer))
        {
            playerPoints[m_localPlayer].AddPoints(points);
            Debug.Log($"El jugador {m_localPlayer} ha obtenido {points}");
        }
    }
}
