using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer.MiniGames;
using TMPro;
using UnityEngine.UI;
using XRMultiplayer;

public class InstantiateMinigameBtns : MonoBehaviour
{
    [SerializeField] private GameObject[] minigamesGO;
    [SerializeField] private GameObject btnPrefab;
    [SerializeField] private GameObject btnHolder;
 
    private void Start()
    {
        minigamesGO = GameObject.FindGameObjectsWithTag("Minigame");

        foreach (GameObject game in minigamesGO)
        {
            MiniGameManager mgManager = game.GetComponent<MiniGameManager>();
            MiniGameBase mgBase = mgManager.currentMiniGame;

            GameObject btn = Instantiate(btnPrefab, btnHolder.transform);
            TextButton dynBtn = new TextButton(btn.GetComponent<Button>());

            btn.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = mgBase.gameName;
            btn.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = mgBase.btnIcon;

            mgManager.m_DynamicButton = dynBtn;
        }
    }
}
