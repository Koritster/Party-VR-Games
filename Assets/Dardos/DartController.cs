using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Cambia MonoBehaviour por NetworkBehaviour
public class DartController : NetworkBehaviour
{
    public float throwForce = 10f;// Fuerza con la que se lanza el dardo
    private Rigidbody rb; // Componente de físicas del dardo
    private bool hasBeenThrown = false; // Flag para controlar si el dardo fue lanzado

    void Start()
    {
        // Obtenemos el componente Rigidbody al inicio
        rb = GetComponent<Rigidbody>();
    }

    // Método para lanzar el dardo
    public void ThrowDart(Vector3 force)
    {
        // Solo si el dardo no ha sido lanzado aún
        if (!hasBeenThrown)
        {
            rb.isKinematic = false;
            rb.AddForce(force * throwForce, ForceMode.Impulse);
            hasBeenThrown = true;

            if (IsServer)
            {
                ThrowDartClientRpc(force);
            }
        }
    }

    // Método RPC para sincronizar el lanzamiento en clientes
    [ClientRpc]
    private void ThrowDartClientRpc(Vector3 force)
    {
        // Detección de colisión
        if (!IsOwner && !hasBeenThrown)
        {
            rb.isKinematic = false;
            rb.AddForce(force * throwForce, ForceMode.Impulse);
            hasBeenThrown = true;
        }
    }

    // Detección de colisión
    private void OnCollisionEnter(Collision collision)
    {
        if (hasBeenThrown)
        {
            rb.isKinematic = true; // Se queda clavado
            ScoreDart(collision);
        }
    }

    // Método para calcular el puntaje
    private void ScoreDart(Collision collision)
    {
        DartBoard dartBoard = collision.gameObject.GetComponent<DartBoard>();
        if (dartBoard != null)
        {
            // Obtenemos el punto de impacto
            Vector3 hitPoint = collision.contacts[0].point;
            // Calculamos el puntaje
            int score = dartBoard.CalculateScore(hitPoint);

            // Si somos el servidor, actualizamos el puntaje en todos los clientes
            if (IsServer)
            {
                UpdateScoreClientRpc(score, OwnerClientId);
            }
        }
    }

    // Método RPC para actualizar el puntaje en todos los clientes
    [ClientRpc]
    private void UpdateScoreClientRpc(int score, ulong clientId)
    {

        // Llamamos al ScoreManager para actualizar el marcador
        ScoreManager.Instance.UpdateScore(clientId, score);
    }
}
