using UnityEngine;
using XRMultiplayer;

public class RevolverTargetArea : MonoBehaviour
{
    public MiniGameDuel revolverManager;

    private void Start()
    {
        if (revolverManager == null)
        {
            revolverManager = FindObjectOfType<MiniGameDuel>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Projectile projectile = other.GetComponent<Projectile>();


                // Sumar puntos
                revolverManager.localPlayerHitTarget(10);

                // Destruir o reciclar el proyectil
                projectile.ResetProjectile(); // o ReturnToPool(), según cómo lo manejes


            
        }
    }
}