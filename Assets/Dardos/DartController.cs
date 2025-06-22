using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DartController : MonoBehaviour
{
    public float throwForce = 10f;
    private Rigidbody rb;
    private bool hasBeenThrown = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ThrowDart(Vector3 force)
    {
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

    [ClientRpc]
    private void ThrowDartClientRpc(Vector3 force)
    {
        if (!IsOwner && !hasBeenThrown)
        {
            rb.isKinematic = false;
            rb.AddForce(force * throwForce, ForceMode.Impulse);
            hasBeenThrown = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasBeenThrown)
        {
            rb.isKinematic = true; // Se queda clavado
            ScoreDart(collision);
        }
    }

    private void ScoreDart(Collision collision)
    {
        DartBoard dartBoard = collision.gameObject.GetComponent<DartBoard>();
        if (dartBoard != null)
        {
            Vector3 hitPoint = collision.contacts[0].point;
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
