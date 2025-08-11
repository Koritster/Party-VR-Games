using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelProjectileController : MonoBehaviour
{
    public int playerId;

    private void OnTriggerEnter(Collider other)
    {
        DuelTarget target = other.GetComponent<DuelTarget>();

        Debug.Log(target);

        if(target != null)
        {
            target.OnHitRegister(playerId);
        }
    }
}
