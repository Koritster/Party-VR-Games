using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer.MiniGames;

public class DuelPlayerCollision : MonoBehaviour
{
    /*private bool isLocal = false;
    private DuelNetworked m_DuelNetworked;

    public void SetupPlayerCollision(DuelNetworked duelNetworked)
    {
        m_DuelNetworked = duelNetworked;
        isLocal = true;
    }

    public void RestartSetup()
    {
        m_DuelNetworked = null;
        isLocal = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Esta mmda choc� con " + other.name);

        if (other.CompareTag("Bullet") && isLocal)
        {
            Debug.LogWarning("Has recibido un disparo!");
            m_DuelNetworked.LocalPlayerLooseServerRpc();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && isLocal)
        {
            Debug.LogWarning("Has recibido un disparo!");
            m_DuelNetworked.LocalPlayerLooseServerRpc();
        }
    }*/
}
