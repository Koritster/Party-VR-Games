using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace XRMultiplayer.MiniGames
{
    /// <summary>
    /// Manages the state of the minigame
    /// </summary>
    public class MiniGameManager : NetworkBehaviour
    {
        /// <summary>
        /// Format for the time text
        /// </summary>
        private const string TIME_FORMAT = "mm':'ss'.'ff";

        /// <summary>
        /// Keeps track of the current game state
        /// </summary>
        public GameState currentNetworkedGameState
        {
            get => networkedGameState.Value;
        }
        public enum GameState { None, PreGame, InGame, PostGame }

        /// <summary>
        /// Keeps track of the current game state synchronized across the network
        /// </summary>
        readonly NetworkVariable<GameState> networkedGameState = new(GameState.PreGame, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        /// <summary>
        /// Dictionary of players and their assigned scoreboard slots
        /// </summary>
        public Dictionary<XRINetworkPlayer, ScoreboardSlot> currentPlayerDictionary = new();

        [Tooltip("The current minigame being used")]
        public MiniGameBase currentMiniGame;

        /// <summary>
        /// Determines if the local player is in the game
        /// </summary>
        public bool LocalPlayerInGame => m_LocalPlayerInGame;
        bool m_LocalPlayerInGame = false;

        [Header("UI")]
        public TextButton m_DynamicButton;
        public GameObject m_WinnerCanvas;

        [Header("Game")]
        public int maxAllowedPlayers = 2;
        [SerializeField] int m_ReadyUpTimeInSeconds = 15;
        [SerializeField] int m_StartCoutdownTimeInSeconds = 5;
        [SerializeField] int m_PostGameWaitTimeInSeconds = 3;
        [SerializeField] int m_PostGameCountdownTimeInSeconds = 7;
        
        [SerializeField] private GameObject[] m_Scores = new GameObject[2];

        [Header("Transform References")]
        [SerializeField] Transform[] m_JoinTeleportTransform = new Transform[2];
        [SerializeField] Transform m_FinishTeleportTransform;

        [Header("Transform Offsets")]
        [SerializeField] bool m_UseInGameOffset = true;
        [SerializeField, Tooltip("Determines the offset of the canvas during game")] Vector3 m_InGameOffset;
        [SerializeField, Tooltip("Determines the offset of the canvas during the pre-game")] Vector3 m_PreGameOffset;

        [Header("Barrier")]
        [SerializeField] bool m_UseBarrier = true;
        [SerializeField] float m_DistanceCheckTime = .5f;
        [SerializeField] float m_BarrierRenderDistance = 30.0f;
        [SerializeField] Renderer m_BarrierRend;

        readonly List<ScoreboardSlot> m_ScoreboardSlots = new();
        NetworkList<ulong> m_CurrentPlayers;
        NetworkList<ulong> m_QueuedUpPlayers;
        readonly NetworkVariable<float> m_BestAllScore = new(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        [HideInInspector] public PlayerLocalInfo localPlayer;

        float m_CurrentTimer = 0.0f;
        IEnumerator m_StartGameRoutine;
        IEnumerator m_PostGameRoutine;

        /// <inheritdoc/>
        void Start()
        {
            if (currentMiniGame == null)
            {
                TryGetComponent(out currentMiniGame);
            }

            localPlayer = FindFirstObjectByType<PlayerLocalInfo>();

            m_QueuedUpPlayers = new NetworkList<ulong>();
            m_CurrentPlayers = new NetworkList<ulong>();

            if (m_BarrierRend == null)
            {
                m_UseBarrier = false;
            }
            else
            {
                if (m_UseBarrier)
                {
                    StartCoroutine(CheckBarrierRendererDistance());
                }
                else
                {
                    m_BarrierRend.enabled = false;
                }
            }

            localPlayer.m_PlayerId = XRINetworkPlayer.LocalPlayer.OwnerClientId;
        }

        /// <inheritdoc/>
        public virtual void Update()
        {
            if (networkedGameState.Value == GameState.InGame)
            {
                float dt = Time.deltaTime;
                m_CurrentTimer += dt;
                currentMiniGame.UpdateGame(dt);
            }
        }

        /// <inheritdoc/>
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        /// <inheritdoc/>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            networkedGameState.OnValueChanged += GameStateValueChanged;
            m_BestAllScore.OnValueChanged += BestAllScoreChanged;
            m_CurrentPlayers.OnListChanged += UpdatePlayerList;

            if (IsServer)
            {
                networkedGameState.Value = GameState.PreGame;
                m_BestAllScore.Value = 0;
            }
            UpdateGameState();

            if (networkedGameState.Value == GameState.InGame)
            {
                ResetContestants(true);
            }
        }

        /// <inheritdoc/>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            m_LocalPlayerInGame = false;
            currentPlayerDictionary.Clear();
        }

        private void UpdatePlayerList(NetworkListEvent<ulong> changeEvent)
        {
            if (networkedGameState.Value != GameState.InGame) return;

            // Wipe scoreboard.
            foreach (ScoreboardSlot s in m_ScoreboardSlots)
            {
                s.SetSlotOpen();
            }

            currentPlayerDictionary.Clear();

            foreach (var playerId in m_CurrentPlayers)
            {
                AddPlayerToList(playerId);
            }

            for (int i = currentPlayerDictionary.Count; i < m_ScoreboardSlots.Count; i++)
            {
                m_ScoreboardSlots[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < currentPlayerDictionary.Count; i++)
            {
                m_ScoreboardSlots[i].gameObject.SetActive(true);
                m_ScoreboardSlots[i].UpdateScore(0, currentMiniGame.currentGameType);
            }
        }

        void BestAllScoreChanged(float old, float current)
        {
            if (m_BestAllScore.Value <= 0.0f)
            {
                //m_BestAllText.text = $"<b>Current Record</b>: No Record Set";
            }
            else
            {
                if (currentMiniGame.currentGameType == MiniGameBase.GameType.Time)
                {
                    TimeSpan time = TimeSpan.FromSeconds(current);
                    //m_BestAllText.text = $"<b>Current Record</b>: {time.ToString(TIME_FORMAT)}";
                }
                else
                {
                    //m_BestAllText.text = $"<b>Current Record</b>: {current:N0}";
                }
            }
        }

        void GameStateValueChanged(GameState oldState, GameState currentState)
        {
            UpdateGameState();
        }

        void UpdateGameState()
        {
            switch (networkedGameState.Value)
            {
                case GameState.PreGame:
                    SetPreGameState();
                    break;
                case GameState.InGame:
                    SetInGameState();
                    break;
                case GameState.PostGame:
                    SetPostGameState();
                    break;
            }
        }

        void SetPreGameState()
        {
            m_LocalPlayerInGame = false;
            if (m_PostGameRoutine != null)
            {
                StopCoroutine(m_PostGameRoutine);
            }

            currentMiniGame.SetupGame();
            //m_ScoreboardTransform.SetPositionAndRotation(m_ScoreboardStartPose.position, m_ScoreboardStartPose.rotation);

            for (int i = 0; i < m_ScoreboardSlots.Count; i++)
            {
                m_ScoreboardSlots[i].gameObject.SetActive(true);
            }

            ResetContestants(false);

            //m_GameStateText.text = "Pre Game";

            //m_DynamicButton.UpdateButton(AddLocalPlayer, "Join");
            m_DynamicButton.UpdateButton(AddAllPlayers, "Join");
            //StartCoroutine(ResetReadyZones());

            tpIndex = 0;
        }

        void SetInGameState()
        {
            m_CurrentTimer = 0.0f;
            ResetContestants(true);

            if (LocalPlayerInGame)
            {
                m_DynamicButton.button.interactable = true;
                PlayerHudNotification.Instance.ShowText($"Game Started!");
                //ToggleShrink(true);
                if (!m_UseInGameOffset)
                {
                    //m_ScoreboardTransform.SetPositionAndRotation(m_ScoreboardInGameTransform.position, m_ScoreboardInGameTransform.rotation);
                }
            }
            else
            {
                m_DynamicButton.button.interactable = false;
            }

            currentMiniGame.StartGame();
        }

        void SetPostGameState()
        {
            if (LocalPlayerInGame)
            {
                //ToggleShrink(false);
                //TeleportToArea(m_LeaveTeleportTransform);
                //Teletransporta al jugador a su lugar del lobby
                localPlayer.TeleportPlayer();

                m_BarrierRend.gameObject.SetActive(true);
                //m_ScoreboardTransform.SetPositionAndRotation(m_ScoreboardStartPose.position, m_ScoreboardStartPose.rotation);
            }

            m_LocalPlayerInGame = false;
            //m_TeleportZonesObject.SetActive(false);
            //SortPlayers();
            //m_GameStateText.text = "Post Game";
            m_DynamicButton.UpdateButton(ResetGame, $"Wait", true, false);
            if (!currentMiniGame.finished)
            {
                currentMiniGame.FinishGame(false);
            }

            m_PostGameRoutine = PostGameRoutine();
            StartCoroutine(m_PostGameRoutine);
            if (currentPlayerDictionary.Count <= 0)
            {
                if (IsServer)
                {
                    networkedGameState.Value = GameState.PreGame;
                }
            }
        }

        IEnumerator PostGameRoutine()
        {
            yield return new WaitForSeconds(m_PostGameWaitTimeInSeconds);
            //m_GameStateText.text = "Next Game in";
            for (int i = m_PostGameCountdownTimeInSeconds; i > 0; i--)
            {
                m_DynamicButton.UpdateButton(ResetGame, $"{i}", true, false);
                yield return new WaitForSeconds(1);
            }

            if (IsServer)
            {
                networkedGameState.Value = GameState.PreGame;
            }
        }

        void CheckPlayersReady()
        {
            int readyCount = 0;
            if (m_QueuedUpPlayers.Count <= 0) return;

            //Falsear los jugadores listos
            readyCount = 2;

            if (readyCount > 0 && readyCount < m_QueuedUpPlayers.Count)
            {
                if (LocalPlayerInGame)
                {
                    m_DynamicButton.button.interactable = false;
                }
                if (m_StartGameRoutine != null) StopCoroutine(m_StartGameRoutine);
                m_StartGameRoutine = StartGameAfterTime(m_ReadyUpTimeInSeconds);
                StartCoroutine(m_StartGameRoutine);
            }
            else if (readyCount <= 0)
            {
                if (LocalPlayerInGame)
                {
                    m_DynamicButton.button.interactable = true;
                }
                if (m_StartGameRoutine != null) StopCoroutine(m_StartGameRoutine);

                if (LocalPlayerInGame)
                {
                    PlayerHudNotification.Instance.ShowText("Game Start Cancelled");
                }
                //m_GameStateText.text = "Pre Game";
            }
            else
            {
                if (LocalPlayerInGame)
                {
                    m_DynamicButton.button.interactable = false;
                }
                if (m_StartGameRoutine != null) StopCoroutine(m_StartGameRoutine);
                m_StartGameRoutine = StartGameAfterTime(m_StartCoutdownTimeInSeconds);
                StartCoroutine(m_StartGameRoutine);
            }
        }

        IEnumerator StartGameAfterTime(int countdownTime)
        {
            for (int i = countdownTime; i > 0; i--)
            {
                //m_GameStateText.text = $"Game Starting In {i}";

                if (LocalPlayerInGame)
                {
                    PlayerHudNotification.Instance.ShowText($"Game Starting In {i}");
                }
                yield return new WaitForSeconds(1);
            }

            //m_GameStateText.text = $"Game Starting Now!";

            if (IsServer)
            {
                m_DynamicButton.button.interactable = false;
                StartGameServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void StartGameServerRpc()
        {
            for (int i = 0; i < m_QueuedUpPlayers.Count; i++)
            {
                m_CurrentPlayers.Add(m_QueuedUpPlayers[i]);
            }
            m_QueuedUpPlayers.Clear();
            networkedGameState.Value = GameState.InGame;
        }

        [ServerRpc(RequireOwnership = false)]
        public void StopGameServerRpc()
        {
            networkedGameState.Value = GameState.PostGame;
            m_CurrentPlayers.Clear();
        }

        /// <summary>
        /// Submits a player score. If <see cref="finishGameOnScoreSubmit"/> is true, it will finish the game for that player.
        /// This function will also check if all players have finished the game, and if so, will stop the game.
        /// </summary>
        /// <param name="score">The Score to set for the player.</param>
        /// <param name="clientId">Client ID of the player to set the score for.</param>
        /// <param name="finishGameOnScoreSubmit">Whether or not to finish the game on score submit.</param>
        [ServerRpc(RequireOwnership = false)]
        public void SubmitScoreServerRpc(float score, ulong clientId, bool finishGameOnScoreSubmit = false, bool someoneWon = false)
        {
            SubmitScoreClientRpc(score, clientId, finishGameOnScoreSubmit, someoneWon);
        }

        [ClientRpc]
        void SubmitScoreClientRpc(float score, ulong clientId, bool finishGameOnScoreSubmit = false, bool someoneWon = false)
        {
            if (XRINetworkGameManager.Instance.GetPlayerByID(clientId, out XRINetworkPlayer player))
            {
                if (currentPlayerDictionary.ContainsKey(player))
                {
                    if (finishGameOnScoreSubmit || someoneWon)
                    {
                        Debug.Log("Finalizando el juego para todos");
                        foreach(ScoreboardSlot finishedPlayer in currentPlayerDictionary.Values)
                        {
                            finishedPlayer.isFinished = true;
                        }

                        FinishGameForAllClientsClientRpc();
                    }
                }
            }

            //SortPlayers();
            //CheckIfAllPlayersAreFinished();
        }

        private bool gameFinished;

        [ClientRpc]
        void FinishGameForAllClientsClientRpc()
        {
            if (gameFinished) return;
            
            gameFinished = true;
            FinishGame();
        }

        void CheckIfAllPlayersAreFinished()
        {
            bool gameOver = true;
            foreach (KeyValuePair<XRINetworkPlayer, ScoreboardSlot> kvp in currentPlayerDictionary)
            {
                if (!kvp.Value.isFinished)
                {
                    gameOver = false;
                    break;
                }
            }

            if (gameOver && IsServer)
            {
                StopGameServerRpc();
            }
        }

        /// <summary>
        /// Called localled on each client when the game is finished.
        /// </summary>
        public void FinishGame()
        {
            if (LocalPlayerInGame)
            {
                //ToggleShrink(false);
            }
            StartCoroutine(TeleportAfterFinish());
        }

        IEnumerator TeleportAfterFinish()
        {
            yield return new WaitForSeconds(1.5f);
            if (networkedGameState.Value == GameState.InGame)
            {
                TeleportToArea(m_FinishTeleportTransform);
            }
        }

        /// <summary>
        /// Called from UI Buttons
        /// </summary>

        int tpIndex = 0;

        public void AddAllPlayers()
        {
            List<ulong> playersID = XRINetworkGameManager.Instance.CurrentPlayerIDs;

            m_DynamicButton.button.interactable = false;
            Debug.Log("Agregando a todos los jugadores");

            foreach (ulong id in playersID)
            {
                AddPlayerServerRpc(id, tpIndex);
                tpIndex++;
            }

            CheckPlayersReady();
        }

        /// <summary>
        /// Called from UI buttons
        /// </summary>
        /*public void RemoveLocalPlayer()
        {
            m_DynamicButton.UpdateButton(AddLocalPlayer, "Join", false, false);
            RemovePlayerServerRpc(XRINetworkPlayer.LocalPlayer.OwnerClientId);
        }*/

        [ServerRpc(RequireOwnership = false)]
        void AddPlayerServerRpc(ulong clientId, int i)
        {
            AddPlayerClientRpc(clientId, i);
            if (m_QueuedUpPlayers.Count < maxAllowedPlayers)
            {
                m_QueuedUpPlayers.Add(clientId);
            }
        }

        /*[ServerRpc(RequireOwnership = false)]
        void RemovePlayerServerRpc(ulong clientId)
        {
            RemovePlayerClientRpc(clientId);

            if (m_QueuedUpPlayers.Contains(clientId))
            {
                m_QueuedUpPlayers.Remove(clientId);
            }

            if (m_CurrentPlayers.Contains(clientId))
            {
                m_CurrentPlayers.Remove(clientId);
            }
        }*/

        //A�adir jugador a la partida
        [ClientRpc]
        void AddPlayerClientRpc(ulong clientId, int i)
        {
            //Verifica que no haya m�s de 2 jugadores
            if (currentPlayerDictionary.Count < maxAllowedPlayers)
            {
                //Comprueba si el juego no ha iniciado
                if (networkedGameState.Value != GameState.PostGame)
                {
                    AddPlayerToList(clientId);
                }

                //Verifica que la id del cliente sea la del jugador local, no confundir con el jugador host
                if (clientId == XRINetworkPlayer.LocalPlayer.OwnerClientId)
                {
                    m_LocalPlayerInGame = true;
                    //m_DynamicButton.UpdateButton(RemoveLocalPlayer, "Leave");

                    localPlayer.TeleportPlayer(m_JoinTeleportTransform[i]);
                    localPlayer.m_Score = m_Scores[i];

                    PlayerHudNotification.Instance.ShowText($"Joined {currentMiniGame.gameName}");
                    m_BarrierRend.gameObject.SetActive(false);
                }

                if (currentPlayerDictionary.Count >= maxAllowedPlayers & !LocalPlayerInGame && networkedGameState.Value != GameState.PostGame)
                {
                    m_DynamicButton.button.interactable = false;
                }
            }
        }

        void AddPlayerToList(ulong clientId)
        {
            if (XRINetworkGameManager.Instance.GetPlayerByID(clientId, out XRINetworkPlayer player))
            {
                if (!currentPlayerDictionary.ContainsKey(player))
                {
                    currentPlayerDictionary.Add(player, null);
                    player.onDisconnected += PlayerDisconnected;
                }
            }
        }

        /*[ClientRpc]
        void RemovePlayerClientRpc(ulong clientId)
        {
            if (XRINetworkGameManager.Instance.GetPlayerByID(clientId, out XRINetworkPlayer player))
            {
                CheckDroppedPlayer(player);
            }

            if (clientId == XRINetworkPlayer.LocalPlayer.OwnerClientId)
            {
                m_LocalPlayerInGame = false;
                //m_TeleportZonesObject.SetActive(false);

                //If local player left, and we are still in game, don't let them rejoin mid game.
                if (networkedGameState.Value != GameState.InGame)
                {
                    m_DynamicButton.button.interactable = true;
                }

                //ToggleShrink(false);
                currentMiniGame.RemoveInteractables();
                PlayerHudNotification.Instance.ShowText($"Left {currentMiniGame.gameName}");
                //TeleportToArea(m_LeaveTeleportTransform);
                localPlayer.TeleportPlayer();
                //m_ScoreboardTransform.SetPositionAndRotation(m_ScoreboardStartPose.position, m_ScoreboardStartPose.rotation);
                m_BarrierRend.gameObject.SetActive(true);
            }
        }*/

        private void PlayerDisconnected(XRINetworkPlayer droppedPlayer)
        {
            CheckDroppedPlayer(droppedPlayer);
        }

        void CheckDroppedPlayer(XRINetworkPlayer droppedPlayer)
        {
            ScoreboardSlot removedSlot = null;
            if (currentPlayerDictionary.ContainsKey(droppedPlayer) && networkedGameState.Value != GameState.PostGame)
            {
                removedSlot = currentPlayerDictionary[droppedPlayer];
                //removedSlot.SetSlotOpen();
                currentPlayerDictionary.Remove(droppedPlayer);
                droppedPlayer.onDisconnected -= PlayerDisconnected;
            }

            if (IsOwner && m_QueuedUpPlayers.Contains(droppedPlayer.OwnerClientId))
            {
                m_QueuedUpPlayers.Remove(droppedPlayer.OwnerClientId);
            }

            if (networkedGameState.Value == GameState.InGame)
            {
                if (removedSlot != null)
                {
                    removedSlot.gameObject.SetActive(false);
                }

                if (currentPlayerDictionary.Count <= 0)
                {
                    m_DynamicButton.button.interactable = false;
                    if (IsServer)
                    {
                        StopGameServerRpc();
                    }
                }
                else
                {
                    CheckIfAllPlayersAreFinished();
                }
            }
            else if (networkedGameState.Value == GameState.PreGame)
            {
                if (currentPlayerDictionary.Count > 0)
                {
                    if (currentPlayerDictionary.Count >= maxAllowedPlayers)
                    {
                        m_DynamicButton.button.interactable = false;
                    }
                    else
                    {
                        m_DynamicButton.button.interactable = true;
                    }
                }
                CheckPlayersReady();
            }
        }

        IEnumerator CheckBarrierRendererDistance()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(m_DistanceCheckTime);
                if (m_UseBarrier)
                {
                    m_BarrierRend.enabled = Vector3.Distance(m_BarrierRend.transform.position, Camera.main.transform.position) < m_BarrierRenderDistance;
                }
            }
        }

        /// <summary>
        /// Updates the player scores based on <see cref="m_CurrentTimer"/>.
        /// </summary>
        public void UpdatePlayerScores()
        {
            foreach (var p in currentPlayerDictionary)
            {
                if (!p.Value.isFinished)
                {
                    //p.Value.UpdateScore(m_CurrentTimer, currentMiniGame.currentGameType);
                }
            }
        }

        /// <summary>
        /// Resets the Game State
        /// </summary>
        /// <remarks>
        /// This function is called locally at times, which creates a divergence between local game state and network game state
        /// </remarks>
        void ResetGame()
        {
            networkedGameState.Value = GameState.PreGame;
            SetPreGameState();
        }

        void ResetContestants(bool showGamePlayers)
        {
            currentPlayerDictionary.Clear();

            if (showGamePlayers)
            {
                // Add all contestants in current match.
                foreach (var playerId in m_CurrentPlayers)
                {
                    AddPlayerToList(playerId);
                }
            }
            else
            {
                // Add all contestants in queue.
                foreach (var playerId in m_QueuedUpPlayers)
                {
                    AddPlayerToList(playerId);
                }
            }
        }

        void TeleportToArea(Transform teleportTransform)
        {
            localPlayer.TeleportPlayer(teleportTransform);
        }
    }
}
