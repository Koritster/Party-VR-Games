using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace XRMultiplayer.MiniGames
{
    public class DuelNetworked : MiniGameNetworked
    {
        [SerializeField] private Transform positiveLimit;
        [SerializeField] private Transform negativeLimit;
        [SerializeField] private float firstTimeToSpawn;
        [SerializeField] private float minLimitTimeToSpawn;
        [SerializeField] private float ratioOfDecreasingTime;

        [SerializeField] private float timeLenght;
        [SerializeField] private GameObject manecilla;
        [SerializeField] private Image clockFillImage;

        private NetworkVariable<float> timer = new NetworkVariable<float>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private float actualTimeToSpawn;
        private float currentTimer;

        private bool inGame;

        private DianaPool m_dianaPool;

        #region Start Functions

        public override void Start()
        {
            base.Start();

            m_dianaPool = GetComponentInChildren<DianaPool>();
        }

        public override void StartGame()
        {
            base.StartGame();

            actualTimeToSpawn = firstTimeToSpawn;
            currentTimer = actualTimeToSpawn;

            timer.OnValueChanged += UpdateTimer;

            if (IsServer)
            {
                timer.Value = timeLenght;
            }

            inGame = true;

            Debug.Log("Iniciando juego de duelo...");
        }

        #endregion

        #region Update Functions

        private void Update()
        {
            if (!IsServer || !inGame) return;

            timer.Value -= Time.deltaTime;

            if (timer.Value <= 0f)
            {
                inGame = false;

                Debug.Log("El tiempo a terminado");

                timer.Value = 0f;

                //Terminar el juego
                int winnerPoints = 0;
                string winnerName = "ERROR";

                if (player1Data.Value.score == player2Data.Value.score)
                {
                    winnerPoints = player1Data.Value.score;
                    winnerName = "Draw";
                }
                else if (player1Data.Value.score > player2Data.Value.score)
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

                SetupGame();

                return;
            }

            //Timer for Diana
            currentTimer -= Time.deltaTime;

            if(currentTimer <= 0)
            {
                GameObject newDiana = m_dianaPool.GetItem();

                if (!newDiana.TryGetComponent(out DuelTarget target))
                {
                    Utils.Log("Projectile component not found on projectile object.", 1);
                    return;
                }

                Vector3 pos = new Vector3(0, Random.Range(negativeLimit.localPosition.y, positiveLimit.localPosition.y), Random.Range(negativeLimit.localPosition.z, positiveLimit.localPosition.z));

                target.transform.localPosition = pos;
                Debug.Log(newDiana.transform.localPosition);

                target.Setup(OnProjectileDestroy);


                if (actualTimeToSpawn > minLimitTimeToSpawn)
                {
                    actualTimeToSpawn -= ratioOfDecreasingTime;
                }

                currentTimer = actualTimeToSpawn;
            }
        }

        private void UpdateTimer(float oldValue, float newValue)
        {
            float t = newValue / timeLenght;

            float t2 = 1 - t;

            clockFillImage.fillAmount = t2;

            float angle = Mathf.Lerp(-90f, 270f, t2);
            manecilla.transform.localRotation = Quaternion.Euler(angle, -90f, -270f);
        }

        void OnProjectileDestroy(DuelTarget target)
        {
            m_dianaPool.ReturnItem(target.gameObject);
        }

        #endregion

        public override void SetupGame()
        {
            base.SetupGame();

            if (IsServer)
            {
                timer.Value = timeLenght;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void LocalPlayerHitServerRpc(int id, int points)
        {
            Debug.Log($"El jugador con id {id} ha dado a un objetivo que le otorgó {points} puntos!");

            if (id == 0)
            {
                var temp = player1Data.Value;
                temp.score += points;
                player1Data.Value = temp;
            }
            else if (id == 1)
            {
                var temp = player2Data.Value;
                temp.score += points;
                player2Data.Value = temp;
            }
        }
    }
}
