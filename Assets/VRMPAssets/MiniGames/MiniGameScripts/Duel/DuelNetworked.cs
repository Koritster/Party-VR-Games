using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

namespace XRMultiplayer.MiniGames
{
    public class DuelNetworked : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI txt_Timer;
        [SerializeField] private DuelMinigame ref_DuelMinigame;

        private NetworkVariable<float> timerToShoot = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

        private float timerTime;
        private bool inGame;

        private void Start()
        {
            ref_DuelMinigame = GetComponent<DuelMinigame>();
        }

        public void StartGame()
        {
            timerToShoot.OnValueChanged += UpdateUITimer;

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
        }

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

        [ClientRpc]
        private void ShowGunsClientRpc(bool show) 
        {
            //Aparecer pistolas
            ref_DuelMinigame.ShowInteractables(show);
        }

        public void FinishRound()
        {
            //Fin de ronda | Decidir el ganador
            inGame = false;
        }
    }
}
