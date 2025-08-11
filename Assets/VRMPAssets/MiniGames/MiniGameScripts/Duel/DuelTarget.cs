using System;
using System.Collections;
using UnityEngine;
using XRMultiplayer.MiniGames;

public class DuelTarget : MonoBehaviour
{
    [SerializeField] private DuelNetworked duelNetworked;
    [SerializeField] protected float m_Lifetime = 4.0f;

    Action<DuelTarget> m_OnReturnToPool;


    void OnEnable()
    {
        Debug.Log("Soy una Diana. Acabo de aparecer");

        if(duelNetworked == null)
        {
            duelNetworked = GetComponentInParent<DuelNetworked>();
        }
    }

    public void OnHitRegister(int playerId)
    {
        duelNetworked.LocalPlayerHitServerRpc(playerId, 1);
        ResetDiana();
    }

    public void Setup(Action<DuelTarget> returnToPoolAction = null)
    {
        if (returnToPoolAction != null)
        {
            m_OnReturnToPool = returnToPoolAction;
            StartCoroutine(ResetProjectileAfterTime());
        }
    }

    IEnumerator ResetProjectileAfterTime()
    {
        yield return new WaitForSeconds(m_Lifetime);
        ResetDiana();
    }

    public void ResetDiana()
    {
        StopAllCoroutines();
        m_OnReturnToPool?.Invoke(this);
        gameObject.SetActive(false); // O el método que uses en tu Pooler
    }
}
