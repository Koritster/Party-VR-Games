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
    [SerializeField] private int idPlayer;

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
        dartsNetworked.LocalPlayerHitServerRpc(idPlayer, points);
        
        SpawnDianaServerRpc(points);
    }

    [ServerRpc]
    private void SpawnDianaServerRpc(int pts)
    {
        Vector3 pos = transform.position;

        SpawnDianaClientRpc(pos, pts);
    }

    [ClientRpc]
    private void SpawnDianaClientRpc(Vector3 pos, int pts)
    {
        newTxt = dartTxtPool.GetItem();

        TextMeshProUGUI text = newTxt.GetComponentInChildren<TextMeshProUGUI>();

        if (text == null)
        {
            Utils.Log("No se encontró el texto en el objeto.", 1);
            return;
        }

        text.transform.SetPositionAndRotation(pos, Quaternion.identity);
        text.transform.localScale = Vector3.one;
        text.text = $"+{pts}";

        Invoke(nameof(ReturnPoolTxt), 2f);

        text.GetComponent<Animator>().Play("MoveTxt");
    }

    private void ReturnPoolTxt()
    {
        dartTxtPool.ReturnItem(newTxt.gameObject);
    }
}
