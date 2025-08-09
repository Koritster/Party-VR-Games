using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using XRMultiplayer;
using XRMultiplayer.MiniGames;

public class DartsNetworked : MiniGameNetworked
{
    public struct PlayerNetworkData : INetworkSerializable
    {
        public FixedString32Bytes playerName;
        public int score;

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
    }

    //public NetworkList<PlayerNetworkData> playerList = new NetworkList<PlayerNetworkData>();
    public NetworkVariable<PlayerNetworkData> player1Data = new NetworkVariable<PlayerNetworkData>(
        new PlayerNetworkData("Jugador 1", 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<PlayerNetworkData> player2Data = new NetworkVariable<PlayerNetworkData>(
        new PlayerNetworkData("Jugador 1", 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

        //Get Score texts
        for (int i = 0; i < m_Scores.Length; i++)
        {
            TextMeshProUGUI[] texts = m_Scores[i].GetComponentsInChildren<TextMeshProUGUI>();

            foreach (TextMeshProUGUI text in texts)
            {
                if (text.CompareTag("Score Text"))
                {
                    txt_Scores[i] = text;
                }
            }
        }

        Debug.Log("Asignando listeners...");
        player1Data.OnValueChanged += (oldData, newData) => OnPlayerDataChanged(0, oldData, newData);
        player2Data.OnValueChanged += (oldData, newData) => OnPlayerDataChanged(1, oldData, newData);
    }

    private bool inGame;

    public override void StartGame()
    {
        base.StartGame();

        //Reiniciar los jugadores registrados
        player1Data.Value = new PlayerNetworkData("Jugador 1", 0);
        player2Data.Value = new PlayerNetworkData("Jugador 2", 0);

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

    private void OnPlayerDataChanged(int id, PlayerNetworkData oldValue, PlayerNetworkData newValue)
    {
        Debug.Log($"Actualizando la información de {newValue.playerName}, ahora tiene {newValue.score} puntos");

        UpdateUI(id, newValue);
    }

    private void UpdateUI(int id, PlayerNetworkData data)
    {
        txt_PlayerNames[id].text = data.playerName.ToString();
        txt_Scores[id].text = data.score.ToString();
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

        if (timer.Value <= 0f)
        {
            inGame = false;
         
            Debug.Log("El tiempo a terminado");

            timer.Value = 0f;

            //Terminar el juego
            int winnerPoints = 0;
            string winnerName = "ERROR";

            if(player1Data.Value.score == player2Data.Value.score)
            {
                winnerPoints = player1Data.Value.score;
                winnerName = "Draw";
            }
            else if(player1Data.Value.score > player2Data.Value.score)
            {
                winnerPoints = player1Data.Value.score;
                winnerName = player1Data.Value.playerName.ToString();
            }
            else
            {
                winnerPoints = player2Data.Value.score;
                winnerName = player2Data.Value.playerName.ToString();
            }

            m_MinigameBase.FinishGame(winnerName, winnerPoints.ToString());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(int playerId)
    {
        XRINetworkGameManager.Instance.GetPlayerByID((ulong)playerId, out XRINetworkPlayer m_localPlayer);
        PlayerNetworkData playerData = new PlayerNetworkData(m_localPlayer.playerName, 0);
        
        if(playerId == 0)
        {
            player1Data.Value = playerData;
            Debug.Log("Se ha registrado al jugador 1");
        }
        else if(playerId == 1)
        {
            player2Data.Value = playerData;
            Debug.Log("Se ha registrado al jugador 2");
        }
        else
        {
            Debug.LogError("Valio vrga");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LocalPlayerHitServerRpc(int id, int points)
    {
        Debug.Log($"El jugador ha dado a un objetivo que le otorgó {points} puntos!");

        if(id == 0)
        {
            var temp = player1Data.Value;
            temp.score += points;
            player1Data.Value = temp;
        }    
        else if(id == 1)
        {
            var temp = player1Data.Value;
            temp.score += points;
            player2Data.Value = temp;
        }
    }
}
