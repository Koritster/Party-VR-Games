using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using XRMultiplayer.MiniGames;
using XRMultiplayer;
using Unity.Collections;

public class MiniGameNetworked : NetworkBehaviour, INetworkMinigame
{
    public MiniGameBase m_MinigameBase;
    public MiniGameManager m_MinigameManager;
    public PlayerLocalInfo m_playerLocalInfo;
    public GameObject[] m_Scores;
    public List<TextMeshProUGUI> txt_PlayerNames = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> txt_Scores = new List<TextMeshProUGUI>();


    public int playerId;

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
        new PlayerNetworkData("Jugador 2", 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public virtual void Start()
    {
        m_MinigameBase = GetComponent<MiniGameBase>();
        m_MinigameManager = GetComponent<MiniGameManager>();
        m_playerLocalInfo = m_MinigameManager.localPlayer;

        m_Scores = m_MinigameManager.m_Scores;

        for (int i = 0; i < m_Scores.Length; i++)
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

    public virtual void StartGame()
    {
        if (IsServer)
        {
            //Reiniciar los jugadores registrados
            player1Data.Value = new PlayerNetworkData("Jugador 1", 0);
            player2Data.Value = new PlayerNetworkData("Jugador 2", 0);
        }

        playerId = (int)m_playerLocalInfo.m_PlayerId;

        //Insertar el jugador a la lista
        RegisterPlayerServerRpc(playerId);
    }

    public virtual void SetupGame()
    {
        Debug.Log("Reiniciando variables de jugadores");

        if (IsServer)
        {
            //Reiniciar los jugadores registrados
            player1Data.Value = new PlayerNetworkData("Jugador 1", 0);
            player2Data.Value = new PlayerNetworkData("Jugador 2", 0);
        }

        playerId = (int)m_playerLocalInfo.m_PlayerId;
    }

    private void OnPlayerDataChanged(int id, PlayerNetworkData oldValue, PlayerNetworkData newValue)
    {
        Debug.Log($"Actualizando la información de {newValue.playerName}, ahora tiene {newValue.score} puntos");

        UpdateUI(id);
    }

    private void UpdateUI(int id)
    {
        Debug.Log($"Actualizando UI del usuario con id {id}...");

        if (id == 0)
        {
            Debug.Log($"Actualizando información del jugador 1, con id {id}, nombre {player1Data.Value.playerName.ToString()} y que ahora tiene {player1Data.Value.score}");
            txt_PlayerNames[id].text = player1Data.Value.playerName.ToString();
            txt_Scores[id].text = player1Data.Value.score.ToString();
        }
        else if (id == 1)
        {
            Debug.Log($"Actualizando información del jugador 2, con id {id}, nombre {player2Data.Value.playerName.ToString()} y que ahora tiene {player2Data.Value.score}");
            txt_PlayerNames[id].text = player2Data.Value.playerName.ToString();
            txt_Scores[id].text = player2Data.Value.score.ToString();
        }
        else
        {
            Debug.LogError($"Esto no debería salir. Id = {id}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(int playerId)
    {
        XRINetworkGameManager.Instance.GetPlayerByID((ulong)playerId, out XRINetworkPlayer m_localPlayer);
        PlayerNetworkData playerData = new PlayerNetworkData(m_localPlayer.playerName, 0);

        if (playerId == 0)
        {
            player1Data.Value = playerData;
            Debug.Log("Se ha registrado al jugador 1");
        }
        else if (playerId == 1)
        {
            player2Data.Value = playerData;
            Debug.Log("Se ha registrado al jugador 2");
        }
        else
        {
            Debug.LogError("Valio vrga");
        }
    }
}

public interface INetworkMinigame
{
    public void StartGame();
}