using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer.MiniGames;

public class DuelPlayerCollision : MonoBehaviour
{
    private bool isLocal = false;
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
        if (other.CompareTag("Projectile") && isLocal)
        {
            Debug.LogWarning("Has recibido un disparo!");
            m_DuelNetworked.LocalPlayerLooseServerRpc();
        }
    }
}
