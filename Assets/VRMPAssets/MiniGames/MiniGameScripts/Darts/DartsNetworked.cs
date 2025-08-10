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
    [SerializeField] private float timeLenght;
    [SerializeField] private GameObject clock;
    [SerializeField] private Gradient clockColors;

    private NetworkVariable<float> timer = new NetworkVariable<float>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void Start()
    {
        base.Start();
    }

    private bool inGame;

    public override void StartGame()
    {
        base.StartGame();

        timer.OnValueChanged += UpdateTimer;

        if (IsServer)
        {
            timer.Value = timeLenght;
        }

        inGame = true;
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

            SetupGame();
        }
    }

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

        if(id == 0)
        {
            var temp = player1Data.Value;
            temp.score += points;
            player1Data.Value = temp;
        }    
        else if(id == 1)
        {
            var temp = player2Data.Value;
            temp.score += points;
            player2Data.Value = temp;
        }
    }
}
