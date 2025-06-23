using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// Cambia MonoBehaviour por NetworkBehaviour
public class DartController : NetworkBehaviour
{
    public float throwForce = 10f;
    private Rigidbody rb;
    private bool hasBeenThrown = false;
    public Collider tipCollider; // Asigna el collider de la punta en el Inspector
    public Collider bodyCollider; // Asigna el collider principal del dardo

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Desactivamos la detección de colisión entre los colliders del dardo
        if (tipCollider != null && bodyCollider != null)
        {
            Physics.IgnoreCollision(tipCollider, bodyCollider);
        }
    }

    public void ThrowDart(Vector3 force)
    {
        if (!hasBeenThrown)
        {
            rb.isKinematic = false;
            rb.AddForce(force * throwForce, ForceMode.Impulse);
            rb.AddTorque(transform.right * 5f, ForceMode.Impulse); // Rotación
            hasBeenThrown = true;

            if (IsServer)
            {
                ThrowDartClientRpc(force);
            }
        }
    }

    [ClientRpc]
    private void ThrowDartClientRpc(Vector3 force)
    {
        if (!IsOwner && !hasBeenThrown)
        {
            rb.isKinematic = false;
            rb.AddForce(force * throwForce, ForceMode.Impulse);
            rb.AddTorque(transform.right * 5f, ForceMode.Impulse);
            hasBeenThrown = true;
        }
    }

    //Solo detectar colisiones con la punta
    private void OnTriggerEnter(Collider other)
    {
        if (!hasBeenThrown || !tipCollider.enabled) return;

        // Verificamos que no sea el tablero u otra superficie clavadle
        if (other.gameObject.CompareTag("DartBoard") || other.gameObject.CompareTag("Wall"))
        {
            StickDart(other.transform);
            ScoreDart(other);
        }
    }

    private void StickDart(Transform surface)
    {
        // Desactivamos físicas y colliders
        rb.isKinematic = true;
        tipCollider.enabled = false;
        bodyCollider.enabled = false;

        // Orientamos el dardo según la superficie
        Vector3 forward = -transform.forward;
        Vector3 normal = surface.forward; // Asume superficie plana, ajusta según necesidad

        // Alineamos el dardo con la normal de la superficie
        transform.rotation = Quaternion.LookRotation(forward, normal);

        // Movemos ligeramente el dardo hacia atrás para que no se hunda
        transform.position -= transform.forward * 0.02f;

        // Replicamos en red
        if (IsServer)
        {
            StickDartClientRpc();
        }
    }

    [ClientRpc]
    private void StickDartClientRpc()
    {
        if (!IsOwner)
        {
            rb.isKinematic = true;
            tipCollider.enabled = false;
            bodyCollider.enabled = false;
        }
    }

    private void ScoreDart(Collider other)
    {
        DartBoard dartBoard = other.GetComponent<DartBoard>();
        if (dartBoard != null)
        {
            Vector3 hitPoint = tipCollider.transform.position;
            int score = dartBoard.CalculateScore(hitPoint);

            if (IsServer)
            {
                UpdateScoreClientRpc(score, OwnerClientId);
            }
        }
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(int score, ulong clientId)
    {
        ScoreManager.Instance.UpdateScore(clientId, score);
    }
}
