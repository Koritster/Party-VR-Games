using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolverTargetArea : MonoBehaviour
{
    public MiniGameDuel basketballManager;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            //Increment player score
            basketballManager.localPlayerHitTarget(10);
        }
    }
}
