using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartController : MonoBehaviour
{
    private Vector3 originPos;
    private Quaternion originRot;
    private Rigidbody rb;

    private void Awake()
    {
        originPos = transform.position;
        originRot = transform.rotation;
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
        transform.rotation = originRot;
    }

    private void OnTriggerEnter(Collider other)
    {
        DartsTarget target = other.GetComponent<DartsTarget>();
        if (target != null)
        {
            target.OnHitRegister();
            rb.isKinematic = true;
        }

    }
}
