using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using XRMultiplayer;
using XRMultiplayer.MiniGames;

public class DartsNetworked : MiniGameNetworked
{
    public struct PlayerNetworkData : INetworkSerializable, IEquatable<PlayerNetworkData>
    {
        private FixedString32Bytes playerName;
        private int score;

        public PlayerNetworkData(FixedString32Bytes playerName, int score)
        {
            this.playerName = playerName;
            this.score = score;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref score);
        }

        public void AddScore(int amount)
        {
            score += amount;
            Debug.Log($"Has obtenido {amount} puntos!");
        }

        public FixedString32Bytes GetPlayerName()
        {
            return playerName;
        }

        public int GetScore()
        {
            return score;
        }

        public bool Equals(PlayerNetworkData other)
        {
            return playerName.Equals(other.playerName) && score == other.score;
        }

    }

    public NetworkList<PlayerNetworkData> playerList = new NetworkList<PlayerNetworkData>();

    [SerializeField] private float timeLenght;
    [SerializeField] private GameObject clock;
    [SerializeField] private Gradient clockColors;
    [SerializeField] List<TextMeshProUGUI> txt_Scores = new List<TextMeshProUGUI>();

    private NetworkVariable<float> timer = new NetworkVariable<float>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void Start()
    {
        base.Start();

        playerList.OnListChanged += OnPlayerAddedToList;
    }

    private bool inGame;

    public override void StartGame()
    {
        base.StartGame();

        //Reiniciar los jugadores registrados
        playerList.Clear();
        playerList.Add(new PlayerNetworkData("Jugador 1", 0));
        playerList.Add(new PlayerNetworkData("Jugador 2", 0));

        playerId = (int)m_playerLocalInfo.m_PlayerId;

        //Insertar el jugador a la lista
        RegisterPlayerServerRpc(playerId);
        
        timer.OnValueChanged += UpdateTimer;

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
    }

    private void Update()
    {
        if (!IsServer || !inGame) return;

        //Actualizar timer solo si está en juego
        timer.Value -= Time.deltaTime;

        Debug.Log("");

        if (timer.Value <= 0f)
        {
            inGame = false;
         
            Debug.Log("El tiempo a terminado");

            timer.Value = 0f;

            //Terminar el juego
            int winnerPoints = 0;
            string winnerName = "ERROR";

            for(int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].GetScore() > winnerPoints)
                {
                    winnerPoints = playerList[i].GetScore();
                    winnerName = playerList[i].GetPlayerName().ToString();
                }
            }

            m_MinigameBase.FinishGame(winnerName, winnerPoints.ToString());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(int playerId)
    {
        XRINetworkGameManager.Instance.GetPlayerByID((ulong)playerId, out XRINetworkPlayer m_localPlayer);
        PlayerNetworkData playerData = new PlayerNetworkData(m_localPlayer.playerName, 0);
        playerList[playerId] = playerData;
    }

    private void OnPlayerAddedToList(NetworkListEvent<PlayerNetworkData> changeEvent)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            TextMeshProUGUI[] texts = m_Scores[i].GetComponentsInChildren<TextMeshProUGUI>();

            foreach (TextMeshProUGUI text in texts)
            {
                if (text.CompareTag("Player Name Text"))
                {
                    Debug.Log("Seteando nombre...");
                    text.text = playerList[i].GetPlayerName().ToString();
                    Debug.Log(text.text);
                }
                else if(text.CompareTag("Score Text"))
                {
                    Debug.Log("Seteando score...");
                    {
                        txt_Scores.Add(text);
                        text.text = "Score: 0";
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LocalPlayerHitServerRpc(int points)
    {
        Debug.Log($"El jugador ha dado a un objetivo que le otorgó {points} puntos!");

        var data = playerList[playerId];
        data.AddScore(points);
        playerList[playerId] = data;

        

        LocalPlayerHitClientRpc();
    }

    [ClientRpc]
    private void LocalPlayerHitClientRpc()
    {
        for(int i = 0; i < playerList.Count; i++)
        {
            Debug.Log($"La nueva puntuación del jugador {playerList[i].GetPlayerName()} ahora tiene {playerList[i].GetScore()}");

            txt_Scores[i].text = $"Score: + {playerList[i].GetScore()}";
        }
    }
}
