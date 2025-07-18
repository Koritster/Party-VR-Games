using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketBallController : MonoBehaviour
{
    private Vector3 originPosition;
    private Rigidbody rb;

    private void Awake()
    {
        originPosition = this.transform.position;
        rb = GetComponent<Rigidbody>();
    }

    public void DelayMove()
    {
        Invoke(nameof(MoveBall), 4f);
    }

    public void MoveBall()
    {
        rb.isKinematic = true;
        this.transform.position = originPosition;
    }
}
