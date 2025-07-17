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

            // Sumar puntos al jugador contrario
            revolverManager.enemyPlayerHitTarget(1); // Puedes cambiar 1 por otra cantidad

            // Reciclar o destruir el proyectil
            if (projectile != null)
            {
                projectile.ResetProjectile(); // O ReturnToPool si usas pooling
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
}
