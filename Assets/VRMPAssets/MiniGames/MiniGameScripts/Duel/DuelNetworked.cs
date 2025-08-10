using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Linq;

namespace XRMultiplayer.MiniGames
{
    public class DuelNetworked : MiniGameNetworked
    {
        [SerializeField] private Transform positiveLimit;
        [SerializeField] private Transform negativeLimit;
        [SerializeField] private float firstTimeToSpawn;
        [SerializeField] private float minLimitTimeToSpawn;
        [SerializeField] private float ratioOfDecreasingTime;

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
            inGame = true;

            Debug.Log("Iniciando juego de duelo...");
        }

        #endregion

        #region Update Functions

        private void Update()
        {
            if (!IsServer || !inGame) return;

            currentTimer -= Time.deltaTime;

            if(currentTimer <= 0)
            {
                GameObject newDiana = m_dianaPool.GetItem();

                Vector3 pos = new Vector3(0, Random.Range(negativeLimit.position.y, positiveLimit.position.y), Random.Range(negativeLimit.position.z, positiveLimit.position.z));

                newDiana.transform.SetPositionAndRotation(pos, Quaternion.identity);

                if(actualTimeToSpawn > minLimitTimeToSpawn)
                {
                    actualTimeToSpawn -= ratioOfDecreasingTime;
                }

                currentTimer = actualTimeToSpawn;
            }
        }

        #endregion

        public void FinishRound(bool keepPlaying)
        {
            //Fin de ronda
            inGame = false;

            actualTimeToSpawn = firstTimeToSpawn;
            currentTimer = actualTimeToSpawn;
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
