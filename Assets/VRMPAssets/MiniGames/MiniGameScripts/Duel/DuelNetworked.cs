using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace XRMultiplayer.MiniGames
{
    public class DuelNetworked : NetworkBehaviour
    {
        [SerializeField] private Text txt_Timer;

        private NetworkVariable<float> timerToShoot = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

        private float timerTime;
        private bool inGame;

        public void StartGame()
        {
            //Obtener los id de los jugadores
            if (!IsOwner) return;


        }

        public void StartRound()
        {
            //Reinicio de ronda
            if(IsOwner)
            {
                inGame = true;
                timerTime = Random.Range(4f, 9f);
                timerToShoot.Value = timerTime;
                ShowGunsClientRpc(false);
            }

            timerToShoot.OnValueChanged += UpdateUITimer;
        }

        private void UpdateUITimer(float oldValue, float newValue)
        {
            txt_Timer.text = Mathf.CeilToInt(newValue).ToString();
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

        [ClientRpc]
        private void ShowGunsClientRpc(bool show) 
        {
            //Aparecer pistolas
        }

        public void FinishRound()
        {
            //Fin de ronda | Decidir el ganador
            inGame = false;
        }
    }
}
