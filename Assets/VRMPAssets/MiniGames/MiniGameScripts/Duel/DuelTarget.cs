using System;
using System.Collections;
using UnityEngine;
using XRMultiplayer;
using XRMultiplayer.MiniGames;
using static UnityEditor.PlayerSettings;

public class DuelTarget : MonoBehaviour
{
    [SerializeField] protected float m_Lifetime = 4.0f;

    Action<DuelTarget> m_OnReturnToPool;

    private ParticlesPool m_particlesPool;

    void OnEnable()
    {
        Debug.Log("Soy una Diana");
        m_particlesPool = FindFirstObjectByType<ParticlesPool>();
    }

    public void OnHitRegister(int playerId)
    {
        DuelNetworked.instance.LocalPlayerHitServerRpc(playerId, 1);

        GameObject newParticle = m_particlesPool.GetItem();

        if (!newParticle.TryGetComponent(out ParticleSystem particle))
        {
            Utils.Log("Particle component not found on projectile object.", 1);
            return;
        }

        particle.transform.localPosition = transform.position;
        particle.Play();

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
        gameObject.SetActive(false);
    }
}
