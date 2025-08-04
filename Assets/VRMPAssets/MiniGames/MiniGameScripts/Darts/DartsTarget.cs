using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using XRMultiplayer;
using XRMultiplayer.MiniGames;

public class DartsTarget : NetworkBehaviour
{
    [SerializeField] private int basePoints;
    [SerializeField] private int multiply;

    private DartsMinigame dartsManager;
    private DartsNetworked dartsNetworked;
    private DartTextPooler dartTxtPool;
    private GameObject newTxt;

    private void Awake()
    {
        dartsManager = GetComponentInParent<DartsMinigame>();
        dartsNetworked = GetComponentInParent<DartsNetworked>();
        dartTxtPool = dartsManager.GetComponentInChildren<DartTextPooler>();
    }

    public void OnHitRegister()
    {
        int points = basePoints * multiply;
        dartsNetworked.LocalPlayerHitServerRpc(points);

        newTxt = dartTxtPool.GetItem();
        
        if(!newTxt.transform.GetChild(0).TryGetComponent(out TextMeshProUGUI text))
        {
            Utils.Log("No se encontró el texto en el objeto.", 1);
            return;
        }

        text.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        text.text = $"+{points}";

        Invoke(nameof(ReturnPoolTxt), 2f);

        text.GetComponent<Animator>().Play("MoveTxt");
    }

    private void ReturnPoolTxt()
    {
        dartTxtPool.ReturnItem(newTxt.gameObject);
    }
}
