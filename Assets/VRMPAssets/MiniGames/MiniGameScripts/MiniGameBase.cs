using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace XRMultiplayer.MiniGames
{
    /// <summary>
    /// Base class for mini-games.
    /// </summary>
    [RequireComponent(typeof(MiniGameManager))]
    public class MiniGameBase : MonoBehaviour, IMiniGame
    {
        public bool finished
        {
            get => m_Finished;
            set => m_Finished = value;
        }

        protected bool m_Finished;

        public string gameName;
        public Sprite btnIcon;

        public enum GameType { Time, Score, Lives }
        public GameType currentGameType
        {
            get => m_GameType;
            set => m_GameType = value;
        }
        [SerializeField] GameType m_GameType;
        
        [SerializeField] protected float m_GameLength = 90.0f;

        [SerializeField] protected XRBaseInteractable[] m_GameInteractables;

        protected MiniGameManager m_MiniGameManager;
        protected XRInteractionManager m_InteractionManager;


        protected string winnerName, score;

        public virtual void Start()
        {
            TryGetComponent(out m_MiniGameManager);
            m_InteractionManager = FindFirstObjectByType<XRInteractionManager>();
        }

        public virtual void SetupGame()
        {
            if (m_GameType == GameType.Score)
            {
            }
        }

        public virtual void StartGame()
        {
            m_Finished = false;
        }

        public virtual void UpdateGame(float deltaTime)
        {
            /*if (m_GameType == GameType.Score)
            {
                m_CurrentTimer -= deltaTime;
                if (m_Finished && !m_GameEndingNotificationSent)
                {
                    m_GameEndingNotificationSent = true;
                    StartCoroutine(CheckForGameEndingRoutine());
                }
                CheckForGameEnd();
            }*/
        }

        /*protected void CheckForGameEnd()
        {
            if (m_CurrentTimer <= 3.5f & !m_GameEndingNotificationSent)
            {
                m_GameEndingNotificationSent = true;
                StartCoroutine(CheckForGameEndingRoutine());
            }
        }*/

        public virtual void FinishGame(string name, string score = "")
        {
            Debug.Log("El juego ha terminado");
            RemoveInteractables();
            m_Finished = true;

            CheckForGameEndingRoutine(name, score);
        }

        IEnumerator CheckForGameEndingRoutine(string name, string score)
        {
            int seconds = 3;
            while (seconds > 0)
            {
                if (m_MiniGameManager.LocalPlayerInGame)
                {
                    PlayerHudNotification.Instance.ShowText($"Game Ending In {seconds}");
                }
                yield return new WaitForSeconds(1.0f);
                seconds--;
            }
            if (m_MiniGameManager.LocalPlayerInGame)
            {
                PlayerHudNotification.Instance.ShowText($"Game Complete!");
            }

            m_MiniGameManager.FinishGameForAllClientsClientRpc(name, score);
        }

        public virtual void ReturnToSelect()
        {
            m_MiniGameManager.StopGameServerRpc();
        }

        public virtual void RestartMinigame()
        {
            m_MiniGameManager.ResetGameServerRpc();
        }

        public virtual void RemoveInteractables()
        {
            foreach (IXRInteractable interactable in m_GameInteractables)
            {
                m_InteractionManager.CancelInteractableSelection((IXRSelectInteractable)interactable);
            }
        }
    }

    /// <summary>
    /// Interface for mini-games.
    /// </summary>
    public interface IMiniGame
    {
        /// <summary>
        /// Sets up the mini-game.
        /// </summary>
        void SetupGame();

        /// <summary>
        /// Starts the mini-game.
        /// </summary>
        void StartGame();

        /// <summary>
        /// Updates the mini-game.
        /// </summary>
        /// <param name="deltaTime">The time since the last frame.</param>
        void UpdateGame(float deltaTime);

        /// <summary>
        /// Finishes the mini-game.
        /// </summary>
        void FinishGame(string name, string score = "");

        /// <summary>
        /// Returns players to select minigame.
        /// </summary>
        void ReturnToSelect();

        /// <summary>
        /// Starts the game again.
        /// </summary>
        void RestartMinigame();
    }
}
