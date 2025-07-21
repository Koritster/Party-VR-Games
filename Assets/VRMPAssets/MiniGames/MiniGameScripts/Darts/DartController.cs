using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartController : MonoBehaviour
{
    private Vector3 originPos;
    private Rigidbody rb;

    private void Awake()
    {
        originPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    public void MoveDelay()
    {
        Invoke(nameof(Move), 4f);
    }

    private void Move()
    {
        rb.isKinematic = true;
        transform.position = originPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TryGetComponent<DartsTarget>(out DartsTarget target))
        {
            target.OnHitRegister();
        }

        rb.isKinematic = true;
    }
}
