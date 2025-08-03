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
        private TextMeshProUGUI txt_Timer;
        private DuelMinigame m_DuelMinigame;
        private MiniGameManager m_MinigameManager;
        private GameObject m_scoreGO;
        private PlayerLocalInfo m_playerLocalInfo;
        private Dictionary<XRINetworkPlayer, List<DuelLives>> playerLives;

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
                m_scoreGO = m_playerLocalInfo.m_Score;

                //Set names
                m_scoreGO.GetComponentInChildren<TextMeshProUGUI>().text = m_localPlayer.name;

                //Set lives objects && set lives
                List<DuelLives> livesGO = GetComponentsInChildren<DuelLives>().ToList<DuelLives>();

                //Declarar la lista de jugadores con las vidas máximas
                foreach (KeyValuePair<XRINetworkPlayer, ScoreboardSlot> kvp in m_MinigameManager.currentPlayerDictionary)
                {
                    playerLives.Add(kvp.Key, livesGO);
                }
            }
        }

        public void StartRound()
        {
            Debug.Log("Iniciando nueva ronda...");

            //Reinicio de ronda
            if (IsOwner)
            {
                inGame = true;
                timerTime = Random.Range(4f, 9f);
                timerToShoot.Value = timerTime;
                ShowGunsClientRpc(false);
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
                LocalPlayerLooseClientRpc(m_playerLocalInfo.m_PlayerId);
        }

        [ClientRpc]
        public void LocalPlayerLooseClientRpc(ulong playerId)
        {
            if (XRINetworkGameManager.Instance.GetPlayerByID(m_playerLocalInfo.m_PlayerId, out XRINetworkPlayer m_localPlayer))
            {
                //Restar una vida al jugador
                playerLives[m_localPlayer].Last<DuelLives>().LiveLost();
                playerLives[m_localPlayer].RemoveAt(playerLives[m_localPlayer].Count - 1);

                //Comprobar si ha perdido
                if(playerLives[m_localPlayer].Count <= 0)
                {
                    foreach(KeyValuePair<XRINetworkPlayer, List<DuelLives>> kvp in playerLives)
                    {
                        if(kvp.Key == m_localPlayer)
                            continue;

                        //Mostrar al ganador
                    }
                }
            }
        }

        #endregion

        public void FinishRound()
        {
            //Fin de ronda | Decidir el ganador
            inGame = false;
        }
    }
}
