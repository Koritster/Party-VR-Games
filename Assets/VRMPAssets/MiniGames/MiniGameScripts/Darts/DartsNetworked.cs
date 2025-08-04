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
        }

        public void AddPoints(int m_points)
        {
            points += m_points;
            scoreTxt.text = points.ToString();
        }
    }

    private Dictionary<XRINetworkPlayer, Points> playerPoints;

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

        
    }

    private void UpdateTimer(float oldValue, float newValue)
    {
        //Cambiar el color del reloj
    }

    private void Update()
    {
        if (!IsServer || !inGame) return;

        //Actualizar timer solo si está en juego
        timer.Value -= Time.deltaTime;

        if (timer.Value <= 0f)
        {
            timer.Value = 0f;

            //Terminar el juego
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LocalPlayerHitServerRpc(int points)
    {
        if (XRINetworkGameManager.Instance.GetPlayerByID(m_playerLocalInfo.m_PlayerId, out XRINetworkPlayer m_localPlayer))
            LocalPlayerHitClientRpc(m_playerLocalInfo.m_PlayerId, points);
    }

    [ClientRpc]
    private void LocalPlayerHitClientRpc(ulong playerId, int points)
    {
        if (XRINetworkGameManager.Instance.GetPlayerByID(playerId, out XRINetworkPlayer m_localPlayer))
        {
            playerPoints[m_localPlayer].AddPoints(points);

        }
    }
}
