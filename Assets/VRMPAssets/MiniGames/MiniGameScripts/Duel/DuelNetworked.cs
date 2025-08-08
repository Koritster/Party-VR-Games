using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Linq;

namespace XRMultiplayer.MiniGames
{
    public class DuelNetworked : NetworkBehaviour
    {
        public DuelPlayerCollision[] playerCollisions;
        
        [SerializeField] private TextMeshProUGUI txt_Timer;
        private DuelMinigame m_DuelMinigame;
        private MiniGameManager m_MinigameManager;
        private GameObject m_scoreGO;
        private PlayerLocalInfo m_playerLocalInfo;
        private Dictionary<XRINetworkPlayer, List<DuelLives>> playerLives = new Dictionary<XRINetworkPlayer, List<DuelLives>>();
        private bool roundEnded;

        private NetworkVariable<float> timerToShoot = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

        private float timerTime;
        private bool inGame;

        #region Start Functions

        private void Start()
        {
            m_DuelMinigame = GetComponent<DuelMinigame>();
            m_MinigameManager = GetComponent<MiniGameManager>();
            m_playerLocalInfo = m_MinigameManager.localPlayer;
        }

        public void StartGame()
        {
            Debug.Log("Iniciando juego de duelo...");

            timerToShoot.OnValueChanged += UpdateUITimer;

            if (XRINetworkGameManager.Instance.GetPlayerByID(m_playerLocalInfo.m_PlayerId, out XRINetworkPlayer m_localPlayer))
            {
                //Declaración del PlayerLocalInfo y XRINetworkPlayer local
                //m_scoreGO = m_playerLocalInfo.m_Score;


                //Registrar el jugador para el diccionario de todos los jugadores
                RegisterPlayerServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RegisterPlayerServerRpc()
        {
            if (XRINetworkGameManager.Instance.GetPlayerByID(m_playerLocalInfo.m_PlayerId, out XRINetworkPlayer m_localPlayer))
            {
                int scoreIndex = -1;
                for(int i = 0; i < m_MinigameManager.m_Scores.Length; i++)
                {
                    if (m_MinigameManager.m_Scores[i] == m_scoreGO)
                    {
                        scoreIndex = i;
                        break;
                    }
                }

                if(scoreIndex < 0)
                {
                    Debug.LogError("No se encontró un indice valido");
                    return;
                }

                playerCollisions[scoreIndex].SetupPlayerCollision(this);

                RegisterPlayerClientRpc(m_playerLocalInfo.m_PlayerId, scoreIndex);
            }
        }

        [ClientRpc]
        private void RegisterPlayerClientRpc(ulong playerId, int scoreIndex)
        {
            if (XRINetworkGameManager.Instance.GetPlayerByID(playerId, out XRINetworkPlayer m_localPlayer))
            {
                if (scoreIndex < 0 || scoreIndex >= m_MinigameManager.m_Scores.Length)
                    return;

                Debug.Log("Registrando al usuario " + m_localPlayer);

                //Set names
                TextMeshProUGUI[] texts = m_MinigameManager.m_Scores[scoreIndex].GetComponentsInChildren<TextMeshProUGUI>();

                foreach (TextMeshProUGUI text in texts)
                {
                    if (text.CompareTag("Player Name Text"))
                    {
                        Debug.Log("Seteando nombre...");
                        text.text = m_localPlayer.name;
                        Debug.Log(text.text);
                    }
                }

                
                //Add player to dictionary
                List<DuelLives> duelLives = m_MinigameManager.m_Scores[scoreIndex].GetComponentsInChildren<DuelLives>().ToList();

                playerLives.Add(m_localPlayer, duelLives);
            }
        }

        public void StartRound()
        {
            Debug.Log("Iniciando nueva ronda...");

            //Reinicio de ronda
            if (IsOwner)
            {
                inGame = true;
                roundEnded = false;
                timerTime = Random.Range(4f, 9f);
                timerToShoot.Value = timerTime;
                //ShowGunsClientRpc(false);
            }
        }

        #endregion

        #region Update Functions

        private void UpdateUITimer(float oldValue, float newValue)
        {
            float actualTime = Mathf.CeilToInt(newValue);
            if(actualTime <= 0)
            {
                txt_Timer.text = "FUEGO!";
            }
            else
            {
                txt_Timer.text = actualTime.ToString();
            }
        }

        private void Update()
        {
            if (!IsServer || !inGame) return;

            //Actualizar timer solo si está en juego
            timerToShoot.Value -= Time.deltaTime;

            if (timerToShoot.Value <= 0f)
            {
                timerToShoot.Value = 0f;

                ShowGunsClientRpc(true);
            }
        }

        #endregion

        #region Rpc Functions

        [ClientRpc]
        private void ShowGunsClientRpc(bool show) 
        {
            //Aparecer pistolas
            m_DuelMinigame.ShowInteractables(show);
        }

        [ServerRpc(RequireOwnership = false)]
        public void LocalPlayerLooseServerRpc()
        {
            if (XRINetworkGameManager.Instance.GetPlayerByID(m_playerLocalInfo.m_PlayerId, out XRINetworkPlayer m_localPlayer))
            {
                LocalPlayerLooseClientRpc(m_playerLocalInfo.m_PlayerId);
            }
        }

        [ClientRpc]
        private void LocalPlayerLooseClientRpc(ulong playerId)
        {
            if (XRINetworkGameManager.Instance.GetPlayerByID(playerId, out XRINetworkPlayer m_localPlayer))
            {
                if (roundEnded)
                    return;

                //Terminar ronda
                roundEnded = true;

                //Restar una vida al jugador
                playerLives[m_localPlayer].Last<DuelLives>().LiveLost();
                playerLives[m_localPlayer].RemoveAt(playerLives[m_localPlayer].Count - 1);

                //Comprobar si ha perdido
                if(playerLives[m_localPlayer].Count <= 0)
                {
                    FinishRound(false);

                    foreach (DuelPlayerCollision playerCollision in playerCollisions)
                    {
                        playerCollision.RestartSetup();
                    }

                    foreach (KeyValuePair<XRINetworkPlayer, List<DuelLives>> kvp in playerLives)
                    {
                        if(kvp.Key == m_localPlayer)
                            continue;

                        //Mostrar al ganador y finalizar juego
                        m_DuelMinigame.FinishGame(kvp.Key.playerName);
                        return;
                    }
                }

                FinishRound(true);
            }
        }

        #endregion

        public void FinishRound(bool keepPlaying)
        {
            //Fin de ronda
            inGame = false;

            if (keepPlaying)
            {
                StartRound();
            }
        }
    }
}
